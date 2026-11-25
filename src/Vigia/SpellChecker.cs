// Vigia -- spell check: Hunspell dictionaries (WeCantSpell.Hunspell), several languages at once.
// Copyright (C) 2026 Gabriel Castiglia. Licensed under the GNU GPL, version 2 only.
// Date: November 25, 2026
// SPDX-License-Identifier: GPL-2.0-only

using System.Collections.Concurrent;
using System.IO;
using WeCantSpell.Hunspell;

namespace Vigia;

/// <summary>One checker for every tab. A word is right when any chosen language knows it, so a text that mixes
/// languages is checked as a whole. The browser's own checker (the Windows spell checker in WebView2) keeps a single
/// dictionary per browser and has no ignore list, so the pages ask this one instead.
/// Bundled dictionaries are in dict/ next to the program (en, es, es-UY: the Spanish of Uruguay, with the voseo); more can be added by the user as a Hunspell pair
/// (name.aff + name.dic) in the settings folder's "dictionaries" folder. Japanese has no spelling dictionary: words in
/// Japanese script are never marked.</summary>
internal static class SpellChecker
{
    private static readonly ConcurrentDictionary<string, WordList?> Lists = new();
    private static readonly HashSet<string> Session = new(StringComparer.Ordinal);   // Ignore: until Vigía closes
    private static readonly object Gate = new();

    public static string BundledDir => Path.Combine(AppContext.BaseDirectory, "dict");
    public static string UserDir => Path.Combine(Settings.Dir, "dictionaries");

    /// <summary>Every language that can be chosen: the bundled ones, Japanese, and the user's dictionaries.</summary>
    public static List<string> Available()
    {
        var l = new List<string> { "en", "es", "es-UY", "ja" };
        try
        {
            if (Directory.Exists(UserDir))
                foreach (var f in Directory.GetFiles(UserDir, "*.dic"))
                {
                    var n = Path.GetFileNameWithoutExtension(f);
                    if (File.Exists(Path.ChangeExtension(f, ".aff")) && !l.Contains(n, StringComparer.OrdinalIgnoreCase)) l.Add(n);
                }
        }
        catch { }
        return l;
    }

    private static WordList? Load(string lang)
    {
        if (lang == "ja") return null;
        return Lists.GetOrAdd(lang, k =>
        {
            foreach (var dir in new[] { UserDir, BundledDir })
            {
                var dic = Path.Combine(dir, k + ".dic");
                if (File.Exists(dic) && File.Exists(Path.ChangeExtension(dic, ".aff")))
                {
                    try { return WordList.CreateFromFiles(dic, Path.ChangeExtension(dic, ".aff")); } catch { }
                }
            }
            return null;
        });
    }

    /// <summary>Loads the dictionaries in the background, so the first check is quick.</summary>
    public static void Warm(IEnumerable<string> langs) { var l = langs.ToList(); _ = Task.Run(() => { foreach (var x in l) Load(x); }); }

    /// <summary>A language whose dictionary cannot be read (a user dictionary deleted or broken).</summary>
    public static bool Missing(string lang) => lang != "ja" && Load(lang) == null;

    public static void Ignore(string word) { lock (Gate) Session.Add(word); }
    public static bool IsIgnored(string word) { lock (Gate) return Session.Contains(word); }
    public static void ForgetSession() { lock (Gate) Session.Clear(); }

    /// <summary>Words in Japanese (or Chinese) script, numbers and single letters are never marked.</summary>
    private static bool Skip(string w) =>
        w.Length < 2 || w.Any(char.IsDigit) || w.Any(c => c >= 0x3000 && c <= 0x9FFF || c >= 0xF900 && c <= 0xFAFF || c >= 0xFF00 && c <= 0xFFEF);

    public static bool Check(string word, IReadOnlyList<string> langs, ICollection<string> own)
    {
        if (Skip(word) || own.Contains(word) || IsIgnored(word)) return true;
        bool any = false;
        foreach (var l in langs)
        {
            var wl = Load(l);
            if (wl == null) continue;
            any = true;
            if (wl.Check(word)) return true;
            // "don’t": a dictionary that does not list the typographic apostrophe among its letters knows the word
            // only with the straight one (the user's Hunspell dictionaries, and en.aff before LibreOffice's 2024 fix)
            if (word.Contains('’') && wl.Check(word.Replace('’', '\''))) return true;
        }
        return !any;   // no dictionary at all: nothing is marked
    }

    /// <summary>The words of a list that no chosen language knows.</summary>
    public static List<string> Misspelled(IEnumerable<string> words, IReadOnlyList<string> langs, ICollection<string> own) =>
        words.Distinct().Where(w => !Check(w, langs, own)).ToList();

    /// <summary>Suggestions from every chosen language, the best of each first, at most eight.</summary>
    public static List<string> Suggest(string word, IReadOnlyList<string> langs)
    {
        var per = langs.Select(Load).Where(w => w != null).Select(w => w!.Suggest(word).Take(5).ToList()).ToList();
        var r = new List<string>();
        for (int i = 0; i < 5 && r.Count < 8; i++)
            foreach (var s in per) if (i < s.Count && !r.Contains(s[i])) r.Add(s[i]);
        return r.Take(8).ToList();
    }

    /// <summary>Adds a Hunspell dictionary (the .dic file; its .aff must be next to it) to the user's ones.</summary>
    public static string AddUserDictionary(string dicPath)
    {
        var aff = Path.ChangeExtension(dicPath, ".aff");
        if (!File.Exists(aff)) throw new FileNotFoundException(aff);
        // read before it is copied: a pair that cannot be read never reaches the folder (it stayed there before, listed
        // as a language that is missing, and replaced a good dictionary of the same name)
        WordList.CreateFromFiles(dicPath, aff);   // throws if it cannot be read
        Directory.CreateDirectory(UserDir);
        var name = Path.GetFileNameWithoutExtension(dicPath);
        File.Copy(dicPath, Path.Combine(UserDir, name + ".dic"), true);
        File.Copy(aff, Path.Combine(UserDir, name + ".aff"), true);
        Lists.TryRemove(name, out _);
        return name;
    }
}
