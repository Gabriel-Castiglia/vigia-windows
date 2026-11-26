<p align="center"><img src="assets/vigia.png" width="140" alt="Vigía"></p>

<h1 align="center">Vigía</h1>
<p align="center"><b>Write Markdown visually. See it like GitHub.</b><br><i>Escribí Markdown en forma visual. Miralo como en GitHub.</i><br><i>Markdown を見たまま書いて、GitHub と同じ見た目で読む。</i></p>
<p align="center"><i>Until version 2.2 it was called Gryphmark. · Hasta la versión 2.2 se llamaba Gryphmark.</i></p>

<p align="center"><img src="docs/img/visual_light_en.png" width="520" alt="The visual editor"></p>

> **Windows 10/11 on x64 (64-bit Intel or AMD) only.** There is no build for ARM processors.
> Solo para Windows 10/11 en x64 (Intel o AMD de 64 bits); no hay versión para procesadores ARM.

---

## English

Vigía is a Markdown viewer and editor for Windows. Format with buttons — headings, bold, lists, tables, code — and
Vigía writes the Markdown tags for you. Documents are rendered exactly like GitHub, in light or dark theme.

**Live reload.** When another program changes the open file, such as an AI assistant editing it, Vigía shows
the new text at once and stays where you were reading, and marks what changed in green for a few seconds. It also
follows tools that save by writing a new file and renaming it over the old one.

