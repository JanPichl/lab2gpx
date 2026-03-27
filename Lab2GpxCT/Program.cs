// Lab2Gpx.cs - Adventure Lab → GPX downloader
// .NET 8+ Console App, žádné NuGet balíčky nejsou potřeba
// Použití: dotnet run -- --lat 50.0755 --lon 14.4378 --radius 15000 --output labs.gpx

//https://api.groundspeak.com/api-docs/index

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

const string ConsumerKey = "A01A9CA1-29E0-46BD-A270-9D894A527B91";
const string UserAgent = "Adventures/1.56.0 (4936) (android/32)";
const string LoginUrl = "https://api.groundspeak.com/adventuresmobile/v1/public/accounts/login";
const string SearchUrl = "https://api.groundspeak.com/adventuresmobile/v1/public/adventures/search";
const string DetailsBase = "https://api.groundspeak.com/adventuresmobile/v1/public/adventures/";

// --- argumenty ---
var argsDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
for (int i = 0; i < args.Length - 1; i++)
    if (args[i].StartsWith("--"))
        argsDict[args[i][2..]] = args[i + 1];

double lat = argsDict.TryGetValue("lat", out var sLat) ? double.Parse(sLat, System.Globalization.CultureInfo.InvariantCulture) : Ask<double>("Zemepisna sirka (lat):");
double lon = argsDict.TryGetValue("lon", out var sLon) ? double.Parse(sLon, System.Globalization.CultureInfo.InvariantCulture) : Ask<double>("Zemepisna delka (lon):");
int radius = argsDict.TryGetValue("radius", out var sRad) ? int.Parse(sRad) : Ask<int>("Radius v metrech:");
int limit = argsDict.TryGetValue("limit", out var sLimit) ? int.Parse(sLimit) : 300;
string output = argsDict.TryGetValue("output", out var sOut) ? sOut : "labs.gpx";

lat = 48.9910231d;
lon = 14.4653933d;
radius = 500;



string username, password;
if (argsDict.TryGetValue("user", out var u) && argsDict.TryGetValue("pass", out var p))
{
    username = u; password = p;
}
else
{
    Console.Write("Geocaching uživatelské jméno: ");
    //username = Console.ReadLine()!.Trim();
    username = "Pichlík";
    Console.Write("Heslo: ");
    //password = ReadPassword();
    password = "JendaPicha*1980";
}

// --- HTTP klient ---
var jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

using var http = new HttpClient();
http.DefaultRequestHeaders.Add("X-Consumer-Key", ConsumerKey);
http.DefaultRequestHeaders.Add("User-Agent", UserAgent);
http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

// --- 1. Prihlaseni ---
Console.WriteLine("\n[1/3] Prihlasuji se...");
var loginBody = JsonSerializer.Serialize(new { Username = username, Password = password });
var loginResp = await http.PostAsync(LoginUrl, new StringContent(loginBody, Encoding.UTF8, "application/json"));
if (!loginResp.IsSuccessStatusCode)
{
    Console.WriteLine($"Prihlaseni selhalo: {loginResp.StatusCode}");
    Console.WriteLine(await loginResp.Content.ReadAsStringAsync());
    return 1;
}
var loginData = JsonSerializer.Deserialize<LoginResponse>(
    await loginResp.Content.ReadAsStringAsync(), jsonOpts)!;
http.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("bearer", loginData.AccessToken);
Console.WriteLine("  OK - Prihlaseni OK");

// --- 2. Hledani ---
Console.WriteLine($"\n[2/3] Hledam Adventure Labs (lat={lat}, lon={lon}, radius={radius}m, limit={limit})...");
var searchBody = JsonSerializer.Serialize(new
{
    Origin = new { Latitude = lat, Longitude = lon },
    RadiusInMeters = radius,
    Take = limit,
    CompletionStatuses = new[] { 0, 1, 2 }, // 0=NotStarted, 1=InProgress, vynechava 2=Completed
    IsComplete = false,
    OnlyHighlyRecommended = false,
    AdventureTypes = Array.Empty<object>(),
    MedianCompletionTimes = Array.Empty<object>(),
    Themes = Array.Empty<object>(),
    ExcludeOwned = false
});
var searchResp = await http.PostAsync(
    SearchUrl, new StringContent(searchBody, Encoding.UTF8, "application/json"));
