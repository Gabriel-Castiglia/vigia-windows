// Vigia -- main window: deleting files and folders from the Files panel, to the Recycle Bin.
// Copyright (C) 2026 Gabriel Castiglia. Licensed under the GNU GPL, version 2 only.
// Date: November 26, 2026
// SPDX-License-Identifier: GPL-2.0-only

using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace Vigia;

/// <summary>Delete (right-click → Delete, or the Delete key on a row of the tree), only with the tree unlocked (the
/// padlock of the Files panel; locked, the entry is greyed out): the file or folder goes to the
/// Recycle Bin, where it can be restored. A network folder or a USB stick has no Recycle Bin: there it is deleted
/// for good, and the question says so. A question comes first, with "Don't ask again in this window": that box
/// lasts only as long as this window; a new window, or Vigía started again, asks again. The tabs of what is deleted
/// are closed, and it leaves Favourites and Open recent. The open folders themselves (the coloured blocks) are
/// closed from their own menu, never deleted from here.</summary>
public partial class MainWindow
{
    /// <summary>"Don't ask again in this window" was ticked. Never saved: only this window remembers it.</summary>
    private bool _deleteNoAsk;

    /// <summary>The self-test deletes for good instead, so its files never reach the user's Recycle Bin.</summary>
    internal bool DeletePermanently;

    private MenuItem DeleteEntry(string path)
    {
        var mi = MenuEntry(Strings.Plain("Delete"), () => _ = DeleteNodeAsync(path), enabled: CanEditNode(path) && !_settings.TreeLocked);
        mi.InputGestureText = Strings.Get("DeleteKey");
        return CanEditNode(path) ? LockedHelp(mi) : mi;
    }

    /// <summary>Asks (unless told not to in this window) and deletes.</summary>
    private async Task DeleteNodeAsync(string path)
    {
        try
        {
            if (!CanEditNode(path)) return;
            if (_settings.TreeLocked) { Flash(Strings.Get("NeedsUnlock")); return; }
            bool isDir = Directory.Exists(path);
            if (!isDir && !File.Exists(path)) { Flash(Strings.Get("ItemGone")); RefreshTree(); return; }
            var name = Path.GetFileName(path);
            bool forGood = DeletePermanently || !HasRecycleBin(path);
            // "don't ask again" never covers a deletion for good: that one always asks
            if (!_deleteNoAsk || forGood)
            {
                var key = (isDir ? "DeleteAskFolder" : "DeleteAskFile") + (forGood ? "ForGood" : "");
                var text = string.Format(Strings.Get(key), name);
                int choice = ChoiceDialog.Ask(this, text, new[] { Strings.Get("Cancel"), Strings.Plain("Delete") }, 0,
                                              Strings.Get("DeleteNoAsk"), out bool noAsk);
                if (choice != 1) return;
                if (noAsk) _deleteNoAsk = true;
            }
            if (await DeletePath(path)) Flash(string.Format(Strings.Get(forGood ? "DeletedForGood" : "Deleted"), name));
        }
        catch (Exception ex) { LogError("DeleteNodeAsync", ex); }
    }

    /// <summary>Deletes without asking: closes the tabs of the file (or of the files inside the folder), sends it to
    /// the Recycle Bin (or deletes it for good where there is none) and takes it out of the lists. False, with a
    /// message, when it could not be done.</summary>
    internal async Task<bool> DeletePath(string path)
    {
        bool isDir = Directory.Exists(path);
        if (!isDir && !File.Exists(path)) { Flash(Strings.Get("ItemGone")); RefreshTree(); return false; }
        bool Affected(string? p) => p != null && (PathCmp.Equals(p, path) || (isDir && IsUnder(p, path)));

        // its tabs go first, without saving: what they hold is being thrown away (the Recycle Bin keeps the file)
        foreach (var t in _tabs.Where(t => Affected(t.Path)).ToList())
        {
            t.Dirty = false;
            if (t == _cur) { _dirty = false; Watch(null); } else WatchTab(t, null);
            await CloseTabAsync(t);
        }
        try
        {
            if (DeletePermanently || !HasRecycleBin(path)) { if (isDir) Directory.Delete(path, true); else File.Delete(path); }
            else ToRecycleBin(path);
        }
        catch (Exception e)
        {
            Flash(string.Format(Strings.Get("DeleteFailed"), Path.GetFileName(path), e.Message));
            RefreshTree();
            return false;
        }

        bool changed = false;
        foreach (var list in new[] { _settings.Recent, _settings.RecentFolders, _settings.FavoriteFiles })
            changed |= list.RemoveAll(p => Affected(p)) > 0;
        if (changed) { _settings.Save(); BuildMenu(); }
        RefreshTree();
        ApplyFilesVisibility();
        return true;
    }

    // ------------------------------------------------------------------ the Recycle Bin (SHFileOperation)
    /// <summary>Only fixed drives have a Recycle Bin: a network share (\\server\share or a mapped drive) or a
    /// removable one (a USB stick) deletes for good, so Vigía asks differently there.</summary>
    internal static bool HasRecycleBin(string path)
    {
        try
        {
            if (path.StartsWith(@"\\")) return false;
            var root = Path.GetPathRoot(path);
            return root != null && new DriveInfo(root).DriveType == DriveType.Fixed;
        }
        catch { return false; }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ShFileOp
    {
        public IntPtr Hwnd;
        public uint Func;
        public string From;
        public string? To;
        public ushort Flags;
        public int Aborted;
        public IntPtr NameMappings;
        public string? ProgressTitle;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHFileOperation(ref ShFileOp op);

    // FO_DELETE deletes; FOF_ALLOWUNDO sends to the Recycle Bin instead of deleting for good; the other three keep
    // Explorer from showing its own question, progress or error windows (Vigía has asked already, and reports errors)
    private const uint FO_DELETE = 3;
    private const ushort FOF_SILENT = 0x4, FOF_NOCONFIRMATION = 0x10, FOF_ALLOWUNDO = 0x40, FOF_NOERRORUI = 0x400;

    /// <summary>Moves a file or a folder (with everything in it) to the Recycle Bin, with no window of Explorer's
    /// own; throws when Windows refuses (in use, no permission). Called only for fixed drives (HasRecycleBin).</summary>
    private static void ToRecycleBin(string path)
    {
        var op = new ShFileOp
        {
            Func = FO_DELETE,
            From = path + "\0\0",   // a list of paths, each ended by a NUL, the list by another
            Flags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_SILENT | FOF_NOERRORUI,
        };
        int rc = SHFileOperation(ref op);
        if (rc != 0 || op.Aborted != 0 || File.Exists(path) || Directory.Exists(path))
            throw new IOException(string.Format(Strings.Get("DeleteRefused"), rc));
    }
}
