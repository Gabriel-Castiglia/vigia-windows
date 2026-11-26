# Changelog

## 3.5.1 — 2026-11-26

- **New file, new folder and delete only with the tree unlocked** (the padlock at the top of the Files panel, as for
  dragging): while it is locked the two toolbar buttons and the *New Markdown file…*, *New folder…* and *Delete*
  entries of the right-click menu are greyed out, and the Delete key does nothing. Resting the pointer on one of
  them says to unlock the tree to enable it. The padlock's tooltip names the three things it unlocks.
- **A network folder or a USB stick has no Recycle Bin**: deleting there is for good, and the question now says so
  (*Delete … for good?*), always, even after *Don't ask again in this window*. 3.5.0 said the item went to the
  Recycle Bin there too.
- About Vigía credits the spell check: WeCantSpell.Hunspell and its dictionaries (SCOWL en_US, RLA-ES), which came
  with 3.3.0 but were missing from that window.

## 3.5.0 — 2026-11-26

- **Delete files and folders** from the Files panel: right-click → *Delete*, or the Delete key on the selected row.
  They go to the Recycle Bin, where they can be restored (a folder with everything in it). A question comes first,
  with *Don't ask again in this window*: ticked, that window stops asking until it closes; a new window, or Vigía
  started again, asks again. The tabs of what is deleted close (without saving: their text is in the Recycle Bin),
  and it leaves Favourites and Open recent. The open folders themselves are closed from their menu, as before,
  never deleted from the tree.

## 3.4.0 — 2026-11-26

- **New Markdown file and New folder**, the first two buttons of the toolbar, and two entries of the right-click
  menu of every folder of the Files panel. Both are made on disk at once, in the folder selected in the tree (the
  folder of the selected file, when a file is selected); with nothing selected, in `Documents\Vigia`, which is
  created and added to the panel the first time. A free name is offered (*New document.md*, *New document 2.md*…); a
  file without a Markdown extension gets `.md` and starts with its name as a heading. The new file opens at once; a
  new folder stays selected, so the next file goes into it. Nothing is ever written over: a name already taken, or
  one Windows would change or refuse, is refused with a message.
- The row selected in the Files panel stays selected when the tree is rebuilt (a file saved, another program writing
  in the folder): before, the selection went back to the open file every time.
- The buttons that show panes (Files, Outline, Search) and the two new ones work in every mode: in the GitHub view
  and in read only the whole toolbar was greyed out; now only the formatting buttons are.
- Narrator reads the toolbar buttons by name (it read the character of their icon) and the rows of the Files panel
  (it read nothing).

## 3.3.1 — 2026-11-25

- **Spanish of Uruguay** in the spell check (`es-UY`, with the voseo: *tenés*, *querés*, *mirá*), on by default next
  to the general Spanish; a list of languages saved by 3.3.0 gets it once (taken out by hand, it stays out). The
  general Spanish dictionary marked every verb of the voseo in red.
- **Newer dictionaries**: Spanish goes from RLA-ES 2.8 to 2.9 (more words, *Montevideo* among them), and English
  takes LibreOffice's fix of January 2024, so *don’t* and *it’s* written with the typographic apostrophe are right.
  A dictionary added by hand that does not list that apostrophe also accepts those words now.
- **Updates signed with either of the author's two keys** (the PC's or the Mac's): every Windows app of the author
  now checks the same `release.sig` (`ReleaseSignature.cs`). A Vigía 3.3.0 knows only the first key, so it offers
  the page of this release instead of installing it: install 3.3.1 by hand once.
- *Add a language…* reads the dictionary before copying it: one that cannot be read no longer stays in the list as a
  missing language, nor replaces a good dictionary of the same name.
- A document with more than 5000 different unknown words left the ones past 5000 unchecked until it was opened
  again; they are asked for in the next message now. A right click on a marked word could offer the suggestions
  of the word clicked before.
- `SHA256SUMS.txt` of the release lists the manuals again (3.3.0 listed only the installer).
- Newer libraries: DOMPurify 3.4.17 → 3.5.0 (it cleans what the GitHub view shows), TOAST UI Editor 3.2.2 → 3.2.3,
  highlight.js 11.11.1 → 11.12.0, marked-footnote 1.2.4 → 1.4.0, github-markdown-css 5.8.1 → 5.9.0, esbuild 0.25.5 →
  0.25.12, WebView2 SDK 1.0.4314.35 → 1.0.4370.26.

