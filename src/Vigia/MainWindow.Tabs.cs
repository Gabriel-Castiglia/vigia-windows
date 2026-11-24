// Vigia -- main window: tabs, several documents in one window, and the tabs of the last session.
// Copyright (C) 2026 Gabriel Castiglia. Licensed under the GNU GPL, version 2 only.
// Date: November 24, 2026
// SPDX-License-Identifier: GPL-2.0-only

using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;
using WebView2 = Microsoft.Web.WebView2.Wpf.WebView2;

namespace Vigia;

/// <summary>One open document: its own editor page and what the window keeps about it. The page is created the
/// first time the tab is shown (a restored session with many tabs starts as fast as with one), and it stays alive
/// afterwards, so each tab keeps its undo history, cursor and scroll position while another one is shown.</summary>
internal sealed class DocTab
{
    public WebView2? View;
    public bool Ready;                    // its page answered "ready" and was set up
    public string? Path;                  // null: untitled, or the sample
    public string? PendingOpen;           // file to load as soon as its page is ready
    public (string Text, bool Dirty)? Restore;   // text carried over a restart of its page
    public bool Dirty, Bom, Crlf, IsSample;
    public int Words;
    public string? DiskText;              // what the file held when last read or written
    public long LastEditSeq;
    public FileSystemWatcher? Watcher;
    public bool ChangedOnDisk;            // its file changed while another tab was shown: checked when shown again
    public string RecoveryId = Guid.NewGuid().ToString("N");
    public string? WebSpell;              // spell check dictionary its browser was started with
    // what its page was last told, so showing the tab again only sends what changed meanwhile
    public string? PageLang, PageTheme, PageMode;
    public bool? PageLocked, PageSpell;
    public int? PageWidth;
    /// <summary>Messages its page sent while another tab was shown, handled when it is shown again.</summary>
    public readonly List<string> Held = new();
    public Border? Header;
    public TextBlock? Label, Dot;
    public Button? CloseBtn;
}

public partial class MainWindow
{
    private readonly List<DocTab> _tabs = new();
    private DocTab _cur = null!;
    private readonly Stack<string> _closedTabs = new();   // Ctrl+Shift+T reopens the last one closed
    private readonly Dictionary<string, CoreWebView2Environment> _envs = new();
    private int _docBusy;   // > 0 while an operation on the shown document awaits: switching tabs waits for it
    private bool _recoveryOffered, _selfTestStarted;   // once per window, on the first page that is ready
    private bool _windowClosing;                       // OnClosing is asking about the unsaved tabs

    // The shown document. The rest of the window works on these, as when there was a single document.
    private WebView2 Web => _cur.View!;
    private string? _path { get => _cur.Path; set => _cur.Path = value; }
    private bool _dirty { get => _cur.Dirty; set => _cur.Dirty = value; }
    private bool _ready { get => _cur.Ready; set => _cur.Ready = value; }
    private string? _pendingOpen { get => _cur.PendingOpen; set => _cur.PendingOpen = value; }
    private (string Text, bool Dirty)? _restore { get => _cur.Restore; set => _cur.Restore = value; }
    private int _words { get => _cur.Words; set => _cur.Words = value; }
    private bool _isSample { get => _cur.IsSample; set => _cur.IsSample = value; }
    private bool _bom { get => _cur.Bom; set => _cur.Bom = value; }
    private bool _crlf { get => _cur.Crlf; set => _cur.Crlf = value; }
    private string? _diskText { get => _cur.DiskText; set => _cur.DiskText = value; }
    private long _lastEditSeq { get => _cur.LastEditSeq; set => _cur.LastEditSeq = value; }
    private string _recoveryId => _cur.RecoveryId;

    /// <summary>Marks an operation on the shown document; a tab switch asked for meanwhile waits until it ends,
    /// so the operation never finishes on another document than the one it started on.</summary>
    private BusyMark DocBusy() { _docBusy++; return new BusyMark(this); }

    private readonly struct BusyMark(MainWindow w) : IDisposable
    {
        public void Dispose() => w._docBusy--;
    }

