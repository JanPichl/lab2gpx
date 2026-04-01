// Lab2Gpx.cs - Adventure Lab → GPX downloader
// .NET 8+ Console App, žádné NuGet balíčky nejsou potřeba
// Použití: dotnet run -- --lat 50.0755 --lon 14.4378 --radius 15000 --output labs.gpx

//https://api.groundspeak.com/api-docs/index
//https://api.groundspeak.com/documentation#adventure-stages
//https://labs-api.geocaching.com/swagger/ui/index#/
//https://labs-api.geocaching.com/swagger/ui/index#!/Adventures/Adventures_Get


namespace Lab2Gpx
{
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
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

        internal const string LABS_API_DETAILS_URL = "https://labs-api.geocaching.com/Api/Adventures/";


        internal static readonly JsonSerializerOptions SerializerOptionsA = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private static JsonSerializerOptions SerializerOptionsB = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        public static string AccesToken { get; set; }

        private static int _step = 0;

        /// <summary>
        /// Main fce
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static async Task<int> Main(string[] args)
        {
            var json = File.ReadAllText(SETTINGS_FILE);

            var settings = JsonSerializer.Deserialize<Settings>(json, SerializerOptionsA);

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
                do
                {
                    Console.WriteLine();
                    Console.WriteLine("   Vyber možnost:  ");
                    Console.WriteLine("-------------------");
                    Console.WriteLine("| 0 - Exit        |");
                    Console.WriteLine("| 1 - Vyhledat    |");
                    Console.WriteLine("| 2 - Stáhnout    |");
                    Console.WriteLine("| 3 - Stáhnout V2 |");
                    Console.WriteLine("| 4 - Nastavení   |");
                    Console.WriteLine("-------------------");

                    Console.Write($"\n #{_step++} > ");

                    var choice = Console.ReadLine();

                    Console.WriteLine();

                    if (choice == "0")
                    {
                        break;
                    }
                    if (choice == "1")
                    {
                        var adventures = await SearchLabs(http, settings);

                        foreach (var x in adventures ?? [])
                        {
                            Console.WriteLine($"{x.Title}");
                        }

                        string outJson = JsonSerializer.Serialize(adventures, SerializerOptionsB);
                        File.WriteAllText("adventures.json", outJson);
                    }
                    else if (choice == "2")
                    {
                        string inJson = File.ReadAllText("adventures.json");

                        var adventures = JsonSerializer.Deserialize<AdventureSummary[]>(inJson, SerializerOptionsB);

                        if (adventures != null)
                        {
                            var details = new List<AdventureDetail>();

                            foreach (var adventure in adventures)
                            {
                                Console.WriteLine(adventure.Title);

                                var adventureLab = await DownloadLab(http, adventure);

                                details.Add(adventureLab);

                            }

                            var gpx = GpxGenerator.CreateGpx(http, details, settings);

                            try
                            {
                                string outJson = JsonSerializer.Serialize(details, SerializerOptionsB);
                                File.WriteAllText("adventuresX.json", outJson);


                                var htmlContent = HtmlGenerator.Generate(details.First());

                                await File.WriteAllTextAsync("index.html", htmlContent, Encoding.UTF8);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                    else if (choice == "3")
                    {
                        string inJson = File.ReadAllText("adventures.json");

                        var adventures = JsonSerializer.Deserialize<AdventureSummary[]>(inJson, SerializerOptionsB);

                        if (adventures != null)
                        {
                            var details = new List<AdventureDetailV2>();

                            foreach (var adventure in adventures)
                            {
                                Console.WriteLine(adventure.Title);

                                var adventureLab = await DownloadLabV2(http, adventure);

                                details.Add(adventureLab);

                                await DownloadLabImages(http, adventureLab);
                            }

                            try
                            {
                                string outJson = JsonSerializer.Serialize(details, SerializerOptionsB);
                                File.WriteAllText("adventuresV2.json", outJson);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }


                } while (true);

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
                Console.WriteLine($"Loging {settings.UserName} in to the server...\n");

                var body = JsonSerializer.Serialize(new
                {
                    Username = settings.UserName,
                    Password = settings.UserPassword
                });

                var response = await http.PostAsync(LOGIN_URL, new StringContent(body, Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    await ProcessingFailedRequest("Login", response);
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var loginData = JsonSerializer.Deserialize<LoginResponse>(content, SerializerOptionsA);

                    if (loginData == null)
                    {
                        throw new Exception($"Login failed to deserialize response: {content}");
                    }

                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", loginData.AccessToken);

                    AccesToken = loginData.AccessToken;

                    File.WriteAllText("token.txt", AccesToken);

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
                Console.WriteLine($"Searching Adventure Labs on (lat={settings.Latitude}, lon={settings.Longitude}, radius={settings.Radius}m, limit={settings.Limit})...\n");

                var body = JsonSerializer.Serialize(new
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

                var response = await http.PostAsync(SEARCH_URL, new StringContent(body, Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    await ProcessingFailedRequest("Search", response);
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var data = JsonSerializer.Deserialize<SearchResponse>(content, SerializerOptionsA);

                    if (data == null)
                    {
                        throw new Exception($"Failed to deserialize response: {content}");
                    }

                    result = data?.Items ?? [];

                    Console.WriteLine($"Found {data?.TotalCount} Adventure Lab - OK");
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

                var response = await http.GetAsync(DETAILS_URL + adventureSummary.AdventureGuid ?? "");

                if (!response.IsSuccessStatusCode)
                {
                    await ProcessingFailedRequest($"Download {adventureSummary.Title}", response);
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var data = JsonSerializer.Deserialize<AdventureDetail>(content, SerializerOptionsA);

                    if (data == null)
                    {
                        throw new Exception($"Failed to deserialize response: {content}");
                    }

                    result = data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {nameof(DownloadLab)} {adventureSummary.Title}: {ex.Message}:{ex.Data}:{ex.StackTrace}");
            }

            return result;
        }

        /// <summary>
        /// Stáhne detail Adventure Lab z labs-api.geocaching.com a uloží výsledky do adventures2.json
        /// </summary>
        internal static async Task<AdventureDetailV2> DownloadLabV2(HttpClient http, AdventureSummary adventure)
        {
            AdventureDetailV2? result = null;

            try
            {
                await Task.Delay(300);

                var response = await http.GetAsync(LABS_API_DETAILS_URL + adventure.AdventureGuid);

                if (!response.IsSuccessStatusCode)
                {
                    await ProcessingFailedRequest("DownloadV2", response);
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var data = JsonSerializer.Deserialize<AdventureDetailV2>(content, SerializerOptionsA);

                    if (data == null)
                    {
                        throw new Exception($"Failed to deserialize response: {content}");
                    }

                    result = data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {nameof(DownloadLab)} {adventure.Title}: {ex.Message}:{ex.Data}:{ex.StackTrace}");
            }

            return result;
        }

        internal static async Task DownloadLabImages(HttpClient http, AdventureDetailV2 detail)
        {
            if (detail?.Title == null) return;

            // Vytvoř bezpečný název složky z titulu
            string folderName = string.Concat(detail.Title.Split(Path.GetInvalidFileNameChars()));
            Directory.CreateDirectory(folderName);

            // Stáhni hlavní obrázek labky
            if (!string.IsNullOrEmpty(detail.KeyImageUrl))
            {
                await DownloadImage(http, detail.KeyImageUrl, Path.Combine(folderName, "cover.jpg"));
            }

            // Stáhni obrázky jednotlivých stagí
            if (detail.GeocacheSummaries != null)
            {
                foreach (var stage in detail.GeocacheSummaries)
                {
                    //Key
                    if (string.IsNullOrEmpty(stage.KeyImageUrl) || string.IsNullOrEmpty(stage.Id))
                        continue;

                    string fileName = $"{"K_"}{stage.Title ?? stage.Id}.jpg";
                    string safeFileName = string.Concat(fileName.Split(Path.GetInvalidFileNameChars()));

                    await DownloadImage(http, stage.KeyImageUrl, Path.Combine(folderName, safeFileName));

                    //Award
                    if (string.IsNullOrEmpty(stage.AwardImageUrl) || string.IsNullOrEmpty(stage.Id))
                        continue;

                    string fileNameA = $"{"A_"}{stage.Title ?? stage.Id}.jpg";
                    string safeFileNamA = string.Concat(fileNameA.Split(Path.GetInvalidFileNameChars()));

                    await DownloadImage(http, stage.AwardImageUrl, Path.Combine(folderName, safeFileNamA));

                }
            }
        }

        internal static async Task DownloadImage(HttpClient http, string url, string filePath)
        {
            try
            {
                await Task.Delay(300);

                var savedHeader = http.DefaultRequestHeaders.Authorization;

                // Dočasně odstraňit Authorization header - Azure Blob ho odmítá
                http.DefaultRequestHeaders.Authorization = null;

                var response = await http.GetAsync(url);

                http.DefaultRequestHeaders.Authorization = savedHeader;

                if (!response.IsSuccessStatusCode)
                {
                    await ProcessingFailedRequest("Download Image", response);
                }
                else
                {
                    var data = await response.Content.ReadAsByteArrayAsync();

                    await File.WriteAllBytesAsync(filePath, data);

                    Console.WriteLine($"  Uloženo: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {nameof(DownloadImage)} {url}: {ex.Message}");
            }
        }

        internal static async Task ProcessingFailedRequest(string name, HttpResponseMessage response)
        {
            Console.WriteLine($"{name} failed: {(int)response.StatusCode} {response.ReasonPhrase}\n");
            Console.WriteLine($"{name} url: {response.RequestMessage?.RequestUri}\n");

            var b = response?.RequestMessage?.Content;
            if (b != null)
            {
                var body = await b.ReadAsStringAsync();
                Console.WriteLine($"{name} body: {body}\n");
            }

            var c = response?.Content;
            if (c != null)
            {
                if (c?.Headers?.ContentType?.MediaType == "application/json")
                {
                    var content = await c.ReadAsStringAsync();
                    Console.WriteLine($"{name} response: {content}\n");
                }
                else 
                {     
                    Console.WriteLine($"{name} response: -");
                }

            }

        }

    }
}