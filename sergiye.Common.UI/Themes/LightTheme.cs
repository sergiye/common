using System.Drawing;

namespace sergiye.Common {
  public class LightTheme : Theme {

    public static readonly Color DefaultMessageColor = Color.ForestGreen;
    public static readonly Color DefaultInfoColor = Color.Gray;
    public static readonly Color DefaultWarnColor = Color.Red;

    public LightTheme() : base("light", "Light") {
      ForegroundColor = Color.Black;
      BackgroundColor = Color.White;
      HyperlinkColor = Color.FromArgb(0, 0, 255);
      SelectedForegroundColor = ForegroundColor;
      SelectedBackgroundColor = Color.CornflowerBlue;
      LineColor = Color.FromArgb(247, 247, 247);
      StrongLineColor = Color.FromArgb(209, 209, 209);
      WindowTitlebarFallbackToImmersiveDarkMode = false;
      MessageColor = DefaultMessageColor;
      InfoColor = DefaultInfoColor;
      WarnColor = DefaultWarnColor;
    }
  }
}
