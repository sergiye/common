using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace sergiye.Common {

  public abstract class Theme {

    public const string SkipThemeTag = "sergiye.Common.Theme.SkipThemeTag";

    private static Theme current;
    public static Theme Current {
      get {
        if (current == null) {
          current = new LightTheme();
          CurrentColorsChanged();
        }
        return current;
      }

      set {
        current = value;
        foreach (Form form in Application.OpenForms) {
          current.Apply(form);
        }
        CurrentColorsChanged();
        SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
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

      if (AppsUseLightTheme) {
        if (Current is not LightTheme)
          Current = new LightTheme();
      }
      else {
        if (Current is not DarkTheme)
          Current = new DarkTheme();
      }

      if (SupportsAutoThemeSwitching()) {
        SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
      }
    }

    private static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e) {
      
      if (AppsUseLightTheme) {
        if (Current is LightTheme)
          return;
      }
      else {
        if (Current is DarkTheme)
          return;
      }
      SetAutoTheme();
    }

    public static bool AppsUseLightTheme =>
      Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1) is int useLightTheme && useLightTheme > 0;

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
    public virtual Color MessageColor { get; protected set; }
    public virtual Color InfoColor { get; protected set; }
    public virtual Color WarnColor { get; protected set; }

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

        //GraphicsPath p = new GraphicsPath();
        //p.AddEllipse(-3, -3, button.Width + 2, button.Height + 2);
        //button.Region = new Region(p);
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
        listView.DrawColumnHeader -= ListView_DrawColumnHeader;
        listView.DrawColumnHeader += ListView_DrawColumnHeader;
        listView.DrawSubItem -= ListView_DrawSubItem;
        listView.DrawSubItem += ListView_DrawSubItem;
      }
      else if (control is TabControl tabControl) {
        control.BackColor = BackgroundColor;
        control.ForeColor = ForegroundColor;
        if (control.Tag == SkipThemeTag) {

        }
        else {
          tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
          tabControl.DrawItem -= TabControl_DrawItem;
          tabControl.DrawItem += TabControl_DrawItem;
        }
      }
      else if (control is TabPage tabPage) {
        tabPage.UseVisualStyleBackColor = true;
        tabPage.BorderStyle = BorderStyle.None;
        control.BackColor = BackgroundColor;
        control.ForeColor = ForegroundColor;
      }
      else if (control is GroupBox groupBox) {
        //groupBox.UseCompatibleTextRendering = true;
        //groupBox.FlatStyle = FlatStyle.Flat;
        groupBox.BackColor = BackgroundColor;
        groupBox.ForeColor = ForegroundColor;
      }
      else if (control is PropertyGrid propertyGrid) {
        propertyGrid.BackColor = BackgroundColor;
        propertyGrid.ForeColor = ForegroundColor;
        propertyGrid.CategoryForeColor = ForegroundColor;
        propertyGrid.CategorySplitterColor = LineColor;
        propertyGrid.CommandsActiveLinkColor = HyperlinkColor;
        propertyGrid.CommandsDisabledLinkColor = InfoColor;
        propertyGrid.CommandsLinkColor = HyperlinkColor;
        propertyGrid.CommandsBackColor = BackgroundColor;
        propertyGrid.CommandsBorderColor = LineColor;
        propertyGrid.CommandsForeColor = ForegroundColor;
        propertyGrid.DisabledItemForeColor = InfoColor;
        propertyGrid.LineColor = LineColor;
        propertyGrid.SelectedItemWithFocusBackColor = SelectedBackgroundColor;
        propertyGrid.SelectedItemWithFocusForeColor = SelectedForegroundColor;
        propertyGrid.ViewBackColor = BackgroundColor;
        propertyGrid.ViewBorderColor = StrongLineColor;
        propertyGrid.ViewForeColor = ForegroundColor;
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

    private static void CurrentColorsChanged() {

      backBrush = new SolidBrush(current.BackgroundColor);
      foreBrush = new SolidBrush(current.ForegroundColor);
      selectedBackBrush = new SolidBrush(current.SelectedBackgroundColor);
      selectedForeBrush = new SolidBrush(current.SelectedForegroundColor);
      selectedForePen = new Pen(current.SelectedForegroundColor, 2);

      OnCurrentChecnged?.Invoke();
    }

    #region Draw resources

    private static Brush backBrush;
    private static Brush foreBrush;
    private static Brush selectedBackBrush;
    private static Brush selectedForeBrush;
    private static Pen selectedForePen;

    #endregion

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
      e.Graphics.FillRectangle(isSelected ? selectedBackBrush : backBrush, e.Bounds);
      e.Graphics.DrawString(combo.Items[e.Index].ToString(), e.Font, isSelected ? selectedForeBrush : foreBrush, e.Bounds);
      e.DrawFocusRectangle();
    }

    private void CheckBox_DrawItem(object sender, PaintEventArgs e) {
      if (sender is not CheckBox checkBox) return;

      e.Graphics.Clear(checkBox.BackColor);
      var boxRect = new Rectangle(0, (checkBox.Height - 16) / 2, 16, 16);
      var textRect = new Rectangle(20, 0, checkBox.Width - 20, checkBox.Height);

      ControlPaint.DrawCheckBox(e.Graphics, boxRect, checkBox.Checked ? ButtonState.Checked : ButtonState.Normal);

      if (checkBox.Checked) {
        e.Graphics.FillRectangle(selectedBackBrush, new Rectangle(boxRect.X + 2, boxRect.Y + 2, boxRect.Width - 4, boxRect.Height - 4));
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        e.Graphics.DrawLines(selectedForePen, new[] {
          new Point(boxRect.Left + 3, boxRect.Top + boxRect.Height / 2),
          new Point(boxRect.Left + boxRect.Width / 2 - 1, boxRect.Bottom - 4),
          new Point(boxRect.Right - 3, boxRect.Top + 4)
        });
      }

      TextRenderer.DrawText(e.Graphics, checkBox.Text, checkBox.Font, textRect, checkBox.ForeColor,
          TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
    }

    private void ListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e) {
      e.Graphics.FillRectangle(backBrush, e.Bounds);
      e.Graphics.DrawString(e.Header.Text, e.Font, foreBrush, e.Bounds);
    }

    private void ListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e) {
      var lView = sender as ListView;
      TextFormatFlags flags = GetTextAlignment(lView, e.ColumnIndex);
      Color itemColor = e.Item.ForeColor;

      if (e.Item.Selected) {
        if (e.ColumnIndex == 0 || lView.FullRowSelect) {
          e.Graphics.FillRectangle(selectedBackBrush, e.Bounds);
          itemColor = SelectedForegroundColor;
        }
      }
      else {
        e.DrawBackground();
      }
      TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, e.Bounds, itemColor, flags);
    }

    private void TabControl_DrawItem(object sender, DrawItemEventArgs e) {
      if (sender is not TabControl tabControl) return;
      var page = tabControl.TabPages[e.Index];
      e.Graphics.FillRectangle(backBrush, e.Bounds);
      var paddedBounds = e.Bounds;
      var yOffset = (e.State == DrawItemState.Selected) ? -2 : 1;
      paddedBounds.Offset(1, yOffset);
      TextRenderer.DrawText(e.Graphics, page.Text, e.Font, paddedBounds, ForegroundColor);
    }

    private TextFormatFlags GetTextAlignment(ListView lstView, int colIndex) {
      TextFormatFlags flags = (lstView.View == View.Tile)
          ? (colIndex == 0) ? TextFormatFlags.Default : TextFormatFlags.Bottom
          : TextFormatFlags.VerticalCenter;

      if (lstView.View == View.Details) flags |= TextFormatFlags.LeftAndRightPadding;

      if (lstView.Columns[colIndex].TextAlign != HorizontalAlignment.Left) {
        flags |= (TextFormatFlags)((int)lstView.Columns[colIndex].TextAlign ^ 3);
      }
      return flags;
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