if (!searchResp.IsSuccessStatusCode)
{
    Console.WriteLine($"Hledani selhalo: {searchResp.StatusCode}");
    Console.WriteLine(await searchResp.Content.ReadAsStringAsync());
    return 1;
}
var searchJson = await searchResp.Content.ReadAsStringAsync();
var searchResult = JsonSerializer.Deserialize<SearchResponse>(searchJson, jsonOpts);
var allLabs = searchResult?.Items ?? new List<AdventureSummary>();

// --- Filtr: pouze nedokoncene ---
var adventures = allLabs
    .Where(a => a.CompletionStatus != "Completed")
    .ToList();

Console.WriteLine($"  Celkem v oblasti:        {allLabs.Count}");
Console.WriteLine($"  Po filtru (nedokoncene): {adventures.Count}");
foreach (var g in allLabs.GroupBy(a => a.CompletionStatus ?? "null").OrderBy(g => g.Key))
    Console.WriteLine($"    {g.Key}: {g.Count()}x");

// --- 3. Detaily + GPX ---
Console.WriteLine("\n[3/3] Stahuji detaily a generuji GPX...");

XNamespace gpxNs = "http://www.topografix.com/GPX/1/0";
XNamespace gsNs = "http://www.groundspeak.com/cache/1/0/1";
var root = new XElement(gpxNs + "gpx",
    new XAttribute("version", "1.0"),
    new XAttribute("creator", "Lab2Gpx C#"),
    new XAttribute(XNamespace.Xmlns + "groundspeak", gsNs),
    new XAttribute("xmlns", gpxNs)
);

int processed = 0, failed = 0;

foreach (var adv in adventures)
{
    try
    {
        await Task.Delay(300);
        var detailResp = await http.GetAsync(DetailsBase + adv.AdventureGuid);
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
            Console.WriteLine("\n  [DEBUG] Detail prvniho labu (prvnich 3000 znaku):");
            Console.WriteLine(detailJson.Length > 3000 ? detailJson[..3000] + "..." : detailJson);
            Console.ResetColor();
        }

        var detail = JsonSerializer.Deserialize<AdventureDetail>(detailJson, jsonOpts)!;

        string shortId = (adv.AdventureGuid ?? "").Replace("-", "")[..8].ToUpper();

        // Hlavni waypoint
        root.Add(MakeWpt(gpxNs, gsNs,
            id: "AL" + shortId,
            name: detail.Title ?? adv.Title ?? "?",
            lat: detail.Location?.Latitude ?? adv.Location?.Latitude ?? 0,
            lon: detail.Location?.Longitude ?? adv.Location?.Longitude ?? 0,
            desc: Strip(detail.Description ?? ""),
            owner: detail.Owner?.Name ?? "",
            extra: $"Adventure Lab, {detail.Locations?.Count ?? 0} zastavek"
        ));

        // Stage waypointy
        int stageNum = 1;
        foreach (var stage in detail.Locations ?? new List<Stage>())
        {
            string stageDesc = Strip(stage.Description ?? "");
            if (!string.IsNullOrEmpty(stage.Question))
                stageDesc += $"\n\nOtazka: {Strip(stage.Question)}";

            root.Add(MakeWpt(gpxNs, gsNs,
                id: $"AL{shortId}{stageNum:D2}",
                name: $"{detail.Title} - {stageNum}/{detail.Locations!.Count}: {stage.Title ?? "Stage " + stageNum}",
                lat: stage.Location?.Latitude ?? 0,
                lon: stage.Location?.Longitude ?? 0,
                desc: stageDesc,
                owner: detail.Owner?.Name ?? "",
                extra: ""
            ));
            stageNum++;
        }

        processed++;
        Console.WriteLine($"  [{processed}/{adventures.Count}] {detail.Title} ({detail.Locations?.Count ?? 0} stagi)");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Chyba u {adv.Title}: {ex.Message}");
        failed++;
    }
}

