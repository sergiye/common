using System.Drawing;

namespace sergiye.Common {

  public class DarkTheme : Theme {

    public static readonly Color DefaultStatusOkColor = Color.LawnGreen;
    public static readonly Color DefaultStatusInfoColor = Color.Gold;
    public static readonly Color DefaultStatusErrorColor = Color.OrangeRed;

    public DarkTheme() : base("dark", "Dark") {

      ForegroundColor = ColorTranslator.FromHtml("#DADADA");
      BackgroundColor = Color.Black;
      HyperlinkColor = ColorTranslator.FromHtml("#90E6E8");
      SelectedForegroundColor = ColorTranslator.FromHtml("#DADADA");
      SelectedBackgroundColor = ColorTranslator.FromHtml("#2B5278");
      LineColor = ColorTranslator.FromHtml("#070A12");
      StrongLineColor = ColorTranslator.FromHtml("#091217");
      WindowTitlebarFallbackToImmersiveDarkMode = true;
      StatusOkColor = DefaultStatusOkColor;
      StatusInfoColor = DefaultStatusInfoColor;
      StatusErrorColor = DefaultStatusErrorColor;
    }
  }
}