    /// <summary>Called first thing from the constructor: the tabs of the last session (when File → Reopen tabs at
    /// start is on) and the files of the command line, the last of these shown. Only the shown tab gets its page
    /// now; the others get theirs when they are first shown.</summary>
    private void InitTabs(IReadOnlyList<string> startFiles)
    {
        TabNew.Click += async (_, _) => await NewDocAsync();
        TabList.Click += (_, _) => ShowTabList();
        TabScroll.PreviewMouseWheel += (_, e) => { TabScroll.ScrollToHorizontalOffset(TabScroll.HorizontalOffset - e.Delta / 2.0); e.Handled = true; };
        var files = new List<string>();
        if (_settings.RestoreTabs && !Settings.IsSecondary)
            foreach (var p in _settings.OpenTabs)
                if (SafeFileExists(p) && !files.Contains(p, PathCmp)) files.Add(p);
        int active = Math.Clamp(_settings.ActiveTab, 0, Math.Max(0, files.Count - 1));
        foreach (var p in startFiles)
        {
            int at = files.FindIndex(f => PathCmp.Equals(f, p));
            if (at < 0) { files.Add(p); at = files.Count - 1; }
            active = at;
        }
        if (files.Count == 0) files.Add(null!);   // one untitled tab (the sample on the very first start)
        foreach (var p in files) AddTab(p);
        _cur = _tabs[active];
        CreateView(_cur);
        RefreshTabs();
    }

    private static bool SafeFileExists(string p) { try { return File.Exists(p); } catch { return false; } }

    private DocTab AddTab(string? pendingFile, int at = -1)
    {
        var t = new DocTab { PendingOpen = pendingFile };
        if (at < 0 || at > _tabs.Count) _tabs.Add(t); else _tabs.Insert(at, t);
        BuildTabHeader(t);
        return t;
    }

    /// <summary>The editor page of a tab, in the cell under the find bar; hidden unless its tab is shown.</summary>
    private void CreateView(DocTab t)
    {
        var view = new WebView2
        {
            DefaultBackgroundColor = IsDark ? System.Drawing.Color.FromArgb(13, 17, 23) : System.Drawing.Color.White,
            Visibility = t == _cur ? Visibility.Visible : Visibility.Collapsed,
        };
        Grid.SetRow(view, 2);
        view.ZoomFactorChanged += (_, _) => { if (view == _cur.View) SyncZoom(); };
        DocGrid.Children.Add(view);
        t.View = view;
    }

    /// <summary>The page's browser for a spell check language. Every tab with that language shares it.</summary>
    private async Task<CoreWebView2Environment> EnvironmentFor(string spell)
    {
        if (_envs.TryGetValue(spell, out var env)) return env;
        env = await CoreWebView2Environment.CreateAsync(null, WebProfile(spell), new CoreWebView2EnvironmentOptions { Language = spell });
        _envs[spell] = env;
        return env;
    }

    private DocTab? TabOf(object? sender) => sender is CoreWebView2 core ? _tabs.FirstOrDefault(t => t.View?.CoreWebView2 == core) : null;

    private DocTab? FindTab(string path) => _tabs.FirstOrDefault(t => PathCmp.Equals(t.Path ?? t.PendingOpen ?? "", path));

    /// <summary>An untitled tab nobody typed in, or the sample as installed: a file opened now takes its place.</summary>
    private bool IsPristine(DocTab t) => t.Path == null && t.PendingOpen == null && t.Restore == null && !t.Dirty && (t.IsSample || t.Words == 0);

