

namespace Lab2Gpx
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using static Lab2Gpx.Models;


    internal static class GpxGenerator
    {
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
