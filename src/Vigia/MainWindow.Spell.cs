// Vigia -- main window: spell check (SpellChecker.cs checks, each page marks; app.js spell check section).
// Copyright (C) 2026 Gabriel Castiglia. Licensed under the GNU GPL, version 2 only.
// Date: November 25, 2026
// SPDX-License-Identifier: GPL-2.0-only

using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace Vigia;

public partial class MainWindow
{
    /// <summary>The spell check messages of a page, answered at once even when its tab is hidden (the page marks its
    /// words in the background). True when the message was one of them.</summary>
    private bool HandleSpellMessage(DocTab tab, string json)
    {
        if (!json.StartsWith("{\"type\":\"spell", StringComparison.Ordinal)) return false;
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        switch (root.GetProperty("type").GetString())
        {
            case "spell":
                var words = root.GetProperty("words").EnumerateArray().Select(w => w.GetString() ?? "").Where(w => w.Length > 0).ToList();
                var langs = _settings.SpellLangs.ToList();
                var own = new HashSet<string>(_settings.SpellWords, StringComparer.Ordinal);
                _ = Task.Run(() => SpellChecker.Misspelled(words, langs, own)).ContinueWith(r =>
                    Dispatcher.BeginInvoke(async () =>
                    {
                        try { await JsIn(tab, $"mdApp.spellResult({JsonSerializer.Serialize(words)},{JsonSerializer.Serialize(r.Result)})"); } catch { }
                    }), TaskScheduler.Default);
                return true;
            case "spellMenu":
                if (tab == _cur) ShowSpellMenu(tab, root.GetProperty("word").GetString() ?? "");
                return true;
        }
        return true;
    }

    /// <summary>The right-click menu of a marked word: suggestions, Ignore, Add to dictionary.</summary>
    private void ShowSpellMenu(DocTab tab, string word)
    {
        var m = new ContextMenu { Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint };
        var sug = SpellChecker.Suggest(word, _settings.SpellLangs);
        foreach (var s in sug)
        {
            var text = s;
            var mi = new MenuItem { Header = text.Replace("_", "__"), FontWeight = FontWeights.SemiBold };
            mi.Click += async (_, _) => { try { await JsIn(tab, $"mdApp.spellReplace({Q(text)})"); } catch { } };
            m.Items.Add(mi);
        }
        if (sug.Count == 0) m.Items.Add(new MenuItem { Header = Strings.Plain("SpellNoSuggestions"), IsEnabled = false });
        m.Items.Add(new Separator());
        m.Items.Add(MenuEntryAsync(Strings.Get("SpellIgnore"), () => SpellAcceptAsync(word, permanent: false)));
        m.Items.Add(MenuEntryAsync(Strings.Get("SpellAdd"), () => SpellAcceptAsync(word, permanent: true)));
        m.Items.Add(new Separator());
        m.Items.Add(MenuEntryAsync(Strings.Get("SpellLangs"), () => { ShowSpellDialog(); return Task.CompletedTask; }));
        m.IsOpen = true;
    }

    /// <summary>Ignore (until Vigía closes, in every tab) or Add to dictionary (kept in settings.json, this profile).</summary>
    internal async Task SpellAcceptAsync(string word, bool permanent)
    {
        if (string.IsNullOrEmpty(word)) return;
        if (permanent)
        {
            if (!_settings.SpellWords.Contains(word)) { _settings.SpellWords.Add(word); _settings.SpellWords.Sort(StringComparer.CurrentCultureIgnoreCase); _settings.Save(); }
        }
        else SpellChecker.Ignore(word);
        foreach (var t in _tabs.Where(t => t.Ready).ToList()) { try { await JsIn(t, $"mdApp.spellAccept({Q(word)})"); } catch { } }
    }

    /// <summary>Languages or the dictionary changed: every page asks for its words again.</summary>
    private async Task SpellResetAllAsync()
    {
        SpellChecker.Warm(_settings.SpellLangs);
        foreach (var t in _tabs.Where(t => t.Ready).ToList()) { try { await JsIn(t, "mdApp.spellReset()"); } catch { } }
    }

    /// <summary>On or off for every open tab (the text and its undo history stay).</summary>
    internal async Task SetSpellAsync(bool on)
    {
        _settings.Spell = on; _settings.Save();
        foreach (var t in _tabs.Where(t => t.Ready).ToList())
        {
            t.PageSpell = on;
            try { await JsIn(t, $"mdApp.setSpell({(on ? "true" : "false")})"); } catch { }
        }
        BuildMenu();
    }

