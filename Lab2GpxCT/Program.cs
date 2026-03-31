// Lab2Gpx.cs - Adventure Lab → GPX downloader
// .NET 8+ Console App, žádné NuGet balíčky nejsou potřeba
// Použití: dotnet run -- --lat 50.0755 --lon 14.4378 --radius 15000 --output labs.gpx

//https://api.groundspeak.com/api-docs/index
//https://api.groundspeak.com/documentation#adventure-stages

namespace Lab2Gpx
{
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

            // HTTP klient
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

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    };

                    string outJson = JsonSerializer.Serialize(adventures, options);
                    File.WriteAllText("adventures.json", outJson);
                }
                else if (choice == "2")
                {
                    string inJson = File.ReadAllText("adventures.json");

                    var option = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    };

                    var adventures = JsonSerializer.Deserialize<AdventureSummary[]>(inJson, option);

                    if (adventures != null)
                    {
                        var details = new List<AdventureDetail>();

                        foreach (var adventure in adventures)
                        {
                            Console.WriteLine(adventure.Title);

                            var adventureLab = await DownloadLab(http, adventure);

                            details.Add(adventureLab);

                        }

                        var gpx = CreateGpx(http, details, settings);

                        try
                        {
                            var options = new JsonSerializerOptions
                            {
                                WriteIndented = true,
                                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                            };

                            string outJson = JsonSerializer.Serialize(details, options);
                            File.WriteAllText("adventuresX.json", outJson);


                            var htmlContent = AdventureHtmlGenerator.Generate(details.First());

                            await File.WriteAllTextAsync("index.html", htmlContent, Encoding.UTF8);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
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
            bool result = false;

            try
            {
                Console.WriteLine($"\nLoging {settings.UserName} in to the server");

                var loginBody = JsonSerializer.Serialize(new { Username = settings.UserName, Password = settings.UserPassword });

                var loginResp = await http.PostAsync(LOGIN_URL, new StringContent(loginBody, Encoding.UTF8, "application/json"));

                var loginContent = await loginResp.Content.ReadAsStringAsync();

                if (!loginResp.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Login failed {loginResp.StatusCode}:{loginResp.ReasonPhrase}:{loginContent}");
                }
                else
                {
                    var loginData = JsonSerializer.Deserialize<LoginResponse>(loginContent, JsonOptions);

                    if (loginData == null)
                    {
                        throw new Exception($"Failed to deserialize response: {loginContent}");
                    }

                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", loginData.AccessToken);

                    Console.WriteLine("Login OK");

                    result = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {nameof(Login)} {ex.Message}:{ex.Data}:{ex.StackTrace}");
            }

            return result;
        }

        /// <summary>
        /// Vyhledání
        /// 
        /// </summary>
        /// <param name="http"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        internal static async Task<IEnumerable<AdventureSummary>> SearchLabs(HttpClient http, Settings settings)
        {
            IEnumerable<AdventureSummary> result = [];

            try
            {
                Console.WriteLine($"\nSearching Adventure Labs (lat={settings.Latitude}, lon={settings.Longitude}, radius={settings.Radius}m, limit={settings.Limit})...");

                var searchBody = JsonSerializer.Serialize(new
                {
                    Origin = new { Latitude = settings.Latitude, Longitude = settings.Longitude },
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

                var searchContent = await searchResp.Content.ReadAsStringAsync();

                if (!searchResp.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Searching failed: {searchResp.StatusCode}:{searchResp.ReasonPhrase}:{searchContent}");
                }
                else
                {
                    var searchData = JsonSerializer.Deserialize<SearchResponse>(searchContent, JsonOptions);

                    if (searchData == null)
                    {
                        throw new Exception($"Failed to deserialize response: {searchContent}");
                    }

                    result = searchData?.Items ?? [];

                    Console.WriteLine($"Searching {searchData?.TotalCount} Labs OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {nameof(SearchLabs)} {ex.Message}:{ex.Data}:{ex.StackTrace}");
            }

            return result;
        }

        /// <summary>
        /// Detail info Labky
        /// 
        /// </summary>
        /// <param name="http"></param>
        /// <param name="adventureSummary"></param>
        /// <returns></returns>
        internal static async Task<AdventureDetail> DownloadLab(HttpClient http, AdventureSummary adventureSummary)
        {
            AdventureDetail? result = null;

            try
            {
                await Task.Delay(300);

                var detailResp = await http.GetAsync(DETAILS_URL + adventureSummary.AdventureGuid ?? "");

                var detailContent = await detailResp.Content.ReadAsStringAsync();

                if (!detailResp.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Load {adventureSummary.Title} detail failed: {detailResp.StatusCode}:{detailResp.ReasonPhrase}:{detailContent}");
                }
                else
                {
                    var detailData = JsonSerializer.Deserialize<AdventureDetail>(detailContent, JsonOptions);

                    if (detailData == null)
                    {
                        throw new Exception($"Failed to deserialize response: {detailContent}");
                    }

                    result = detailData;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {nameof(DownloadLab)} {adventureSummary.Title}: {ex.Message}:{ex.Data}:{ex.StackTrace}");
            }

            return result;
        }

        internal static bool CreateGpx(HttpClient http, IEnumerable<AdventureDetail> allLabs, Settings settings)
        {
            bool result = false;

            try
            {
                Console.WriteLine("\nGenerating GPX...");

                XNamespace gpxNs = "http://www.topografix.com/GPX/1/0";
                XNamespace gsNs = "http://www.groundspeak.com/cache/1/0/1";

                var root = new XElement(gpxNs + "gpx",
                    new XAttribute("version", "1.0"),
                    new XAttribute("creator", "Lab2Gpx C#"),
                    new XAttribute(XNamespace.Xmlns + "groundspeak", gsNs),
                    new XAttribute("xmlns", gpxNs)
                );

                foreach (var adv in allLabs ?? [])
                {
                    try
                    {
                        string shortId = (adv.AdventureGuid ?? "").Replace("-", "")[..8].ToUpper();

                        // Hlavni waypoint
                        root.Add(Helpers.MakeWpt(gpxNs, gsNs,
                            id: "AL" + shortId,
                            name: adv.Title ?? adv.Title ?? "?",
                            lat: adv.Location?.Latitude ?? adv.Location?.Latitude ?? 0,
                            lon: adv.Location?.Longitude ?? adv.Location?.Longitude ?? 0,
                            desc: Helpers.Strip(adv.Description ?? ""),
                           // owner: detail.Owner?.Name ?? "",
                           owner: adv.OwnerUsername ?? "",
                            extra: $"Adventure Lab, {adv.StageSummaries?.Count ?? 0} zastavek"
                        ));

                        // Stage waypointy
                        int stageNum = 1;
                        foreach (var stage in adv.StageSummaries ?? new List<StageSummary>())
                        {
                            string stageDesc = Helpers.Strip(stage.Description ?? "");
                            if (!string.IsNullOrEmpty(stage.Question))
                                stageDesc += $"\n\nOtazka: {Helpers.Strip(stage.Question)}";

                            root.Add(Helpers.MakeWpt(gpxNs, gsNs,
                                id: $"AL{shortId}{stageNum:D2}",
                                name: $"{adv.Title} - {stageNum}/{adv.StageSummaries!.Count}: {stage.Title ?? "Stage " + stageNum}",
                                lat: stage.Location?.Latitude ?? 0,
                                lon: stage.Location?.Longitude ?? 0,
                                desc: stageDesc,
                                owner: adv.OwnerUsername ?? "",
                                extra: ""
                            ));
                            stageNum++;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"{adv.Title}", ex.InnerException);
                    }
                }

                new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root).Save(settings.OutputFileName);
                Console.WriteLine($"\nHotovo! Ulozeno: {Path.GetFullPath(settings.OutputFileName)}");

                result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {nameof(CreateGpx)} {ex.Message}:{ex.Data}:{ex.StackTrace}");
            }

            return result;
        }
    }

}