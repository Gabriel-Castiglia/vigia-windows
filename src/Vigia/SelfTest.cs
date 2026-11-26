// Vigia -- self-tests used by the build (--render-chrome).
// Copyright (C) 2026 Gabriel Castiglia. Licensed under the GNU GPL, version 2 only.
// Date: November 10, 2026
// SPDX-License-Identifier: GPL-2.0-only

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Vigia;

/// <summary>Vigia.exe --render-chrome &lt;dir&gt;: renders the menu bar, every top-level popup, the dialogs, the Files panel,
/// the layout pictures and the tab strip to PNG (light and dark, English, Spanish and Japanese).</summary>
public static class SelfTest
{
    public static void RenderChrome(MainWindow w, string dir)
    {
        Directory.CreateDirectory(dir);
        foreach (var dark in new[] { false, true })
        {
            ThemeManager.Apply(w, dark);
            w.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
            // everything below in every language: the menus, the toolbar, the dialogs and the panes
            var keepLang = Strings.Lang;
            foreach (var lang in new[] { "en", "es", "ja" })
            {
                Strings.Lang = lang;
                w.RelocalizeForTest();
                w.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
                string sfx = $"{lang}_{(dark ? "dark" : "light")}";
                var menu = (Menu)w.FindName("MainMenu");
                Save(menu, Path.Combine(dir, $"menubar_{sfx}.png"));
                Save((FrameworkElement)w.FindName("ToolBarHost"), Path.Combine(dir, $"toolbar_{sfx}.png"));
                Save((FrameworkElement)w.FindName("Status"), Path.Combine(dir, $"statusbar_{sfx}.png"));
                // narrow window: the buttons that do not fit go into the overflow menu instead of overlapping
                var bar = (FrameworkElement)w.FindName("ToolBarHost");
                double keepW = bar.Width;
                foreach (var narrow in new[] { 760.0, 560.0 })
                {
                    bar.Width = narrow; bar.UpdateLayout(); w.FitToolbarForTest(); bar.UpdateLayout();
                    Save(bar, Path.Combine(dir, $"toolbar_{narrow}_{sfx}.png"));
                }
                bar.Width = keepW; bar.UpdateLayout(); w.FitToolbarForTest();
                int i = 0;
                foreach (MenuItem top in menu.Items)
                {
                    top.IsSubmenuOpen = true;
                    w.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
                    top.ApplyTemplate();
                    if (top.Template.FindName("PART_Popup", top) is Popup p && p.Child is FrameworkElement child)
                    {
                        child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        child.Arrange(new Rect(child.DesiredSize));
                        child.UpdateLayout();
                        Save(child, Path.Combine(dir, $"popup{i}_{sfx}.png"), dark);
                    }
                    top.IsSubmenuOpen = false;
                    i++;
                }
                // the questions and the input dialogs
                var dialogs = new (string Name, Window Dlg)[]
                {
                    ("ask_save", ChoiceDialog.ForTest(w, string.Format(Strings.Get("AskSave"), "Plan.md"), new[] { Strings.Get("DontSave"), Strings.Get("Cancel"), Strings.Get("SaveBtn") }, 1)),
                    ("ask_delete", ChoiceDialog.ForTest(w, string.Format(Strings.Get("DeleteAskFolder"), "Notes"), new[] { Strings.Get("Cancel"), Strings.Plain("Delete") }, 0, Strings.Get("DeleteNoAsk"))),
                    ("ask_changed", ChoiceDialog.ForTest(w, string.Format(Strings.Get("ChangedOnDisk"), "Plan.md"), new[] { Strings.Get("KeepMine"), Strings.Get("ReloadBtn") }, 0)),
                    ("ask_update", ChoiceDialog.ForTest(w, string.Format(Strings.Get("UpdateAsk"), "9.9.9", "3.2.0"), new[] { Strings.Get("UpdateLater"), Strings.Get("UpdateNotes"), Strings.Get("UpdateNow") }, 0)),
                    ("input_link", new InputDialog(w, Strings.Plain("Link"), new[] { (Strings.Get("LinkText"), "Vigía"), (Strings.Get("LinkUrl"), "https://") })),
                    ("input_table", new InputDialog(w, Strings.Plain("Table"), new[] { (Strings.Get("Columns"), "3"), (Strings.Get("Rows"), "3") })),
                };
                foreach (var (name, dlg) in dialogs)
                {
                    var card = (FrameworkElement)dlg.Content;
                    Layout(card);
                    Save(card, Path.Combine(dir, $"{name}_{sfx}.png"), dark);
                    dlg.Close();
                }
                var about = new AboutDialog(w, "1.6.0");
                Layout(about.Card);
                Save(about.Card, Path.Combine(dir, $"about_{sfx}.png"), dark);
                about.Close();
                var theme = new ThemeDialog(w, "auto", Accents.Default, _ => { }, _ => { });
                Layout(theme.Card);
                Save(theme.Card, Path.Combine(dir, $"theme_{sfx}.png"), dark);
                theme.Close();
                var tips = new StackPanel();
                foreach (var (n, t, g) in new[] { ("ModeVisual", "TipVisual", "Ctrl+1"), ("ModeSource", "TipSource", "Ctrl+2"), ("ModePreview", "TipPreview", "Ctrl+3") })
                {
                    // same look as the ToolTip template in Theme.xaml (a real ToolTip cannot be parented)
                    var tt = new Border { CornerRadius = new CornerRadius(7), Padding = new Thickness(9, 5, 9, 5), Margin = new Thickness(6, 4, 6, 6),
                                          BorderThickness = new Thickness(1), Child = (UIElement)MainWindow.SegTip(n, t, g),
                                          HorizontalAlignment = HorizontalAlignment.Left };
                    tt.SetResourceReference(Border.BackgroundProperty, "Gm.Popup");
                    tt.SetResourceReference(Border.BorderBrushProperty, "Gm.PopupBorder");
                    tt.SetResourceReference(TextElement.ForegroundProperty, "Gm.Text");
                    tt.SetResourceReference(TextElement.FontFamilyProperty, "Gm.Font");
                    TextElement.SetFontSize(tt, 12);
                    tips.Children.Add(tt);
                }
                Layout(tips);
                Save(tips, Path.Combine(dir, $"tooltips_{sfx}.png"), dark);

                // Files panel: empty, then a small folder with one file selected
                var pane = (FrameworkElement)w.FindName("FilesPane");
                w.TestFolder(null);
                w.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
                Save(pane, Path.Combine(dir, $"files_empty_{sfx}.png"));
                var demo = DemoFolder();
                var second = Path.Combine(Path.GetDirectoryName(demo)!, "Website");
                foreach (var f in new[] { "index.md", "docs/setup.md" }) { var p = Path.Combine(second, f); Directory.CreateDirectory(Path.GetDirectoryName(p)!); if (!File.Exists(p)) File.WriteAllText(p, "# x"); }
                w.TestFolder(Path.Combine(demo, "Notes", "Projects", "Plan.md"), demo, second);
                w.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
                w.UpdateLayout();
                Save(pane, Path.Combine(dir, $"files_tree_{sfx}.png"));
                // favourites: two starred files from two folders, and one that is gone
                w.SetFavoritesForTest(Path.Combine(demo, "Archive"), Path.Combine(demo, "Notes", "Ideas.md"), Path.Combine(second, "docs", "setup.md"), Path.Combine(demo, "Gone.md"));
                w.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
                w.UpdateLayout();
                Save(pane, Path.Combine(dir, $"files_favorites_{sfx}.png"));
                w.SetFavoritesForTest();
                // the layout picker, and a pane being dragged (the place it would take)
                var picker = w.LayoutPickerForTest();
                picker.Effect = null;   // the shadow widens the bounds to the left and the picture loses its right edge
                Layout(picker);
                Save(picker, Path.Combine(dir, $"layout_picker_{sfx}.png"), dark);
                foreach (var (files, fx, fy, name) in new[] { (false, 0.95, 0.5, "right"), (true, 0.03, 0.85, "below"), (false, 0.06, 0.5, "beside") })
                {
                    var shot = w.DockOverlayForTest(files, fx, fy);
                    Layout(shot);
                    Save(shot, Path.Combine(dir, $"layout_drag_{name}_{sfx}.png"), dark);
                }
                Save((FrameworkElement)w.FindName("LayoutBtn"), Path.Combine(dir, $"layout_button_{sfx}.png"), dark);
                w.TestFolder(null);
                // the tab strip: four documents, the second with unsaved changes
                var strip = w.TabStripForTest(true);
                w.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
                Save(strip, Path.Combine(dir, $"tabs_{sfx}.png"));
                w.TabStripForTest(false);
            }
            Strings.Lang = keepLang;
        }
    }

