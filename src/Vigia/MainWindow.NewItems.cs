// Vigia -- main window: a new Markdown file or a new folder, from the toolbar or the Files panel.
// Copyright (C) 2026 Gabriel Castiglia. Licensed under the GNU GPL, version 2 only.
// Date: November 26, 2026
// SPDX-License-Identifier: GPL-2.0-only

using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Ellipse = System.Windows.Shapes.Ellipse;
using Shape = System.Windows.Shapes.Shape;

namespace Vigia;

/// <summary>The first two buttons of the toolbar, and two entries of the menu of every folder of the Files panel:
/// New Markdown file and New folder, only with the tree unlocked (the padlock of the Files panel; locked, they are
/// greyed out). Both are made on disk at once, in the folder selected in the tree (the folder of
/// the selected file, when a file is selected); with nothing selected there, in Documents\Vigia, which is created
/// and added to the panel the first time. The new item is shown in the tree; a new file is also opened, and a new
/// folder stays selected, so the next file can go straight into it.</summary>
public partial class MainWindow
{
    /// <summary>The two toolbar buttons, greyed out while the tree is locked (the padlock of the Files panel).</summary>
    private Button[] _newItemTools = Array.Empty<Button>();

    /// <summary>New files and folders, like deleting, need the tree unlocked: locked, the buttons and the menu
    /// entries are greyed out and say why.</summary>
    private void UpdateNewItemTools()
    {
        bool locked = _settings.TreeLocked;
        foreach (var b in _newItemTools)
        {
            b.IsEnabled = !locked;
            var name = System.Windows.Automation.AutomationProperties.GetName(b);
            b.ToolTip = locked ? name + "\n" + Strings.Get("NeedsUnlock") : name;
            ToolTipService.SetShowOnDisabled(b, true);
        }
    }

    /// <summary>Where new items go when nothing in the tree says otherwise. The self-test points it elsewhere.</summary>
    internal string DefaultNewDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Vigia");

    /// <summary>An open folder of the panel that is reachable now, or a folder inside one: what the tree can show.</summary>
    private bool ShownInTree(string dir) =>
        _roots.Any(r => !_missing.Contains(r) && (PathCmp.Equals(r, dir) || IsUnder(dir, r)));

    /// <summary>The folder a new item goes into: the selected folder, or the folder of the selected file, as long as
    /// the tree shows it; otherwise <see cref="DefaultNewDir"/> (isDefault = true).</summary>
    internal string NewItemDir(out bool isDefault)
    {
        isDefault = false;
        if (FilesTree.SelectedItem is TreeViewItem { Tag: string p } && !p.StartsWith("::"))
        {
            var dir = SafeDirExists(p) ? p : Path.GetDirectoryName(p);
            if (dir != null && SafeDirExists(dir) && ShownInTree(dir)) return dir;
        }
        isDefault = true;
        return DefaultNewDir;
    }

    /// <summary>"New document.md", or "New document 2.md" and so on when that name is taken.</summary>
    internal static string FreeName(string dir, string stem, string ext)
    {
        for (int n = 1; ; n++)
        {
            var name = n == 1 ? stem + ext : $"{stem} {n}{ext}";
            var p = Path.Combine(dir, name);
            if (!File.Exists(p) && !Directory.Exists(p)) return name;
        }
    }