    /// <summary>View → Spell check languages and dictionary: on/off, the languages (several at once), adding a
    /// Hunspell dictionary, and the words added to the dictionary, which can be removed.</summary>
    private void ShowSpellDialog()
    {
        var w = new Window
        {
            Title = Strings.Get("SpellTitle"), Owner = this, Width = 560, SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner, ResizeMode = ResizeMode.NoResize, ShowInTaskbar = false,
        };
        w.SetResourceReference(BackgroundProperty, "Gm.Page");
        w.SetResourceReference(ForegroundProperty, "Gm.Text");
        var panel = new StackPanel { Margin = new Thickness(18) };
        var on = new CheckBox { Content = Strings.Get("SpellOn"), IsChecked = _settings.Spell, Margin = new Thickness(0, 0, 0, 12) };
        on.SetResourceReference(ForegroundProperty, "Gm.Text");
        on.Click += async (_, _) => await SetSpellAsync(on.IsChecked == true);
        panel.Children.Add(on);
        panel.Children.Add(new TextBlock { Text = Strings.Get("SpellLangsHint"), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 6) });
        var langs = new StackPanel { Margin = new Thickness(12, 0, 0, 6) };
        void FillLangs()
        {
            langs.Children.Clear();
            foreach (var code in SpellChecker.Available())
            {
                var key = "SpellLang" + code;
                string name = Strings.Get(key) is { } n && n != key ? n : code;
                if (SpellChecker.Missing(code)) name += " " + Strings.Get("SpellMissing");
                var cb = new CheckBox { Content = name, IsChecked = _settings.SpellLangs.Contains(code), Margin = new Thickness(0, 2, 0, 2) };
                cb.SetResourceReference(ForegroundProperty, "Gm.Text");
                var c = code;
                cb.Click += async (_, _) =>
                {
                    _settings.SpellLangs.Remove(c);
                    if (cb.IsChecked == true) _settings.SpellLangs.Add(c);
                    _settings.Save();
                    await SpellResetAllAsync();
                };
                langs.Children.Add(cb);
            }
        }
        FillLangs();
        panel.Children.Add(langs);
        var add = new Button { Content = Strings.Get("SpellAddLang"), HorizontalAlignment = HorizontalAlignment.Left, Padding = new Thickness(10, 3, 10, 3), Margin = new Thickness(12, 0, 0, 14) };
        add.Click += async (_, _) =>
        {
            var dlg = new OpenFileDialog { Filter = Strings.Get("SpellAddLangFilter") };
            if (dlg.ShowDialog(w) != true) return;
            try
            {
                var name = SpellChecker.AddUserDictionary(dlg.FileName);
                if (!_settings.SpellLangs.Contains(name)) _settings.SpellLangs.Add(name);
                _settings.Save();
                FillLangs();
                await SpellResetAllAsync();
            }
            catch (Exception ex) { MessageBox.Show(w, string.Format(Strings.Get("SpellAddLangErr"), ex.Message), "Vigía", MessageBoxButton.OK, MessageBoxImage.Warning); }
        };
        panel.Children.Add(add);
        panel.Children.Add(new TextBlock { Text = Strings.Get("SpellWordsHint"), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 6) });
        var list = new ListBox { Height = 150, SelectionMode = SelectionMode.Extended };
        foreach (var x in _settings.SpellWords) list.Items.Add(x);
        panel.Children.Add(list);
        var row = new DockPanel { Margin = new Thickness(0, 10, 0, 0), LastChildFill = false };
        var remove = new Button { Content = Strings.Get("SpellRemove"), Padding = new Thickness(10, 3, 10, 3) };
        remove.Click += async (_, _) =>
        {
            foreach (var x in list.SelectedItems.Cast<string>().ToList()) { _settings.SpellWords.Remove(x); list.Items.Remove(x); }
            _settings.Save();
            await SpellResetAllAsync();
        };
        var close = new Button { Content = Strings.Get("SpellClose"), Padding = new Thickness(14, 3, 14, 3), IsCancel = true, IsDefault = true };
        close.Click += (_, _) => w.Close();
        DockPanel.SetDock(close, Dock.Right);
        row.Children.Add(remove); row.Children.Add(close);
        panel.Children.Add(row);
        w.Content = panel;
        w.ShowDialog();
    }
}
