using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace sergiye.Common {

  public abstract class Theme {

    private static Theme current = new LightTheme();
    public static Theme Current {
      get => current;
      set {
        current = value;
        foreach (Form form in Application.OpenForms) {
          current.Apply(form);
        }
        OnCurrentChecnged?.Invoke();
      }
    }

    public static Action OnCurrentChecnged;
    public static Func<Control, Theme, bool> OnApplyToControl;

    public static bool SupportsAutoThemeSwitching() {
      if (OperatingSystemHelper.IsUnix) {
        return false;
      }

      if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", -1) is int useLightTheme) {
        return useLightTheme != -1;
      }
      return false;
    }

    public static void SetAutoTheme() {
      if (OperatingSystemHelper.IsUnix) {
        return;
      }

      if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1) is int useLightTheme) {
        if (useLightTheme > 0) {
          Current = new LightTheme();
        }
        else {
          Current = new DarkTheme();
        }
      }
      else {
        // Fallback incase registry fails
        Current = new LightTheme();
      }
    }

    protected Theme(string id, string displayName) {
      Id = id;
      DisplayName = displayName;
    }

    public string Id { get; }
    public string DisplayName { get; }
    public virtual Color BackgroundColor { get; protected set; }
    public virtual Color ForegroundColor { get; protected set; }
    public virtual Color HyperlinkColor { get; protected set; }
    public virtual Color LineColor { get; protected set; }
    public virtual Color StrongLineColor { get; protected set; }
    public virtual Color SelectedBackgroundColor { get; protected set; }
    public virtual Color SelectedForegroundColor { get; protected set; }

    // statuses
    public virtual Color StatusOkColor { get; protected set; }
    public virtual Color StatusInfoColor { get; protected set; }
    public virtual Color StatusErrorColor { get; protected set; }

    // button
    public virtual Color ButtonBackgroundColor => BackgroundColor;
    public virtual Color ButtonBorderColor => ForegroundColor;
    public virtual Color ButtonHoverBackgroundColor => SelectedBackgroundColor;
    public virtual Color ButtonPressedBackgroundColor => LineColor;
    public virtual Color ButtonTextColor => ForegroundColor;

    // menu
    public virtual Color MenuBackgroundColor => BackgroundColor;
    public virtual Color MenuBorderColor => StrongLineColor;
    public virtual Color MenuForegroundColor => ForegroundColor;
    public virtual Color MenuSelectedBackgroundColor => SelectedBackgroundColor;
    public virtual Color MenuSelectedForegroundColor => SelectedForegroundColor;

    // scrollbar
    public virtual Color ScrollbarBackground => BackgroundColor;
    public virtual Color ScrollbarTrack => StrongLineColor;

    // splitter
    public virtual Color SplitterColor => BackgroundColor;
    public virtual Color SplitterHoverColor => SelectedBackgroundColor;

    // tree
    public virtual Color TreeBackgroundColor => BackgroundColor;
    public virtual Color TreeOutlineColor => ForegroundColor;
    public virtual Color TreeSelectedBackgroundColor => SelectedBackgroundColor;
    public virtual Color TreeTextColor => ForegroundColor;
    public virtual Color TreeSelectedTextColor => SelectedForegroundColor;
    public virtual Color TreeRowSepearatorColor => LineColor;

    // window
    public virtual Color WindowTitlebarBackgroundColor => BackgroundColor;
    public virtual bool WindowTitlebarFallbackToImmersiveDarkMode { get; protected set; }
    public virtual Color WindowTitlebarForegroundColor => ForegroundColor;

    public void Apply(Form form) {
      if (IsWindows10OrGreater(22000)) {
        // Windows 11, Set the titlebar color based on theme
        var color = ColorTranslator.ToWin32(WindowTitlebarBackgroundColor);
        DwmSetWindowAttribute(form.Handle, DWMWA_CAPTION_COLOR, ref color, sizeof(int));
        color = ColorTranslator.ToWin32(WindowTitlebarForegroundColor);
        DwmSetWindowAttribute(form.Handle, DWMWA_TEXT_COLOR, ref color, sizeof(int));
      }
      else if (IsWindows10OrGreater(17763)) {
        // Windows 10, fallback to using "Immersive Dark Mode" instead
        var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
        if (IsWindows10OrGreater(18985)) {
          // Windows 10 20H1 or later
          attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
        }
        var useImmersiveDarkMode = WindowTitlebarFallbackToImmersiveDarkMode ? 1 : 0;
        DwmSetWindowAttribute(form.Handle, attribute, ref useImmersiveDarkMode, sizeof(int));
      }
      form.BackColor = BackgroundColor;
      foreach (Control control in form.Controls) {
        Apply(control);
      }
    }

    public void Apply(Control control) {
      if (control is Button button) {
        button.ForeColor = ButtonTextColor;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = ButtonBorderColor;
        button.FlatAppearance.MouseOverBackColor = ButtonHoverBackgroundColor;
        button.FlatAppearance.MouseDownBackColor = ButtonPressedBackgroundColor;
      }
      if (control is ComboBox combo) {
        combo.ForeColor = ForegroundColor;
        combo.BackColor = BackgroundColor;
        combo.FlatStyle = FlatStyle.Flat;
        combo.DrawMode = DrawMode.OwnerDrawFixed;
        combo.DrawItem -= ComboBox_DrawItem;
        combo.DrawItem += ComboBox_DrawItem;
      }
      else if (control is LinkLabel linkLabel) {
        linkLabel.LinkColor = HyperlinkColor;
      }
      else if (OnApplyToControl == null || !OnApplyToControl(control, current)) {
        control.BackColor = BackgroundColor;
        control.ForeColor = ForegroundColor;
      }

      foreach (Control child in control.Controls) {
        Apply(child);
      }
    }

    private void ComboBox_DrawItem(object sender, DrawItemEventArgs e) {
      if (e.Index < 0) return;
      if (sender is not ComboBox combo) return;
      var isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
      using (var backgroundBrush = new SolidBrush(isSelected ? SelectedBackgroundColor : combo.BackColor)) {
        e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
      }
      using (var textBrush = new SolidBrush(isSelected ? SelectedForegroundColor : combo.ForeColor)) {
        e.Graphics.DrawString(combo.Items[e.Index].ToString(), e.Font, textBrush, e.Bounds);
      }
      e.DrawFocusRectangle();
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMWA_BORDER_COLOR = 34;
    private const int DWMWA_CAPTION_COLOR = 35;
    private const int DWMWA_TEXT_COLOR = 36;

    private static bool IsWindows10OrGreater(int build = -1) {
      return !OperatingSystemHelper.IsUnix && Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
    }
  }
}