    /// <summary>Makes the file or folder on disk and returns its path. A file without a Markdown extension gets
    /// ".md"; it starts with its name as a heading. Throws, with a message for the user, when the name is not valid
    /// or already taken; never overwrites anything.</summary>
    internal static string CreateNewItem(string dir, string name, bool folder)
    {
        name = name.Trim();
        if (!folder && !MdExtensions.Contains(Path.GetExtension(name))) name += ".md";
        if (!ValidName(name)) throw new InvalidOperationException(string.Format(Strings.Get("NameInvalid"), name));
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, name);
        if (File.Exists(path) || Directory.Exists(path)) throw new InvalidOperationException(string.Format(Strings.Get("NameExists"), name));
        if (folder) Directory.CreateDirectory(path);
        else
        {
            using var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write);   // CreateNew: never over a file made meanwhile
            using var w = new StreamWriter(fs, new UTF8Encoding(false));
            w.Write("# " + Path.GetFileNameWithoutExtension(name) + Environment.NewLine);
        }
        return path;
    }

    /// <summary>Asks for the name (a free one is offered), makes the item and shows it. <paramref name="inDir"/>
    /// is the folder of a menu of the panel; without it, the selection decides (<see cref="NewItemDir"/>).</summary>
    private async Task NewItemAsync(bool folder, string? inDir = null)
    {
        try
        {
            if (_settings.TreeLocked) { Flash(Strings.Get("NeedsUnlock")); return; }
            bool isDefault = false;
            var dir = inDir ?? NewItemDir(out isDefault);
            var stem = Strings.Plain(folder ? "NewFolderDefault" : "NewMarkdownDefault");
            var offer = FreeName(dir, stem, folder ? "" : ".md");
            var label = string.Format(Strings.Get("NewItemName"), FolderLabel(dir));
            var dlg = new InputDialog(this, Strings.Plain(folder ? "NewFolder" : "NewMarkdown"), new[] { (label, offer) });
            if (dlg.ShowDialog() != true || dlg.Values[0].Length == 0) return;

            string path;
            try { path = CreateNewItem(dir, dlg.Values[0], folder); }
            catch (InvalidOperationException e) { Flash(e.Message); return; }

            if (isDefault && !_roots.Contains(dir, PathCmp)) OpenFolder(dir);   // the default folder joins the panel
            else if (!_settings.FilesPanel) { _settings.FilesPanel = true; _settings.Save(); ApplyFilesVisibility(); BuildMenu(); }
            RefreshTree();
            if (folder) RevealPath(path);
            else await OpenPathAsync(path);   // opening it shows it in the tree (RevealCurrent)
        }
        catch (Exception ex)
        {
            Flash(string.Format(Strings.Get("NewItemFailed"), ex.Message));
            LogError("NewItemAsync", ex);
        }
    }

    /// <summary>A page with a round + badge, drawn like the New folder icon (the icon font has no such page):
    /// the page and the disc take the button's colour, the + the toolbar's.</summary>
    private UIElement NewFileGlyph()
    {
        var icons = (FontFamily)FindResource("Gm.Icons");
        var badge = new Grid { Width = 9, Height = 9, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(0, 0, -2, -1) };
        var ring = new Ellipse { Width = 11, Height = 11, Margin = new Thickness(-1) };
        ring.SetResourceReference(Shape.FillProperty, "Gm.Chrome");
        var disc = new Ellipse();
        disc.SetBinding(Shape.FillProperty, new Binding("Foreground") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Button), 1) });
        var plus = new TextBlock { Text = "\uE710", FontFamily = icons, FontSize = 6, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        plus.SetResourceReference(TextBlock.ForegroundProperty, "Gm.Chrome");
        badge.Children.Add(ring); badge.Children.Add(disc); badge.Children.Add(plus);
        var g = new Grid();
        g.Children.Add(new TextBlock { Text = "\uE8A5", FontFamily = icons });
        g.Children.Add(badge);
        return g;
    }

    private MenuItem NewItemEntry(string dir, bool folder, bool enabled = true) =>
        LockedHelp(MenuEntry(Strings.Get(folder ? "NewFolder" : "NewMarkdown") + "…", () => _ = NewItemAsync(folder, dir), enabled: enabled && !_settings.TreeLocked));

    /// <summary>A menu entry greyed out by the locked tree says why when the pointer rests on it.</summary>
    private MenuItem LockedHelp(MenuItem mi)
    {
        if (!_settings.TreeLocked) return mi;
        mi.ToolTip = Strings.Get("NeedsUnlock");
        ToolTipService.SetShowOnDisabled(mi, true);
        return mi;
    }
}
