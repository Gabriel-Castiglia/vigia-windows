// Vigia -- main window: the web view, files and commands.
// Copyright (C) 2026 Gabriel Castiglia. Licensed under the GNU GPL, version 2 only.
// Date: November 10, 2026
// SPDX-License-Identifier: GPL-2.0-only

using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;

namespace Vigia;

public partial class MainWindow : Window
{
    private readonly Settings _settings = Settings.Load();
    private bool _closingConfirmed;
    // The shown document's path, state and editor (_path, _dirty, _ready, Web…) live in its tab: MainWindow.Tabs.cs.

    private static string AppDir => AppContext.BaseDirectory;
    private static string Version => typeof(MainWindow).Assembly.GetName().Version?.ToString(3) ?? "1.0.0";

    /// <summary>startPaths: what the command line named. A folder goes to the Files panel (the first one); each file
    /// gets a tab, the last one shown.</summary>
    public MainWindow(IReadOnlyList<string>? startPaths)
    {
        InitializeComponent();
        Strings.Lang = _settings.Lang;
        string? startFolder = startPaths?.FirstOrDefault(Directory.Exists);
        InitTabs(startPaths?.Where(p => !Directory.Exists(p)).ToList() ?? new List<string>());
        ApplySavedBounds();
        if (Application.Current != null)
        {
            Application.Current.SessionEnding += OnSessionEnding;
            Closed += (_, _) => Application.Current.SessionEnding -= OnSessionEnding;
        }
        if (!_settings.AccentChosen) _settings.Accent = Accents.Default;
        ThemeManager.Apply(this, IsDark, _settings.Accent);
        Icon = Accents.WindowIcon(_settings.Accent) ?? Icon;
        // (each page gets the page colour from its first frame, CreateView: no white/black flash while it loads)
        BuildMenu();
        UpdateTitle();
        SourceInitialized += (_, _) => ThemeManager.Apply(this, IsDark, _settings.Accent);
        Accents.ApplyToSystem(_settings.Accent);   // e.g. after an update reinstalled the default icon
        Microsoft.Win32.SystemEvents.UserPreferenceChanged += OnSystemPreference;
        Closed += (_, _) => Microsoft.Win32.SystemEvents.UserPreferenceChanged -= OnSystemPreference;
        Loaded += async (_, _) => await InitWebAsync(_cur);
        Closing += OnClosing;
        InitFeatures();
        if (_settings.Spell) SpellChecker.Warm(_settings.SpellLangs);
        InitUpdate();
        InitFit();
        InitFolder(startFolder);
        PreviewKeyDown += OnKey;
        Drop += async (_, e) =>
        {
            // several files dropped at once: a tab each
            if (e.Data.GetData(DataFormats.FileDrop) is string[] f)
                foreach (var p in f) { if (Directory.Exists(p)) OpenFolder(p); else await OpenPathAsync(p); }
        };
    }

    // ------------------------------------------------------------------ WebView
    // The browser of each interface language: its own menus (the page's right-click menu) speak that language. A
    // browser's language is fixed when it starts, and browsers with different languages cannot share a profile
    // folder, so each language has its own (the folder names are the ones used since 3.0, so profiles are kept).
    private static string BrowserLang(string lang) => lang switch { "es" => "es-419", "ja" => "ja", _ => "en-US" };

