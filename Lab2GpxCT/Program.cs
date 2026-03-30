// Lab2Gpx.cs - Adventure Lab → GPX downloader
// .NET 8+ Console App, žádné NuGet balíčky nejsou potřeba
// Použití: dotnet run -- --lat 50.0755 --lon 14.4378 --radius 15000 --output labs.gpx

//https://api.groundspeak.com/api-docs/index
//https://api.groundspeak.com/documentation#adventure-stages

namespace Lab2Gpx
{
    using System.Data;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Text.Unicode;
    using System.Xml.Linq;
    using static Lab2Gpx.Models;

    internal class Program
    {
        internal const string SETTINGS_FILE = "settings.json";

        internal const string CONSUMER_KEY = "A01A9CA1-29E0-46BD-A270-9D894A527B91";

        internal const string USER_AGENT = "Adventures/1.56.0 (4936) (android/32)";

        internal const string LOGIN_URL = "https://api.groundspeak.com/adventuresmobile/v1/public/accounts/login";

        internal const string SEARCH_URL = "https://api.groundspeak.com/adventuresmobile/v1/public/adventures/search";

        internal const string DETAILS_URL = "https://api.groundspeak.com/adventuresmobile/v1/public/adventures/";

        internal static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private static async Task<int> Main(string[] args)
        {           
            var json = File.ReadAllText(SETTINGS_FILE);

            var settings = JsonSerializer.Deserialize<Settings>(json, JsonOptions);

            if (settings == null)
            {
                Console.WriteLine($"Čtení ze souboru {SETTINGS_FILE} selhalo.");

                return 1;
            }

            // --- HTTP klient ---
            using HttpClient http = new HttpClient();

            http.DefaultRequestHeaders.Add("X-Consumer-Key", CONSUMER_KEY);
            http.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var login = await Login(http, settings);

            if (login)
            {
                Console.WriteLine("Vyber možnost:");
                Console.WriteLine("1 - Vyhledat");
                Console.WriteLine("2 - Stáhnout");

                var choice = Console.ReadLine();

                if (choice == "1")
                {
                    var adventures = await SearchLabs(http, settings);

                    var simplified = adventures.Select(a => new
                    {
                        adventureGuid = a.AdventureGuid,
                        title = a.Title
                    });


                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    };

                    string outJson = JsonSerializer.Serialize(simplified, options);

                    File.WriteAllText("adventures.json", outJson);

                }
                else if (choice == "2")
                {
                    string inJson = File.ReadAllText("adventures.json");

                    var adventuresGuids = JsonSerializer.Deserialize<AdventureSummary2[]>(inJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    });

                    if (adventuresGuids != null)
                    {
                        foreach (var a in adventuresGuids)
                        {
                            Console.WriteLine(a.AdventureGuid);
                        }
                    }

                    // var gpx = await CreateGpx(allLabs, http, settings);
                }
                else
                {
                    //
                }
               

              

                return 0;
            }

