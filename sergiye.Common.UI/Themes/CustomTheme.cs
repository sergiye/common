using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace sergiye.Common {

  public class CustomTheme : Theme {
    private class ThemeDto {
      public string DisplayName { get; set; }
      public string ForegroundColor { get; set; }
      public string BackgroundColor { get; set; }
      public string HyperlinkColor { get; set; }
      public string SelectedForegroundColor { get; set; }
      public string SelectedBackgroundColor { get; set; }
      public string LineColor { get; set; }
      public string StrongLineColor { get; set; }
      public bool DarkMode { get; set; }
      public string MessageColor { get; set; }
      public string InfoColor { get; set; }
      public string WarnColor { get; set; }
    }

    private CustomTheme(string id, ThemeDto theme) : base(id, theme.DisplayName) {
      ForegroundColor = ColorTranslator.FromHtml(theme.ForegroundColor);
      BackgroundColor = ColorTranslator.FromHtml(theme.BackgroundColor);
      HyperlinkColor = ColorTranslator.FromHtml(theme.HyperlinkColor);
      SelectedForegroundColor = ColorTranslator.FromHtml(theme.SelectedForegroundColor);
      SelectedBackgroundColor = ColorTranslator.FromHtml(theme.SelectedBackgroundColor);
      LineColor = ColorTranslator.FromHtml(theme.LineColor);
      StrongLineColor = ColorTranslator.FromHtml(theme.StrongLineColor);
      WindowTitlebarFallbackToImmersiveDarkMode = theme.DarkMode;
      MessageColor = theme.MessageColor != null
        ? ColorTranslator.FromHtml(theme.MessageColor)
        : theme.DarkMode ? DarkTheme.DefaultMessageColor : LightTheme.DefaultMessageColor;
      InfoColor = theme.InfoColor != null
        ? ColorTranslator.FromHtml(theme.InfoColor)
        : theme.DarkMode ? DarkTheme.DefaultInfoColor : LightTheme.DefaultInfoColor;
      WarnColor = theme.WarnColor != null
        ? ColorTranslator.FromHtml(theme.WarnColor)
        : theme.DarkMode ? DarkTheme.DefaultWarnColor : LightTheme.DefaultWarnColor;
    }

    public static IEnumerable<Theme> GetAllThemes(string themesFolder = "themes", string resourcesPath = null) {
      var assembly = typeof(Theme).Assembly;
      foreach (var type in assembly.GetTypes()) {
        if (type == typeof(Theme) || !typeof(Theme).IsAssignableFrom(type))
          continue;
        var theme = (Theme)type.GetConstructor(new Type[] { })?.Invoke(new object[] { });
        if (theme != null)
          yield return theme;
      }

      if (!string.IsNullOrEmpty(resourcesPath))
      foreach (string path in assembly.GetManifestResourceNames().Where(n => n.StartsWith(resourcesPath) && n.EndsWith(".json"))) {
        using (Stream stream = assembly.GetManifestResourceStream(path)) {
          using (StreamReader reader = new(stream)) {
            ThemeDto dto;
            try {
              dto = reader.ReadToEnd().FromJson<ThemeDto>();
            }
            catch (Exception) {
              continue;
            }
            yield return new CustomTheme(dto.DisplayName, dto);
          }
        }
      }

      var appPath = Path.GetDirectoryName(Updater.CurrentFileLocation);
      var themesPath = Path.Combine(appPath, themesFolder);
      var di = new DirectoryInfo(themesPath);
      //var dt = CustomTheme.ToDto(new DarkTheme());
      //Directory.CreateDirectory(themesPath);
      //dt.ToJsonFile(Path.Combine(themesPath, "custom.json"));
      if (!di.Exists) {
        yield break;
      }
      foreach (var fi in di.GetFiles("*.json", SearchOption.TopDirectoryOnly)) {
        ThemeDto dto = null;
        try {
          var json = File.ReadAllText(fi.FullName);
          dto = json.FromJson<ThemeDto>();
          if (string.IsNullOrEmpty(dto.DisplayName))
            dto.DisplayName = Path.GetFileNameWithoutExtension(fi.Name);
        }
        catch (Exception) {
          //ignore
        }

        if (dto != null)
          yield return new CustomTheme(fi.Name, dto);
      }
    }

    //private static ThemeDto ToDto(Theme theme) {
    //  return new ThemeDto {
    //    DisplayName = string.IsNullOrEmpty(theme.DisplayName) ? theme.Id : theme.DisplayName,
    //    ForegroundColor = ColorTranslator.ToHtml(theme.ForegroundColor),
    //    BackgroundColor = ColorTranslator.ToHtml(theme.BackgroundColor),
    //    HyperlinkColor = ColorTranslator.ToHtml(theme.HyperlinkColor),
    //    SelectedForegroundColor = ColorTranslator.ToHtml(theme.SelectedForegroundColor),
    //    SelectedBackgroundColor = ColorTranslator.ToHtml(theme.SelectedBackgroundColor),
    //    LineColor = ColorTranslator.ToHtml(theme.LineColor),
    //    StrongLineColor = ColorTranslator.ToHtml(theme.StrongLineColor),
    //    DarkMode = theme.WindowTitlebarFallbackToImmersiveDarkMode,
    //  };
    //}
  }
}