    // ------------------------------------------------------------------ switching
    /// <summary>Shows a tab. The one left is saved first when autosave may write it (its draft is kept otherwise),
    /// and the one shown gets the settings changed while it was hidden (mode, lock, width, theme, language).</summary>
    private async Task ActivateAsync(DocTab t)
    {
        if (t == _cur || !_tabs.Contains(t)) { RefreshTabs(); return; }
        var end = DateTime.UtcNow.AddSeconds(5);
        while (_docBusy > 0 && DateTime.UtcNow < end) await Task.Delay(15);
        if (t == _cur || !_tabs.Contains(t)) return;
        var old = _cur;
        if (old.Ready)
        {
            _settings.Zoom = Web.ZoomFactor;
            if (old.Dirty) { if (CanAutoSave()) await SaveAsync(false, auto: true); else await SaveRecoveryAsync(); }
            if (FindBar.Visibility == Visibility.Visible) { try { await Js("mdApp.findClose()"); } catch { } }
        }
        _autoSaveTimer.Stop();
        if (t == _cur || !_tabs.Contains(t)) return;
        bool fresh = t.View == null;
        _cur = t;
        if (fresh) CreateView(t);
        foreach (var x in _tabs) if (x.View != null) x.View.Visibility = x == t ? Visibility.Visible : Visibility.Collapsed;
        if (fresh) _ = InitWebAsync(t);
        else if (t.View!.CoreWebView2 != null && Math.Abs(t.View.ZoomFactor - _settings.Zoom) > 0.001) t.View.ZoomFactor = _settings.Zoom;
        OutlineList.Items.Clear(); OutlineEmpty.Visibility = Visibility.Visible;
        RefreshTabs(); BuildMenu(); UpdateTitle(); UpdateStatus(); SyncZoom();
        if (!t.Ready) { await ReplayHeldAsync(t); return; }   // a page still starting may have said "ready" meanwhile
        await SyncPageAsync();
        if (t != _cur) return;
        await ReplayHeldAsync(t);
        await Js("mdApp.outline()");
        if (FindBar.Visibility == Visibility.Visible) await FindRun(0);
        if (t.ChangedOnDisk && t.Path != null)
        {
            t.ChangedOnDisk = false;
            if (SafeFileExists(t.Path)) OnExternalChange(t.Path); else OnPossiblyGone(t.Path);
        }
        Web.Focus();
        await Js("mdApp.focus()");
    }

    private async Task ReplayHeldAsync(DocTab t)
    {
        while (t == _cur && t.Held.Count > 0)
        {
            var json = t.Held[0]; t.Held.RemoveAt(0);
            await HandleWebMessage(json, null);
        }
    }

    /// <summary>Tells the shown page the settings it does not have yet. Language and theme rebuild the editor (its undo
    /// history starts again), so they are only sent when they changed; a new spell check language needs a new browser.</summary>
    private async Task SyncPageAsync()
    {
        var t = _cur;
        if (!t.Ready) return;
        if (t.WebSpell != SpellLang(_settings.Lang)) { await RestartWebAsync(); return; }
        string theme = IsDark ? "dark" : "light";
        if (t.PageTheme != theme) { t.PageTheme = theme; await Js($"mdApp.setTheme({Q(theme)})"); }
        if (t.PageLang != _settings.Lang) { t.PageLang = _settings.Lang; await Js($"mdApp.setLang({Q(_settings.Lang)})"); }
        if (t.PageMode != _settings.Mode) { t.PageMode = _settings.Mode; await Js($"mdApp.setMode({Q(_settings.Mode)})"); }
        if (t.PageLocked != _settings.ReadOnly) { t.PageLocked = _settings.ReadOnly; await Js($"mdApp.setLocked({(_settings.ReadOnly ? "true" : "false")})"); }
        if (t.PageWidth != _settings.TextWidth) { t.PageWidth = _settings.TextWidth; await Js($"mdApp.setWidth({_settings.TextWidth})"); }
        if (t.PageSpell != _settings.Spell) { t.PageSpell = _settings.Spell; await Js($"mdApp.setSpell({(_settings.Spell ? "true" : "false")})"); }
    }

    /// <summary>What a page knows right after mdApp.init.</summary>
    private void PageInitialised(DocTab t)
    {
        t.PageLang = _settings.Lang; t.PageTheme = IsDark ? "dark" : "light"; t.PageMode = _settings.Mode;
        t.PageLocked = _settings.ReadOnly; t.PageSpell = _settings.Spell; t.PageWidth = _settings.TextWidth;
    }

    private Task NextTabAsync(int step)
    {
        if (_tabs.Count < 2) return Task.CompletedTask;
        int i = (_tabs.IndexOf(_cur) + step + _tabs.Count) % _tabs.Count;
        return ActivateAsync(_tabs[i]);
    }

    /// <summary>Waits until a tab's page is up and its file loaded (false when it was closed or another tab was shown).</summary>
    private async Task<bool> WaitLoadedAsync(DocTab t)
    {
        await WaitFor(() => Task.FromResult(!_tabs.Contains(t) || t != _cur || (t.Ready && t.PendingOpen == null && t.Restore == null && _docBusy == 0)), 20);
        return _tabs.Contains(t) && t == _cur && t.Ready;
    }