## 3.3.0 — 2026-11-25

- **Spell check in several languages at once**, on by default with English, Spanish and Japanese: a word is right
  when any chosen language knows it, so a text that mixes languages is checked as a whole. The checking is done by
  Vigía itself with Hunspell dictionaries (English and Spanish come with it; nothing to install in Windows): the
  browser's own checker kept a single language and had no ignore list. Marked words get a red wavy line in the
  Visual and Markdown views; code blocks, inline code and addresses are left out. Japanese text is never marked
  (there is no Japanese spelling dictionary); the English and Spanish words inside it are checked.
  - **Right-click a marked word**: its suggestions (a click replaces it), *Ignore (until Vigía closes)* — that word
    stops being marked in every tab until Vigía is closed — and *Add to dictionary*, kept in this profile's
    `settings.json`.
  - **View → Spell check languages and dictionary…**: on/off, the languages (several at once), the words added to
    the dictionary (select and *Remove*), and *Add a language…* for any Hunspell dictionary (`.dic` + `.aff`).
  - *View → Spell check* turns it on or off in every open tab without touching their text or undo history.
  - The dictionaries are in `dict/` with their licences (English: SCOWL, MIT/BSD; Spanish: RLA-ES, GPL/LGPL/MPL).
- **Preview tab**: a click on a file in the Files panel (or in Favourites or Recent) shows it in a passing tab, its
  name in italics, that the next click there reuses, so browsing a folder no longer leaves a tab per file. Typing in
  it, or a double click on the file or on the tab, keeps it. *File → Preview tab for files from the panel* turns it
  off.
- **New tabs open at once**: a page is started ahead, hidden, and the next tab takes it; starting the page took 0.8 to
  1.2 s per tab. The page is set up with the settings of the moment it is taken (language, theme, view).
- **Signed updates**: a release's `SHA256SUMS.txt` must now carry the author's ECDSA P-256 signature
  (`release.sig`), checked against a key built into Vigía, before the installer is downloaded; a release without a
  valid signature is refused and never run (it can still be downloaded by hand from its page).
- **Smaller page**: the GitHub view carries the ~40 common highlight.js languages (plus PowerShell, batch and
  Dockerfile) instead of all of them; a code block in any other language loads the full set only then. `bundle.js`
  goes from 2.5 MB to about 1.5 MB, so each page starts faster.
- The browser's own spell checker is turned off in its profile (it loaded dictionaries nobody used).
- The user manuals (English, Spanish, Japanese) explain the spell check, the preview tab and the signed updates.

## 3.2.1 — 2026-11-25

- **A file with a table whose cells end in inline HTML** (`| x <sub>L-02</sub> |`, two rows or more) showed a blank
  page in the Visual view, an empty outline and 0 words, while the GitHub view showed it whole: the editor library
  (TOAST UI) left its conversion unbalanced and returned an empty document. Each table cell now closes what it opened,
  and the file shows in every view. An edit made on that blank page could have saved an almost empty file.
- **`Ctrl+Z` right after opening a file emptied the document**, and autosave then wrote a single line break over the
  file: the loading of the text was kept in the undo history. It is cleared after a file is opened or reloaded from
  disk, so undo only takes back what was typed.
- **Saving writes the file in one step**: the text goes to a temporary file next to it, reaches the disk, and then
  takes the place of the old one (attributes and permissions kept). A power cut, a crash or an antivirus in the middle
  of a save leaves the old version or the new one, never an empty or cut file. Recovery drafts and `settings.json`
  are written the same way; `settings.json.bak` keeps the previous settings and is read when the file is damaged, so
  a dedicated instance no longer loses its folders, favourites and colours to a broken file. Autosave no longer
  rewrites the settings each time.
- **Closing the window and choosing *Don't save*** with a document that could not be autosaved (untitled, or
  autosave off) crashed the program; it closes normally now. An unexpected error no longer closes Vigía without
  warning: it is written to `error.log` in the settings folder and the drafts of every tab are saved.