            return 1;
        }

        /// <summary>
        /// Přihlášení
        /// 
        /// </summary>
        /// <param name="http"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        internal static async Task<bool> Login(HttpClient http, Settings settings)
        {
            Console.WriteLine("\n[1/3] Prihlasuji se...");

            var loginBody = JsonSerializer.Serialize(new { Username = settings.UserName, Password = settings.UserPassword });

            var loginResp = await http.PostAsync(LOGIN_URL, new StringContent(loginBody, Encoding.UTF8, "application/json"));

            if (!loginResp.IsSuccessStatusCode)
            {
                Console.WriteLine($"Prihlaseni selhalo: {loginResp.StatusCode}");
                Console.WriteLine(await loginResp.Content.ReadAsStringAsync());

                return false;
            }

            var loginData = JsonSerializer.Deserialize<LoginResponse>(await loginResp.Content.ReadAsStringAsync(), JsonOptions)!;
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", loginData.AccessToken);

            Console.WriteLine("  OK - Prihlaseni OK");

            return true;
        }

        internal static async Task<List<AdventureSummary>> SearchLabs(HttpClient http, Settings settings)
        {

            // --- 2. Hledani ---
            Console.WriteLine($"\n[2/3] Hledam Adventure Labs (lat={settings.Latitude}, lon={settings.Longitute}, radius={settings.Radius}m, limit={settings.Limit})...");

            var searchBody = JsonSerializer.Serialize(new
            {
                Origin = new { Latitude = settings.Latitude, Longitude = settings.Longitute },
                RadiusInMeters = settings.Radius,
                Take = settings.Limit,
                CompletionStatuses = new[] { 0, 1, 2 }, // 0=NotStarted, 1=InProgress, vynechava 2=Completed
               // IsComplete = false,
                OnlyHighlyRecommended = false,
                AdventureTypes = Array.Empty<object>(),
                MedianCompletionTimes = Array.Empty<object>(),
                Themes = Array.Empty<object>(),
                ExcludeOwned = false
            });

            var searchResp = await http.PostAsync(SEARCH_URL, new StringContent(searchBody, Encoding.UTF8, "application/json"));

            if (!searchResp.IsSuccessStatusCode)
            {
                Console.WriteLine($"Hledani selhalo: {searchResp.StatusCode}");
                Console.WriteLine(await searchResp.Content.ReadAsStringAsync());

                return new List<AdventureSummary>();
            }

            var searchJson = await searchResp.Content.ReadAsStringAsync();
            var searchResult = JsonSerializer.Deserialize<SearchResponse>(searchJson, JsonOptions);
            var allLabs = searchResult?.Items ?? new List<AdventureSummary>();

            return allLabs;
        }


        internal static async Task DownloadLab(HttpClient http, List<AdventureSummary> adventures)
        {
            // --- Filtr: pouze nedokoncene ---
            /* var adventures = allLabs.Where(a => a.CompletionStatus != "Completed").ToList();

             Console.WriteLine($"  Celkem v oblasti:        {allLabs.Count}");
             Console.WriteLine($"  Po filtru (nedokoncene): {adventures.Count}");
             foreach (var g in allLabs.GroupBy(a => a.CompletionStatus ?? "null").OrderBy(g => g.Key))
             {
                 Console.WriteLine($"    {g.Key}: {g.Count()}x");
             }*/


            int processed = 0, failed = 0;

            foreach (var adv in adventures)
            {
                try
                {
                    await Task.Delay(300);

                    var detailResp = await http.GetAsync(DETAILS_URL + adv.AdventureGuid);
                    if (!detailResp.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"  Preskocen {adv.Title} ({detailResp.StatusCode})");
                        failed++;
                        continue;
                    }

                    var detailJson = await detailResp.Content.ReadAsStringAsync();

                    // DEBUG - zobraz prvni detail jednou
                    if (processed == 0 && failed == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        // Console.WriteLine("\n  [DEBUG] Detail prvniho labu (prvnich 3000 znaku):");
                        // Console.WriteLine(detailJson.Length > 3000 ? detailJson[..3000] + "..." : detailJson);
                        Console.WriteLine(detailJson);
                        Console.ResetColor();
                    }

                    var detail = JsonSerializer.Deserialize<AdventureDetail>(detailJson, JsonOptions)!;

                    string shortId = (adv.AdventureGuid ?? "").Replace("-", "")[..8].ToUpper();

                    // Hlavni waypoint
                    /*  root.Add(Helpers.MakeWpt(gpxNs, gsNs,
                          id: "AL" + shortId,
                          name: detail.Title ?? adv.Title ?? "?",
                          lat: detail.Location?.Latitude ?? adv.Location?.Latitude ?? 0,
                          lon: detail.Location?.Longitude ?? adv.Location?.Longitude ?? 0,
                          desc: Helpers.Strip(detail.Description ?? ""),
                         // owner: detail.Owner?.Name ?? "",
                         owner: detail.OwnerUsername ?? "",
                          extra: $"Adventure Lab, {detail.StageSummaries?.Count ?? 0} zastavek"
                      ));*/

                    // Stage waypointy
                    int stageNum = 1;
                    /*  foreach (var stage in detail.Locations ?? new List<Stage>())
                      {
                          string stageDesc = Helpers.Strip(stage.Description ?? "");
                          if (!string.IsNullOrEmpty(stage.Question))
                              stageDesc += $"\n\nOtazka: {Helpers.Strip(stage.Question)}";

                          root.Add(Helpers.MakeWpt(gpxNs, gsNs,
                              id: $"AL{shortId}{stageNum:D2}",
                              name: $"{detail.Title} - {stageNum}/{detail.Locations!.Count}: {stage.Title ?? "Stage " + stageNum}",
                              lat: stage.Location?.Latitude ?? 0,
                              lon: stage.Location?.Longitude ?? 0,
                              desc: stageDesc,
                              owner: detail.Owner?.Name ?? "",
                              extra: ""
                          ));
                          stageNum++;
                      }*/

                    processed++;
                    // Console.WriteLine($"  [{processed}/{adventures.Count}] {detail.Title} ({detail.Locations?.Count ?? 0} stagi)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Chyba u {adv.Title}: {ex.Message}");
                    failed++;
                }
            }
        }

        internal static async Task<AdventureDetail> ReadStages(AdventureSummary adventure, HttpClient http)
        {
            try
            {
                await Task.Delay(300);

                var detailResp = await http.GetAsync(DETAILS_URL + adventure.AdventureGuid);

                if (!detailResp.IsSuccessStatusCode)
                {
                    throw new Exception($"Error code:{detailResp.StatusCode}: {detailResp.RequestMessage}");
                }

                var detailJson = await detailResp.Content.ReadAsStringAsync();

                var detail = JsonSerializer.Deserialize<AdventureDetail>(detailJson, JsonOptions)!;

                // string shortId = (adventure.AdventureGuid ?? "").Replace("-", "")[..8].ToUpper();

                // Console.WriteLine($"  [{processed}/{adventures.Count}] {detail.Title} ({detail.Locations?.Count ?? 0} stagi)");

                return detail;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Chyba u {adventure.Title}: {ex.Message}");
            }

            return default;
        }

        internal static async Task<int> CreateGpx(List<AdventureSummary> allLabs, HttpClient http, Settings settings)
        {
            // --- 3. Detaily + GPX ---
            Console.WriteLine("\n[3/3] Generuji GPX...");

            XNamespace gpxNs = "http://www.topografix.com/GPX/1/0";
            XNamespace gsNs = "http://www.groundspeak.com/cache/1/0/1";

            var root = new XElement(gpxNs + "gpx",
                new XAttribute("version", "1.0"),
                new XAttribute("creator", "Lab2Gpx C#"),
                new XAttribute(XNamespace.Xmlns + "groundspeak", gsNs),
                new XAttribute("xmlns", gpxNs)
            );



            foreach (var adv in allLabs)
            {
                try
                {
                    await Task.Delay(300);

                    var detailResp = await http.GetAsync(DETAILS_URL + adv.AdventureGuid);
                    if (!detailResp.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"  Preskocen {adv.Title} ({detailResp.StatusCode})");
                        //  failed++;
                        continue;
                    }

                    var detailJson = await detailResp.Content.ReadAsStringAsync();

                    // DEBUG - zobraz prvni detail jednou
                    /*  if (processed == 0 && failed == 0)
                      {
                          Console.ForegroundColor = ConsoleColor.DarkGray;
                          // Console.WriteLine("\n  [DEBUG] Detail prvniho labu (prvnich 3000 znaku):");
                          // Console.WriteLine(detailJson.Length > 3000 ? detailJson[..3000] + "..." : detailJson);
                          Console.WriteLine(detailJson);
                          Console.ResetColor();
                      }*/

                    var detail = JsonSerializer.Deserialize<AdventureDetail>(detailJson, JsonOptions)!;

                    string shortId = (adv.AdventureGuid ?? "").Replace("-", "")[..8].ToUpper();

                    // Hlavni waypoint
                    root.Add(Helpers.MakeWpt(gpxNs, gsNs,
                        id: "AL" + shortId,
                        name: detail.Title ?? adv.Title ?? "?",
                        lat: detail.Location?.Latitude ?? adv.Location?.Latitude ?? 0,
                        lon: detail.Location?.Longitude ?? adv.Location?.Longitude ?? 0,
                        desc: Helpers.Strip(detail.Description ?? ""),
                       // owner: detail.Owner?.Name ?? "",
                       owner: detail.OwnerUsername ?? "",
                        extra: $"Adventure Lab, {detail.StageSummaries?.Count ?? 0} zastavek"
                    ));

                    // Stage waypointy
                    int stageNum = 1;
                    /*  foreach (var stage in detail.Locations ?? new List<Stage>())
                      {
                          string stageDesc = Helpers.Strip(stage.Description ?? "");
                          if (!string.IsNullOrEmpty(stage.Question))
                              stageDesc += $"\n\nOtazka: {Helpers.Strip(stage.Question)}";

                          root.Add(Helpers.MakeWpt(gpxNs, gsNs,
                              id: $"AL{shortId}{stageNum:D2}",
                              name: $"{detail.Title} - {stageNum}/{detail.Locations!.Count}: {stage.Title ?? "Stage " + stageNum}",
                              lat: stage.Location?.Latitude ?? 0,
                              lon: stage.Location?.Longitude ?? 0,
                              desc: stageDesc,
                              owner: detail.Owner?.Name ?? "",
                              extra: ""
                          ));
                          stageNum++;
                      }*/

                    //   processed++;
                    // Console.WriteLine($"  [{processed}/{adventures.Count}] {detail.Title} ({detail.Locations?.Count ?? 0} stagi)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Chyba u {adv.Title}: {ex.Message}");
                    // failed++;
                }
            }
            var a = 1;

            new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root).Save(settings.OutputFileName);
            Console.WriteLine($"\nHotovo! Ulozeno: {Path.GetFullPath(settings.OutputFileName)}");
            // Console.WriteLine($"  {processed} Adventure Labs, {failed} chyb");

            return 0;
        }
    }

}