    /// <summary>A tab for new content (a file, the sample, a recovered draft): the shown one when nobody typed in
    /// it, a new one otherwise. True once that tab's page is ready and shown.</summary>
    private async Task<bool> TabForContentAsync()
    {
        if (IsPristine(_cur) && _cur.Ready) return true;
        var t = AddTab(null, _tabs.IndexOf(_cur) + 1);
        await ActivateAsync(t);
        return await WaitLoadedAsync(t);
    }

    // ------------------------------------------------------------------ opening
    private async void OpenPath(string path) => await OpenPathAsync(path);

    /// <summary>Opens a file: the tab that already has it is shown; otherwise it goes into the shown tab when nobody
    /// typed in it, or into a new tab next to it. True once the file is shown.</summary>
    private async Task<bool> OpenPathAsync(string path)
    {
        if (Directory.Exists(path)) { OpenFolder(path); return false; }
        try { path = Path.GetFullPath(path); } catch { return false; }
        DocTab t;
        if (FindTab(path) is { } open) t = open;
        else if (IsPristine(_cur) && !_cur.Ready && _cur.View != null) { t = _cur; t.PendingOpen = path; t.IsSample = false; RefreshTabs(); }
        else if (IsPristine(_cur) && _cur.Ready) { await LoadFileAsync(path); return PathCmp.Equals(_path ?? "", path); }
        else t = AddTab(path, _tabs.IndexOf(_cur) + 1);
        await ActivateAsync(t);
        return await WaitLoadedAsync(t) && PathCmp.Equals(t.Path ?? "", path);
    }

    // ------------------------------------------------------------------ closing
    /// <summary>Closes a tab, asking about its unsaved changes as on closing the window. The last tab is not removed:
    /// it becomes an empty untitled document, so the window always has one.</summary>
    private async Task<bool> CloseTabAsync(DocTab t)
    {
        if (!_tabs.Contains(t)) return true;
        if (t.Dirty)
        {
            await ActivateAsync(t);
            if (_cur != t || !await ConfirmDiscardAsync()) return false;
        }
        var file = t.Path ?? t.PendingOpen;
        if (file != null) _closedTabs.Push(file);
        if (_tabs.Count == 1)
        {
            ClearRecovery();
            await ClearDocumentAsync();
            return true;
        }
        int i = _tabs.IndexOf(t);
        if (t == _cur) await ActivateAsync(_tabs[i + 1 < _tabs.Count ? i + 1 : i - 1]);
        if (t == _cur) return false;   // still busy: left as it was
        DropTab(t);
        return true;
    }

    /// <summary>Takes a hidden tab out of the window: its page, its watcher and its draft go.</summary>
    private void DropTab(DocTab t)
    {
        try { File.Delete(Path.Combine(RecoveryDir, t.RecoveryId + ".md")); File.Delete(Path.Combine(RecoveryDir, t.RecoveryId + ".json")); } catch { }
        t.Watcher?.Dispose(); t.Watcher = null;
        _tabs.Remove(t);
        if (t.View != null)
        {
            if (t.View.CoreWebView2 != null) t.View.CoreWebView2.WebMessageReceived -= OnWebMessage;
            DocGrid.Children.Remove(t.View);
            t.View.Dispose();
            t.View = null;
        }
        RefreshTabs(); BuildMenu();
    }

    /// <summary>Close the others / the ones to the right / all: each one with unsaved changes asks; Cancel stops there.</summary>
    private async Task CloseTabsAsync(IEnumerable<DocTab> which)
    {
        foreach (var t in which.ToList())
            if (!await CloseTabAsync(t)) return;
    }

    private async Task ReopenClosedTabAsync()
    {
        while (_closedTabs.Count > 0)
        {
            var p = _closedTabs.Pop();
            if (SafeFileExists(p)) { await OpenPathAsync(p); return; }
        }
        Flash(Strings.Get("TabNoClosed"));
    }

    /// <summary>Every tab with unsaved changes is shown and asked about in turn (autosave writes the ones it may).
    /// False when one was cancelled: nothing closes then.</summary>
    private async Task<bool> ConfirmDiscardAllAsync()
    {
        foreach (var t in _tabs.Where(x => x.Dirty).ToList())
        {
            if (!_tabs.Contains(t) || !t.Dirty) continue;
            await ActivateAsync(t);
            if (_cur != t || !await ConfirmDiscardAsync()) return false;
        }
        return true;
    }

