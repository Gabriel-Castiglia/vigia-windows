// Vigia -- main window: live reload, updates and other features.
// Copyright (C) 2026 Gabriel Castiglia. Licensed under the GNU GPL, version 2 only.
// Date: November 10, 2026
// SPDX-License-Identifier: GPL-2.0-only

using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Vigia;

/// <summary>Find/replace, outline, the watch for changes made by other programs, autosave, crash recovery and the
/// single-instance hand-over (updates: MainWindow.Update.cs).</summary>
public partial class MainWindow
{
    // Per document, kept in its tab (MainWindow.Tabs.cs): _crlf and _bom (written back as the file had them), the
    // watcher, _diskText (the text last read from or written to disk: the same text again is not a change),
    // _lastEditSeq (the last edit the page reported, app.js editSeq) and the id of its recovery draft.
    private bool _saving;
    private readonly DispatcherTimer _autoSaveTimer = new() { Interval = TimeSpan.FromSeconds(1.5) };
    private readonly DispatcherTimer _recoveryTimer = new() { Interval = TimeSpan.FromSeconds(20) };
    private readonly DispatcherTimer _statusTimer = new() { Interval = TimeSpan.FromSeconds(4) };
    private static string RecoveryDir => Path.Combine(Settings.Dir, "recovery");
    public const string Repo = "Gabriel-Castiglia/vigia-windows";

    private void InitFeatures()
    {
        FindBox.TextChanged += async (_, _) => await FindRun(0);
        FindBox.PreviewKeyDown += async (_, e) =>
        {
            if (e.Key == Key.Enter) { e.Handled = true; await FindStep(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? -1 : 1); }
            else if (e.Key == Key.Escape) { e.Handled = true; await HideFind(); }
        };
        ReplaceBox.PreviewKeyDown += async (_, e) =>
        {
            if (e.Key == Key.Enter) { e.Handled = true; await ReplaceOne(); }
            else if (e.Key == Key.Escape) { e.Handled = true; await HideFind(); }
        };
        FindCase.Click += async (_, _) => await FindRun(0);
        FindPrev.Click += async (_, _) => await FindStep(-1);
        FindNextBtn.Click += async (_, _) => await FindStep(1);
        FindCloseBtn.Click += async (_, _) => await HideFind();
        ReplaceBtn.Click += async (_, _) => await ReplaceOne();
        ReplaceAllBtn.Click += async (_, _) => await ReplaceAllAsync();
        OutlineList.PreviewMouseLeftButtonUp += async (_, _) =>
        {
            if (OutlineList.SelectedIndex >= 0 && _ready) await Js($"mdApp.scrollToHeading({OutlineList.SelectedIndex})");
        };
        _recoveryTimer.Tick += async (_, _) => await SaveRecoveryAsync();
        _recoveryTimer.Start();
        _statusTimer.Tick += (_, _) => { _statusTimer.Stop(); UpdateStatus(); };
        _autoSaveTimer.Tick += async (_, _) => await AutoSaveAsync();
        ApplyOutlineVisibility();
        LocalizeFeatures();
        Loaded += (_, _) => Dispatcher.BeginInvoke(async () =>
        {
            await Task.Delay(1500);
            if (SelfTestLog == null && !Settings.IsSecondary) await CheckUpdatesAsync(false);   // once a day for all the profiles
        });
    }

    private void LocalizeFeatures()
    {
        FindBox.Tag = Strings.Get("FindPlaceholder");
        ReplaceBox.Tag = Strings.Get("ReplacePlaceholder");
        ReplaceBtn.Content = Strings.Get("ReplaceOne");
        ReplaceAllBtn.Content = Strings.Get("ReplaceAll");
        FindCase.ToolTip = Strings.Get("MatchCase");
        FindPrev.ToolTip = Strings.Get("Prev");
        FindNextBtn.ToolTip = Strings.Get("Next");
        FindCloseBtn.ToolTip = Strings.Get("Close");
        OutlineTitle.Text = Strings.Get("OutlineTitle");
        OutlineEmpty.Text = Strings.Get("OutlineEmpty");
        if (_folderInit) LocalizeFolder();
    }