- **Tabs**: a save that took long (a network folder, a file held by another program) while you switched tabs could
  finish on the other tab and later write its text into the first file. A save, and a reload after a change on disk,
  now always works on the tab it started on, and a tab switch waits for that tab's save to end. A change on disk
  that arrived while another one was being handled is no longer lost.
- **Switching tabs is faster**: the page is shown and focused first; the menu, the outline and the Files panel catch
  up right after.
- **Files panel**: the tree no longer jumps. A change on disk (a file created, renamed or deleted by another program
  or a sync tool) refreshes it without opening folders again or scrolling, and saving a file no longer rebuilds it.
  A file opened from Favourites (a click, `Enter` or *Open* in its right-click menu) leaves the trees as they are;
  opened from its folder, the panel still follows it. The right-click menus of the rows are built when they open.
- **Network pictures** (`\\server\share\a.png`) show only from the document's folder or a folder open in the panel;
  others are blocked, so a downloaded Markdown file cannot make Windows send your credentials to a server.

## 3.2.0 — 2026-11-24

- **Japanese** (日本語): the whole interface — menus, toolbar, tooltips, dialogs, panes, the editor's own menus and
  the theme colours — plus a Japanese user manual (`docs/Vigia_Manual_JA.pdf`, *Help → User manual*), a Japanese
  sample document, the installer in Japanese and a Japanese section in the README. *Language → 日本語*; on a Windows
  in Japanese it is the language from the first start. Access keys follow the Japanese convention, `ファイル(F)`.
  *Help → Markdown guide* opens GitHub's Japanese page. Windows has no Japanese spell checker: with the interface in
  Japanese, the English words of a text are checked. The status bar counts the words of a Japanese text with the
  browser's word breaker (a text without spaces counted as one word).
- The Files panel's title shows whole or not at all: next to the buttons, a narrow panel showed only "FO…".
- The sample document, in every language, has a section on tabs.
- **Tabs**: several documents in one window. Each tab has its own editor, so it keeps its undo history, cursor and
  scroll position while another one is shown; a dot on the tab marks unsaved changes. Opening a file shows the tab
  that already has it, or opens it in a new tab next to the shown one (in the shown tab itself when it is an empty
  untitled document or the sample as installed). Files dropped on the window, picked together in *File → Open…* or
  passed on the command line open a tab each, and so do the files of a second Vigía started meanwhile.
  - `Ctrl+N` (*File → New tab*) opens an untitled tab; `Ctrl+W` (*File → Close tab*, the × of a tab or a middle
    click) closes one, asking about its unsaved changes; the last tab closed becomes an empty untitled document.
    `Ctrl+Tab` / `Ctrl+Shift+Tab` and `Ctrl+PageDown` / `Ctrl+PageUp` switch tabs; `Ctrl+Shift+T` (*File → Reopen
    closed tab*) brings back the last one closed. A tab dragged along the strip moves among the others; the ⌄ button
    at the end lists them all, and a right click offers *Close the other tabs*, *Close the tabs to the right*,
    *Close all tabs*, *Copy path* and *Show in folder*.
  - **`Ctrl+W` now closes the tab, not the window** (`Alt+F4` or *File → Exit* close the window).
  - Closing the window closes every tab, each one with unsaved changes asking first (autosave writes the ones it
    may). With **File → Reopen tabs at start** on (off by default), the files of the tabs open again at the next
    start, with the same tab shown; the list is kept as the tabs change, so it survives a crash too. Untitled
    documents are not kept, and a file that is gone by then is left out.
  - A tab whose file changes on disk while another tab is shown is reloaded (or asks, with unsaved changes) when it
    is shown again. Moving or renaming files in the Files panel takes their tabs along. Each tab has its own crash
    recovery draft. An update reopens every tab.
  - Mode, read only, text width, spell check, theme, language and zoom are the same for every tab; a hidden tab
    takes a change when it is shown again.
- **Expand all**: the button in the Files panel's title collapses every folder while any is open, and expands them
  all, every level, when everything is closed; its icon and tooltip say which. Also in the panel's ⋯ menu. Expanding
  stops after 1,500 folders, so a tree as big as a whole drive does not freeze the window.