    /// <summary>The window is closing: the files of the tabs are kept for the next start when File → Reopen tabs
    /// at start is on, and forgotten otherwise. Untitled documents are never kept.</summary>
    private void SaveTabSession()
    {
        if (Settings.IsSecondary) return;   // the first window owns the session
        if (!_settings.RestoreTabs) { _settings.OpenTabs = new(); _settings.ActiveTab = 0; return; }
        var files = new List<string>();
        int active = 0;
        foreach (var t in _tabs)
        {
            var f = t.Path ?? t.PendingOpen;
            if (f == null || files.Contains(f, PathCmp)) continue;
            if (t == _cur) active = files.Count;
            files.Add(f);
        }
        _settings.OpenTabs = files; _settings.ActiveTab = active;
    }

    private void ToggleRestoreTabs()
    {
        _settings.RestoreTabs = !_settings.RestoreTabs;
        SaveTabSession(); _settings.Save();
        BuildMenu();
        Flash(Strings.Get(_settings.RestoreTabs ? "RestoreTabsOn" : "RestoreTabsOff"));
    }

    /// <summary>The files of every tab, the shown one last (an update reopens them in that order, Update.cs).</summary>
    private IEnumerable<string> TabFiles() =>
        _tabs.Where(t => t != _cur).Append(_cur).Select(t => t.Path ?? t.PendingOpen).Where(p => p != null && SafeFileExists(p)).Select(p => p!);

    /// <summary>A tab's files moved or were renamed in the Files panel: the tab follows (MainWindow.FileOps.cs).</summary>
    private async Task FollowMoveInTabs(string from, string to, bool isDir)
    {
        foreach (var t in _tabs.Where(x => x != _cur))
        {
            foreach (var get in new Func<string?>[] { () => t.Path, () => t.PendingOpen })
            {
                var p = get();
                if (p == null || !(PathCmp.Equals(p, from) || (isDir && IsUnder(p, from)))) continue;
                var np = to + p.Substring(from.Length);
                if (p == t.Path)
                {
                    t.Path = np;
                    WatchTab(t, np);
                    if (t.Ready && t.View?.CoreWebView2 != null) { try { await t.View.CoreWebView2.ExecuteScriptAsync($"mdApp.setBase({DocBaseJs(np)})"); } catch { } }
                }
                else t.PendingOpen = np;
            }
        }
        RefreshTabs();
    }

    // ------------------------------------------------------------------ the tab strip
    private static string NameOf(DocTab t) =>
        t.Path != null ? Path.GetFileName(t.Path)
        : t.PendingOpen != null ? Path.GetFileName(t.PendingOpen)
        : Strings.Get(t.IsSample ? "SampleTitle" : "Untitled");

