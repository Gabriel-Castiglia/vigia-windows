// Vigia -- writing a file so that it is never left empty or cut in half.
// Copyright (C) 2026 Gabriel Castiglia. Licensed under the GNU GPL, version 2 only.
// Date: November 25, 2026
// SPDX-License-Identifier: GPL-2.0-only

using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Vigia;

/// <summary>Writes a file in one step: the new text goes to a temporary file next to it, reaches the disk, and only
/// then takes the place of the old one. A power cut, a crash or an antivirus holding the file in the middle of a write
/// leaves the old file or the new one, never an empty or half-written one (File.WriteAllText first empties the file
/// and then writes it, so a write cut short lost the document). The file keeps its attributes and permissions.</summary>
internal static class AtomicFile
{
    /// <param name="backup">Where the previous version is kept (settings.json.bak), or null for none.</param>
    public static void WriteAllText(string path, string text, Encoding encoding, string? backup = null)
    {
        path = Path.GetFullPath(path);
        var dir = Path.GetDirectoryName(path)!;
        if (File.Exists(path) && (File.GetAttributes(path) & FileAttributes.ReadOnly) != 0)
            throw new UnauthorizedAccessException($"Access to the path '{path}' is denied.");   // as File.WriteAllText says it
        var tmp = Path.Combine(dir, "." + Path.GetFileName(path) + "." + Guid.NewGuid().ToString("N")[..8] + ".tmp");
        try
        {
            var preamble = encoding.GetPreamble();
            var body = encoding.GetBytes(text);
            using (var fs = new FileStream(tmp, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            {
                fs.Write(preamble, 0, preamble.Length);
                fs.Write(body, 0, body.Length);
                fs.Flush(true);
            }
            if (!File.Exists(path)) { File.Move(tmp, path); return; }
            try
            {
                if (backup != null && File.Exists(backup)) File.Delete(backup);
                Replaced[path] = DateTime.UtcNow;   // before the events it raises arrive (JustReplaced)
                File.Replace(tmp, path, backup, ignoreMetadataErrors: true);
            }
            catch (IOException) when (File.Exists(tmp) && !IsLocked(path))
            {
                // a file system that cannot replace in one step (some network shares): write the old way, which is
                // what every version before 3.2.1 did
                if (backup != null) { try { File.Copy(path, backup, true); } catch { } }
                File.WriteAllText(path, text, encoding);
            }
        }
        finally
        {
            try { if (File.Exists(tmp)) File.Delete(tmp); } catch { /* left behind: harmless, and the Files panel lists only Markdown files */ }
        }
    }

    // ------------------------------------------------------------------ the events a write raises
    // Replacing a file raises file-name events (the temporary file created, Windows' own "a.md~RF1a2b.TMP", the file
    // renamed away and the temporary file renamed in its place). The folder watchers of the Files panel rebuilt the
    // tree for each of them, so on every save, autosaves included. They ask here what to leave out.

    private static readonly ConcurrentDictionary<string, DateTime> Replaced = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Regex OwnTemp = new(@"^\..+\.[0-9a-f]{8}\.tmp$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex ReplaceTemp = new(@"~RF[0-9a-f]*\.TMP$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>A temporary file of a write in one step: ours (".a.md.1a2b3c4d.tmp") or the one Windows makes while
    /// replacing ("a.md~RF1a2b.TMP"). Takes a name or a path.</summary>
    public static bool IsTemporaryName(string? nameOrPath)
    {
        if (string.IsNullOrEmpty(nameOrPath)) return false;
        var name = Path.GetFileName(nameOrPath);
        return OwnTemp.IsMatch(name) || ReplaceTemp.IsMatch(name);
    }

    /// <summary>True for a few seconds after this program replaced the file (not after it created it: a new file is a
    /// change the Files panel must show).</summary>
    public static bool JustReplaced(string? path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        var now = DateTime.UtcNow;
        foreach (var (k, t) in Replaced) if (now - t > TimeSpan.FromSeconds(5)) Replaced.TryRemove(k, out _);
        try { path = Path.GetFullPath(path); } catch { return false; }
        return Replaced.TryGetValue(path, out var when) && now - when <= TimeSpan.FromSeconds(5);
    }

    /// <summary>A name in a file-name event that a write in one step explains: a temporary file, or the file it replaced.</summary>
    public static bool ExplainedByWrite(string? path) => IsTemporaryName(path) || JustReplaced(path);

    /// <summary>True when another program holds the file so that it cannot be written now.</summary>
    private static bool IsLocked(string path)
    {
        try { using var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete); return false; }
        catch (IOException) { return true; }
        catch (UnauthorizedAccessException) { return false; }
    }
}
