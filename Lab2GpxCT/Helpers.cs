

namespace Lab2Gpx
{
    using System;
    using System.Text;
    using System.Xml.Linq;

    internal class Helpers
    {

        internal static XElement MakeWpt(XNamespace gpxNs, XNamespace gsNs, string id, string name, double lat, double lon, string desc, string owner, string extra)
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

        internal static string Strip(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return "";
            }

            var s = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", "");

            return System.Net.WebUtility.HtmlDecode(s).Trim();
        }

        internal static T Ask<T>(string prompt) where T : IParsable<T>
        {
            Console.Write(prompt + " ");

            return T.Parse(Console.ReadLine()!.Trim(), System.Globalization.CultureInfo.InvariantCulture);
        }

        internal static string ReadPassword()
        {
            var sb = new StringBuilder();

            while (true)
            {
                var key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                }
                else if (key.Key != ConsoleKey.Backspace)
                {
                    sb.Append(key.KeyChar);
                }
            }

            Console.WriteLine();

            return sb.ToString();
        }
    }
}