    private void BuildTabHeader(DocTab t)
    {
        var label = new TextBlock { TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center, FontSize = 12.5 };
        var dot = new TextBlock { Text = "●", FontSize = 10, Margin = new Thickness(0, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center, Visibility = Visibility.Collapsed };
        dot.SetResourceReference(TextBlock.ForegroundProperty, "Gm.Accent");
        var close = new Button { Content = "", Style = (Style)FindResource("Gm.ToolButton"), FontSize = 9, Width = 20, Height = 20, Margin = new Thickness(6, 0, 0, 0), Focusable = false };
        close.Click += async (_, e) => { e.Handled = true; await CloseTabAsync(t); };
        var row = new DockPanel();
        DockPanel.SetDock(close, Dock.Right);
        DockPanel.SetDock(dot, Dock.Left);
        row.Children.Add(close); row.Children.Add(dot); row.Children.Add(label);
        var header = new Border
        {
            Child = row, CornerRadius = new CornerRadius(7, 7, 0, 0), Padding = new Thickness(12, 4, 5, 4), Margin = new Thickness(0, 0, 2, -1),
            MinWidth = 96, MaxWidth = 230, BorderThickness = new Thickness(1, 1, 1, 0), Cursor = Cursors.Arrow,
        };
        header.SetResourceReference(TextElement.ForegroundProperty, "Gm.Text");
        ToolTipService.SetInitialShowDelay(header, 700);
        header.MouseEnter += (_, _) => PaintTab(t);
        header.MouseLeave += (_, _) => PaintTab(t);
        header.MouseDown += async (_, e) =>
        {
            if (e.ChangedButton == MouseButton.Middle) { e.Handled = true; await CloseTabAsync(t); }
        };
        header.MouseLeftButtonDown += (_, e) => { _tabDrag = t; _tabDragStart = e.GetPosition(TabPanel); _tabDragged = false; header.CaptureMouse(); e.Handled = true; };
        header.MouseMove += (_, e) => DragTab(e);
        header.MouseLeftButtonUp += async (_, e) =>
        {
            header.ReleaseMouseCapture();
            bool dragged = _tabDragged; _tabDrag = null;
            if (!dragged) await ActivateAsync(t);
            e.Handled = true;
        };
        header.ContextMenuOpening += (_, _) => header.ContextMenu = TabMenu(t);
        header.ContextMenu = new ContextMenu();
        t.Header = header; t.Label = label; t.Dot = dot; t.CloseBtn = close;
    }

    /// <summary>Puts the headers in the order of the tabs and refreshes every name, mark and colour.</summary>
    private void RefreshTabs()
    {
        if (TabPanel == null) return;
        TabPanel.Children.Clear();
        foreach (var t in _tabs)
        {
            if (t.Header == null) BuildTabHeader(t);
            TabPanel.Children.Add(t.Header);
            RefreshTabHeader(t);
        }
        TabNew.ToolTip = Strings.Plain("TabNew") + "   Ctrl+N";
        TabList.ToolTip = Strings.Get("TabList");
        TabList.Visibility = _tabs.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        if (_cur?.Header != null) Dispatcher.BeginInvoke(() => _cur?.Header?.BringIntoView(), System.Windows.Threading.DispatcherPriority.Loaded);
        // with Reopen tabs at start on, the session is written as the tabs change, not only on closing: it survives
        // a crash or a power cut too
        // (not while the window is closing: asking about each unsaved tab shows it, and the shown tab must stay the one left)
        if (_settings.RestoreTabs && _folderInit && !_windowClosing && !Settings.IsSecondary) { SaveTabSession(); _settings.Save(); }
    }

    private void RefreshTabHeader(DocTab t)
    {
        if (t.Label == null || t.Header == null || t.Dot == null || t.CloseBtn == null) return;
        t.Label.Text = NameOf(t);
        t.Label.FontWeight = t == _cur ? FontWeights.SemiBold : FontWeights.Normal;
        t.Dot.Visibility = t.Dirty ? Visibility.Visible : Visibility.Collapsed;
        t.Header.ToolTip = (t.Path ?? t.PendingOpen ?? NameOf(t)) + (t.Dirty ? "\n" + Strings.Get("Modified") : "");
        t.CloseBtn.ToolTip = Strings.Plain("TabClose") + "   Ctrl+W";
        PaintTab(t);
    }

    private void PaintTab(DocTab t)
    {
        if (t.Header == null || t.CloseBtn == null) return;
        bool cur = t == _cur, over = t.Header.IsMouseOver;
        t.Header.SetResourceReference(Border.BackgroundProperty, cur ? "Gm.Page" : over ? "Gm.Hover" : "Gm.Chrome");
        t.Header.SetResourceReference(Border.BorderBrushProperty, cur ? "Gm.ChromeBorder" : "Gm.Chrome");
        t.CloseBtn.Opacity = cur || over ? 1 : 0;   // keeps its room: the names do not move under the pointer
    }

    private DocTab? _tabDrag;
    private Point _tabDragStart;
    private bool _tabDragged;

    /// <summary>Dragging a tab along the strip moves it among the others.</summary>
    private void DragTab(MouseEventArgs e)
    {
        if (_tabDrag == null || e.LeftButton != MouseButtonState.Pressed) return;
        var pos = e.GetPosition(TabPanel);
        if (!_tabDragged && Math.Abs(pos.X - _tabDragStart.X) < SystemParameters.MinimumHorizontalDragDistance) return;
        _tabDragged = true;
        int from = _tabs.IndexOf(_tabDrag), to = from;
        double x = 0;
        for (int i = 0; i < _tabs.Count; i++)
        {
            double w = _tabs[i].Header?.ActualWidth ?? 0;
            if (pos.X < x + w) { to = i; break; }
            x += w + 2; to = i;
        }
        if (to == from) return;
        _tabs.RemoveAt(from); _tabs.Insert(to, _tabDrag);
        var h = _tabDrag.Header!;
        TabPanel.Children.Remove(h); TabPanel.Children.Insert(to, h);
        h.CaptureMouse();   // taking it out of the panel lost the capture
    }

    private ContextMenu TabMenu(DocTab t)
    {
        int i = _tabs.IndexOf(t);
        var file = t.Path ?? t.PendingOpen;
        var m = new ContextMenu();
        m.Items.Add(MenuEntryAsync(Strings.Plain("TabClose"), () => CloseTabAsync(t), gesture: "Ctrl+W"));
        m.Items.Add(MenuEntryAsync(Strings.Get("TabCloseOthers"), () => CloseTabsAsync(_tabs.Where(x => x != t)), enabled: _tabs.Count > 1));
        m.Items.Add(MenuEntryAsync(Strings.Get("TabCloseRight"), () => CloseTabsAsync(_tabs.Skip(i + 1)), enabled: i < _tabs.Count - 1));
        m.Items.Add(MenuEntryAsync(Strings.Get("TabCloseAll"), () => CloseTabsAsync(_tabs.AsEnumerable().Reverse())));
        m.Items.Add(new Separator());
        m.Items.Add(MenuEntryAsync(Strings.Plain("TabReopen"), ReopenClosedTabAsync, gesture: "Ctrl+Shift+T", enabled: _closedTabs.Count > 0));
        m.Items.Add(new Separator());
        m.Items.Add(MenuEntry(Strings.Get("CopyPath"), () => Clipboard.SetText(file!), enabled: file != null));
        m.Items.Add(MenuEntry(Strings.Plain("OpenFolder"), () => Process.Start("explorer.exe", $"/select,\"{file}\""), enabled: file != null && SafeFileExists(file)));
        m.Items.Add(new Separator());
        m.Items.Add(MenuEntry(Strings.Plain("RestoreTabs"), ToggleRestoreTabs, _settings.RestoreTabs));
        return m;
    }

    private MenuItem MenuEntryAsync(string text, Func<Task> run, string? gesture = null, bool enabled = true)
    {
        var mi = new MenuItem { Header = text, IsEnabled = enabled, InputGestureText = gesture ?? "" };
        mi.Click += async (_, _) => { try { await run(); } catch { } };
        return mi;
    }

    /// <summary>--render-chrome: the strip with four documents of a sample folder (never loaded), the second one with
    /// unsaved changes; false takes them away again.</summary>
    internal FrameworkElement TabStripForTest(bool show)
    {
        var dir = Path.Combine(Path.GetTempPath(), "gm_files_demo", "My notes");
        if (show)
        {
            _cur.Path = Path.Combine(dir, "Notes", "Projects", "Plan.md");
            foreach (var n in new[] { "Notes\\Ideas.md", "README.md", "Changelog.md" }) AddTab(Path.Combine(dir, n));
            _tabs[1].Dirty = true;
            TabStrip.Width = 640;
        }
        else
        {
            foreach (var t in _tabs.Where(t => t != _cur).ToList()) DropTab(t);
            _cur.Path = null;
            TabStrip.Width = double.NaN;
        }
        RefreshTabs();
        TabStrip.UpdateLayout();
        return TabStrip;
    }

    /// <summary>The ⌄ button at the end of the strip: every tab by name, for when they no longer fit.</summary>
    private void ShowTabList()
    {
        var m = new ContextMenu { PlacementTarget = TabList, Placement = PlacementMode.Bottom };
        foreach (var t in _tabs)
        {
            var tab = t;
            var mi = new MenuItem { Header = (t.Dirty ? "● " : "") + NameOf(t).Replace("_", "__"), IsCheckable = true, IsChecked = t == _cur, ToolTip = t.Path ?? t.PendingOpen };
            mi.Click += async (_, _) => await ActivateAsync(tab);
            m.Items.Add(mi);
        }
        m.IsOpen = true;
    }
}
