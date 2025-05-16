using System;
using System.Drawing;
using System.Windows.Forms;

namespace sergiye.Common {

  public class ThemedToolStripRenderer : ToolStripRenderer {

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e) {
      if (e.Item is not ToolStripSeparator) {
        base.OnRenderSeparator(e);
        return;
      }

      Rectangle bounds = new(Point.Empty, e.Item.Size);
      using (Brush brush = new SolidBrush(Theme.Current.BackgroundColor))
        e.Graphics.FillRectangle(brush, bounds);
    }

    protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e) {
      e.ArrowColor = e.Item.Selected ? Theme.Current.SelectedForegroundColor : Theme.Current.ForegroundColor;
      base.OnRenderArrow(e);
    }

    protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e) {
      using (var pen = new Pen(e.Item.Selected ? Theme.Current.SelectedForegroundColor : Theme.Current.ForegroundColor, (float)1.7)) {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var offset = Math.Min(e.ImageRectangle.Height, e.ImageRectangle.Width) / 3;
        e.Graphics.DrawLines(pen, new[] { 
          new Point(e.ImageRectangle.Left + offset, e.ImageRectangle.Top + e.ImageRectangle.Height / 2),
          new Point(e.ImageRectangle.Left + e.ImageRectangle.Width / 2 - 1, e.ImageRectangle.Bottom - offset),
          new Point(e.ImageRectangle.Right - offset, e.ImageRectangle.Top + offset)
        });
      }
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e) {
      e.TextColor = e.Item.Selected ? Theme.Current.SelectedForegroundColor : Theme.Current.ForegroundColor;
      base.OnRenderItemText(e);
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) {
      if (e.ToolStrip.Parent is not Form) {
        Rectangle bounds = new(Point.Empty, new Size(e.ToolStrip.Width - 1, e.ToolStrip.Height - 1));
        using (var pen = new Pen(Theme.Current.StrongLineColor))
          e.Graphics.DrawRectangle(pen, bounds);
      }
    }

    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e) {
      Rectangle bounds = new(Point.Empty, e.ToolStrip.Size);
      using (Brush brush = new SolidBrush(Theme.Current.BackgroundColor))
        e.Graphics.FillRectangle(brush, bounds);
    }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e) {
      Rectangle bounds = new(Point.Empty, e.Item.Size);

      using (Brush brush = new SolidBrush(e.Item.Selected ? Theme.Current.SelectedBackgroundColor : Theme.Current.BackgroundColor))
        e.Graphics.FillRectangle(brush, bounds);
    }
  }
}