**Download:** the setup is in [Releases](https://github.com/Gabriel-Castiglia/vigia-windows/releases/latest).

**macOS version:** <https://github.com/Gabriel-Castiglia/vigia> (Apple silicon only).

- Visual editor, Markdown source with live preview, and read-only GitHub preview (`Ctrl+1/2/3`)
- English, Spanish and Japanese interface, switchable at runtime
- **Tabs**: several documents in one window, each with its own undo history and scroll position (`Ctrl+N` new tab, `Ctrl+W` closes one, `Ctrl+Tab` / `Ctrl+PageDown` switch, `Ctrl+Shift+T` reopens the last one closed; drag a tab to move it, middle-click closes it). Closing the window closes them all; with *File → Reopen tabs at start* the next start opens the same files again
- Relative images and links, also in other folders (`../`); `.md` links open in Vigía
- Pictures by absolute path or `file:` address, with spaces in the path, as `![[picture.png|300]]` (Obsidian, looked for in the open folders) or `![alt|300](a.png)`; a missing one shows its name in a dashed box, a click opens it in its viewer
- Tables fit the column: a long path or piece of code in a cell breaks where it has to instead of pushing the table behind a scrollbar
- GitHub alerts, Mermaid diagrams, math, footnotes and emoji
- **Files panel**: open a whole folder and browse its Markdown files in a tree beside the document, like VS Code or Obsidian (`Ctrl+Shift+E`); several folders at once, each with its own colour, pinning, sorting and a choice of what opens at start
- **Read only**: the *Edit | Read only* switch in the toolbar locks the text so nothing changes by mistake (`Ctrl+Shift+L`)
- **Themes**: light, dark or automatic (follows Windows), and a colour catalogue that changes the icon everywhere in Windows and the highlights of the app (*View → Theme…*); sky blue by default
- **Dedicated instances**: open a file or folder in its own Vigía, separate from the main window (`--profile "Name"`, or right-click in the Files panel → desktop shortcut)
- **Zoom bar** in the status bar (also `Ctrl` + mouse wheel); on a narrow window the buttons that do not fit go into a ⋯ menu
- Find & replace; spell check in English, Spanish (also the Spanish of Uruguay, with the voseo) and Japanese at once (mixed-language text), with suggestions, *Ignore* and *Add to dictionary*; a sidebar with the files above and the outline below, as on the Mac (drag its edge and the line between them)
- **Layouts**: the files and the outline go on either side of the text, one above the other or side by side; pick one from the drawings behind the layout button of the toolbar (or *View → Layout*), or drag a pane by its title to an edge of the window. *View → Text width* widens the column up to the whole window
- **Favourites**: a star on each file or folder row puts it in one list for all the open folders, at the top of the Files panel (drag to reorder); an optional RECENT block below it
- **Search in the folders** (`Ctrl+Shift+F`): every Markdown file of the open folders, results by file and line, a click opens the file at the match; a pane you place like the others
- **New Markdown file** and **New folder**: the first two buttons of the toolbar (or right-click a folder) make them on disk at once, in the folder selected in the tree, or in `Documents\Vigia` with nothing selected, and show them there; a new file opens at once (with the tree unlocked)
- **Delete** files and folders from the Files panel (right-click or `Del`, with the tree unlocked): to the Recycle Bin, after a question that can be silenced for the rest of that window
- Rename and move files and folders from the Files panel (`F2`, or drag them with the padlock open), with undo and redo; the open document follows its file
- Properties at the top of a file (front matter) shown as a folded box and as a table, saved as written
- **Convert to TXT**: right-click a file in the Files panel; a .txt is written next to it, as the document reads in the GitHub view, and the Markdown file is not changed
- **Autosave**, on by default: a file with a name saves itself a moment after you stop typing; the *Autosave* switch in the toolbar (also *File → Autosave*) turns it off. It never writes over a change made by another program
- Keeps your formatting: only what you edit changes on save, down to the part of a line you touched (CRLF/BOM preserved)
- **[[name]] links** in the GitHub view open the Markdown file of that name in the open folders, as in a wiki (a missing one is red), as on the Mac
- Links in a document open only web pages, e-mail, other Markdown files, pictures and PDFs; a link to any other file shows it in Explorer instead of running it
- Crash recovery, single instance, and **updates that install themselves** (checked against the release's SHA-256, every open window reopened afterwards)
- Export to self-contained HTML, print / save as PDF
- Per-user installer (no admin rights), optional `.md` association
- Manuals: [`docs/Vigia_Manual_EN.pdf`](docs/Vigia_Manual_EN.pdf)

**Build** (on Windows x64): .NET 10 SDK or later, Node.js with npm, Inno Setup 6 and a Chromium browser (Edge, Chrome or
Brave) to print the manuals. The version is written only in `<Version>` of `src/Vigia/Vigia.csproj`.

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
# -> out/Vigia-Setup-<version>.exe
```

`-SkipNpm` keeps the web bundle in `src/Vigia/wwwroot/lib` as it is. `-Art` redraws the icons and the installer
pictures, which are committed; it needs Python 3 with Pillow, numpy, scipy and opencv-python.
The versions used for the released build are listed under *Build tools* below.

Up to version 1.1.1 Vigía was called Quillforge. The new installer removes the old version and
the settings are carried over.

## Español

Vigía es un visor y editor de Markdown para Windows. Das formato con botones —títulos, negrita, listas, tablas,
código— y Vigía escribe las etiquetas de Markdown por vos. Los documentos se ven exactamente como en GitHub, con
tema claro u oscuro.

**Recarga en vivo.** Cuando otro programa cambia el archivo abierto, por ejemplo un asistente de IA que lo edita,
Vigía muestra el texto nuevo enseguida y no te mueve de donde estabas leyendo. También sigue a las herramientas
que guardan escribiendo un archivo nuevo y renombrándolo encima del viejo.

**Descarga:** el instalador está en [Releases](https://github.com/Gabriel-Castiglia/vigia-windows/releases/latest).

**Versión para macOS:** <https://github.com/Gabriel-Castiglia/vigia> (solo Apple silicon).

- Editor visual, código Markdown con vista previa en vivo y vista GitHub de solo lectura (`Ctrl+1/2/3`)
- Interfaz en inglés, español y japonés, se cambia sin reiniciar
- **Pestañas**: varios documentos en una ventana, cada uno con su propio deshacer y su posición (`Ctrl+N` pestaña nueva, `Ctrl+W` cierra una, `Ctrl+Tab` / `Ctrl+AvPág` cambian, `Ctrl+Shift+T` reabre la última cerrada; se mueven arrastrándolas y el clic del medio las cierra). Cerrar la ventana las cierra todas; con *Archivo → Reabrir las pestañas al iniciar*, el próximo arranque vuelve a abrir los mismos archivos
- Imágenes y enlaces relativos, también en otras carpetas (`../`); los enlaces a `.md` se abren en Vigía
- Imágenes con ruta absoluta o dirección `file:`, con espacios en la ruta, como `![[imagen.png|300]]` (Obsidian, se buscan en las carpetas abiertas) o `![alt|300](a.png)`; una que falta muestra su nombre en un recuadro punteado, y un clic la abre en su visor
- Las tablas entran en la columna: una ruta o un código largo dentro de una celda se corta donde hace falta, en vez de mandar la tabla detrás de una barra de desplazamiento
- Avisos de GitHub, diagramas Mermaid, fórmulas, notas al pie y emojis
- **Panel de archivos**: abrís una carpeta entera y recorrés sus archivos Markdown en un árbol al costado del documento, como en VS Code u Obsidian (`Ctrl+Shift+E`); varias carpetas a la vez, cada una con su color, fijado, orden y qué se abre al iniciar
- **Solo lectura**: el selector *Editar | Solo lectura* de la barra bloquea el texto para que nada cambie por error (`Ctrl+Shift+L`)
- **Temas**: claro, oscuro o automático (sigue a Windows), y un catálogo de colores que cambia el ícono en todo Windows y el resaltado de la app (*Ver → Tema…*)
- **Instancias dedicadas**: abrir un archivo o una carpeta en su propio Vigía, separado de la ventana principal (`--profile "Nombre"`, o clic derecho en el panel de archivos → acceso directo en el escritorio)
- **Barra de zoom** en la barra de estado (también `Ctrl` + rueda del mouse); con la ventana angosta, los botones que no entran van a un menú ⋯
- Buscar y reemplazar; corrector ortográfico en inglés, español y japonés a la vez (textos con idiomas mezclados), con sugerencias, *Omitir* y *Agregar al diccionario*; una barra lateral con los archivos arriba y el esquema abajo, como en la Mac (se arrastran su borde y la línea entre los dos)
- **Disposición**: los archivos y el esquema van a cualquiera de los dos lados del texto, uno arriba del otro o lado a lado; se elige con los dibujos del botón de disposición de la barra (o *Ver → Disposición*), o arrastrando un panel de su título hasta un costado de la ventana. *Ver → Ancho del texto* ensancha la columna hasta toda la ventana
- **Favoritos**: una estrella en cada archivo o carpeta lo suma a una sola lista para todas las carpetas abiertas, arriba del panel de archivos (se reordena arrastrando); debajo, un bloque RECIENTES opcional
- **Buscar en las carpetas** (`Ctrl+Shift+F`): todos los archivos Markdown de las carpetas abiertas, resultados por archivo y línea, un clic abre el archivo en la coincidencia; un panel que se ubica como los otros
- **Nuevo archivo Markdown** y **Nueva carpeta**: los dos primeros botones de la barra (o clic derecho sobre una carpeta) los crean en el disco en el acto, en la carpeta seleccionada en el árbol, o en `Documentos\Vigia` si no hay nada seleccionado, y los muestran ahí; el archivo nuevo se abre enseguida (con el árbol desbloqueado)
- **Eliminar** archivos y carpetas desde el panel de archivos (clic derecho o `Supr`, con el árbol desbloqueado): a la Papelera de reciclaje, después de una pregunta que se puede callar por el resto de esa ventana
- Cambiar nombre y mover archivos y carpetas desde el panel de archivos (`F2`, o arrastrándolos con el candado abierto), con deshacer y rehacer; el documento abierto sigue a su archivo
- Propiedades al principio de un archivo (front matter) como un recuadro plegado y como una tabla, guardadas tal como estaban
- **Convertir a TXT**: clic derecho en un archivo del panel de archivos; se escribe un .txt al lado, como se lee el documento en la vista GitHub, y el archivo Markdown no cambia
- Autoguardado, activado de entrada: un archivo con nombre se guarda solo un momento después de que dejás de escribir; el interruptor *Autoguardado* de la barra (también *Archivo → Autoguardado*) lo apaga. Nunca escribe encima de un cambio hecho por otro programa
- Respeta tu formato: al guardar solo cambia lo que editaste, hasta la parte de la línea que tocaste (conserva CRLF/BOM)
- Enlaces `[[nombre]]` en la vista GitHub: abren el archivo Markdown de ese nombre en las carpetas abiertas, como en una wiki (uno que falta se ve en rojo), como en la Mac
- Los enlaces de un documento solo abren páginas web, correo, otros archivos Markdown, imágenes y PDF; un enlace a cualquier otro archivo lo muestra en el Explorador en vez de ejecutarlo
- Recuperación ante cierres, instancia única y **actualizaciones que se instalan solas** (verificadas contra el SHA-256 de la versión publicada; las ventanas abiertas se vuelven a abrir al terminar)
- Exportar a HTML autónomo, imprimir o guardar como PDF
- Instalador por usuario (sin permisos de administrador), asociación opcional de `.md`
- Manuales: [`docs/Vigia_Manual_ES.pdf`](docs/Vigia_Manual_ES.pdf)

**Compilar** (en Windows x64): SDK de .NET 10 o posterior, Node.js con npm, Inno Setup 6 y un navegador Chromium (Edge, Chrome
o Brave) para imprimir los manuales. La versión se escribe solo en `<Version>` de `src/Vigia/Vigia.csproj`.

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
# -> out/Vigia-Setup-<versión>.exe
```

`-SkipNpm` deja como está el paquete web de `src/Vigia/wwwroot/lib`. `-Art` vuelve a dibujar los íconos y las
imágenes del instalador, que ya están en el repositorio; necesita Python 3 con Pillow, numpy, scipy y opencv-python.
Las versiones con que se compiló la versión publicada están abajo, en *Build tools*.

Hasta la versión 1.1.1 Vigía se llamaba Quillforge. El instalador nuevo quita la versión anterior y la
configuración se copia sola.

## 日本語

Vigía は Windows 用の Markdown ビューアー兼エディターです。見出し、太字、リスト、表、コードなどの書式をボタンで付けると、
Markdown のタグは Vigía が書きます。文書は GitHub とまったく同じように、ライトまたはダークのテーマで表示されます。

**リアルタイムの再読み込み。** AI アシスタントなどほかのプログラムが開いているファイルを変更すると、Vigía は新しいテキストをすぐに表示し、
読んでいた位置はそのままで、変わった部分を数秒間緑色で示します。新しいファイルに書いてから古いファイルの上に名前を変えて保存するツールにも追従します。

**ダウンロード:** インストーラーは [Releases](https://github.com/Gabriel-Castiglia/vigia-windows/releases/latest) にあります。

- ビジュアル エディター、リアルタイム プレビュー付きの Markdown ソース、読み取り専用の GitHub プレビュー（`Ctrl+1/2/3`）
- 英語、スペイン語、日本語の画面。再起動せずに切り替えられます
- **タブ**: 1 つのウィンドウで複数の文書を、それぞれ自分の元に戻す履歴とスクロール位置付きで（`Ctrl+N` 新しいタブ、`Ctrl+W` 閉じる、`Ctrl+Tab` / `Ctrl+PageDown` 切り替え、`Ctrl+Shift+T` 最後に閉じたタブを開き直す。ドラッグで移動、中クリックで閉じる）。ウィンドウを閉じるとすべて閉じます。*ファイル → 起動時にタブを復元* をオンにすると、次回の起動時に同じファイルが開きます
- 相対パスの画像とリンク（別のフォルダー `../` も可）。`.md` へのリンクは Vigía で開きます
- 絶対パスや `file:` アドレスの画像（パスに空白があっても可）、`![[picture.png|300]]`（Obsidian 形式。開いているフォルダーから探します）や `![alt|300](a.png)`。見つからない画像は名前を点線の枠で示し、クリックでビューアーで開きます
- 表は列に収まります: セル内の長いパスやコードは必要な所で折り返し、表がスクロール バーの陰に隠れません
- GitHub のアラート、Mermaid の図、数式、脚注、絵文字
- **ファイル パネル**: VS Code や Obsidian のように、フォルダーを丸ごと開いてその中の Markdown ファイルを文書の横のツリーでたどれます（`Ctrl+Shift+E`）。複数のフォルダーを同時に開け、それぞれに色、固定、並べ替え、起動時に開くものの設定があります。1 つのボタンですべてのフォルダーを折りたたみ、すべて閉じているときはすべて展開します
- **読み取り専用**: ツール バーの *編集 | 読み取り専用* で、誤って変更しないようにテキストをロックします（`Ctrl+Shift+L`）
- **テーマ**: ライト、ダーク、自動（Windows に合わせる）と、Windows 全体のアイコンとアプリの強調表示の色を変える色のカタログ（*表示 → テーマ…*）。既定はスカイブルー
- **専用インスタンス**: ファイルやフォルダーを、メイン ウィンドウとは別の専用の Vigía で開けます（`--profile "名前"`、またはファイル パネルで右クリック → デスクトップにショートカット）
- ステータス バーの **ズーム バー**（`Ctrl` + マウス ホイールでも可）。ウィンドウが狭いと、入らないボタンは ⋯ メニューに入ります
- 検索と置換。英語・スペイン語・日本語を同時に調べるスペル チェック（複数言語の混在に対応、候補・無視・辞書に追加）。上にファイル、下に目次のサイド バー（端と 2 つの間の線をドラッグできます）
- **レイアウト**: ファイルと目次は本文のどちら側にも、上下にも左右にも置けます。ツール バーのレイアウト ボタンの図（または *表示 → レイアウト*）から選ぶか、パネルのタイトルをウィンドウの端までドラッグします。*表示 → 本文の幅* で列をウィンドウ全体まで広げられます
- **お気に入り**: ファイルやフォルダーの行の星で、開いているすべてのフォルダーに共通の一覧に加え、ファイル パネルの先頭に表示します（ドラッグで並べ替え）。その下に任意の「最近使ったファイル」ブロック
- **フォルダー内を検索**（`Ctrl+Shift+F`）: 開いているフォルダーのすべての Markdown ファイルを検索し、結果はファイルと行ごと。クリックで一致箇所を開きます。ほかと同じように配置できるパネルです
- **新しい Markdown ファイル**と**新しいフォルダー**: ツール バーの最初の 2 つのボタン（またはフォルダーの右クリック）で、ツリーで選んだフォルダー（何も選んでいなければ `ドキュメント\Vigia`）にすぐ作られ、ツリーに表示されます。新しいファイルはすぐに開きます（ツリーのロック解除時のみ）
- ファイル パネルでファイルとフォルダーを**削除**（右クリックまたは `Del`。ツリーのロック解除時のみ）: 確認のあとごみ箱へ移動。確認はそのウィンドウの間だけ止められます
- ファイル パネルでファイルとフォルダーの名前の変更と移動（`F2`、または錠前を開けてドラッグ）。元に戻す / やり直しができ、開いている文書はファイルに付いていきます
- ファイル先頭のプロパティ（front matter）は折りたたんだ枠と表で表示し、書かれたとおりに保存します
- **TXT に変換**: ファイル パネルでファイルを右クリック。GitHub 表示での見え方どおりの .txt を隣に書き出し、Markdown ファイルは変えません
- **自動保存**（既定でオン）: 名前のあるファイルは、入力が止まって少しすると自動で保存されます。ツール バーの *自動保存* スイッチ（*ファイル → 自動保存* でも可）でオフにできます。ほかのプログラムが加えた変更を上書きすることはありません
- 書式はそのまま: 保存で変わるのは編集した部分だけで、触れた行の一部分までに限られます（CRLF/BOM も維持）
- GitHub 表示の **[[名前]] リンク** は、wiki のように、開いているフォルダーにあるその名前の Markdown ファイルを開きます（見つからないものは赤）
- 文書内のリンクで開くのは Web ページ、メール、ほかの Markdown ファイル、画像、PDF だけです。それ以外のファイルへのリンクは、実行せずにエクスプローラーで表示します
- 異常終了からの復元、単一インスタンス、**自動でインストールされる更新**（公開版の SHA-256 で照合し、終わると開いていたウィンドウをすべて開き直します）
- 自己完結した HTML へのエクスポート、印刷 / PDF として保存
- ユーザーごとのインストーラー（管理者権限は不要）、`.md` の関連付けは任意
- マニュアル: [`docs/Vigia_Manual_JA.pdf`](docs/Vigia_Manual_JA.pdf)

**ビルド** の手順と必要なツールは、上の English の節を参照してください。


| Path | Content |
|---|---|
| `src/Vigia/` | WPF + WebView2 app (C#) and `wwwroot/` editor page |
| `_npm/` | Web dependencies and esbuild entry (`entry.js`) |
| `installer/` | Inno Setup script and wizard art |
| `tools/` | Icon, wizard art and manual generators; `jstest.py`, a headless test of the editor page |
| `docs/` | Manuals (HTML + PDF, EN/ES/JA) and screenshots |
| `out/` | Build output: publish folder and installer |

## Build tools / Herramientas de compilación

Vigía 3.5.1 was built and tested with / se compiló y probó con:

| Tool | Version |
|---|---|
| Windows 11 Pro | x64 |
| .NET SDK / runtime (self-contained, net10.0-windows) | 11.0.100 / 10.0.15 |
| Microsoft.Web.WebView2 (NuGet) | 1.0.4370.26 |
| WeCantSpell.Hunspell (NuGet) | 7.0.1 |
| Node.js / npm (web bundle; not rebuilt for 3.4.0 to 3.5.1, the libraries did not change) | 26.12.1 / 11.20.0 |
| esbuild | 0.25.12 |
| Inno Setup | 6.7.3 |
| Microsoft Edge (manual PDFs) | 158.0.4370.33 |
| Python (`-Art` only; not used for 3.4.0 to 3.5.1, the art did not change) | 3.13.17, Pillow 12.3.0, numpy 2.5.4, scipy 1.18.1, opencv-python 5.0.0 |

Web libraries / Bibliotecas web (exact versions in `_npm/package-lock.json`): TOAST UI Editor 3.2.3,
marked 15.0.12, KaTeX 0.19.1, Mermaid 12.3.0, highlight.js 11.12.0, jsdiff 9.0.0, DOMPurify 3.5.0,
github-markdown-css 5.9.0, gemoji 8.1.0.

## License / Licencia

Copyright (C) 2026 Gabriel Castiglia.

Vigía is free software: you can redistribute it and/or modify it under the terms of the GNU General
Public License, version 2 only (GPL-2.0-only), as published by the Free Software Foundation. It is
distributed WITHOUT ANY WARRANTY. See [`LICENSE`](LICENSE) for the full text.

Vigía es software libre: podés redistribuirlo y/o modificarlo bajo los términos de la GNU General
Public License, versión 2 únicamente (GPL-2.0-only). Se distribuye SIN NINGUNA GARANTÍA. El texto completo
está en [`LICENSE`](LICENSE).

## Credits / Créditos

| Component | License |
|---|---|
| TOAST UI Editor, ProseMirror, marked (+ alert, footnote, katex extensions), github-markdown-css, Mermaid, KaTeX, gemoji | MIT |
| highlight.js, jsdiff | BSD-3-Clause |
| DOMPurify | MPL-2.0 (used under the MPL-2.0 option of its MPL-2.0 / Apache-2.0 dual licence) |
| .NET 10 / WPF | MIT |
| WeCantSpell.Hunspell (spell check) | MIT |
| English dictionary (`dict/en.*`, SCOWL / Hunspell en_US 2020.12.07 with LibreOffice's 2024 apostrophe fix) | MIT-like / BSD, see `dict/LICENSE-en.txt` |
| Spanish dictionaries (`dict/es.*`, `dict/es-UY.*`, RLA-ES 2.9) | GPL-3.0+ / LGPL-3.0+ / MPL-1.1+, see `dict/LICENSE-es.txt` |
| Microsoft Edge WebView2 SDK | Microsoft WebView2 SDK license (redistributable); the WebView2 Runtime is a Windows system component |
| Icon / ícono: lighthouse from Debenne's design for a lighthouse (c. 1815), The Metropolitan Museum of Art | CC0 |
