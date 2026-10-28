using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Newtonsoft.Json;

namespace Gml.Launcher.Core.Services;

/// <summary>
/// Система шаблонов (тем) лаунчера для владельца проекта (white-label).
/// Активный шаблон задаётся при сборке файлом Themes/active.json — игрок тему не выбирает.
/// </summary>
public static class ThemeService
{
    public const string DefaultThemeName = "default";
    public const string DefaultLogoPath = "/Assets/Images/logo.svg";
    public const string DefaultBrandName = "GAMERVII";
    public const string ActiveConfigFileName = "active.json";

    /// <summary>Имена слотов видимости, которые Views уже умеют читать.</summary>
    public static readonly string[] VisibilitySlots =
    [
        "News",
        "Online",
        "ServerInfo",
        "SocialLinks",
        "ModsListButton",
        "Logo",
        "ProfileButton",
        "LogoutButton",
        "Sidebar",
        "Footer"
    ];

    public static string ThemesRoot =>
        Path.Combine(AppContext.BaseDirectory, "Themes");

    public static string? CurrentThemeDirectory { get; private set; }

    public static string? CurrentThemeName { get; private set; }

    /// <summary>
    /// Имя темы из Themes/active.json (задаётся владельцем при сборке/упаковке).
    /// Игрок это не меняет.
    /// </summary>
    public static string ResolveActiveThemeName()
    {
        try
        {
            var path = Path.Combine(ThemesRoot, ActiveConfigFileName);
            if (File.Exists(path))
            {
                var cfg = JsonConvert.DeserializeObject<ActiveThemeConfig>(File.ReadAllText(path));
                if (!string.IsNullOrWhiteSpace(cfg?.Theme))
                    return cfg.Theme.Trim();
            }
        }
        catch
        {
            // fallback ниже
        }

        return DefaultThemeName;
    }

    /// <summary>Применить тему, указанную в active.json.</summary>
    public static void ApplyActive() => Apply(ResolveActiveThemeName());

    public static void Apply(string? themeName = null)
    {
        EnsureBundledThemesPresent();

        var name = string.IsNullOrWhiteSpace(themeName)
            ? ResolveActiveThemeName()
            : themeName.Trim();

        var themeDir = Path.Combine(ThemesRoot, name);
        if (!Directory.Exists(themeDir) || !File.Exists(Path.Combine(themeDir, "theme.json")))
        {
            name = DefaultThemeName;
            themeDir = Path.Combine(ThemesRoot, name);
        }

        CurrentThemeName = name;
        CurrentThemeDirectory = themeDir;
        var manifest = TryReadManifest(themeDir) ?? CreateDefaultManifest(name, name);

        void ApplyAll()
        {
            var app = Application.Current;
            if (app?.Resources is null)
                return;

            ApplyColors(app, manifest);
            ApplyVisibility(app, manifest);
            ApplyLayout(app, manifest);
            ApplyAssets(app, themeDir, manifest);
        }

        if (Dispatcher.UIThread.CheckAccess())
            ApplyAll();
        else
            Dispatcher.UIThread.Post(ApplyAll);
    }