## 3.1.0 — 2026-11-21

- The default theme colour is sky blue, the lighthouse against the sky (it was dark gold), and so are the program's
  icon, its shortcuts and the installer. A colour chosen before stays.
- The same number as Vigía 3.1.0 for the Mac, which now lays out its panes as this version does.

## 3.0.0 — 2026-11-20

**Gryphmark is now Vigía**, a name easier to remember: the one who keeps watch from above, as the app watches a
Markdown file while an AI writes it. The icon is a lighthouse, from Debenne's design for a lighthouse (c. 1815,
The Met, CC0). The repository is now `vigia-windows` (GitHub forwards the old address).

- Installing Vigía removes Gryphmark (and Quillforge, if it is still there) in silence. The first time Vigía starts,
  everything under `%APPDATA%\Gryphmark` comes over to `%APPDATA%\Vigia`: settings, the dedicated instances' profiles,
  recovery drafts. The old folder is left untouched.
- The program is `Vigia.exe` in `%LOCALAPPDATA%\Programs\Vigia`; the installer is `Vigia-Setup-<version>.exe`.
- What 2.2.1 was going to bring is here too:
  - Search in the folders: a click on a result in the Visual or GitHub view now lands on that very match. The
    matches are counted in the file's text, and those views hide part of it (link addresses, tags): in a file with
    the words inside a link address, the click went to a later match. When the view holds another number of matches,
    the one taken is the one whose paragraph shares most words with that line of the file.
  - Search in the folders: in a line with the words twice, the second result shows the second one in bold (both
    showed the first).
  - Updates: a window whose close was cancelled during an update (its question answered *Cancel*), or one where the
    update did not go on, now hears the next update; before, it stopped listening after the first signal and the
    next update waited two minutes for it and gave up.
  - Updates: the installer's name is checked (letters, digits, dot, hyphen, underscore) before it is saved, and its
    address must be HTTPS.
  - The find bar no longer moves to the next match when its text is set to the same words from outside.

## 2.2.0 — 2026-11-20

