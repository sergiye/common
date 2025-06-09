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

      //SystemEvents.PaletteChanged += (s, e) => {
      //};

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
        button.ForeColor = ForegroundColor;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = ForegroundColor;
        button.FlatAppearance.MouseOverBackColor = SelectedBackgroundColor;
        button.FlatAppearance.MouseDownBackColor = LineColor;
        button.MouseEnter -= Button_MouseEnter;
        button.MouseLeave -= Button_MouseLeave;
        button.MouseEnter += Button_MouseEnter;
        button.MouseLeave += Button_MouseLeave;
        //button.MouseDown += (s, e) => {
        //  button.ForeColor = pressedForeColor;
        //};
        //button.MouseUp += (s, e) => {
        //  if (button.ClientRectangle.Contains(button.PointToClient(Cursor.Position)))
        //    button.ForeColor = hoverForeColor;
        //  else
        //    button.ForeColor = defaultForeColor;
        //};
      }
      if (control is ComboBox combo) {
        combo.ForeColor = ForegroundColor;
        combo.BackColor = BackgroundColor;
        combo.FlatStyle = FlatStyle.Flat;
        combo.DrawMode = DrawMode.OwnerDrawFixed;
        combo.DrawItem -= ComboBox_DrawItem;
        combo.DrawItem += ComboBox_DrawItem;
      }
      if (control is CheckBox checkBox) {
        checkBox.ForeColor = ForegroundColor;
        checkBox.BackColor = BackgroundColor;
        //checkBox.FlatStyle = FlatStyle.Flat;
        //checkBox.Paint -= CheckBox_DrawItem;
        //checkBox.Paint += CheckBox_DrawItem;
      }
      else if (control is LinkLabel linkLabel) {
        linkLabel.LinkColor = HyperlinkColor;
      }
      else if (control is ListBox listBox) {
        listBox.ForeColor = ForegroundColor;
        listBox.BackColor = BackgroundColor;
      }
      else if (control is ListView listView) {
        control.BackColor = BackgroundColor;
        control.ForeColor = ForegroundColor;
        listView.OwnerDraw = true;
        listView.DrawColumnHeader += (sender, e) => {
          using (var backBrush = new SolidBrush(BackgroundColor))
            e.Graphics.FillRectangle(backBrush, e.Bounds);
          using (var foreBrush = new SolidBrush(ForegroundColor))
            e.Graphics.DrawString(e.Header.Text, e.Font, foreBrush, e.Bounds);
        };
        listView.DrawSubItem += (sender, e) => {
          e.DrawDefault = true;
        };
      }
      else if (control is TabControl tabControl) {
        control.BackColor = BackgroundColor;
        control.ForeColor = ForegroundColor;
        tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
        tabControl.DrawItem += (sender, e) => {
          var page = tabControl.TabPages[e.Index];
          e.Graphics.FillRectangle(new SolidBrush(page.BackColor), e.Bounds);
          var paddedBounds = e.Bounds;
          var yOffset = (e.State == DrawItemState.Selected) ? -2 : 1;
          paddedBounds.Offset(1, yOffset);
          TextRenderer.DrawText(e.Graphics, page.Text, e.Font, paddedBounds, page.ForeColor);
        };
      }
      else if (control is TabPage tabPage) {
        tabPage.UseVisualStyleBackColor = true;
        tabPage.BorderStyle = BorderStyle.None;
        control.BackColor = BackgroundColor;
        control.ForeColor = ForegroundColor;
      }
      else if (OnApplyToControl == null || !OnApplyToControl(control, current)) {
        control.BackColor = BackgroundColor;
        control.ForeColor = ForegroundColor;
      }

      if (WindowTitlebarFallbackToImmersiveDarkMode) {
        SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
      }
      else {
        SetWindowTheme(control.Handle, "Explorer", null);
      }

      foreach (Control child in control.Controls) {
        Apply(child);
      }
    }

    private void Button_MouseEnter(object sender, EventArgs e) {
      if (sender is Button button)
        button.ForeColor = SelectedForegroundColor;
    }

    private void Button_MouseLeave(object sender, EventArgs e) {
      if (sender is Button button)
        button.ForeColor = ForegroundColor;
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

    private void CheckBox_DrawItem(object sender, PaintEventArgs e) {
      var checkBox = sender as CheckBox;
      if (checkBox == null) return;

      e.Graphics.Clear(checkBox.BackColor);
      var boxRect = new Rectangle(0, (checkBox.Height - 16) / 2, 16, 16);
      var textRect = new Rectangle(20, 0, checkBox.Width - 20, checkBox.Height);

      ControlPaint.DrawCheckBox(e.Graphics, boxRect, checkBox.Checked ? ButtonState.Checked : ButtonState.Normal);

      if (checkBox.Checked) {
        using (var brush = new SolidBrush(SelectedBackgroundColor)) {
          e.Graphics.FillRectangle(brush, new Rectangle(boxRect.X + 2, boxRect.Y + 2, boxRect.Width - 4, boxRect.Height - 4));
        }

        //using (var pen = new Pen(SelectedBackgroundColor, 2)) {
        //  e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        //  e.Graphics.DrawLines(pen, new[] {
        //    new Point(boxRect.Left + 3, boxRect.Top + boxRect.Height / 2),
        //    new Point(boxRect.Left + boxRect.Width / 2 - 1, boxRect.Bottom - 4),
        //    new Point(boxRect.Right - 3, boxRect.Top + 4)
        //  });
        //}
      }

      TextRenderer.DrawText(e.Graphics, checkBox.Text, checkBox.Font, textRect, checkBox.ForeColor,
          TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    [DllImport("uxtheme.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

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