    private void StopFeatures()
    {
        _recoveryTimer.Stop();
        _autoSaveTimer.Stop();
        foreach (var t in _tabs) { t.Watcher?.Dispose(); t.Watcher = null; }
    }

    private void Flash(string text)
    {
        StatusState.Text = text;
        _statusTimer.Stop(); _statusTimer.Start();
    }

    // ------------------------------------------------------------------ find / replace
    private void ShowFind(bool replace)
    {
        FindBar.Visibility = Visibility.Visible;
        ReplaceRow.Visibility = replace ? Visibility.Visible : Visibility.Collapsed;
        Dispatcher.BeginInvoke(async () =>
        {
            if (string.IsNullOrEmpty(FindBox.Text)) { var sel = await SelectedTextAsync(); if (sel.Length is > 0 and < 200 && !sel.Contains('\n')) FindBox.Text = sel; }
            FindBox.Focus(); FindBox.SelectAll();
        }, DispatcherPriority.Input);
    }

    private async Task HideFind()
    {
        if (FindBar.Visibility != Visibility.Visible) return;
        FindBar.Visibility = Visibility.Collapsed;
        if (_ready) { await Js("mdApp.findClose()"); await Js("mdApp.focus()"); }
        Web.Focus();
    }

    private string CaseArg => FindCase.IsChecked == true ? "true" : "false";

    private async Task FindRun(int step)
    {
        if (!_ready) return;
        await Js($"mdApp.find({Q(FindBox.Text)},{CaseArg},{step})");
    }

    /// <summary>F3 / Shift+F3: the next or previous match; opens the find bar first when it is closed.</summary>
    private async Task FindStep(int step)
    {
        if (FindBar.Visibility != Visibility.Visible) { ShowFind(false); return; }
        if (FindBox.Text.Length > 0) await FindRun(step);
    }

    private void UpdateFindCount(int index, int total)
    {
        FindCount.Text = FindBox.Text.Length == 0 ? "" : total == 0 ? Strings.Get("NoResults") : string.Format(Strings.Get("OfCount"), index + 1, total);
    }

    private async Task ReplaceOne()
    {
        if (!_ready || FindBox.Text.Length == 0) return;
        await Js($"mdApp.replace({Q(FindBox.Text)},{CaseArg},{Q(ReplaceBox.Text)})");
    }

    private async Task ReplaceAllAsync()
    {
        if (!_ready || FindBox.Text.Length == 0) return;
        var n = await Js($"mdApp.replaceAll({Q(FindBox.Text)},{CaseArg},{Q(ReplaceBox.Text)})");
        Flash(string.Format(Strings.Get("Replaced"), n));
    }

    // ------------------------------------------------------------------ outline
    private void ToggleOutline()
    {
        _settings.Outline = !_settings.Outline; _settings.Save();
        ApplyOutlineVisibility(); BuildMenu();
    }

    private void ApplyOutlineVisibility()
    {
        if (_folderInit) ApplySideLayout();   // the sidebar holds both panes (MainWindow.Folder.cs)
    }