    private static void ApplyColors(Application app, ThemeManifest manifest)
    {
        if (manifest.Colors is null || manifest.Colors.Count == 0)
            return;

        foreach (var (key, value) in manifest.Colors)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                continue;

            if (!TryParseColor(value, out var color))
                continue;

            app.Resources[key] = new SolidColorBrush(color);
        }
    }

    private static void ApplyVisibility(Application app, ThemeManifest manifest)
    {
        foreach (var slot in VisibilitySlots)
        {
            var visible = true;
            if (manifest.Visibility != null &&
                manifest.Visibility.TryGetValue(slot, out var flag))
                visible = flag;

            app.Resources[$"ThemeVisible.{slot}"] = visible;
        }
    }

    private static void ApplyLayout(Application app, ThemeManifest manifest)
    {
        var layout = manifest.Layout ?? new ThemeLayout();

        var sidebar = string.Equals(layout.SidebarPosition, "Right", StringComparison.OrdinalIgnoreCase)
            ? "Right"
            : "Left";

        app.Resources["ThemeLayout.SidebarPosition"] = sidebar;
        app.Resources["ThemeLayout.SidebarDock"] = sidebar == "Right" ? Dock.Right : Dock.Left;
        app.Resources["ThemeLayout.BrandName"] =
            string.IsNullOrWhiteSpace(layout.BrandName) ? DefaultBrandName : layout.BrandName.Trim();
    }

    private static void ApplyAssets(Application app, string themeDir, ThemeManifest manifest)
    {
        var logoPath = DefaultLogoPath;
        var candidates = new[]
        {
            manifest.Assets?.Logo,
            "logo.svg",
            "logo.png",
            "Assets/logo.svg",
            "Assets/logo.png"
        };

        foreach (var relative in candidates)
        {
            if (string.IsNullOrWhiteSpace(relative))
                continue;

            var full = Path.IsPathRooted(relative)
                ? relative
                : Path.Combine(themeDir, relative);

            if (File.Exists(full))
            {
                logoPath = full;
                break;
            }
        }

        app.Resources["ThemeLogoPath"] = logoPath;
    }

    /// <summary>
    /// Гарантирует наличие active.json и темы default рядом с exe.
    /// Не создаёт «образцы» вроде ocean у игрока — они только в репозитории (Themes/samples).
    /// </summary>
    public static void EnsureBundledThemesPresent()
    {
        try
        {
            Directory.CreateDirectory(ThemesRoot);

            var activePath = Path.Combine(ThemesRoot, ActiveConfigFileName);
            if (!File.Exists(activePath))
            {
                File.WriteAllText(activePath,
                    JsonConvert.SerializeObject(new ActiveThemeConfig { Theme = DefaultThemeName }, Formatting.Indented));
            }

            var defaultDir = Path.Combine(ThemesRoot, DefaultThemeName);
            Directory.CreateDirectory(defaultDir);
            var themePath = Path.Combine(defaultDir, "theme.json");
            if (!File.Exists(themePath))
            {
                var theme = CreateDefaultManifest(DefaultThemeName, "Gml Default");
                File.WriteAllText(themePath, JsonConvert.SerializeObject(theme, Formatting.Indented));
            }

            var readme = Path.Combine(ThemesRoot, "README.md");
            if (!File.Exists(readme))
            {
                File.WriteAllText(readme,
                    """
                    # Шаблоны лаунчера (для владельца проекта)

                    Игрок тему НЕ выбирает. Какой шаблон использовать — решает `active.json` при сборке.

                    1. Скопируйте папку `default` → `my-brand`
                    2. Отредактируйте `my-brand/theme.json` (+ logo.svg)
                    3. В `active.json` укажите: `{ "theme": "my-brand" }`
                    4. Соберите / опубликуйте лаунчер

                    Документация: `docs/THEMES.md` в репозитории.
                    """);
            }
        }
        catch
        {
            // темы не критичны для запуска
        }
    }

    private static ThemeManifest CreateDefaultManifest(string name, string displayName) => new()
    {
        Name = name,
        DisplayName = displayName,
        Colors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["PrimaryColor"] = "#008C45",
            ["PrimaryColorHover"] = "#01763B",
            ["PrimaryTransparentColor"] = "#101412",
            ["HeadlineColor"] = "#FFFFFF",
            ["ContentColor"] = "#969696",
            ["BackgroundColor"] = "#0E0E0E",
            ["FrameBackgroundColor"] = "#111111",
            ["FrameBackgroundBorderColor"] = "#2E2E2E",
            ["SecondaryColor"] = "#060B0D",
            ["SecondaryColorHover"] = "#131719",
            ["FormBackgroundColor"] = "#151515",
            ["FormBackgroundHoverColor"] = "#1C1C1C",
            ["FormBorderColor"] = "#212121",
            ["FormBorderHoverColor"] = "#2C2C2C",
            ["BadgeBorder"] = "#20FFFFFF"
        },
        Visibility = VisibilitySlots.ToDictionary(s => s, _ => true, StringComparer.OrdinalIgnoreCase),
        Layout = new ThemeLayout
        {
            SidebarPosition = "Left",
            BrandName = DefaultBrandName
        },
        Assets = new ThemeAssets()
    };

    private static ThemeManifest? TryReadManifest(string themeDir)
    {
        try
        {
            var path = Path.Combine(themeDir, "theme.json");
            if (!File.Exists(path))
                return null;
            return JsonConvert.DeserializeObject<ThemeManifest>(File.ReadAllText(path));
        }
        catch
        {
            return null;
        }
    }

    private static bool TryParseColor(string value, out Color color)
    {
        color = default;
        try
        {
            var text = value.Trim();
            if (!text.StartsWith('#'))
                text = "#" + text;

            color = Color.Parse(text);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private sealed class ActiveThemeConfig
    {
        public string? Theme { get; set; }
    }

    private sealed class ThemeManifest
    {
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public Dictionary<string, string>? Colors { get; set; }
        public Dictionary<string, bool>? Visibility { get; set; }
        public ThemeLayout? Layout { get; set; }
        public ThemeAssets? Assets { get; set; }
    }

    private sealed class ThemeLayout
    {
        public string? SidebarPosition { get; set; }
        public string? BrandName { get; set; }
    }

    private sealed class ThemeAssets
    {
        public string? Logo { get; set; }
    }
}
