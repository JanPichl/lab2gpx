

namespace Lab2Gpx
{
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using static Lab2Gpx.Models;

    internal static class HtmlGenerator
    {

        /// <summary>
        /// Generuje HTML stránku z JSON stringu obsahujícího AdventureDetail.
        /// </summary>
        internal static string GenerateFromJson(string json)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var adventure = JsonSerializer.Deserialize<AdventureDetail>(json, options)
                ?? throw new ArgumentException("Nepodařilo se deserializovat AdventureDetail.");
            return Generate(adventure);
        }

        /// <summary>
        /// Generuje HTML stránku přímo z AdventureDetail objektu.
        /// </summary>
        internal static string Generate(AdventureDetail adventure)
        {
            var sb = new StringBuilder();

            var rating = adventure.RatingsAverage.ToString("0.0");
            var created = adventure.CreatedUtc?.ToString("d. M. yyyy") ?? "—";
            var published = adventure.PublishedUtc?.ToString("d. M. yyyy") ?? "—";
            var median = FormatDuration(adventure.MedianTimeToComplete);
            var themes = adventure.AdventureThemes is { Count: > 0 }
                                ? string.Join(", ", adventure.AdventureThemes)
                                : null;
            var visibility = Capitalize(adventure.Visibility);
            var adventureType = Capitalize(adventure.AdventureType);

            sb.AppendLine($$"""
<!DOCTYPE html>
<html lang="cs">
<head>
<meta charset="UTF-8" />
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>{{Escape(adventure.Title ?? "Adventura")}}</title>
<link rel="preconnect" href="https://fonts.googleapis.com" />
<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Playfair+Display:wght@600;700&family=DM+Sans:wght@300;400;500&display=swap" />
<style>
  *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

  :root {
    --bg:        #f7f4ef;
    --surface:   #ffffff;
    --border:    #e8e2d9;
    --text:      #1a1714;
    --muted:     #7a7069;
    --accent:    #c8602a;
    --accent2:   #3d6b4f;
    --gold:      #d4a843;
    --radius:    12px;
    --shadow:    0 2px 16px rgba(0,0,0,.07);
  }

  body {
    font-family: 'DM Sans', sans-serif;
    background: var(--bg);
    color: var(--text);
    min-height: 100vh;
    padding-bottom: 64px;
  }

  /* ── HERO ────────────────────────────────── */
  .hero {
    position: relative;
    width: 100%;
    max-height: 520px;
    overflow: hidden;
  }
  .hero img {
    width: 100%;
    height: 520px;
    object-fit: cover;
    display: block;
    filter: brightness(.88);
  }
  .hero-placeholder {
    width: 100%;
    height: 320px;
    background: linear-gradient(135deg, #d9cfc4 0%, #c4b89e 100%);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 64px;
    opacity: .5;
  }
  .hero-overlay {
    position: absolute;
    inset: 0;
    background: linear-gradient(to top, rgba(26,23,20,.72) 0%, transparent 55%);
  }
  .hero-content {
    position: absolute;
    bottom: 0; left: 0; right: 0;
    padding: 40px 48px 36px;
  }
  .hero-badges {
    display: flex;
    gap: 8px;
    flex-wrap: wrap;
    margin-bottom: 14px;
  }
  .badge {
    display: inline-flex;
    align-items: center;
    gap: 5px;
    padding: 4px 12px;
    border-radius: 100px;
    font-size: 12px;
    font-weight: 500;
    letter-spacing: .03em;
    text-transform: uppercase;
    backdrop-filter: blur(8px);
  }
  .badge-type    { background: rgba(200,96,42,.85);  color: #fff; }
  .badge-vis     { background: rgba(61,107,79,.85);  color: #fff; }
  .badge-arch    { background: rgba(180,160,120,.85); color: #fff; }
  .badge-recommended { background: rgba(212,168,67,.9); color: #1a1714; }

  .hero-title {
    font-family: 'Playfair Display', serif;
    font-size: clamp(28px, 4.5vw, 52px);
    font-weight: 700;
    color: #fff;
    line-height: 1.15;
    max-width: 720px;
    text-shadow: 0 2px 12px rgba(0,0,0,.35);
    margin-bottom: 10px;
  }
  .hero-owner {
    color: rgba(255,255,255,.75);
    font-size: 14px;
    font-weight: 300;
  }
  .hero-owner strong { color: #fff; font-weight: 500; }

  /* ── MAIN GRID ───────────────────────────── */
  .container {
    max-width: 1060px;
    margin: 0 auto;
    padding: 0 24px;
  }
  .main-grid {
    display: grid;
    grid-template-columns: 1fr 300px;
    gap: 32px;
    margin-top: 36px;
    align-items: start;
  }
  @media (max-width: 760px) {
    .main-grid { grid-template-columns: 1fr; }
    .hero-content { padding: 24px; }
  }

  /* ── CARD ────────────────────────────────── */
  .card {
    background: var(--surface);
    border: 1px solid var(--border);
    border-radius: var(--radius);
    box-shadow: var(--shadow);
    padding: 28px 32px;
    margin-bottom: 24px;
  }
  .card-title {
    font-family: 'Playfair Display', serif;
    font-size: 17px;
    font-weight: 600;
    margin-bottom: 18px;
    padding-bottom: 12px;
    border-bottom: 1px solid var(--border);
    color: var(--text);
  }

  /* ── DESCRIPTION ─────────────────────────── */
  .description {
    font-size: 15.5px;
    line-height: 1.75;
    color: #3c3630;
    font-weight: 300;
  }

  /* ── STATS ROW ───────────────────────────── */
  .stats-row {
    display: flex;
    gap: 0;
    background: var(--surface);
    border: 1px solid var(--border);
    border-radius: var(--radius);
    box-shadow: var(--shadow);
    overflow: hidden;
    margin-bottom: 24px;
  }
  .stat-item {
    flex: 1;
    padding: 20px 16px;
    text-align: center;
    border-right: 1px solid var(--border);
    position: relative;
  }
  .stat-item:last-child { border-right: none; }
  .stat-value {
    font-family: 'Playfair Display', serif;
    font-size: 26px;
    font-weight: 700;
    color: var(--accent);
    display: block;
    line-height: 1;
    margin-bottom: 4px;
  }
  .stat-label {
    font-size: 11px;
    color: var(--muted);
    text-transform: uppercase;
    letter-spacing: .05em;
    font-weight: 500;
  }
  .star { color: var(--gold); }

  /* ── STAGES ──────────────────────────────── */
  .stages-list { display: flex; flex-direction: column; gap: 14px; }
  .stage-item {
    display: grid;
    grid-template-columns: 40px 1fr auto;
    gap: 14px;
    align-items: center;
    padding: 16px 18px;
    border: 1px solid var(--border);
    border-radius: 10px;
    background: #faf9f7;
    transition: box-shadow .15s, border-color .15s;
    cursor: default;
  }
  .stage-item:hover {
    box-shadow: 0 4px 20px rgba(0,0,0,.08);
    border-color: #d0c8be;
  }
  .stage-item.complete { border-left: 3px solid var(--accent2); }
  .stage-item.final    { border-left: 3px solid var(--gold); }

  .stage-num {
    width: 40px; height: 40px;
    border-radius: 50%;
    display: flex; align-items: center; justify-content: center;
    font-weight: 600; font-size: 14px;
    flex-shrink: 0;
  }
  .stage-num.done   { background: #e8f2ec; color: var(--accent2); }
  .stage-num.todo   { background: #f0ece6; color: var(--muted); }
  .stage-num.finale { background: #fef7e6; color: var(--gold); }

  .stage-info {}
  .stage-name {
    font-weight: 500;
    font-size: 14.5px;
    margin-bottom: 3px;
  }
  .stage-meta {
    font-size: 12px;
    color: var(--muted);
    display: flex;
    gap: 10px;
    flex-wrap: wrap;
  }
  .stage-tag {
    padding: 2px 8px;
    border-radius: 100px;
    background: #ede8e1;
    font-size: 11px;
    font-weight: 500;
    color: var(--muted);
  }
  .stage-check {
    font-size: 18px;
    flex-shrink: 0;
  }

  /* ── SIDEBAR ─────────────────────────────── */
  .meta-list { display: flex; flex-direction: column; gap: 14px; }
  .meta-row {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: 12px;
    font-size: 13.5px;
    padding-bottom: 12px;
    border-bottom: 1px solid var(--border);
  }
  .meta-row:last-child { border-bottom: none; padding-bottom: 0; }
  .meta-key { color: var(--muted); font-weight: 400; white-space: nowrap; }
  .meta-val { color: var(--text); font-weight: 500; text-align: right; word-break: break-all; }

  .link-btn {
    display: block;
    text-align: center;
    padding: 11px 16px;
    border-radius: 8px;
    font-size: 13px;
    font-weight: 500;
    text-decoration: none;
    margin-top: 8px;
    transition: opacity .15s;
  }
  .link-btn:hover { opacity: .82; }
  .link-btn-primary { background: var(--accent); color: #fff; }
  .link-btn-secondary { background: #f0ece6; color: var(--text); border: 1px solid var(--border); }

  /* ── THEMES ──────────────────────────────── */
  .themes { display: flex; flex-wrap: wrap; gap: 7px; }
  .theme-pill {
    padding: 5px 14px;
    border-radius: 100px;
    background: #ede8e0;
    font-size: 12.5px;
    color: #4a433c;
    font-weight: 500;
    letter-spacing: .02em;
  }

  /* ── COMPLETION BAR ──────────────────────── */
  .progress-wrap { margin-top: 6px; }
  .progress-label {
    display: flex; justify-content: space-between;
    font-size: 12px; color: var(--muted); margin-bottom: 6px;
  }
  .progress-bar {
    height: 7px;
    background: #e8e2d9;
    border-radius: 100px;
    overflow: hidden;
  }
  .progress-fill {
    height: 100%;
    background: linear-gradient(90deg, var(--accent2), #6aab82);
    border-radius: 100px;
    transition: width .4s ease;
  }
</style>
</head>
<body>
""");

            // ── HERO ─────────────────────────────────────────────────────────
            sb.AppendLine("""<div class="hero">""");
            if (!string.IsNullOrWhiteSpace(adventure.KeyImageUrl))
                sb.AppendLine($"""  <img src="{Escape(adventure.KeyImageUrl)}" alt="{Escape(adventure.Title)}" />""");
            else
                sb.AppendLine("""  <div class="hero-placeholder">🗺️</div>""");
            sb.AppendLine("""  <div class="hero-overlay"></div>""");
            sb.AppendLine("""  <div class="hero-content">""");
            sb.AppendLine("""    <div class="hero-badges">""");
            if (!string.IsNullOrEmpty(adventureType))
                sb.AppendLine($"""      <span class="badge badge-type">{Escape(adventureType)}</span>""");
            if (!string.IsNullOrEmpty(visibility))
                sb.AppendLine($"""      <span class="badge badge-vis">{Escape(visibility)}</span>""");
            if (adventure.IsArchived)
                sb.AppendLine("""      <span class="badge badge-arch">Archivováno</span>""");
            if (adventure.IsHighlyRecommended)
                sb.AppendLine("""      <span class="badge badge-recommended">⭐ Vysoce doporučeno</span>""");
            sb.AppendLine("""    </div>""");
            sb.AppendLine($"""    <h1 class="hero-title">{Escape(adventure.Title ?? "Bez názvu")}</h1>""");
            if (!string.IsNullOrEmpty(adventure.OwnerUsername))
                sb.AppendLine($"""    <p class="hero-owner">Vytvořil <strong>{Escape(adventure.OwnerUsername)}</strong> · {created}</p>""");
            sb.AppendLine("""  </div>""");
            sb.AppendLine("""</div>""");

            sb.AppendLine("""<div class="container">""");

            // ── STATS ROW ─────────────────────────────────────────────────────
            sb.AppendLine("""<div class="stats-row">""");
            sb.AppendLine($"""
  <div class="stat-item">
    <span class="stat-value"><span class="star">★</span> {rating}</span>
    <span class="stat-label">Hodnocení ({adventure.RatingsTotalCount})</span>
  </div>
  <div class="stat-item">
    <span class="stat-value">{adventure.CompletionCount}</span>
    <span class="stat-label">Dokončení</span>
  </div>
  <div class="stat-item">
    <span class="stat-value">{adventure.RecommendedCount}</span>
    <span class="stat-label">Doporučení</span>
  </div>
  <div class="stat-item">
    <span class="stat-value">{median}</span>
    <span class="stat-label">Střední čas</span>
  </div>
  <div class="stat-item">
    <span class="stat-value">{adventure.JournalsTotalCount}</span>
    <span class="stat-label">Deníků</span>
  </div>
""");
            sb.AppendLine("""</div>""");

            // ── MAIN GRID ─────────────────────────────────────────────────────
            sb.AppendLine("""<div class="main-grid">""");
            sb.AppendLine("""<div class="left-col">""");

            // Description
            if (!string.IsNullOrWhiteSpace(adventure.Description))
            {
                sb.AppendLine($$"""
<div class="card">
  <h2 class="card-title">O dobrodružství</h2>
  <p class="description">{{Escape(adventure.Description)}}</p>
</div>
""");
            }

            // Themes
            if (adventure.AdventureThemes is { Count: > 0 })
            {
                sb.AppendLine("""<div class="card">""");
                sb.AppendLine("""  <h2 class="card-title">Témata</h2>""");
                sb.AppendLine("""  <div class="themes">""");
                foreach (var t in adventure.AdventureThemes)
                    sb.AppendLine($"""    <span class="theme-pill">{Escape(t)}</span>""");
                sb.AppendLine("""  </div>""");
                sb.AppendLine("""</div>""");
            }

            // Stages
            if (adventure.StageSummaries is { Count: > 0 })
            {
                int total = adventure.StageSummaries.Count;
                int completed = adventure.CompletedStagesCount;
                int pct = total > 0 ? (int)Math.Round(completed * 100.0 / total) : 0;

                sb.AppendLine("""<div class="card">""");
                sb.AppendLine("""  <h2 class="card-title">Etapy</h2>""");

                // progress bar
                sb.AppendLine($"""
  <div class="progress-wrap">
    <div class="progress-label">
      <span>Pokrok</span>
      <span>{completed} / {total}</span>
    </div>
    <div class="progress-bar">
      <div class="progress-fill" style="width:{pct}%"></div>
    </div>
  </div>
  <div style="height:20px"></div>
""");

                sb.AppendLine("""  <div class="stages-list">""");
                for (int i = 0; i < adventure.StageSummaries.Count; i++)
                {
                    var s = adventure.StageSummaries[i];
                    string itemClass = s.IsFinal.GetValueOrDefault() ? "stage-item final" : s.IsComplete.GetValueOrDefault() ? "stage-item complete" : "stage-item";
                    string numClass = s.IsFinal.GetValueOrDefault() ? "stage-num finale" : s.IsComplete.GetValueOrDefault() ? "stage-num done" : "stage-num todo";
                    string numText = s.IsFinal.GetValueOrDefault() ? "🏁" : (i + 1).ToString();
                    string checkIcon = s.IsComplete.GetValueOrDefault() ? "✅" : (s.IsFinal.GetValueOrDefault() ? "🏆" : "○");

                    sb.AppendLine($"""    <div class="{itemClass}">""");
                    sb.AppendLine($"""      <div class="{numClass}">{numText}</div>""");
                    sb.AppendLine("""      <div class="stage-info">""");
                    sb.AppendLine($"""        <div class="stage-name">{Escape(s.Title ?? $"Etapa {i + 1}")}</div>""");
                    sb.AppendLine("""        <div class="stage-meta">""");
                    if (!string.IsNullOrEmpty(s.ChallengeType))
                        sb.AppendLine($"""          <span class="stage-tag">{Escape(Capitalize(s.ChallengeType))}</span>""");
                    if (s.GeofencingRadius > 0)
                        sb.AppendLine($"""          <span>📍 {s.GeofencingRadius} m</span>""");
                    if (!string.IsNullOrEmpty(s.Description))
                        sb.AppendLine($"""          <span title="{Escape(s.Description)}">{Truncate(s.Description, 60)}</span>""");
                    sb.AppendLine("""        </div>""");
                    sb.AppendLine("""      </div>""");
                    sb.AppendLine($"""      <span class="stage-check">{checkIcon}</span>""");
                    sb.AppendLine("""    </div>""");
                }
                sb.AppendLine("""  </div>""");
                sb.AppendLine("""</div>""");
            }

            sb.AppendLine("""</div>"""); // left-col

            // ── SIDEBAR ───────────────────────────────────────────────────────
            sb.AppendLine("""<div class="right-col">""");
            sb.AppendLine("""<div class="card">""");
            sb.AppendLine("""  <h2 class="card-title">Informace</h2>""");
            sb.AppendLine("""  <div class="meta-list">""");

            void MetaRow(string key, string? val)
            {
                if (string.IsNullOrEmpty(val)) return;
                sb.AppendLine($"""    <div class="meta-row"><span class="meta-key">{key}</span><span class="meta-val">{Escape(val)}</span></div>""");
            }

            MetaRow("Typ", adventureType);
            MetaRow("Viditelnost", visibility);
            MetaRow("Stav", Capitalize(adventure.CompletionStatus));
            MetaRow("Vytvořeno", created);
            MetaRow("Publikováno", published);
            MetaRow("Etapy", $"{adventure.CompletedStagesCount} / {adventure.StageSummaries?.Count ?? 0}");
            MetaRow("Témata", themes);
            if (adventure.IsTest)
                MetaRow("Testovací", "Ano");

            sb.AppendLine("""  </div>""");

            // Links
            if (!string.IsNullOrEmpty(adventure.SmartLink))
                sb.AppendLine($"""  <a class="link-btn link-btn-primary" href="{Escape(adventure.SmartLink)}" target="_blank">🔗 Otevřít SmartLink</a>""");
            if (!string.IsNullOrEmpty(adventure.DeepLink))
                sb.AppendLine($"""  <a class="link-btn link-btn-secondary" href="{Escape(adventure.DeepLink)}" target="_blank">📱 Deep Link</a>""");
            if (!string.IsNullOrEmpty(adventure.CustomAccessCode))
            {
                sb.AppendLine($"""
  <div style="margin-top:16px;padding:12px 16px;background:#f0ece6;border-radius:8px;text-align:center">
    <div style="font-size:11px;color:var(--muted);text-transform:uppercase;letter-spacing:.05em;margin-bottom:4px">Přístupový kód</div>
    <div style="font-family:'Playfair Display',serif;font-size:22px;font-weight:700;letter-spacing:.1em;color:var(--accent)">{Escape(adventure.CustomAccessCode)}</div>
  </div>
""");
            }

            sb.AppendLine("""</div>"""); // card
            sb.AppendLine("""</div>"""); // right-col
            sb.AppendLine("""</div>"""); // main-grid
            sb.AppendLine("""</div>"""); // container

            sb.AppendLine("""</body>""");
            sb.AppendLine(""" </ html > """);



            return sb.ToString();
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static string Escape(string? s) =>
            string.IsNullOrEmpty(s) ? "" : s
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");

        private static string Truncate(string? s, int max) =>
            string.IsNullOrEmpty(s) ? "" : s.Length <= max ? s : s[..max] + "…";

        private static string? Capitalize(string? s) =>
            string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];

        private static string FormatDuration(int minutes) => minutes switch
        {
            <= 0 => "—",
            < 60 => $"{minutes} min",
            _ => $"{minutes / 60}h {minutes % 60:D2}m"
        };
    }
}