    private void UpdateOutline(JsonElement items)
    {
        OutlineList.Items.Clear();
        foreach (var it in items.EnumerateArray())
        {
            int level = it.GetProperty("level").GetInt32();
            var tb = new TextBlock
            {
                Text = it.GetProperty("text").GetString(),
                TextTrimming = TextTrimming.CharacterEllipsis,
                FontWeight = level == 1 ? FontWeights.SemiBold : FontWeights.Normal,
                Opacity = level >= 3 ? 0.75 : 1,
            };
            OutlineList.Items.Add(new ListBoxItem { Content = tb, Padding = new Thickness(10 + (level - 1) * 14, 5, 8, 5), ToolTip = tb.Text });
        }
        OutlineEmpty.Visibility = OutlineList.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    // ------------------------------------------------------------------ external changes
    private void Watch(string? path) => WatchTab(_cur, path);

    /// <summary>Watches a tab's file. A change while the tab is shown is handled at once; while it is hidden, it
    /// is noted and handled when the tab is shown again (MainWindow.Tabs.cs ActivateAsync).</summary>
    private void WatchTab(DocTab tab, string? path)
    {
        tab.Watcher?.Dispose(); tab.Watcher = null;
        if (path == null) return;
        try
        {
            var w = new FileSystemWatcher(Path.GetDirectoryName(path)!, Path.GetFileName(path))
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
            };
            void Changed(bool maybeGone) => Dispatcher.BeginInvoke(() =>
            {
                if (tab.Watcher != w) return;   // watching another file by now
                if (tab != _cur) { tab.ChangedOnDisk = true; return; }
                if (maybeGone) OnPossiblyGone(path); else OnExternalChange(path);
            });
            // Many programs save by writing a temporary file and renaming it over this one, or by deleting and
            // recreating it: every one of those is a change to reload.
            w.Changed += (_, _) => Changed(false);
            w.Created += (_, _) => Changed(false);
            // our own save renames the file away for a moment ("a.md -> a.md~RF1a2b.TMP") and puts the new one in its
            // place: not a deletion. It is still checked as a change (the text is compared, so nothing shows), so a
            // real change by another program in the same seconds is not missed.
            w.Renamed += (_, e) =>
            {
                bool away = !string.Equals(e.FullPath, path, StringComparison.OrdinalIgnoreCase);
                Changed(away && !(AtomicFile.IsTemporaryName(e.FullPath) && AtomicFile.JustReplaced(path)));
            };
            w.Deleted += (_, _) => Changed(true);
            tab.Watcher = w;
            w.EnableRaisingEvents = true;
        }
        catch { /* network folders without notifications: nothing to watch */ }
    }

    /// <summary>Deleted or renamed away: a save by replacement brings it back a moment later.</summary>
    private async void OnPossiblyGone(string path)
    {
        try
        {
            await Task.Delay(600);
            if (!string.Equals(path, _path, StringComparison.OrdinalIgnoreCase)) return;
            if (File.Exists(path)) OnExternalChange(path);
            else Flash(Strings.Get("DeletedOnDisk"));
        }
        catch (Exception ex) { LogError("OnPossiblyGone", ex); }
    }

    /// <summary>The shown tab's file changed on disk. The state of the handling is the tab's own: a change of another
    /// tab's file while this one is handled is no longer lost. If another tab is shown meanwhile, this one is checked
    /// again when it is shown back.</summary>
    private async void OnExternalChange(string path)
    {
        var t = _cur;
        if (!string.Equals(path, t.Path, StringComparison.OrdinalIgnoreCase)) return;
        if (t.ChangePending) { t.ChangeAgain = true; return; }   // a burst of writes: read once more at the end
        t.ChangePending = true;
        bool reloaded = false;
        try
        {
            do
            {
                t.ChangeAgain = false;
                await Task.Delay(250);   // let the other program finish writing
                if (!_tabs.Contains(t) || !File.Exists(path) || !string.Equals(path, t.Path, StringComparison.OrdinalIgnoreCase)) return;
                if (t != _cur) { t.ChangedOnDisk = true; return; }   // handled when its tab is shown again
                var now = PeekDocument(path);
                if (now == null) { t.ChangeAgain = true; await Task.Delay(250); continue; }   // still being written
                // Our own save (autosave writes often), or a touch without a change. Comparing the text
                // and not the time since the last save: another program may write a second after us.
                if (now == t.DiskText) continue;
                if (t.Dirty)
                {
                    // Keep my changes (Escape) · Reload (default), as on the Mac
                    int r = ChoiceDialog.Ask(this, string.Format(Strings.Get("ChangedOnDisk"), Path.GetFileName(path)),
                        new[] { Strings.Get("KeepMine"), Strings.Get("ReloadBtn") }, 0);
                    if (r != 1) { t.DiskText = now; return; }   // asked about this version; a newer one asks again
                    t.Dirty = false;
                }
                if (t != _cur) { t.ChangedOnDisk = true; return; }
                if (await ReloadFromDiskAsync(t, path)) reloaded = true;
                else { t.ChangeAgain = true; await Task.Delay(250); }
            } while (t.ChangeAgain);
            if (reloaded && t == _cur) Flash(Strings.Get("Reloaded"));
        }
        catch (Exception ex) { LogError("OnExternalChange", ex); }
        finally { t.ChangePending = false; t.ChangeAgain = false; }
    }