    // Spell check is Vigía's own (SpellChecker.cs), so the browser's checker is turned off in its profile before it
    // starts: it would otherwise load (and download) dictionaries nobody uses.
    private static string WebProfile(string lang)
    {
        var dir = Path.Combine(Settings.Dir, "WebView2", lang);
        try
        {
            var prefsFile = Path.Combine(dir, "EBWebView", "Default", "Preferences");
            Directory.CreateDirectory(Path.GetDirectoryName(prefsFile)!);
            var prefs = (File.Exists(prefsFile) ? System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(prefsFile)) as System.Text.Json.Nodes.JsonObject : null)
                        ?? new System.Text.Json.Nodes.JsonObject();
            prefs["browser"] ??= new System.Text.Json.Nodes.JsonObject();
            prefs["browser"]!["enable_spellchecking"] = false;
            prefs["spellcheck"] ??= new System.Text.Json.Nodes.JsonObject();
            prefs["spellcheck"]!["dictionaries"] = new System.Text.Json.Nodes.JsonArray();
            prefs["intl"] ??= new System.Text.Json.Nodes.JsonObject();
            prefs["intl"]!["selected_languages"] = lang;
            File.WriteAllText(prefsFile, prefs.ToJsonString());
        }
        catch { /* the browser then falls back to its own default */ }
        return dir;
    }

    /// <summary>Starts a tab's page. Works on that tab, not on the shown one: the user may switch meanwhile.</summary>
    private async Task InitWebAsync(DocTab t)
    {
        var view = t.View!;
        try
        {
            t.WebLang = BrowserLang(_settings.Lang);
            var env = await EnvironmentFor(t.WebLang);
            await view.EnsureCoreWebView2Async(env);
        }
        catch (Exception)
        {
            if (t == _spare) { DropSpare(); return; }   // only a page ahead: the next tab starts its own
            if (t.View != view) return;   // closed while starting
            MessageBox.Show(this, Strings.Get("NoWebView"), "Vigía", MessageBoxButton.OK, MessageBoxImage.Error);
            if (_tabs.Count == 1) Close();
            return;
        }
        if (t.View != view) return;
        var core = view.CoreWebView2;
        core.SetVirtualHostNameToFolderMapping("app.local", Path.Combine(AppDir, "wwwroot"), CoreWebView2HostResourceAccessKind.Allow);
        InitDocFiles(core);
        core.Settings.AreDevToolsEnabled = false;
        core.Settings.IsStatusBarEnabled = false;
        core.Settings.AreDefaultContextMenusEnabled = true;
        core.Settings.IsZoomControlEnabled = true;
        core.WebMessageReceived += OnWebMessage;
        core.NewWindowRequested += (_, e) => { e.Handled = true; OpenLink(e.Uri); };
        core.NavigationStarting += (_, e) =>
        {
            if (!e.Uri.StartsWith("https://app.local/", StringComparison.OrdinalIgnoreCase)) { e.Cancel = true; OpenLink(e.Uri); }
        };
        view.ZoomFactor = _settings.Zoom;
        core.Navigate("https://app.local/index.html");
        t.Started = true;
    }

    private async Task<string> Js(string script) => await Web.CoreWebView2.ExecuteScriptAsync(script);

    /// <summary>A script in a given tab's page, which may no longer be the one shown when the call returns.</summary>
    private static async Task<string> JsIn(DocTab t, string script) =>
        t.View?.CoreWebView2 is { } core ? await core.ExecuteScriptAsync(script) : "null";
    private static string Q(string s) => JsonSerializer.Serialize(s);

    /// <summary>A message from a page. Only the shown tab's page acts at once; a hidden one's message waits until
    /// its tab is shown (MainWindow.Tabs.cs ActivateAsync).</summary>
    private async void OnWebMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            var tab = TabOf(sender);
            if (tab == null) return;
            string json = e.WebMessageAsJson;
            if (HandleSpellMessage(tab, json)) return;
            if (tab != _cur) { if (json != "\"drop\"") tab.Held.Add(json); return; }
            await HandleWebMessage(json, e);
        }
        catch (Exception ex) { LogError("OnWebMessage", ex); }
    }

    private async Task HandleWebMessage(string json, CoreWebView2WebMessageReceivedEventArgs? e)
    {
        if (json == "\"drop\"")
        {
            // files dropped on the page: a tab each
            foreach (var f in e?.AdditionalObjects?.OfType<CoreWebView2File>() ?? Enumerable.Empty<CoreWebView2File>()) await OpenPathAsync(f.Path);
            return;
        }
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object) return;
        switch (root.GetProperty("type").GetString())
        {
            case "ready":
                using (DocBusy())
                {
                    await Js($"mdApp.init({Q(_settings.Lang)},{Q(IsDark ? "dark" : "light")},{Q(_settings.Mode)},{(_settings.Spell ? "true" : "false")},{(_settings.ReadOnly ? "true" : "false")},true)");
                    await Js($"mdApp.setWidth({_settings.TextWidth})");
                    PageInitialised(_cur);
                    _ready = true;
                    _lastEditSeq = 0;   // a new page counts its edits from zero
                    if (_restore is { } r) { _restore = null; await RestoreDocAsync(r.Text, r.Dirty); }
                    else if (_pendingOpen != null) { var p = _pendingOpen; _pendingOpen = null; await LoadFileAsync(p); }
                    else if (!_settings.SampleShown && SamplePath != null) { _settings.SampleShown = true; _settings.Save(); await LoadSampleAsync(); }
                    else UpdateStatus();
                }
                RefreshTabs();
                PrepareSpare();   // the page the next tab will take (MainWindow.Tabs.cs)
                if (SelfTestLog != null) { if (!_selfTestStarted) { _selfTestStarted = true; _ = Dispatcher.BeginInvoke(async () => await RunSelfTestAsync()); } break; }
                if (!_recoveryOffered) { _recoveryOffered = true; await OfferRecoveryAsync(); }
                break;
            case "dirty":
                if (root.TryGetProperty("words", out var w)) _words = w.GetInt32();
                if (root.TryGetProperty("seq", out var sq)) _lastEditSeq = Math.Max(_lastEditSeq, sq.GetInt64());
                if (!_dirty) { _dirty = true; UpdateTitle(); }
                if (_cur.Preview) KeepTab(_cur);   // typed in: no longer a passing tab
                UpdateStatus();
                ScheduleAutoSave();
                break;
            case "lockedHint":
                Flash(Strings.Get("LockedHint"));
                break;
            case "cmd":
                await RunCommand(root.GetProperty("cmd").GetString() ?? "");
                break;
            case "find":
                UpdateFindCount(root.GetProperty("index").GetInt32(), root.GetProperty("total").GetInt32());
                break;
            case "outline":
                UpdateOutline(root.GetProperty("items"));
                break;
            case "export":
                WriteExport(root);
                break;
            case "printReady":
                Web.CoreWebView2.ShowPrintUI(CoreWebView2PrintDialogKind.Browser);
                break;
            case "link":
                OpenLink(root.GetProperty("href").GetString() ?? "");
                break;
            case "wikilink":
                OpenWiki(root.GetProperty("name").GetString() ?? "");
                break;
            case "findfile":
                await FindFileAsync(root.GetProperty("name").GetString() ?? "");
                break;
            case "wikinames":
                var names = root.GetProperty("names").EnumerateArray().Select(n => n.GetString() ?? "").ToList();
                string? docDir = _path != null ? Path.GetDirectoryName(_path) : null;
                var panel = _roots.ToList();
                var found = await Task.Run(() => Wiki.Exist(names, docDir, panel));   // a disk walk: off the window's thread
                if (_ready) await Js($"mdApp.wikiState({JsonSerializer.Serialize(found)})");
                break;
        }
    }

    private void OpenLink(string uri)
    {
        if (string.IsNullOrEmpty(uri)) return;
        try
        {
            if (uri.StartsWith(DocHost, StringComparison.OrdinalIgnoreCase))
            {
                var local = DocPath(uri);
                if (local == null) return;
                // A document must never be able to run a program: a Markdown file it may reach opens here, a
                // picture opens in its viewer (from anywhere, as the page may show it), a PDF it may reach too;
                // any other file it may reach (.exe, .bat, .lnk...) is only shown in Explorer; the rest is ignored.
                if (IsPicture(local) && File.Exists(local)) { Process.Start(new ProcessStartInfo(local) { UseShellExecute = true }); return; }
                if (!InReach(local)) return;
                if (Directory.Exists(local)) { Process.Start("explorer.exe", $"\"{local}\""); return; }
                if (!File.Exists(local)) return;
                if (IsMarkdown(local)) { OpenPath(local); return; }
                if (IsViewable(local)) Process.Start(new ProcessStartInfo(local) { UseShellExecute = true });
                else Process.Start("explorer.exe", $"/select,\"{local}\"");
                return;
            }
            if (Uri.TryCreate(uri, UriKind.Absolute, out var u) && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps || u.Scheme == Uri.UriSchemeMailto))
                Process.Start(new ProcessStartInfo(u.AbsoluteUri) { UseShellExecute = true });
        }
        catch { /* nothing sensible to do with a broken link */ }
    }

    /// <summary>A [[name]] link of the GitHub view: the file of that name in the open folders (Wiki.cs).</summary>
    private void OpenWiki(string name)
    {
        Wiki.Forget();
        var f = Wiki.Resolve(name, _path != null ? Path.GetDirectoryName(_path) : null, _roots);
        if (f != null) OpenPath(f);
        else Flash(string.Format(Strings.Get("WikiMissing"), Wiki.Key(name)));
    }

    private static bool IsViewable(string p) =>
        new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".bmp", ".pdf" }.Contains(Path.GetExtension(p).ToLowerInvariant());

    private static bool IsMarkdown(string p) =>
        new[] { ".md", ".markdown", ".mdown", ".mkd", ".txt" }.Contains(Path.GetExtension(p).ToLowerInvariant());

    // ------------------------------------------------------------------ Files
    // (opening a file picks or makes its tab: OpenPath / OpenPathAsync in MainWindow.Tabs.cs)

    /// <summary>The file as text; remembers its BOM and line endings so a save writes them back.
    /// Shared read access: another program may still have it open for writing.</summary>
    private string ReadDocument(string path, DocTab? tab = null)
    {
        var t = tab ?? _cur;   // the tab whose BOM and line endings are noted
        byte[] bytes;
        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
        using (var ms = new MemoryStream())
        {
            fs.CopyTo(ms);
            bytes = ms.ToArray();
        }
        t.Bom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
        string text = new UTF8Encoding(false).GetString(bytes, t.Bom ? 3 : 0, bytes.Length - (t.Bom ? 3 : 0));
        t.Crlf = text.Contains("\r\n");
        return text;
    }

    /// <summary>Loads a file into the shown tab, in place of what it had.</summary>
    private async Task LoadFileAsync(string path)
    {
        using var busy = DocBusy();
        try
        {
            path = Path.GetFullPath(path);
            if (!File.Exists(path))
            {
                MessageBox.Show(this, string.Format(Strings.Get("NotFound"), path), "Vigía", MessageBoxButton.OK, MessageBoxImage.Warning);
                _settings.Recent.Remove(path); BuildMenu();
                return;
            }
            string text = ReadDocument(path);
            var dir = Path.GetDirectoryName(path)!;
            await Js($"mdApp.load({Q(text)},{DocBaseJs(path)})");
            _path = path; _dirty = false; _isSample = false; _diskText = text;
            Watch(path);
            _words = int.TryParse(await Js("mdApp.words()"), out var n) ? n : 0;
            _settings.AddRecent(path); _settings.Save();
            BuildMenu(); UpdateTitle(); UpdateStatus();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, string.Format(Strings.Get("ErrOpen"), ex.Message), "Vigía", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>Writes the document. auto: an autosave, which reports a failure in the status bar
    /// instead of a dialog.</summary>
    private async Task<bool> SaveAsync(bool forceDialog, bool auto = false)
    {
        // Everything below works on the tab that was shown when the save started: a save that outlasted the wait of
        // a tab switch used to finish on the tab shown by then, which took the path of the first file, and the next
        // autosave wrote the second document over the first file.
        var t = _cur;
        var target = t.Path;
        if (forceDialog || target == null)
        {
            var dlg = new SaveFileDialog
            {
                Filter = Strings.Get("MdFilter"),
                DefaultExt = ".md",
                FileName = target != null ? Path.GetFileName(target) : Strings.Get("Untitled") + ".md",
                InitialDirectory = target != null ? Path.GetDirectoryName(target) : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            };
            if (dlg.ShowDialog(this) != true) return false;
            target = dlg.FileName;
            if (t != _cur || !_tabs.Contains(t)) return false;   // the tab changed while the dialog was open
        }
        if (!t.Ready || t.View?.CoreWebView2 == null) return false;
        using var busy = DocBusy();
        _saving = true; t.Saving++;
        try
        {
            // untouched blocks keep their original Markdown; line endings and BOM as the file had them.
            // The text is taken and marked as saved in one call; seq is the last edit it holds.
            using var snap = JsonDocument.Parse(await JsIn(t, "mdApp.takeForSave()"));
            var md = snap.RootElement.GetProperty("text").GetString() ?? "";
            long seq = snap.RootElement.GetProperty("seq").GetInt64();
            var disk = t.Crlf ? md.Replace("\r\n", "\n").Replace("\n", "\r\n") : md;
            AtomicFile.WriteAllText(target!, disk, new UTF8Encoding(t.Bom));
            t.DiskText = disk;   // the watcher sees this write; the same text on disk is not a change
            bool moved = !string.Equals(target, t.Path, StringComparison.OrdinalIgnoreCase);
            t.Path = target;
            t.Dirty = t.LastEditSeq > seq;   // typed while saving: still modified, and saved again shortly
            if (t.Dirty && t == _cur) ScheduleAutoSave();
            WatchTab(t, target!); ClearRecoveryOf(t);
            if (moved) await JsIn(t, $"mdApp.setBase({DocBaseJs(target)})");
            bool menu = moved;
            if (_settings.AddRecent(target!)) { _settings.Save(); menu = true; }   // the same first file: nothing to write
            if (t == _cur) { if (menu) BuildMenu(); UpdateTitle(); UpdateStatus(); }
            else RefreshTabHeader(t);
            return true;
        }
        catch (Exception ex)
        {
            if (auto) Flash(string.Format(Strings.Get("ErrAutoSave"), ex.Message));
            else MessageBox.Show(this, string.Format(Strings.Get("ErrSave"), ex.Message), "Vigía", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
        finally { _saving = false; t.Saving--; }
    }

    private async Task<bool> ConfirmDiscardAsync()
    {
        if (!_dirty) return true;
        // With autosave on, a file with a name is saved without asking, unless another program
        // changed it meanwhile: then the question below decides.
        if (CanAutoSave() && await SaveAsync(false, auto: true)) return true;
        var name = _path != null ? Path.GetFileName(_path) : Strings.Get("Untitled");
        // Don't save · Cancel · Save (default), as on the Mac
        int r = ChoiceDialog.Ask(this, string.Format(Strings.Get("AskSave"), name), new[] { Strings.Get("DontSave"), Strings.Get("Cancel"), Strings.Get("SaveBtn") }, 1);
        if (r == 1) return false;
        if (r == 2) return await SaveAsync(false);
        return true;
    }

    // ------------------------------------------------------------------ Window size and position
    // The next start opens where the window was closed: same size, same place, maximized or not.
    // A position that no longer falls on a screen (a monitor was unplugged) is dropped: centred instead.
    private void ApplySavedBounds()
    {
        Width = Math.Max(MinWidth, _settings.Width);
        Height = Math.Max(MinHeight, _settings.Height);
        if (_settings.Left is double l && _settings.Top is double t && IsOnScreen(l, t, Width))
        {
            if (Settings.IsSecondary) { l += 32; t += 32; }   // an extra window, not right on top of the first
            if (!IsOnScreen(l, t, Width)) { l -= 32; t -= 32; }
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = l; Top = t;
        }
        if (_settings.Maximized) WindowState = WindowState.Maximized;   // on the monitor of the saved place
    }

    /// <summary>True when the title bar would be on the desktop, so the window can be grabbed.</summary>
    private static bool IsOnScreen(double left, double top, double width)
    {
        double vl = SystemParameters.VirtualScreenLeft, vt = SystemParameters.VirtualScreenTop;
        double vr = vl + SystemParameters.VirtualScreenWidth, vb = vt + SystemParameters.VirtualScreenHeight;
        return left + width - 120 >= vl && left + 120 <= vr && top >= vt - 8 && top + 40 <= vb;
    }

    private void SaveBounds()
    {
        // Maximized or minimized: RestoreBounds is the normal size and place it goes back to
        var b = WindowState == WindowState.Normal ? new Rect(Left, Top, ActualWidth, ActualHeight) : RestoreBounds;
        if (WindowState != WindowState.Minimized) _settings.Maximized = WindowState == WindowState.Maximized;
        if (b.IsEmpty || double.IsNaN(b.Left) || b.Width < MinWidth / 2 || !IsOnScreen(b.Left, b.Top, b.Width)) return;
        _settings.Width = b.Width; _settings.Height = b.Height;
        _settings.Left = b.Left; _settings.Top = b.Top;
    }

    // Logging off or shutting down with the window open: keep its place too
    private void OnSessionEnding(object? sender, SessionEndingCancelEventArgs e)
    {
        SaveBounds();
        if (_ready) _settings.Zoom = Web.ZoomFactor;
        SaveTabSession();
        _settings.Save();
    }

    /// <summary>Closing the window closes every tab: each one with unsaved changes asks first (Cancel keeps the
    /// window open). With File → Reopen tabs at start on, their files open again at the next start.</summary>
    private async void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        SaveBounds();
        if (_ready) _settings.Zoom = Web.ZoomFactor;
        if (!_closingConfirmed) SaveTabSession();   // the tabs as they are, before any of them is asked about
        _settings.Save();
        if (_closingConfirmed || !_tabs.Any(t => t.Dirty)) { Hide(); ClearAllRecovery(); StopFeatures(); return; }
        e.Cancel = true;
        _windowClosing = true;
        bool go = await ConfirmDiscardAllAsync();
        _windowClosing = false;
        // Close() again from inside this Closing handler throws ("cannot call Close while closing") and ended the
        // program at once (8 crashes of Gryphmark 1.7-1.9 in the event log): asked for once this handler has returned
        if (go) { _closingConfirmed = true; ClearAllRecovery(); StopFeatures(); _ = Dispatcher.BeginInvoke(Close); }
    }

    // ------------------------------------------------------------------ Sample document
    // Installed hidden in {app}\samples (one per language). It opens as an untitled copy, so the
    // installed file is never overwritten; Save asks for a new name. (_isSample: the shown tab holds it.)

    private static string? SamplePath
    {
        get
        {
            foreach (var lang in new[] { Strings.Lang, "en" })
            {
                var p = Path.Combine(AppDir, "samples", $"sample_{lang}.md");
                if (File.Exists(p)) return p;
            }
            return null;
        }
    }

    /// <summary>Help → Sample document: in its own tab (or the shown one, when nobody typed in it).</summary>
    private async Task OpenSampleAsync()
    {
        if (_tabs.FirstOrDefault(t => t.IsSample && t.Path == null) is { } open) { await ActivateAsync(open); return; }
        if (!await TabForContentAsync()) return;
        await LoadSampleAsync();
    }

    private async Task LoadSampleAsync()
    {
        var p = SamplePath;
        if (p == null) return;
        using var busy = DocBusy();
        try
        {
            var text = File.ReadAllText(p, Encoding.UTF8);
            await Js($"mdApp.load({Q(text)}, null)");
            _path = null; _dirty = false; _crlf = false; _bom = false; _isSample = true;
            Watch(null); ClearRecovery();
            _words = int.TryParse(await Js("mdApp.words()"), out var n) ? n : 0;
            UpdateTitle(); UpdateStatus();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, string.Format(Strings.Get("ErrOpen"), ex.Message), "Vigía", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>File → New tab (Ctrl+N): an untitled document in a new tab next to the shown one. When the shown
    /// tab is already an empty untitled document, that one is used.</summary>
    private async Task NewDocAsync()
    {
        if (IsPristine(_cur) && !_cur.IsSample) { if (_ready) await Js("mdApp.focus()"); return; }
        var t = AddTab(null, _tabs.IndexOf(_cur) + 1);
        await ActivateAsync(t);
        await WaitLoadedAsync(t);
    }

    /// <summary>The shown tab becomes an empty untitled document (closing the last tab).</summary>
    private async Task ClearDocumentAsync()
    {
        if (!_ready) { _pendingOpen = null; _restore = null; }
        else await Js("mdApp.load('', null)");
        _path = null; _dirty = false; _words = 0; _crlf = false; _bom = false; _isSample = false;
        Watch(null); ClearRecovery();
        RefreshTabs(); BuildMenu(); UpdateTitle(); UpdateStatus();
    }

    private async Task ExportHtmlAsync()
    {
        var dlg = new SaveFileDialog
        {
            Filter = Strings.Get("HtmlFilter"), DefaultExt = ".html",
            FileName = Path.GetFileNameWithoutExtension(_path ?? Strings.Get("Untitled")) + ".html",
            InitialDirectory = _path != null ? Path.GetDirectoryName(_path) : null,
        };
        if (dlg.ShowDialog(this) != true) return;
        _pendingExport = dlg.FileName;
        // the page renders (diagrams included) and answers with an "export" message
        await Js($"mdApp.exportHtml({Q(Path.GetFileNameWithoutExtension(dlg.FileName))})");
    }

    private string? _pendingExport;

    private void WriteExport(JsonElement r)
    {
        if (_pendingExport == null) return;
        var file = _pendingExport; _pendingExport = null;
        var title = r.GetProperty("title").GetString() ?? "";
        bool dark = r.GetProperty("theme").GetString() == "dark";
        bool math = r.TryGetProperty("math", out var mv) && mv.GetBoolean();
        string css = File.ReadAllText(Path.Combine(AppDir, "wwwroot", "lib", dark ? "github-markdown-dark.css" : "github-markdown-light.css"));
        string hl = File.ReadAllText(Path.Combine(AppDir, "wwwroot", "lib", dark ? "hljs-dark.css" : "hljs-light.css"));
        string bg = dark ? "#0d1117" : "#ffffff";
        string body = r.GetProperty("body").GetString() ?? "";
        string html = $$"""
            <!doctype html>
            <html lang="{{Strings.Lang}}">
            <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <meta name="generator" content="Vigía {{Version}}">
            <title>{{System.Net.WebUtility.HtmlEncode(title)}}</title>
            {{(math ? "<link rel=\"stylesheet\" href=\"https://cdn.jsdelivr.net/npm/katex@0.19.1/dist/katex.min.css\">" : "")}}
            <style>
            {{css}}
            {{hl}}
            body { margin: 0; background: {{bg}}; }
            .markdown-body { box-sizing: border-box; max-width: 980px; margin: 0 auto; padding: 45px; }
            @media (max-width: 767px) { .markdown-body { padding: 15px; } }
            </style>
            </head>
            <body>
            <article class="markdown-body">
            {{body}}
            </article>
            </body>
            </html>
            """;
        try { File.WriteAllText(file, html, new UTF8Encoding(false)); }
        catch (Exception ex) { MessageBox.Show(this, string.Format(Strings.Get("ErrSave"), ex.Message), "Vigía", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    // ------------------------------------------------------------------ Commands
    private async Task RunCommand(string cmd)
    {
        // tab commands work while the shown tab's page is still starting
        if (!_ready && cmd is not ("exit" or "close" or "nexttab" or "prevtab" or "reopentab" or "new")) return;
        switch (cmd)
        {
            case "new": await NewDocAsync(); break;
            case "newwindow": Process.Start(new ProcessStartInfo(Environment.ProcessPath!, "--new-window") { UseShellExecute = true }); break;
            case "open":
                var dlg = new OpenFileDialog
                {
                    Filter = Strings.Get("MdFilter"),
                    InitialDirectory = _path != null ? Path.GetDirectoryName(_path) : null,
                    Multiselect = true,   // several files: a tab each
                };
                if (dlg.ShowDialog(this) == true) foreach (var f in dlg.FileNames) await OpenPathAsync(f);
                break;
            case "save": await SaveAsync(false); break;
            case "saveas": await SaveAsync(true); break;
            case "export": await ExportHtmlAsync(); break;
            case "print": await Js("mdApp.preparePrint()"); break;   // continues on "printReady"
            case "find": ShowFind(false); break;
            case "replace": ShowFind(true); break;
            case "findnext": await FindStep(1); break;
            case "findprev": await FindStep(-1); break;
            case "findclose": await HideFind(); break;
            case "outline": ToggleOutline(); break;
            case "files": ToggleFiles(); break;
            case "search": ShowSearch(); break;
            case "readonly": await SetReadOnly(!_settings.ReadOnly); break;
            case "opendir": PickFolder(); break;
            case "manual": OpenManual(); break;
            case "folder":
                if (_path != null) Process.Start("explorer.exe", $"/select,\"{_path}\"");
                break;
            case "close": await CloseTabAsync(_cur); break;
            case "nexttab": await NextTabAsync(1); break;
            case "prevtab": await NextTabAsync(-1); break;
            case "reopentab": await ReopenClosedTabAsync(); break;
            case "exit": Close(); break;
            case "mode1": await SetMode("visual"); break;
            case "mode2": await SetMode("source"); break;
            case "mode3": await SetMode("preview"); break;
            case "zoom+": Web.ZoomFactor = Math.Min(Web.ZoomFactor + 0.1, 3); break;
            case "zoom-": Web.ZoomFactor = Math.Max(Web.ZoomFactor - 0.1, 0.5); break;
            case "zoom0": Web.ZoomFactor = 1; break;
        }
    }

    // Mode, lock, width, spell check, theme and language apply to every tab: the shown page gets them now
    // (SyncPageAsync), each hidden one when it is shown again.
    private async Task SetMode(string mode)
    {
        _settings.Mode = mode; _settings.Save();
        SyncSegments();
        await SyncPageAsync();
        BuildMenu(); UpdateStatus();
    }

    // ------------------------------------------------------------------ theme (View → Theme…)
    private bool IsDark => _settings.Theme == "dark" || (_settings.Theme == "auto" && ThemeManager.SystemDark());
    internal string AccentKey => _settings.Accent;

    private async Task ApplyThemeAsync()
    {
        bool dark = IsDark;
        ThemeManager.Apply(this, dark, _settings.Accent);
        Icon = Accents.WindowIcon(_settings.Accent) ?? Icon;
        foreach (var t in _tabs)
            if (t.View != null) t.View.DefaultBackgroundColor = dark ? System.Drawing.Color.FromArgb(13, 17, 23) : System.Drawing.Color.White;
        await SyncPageAsync();
        if (_folderInit) RefreshTree();   // folder colours follow the theme
    }

    private void ShowThemeDialog()
    {
        var dlg = new ThemeDialog(this, _settings.Theme, _settings.Accent,
            async mode => { _settings.Theme = mode; _settings.Save(); await ApplyThemeAsync(); },
            async key => { _settings.Accent = key; _settings.AccentChosen = true; _settings.Save(); await ApplyThemeAsync(); Accents.ApplyToSystem(key); });
        dlg.ShowDialog();
    }

    private void OnSystemPreference(object? sender, Microsoft.Win32.UserPreferenceChangedEventArgs e)
    {
        if (_settings.Theme == "auto") Dispatcher.BeginInvoke(async () => await ApplyThemeAsync());
    }

    private async Task SetReadOnly(bool on)
    {
        _settings.ReadOnly = on; _settings.Save();
        SyncSegments();
        await SyncPageAsync();
        BuildMenu(); UpdateStatus();
        if (_ready && !on) await Js("mdApp.focus()");
    }

    private async Task Exec(string cmd, string payload = "undefined")
    {
        if (_ready) await Js($"mdApp.exec({Q(cmd)},{payload})");
    }

    private async void OnKey(object sender, KeyEventArgs e)
    {
        try
        {
            // Shortcuts when the focus is on the menu or the window frame (the page handles its own)
            if (Keyboard.Modifiers != ModifierKeys.Control && Keyboard.Modifiers != (ModifierKeys.Control | ModifierKeys.Shift)) return;
            if (Web.IsKeyboardFocusWithin) return;
            string? cmd = e.Key switch
            {
                Key.Tab => Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? "prevtab" : "nexttab",
                Key.PageDown => "nexttab", Key.PageUp => "prevtab",
                Key.T when Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) => "reopentab",
                Key.N => "new", Key.O => "open", Key.P => "print", Key.W => "close",
                Key.S => Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? "saveas" : "save",
                Key.D1 => "mode1", Key.D2 => "mode2", Key.D3 => "mode3",
                Key.E when Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) => "files",
                Key.F when Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) => "search",
                Key.L when Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) => "readonly",
                _ => null,
            };
            if (cmd != null) { e.Handled = true; await RunCommand(cmd); }
        }
        catch (Exception ex) { LogError("OnKey", ex); }
    }

    // ------------------------------------------------------------------ Menu
    // Segoe Fluent Icons glyph per command (empty = no icon)
    private static readonly Dictionary<string, string> Glyphs = new()
    {
        ["New"] = "", ["NewWindow"] = "", ["Open"] = "", ["Recent"] = "",
        ["Save"] = "", ["SaveAs"] = "", ["ExportHtml"] = "", ["Print"] = "",
        ["OpenFolder"] = "", ["Exit"] = "", ["Undo"] = "", ["Redo"] = "",
        ["Bold"] = "", ["Italic"] = "", ["Strike"] = "", ["Code"] = "",
        ["Heading"] = "", ["Quote"] = "", ["Ul"] = "", ["Ol"] = "",
        ["Task"] = "", ["CodeBlock"] = "", ["Hr"] = "",
        ["ModeVisual"] = "", ["ModeSource"] = "", ["ModePreview"] = "",
        ["Dark"] = "", ["ZoomIn"] = "", ["ZoomOut"] = "", ["ZoomReset"] = "",
        ["AutoSave"] = "\uE895", ["FavCurrent"] = "\uE734", ["SearchPanelMenu"] = "\uE721", ["RecentPanelMenu"] = "\uE81C",
        ["OpenDir"] = "\uE838", ["Files"] = "\uE8B7", ["CloseDir"] = "\uE711", ["Sample"] = "\uE736", ["Manual"] = "", ["Guide"] = "", ["About"] = "",
        ["Find"] = "", ["Replace"] = "", ["FindNext"] = "", ["Outline"] = "", ["CheckUpdates"] = "", ["RecentClear"] = "",
    };

    private static object? Glyph(string key) => Glyphs.TryGetValue(key, out var g)
        ? new TextBlock { Text = g, FontFamily = (FontFamily)Application.Current.Resources["Gm.Icons"], FontSize = 14 }
        : null;

    private void SetSampleHidden(bool hidden)
    {
        _settings.SampleHidden = hidden;
        _settings.Save();
        BuildMenu();
    }

    private MenuItem Item(string key, string? gesture, Func<Task> action, bool? check = null, bool enabled = true)
    {
        var mi = new MenuItem { Header = Strings.Get(key), InputGestureText = gesture ?? "", IsEnabled = enabled, Icon = Glyph(key) };
        if (check.HasValue) { mi.IsCheckable = true; mi.IsChecked = check.Value; }
        mi.Click += async (_, _) => await action();
        return mi;
    }

    private void BuildMenu()
    {
        string ctrl = "Ctrl+";
        MainMenu.Items.Clear();

        var file = new MenuItem { Header = Strings.Get("File") };
        file.Items.Add(Item("New", ctrl + "N", () => RunCommand("new")));
        file.Items.Add(Item("NewWindow", null, () => RunCommand("newwindow")));
        file.Items.Add(Item("TabClose", ctrl + "W", () => RunCommand("close")));
        file.Items.Add(Item("TabReopen", ctrl + "Shift+T", () => RunCommand("reopentab"), enabled: _closedTabs.Count > 0));
        file.Items.Add(Item("Open", ctrl + "O", () => RunCommand("open")));
        file.Items.Add(Item("OpenDir", null, () => RunCommand("opendir")));
        var recent = new MenuItem { Header = Strings.Get("Recent"), Icon = Glyph("Recent") };
        if (SamplePath != null && !_settings.SampleHidden)
        {
            var smi = new MenuItem { Header = Strings.Get("SampleItem"), Icon = Glyph("Sample"), ToolTip = Strings.Get("SampleTip") };
            smi.Click += async (_, _) => await OpenSampleAsync();
            var hide = new MenuItem { Header = Strings.Get("SampleHide") };
            hide.Click += (_, _) => SetSampleHidden(true);
            smi.ContextMenu = new ContextMenu { Items = { hide } };
            recent.Items.Add(smi);
            var hideVisible = new MenuItem { Header = Strings.Get("SampleHide"), Icon = Glyph("CloseDir") };
            hideVisible.Click += (_, _) => SetSampleHidden(true);
            recent.Items.Add(hideVisible);
            recent.Items.Add(new Separator());
        }
        if (_settings.Recent.Count == 0 && _settings.RecentFolders.Count == 0) recent.Items.Add(new MenuItem { Header = Strings.Get("RecentEmpty"), IsEnabled = false });
        else
        {
            int i = 1;
            foreach (var p in _settings.Recent.ToList())
            {
                var mi = new MenuItem { Header = $"_{(i <= 9 ? i.ToString() : "")}  {Path.GetFileName(p).Replace("_", "__")}", InputGestureText = ShortDir(p), ToolTip = p };
                var path = p;
                mi.Click += (_, _) => OpenPath(path);
                recent.Items.Add(mi); i++;
            }
        }
        if (_settings.RecentFolders.Count > 0)
        {
            recent.Items.Add(new Separator());
            recent.Items.Add(new MenuItem { Header = Strings.Get("RecentFolders"), IsEnabled = false, FontSize = 11 });
            foreach (var d in _settings.RecentFolders.ToList())
            {
                var mi = new MenuItem { Header = FolderLabel(d).Replace("_", "__"), InputGestureText = ShortDir(d), ToolTip = d, Icon = Glyph("Files") };
                var dir = d;
                mi.Click += (_, _) => OpenFolder(dir);
                recent.Items.Add(mi);
            }
        }
        if (_settings.Recent.Count > 0 || _settings.RecentFolders.Count > 0)
        {
            recent.Items.Add(new Separator());
            recent.Items.Add(Item("RecentClear", null, () => { _settings.Recent.Clear(); _settings.RecentFolders.Clear(); _settings.Save(); BuildMenu(); return Task.CompletedTask; }));
        }
        file.Items.Add(recent);
        file.Items.Add(Item("CloseDir", null, () => { CloseAllFolders(); return Task.CompletedTask; }, enabled: _roots.Count > 0));
        file.Items.Add(Item("FavCurrent", null, () => { if (_path != null) ToggleFavorite(_path); return Task.CompletedTask; },
            _path != null && IsFavorite(_path), enabled: _path != null));
        var startup = new MenuItem { Header = Strings.Get("StartupMenu") };
        foreach (var mi in StartupItems()) startup.Items.Add(mi);
        file.Items.Add(startup);
        var restore = Item("RestoreTabs", null, () => { ToggleRestoreTabs(); return Task.CompletedTask; }, _settings.RestoreTabs);
        restore.ToolTip = Strings.Get("RestoreTabsTip");
        file.Items.Add(restore);
        var preview = Item("PreviewTab", null, () => { TogglePreviewTab(); return Task.CompletedTask; }, _settings.PreviewTab);
        preview.ToolTip = Strings.Get("PreviewTabTip");
        file.Items.Add(preview);
        file.Items.Add(new Separator());
        file.Items.Add(Item("Save", ctrl + "S", () => RunCommand("save")));
        file.Items.Add(Item("SaveAs", ctrl + "Shift+S", () => RunCommand("saveas")));
        file.Items.Add(Item("AutoSave", null, () => SetAutoSave(!_settings.AutoSave), _settings.AutoSave));
        file.Items.Add(new Separator());
        file.Items.Add(Item("ExportHtml", null, () => RunCommand("export")));
        file.Items.Add(Item("Print", ctrl + "P", () => RunCommand("print")));
        file.Items.Add(Item("OpenFolder", null, () => RunCommand("folder"), enabled: _path != null));
        file.Items.Add(new Separator());
        file.Items.Add(Item("Exit", "Alt+F4", () => RunCommand("exit")));
        MainMenu.Items.Add(file);

        var edit = new MenuItem { Header = Strings.Get("Edit") };
        edit.Items.Add(Item("Undo", ctrl + "Z", () => Exec("undo")));
        edit.Items.Add(Item("Redo", ctrl + "Y", () => Exec("redo")));
        edit.Items.Add(new Separator());
        edit.Items.Add(Item("Find", ctrl + "F", () => RunCommand("find")));
        edit.Items.Add(Item("Replace", ctrl + "H", () => RunCommand("replace")));
        edit.Items.Add(Item("FindNext", "F3", () => RunCommand("findnext")));
        MainMenu.Items.Add(edit);

        var fmt = new MenuItem { Header = Strings.Get("Format") };
        fmt.Items.Add(Item("Bold", ctrl + "B", () => Exec("bold")));
        fmt.Items.Add(Item("Italic", ctrl + "I", () => Exec("italic")));
        fmt.Items.Add(Item("Strike", null, () => Exec("strike")));
        fmt.Items.Add(Item("Code", null, () => Exec("code")));
        fmt.Items.Add(new Separator());
        var heading = new MenuItem { Header = Strings.Get("Heading"), Icon = Glyph("Heading") };
        for (int lv = 1; lv <= 6; lv++)
        {
            int level = lv;
            var mi = new MenuItem { Header = $"H{level}", FontSize = 13 + (6 - level) * 1.5, FontWeight = FontWeights.SemiBold };
            mi.Click += async (_, _) => await Exec("heading", $"{{level:{level}}}");
            heading.Items.Add(mi);
        }
        heading.Items.Add(new Separator());
        heading.Items.Add(Item("Paragraph", null, () => Exec("heading", "{level:0}")));
        fmt.Items.Add(heading);
        fmt.Items.Add(Item("Quote", null, () => Exec("blockQuote")));
        fmt.Items.Add(Item("Ul", null, () => Exec("bulletList")));
        fmt.Items.Add(Item("Ol", null, () => Exec("orderedList")));
        fmt.Items.Add(Item("Task", null, () => Exec("taskList")));
        fmt.Items.Add(Item("CodeBlock", null, () => Exec("codeBlock")));
        fmt.Items.Add(Item("Hr", null, () => Exec("hr")));
        MainMenu.Items.Add(fmt);

        var view = new MenuItem { Header = Strings.Get("View") };
        view.Items.Add(Item("ModeVisual", ctrl + "1", () => SetMode("visual"), _settings.Mode == "visual"));
        view.Items.Add(Item("ModeSource", ctrl + "2", () => SetMode("source"), _settings.Mode == "source"));
        view.Items.Add(Item("ModePreview", ctrl + "3", () => SetMode("preview"), _settings.Mode == "preview"));
        view.Items.Add(new Separator());
        view.Items.Add(Item("ReadOnly", ctrl + "Shift+L", () => SetReadOnly(!_settings.ReadOnly), _settings.ReadOnly));
        view.Items.Add(new Separator());
        view.Items.Add(Item("ThemeMenu", null, () => { ShowThemeDialog(); return Task.CompletedTask; }));
        view.Items.Add(new Separator());
        view.Items.Add(Item("ZoomIn", ctrl + "+", () => RunCommand("zoom+")));
        view.Items.Add(Item("ZoomOut", ctrl + "-", () => RunCommand("zoom-")));
        view.Items.Add(Item("ZoomReset", ctrl + "0", () => RunCommand("zoom0")));
        view.Items.Add(new Separator());
        view.Items.Add(Item("Files", ctrl + "Shift+E", () => { ToggleFiles(); return Task.CompletedTask; }, _settings.FilesPanel));
        view.Items.Add(Item("Outline", ctrl + "Shift+O", () => { ToggleOutline(); return Task.CompletedTask; }, _settings.Outline));
        view.Items.Add(Item("SearchPanelMenu", ctrl + "Shift+F", () => { ToggleSearch(); return Task.CompletedTask; }, _settings.SearchPanel));
        view.Items.Add(Item("RecentPanelMenu", null, () => { ToggleRecentPanel(); return Task.CompletedTask; }, _settings.RecentPanel));
        view.Items.Add(LayoutMenu());
        var widthMenu = new MenuItem { Header = Strings.Get("TextWidth") };
        foreach (var (key, px) in new[] { ("WidthNormal", 980), ("WidthWide", 1280), ("WidthWider", 1600), ("WidthFull", 0) })
            widthMenu.Items.Add(Item(key, null, async () =>
            {
                _settings.TextWidth = px; _settings.Save();
                await SyncPageAsync();
                BuildMenu();
            }, _settings.TextWidth == px));
        view.Items.Add(widthMenu);
        view.Items.Add(Item("Spell", null, () => SetSpellAsync(!_settings.Spell), _settings.Spell));
        view.Items.Add(Item("SpellLangs", null, () => { ShowSpellDialog(); return Task.CompletedTask; }));
        MainMenu.Items.Add(view);

        var lang = new MenuItem { Header = Strings.Get("Language") };
        foreach (var (code, name) in Strings.Languages)
        {
            var mi = new MenuItem { Header = name, IsCheckable = true, IsChecked = _settings.Lang == code };
            mi.Click += async (_, _) => await SetLanguage(code);
            lang.Items.Add(mi);
        }
        MainMenu.Items.Add(lang);

        var help = new MenuItem { Header = Strings.Get("Help") };
        help.Items.Add(Item("Manual", "F1", () => { OpenManual(); return Task.CompletedTask; }));
        help.Items.Add(Item("Sample", null, OpenSampleAsync, enabled: SamplePath != null));
        help.Items.Add(Item("SampleInRecent", null, () => { SetSampleHidden(!_settings.SampleHidden); return Task.CompletedTask; },
            check: !_settings.SampleHidden, enabled: SamplePath != null));
        help.Items.Add(Item("Guide", null, () =>
        {
            // GitHub's own guide in the interface language (it has English, Spanish and Japanese pages)
            OpenLink($"https://docs.github.com/{Strings.Lang}/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax");
            return Task.CompletedTask;
        }));
        help.Items.Add(Item("CheckUpdates", null, () => CheckUpdatesAsync(true)));
        help.Items.Add(new Separator());
        help.Items.Add(Item("About", null, () =>
        {
            new AboutDialog(this, Version).ShowDialog();
            return Task.CompletedTask;
        }));
        MainMenu.Items.Add(help);

        BuildToolbar();
        if (_folderInit) { ApplySideLayout(); UpdateRecentBlock(); }   // the layout drawing follows the theme colour; RECENT follows Open recent
        InputBindings.Clear();
        var f1 = new KeyBinding(new RelayCommand(OpenManual), Key.F1, ModifierKeys.None);
        InputBindings.Add(f1);
    }

    // ------------------------------------------------------------------ Native toolbar
    private bool _segWired;
    private readonly HashSet<Button> _panelTools = new();

    private Button Tool(string glyph, string tipKey, string? gesture, Func<Task> action)
    {
        var b = new Button
        {
            Content = glyph,
            Style = (Style)FindResource("Gm.ToolButton"),
            ToolTip = Strings.Plain(tipKey) + (gesture != null ? "   " + gesture : ""),
        };
        System.Windows.Automation.AutomationProperties.SetName(b, Strings.Plain(tipKey));   // Narrator says this, not the icon's character
        b.Click += async (_, _) => { await action(); if (_ready) await Js("mdApp.focus()"); };
        return b;
    }

    private void BuildToolbar()
    {
        Tools.Children.Clear();
        void Sep() => Tools.Children.Add(new Border { Style = (Style)FindResource("Gm.ToolSep") });
        var newFile = Tool("\uE8A5", "NewMarkdown", null, () => NewItemAsync(false));
        newFile.Content = NewFileGlyph(); newFile.Tag = "\uE8A5";   // Tag: the glyph of its entry in the ⋯ menu
        Tools.Children.Add(newFile);
        var newFolder = Tool("\uE8F4", "NewFolder", null, () => NewItemAsync(true));
        Tools.Children.Add(newFolder);
        _newItemTools = new[] { newFile, newFolder };
        Sep();
        Tools.Children.Add(Tool("\uE8B7", "Files", "Ctrl+Shift+E", () => { ToggleFiles(); return Task.CompletedTask; }));
        Tools.Children.Add(Tool("", "Outline", "Ctrl+Shift+O", () => { ToggleOutline(); return Task.CompletedTask; }));
        Tools.Children.Add(Tool("\uE721", "SearchPanelMenu", "Ctrl+Shift+F", () => { ToggleSearch(); return Task.CompletedTask; }));
        // the buttons so far make files and folders or show panes: they work in every mode, also GitHub preview and
        // read-only, where the formatting buttons after them are greyed out (SyncSegments); the first two also need
        // the tree unlocked (UpdateNewItemTools)
        _panelTools.Clear();
        _panelTools.UnionWith(Tools.Children.OfType<Button>());
        Sep();

        var h = Tool("", "Heading", null, () => Task.CompletedTask);
        h.Click += (_, _) =>
        {
            // from the ⋯ menu the button is hidden: open at the mouse instead
            var cm = new ContextMenu { PlacementTarget = h, Placement = h.IsVisible ? System.Windows.Controls.Primitives.PlacementMode.Bottom : System.Windows.Controls.Primitives.PlacementMode.MousePoint };
            for (int lv = 1; lv <= 6; lv++)
            {
                int level = lv;
                var mi = new MenuItem { Header = $"H{level}  " + Strings.Plain("Heading") + $" {level}", FontSize = 15 - level * 0.5, FontWeight = level <= 3 ? FontWeights.SemiBold : FontWeights.Normal };
                mi.Click += async (_, _) => { await Exec("heading", $"{{level:{level}}}"); await Js("mdApp.focus()"); };
                cm.Items.Add(mi);
            }
            cm.Items.Add(new Separator());
            var p = new MenuItem { Header = Strings.Plain("Paragraph") };
            p.Click += async (_, _) => { await Exec("heading", "{level:0}"); await Js("mdApp.focus()"); };
            cm.Items.Add(p);
            cm.IsOpen = true;
        };
        Tools.Children.Add(h);
        Tools.Children.Add(Tool("", "Bold", "Ctrl+B", () => Exec("bold")));
        Tools.Children.Add(Tool("", "Italic", "Ctrl+I", () => Exec("italic")));
        Tools.Children.Add(Tool("", "Strike", null, () => Exec("strike")));
        Sep();
        Tools.Children.Add(Tool("", "Quote", null, () => Exec("blockQuote")));
        Tools.Children.Add(Tool("", "Hr", null, () => Exec("hr")));
        Sep();
        Tools.Children.Add(Tool("", "Ul", null, () => Exec("bulletList")));
        Tools.Children.Add(Tool("", "Ol", null, () => Exec("orderedList")));
        Tools.Children.Add(Tool("", "Task", null, () => Exec("taskList")));
        Tools.Children.Add(Tool("", "Outdent", null, () => Exec("outdent")));
        Tools.Children.Add(Tool("", "Indent", null, () => Exec("indent")));
        Sep();
        Tools.Children.Add(Tool("", "Table", null, InsertTableAsync));
        Tools.Children.Add(Tool("", "Image", null, InsertImageAsync));
        Tools.Children.Add(Tool("", "Link", null, InsertLinkAsync));
        Sep();
        Tools.Children.Add(Tool("", "Code", null, () => Exec("code")));
        Tools.Children.Add(Tool("", "CodeBlock", null, () => Exec("codeBlock")));

        SegVisual.Content = Strings.Get("SegVisual");
        SegSource.Content = Strings.Get("SegSource");
        SegPreview.Content = Strings.Get("SegPreview");
        SegVisual.ToolTip = SegTip("ModeVisual", "TipVisual", "Ctrl+1");
        SegSource.ToolTip = SegTip("ModeSource", "TipSource", "Ctrl+2");
        SegPreview.ToolTip = SegTip("ModePreview", "TipPreview", "Ctrl+3");
        if (ZoomSlider != null && ToolsMore.ToolTip != null) LocalizeFit();
        Dispatcher.BeginInvoke(FitToolbar);
        AutoSaveBtn.Content = Strings.Plain("AutoSave");
        AutoSaveBtn.ToolTip = SegTip("AutoSave", "TipAutoSave", "");
        AutoSaveBtn.IsChecked = _settings.AutoSave;
        SegEdit.Content = SegLabel("\uE70F", "SegEdit");
        SegRead.Content = SegLabel("\uE72E", "SegRead");
        SegEdit.ToolTip = SegTip("SegEdit", "TipEdit", "Ctrl+Shift+L");
        SegRead.ToolTip = SegTip("SegRead", "TipRead", "Ctrl+Shift+L");
        SyncSegments();
        if (!_segWired)
        {
            _segWired = true;
            SegVisual.Checked += async (_, _) => { if (_settings.Mode != "visual") await SetMode("visual"); };
            SegSource.Checked += async (_, _) => { if (_settings.Mode != "source") await SetMode("source"); };
            SegPreview.Checked += async (_, _) => { if (_settings.Mode != "preview") await SetMode("preview"); };
            SegEdit.Checked += async (_, _) => { if (_settings.ReadOnly) await SetReadOnly(false); };
            SegRead.Checked += async (_, _) => { if (!_settings.ReadOnly) await SetReadOnly(true); };
            // Checked/Unchecked, not Click: the switch also turns by keyboard and by accessibility tools
            AutoSaveBtn.Checked += async (_, _) => { if (!_settings.AutoSave) await SetAutoSave(true); };
            AutoSaveBtn.Unchecked += async (_, _) => { if (_settings.AutoSave) await SetAutoSave(false); };
        }
    }

    private static object SegLabel(string glyph, string key)
    {
        var p = new StackPanel { Orientation = Orientation.Horizontal };
        p.Children.Add(new TextBlock { Text = glyph, FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"), FontSize = 13, VerticalAlignment = VerticalAlignment.Center });
        p.Children.Add(new TextBlock { Text = Strings.Get(key), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 0, 0) });
        return p;
    }

    /// <summary>Two-line tooltip for the mode buttons: name + shortcut in bold, then what the mode is for.</summary>
    internal static object SegTip(string nameKey, string tipKey, string gesture)
    {
        var p = new StackPanel { MaxWidth = 380 };
        p.Children.Add(new TextBlock { Text = (Strings.Plain(nameKey) + "   " + gesture).TrimEnd(), FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 3) });
        p.Children.Add(new TextBlock { Text = Strings.Get(tipKey), TextWrapping = TextWrapping.Wrap });
        return p;
    }

    private void SyncSegments()
    {
        SegVisual.IsChecked = _settings.Mode == "visual";
        SegSource.IsChecked = _settings.Mode == "source";
        SegPreview.IsChecked = _settings.Mode == "preview";
        SegEdit.IsChecked = !_settings.ReadOnly;
        SegRead.IsChecked = _settings.ReadOnly;
        foreach (var (seg, on) in new[] { (SegEdit, !_settings.ReadOnly), (SegRead, _settings.ReadOnly) })
            if (seg.Content is StackPanel sp && sp.Children.Count == 2) sp.Children[1].Visibility = on ? Visibility.Visible : Visibility.Collapsed;
        bool editing = _settings.Mode != "preview" && !_settings.ReadOnly;
        foreach (var b in Tools.Children.OfType<Button>()) b.IsEnabled = editing || _panelTools.Contains(b);
        UpdateNewItemTools();
    }

    private async Task<string> SelectedTextAsync() =>
        _ready ? JsonSerializer.Deserialize<string>(await Js("mdApp.selectedText()")) ?? "" : "";

    private async Task InsertLinkAsync()
    {
        if (!_ready) return;
        var sel = await SelectedTextAsync();
        var dlg = new InputDialog(this, Strings.Plain("Link"), new[] { (Strings.Get("LinkText"), sel), (Strings.Get("LinkUrl"), "https://") });
        if (dlg.ShowDialog() != true) return;
        var v = dlg.Values;
        if (v[1].Length == 0) return;
        await Exec("addLink", JsonSerializer.Serialize(new { linkUrl = v[1], linkText = v[0].Length > 0 ? v[0] : v[1] }));
    }

    private async Task InsertImageAsync()
    {
        if (!_ready) return;
        var dlg = new InputDialog(this, Strings.Plain("Image"), new[] { (Strings.Get("ImageUrl"), ""), (Strings.Get("ImageAlt"), "") },
            (d, i) =>
            {
                if (i != 0) return null;
                var b = new Button { Content = Strings.Get("Browse"), Style = (Style)FindResource("Gm.DialogButton") };
                b.Click += (_, _) =>
                {
                    var ofd = new OpenFileDialog { Filter = Strings.Get("ImgFilter"), InitialDirectory = _path != null ? Path.GetDirectoryName(_path) : null };
                    if (ofd.ShowDialog(d) != true) return;
                    var f = ofd.FileName;
                    if (_path != null)
                    {
                        var rel = Path.GetRelativePath(Path.GetDirectoryName(_path)!, f);
                        if (!rel.StartsWith("..") && !Path.IsPathRooted(rel)) f = rel;
                    }
                    d.SetField(0, f.Replace('\\', '/').Replace(" ", "%20"));
                    d.SetField(1, Path.GetFileNameWithoutExtension(ofd.FileName));
                };
                return b;
            });
        if (dlg.ShowDialog() != true) return;
        var v = dlg.Values;
        if (v[0].Length == 0) return;
        await Exec("addImage", JsonSerializer.Serialize(new { imageUrl = v[0], altText = v[1] }));
    }

    private async Task InsertTableAsync()
    {
        if (!_ready) return;
        var dlg = new InputDialog(this, Strings.Plain("Table"), new[] { (Strings.Get("Columns"), "3"), (Strings.Get("Rows"), "3") });
        if (dlg.ShowDialog() != true) return;
        var v = dlg.Values;
        int cols = int.TryParse(v[0], out var c) ? Math.Clamp(c, 1, 20) : 3;
        int rows = int.TryParse(v[1], out var r) ? Math.Clamp(r, 1, 100) : 3;
        await Exec("addTable", JsonSerializer.Serialize(new { columnCount = cols, rowCount = rows + 1 }));
    }

    private static string ShortDir(string p)
    {
        var d = Path.GetDirectoryName(p) ?? "";
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (d.StartsWith(home, StringComparison.OrdinalIgnoreCase)) d = "~" + d[home.Length..];
        return d.Length > 38 ? "…" + d[^37..] : d;
    }

    private void OpenManual()
    {
        var pdf = Path.Combine(AppDir, "docs", $"Vigia_Manual_{Strings.Lang.ToUpperInvariant()}.pdf");
        if (!File.Exists(pdf)) pdf = Path.Combine(AppDir, "docs", "Vigia_Manual_EN.pdf");
        if (File.Exists(pdf)) Process.Start(new ProcessStartInfo(pdf) { UseShellExecute = true });
    }

    private async Task SetLanguage(string code)
    {
        _settings.Lang = code; Strings.Lang = code; _settings.Save();
        BuildMenu(); UpdateTitle(); UpdateStatus(); LocalizeFeatures(); RefreshTabs();
        // a new language needs a new browser for its menus (the document is carried over): SyncPageAsync
        bool restart = _ready && BrowserLang(code) != _cur.WebLang;
        if (_spare != null && _spare.WebLang != BrowserLang(code)) { DropSpare(); PrepareSpare(); }
        await SyncPageAsync();
        if (restart) return;
        // the sample follows the interface language (unless it was edited: those changes are kept)
        if (_ready && _isSample && _path == null && !_dirty) await LoadSampleAsync();
    }

    /// <summary>A new browser for the shown tab (a new interface language): its text and state are carried over.</summary>
    private async Task RestartWebAsync()
    {
        var t = _cur;
        string text;
        using (DocBusy()) text = JsonSerializer.Deserialize<string>(await Js("mdApp.getMarkdownPreserving()")) ?? "";
        if (t != _cur || t.View == null) return;
        t.Restore = (text, t.Dirty);
        _settings.Zoom = t.View.ZoomFactor;
        t.Ready = false;
        var old = t.View;
        if (old.CoreWebView2 != null) old.CoreWebView2.WebMessageReceived -= OnWebMessage;
        DocGrid.Children.Remove(old);
        old.Dispose();
        t.View = null;
        CreateView(t);
        await InitWebAsync(t);
    }

    private async Task RestoreDocAsync(string text, bool dirty)
    {
        using var busy = DocBusy();
        if (_isSample && _path == null && !dirty) { await LoadSampleAsync(); return; }
        await Js($"mdApp.load({Q(text)},{DocBaseJs(_path)})");
        _dirty = dirty;
        _words = int.TryParse(await Js("mdApp.words()"), out var n) ? n : 0;
        UpdateTitle(); UpdateStatus();
    }

    // ------------------------------------------------------------------ Chrome
    /// <param name="reveal">false on a tab switch, which shows the file in the Files panel a moment later (AfterSwitch).</param>
    private void UpdateTitle(bool reveal = true)
    {
        var name = _path != null ? Path.GetFileName(_path) : Strings.Get(_isSample ? "SampleTitle" : "Untitled");
        Title = $"{(_dirty ? "● " : "")}{name} — Vigía" + (Settings.Profile != null ? $"  [{Settings.Profile}]" : "");
        RefreshTabHeader(_cur);   // its tab: name and the dot of unsaved changes
        if (reveal) RevealCurrent();   // the Files panel follows the open file
    }

    private void UpdateStatus()
    {
        StatusPath.Text = _path ?? Strings.Get(_isSample ? "SampleTitle" : "Untitled");
        StatusWords.Text = string.Format(Strings.Get("Words"), _words);
        StatusMode.Text = Strings.Plain(_settings.Mode switch { "source" => "ModeSource", "preview" => "ModePreview", _ => "ModeVisual" })
            + (_settings.ReadOnly ? " · \U0001F512 " + Strings.Get("SegRead") : "");
        StatusState.Text = _dirty ? Strings.Get("Modified") : (_path != null ? Strings.Get("Saved") : "");
    }
}

public sealed class RelayCommand(Action run) : ICommand
{
    public event EventHandler? CanExecuteChanged { add { } remove { } }
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => run();
}
