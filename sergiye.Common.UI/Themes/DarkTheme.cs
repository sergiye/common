using System.Drawing;

namespace sergiye.Common {

  public class DarkTheme : Theme {

    public static readonly Color DefaultMessageColor = Color.LawnGreen;
    public static readonly Color DefaultInfoColor = Color.Gold;
    public static readonly Color DefaultWarnColor = Color.OrangeRed;

    public DarkTheme() : base("dark", "Dark") {

      ForegroundColor = ColorTranslator.FromHtml("#DADADA");
      BackgroundColor = Color.Black;
      HyperlinkColor = ColorTranslator.FromHtml("#90E6E8");
      SelectedForegroundColor = ColorTranslator.FromHtml("#DADADA");
      SelectedBackgroundColor = ColorTranslator.FromHtml("#2B5278");
      LineColor = ColorTranslator.FromHtml("#070A12");
      StrongLineColor = ColorTranslator.FromHtml("#091217");
      WindowTitlebarFallbackToImmersiveDarkMode = true;
      MessageColor = DefaultMessageColor;
      InfoColor = DefaultInfoColor;
      WarnColor = DefaultWarnColor;
    }
  }
}