    /// <summary>The text on disk as ReadDocument gives it, without touching the document's state;
    /// null while another program holds the file or when it is gone.</summary>
    private static string? PeekDocument(string path)
    {
        try
        {
            var bytes = File.ReadAllBytes(path);
            bool bom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
            return new UTF8Encoding(false).GetString(bytes, bom ? 3 : 0, bytes.Length - (bom ? 3 : 0));
        }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
    }

    /// <summary>Reads the file again and swaps the text in place, keeping the scroll position of
    /// every view. False while the other program still holds the file (tried again).</summary>
    private async Task<bool> ReloadFromDiskAsync(DocTab t, string path)
    {
        using var busy = DocBusy();
        string text;
        try { text = ReadDocument(path, t); }
        catch (IOException) { return false; }
        catch (UnauthorizedAccessException) { return false; }
        t.DiskText = text;
        await JsIn(t, $"mdApp.reload({Q(text)})");
        t.Dirty = false;
        t.Words = int.TryParse(await JsIn(t, "mdApp.words()"), out var n) ? n : 0;
        if (t == _cur) { UpdateTitle(); UpdateStatus(); } else RefreshTabHeader(t);
        return true;
    }

    // ------------------------------------------------------------------ autosave
    // On by default: a document with a name is written 1.5 s after the typing stops. It never writes
    // over a change made by another program (the live reload asks first), never a new untitled
    // document or the sample, and never while saving or reloading.
    private bool CanAutoSave() =>
        _settings.AutoSave && _ready && _path != null && !_isSample && !_saving && !_cur.ChangePending
        && PeekDocument(_path) is { } now && now == _diskText;

    private void ScheduleAutoSave()
    {
        if (!_settings.AutoSave || _path == null) return;
        _autoSaveTimer.Stop(); _autoSaveTimer.Start();   // restarts with every edit
    }

    private async Task AutoSaveAsync()
    {
        _autoSaveTimer.Stop();
        if (!_dirty || _path == null) return;
        if (_saving || _cur.ChangePending || (File.Exists(_path) && PeekDocument(_path) == null)) { ScheduleAutoSave(); return; }   // busy: later
        if (CanAutoSave()) await SaveAsync(false, auto: true);
    }

    private async Task SetAutoSave(bool on)
    {
        _settings.AutoSave = on; _settings.Save();
        AutoSaveBtn.IsChecked = on;
        BuildMenu();
        Flash(Strings.Get(on ? "AutoSaveOn" : "AutoSaveOff"));
        if (on && _dirty) await AutoSaveAsync();
        else if (!on) _autoSaveTimer.Stop();
    }

    // ------------------------------------------------------------------ crash recovery
    // each tab's draft: its text, and a note of its file, this process and the time (read by OfferRecoveryAsync)
    private static string DraftPath(DocTab t) => Path.Combine(RecoveryDir, t.RecoveryId + ".md");
    private static string DraftMeta(DocTab t) => Path.Combine(RecoveryDir, t.RecoveryId + ".json");

    /// <summary>The draft of the shown tab, every 20 s while it has unsaved changes (and when another tab is shown).
    /// Each tab has its own draft.</summary>
    private async Task SaveRecoveryAsync()
    {
        if (!_dirty || !_ready) return;
        using var busy = DocBusy();
        await SaveRecoveryOfAsync(_cur);
    }

    /// <summary>The draft of one tab, shown or not.</summary>
    private static async Task SaveRecoveryOfAsync(DocTab t)
    {
        if (!t.Dirty || !t.Ready) return;
        try
        {
            Directory.CreateDirectory(RecoveryDir);
            var md = JsonSerializer.Deserialize<string>(await JsIn(t, "mdApp.getMarkdownPreserving()")) ?? "";
            AtomicFile.WriteAllText(DraftPath(t), md, new UTF8Encoding(false));
            AtomicFile.WriteAllText(DraftMeta(t),
                JsonSerializer.Serialize(new { path = t.Path, pid = Environment.ProcessId, time = DateTime.Now }), new UTF8Encoding(false));
        }
        catch { /* best effort */ }
    }