    /// <summary>A throw-away folder for the Files panel pictures (hidden and tool folders must not show).</summary>
    private static string DemoFolder()
    {
        var root = Path.Combine(Path.GetTempPath(), "gm_files_demo", "My notes");
        foreach (var f in new[] { "README.md", "Changelog.md", "Notes/Ideas.md", "Notes/Projects/Plan.md", "Notes/Projects/Budget.md",
                                  "Archive/2025.md", "node_modules/skip.md", "image.png" })
        {
            var p = Path.Combine(root, f);
            Directory.CreateDirectory(Path.GetDirectoryName(p)!);
            if (!File.Exists(p)) File.WriteAllText(p, "# " + Path.GetFileNameWithoutExtension(p));
        }
        var hidden = Directory.CreateDirectory(Path.Combine(root, ".obsidian"));
        hidden.Attributes |= FileAttributes.Hidden;
        return root;
    }

    private static void Layout(FrameworkElement el)
    {
        el.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        el.Arrange(new Rect(el.DesiredSize));
        el.UpdateLayout();
    }

    private static void Save(FrameworkElement el, string path, bool? darkBg = null)
    {
        const double scale = 1.5;
        int wpx = (int)Math.Ceiling(Math.Max(el.ActualWidth, el.DesiredSize.Width) * scale);
        int hpx = (int)Math.Ceiling(Math.Max(el.ActualHeight, el.DesiredSize.Height) * scale);
        if (wpx <= 0 || hpx <= 0) return;
        var rtb = new RenderTargetBitmap(wpx, hpx, 96 * scale, 96 * scale, PixelFormats.Pbgra32);
        var dv = new DrawingVisual();
        using (var dc = dv.RenderOpen())
        {
            var r = new Rect(0, 0, wpx / scale, hpx / scale);
            // page colour behind popups so the shadow is visible
            if (darkBg.HasValue) dc.DrawRectangle(new SolidColorBrush(darkBg.Value ? Color.FromRgb(13, 17, 23) : Colors.White), null, r);
            // VisualBrush ignores the element's offset inside the window
            dc.DrawRectangle(new VisualBrush(el) { Stretch = Stretch.None, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top }, null, r);
        }
        rtb.Render(dv);
        var enc = new PngBitmapEncoder();
        enc.Frames.Add(BitmapFrame.Create(rtb));
        using var fs = File.Create(path);
        enc.Save(fs);
    }
}