- Search in the folders: a Search pane (View → Search in the folders, the toolbar's magnifier or Ctrl+Shift+F) looks
  for a word or a phrase in every Markdown file of the open folders, the same files the trees show. Results come
  grouped by file, each line with its number and the match in bold; a click opens the file at that very match, with
  the find bar showing it. Match case on demand; it searches a moment after the typing stops and stops at 300 files
  or 5,000 matches. It is a pane like Files and Outline: anywhere, by the layout button or by dragging its title.
- Three panes, any of them on either side, one above the other or side by side; each side keeps its own arrangement
  and the order of its panes, with a divider between each two.
- Each layout keeps its own sidebar width: going back to a layout brings it back as it was left. Widths set in 2.1
  are kept.
- Favourite folders, in the same list as the files and shown before them; a favourite folder opens like a tree.
- Favourites are reordered by dragging one onto another (locked tree or not); Move up / Move down stay.
- A RECENT block with the last files opened, under the favourites. Off by default: View → Recent files in the
  panel, or the panel's ⋯ menu.
- The profiles left behind by the self-test and the pictures for the manual are deleted at a later start.
- The pictures of the manual and the README were made again from a neutral sample.

## 2.1.0 — 2026-11-20

- Panes go anywhere. Files and Outline can each sit on the left or on the right of the text; on the same side,
  one above the other or side by side, in either order. Six layouts in all, up from three.
- A layout button at the right end of the toolbar, drawn as the current layout. A click opens a panel with every
  layout drawn: a click on one applies it at once; below, Files and Outline on or off and *Swap Files and Outline*.
  *View → Layout* has the same choices.
- Drag a pane by its title (FILES or CONTENTS) to an edge of the window. While dragging, both edges are outlined
  and the place the pane would take is filled in with its name; near the other pane, the upper part puts it above,
  the lower part below and the middle beside it. Letting go in the middle of the window, or Esc, changes nothing.
  The text is a browser window that WPF cannot draw over, so the guide is a see-through window of its own.
  A 2.0 layout is carried over as it was.
- Favourites. A star at the right of each file row (faint on hover, golden when on), right-click → *Add to
  favourites*, or *File → Favourite* for the open file. The favourites make one list for every open folder, the
  first block of the Files panel, with each file's folder in grey; they can be moved up or down, sorted A → Z,
  cleaned of files that are gone, or emptied. A favourite renamed or moved from the panel follows its file.
- Updates install themselves. *Help → Check for updates* (and the daily check, now once a day for all the
  dedicated instances) asks once — *Update now*, *What's new* or *Later* — and then: the installer is downloaded
  and checked against the release's `SHA256SUMS.txt` (one that does not match is deleted, never run); every
  Gryphmark window closes, saving first or asking as when closed by hand; the installer runs in silence keeping the
  install options, waits for the last window to be gone, and opens every window again with its profile and its
  file. A window that does not close stops the update, and the windows that had closed come back.
- `--render-chrome` uses a throw-away profile: its pictures no longer change the user's own settings.

## 2.0.0 — 2026-11-16

Windows and macOS now share one version number: this is 2.0.0 on both.

- Pictures work in more places. A picture in a parent folder (`../img/a.png`) and a link to a Markdown file
  in another folder (`[x](../other.md)`) now reach the file: until now the page saw only the document's own folder,
  and both pointed inside it. In the GitHub view a picture can also be given by an absolute path (`C:\Pics\a.png`,
  `\\server\share\a.png`, a `file:` address), with spaces in its path (`![a](my picture.png)`), as `![[picture.png]]`
  or `![[picture.png|300]]` (Obsidian; a name not beside the document is looked for in the open folders), and with a
  size as `![alt|300](a.png)`. A picture that cannot be found is a dashed box with its name instead of a broken icon,
  and a click on a picture opens it in its viewer. What the page may load: the document's folder, the folders of the
  Files panel when the document is in one of them, and pictures from anywhere; nothing else.
- Tables fit the column. A long path, address or piece of code inside a cell breaks where it has to, so the
  columns share the width instead of the table hiding behind a scrollbar; plain words stay whole. A table of eight
  columns or more takes less padding in its cells. Measured on 58 notes with such tables: in a 1200 px window, 6
  scrolled before and none now; in an 800 px one, 8 before and 3 now.
- Text width, in View → Text width: *Normal* (980 px, GitHub's column, as before), *Wide* (1280 px),
  *Very wide* (1600 px) or *The whole window*, for the Visual and GitHub views. Remembered per profile; printing and
  exported pages keep GitHub's width.
- Three layouts for the panels, in View → Layout: *Files above Outline* (as before), *Files and Outline side
  by side* and *Outline on the right of the text*. Each layout keeps its own widths.
- Rename and move from the Files panel: <kbd>F2</kbd> or right-click → *Rename*; with the tree unlocked (the
  padlock in the panel header) files and folders dragged onto a folder are moved there on disk. Two arrows undo and
  redo renames and moves (also Ctrl+Z / Ctrl+Y in the tree). The open document follows its file, saved first if it
  had changes, and so does *File → Open recent*. A name Windows would change or refuse (a final dot or space, CON,
  NUL, COM1…) is refused; a name already taken, or a folder into itself, too.
- Properties at the top of a file (front matter, between two `---` lines) are a grey folded box in the Visual
  view and a table under a folded *Properties* line in the GitHub view, saved exactly as written; `+++` (TOML) blocks
  too. A block counts only when it closes and holds a key: a first `---` that never closes, or a file that starts
  with a rule, keeps its text in sight in both views. The Outline skips the block and keeps `IMG_1915` as
  written.
- Pasting inside a code block no longer splits it: whatever is pasted there goes in as plain text.
- The window opens again where it was closed: same size, same place on the same monitor, maximized or not; a
  place no longer on any screen opens centred.

## 1.13.0 — 2026-11-14

- **[[name]] links**, as on the Mac: in the GitHub view `[[name]]`, `[[name|text]]` and `[[name#heading]]` open the
  Markdown file of that name in the open folders (the nearest one wins); a name that matches no file is shown in red.
- **Reading at the end, you follow the end**: when another program adds text at the bottom of the file and you were
  at the end of the view, the view goes down with it, as in a terminal. Anywhere else you stay where you were.
- A self-test in the real window, out of sight (`--self-test <log>`): per-monitor DPI, the sidebar, autosave and its
  switch, the live reload and its green marks, [[name]] links.

## 1.12.0 — 2026-11-13

- **What another program changed is marked in green** for a few seconds after a reload, in every view: new or
  rewritten paragraphs, and only the point or row added to a list or a table.
- **An edited line keeps all of itself but your change.** The visual editor rewrote the whole line in its own style
  when one letter of it changed: `\|` and `\.` in a line with a pipe, `*x*` for `_x_`, `\~` for `~`, and inline tags
  such as `<kbd>` were lost. Now only what you typed or deleted changes in the file.

## 1.11.0 — 2026-11-13

- **One sidebar, as on the Mac**: the Files panel above and the outline below, with a line between them that can be
  dragged to change their heights. Each one still shows and hides on its own (`Ctrl+Shift+E`, `Ctrl+Shift+O`); the
  one left on fills the sidebar. The sidebar's width and the split are kept. The Files panel is open the first time.
- **The questions name their answers**, as on the Mac: when another program changes a file with unsaved changes,
  *Reload* or *Keep my changes*; when closing, *Save*, *Don't save* or *Cancel* (they were Yes and No).
- **Sharp on a second screen**: the app follows the scale of each monitor (per-monitor DPI). Before, on a screen with
  another scale Windows stretched the whole window as a picture.
- Editing in the Visual view a paragraph that holds `[[name]]` links keeps them as written; it saved them as
  `\[\[name\]\]`.
- The editor page is now the same file as in the macOS version (checked on every commit), and WebView2 SDK
  1.0.4314.35.

## 1.10.0 — 2026-11-13

- **Autosave**, on by default: a file with a name saves itself 1.5 seconds after you stop typing. A switch in the
  toolbar, and File → Autosave, turn it on and off. It never writes over a change made by another program (the
  live reload asks first), and never a new untitled document: that one is saved by itself once it has a name.
  Closing a window or opening another file saves without asking when autosave is on.
- **The scrollbar of the Visual view is at the window's edge.** In a window wider than the 980 px text column
  (maximised, or dragged until Windows enlarged it) it stood at the edge of the column, with a wide empty margin to
  its right. The column is the same width as before.
- Saving takes the text and marks it saved in one step: a key pressed while a save was under way could be left out
  of the file and of the next save.
- The live reload tells our own saves apart by their text, not by the time since saving: a change written by
  another program in the two seconds after a save was ignored.

## 1.9.0 — 2026-11-12

- **Convert to TXT**: right-click a file in the Files panel. A .txt is written next to it as the document reads in the
  GitHub view: headings underlined, lists with bullets or numbers, tables in columns, links with their address. The
  Markdown file is not changed, and an existing .txt is never overwritten (`name (2).txt`).
- The outline can be made wider by dragging its right edge (160 to 600 px), as the Files panel already could; each
  window keeps its width.

## 1.8.2 — 2026-11-12

- A Markdown file with an HTML table whose `<tbody>` is empty (pandoc writes one for every one-row table of a Word
  document) opened as a blank page, with no message. The editor library (TOAST UI Editor 3.2.2) failed on that table;
  its code is now corrected in the bundle, and `build.ps1` corrects it again on every rebuild.
- If the editor library ever fails on a document again, the GitHub view still shows all of it, the visual and Markdown
  views say what happened instead of staying blank, and saving writes the file back exactly as it is on disk.
- The icon in the README and on the cover of the manuals is the gold one of the app; it was still the orange of
  1.4.0. `build.ps1 -Art` now writes it from the gold variant.

## 1.8.1 — 2026-11-10

- In the dark theme, list items and table headers in the visual view were almost black on the dark page; they now
  have the colour of the rest of the text.

## 1.8.0 — 2026-11-10

- **Safer documents:** the HTML inside a Markdown file is cleaned (DOMPurify) before the GitHub view and the HTML
  export show it, so a document can no longer run scripts in the editor page. Links open only web pages, e-mail,
  other Markdown files, pictures and PDFs; a link to any other file (a program, a script, a shortcut) shows it in
  Explorer instead of running it.
- **Live reload that keeps up:** a file changed by another program, such as an AI assistant, is reloaded in place,
  and every view keeps its scroll position. Saves that write a new file and rename it over the old one are now
  followed too, and a burst of changes always ends with the last version on screen. A file that another program
  holds open is read with shared access. Typing while a reload happens always marks the document modified, so
  the next change on disk asks before replacing it.
- **.NET 10:** Gryphmark now runs on .NET 10 (.NET 8 support ends on 10 November 2026). Builds carry no paths of the
  build machine.
- **Web libraries updated:** Mermaid 12.3, KaTeX 0.19.1 (also in the stylesheet that exported pages link to), jsdiff 9,
  and one copy of DOMPurify for the whole bundle;
  `npm audit` reports no known vulnerabilities (there were 14).
- x64 Windows only, stated in the README and the manuals; the About window and the manuals name the project page.
- Build: the version is written only in `Gryphmark.csproj`; `build.ps1 -Art` redraws the art, otherwise the committed
  art is used; any Chromium browser can print the manuals. Every source file carries the GPL-2.0-only header.

## 1.7.2 — 2026-11-10

- **Spell check follows the interface language:** switching between English and Spanish now also switches
  the spell check dictionary (English: en-US, Spanish: es-419); the open document, its unsaved changes and the
  zoom are kept. Before, the dictionary chosen the first time Gryphmark ran stayed for good.

## 1.7.1 — 2026-11-10

- **Task lists in the GitHub view:** check boxes are now clear, GitHub-style boxes (green with a white tick when done,
  in light and dark) instead of greyed-out disabled controls with a list bullet drawn on top of them. Done items
  are shown in a softer colour.

## 1.7.0 — 2026-11-10

- **Dedicated instances:** a file or folder can open in its own Gryphmark, always separate from the main window,
  with its own settings, open folders, recent files and window (`--profile "Name"`, kept under
  `%APPDATA%\Gryphmark\profiles\Name`; a new one starts with the main language, theme and colour). Opening it
  again brings that window to the front instead of starting another. Right-click a file or a folder title in the
  Files panel → *Open in a dedicated instance* or *Desktop shortcut (dedicated instance)*. The title bar shows the
  instance name.
- **Several windows done right:** an extra window (*Open in a new window*) no longer writes over the main
  window's session (open folders, window size); it only saves preference changes. Crash-recovery drafts of a
  window that is still running are never offered to another one.

## 1.6.0 — 2026-11-10

- **Several folders at once** in the Files panel, one tree per folder, added in the order they are opened. Each
  folder is a block with its own pastel colour and a stripe on the left: by default tones of the theme colour,
  or *Multicolour*, or *No colour*, and any folder can get its own colour from its right-click menu.
- **Order** from the panel's ⋯ button: order opened (default), name A → Z, name Z → A, recently modified first;
  *Move up / Move down* in the right-click menu.
- **Pin to top**: pinned folders always stay first.
- **Folders at start** (*File → Folders at start*, also in the ⋯ button): reopen the last session's folders
  (default), start clean, or start clean with only the pinned folders.
- **Sturdier when things disappear**: a deleted open folder or a lost network drive is shown as *not available*
  and comes back by itself; a file deleted in Explorer leaves the tree at once, and clicking a stale entry just
  refreshes the panel. Watcher errors and buffer overflows trigger a refresh instead of being lost.
- **Narrow windows**: the formatting buttons that do not fit go into a ⋯ menu instead of sliding under the view
  and read-only switches.
- **Zoom bar** in the status bar: −, slider, +, and the percentage (click it for 100 %); Ctrl + mouse wheel works
  over the whole window.
- The default theme colour is now **dark gold**. A colour saved by 1.5 without being chosen gives way to it.

## 1.5.1 — 2026-11-10

- The theme colour now reaches the desktop and Start-menu shortcuts at once: Windows caches icons by file
  path, so each colour has its own icon file and the shortcuts and file-type entries are pointed at it.

## 1.5.0 — 2026-11-10

- **Themes** (*View → Theme…*): **Light**, **Dark** or **Automatic** (follows the Windows app mode and switches
  live), plus a catalogue of eleven colours. The colour is the background of the icon — window, taskbar,
  desktop and Start-menu shortcuts, `.md` files and the folder menu — and the highlight of the app (folders in
  the Files panel, check marks, the selected item). Shortcuts with an icon of their own are left alone.
- The default colour became Advent purple (1.6.0 changes it to dark gold); the original orange is in the catalogue. New installs start in
  Automatic; existing ones keep their light or dark choice.

## 1.4.0 — 2026-11-10

- **Read only:** a new *Edit | Read only* switch next to the view buttons (also *View → Read only*,
  `Ctrl+Shift+L`) locks the text in the Visual and Markdown views, so a stray key, paste, drop or click on a
  task box cannot change the document. Reading, selecting, copying, finding and switching views still work;
  typing shows a hint in the status bar. The choice is remembered.
- **Hiding the sample is now visible:** *File → Open recent* shows a **Hide sample** item right below it
  (before, it was only a right-click inside the submenu), and *Help → Show sample in Open recent* turns it
  off and on again.

## 1.3.0 — 2026-11-10

- **Files panel** (`Ctrl+Shift+E`, the folder button, or *View → Files panel*): open a whole folder and
  browse its subfolders and Markdown files in a tree on the left, like VS Code or Obsidian. A click opens a
  file beside the tree; the panel follows changes on disk, marks the open file and can be resized.
  Hidden folders and tool folders (`.git`, `node_modules`, `bin`, `obj`…) are left out.
- Open a folder with *File → Open folder…*, by dropping it on the window, from the command line, or with
  the new optional installer task *Open with Gryphmark* in the Explorer context menu of folders.
- The last folder is reopened on the next start; recent folders are listed in *File → Open recent*.
- Right-click menu in the tree: open, open in a new window, show in folder, copy path.

## 1.2.1 — 2026-11-10

- Sample document installed with the program (English and Spanish, hidden file in `samples\`): it opens on
  the first start, from **Help → Sample document**, and pinned at the top of **File → Open recent**, where a
  right-click hides it. It opens as an untitled copy, so the installed file is never overwritten, and it
  switches language together with the interface.
- Closing hides the window at once; the editor is torn down behind it.
- The daily update check is remembered even when it fails, so it no longer runs on every start.
- Diagnostics: `GM_TRACE_CLOSE=<log>` times each shutdown step.

## 1.2.0 — 2026-11-10

- **Renamed from Quillforge to Gryphmark**, after the griffin in the icon. Program, installer, manuals,
  settings folder (`%APPDATA%\Gryphmark`) and file association use the new name.
- The installer removes a previous Quillforge install; settings, recent files and recovery drafts are
  copied over on first start (the old folder is left untouched).

## 1.1.1 — 2026-11-10

- New About window in the app style: icon, version, author, GPL-2.0 license with a link to the full text,
  complete credits (highlight.js and jsdiff are BSD-3-Clause) and a *Copy info* button. English and Spanish.
- The Visual / Markdown / GitHub buttons now explain in their tooltip what each mode is for.
- Manual: the modes are named as on the buttons; About documented; Spanish screenshots.

## 1.1.0 — 2026-11-10

- Find & replace bar (`Ctrl+F`, `Ctrl+H`, `F3` / `Shift+F3`, match case).
- Outline sidebar with the document headings (`Ctrl+Shift+O`).
- Formatting is preserved on save: only edited lines change; list markers, emphasis style,
  table alignment, CRLF/LF and BOM are kept.
- GitHub extras: alerts (`[!NOTE]` … `[!CAUTION]`), Mermaid diagrams, KaTeX math, footnotes, emoji shortcodes.
- Reloads the file when another program changes it (asks first if there are unsaved changes).
- Crash recovery: unsaved work is backed up every 20 seconds and offered on the next start.
- Spell check (View menu).
- Single instance: opening a `.md` brings the running window forward (`File → New window` still opens another).
- Daily update check against GitHub releases (Help → Check for updates).
- Native, Apple-style toolbar and menus; refreshed light/dark palette.
- Installer shows and installs the license (GPL-2.0-only). Optional code signing in `build.ps1`.

## 1.0.0 — 2026-11-10

- First release: visual editor, Markdown source with live preview and GitHub preview; English/Spanish UI;
  light/dark theme; export to HTML; print/PDF; recent files; per-user installer with `.md` association; manuals EN/ES.