    /// <summary>After an unexpected error: the drafts of every tab with unsaved changes, so nothing typed is lost if the
    /// window does not survive it (App.xaml.cs).</summary>
    internal async Task SaveAllRecoveryAsync()
    {
        foreach (var t in _tabs.ToList()) await SaveRecoveryOfAsync(t);
    }

    /// <summary>An unexpected error, in error.log beside the settings (each profile its own), so it can be looked at
    /// afterwards. The log is started again past 1 MB.</summary>
    internal static void LogError(string where, Exception? ex)
    {
        try
        {
            Directory.CreateDirectory(Settings.Dir);
            var f = Path.Combine(Settings.Dir, "error.log");
            if (File.Exists(f) && new FileInfo(f).Length > 1_000_000) File.Delete(f);
            File.AppendAllText(f, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{where}] Vigía {Version}{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}");
        }
        catch { /* nowhere to write: nothing more to do */ }
    }

    private void ClearRecovery() => ClearRecoveryOf(_cur);

    private static void ClearRecoveryOf(DocTab t)
    {
        try { File.Delete(DraftPath(t)); File.Delete(DraftMeta(t)); } catch { }
    }

    private void ClearAllRecovery()
    {
        foreach (var t in _tabs) ClearRecoveryOf(t);
    }

    /// <summary>Called once the first page is ready: offer the drafts left by a Vigia that is no longer running,
    /// one by one (it had a draft per tab). A draft taken opens in a tab of its own, marked as modified.</summary>
    private async Task OfferRecoveryAsync()
    {
        if (!Directory.Exists(RecoveryDir)) return;
        foreach (var meta in Directory.GetFiles(RecoveryDir, "*.json"))
        {
            if (_tabs.Any(t => PathCmp.Equals(t.RecoveryId, Path.GetFileNameWithoutExtension(meta)))) continue;   // one of ours
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(meta));
                var r = doc.RootElement;
                int pid = r.GetProperty("pid").GetInt32();
                bool alive = false;
                try { alive = Process.GetProcessById(pid).ProcessName.Equals("Vigia", StringComparison.OrdinalIgnoreCase); } catch { }
                if (alive) continue;
                var draft = Path.ChangeExtension(meta, ".md");
                string? orig = r.GetProperty("path").GetString();
                var when = r.GetProperty("time").GetDateTime();
                var name = orig != null ? Path.GetFileName(orig) : Strings.Get("Untitled");
                var ans = MessageBox.Show(this, string.Format(Strings.Get("Recover"), name, when.ToString("g")), "Vigía",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ans == MessageBoxResult.Yes && File.Exists(draft))
                {
                    var text = File.ReadAllText(draft, Encoding.UTF8);
                    bool ok;
                    if (orig != null && File.Exists(orig)) ok = await OpenPathAsync(orig);   // its tab, or a new one
                    else ok = await TabForContentAsync();
                    if (ok)
                    {
                        await Js($"mdApp.load({Q(text)},{DocBaseJs(orig)})");
                        _path = orig; _dirty = true;
                        RefreshTabs(); UpdateTitle(); UpdateStatus(); Flash(Strings.Get("Recovered"));
                    }
                }
                File.Delete(meta); File.Delete(draft);
            }
            catch { try { File.Delete(meta); } catch { } }
        }
    }

    // ------------------------------------------------------------------ single instance
    /// <summary>A second Vigia passed us its files and folders (or just asked to be shown): a tab per file.</summary>
    public async void ActivateFromOtherInstance(IReadOnlyList<string> paths)
    {
        try
        {
            if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
            Activate(); Topmost = true; Topmost = false; Focus();
            foreach (var p in paths.Where(p => !string.IsNullOrEmpty(p))) await OpenPathAsync(p);
        }
        catch (Exception ex) { LogError("ActivateFromOtherInstance", ex); }
    }
}