new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root).Save(output);
Console.WriteLine($"\nHotovo! Ulozeno: {Path.GetFullPath(output)}");
Console.WriteLine($"  {processed} Adventure Labs, {failed} chyb");
return 0;

// ── pomocne metody ───────────────────────────────────────────────────────────

static XElement MakeWpt(XNamespace gpxNs, XNamespace gsNs,
    string id, string name, double lat, double lon,
    string desc, string owner, string extra)
{
    var ic = System.Globalization.CultureInfo.InvariantCulture;
    return new XElement(gpxNs + "wpt",
        new XAttribute("lat", lat.ToString("F6", ic)),
        new XAttribute("lon", lon.ToString("F6", ic)),
        new XElement(gpxNs + "name", id),
        new XElement(gpxNs + "desc", name),
        new XElement(gpxNs + "sym", "Geocache"),
        new XElement(gpxNs + "type", "Geocache|Lab Cache"),
        new XElement(gsNs + "cache",
            new XAttribute("id", "0"),
            new XAttribute("available", "True"),
            new XAttribute("archived", "False"),
            new XElement(gsNs + "name", name),
            new XElement(gsNs + "placed_by", owner),
            new XElement(gsNs + "type", "Lab Cache"),
            new XElement(gsNs + "difficulty", "1.0"),
            new XElement(gsNs + "terrain", "1.0"),
            new XElement(gsNs + "short_description",
                new XAttribute("html", "False"), extra),
            new XElement(gsNs + "long_description",
                new XAttribute("html", "False"), desc)
        )
    );
}

static string Strip(string html)
{
    if (string.IsNullOrEmpty(html)) return "";
    var s = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", "");
    return System.Net.WebUtility.HtmlDecode(s).Trim();
}

static T Ask<T>(string prompt) where T : IParsable<T>
{
    Console.Write(prompt + " ");
    return T.Parse(Console.ReadLine()!.Trim(),
        System.Globalization.CultureInfo.InvariantCulture);
}

static string ReadPassword()
{
    var sb = new StringBuilder();
    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter) break;
        if (key.Key == ConsoleKey.Backspace && sb.Length > 0) sb.Remove(sb.Length - 1, 1);
        else if (key.Key != ConsoleKey.Backspace) sb.Append(key.KeyChar);
    }
    Console.WriteLine();
    return sb.ToString();
}

// ── modely ───────────────────────────────────────────────────────────────────

record LoginResponse(
    [property: JsonPropertyName("accessToken")] string AccessToken,
    [property: JsonPropertyName("refreshToken")] string RefreshToken,
    [property: JsonPropertyName("expiresIn")] int ExpiresIn
);

record SearchResponse(
    [property: JsonPropertyName("items")] List<AdventureSummary> Items,
    [property: JsonPropertyName("totalCount")] int TotalCount
);

record AdventureSummary(
    [property: JsonPropertyName("adventureGuid")] string? AdventureGuid,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("location")] LatLon? Location,
    [property: JsonPropertyName("completionStatus")] string? CompletionStatus
);

record AdventureDetail(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("location")] LatLon? Location,
    [property: JsonPropertyName("owner")] Owner? Owner,
    [property: JsonPropertyName("locations")] List<Stage>? Locations
);

record LatLon(
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude
);

record Owner(
    [property: JsonPropertyName("name")] string? Name
);

record Stage(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("question")] string? Question,
    [property: JsonPropertyName("location")] LatLon? Location
);