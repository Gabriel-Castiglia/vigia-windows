# Registro de cambios

[English](CHANGELOG.md) · Español · [日本語](CHANGELOG.ja.md)

## 3.5.3 — 2026-11-27

- **Ícono nuevo**: un faro dibujado plano, en crema con contorno negro sobre el color del tema, con sus rayas, la
  galería, la linterna encendida y dos haces de luz. El grabado al que reemplaza, en tamaño chico, parecía una vela.
  El ícono sigue el color elegido en *Ver → Tema*, en los doce colores, y también lo usan las imágenes del instalador.
  El dibujo está en `assets/icon.svg`; los scripts que armaban el ícono viejo desde el grabado ya no están.

## 3.5.2 — 2026-11-26

- **Favoritos: vuelve clic derecho → *Subir* / *Bajar*.** Desde la 3.2.1 el menú de clic derecho de una fila se arma
  cuando se abre, y las dos entradas de un favorito se agregaban al menú que se descartaba; la lista solo se podía
  reordenar arrastrando.
- **Eliminar un archivo cierra todas sus pestañas**, también una reabierta al iniciar (*Archivo → Reabrir las
  pestañas al iniciar*) que todavía no se había mostrado: quedaba abierta y, al mostrarla, decía que el archivo ya no
  existía.
- **Los atajos de la ventana son los de la página**: con el foco en el panel de archivos, en otro panel o en el menú,
  `Ctrl+Mayús+O` mostraba el diálogo Abrir en vez del índice, y `Ctrl+F`, `Ctrl+H`, `Ctrl++`, `Ctrl+-` y `Ctrl+0` no
  hacían nada.
- La revisión de los enlaces `[[nombre]]` y la búsqueda de una `![[imagen]]` en las carpetas abiertas le contestan a
  la pestaña que preguntó, aunque para entonces se muestre otra.
- **El instalador también está en japonés**, como anunciaba la 3.2.0 (solo ofrecía inglés y español), y pone el manual
  en japonés en el menú Inicio.
- *Acerca de Vigía* dice «diccionarios» en el idioma de la interfaz.
- Los manuales de usuario usan los colores de la app (celeste sobre el gris claro de la ventana), con el ícono en la
  tapa y junto al contenido. Describen la barra de herramientas completa, el botón Búsqueda del panel de disposición
  y las entradas nuevas del menú de clic derecho de una carpeta, y nombran a Gryphmark entre los nombres anteriores; el
  manual en español dice *índice*, como los menús, y lista todos los atajos.
- README y registro de cambios en inglés, español y japonés.
- Limpieza del código sin cambios de comportamiento: los diálogos comparten un mismo marco, se unificaron funciones
  repetidas, se quitaron opciones y funciones de la página que nadie usaba, y se revisaron todos los comentarios.

## 3.5.1 — 2026-11-26

- **Nuevo archivo, nueva carpeta y eliminar, solo con el árbol desbloqueado** (el candado de arriba del panel de
  archivos, como para arrastrar): mientras está bloqueado, los dos botones de la barra y las entradas *Nuevo archivo
  Markdown…*, *Nueva carpeta…* y *Eliminar* del menú de clic derecho quedan en gris, y la tecla Supr no hace nada. Al
  dejar el puntero sobre una de ellas avisa que hay que desbloquear el árbol para habilitarla. La ayuda del candado
  nombra las tres cosas que desbloquea.
- **Una carpeta de red o un pendrive no tienen Papelera de reciclaje**: ahí eliminar es del todo, y ahora la pregunta
  lo dice (*¿Eliminar … del todo?*), siempre, aun después de *No volver a preguntar en esta ventana*. La 3.5.0 decía
  que el elemento iba a la Papelera también ahí.
- Acerca de Vigía da crédito al corrector ortográfico: WeCantSpell.Hunspell y sus diccionarios (SCOWL en_US, RLA-ES),
  que llegaron con la 3.3.0 pero faltaban en esa ventana.

## 3.5.0 — 2026-11-26

- **Eliminar archivos y carpetas** desde el panel de archivos: clic derecho → *Eliminar*, o la tecla Supr sobre la
  fila seleccionada. Van a la Papelera de reciclaje, de donde se pueden recuperar (una carpeta, con todo lo que tiene
  adentro). Primero viene una pregunta, con *No volver a preguntar en esta ventana*: marcada, esa ventana deja de
  preguntar hasta que se cierra; una ventana nueva, o Vigía abierto de nuevo, vuelve a preguntar. Las pestañas de lo
  eliminado se cierran (sin guardar: su texto está en la Papelera), y sale de Favoritos y de Abrir reciente. Las
  carpetas abiertas mismas se cierran desde su menú, como antes; nunca se eliminan desde el árbol.

## 3.4.0 — 2026-11-26

- **Nuevo archivo Markdown y Nueva carpeta**, los dos primeros botones de la barra, y dos entradas del menú de clic
  derecho de cada carpeta del panel de archivos. Los dos se crean en el disco en el acto, en la carpeta seleccionada
  en el árbol (la del archivo seleccionado, si hay un archivo seleccionado); sin nada seleccionado, en
  `Documentos\Vigia`, que se crea y se agrega al panel la primera vez. Se ofrece un nombre libre (*Documento
  nuevo.md*, *Documento nuevo 2.md*…); a un archivo sin extensión Markdown se le agrega `.md` y empieza con su nombre
  como título. El archivo nuevo se abre enseguida; una carpeta nueva queda seleccionada, así el próximo archivo va
  adentro. Nunca se escribe encima de nada: un nombre ocupado, o uno que Windows cambiaría o rechazaría, se rechaza con
  un mensaje.
- La fila seleccionada en el panel de archivos sigue seleccionada cuando el árbol se reconstruye (un archivo guardado,
  otro programa que escribe en la carpeta): antes, la selección volvía cada vez al archivo abierto.
- Los botones que muestran paneles (archivos, índice, búsqueda) y los dos nuevos andan en cualquier modo: en la vista
  GitHub y en solo lectura toda la barra quedaba en gris; ahora solo los botones de formato.
- El Narrador lee los botones de la barra por su nombre (leía el carácter de su ícono) y las filas del panel de
  archivos (no leía nada).

## 3.3.1 — 2026-11-25

- **Español del Uruguay** en el corrector (`es-UY`, con el voseo: *tenés*, *querés*, *mirá*), activado de entrada
  junto al español general; una lista de idiomas guardada por la 3.3.0 lo recibe una vez (si se saca a mano, queda
  afuera). El diccionario de español general marcaba en rojo todos los verbos del voseo.
- **Diccionarios más nuevos**: el español pasa de RLA-ES 2.8 a 2.9 (más palabras, *Montevideo* entre ellas), y el
  inglés toma el arreglo de LibreOffice de enero de 2024, así *don’t* e *it’s* escritos con el apóstrofo tipográfico
  están bien. Un diccionario agregado a mano que no incluya ese apóstrofo ahora también acepta esas palabras.
- **Actualizaciones firmadas con cualquiera de las dos claves del autor** (la de la PC o la de la Mac): todas las apps
  de Windows del autor revisan ahora el mismo `release.sig` (`ReleaseSignature.cs`). Un Vigía 3.3.0 conoce solo la
  primera clave, así que ofrece la página de esta versión en vez de instalarla: instalá la 3.3.1 a mano una vez.
- *Agregar un idioma…* lee el diccionario antes de copiarlo: uno que no se puede leer ya no queda en la lista como
  idioma faltante, ni reemplaza a un diccionario bueno del mismo nombre.
- Un documento con más de 5000 palabras desconocidas distintas dejaba sin revisar las que pasaban de 5000 hasta que se
  volvía a abrir; ahora se piden en el mensaje siguiente. Un clic derecho sobre una palabra marcada podía ofrecer las
  sugerencias de la palabra en la que se había hecho clic antes.
- El `SHA256SUMS.txt` de la versión publicada vuelve a listar los manuales (la 3.3.0 listaba solo el instalador).
- Bibliotecas más nuevas: DOMPurify 3.4.17 → 3.5.0 (limpia lo que muestra la vista GitHub), TOAST UI Editor 3.2.2 →
  3.2.3, highlight.js 11.11.1 → 11.12.0, marked-footnote 1.2.4 → 1.4.0, github-markdown-css 5.8.1 → 5.9.0, esbuild
  0.25.5 → 0.25.12, WebView2 SDK 1.0.4314.35 → 1.0.4370.26.

## 3.3.0 — 2026-11-25

- **Corrector ortográfico en varios idiomas a la vez**, activado de entrada con inglés, español y japonés: una palabra
  está bien si alguno de los idiomas elegidos la conoce, así un texto que mezcla idiomas se revisa entero. La revisión
  la hace Vigía mismo con diccionarios Hunspell (el inglés y el español vienen incluidos; no hay que instalar nada en
  Windows): el corrector propio del navegador tenía un solo idioma y ninguna lista para omitir. Las palabras marcadas
  llevan una línea ondulada roja en las vistas Visual y Markdown; los bloques de código, el código en línea y las
  direcciones quedan afuera. El texto en japonés nunca se marca (no existe diccionario ortográfico de japonés); se
  revisan las palabras en inglés y en español que tenga.
  - **Clic derecho sobre una palabra marcada**: sus sugerencias (un clic la reemplaza), *Omitir (hasta cerrar Vigía)*
    —esa palabra deja de marcarse en todas las pestañas hasta que se cierra Vigía— y *Agregar al diccionario*, que
    queda en el `settings.json` de este perfil.
  - **Ver → Idiomas y diccionario del corrector…**: activado o no, los idiomas (varios a la vez), las palabras
    agregadas al diccionario (seleccionar y *Quitar*), y *Agregar un idioma…* para cualquier diccionario Hunspell
    (`.dic` + `.aff`).
  - *Ver → Corrector ortográfico* lo activa o desactiva en todas las pestañas abiertas sin tocar su texto ni su
    historial de deshacer.
  - Los diccionarios están en `dict/` con sus licencias (inglés: SCOWL, MIT/BSD; español: RLA-ES, GPL/LGPL/MPL).
- **Pestaña de vista previa**: un clic en un archivo del panel de archivos (o en Favoritos o Recientes) lo muestra en
  una pestaña de paso, con el nombre en cursiva, que el próximo clic ahí reutiliza, así recorrer una carpeta ya no deja
  una pestaña por archivo. Escribir en ella, o un doble clic en el archivo o en la pestaña, la conserva. *Archivo →
  Pestaña de vista previa para los archivos del panel* la apaga.
- **Las pestañas nuevas se abren en el acto**: una página se arranca de antemano, oculta, y la próxima pestaña la toma;
  arrancar la página tardaba de 0,8 a 1,2 s por pestaña. La página se configura con las opciones del momento en que se
  toma (idioma, tema, vista).
- **Actualizaciones firmadas**: el `SHA256SUMS.txt` de una versión publicada ahora tiene que traer la firma ECDSA P-256
  del autor (`release.sig`), verificada contra una clave incluida en Vigía, antes de descargar el instalador; una
  versión sin firma válida se rechaza y nunca se ejecuta (igual se puede descargar a mano desde su página).
- **Página más liviana**: la vista GitHub trae los ~40 lenguajes comunes de highlight.js (más PowerShell, batch y
  Dockerfile) en vez de todos; un bloque de código en cualquier otro lenguaje carga el juego completo recién entonces.
  `bundle.js` pasa de 2,5 MB a cerca de 1,5 MB, así cada página arranca más rápido.
- El corrector propio del navegador se apaga en su perfil (cargaba diccionarios que nadie usaba).
- Los manuales de usuario (inglés, español, japonés) explican el corrector, la pestaña de vista previa y las
  actualizaciones firmadas.

## 3.2.1 — 2026-11-25

- **Un archivo con una tabla cuyas celdas terminan en HTML en línea** (`| x <sub>L-02</sub> |`, dos filas o más)
  mostraba una página en blanco en la vista Visual, un índice vacío y 0 palabras, mientras la vista GitHub lo mostraba
  entero: la biblioteca del editor (TOAST UI) dejaba su conversión desbalanceada y devolvía un documento vacío. Ahora
  cada celda cierra lo que abrió, y el archivo se ve en todas las vistas. Una edición hecha en esa página en blanco
  podía haber guardado un archivo casi vacío.
- **`Ctrl+Z` justo después de abrir un archivo vaciaba el documento**, y el autoguardado escribía después un solo salto
  de línea encima del archivo: la carga del texto quedaba en el historial de deshacer. Ahora se borra después de abrir
  un archivo o de recargarlo desde el disco, así deshacer solo devuelve lo que se escribió.
- **Guardar escribe el archivo de una sola vez**: el texto va a un archivo temporal al lado, llega al disco y recién
  entonces ocupa el lugar del viejo (se conservan atributos y permisos). Un corte de luz, un cuelgue o un antivirus en
  medio de un guardado deja la versión vieja o la nueva, nunca un archivo vacío o cortado. Los borradores de
  recuperación y `settings.json` se escriben igual; `settings.json.bak` guarda la configuración anterior y se lee si
  el archivo está dañado, así una instancia dedicada ya no pierde sus carpetas, favoritos y colores por un archivo
  roto. El autoguardado ya no reescribe la configuración cada vez.
- **Cerrar la ventana y elegir *No guardar*** con un documento que no se podía autoguardar (sin título, o con el
  autoguardado apagado) colgaba el programa; ahora se cierra normalmente. Un error inesperado ya no cierra Vigía sin
  aviso: queda en `error.log`, en la carpeta de configuración, y se guardan los borradores de todas las pestañas.
- **Pestañas**: un guardado que tardaba (una carpeta de red, un archivo tomado por otro programa) mientras cambiabas de
  pestaña podía terminar en la otra pestaña y después escribir su texto en el primer archivo. Un guardado, y una
  recarga después de un cambio en el disco, ahora trabajan siempre sobre la pestaña en la que empezaron, y un cambio
  de pestaña espera a que termine el guardado de esa pestaña. Un cambio en el disco que llegaba mientras se atendía
  otro ya no se pierde.
- **Cambiar de pestaña es más rápido**: primero se muestra la página y toma el foco; el menú, el índice y el panel de
  archivos se ponen al día justo después.
- **Panel de archivos**: el árbol ya no salta. Un cambio en el disco (un archivo creado, renombrado o borrado por otro
  programa o por una herramienta de sincronización) lo actualiza sin volver a abrir carpetas ni desplazarlo, y guardar
  un archivo ya no lo reconstruye. Un archivo abierto desde Favoritos (un clic, `Enter` o *Abrir* en su menú de clic
  derecho) deja los árboles como están; abierto desde su carpeta, el panel lo sigue igual que antes. Los menús de clic
  derecho de las filas se arman cuando se abren.
- **Imágenes de red** (`\\servidor\carpeta\a.png`): se muestran solo desde la carpeta del documento o una carpeta
  abierta en el panel; las demás se bloquean, así un archivo Markdown bajado de internet no puede hacer que Windows
  mande tus credenciales a un servidor.

## 3.2.0 — 2026-11-24

- **Japonés** (日本語): toda la interfaz —menús, barra de herramientas, ayudas, diálogos, paneles, los menús propios
  del editor y los colores del tema— más un manual de usuario en japonés (`docs/Vigia_Manual_JA.pdf`, *Ayuda → Manual
  de usuario*), un documento de ejemplo en japonés, el instalador en japonés y una sección en japonés en el README.
  *Idioma → 日本語*; en un Windows en japonés es el idioma desde el primer arranque. Las teclas de acceso siguen la
  convención japonesa, `ファイル(F)`. *Ayuda → Guía de Markdown* abre la página en japonés de GitHub. Windows no tiene
  corrector ortográfico de japonés: con la interfaz en japonés, se revisan las palabras en inglés de un texto. La barra
  de estado cuenta las palabras de un texto en japonés con el separador de palabras del navegador (un texto sin
  espacios contaba como una sola palabra).
- El título del panel de archivos se muestra entero o no se muestra: junto a los botones, un panel angosto mostraba
  solo «AR…».
- El documento de ejemplo, en todos los idiomas, tiene una sección sobre las pestañas.
- **Pestañas**: varios documentos en una ventana. Cada pestaña tiene su propio editor, así conserva su historial de
  deshacer, el cursor y la posición mientras se muestra otra; un punto en la pestaña indica cambios sin guardar. Abrir
  un archivo muestra la pestaña que ya lo tiene, o lo abre en una pestaña nueva al lado de la que se muestra (en esa
  misma, si es un documento sin título vacío o el ejemplo tal como se instaló). Los archivos que se sueltan sobre la
  ventana, los que se eligen juntos en *Archivo → Abrir…* o los que llegan por la línea de comandos abren una pestaña
  cada uno, y también los de un segundo Vigía abierto mientras tanto.
  - `Ctrl+N` (*Archivo → Nueva pestaña*) abre una pestaña sin título; `Ctrl+W` (*Archivo → Cerrar pestaña*, la × de
    una pestaña o un clic del medio) cierra una, preguntando por sus cambios sin guardar; la última pestaña que se
    cierra queda como un documento sin título vacío. `Ctrl+Tab` / `Ctrl+Mayús+Tab` y `Ctrl+AvPág` / `Ctrl+RePág`
    cambian de pestaña; `Ctrl+Mayús+T` (*Archivo → Reabrir la pestaña cerrada*) recupera la última que se cerró. Una
    pestaña arrastrada a lo largo de la franja se mueve entre las otras; el botón ⌄ del final las lista todas, y un
    clic derecho ofrece *Cerrar las otras pestañas*, *Cerrar las pestañas de la derecha*, *Cerrar todas las
    pestañas*, *Copiar ruta* y *Mostrar en la carpeta*.
  - **`Ctrl+W` ahora cierra la pestaña, no la ventana** (`Alt+F4` o *Archivo → Salir* cierran la ventana).
  - Cerrar la ventana cierra todas las pestañas, y cada una con cambios sin guardar pregunta antes (el autoguardado
    escribe las que puede). Con **Archivo → Reabrir las pestañas al iniciar** activado (apagado de entrada), los
    archivos de las pestañas se vuelven a abrir en el próximo arranque, con la misma pestaña a la vista; la lista se
    guarda a medida que cambian las pestañas, así también sobrevive a un cuelgue. Los documentos sin título no se
    guardan, y un archivo que para entonces ya no existe se deja afuera.
  - Una pestaña cuyo archivo cambia en el disco mientras se muestra otra se recarga (o pregunta, si tiene cambios sin
    guardar) cuando se vuelve a mostrar. Mover o cambiar el nombre de archivos en el panel de archivos se lleva sus
    pestañas. Cada pestaña tiene su propio borrador de recuperación. Una actualización vuelve a abrir todas las
    pestañas.
  - El modo, solo lectura, el ancho del texto, la ortografía, el tema, el idioma y el zoom son los mismos para todas
    las pestañas; una pestaña oculta toma el cambio cuando se vuelve a mostrar.
- **Expandir todo**: el botón del título del panel de archivos contrae todas las carpetas mientras haya alguna
  abierta, y las expande todas, todos los niveles, cuando está todo cerrado; su ícono y su ayuda dicen cuál hace.
  También en el menú ⋯ del panel. Expandir se detiene a las 1.500 carpetas, así un árbol del tamaño de un disco entero
  no congela la ventana.

## 3.1.0 — 2026-11-21

- El color de tema por defecto es el celeste, el faro contra el cielo (era el dorado oscuro), y también lo son el
  ícono del programa, sus accesos directos y el instalador. Un color elegido antes se mantiene.
- El mismo número que Vigía 3.1.0 para la Mac, que ahora distribuye sus paneles como esta versión.

## 3.0.0 — 2026-11-20

**Gryphmark ahora es Vigía**, un nombre más fácil de recordar: el que vigila desde lo alto, como la app vigila un
archivo Markdown mientras otro programa lo escribe. El ícono es un faro, del proyecto de faro de Debenne (c. 1815,
The Met, CC0). El repositorio ahora es `vigia-windows` (GitHub redirige la dirección vieja).

- Instalar Vigía quita Gryphmark (y Quillforge, si sigue ahí) en silencio. La primera vez que arranca Vigía, todo lo
  que hay en `%APPDATA%\Gryphmark` pasa a `%APPDATA%\Vigia`: la configuración, los perfiles de las instancias
  dedicadas, los borradores de recuperación. La carpeta vieja queda intacta.
- El programa es `Vigia.exe` en `%LOCALAPPDATA%\Programs\Vigia`; el instalador es `Vigia-Setup-<versión>.exe`.
- Lo que iba a traer la 2.2.1 también está acá:
  - Buscar en las carpetas: un clic en un resultado en la vista Visual o GitHub ahora cae justo en esa coincidencia.
    Las coincidencias se cuentan en el texto del archivo, y esas vistas esconden parte de él (direcciones de enlaces,
    etiquetas): en un archivo con las palabras dentro de la dirección de un enlace, el clic iba a una coincidencia
    posterior. Cuando la vista tiene otra cantidad de coincidencias, se toma aquella cuyo párrafo comparte más
    palabras con esa línea del archivo.
  - Buscar en las carpetas: en una línea con las palabras dos veces, el segundo resultado muestra la segunda en negrita
    (los dos mostraban la primera).
  - Actualizaciones: una ventana cuyo cierre se canceló durante una actualización (su pregunta respondida con
    *Cancelar*), o una en la que la actualización no siguió, ahora escucha la próxima actualización; antes dejaba de
    escuchar después de la primera señal, y la siguiente actualización la esperaba dos minutos y se rendía.
  - Actualizaciones: el nombre del instalador se revisa (letras, dígitos, punto, guion, guion bajo) antes de
    guardarlo, y su dirección tiene que ser HTTPS.
  - La barra de búsqueda ya no salta a la coincidencia siguiente cuando desde afuera se le ponen las mismas palabras.

## 2.2.0 — 2026-11-20

- Buscar en las carpetas: un panel de búsqueda (Ver → Buscar en las carpetas, la lupa de la barra o Ctrl+Mayús+F)
  busca una palabra o una frase en todos los archivos Markdown de las carpetas abiertas, los mismos que muestran los
  árboles. Los resultados vienen agrupados por archivo, cada línea con su número y la coincidencia en negrita; un clic
  abre el archivo justo en esa coincidencia, con la barra de búsqueda mostrándola. Distingue mayúsculas a pedido;
  busca un momento después de que dejás de escribir y se detiene a los 300 archivos o 5.000 coincidencias. Es un panel
  como el de archivos y el índice: en cualquier lugar, con el botón de disposición o arrastrando su título.
- Tres paneles, cualquiera de ellos de cualquier lado, uno arriba del otro o lado a lado; cada lado guarda su propia
  distribución y el orden de sus paneles, con un divisor entre cada dos.
- Cada disposición guarda su propio ancho de barra lateral: volver a una disposición la trae como se dejó. Los anchos
  fijados en la 2.1 se conservan.
- Carpetas favoritas, en la misma lista que los archivos y mostradas antes que ellos; una carpeta favorita se abre como
  un árbol.
- Los favoritos se reordenan arrastrando uno sobre otro (con el árbol bloqueado o no); Subir / Bajar siguen estando.
- Un bloque RECIENTES con los últimos archivos abiertos, debajo de los favoritos. Apagado de entrada: Ver → Archivos
  recientes en el panel, o el menú ⋯ del panel.
- Los perfiles que dejan la autoprueba y las imágenes del manual se borran en un arranque posterior.
- Las imágenes del manual y del README se hicieron de nuevo a partir de un ejemplo neutro.

## 2.1.0 — 2026-11-20

- Los paneles van a cualquier lado. El de archivos y el índice pueden estar cada uno a la izquierda o a la derecha del
  texto; del mismo lado, uno arriba del otro o lado a lado, en cualquier orden. Seis disposiciones en total, en vez de
  tres.
- Un botón de disposición en el extremo derecho de la barra, dibujado como la disposición actual. Un clic abre un panel
  con todas las disposiciones dibujadas: un clic en una la aplica en el acto; debajo, prender o apagar archivos e
  índice, e *Intercambiar archivos e índice*. *Ver → Disposición* tiene las mismas opciones.
- Arrastrar un panel de su título (ARCHIVOS o CONTENIDO) hasta un costado de la ventana. Mientras se arrastra, se marcan
  los dos costados y se rellena con su nombre el lugar que tomaría el panel; junto al otro panel, la parte de arriba lo
  pone encima, la de abajo debajo y el medio al costado. Soltarlo en el medio de la ventana, o Esc, no cambia nada. El
  texto es una ventana de navegador sobre la que WPF no puede dibujar, así que la guía es una ventana transparente
  propia. Una disposición de la 2.0 se conserva como estaba.
- Favoritos. Una estrella a la derecha de cada fila de archivo (tenue al pasar el puntero, dorada si está activa), clic
  derecho → *Agregar a favoritos*, o *Archivo → Favorito* para el archivo abierto. Los favoritos forman una sola lista
  para todas las carpetas abiertas, el primer bloque del panel de archivos, con la carpeta de cada archivo en gris; se
  pueden subir o bajar, ordenar de la A a la Z, limpiar de archivos que ya no existen o vaciar. Un favorito renombrado o
  movido desde el panel sigue a su archivo.
- Las actualizaciones se instalan solas. *Ayuda → Buscar actualizaciones* (y la revisión diaria, ahora una sola vez por
  día para todas las instancias dedicadas) pregunta una vez —*Actualizar ahora*, *Ver novedades* o *Más tarde*— y
  después: el instalador se descarga y se compara con el `SHA256SUMS.txt` de la versión publicada (uno que no coincide
  se borra y nunca se ejecuta); se cierran todas las ventanas de Gryphmark, cada una guardando antes o preguntando como
  cuando se cierra a mano; el instalador corre en silencio con las mismas opciones de instalación, espera a que se
  cierre la última ventana y vuelve a abrir todas las ventanas con su perfil y su archivo. Una ventana que no se cierra
  frena la actualización, y las ventanas que se habían cerrado vuelven.
- `--render-chrome` usa un perfil descartable: sus imágenes ya no cambian la configuración del usuario.

## 2.0.0 — 2026-11-16

Windows y macOS ahora comparten un mismo número de versión: esta es la 2.0.0 en los dos.

- Las imágenes funcionan en más lugares. Una imagen en una carpeta superior (`../img/a.png`) y un enlace a un archivo
  Markdown en otra carpeta (`[x](../otro.md)`) ahora llegan al archivo: hasta ahora la página veía solo la carpeta del
  documento, y los dos apuntaban adentro de ella. En la vista GitHub una imagen también se puede dar con una ruta
  absoluta (`C:\Imagenes\a.png`, `\\servidor\carpeta\a.png`, una dirección `file:`), con espacios en la ruta
  (`![a](mi imagen.png)`), como `![[imagen.png]]` o `![[imagen.png|300]]` (Obsidian; un nombre que no está al lado
  del documento se busca en las carpetas abiertas), y con un tamaño, como `![alt|300](a.png)`. Una imagen que no se
  encuentra es un recuadro punteado con su nombre en vez de un ícono roto, y un clic en una imagen la abre en su visor.
  Lo que la página puede cargar: la carpeta del documento, las carpetas del panel de archivos cuando el documento está
  en una de ellas, e imágenes de cualquier lugar; nada más.
- Las tablas entran en la columna. Una ruta, una dirección o un código largo dentro de una celda se corta donde hace
  falta, así las columnas se reparten el ancho en vez de quedar la tabla detrás de una barra de desplazamiento; las
  palabras comunes quedan enteras. Una tabla de ocho columnas o más lleva menos relleno en sus celdas. Medido en 58
  notas con tablas así: en una ventana de 1200 px, 6 se desplazaban antes y ninguna ahora; en una de 800 px, 8 antes y
  3 ahora.
- Ancho del texto, en Ver → Ancho del texto: *Normal* (980 px, la columna de GitHub, como antes), *Ancho* (1280 px),
  *Muy ancho* (1600 px) o *Toda la ventana*, para las vistas Visual y GitHub. Se recuerda por perfil; la impresión y
  las páginas exportadas mantienen el ancho de GitHub.
- Tres disposiciones para los paneles, en Ver → Disposición: *Archivos arriba del índice* (como antes), *Archivos e
  índice lado a lado* e *Índice a la derecha del texto*. Cada disposición guarda sus propios anchos.
- Cambiar nombre y mover desde el panel de archivos: <kbd>F2</kbd> o clic derecho → *Cambiar nombre*; con el árbol
  desbloqueado (el candado de la cabecera del panel), los archivos y carpetas que se arrastran sobre una carpeta se
  mueven ahí en el disco. Dos flechas deshacen y rehacen los cambios de nombre y los movimientos (también Ctrl+Z /
  Ctrl+Y en el árbol). El documento abierto sigue a su archivo, y antes se guarda si tenía cambios, y lo mismo hace
  *Archivo → Abrir reciente*. Un nombre que Windows cambiaría o rechazaría (un punto o un espacio al final, CON, NUL,
  COM1…) se rechaza; un nombre ya ocupado, o una carpeta dentro de sí misma, también.
- Las propiedades al principio de un archivo (front matter, entre dos líneas `---`) son un recuadro gris plegado en la
  vista Visual y una tabla bajo una línea *Propiedades* plegada en la vista GitHub, y se guardan exactamente como
  estaban escritas; los bloques `+++` (TOML) también. Un bloque cuenta solo si se cierra y tiene una clave: un primer
  `---` que nunca se cierra, o un archivo que empieza con una línea horizontal, mantiene su texto a la vista en las dos
  vistas. El índice saltea el bloque y mantiene `IMG_1915` tal como está escrito.
- Pegar dentro de un bloque de código ya no lo parte: lo que se pega ahí entra como texto sin formato.
- La ventana vuelve a abrirse donde se cerró: el mismo tamaño, el mismo lugar en el mismo monitor, maximizada o no; un
  lugar que ya no está en ninguna pantalla se abre centrado.

## 1.13.0 — 2026-11-14

- **Enlaces [[nombre]]**, como en la Mac: en la vista GitHub, `[[nombre]]`, `[[nombre|texto]]` y `[[nombre#título]]`
  abren el archivo Markdown de ese nombre en las carpetas abiertas (gana el más cercano); un nombre sin archivo se ve en
  rojo.
- **Si leés el final, seguís el final**: cuando otro programa agrega texto al final del archivo y estabas al final de la
  vista, la vista baja con lo nuevo, como en una terminal. En cualquier otro lugar te quedás donde estabas.
- Una autoprueba en la ventana real, fuera de la vista (`--self-test <log>`): DPI por monitor, la barra lateral, el
  autoguardado y su interruptor, la recarga en vivo y sus marcas verdes, los enlaces [[nombre]].

## 1.12.0 — 2026-11-13

- **Lo que cambió otro programa se marca en verde** unos segundos después de una recarga, en todas las vistas: los
  párrafos nuevos o reescritos, y en una lista o una tabla solo el punto o la fila que se agregó.
- **Una línea editada conserva todo menos tu cambio.** El editor visual reescribía la línea entera con su propio estilo
  cuando cambiaba una letra: se perdían `\|` y `\.` en una línea con una barra vertical, `*x*` por `_x_`, `\~` por `~`,
  y etiquetas en línea como `<kbd>`. Ahora en el archivo solo cambia lo que escribiste o borraste.

## 1.11.0 — 2026-11-13

- **Una sola barra lateral, como en la Mac**: el panel de archivos arriba y el índice abajo, con una línea entre los dos
  que se arrastra para cambiar sus alturas. Cada uno se sigue mostrando y ocultando por su lado (`Ctrl+Mayús+E`,
  `Ctrl+Mayús+O`); el que queda ocupa toda la barra. El ancho de la barra y la división se recuerdan. El panel de
  archivos está abierto la primera vez.
- **Las preguntas nombran sus respuestas**, como en la Mac: cuando otro programa cambia un archivo con cambios sin
  guardar, *Recargar* o *Conservar mis cambios*; al cerrar, *Guardar*, *No guardar* o *Cancelar* (eran Sí y No).
- **Nítido en una segunda pantalla**: la app sigue la escala de cada monitor (DPI por monitor). Antes, en una pantalla
  con otra escala, Windows estiraba la ventana entera como una imagen.
- Editar en la vista Visual un párrafo con enlaces `[[nombre]]` los conserva como están escritos; los guardaba como
  `\[\[nombre\]\]`.
- La página del editor ahora es el mismo archivo que en la versión para macOS (se verifica en cada commit), y el
  WebView2 SDK es el 1.0.4314.35.

## 1.10.0 — 2026-11-13

- **Autoguardado**, activado de entrada: un archivo con nombre se guarda solo 1,5 segundos después de que dejás de
  escribir. Un interruptor en la barra, y Archivo → Autoguardado, lo prenden y lo apagan. Nunca escribe encima de un
  cambio hecho por otro programa (la recarga en vivo pregunta antes), ni un documento nuevo sin título: ese se guarda
  solo cuando ya tiene nombre. Cerrar una ventana o abrir otro archivo guarda sin preguntar cuando el autoguardado está
  activado.
- **La barra de desplazamiento de la vista Visual está en el borde de la ventana.** En una ventana más ancha que la
  columna de texto de 980 px (maximizada, o agrandada hasta que Windows la ensanchó) quedaba en el borde de la columna,
  con un margen vacío grande a su derecha. La columna tiene el mismo ancho que antes.
- Guardar toma el texto y lo marca como guardado en un solo paso: una tecla apretada mientras se guardaba podía quedar
  afuera del archivo y del guardado siguiente.
- La recarga en vivo distingue nuestros propios guardados por su texto, no por el tiempo desde que se guardó: un cambio
  escrito por otro programa en los dos segundos posteriores a un guardado se ignoraba.

## 1.9.0 — 2026-11-12

- **Convertir a TXT**: clic derecho en un archivo del panel de archivos. Se escribe un .txt al lado como se lee el
  documento en la vista GitHub: títulos subrayados, listas con viñetas o números, tablas en columnas, enlaces con su
  dirección. El archivo Markdown no cambia, y un .txt que ya existe nunca se pisa (`nombre (2).txt`).
- El índice se puede ensanchar arrastrando su borde derecho (de 160 a 600 px), como ya pasaba con el panel de
  archivos; cada ventana recuerda su ancho.

## 1.8.2 — 2026-11-12

- Un archivo Markdown con una tabla HTML cuyo `<tbody>` está vacío (pandoc escribe uno por cada tabla de una sola fila
  de un documento de Word) se abría como una página en blanco, sin ningún mensaje. La biblioteca del editor (TOAST UI
  Editor 3.2.2) fallaba con esa tabla; su código ahora está corregido en el paquete, y `build.ps1` lo vuelve a corregir
  en cada recompilación.
- Si la biblioteca del editor vuelve a fallar con un documento, la vista GitHub lo sigue mostrando entero, las vistas
  visual y Markdown dicen qué pasó en vez de quedar en blanco, y guardar escribe el archivo exactamente como está en el
  disco.
- El ícono del README y de la tapa de los manuales es el dorado de la app; seguía siendo el naranja de la 1.4.0.
  `build.ps1 -Art` ahora lo escribe a partir de la variante dorada.

## 1.8.1 — 2026-11-10

- En el tema oscuro, los ítems de lista y los encabezados de tabla de la vista visual eran casi negros sobre la página
  oscura; ahora tienen el color del resto del texto.

## 1.8.0 — 2026-11-10

- **Documentos más seguros:** el HTML dentro de un archivo Markdown se limpia (DOMPurify) antes de que lo muestren la
  vista GitHub y la exportación a HTML, así un documento ya no puede ejecutar scripts en la página del editor. Los
  enlaces solo abren páginas web, correo, otros archivos Markdown, imágenes y PDF; un enlace a cualquier otro archivo
  (un programa, un script, un acceso directo) lo muestra en el Explorador en vez de ejecutarlo.
- **Una recarga en vivo que no se queda atrás:** un archivo cambiado por otro programa se recarga en el lugar, y cada
  vista conserva su posición. Ahora también se siguen los guardados que escriben un archivo nuevo y lo renombran encima
  del viejo, y una ráfaga de cambios siempre termina con la última versión en pantalla. Un archivo que otro programa
  tiene abierto se lee con acceso compartido. Escribir mientras ocurre una recarga siempre marca el documento como
  modificado, así el próximo cambio en el disco pregunta antes de reemplazarlo.
- **.NET 10:** Gryphmark ahora corre sobre .NET 10 (el soporte de .NET 8 termina el 10 de noviembre de 2026). Las
  compilaciones no llevan rutas de la máquina en la que se compilaron.
- **Bibliotecas web actualizadas:** Mermaid 12.3, KaTeX 0.19.1 (también en la hoja de estilos a la que enlazan las
  páginas exportadas), jsdiff 9, y una sola copia de DOMPurify para todo el paquete; `npm audit` no informa
  vulnerabilidades conocidas (había 14).
- Solo Windows x64, dicho en el README y en los manuales; la ventana Acerca de y los manuales nombran la página del
  proyecto.
- Compilación: la versión se escribe solo en `Gryphmark.csproj`; `build.ps1 -Art` vuelve a dibujar las imágenes, si no
  se usan las que están en el repositorio; cualquier navegador Chromium puede imprimir los manuales. Cada archivo
  fuente lleva el encabezado GPL-2.0-only.

## 1.7.2 — 2026-11-10

- **El corrector sigue el idioma de la interfaz:** cambiar entre inglés y español ahora también cambia el diccionario
  del corrector (inglés: en-US, español: es-419); se conservan el documento abierto, sus cambios sin guardar y el zoom.
  Antes, el diccionario elegido la primera vez que se abrió Gryphmark quedaba para siempre.

## 1.7.1 — 2026-11-10

- **Listas de tareas en la vista GitHub:** las casillas ahora son cuadros claros al estilo de GitHub (verdes con una
  tilde blanca cuando están hechas, en claro y en oscuro) en vez de controles deshabilitados en gris con una viñeta
  dibujada encima. Los ítems hechos se muestran en un color más suave.

## 1.7.0 — 2026-11-10

- **Instancias dedicadas:** un archivo o una carpeta se puede abrir en su propio Gryphmark, siempre separado de la
  ventana principal, con su propia configuración, carpetas abiertas, archivos recientes y ventana (`--profile
  "Nombre"`, guardado en `%APPDATA%\Gryphmark\profiles\Nombre`; una nueva arranca con el idioma, el tema y el color
  principales). Abrirla de nuevo trae esa ventana al frente en vez de abrir otra. Clic derecho en un archivo o en el
  título de una carpeta del panel de archivos → *Abrir en una instancia dedicada* o *Acceso directo en el escritorio
  (instancia dedicada)*. La barra de título muestra el nombre de la instancia.
- **Varias ventanas bien hechas:** una ventana extra (*Abrir en otra ventana*) ya no escribe encima de la sesión de la
  ventana principal (carpetas abiertas, tamaño de la ventana); solo guarda los cambios de preferencias. Los borradores
  de recuperación de una ventana que sigue abierta nunca se le ofrecen a otra.

## 1.6.0 — 2026-11-10

- **Varias carpetas a la vez** en el panel de archivos, un árbol por carpeta, agregadas en el orden en que se abren.
  Cada carpeta es un bloque con su propio color pastel y una franja a la izquierda: por defecto, tonos del color del
  tema, o *Multicolor*, o *Sin color*, y cualquier carpeta puede tener su propio color desde su menú de clic derecho.
- **Orden** desde el botón ⋯ del panel: orden de apertura (por defecto), nombre A → Z, nombre Z → A, modificadas
  recientemente primero; *Subir / Bajar* en el menú de clic derecho.
- **Fijar arriba**: las carpetas fijadas quedan siempre primero.
- **Carpetas al iniciar** (*Archivo → Carpetas al iniciar*, también en el botón ⋯): reabrir las carpetas de la última
  sesión (por defecto), empezar limpio, o empezar limpio solo con las carpetas fijadas.
- **Más firme cuando las cosas desaparecen**: una carpeta abierta que se borra o una unidad de red que se pierde se
  muestra como *no disponible* y vuelve sola; un archivo borrado en el Explorador sale del árbol enseguida, y un clic
  en una entrada vieja solo actualiza el panel. Los errores del vigilante de carpetas y los desbordes de su búfer
  disparan una actualización en vez de perderse.
- **Ventanas angostas**: los botones de formato que no entran van a un menú ⋯ en vez de deslizarse debajo de los
  selectores de vista y de solo lectura.
- **Barra de zoom** en la barra de estado: −, deslizador, + y el porcentaje (clic para 100 %); Ctrl + rueda del mouse
  funciona en toda la ventana.
- El color de tema por defecto ahora es el **dorado oscuro**. Un color guardado por la 1.5 sin haberlo elegido le deja
  el lugar.

## 1.5.1 — 2026-11-10

- El color del tema ahora llega enseguida a los accesos directos del escritorio y del menú Inicio: Windows guarda los
  íconos por ruta de archivo, así que cada color tiene su propio archivo de ícono y los accesos directos y las entradas
  de tipo de archivo apuntan a él.

## 1.5.0 — 2026-11-10

- **Temas** (*Ver → Tema…*): **Claro**, **Oscuro** o **Automático** (sigue el modo de apps de Windows y cambia en
  vivo), más un catálogo de once colores. El color es el fondo del ícono —ventana, barra de tareas, accesos del
  escritorio y del menú Inicio, archivos `.md` y el menú de carpetas— y el resaltado de la app (carpetas del panel de
  archivos, marcas de verificación, el elemento seleccionado). Los accesos directos con un ícono propio no se tocan.
- El color por defecto pasó a ser el morado de Adviento (la 1.6.0 lo cambia por el dorado oscuro); el naranja original
  está en el catálogo. Las instalaciones nuevas arrancan en Automático; las existentes conservan su elección de claro u
  oscuro.

## 1.4.0 — 2026-11-10

- **Solo lectura:** un selector nuevo *Editar | Solo lectura* junto a los botones de vista (también *Ver → Solo
  lectura*, `Ctrl+Mayús+L`) bloquea el texto en las vistas Visual y Markdown, así una tecla suelta, pegar, soltar o un
  clic en una casilla de tarea no pueden cambiar el documento. Leer, seleccionar, copiar, buscar y cambiar de vista
  siguen funcionando; escribir muestra una ayuda en la barra de estado. La elección se recuerda.
- **Ocultar el ejemplo ahora se ve:** *Archivo → Abrir reciente* muestra un ítem **Ocultar ejemplo** justo debajo
  (antes era solo un clic derecho dentro del submenú), y *Ayuda → Mostrar el ejemplo en Abrir reciente* lo apaga y lo
  vuelve a prender.

## 1.3.0 — 2026-11-10

- **Panel de archivos** (`Ctrl+Mayús+E`, el botón de carpeta, o *Ver → Panel de archivos*): abrir una carpeta entera y
  recorrer sus subcarpetas y archivos Markdown en un árbol a la izquierda, como en VS Code u Obsidian. Un clic abre un
  archivo al costado del árbol; el panel sigue los cambios en el disco, marca el archivo abierto y se puede cambiar de
  tamaño. Las carpetas ocultas y las de herramientas (`.git`, `node_modules`, `bin`, `obj`…) no se muestran.
- Abrir una carpeta con *Archivo → Abrir carpeta…*, soltándola sobre la ventana, desde la línea de comandos, o con la
  nueva tarea opcional del instalador *Abrir con Gryphmark* en el menú contextual de carpetas del Explorador.
- La última carpeta se vuelve a abrir en el próximo arranque; las carpetas recientes aparecen en *Archivo → Abrir
  reciente*.
- Menú de clic derecho en el árbol: abrir, abrir en otra ventana, mostrar en la carpeta, copiar ruta.

## 1.2.1 — 2026-11-10

- Documento de ejemplo instalado con el programa (inglés y español, archivo oculto en `samples\`): se abre en el primer
  arranque, desde **Ayuda → Documento de ejemplo**, y está fijo arriba de todo en **Archivo → Abrir reciente**, donde
  un clic derecho lo oculta. Se abre como copia sin título, así el archivo instalado nunca se pisa, y cambia de idioma
  junto con la interfaz.
- Cerrar oculta la ventana enseguida; el editor se desarma detrás.
- La revisión diaria de actualizaciones se recuerda aunque falle, así ya no corre en cada arranque.
- Diagnóstico: `GM_TRACE_CLOSE=<log>` mide cada paso del cierre.

## 1.2.0 — 2026-11-10

- **De Quillforge a Gryphmark**, por el grifo del ícono. El programa, el instalador, los manuales, la carpeta de
  configuración (`%APPDATA%\Gryphmark`) y la asociación de archivos usan el nombre nuevo.
- El instalador quita una instalación anterior de Quillforge; la configuración, los archivos recientes y los
  borradores de recuperación se copian en el primer arranque (la carpeta vieja queda intacta).

## 1.1.1 — 2026-11-10

- Ventana Acerca de nueva con el estilo de la app: ícono, versión, autor, licencia GPL-2.0 con un enlace al texto
  completo, créditos completos (highlight.js y jsdiff son BSD-3-Clause) y un botón *Copiar info*. En inglés y español.
- Los botones Visual / Markdown / GitHub ahora explican en su ayuda para qué sirve cada modo.
- Manual: los modos se llaman como en los botones; Acerca de documentado; capturas en español.

## 1.1.0 — 2026-11-10

- Barra de buscar y reemplazar (`Ctrl+F`, `Ctrl+H`, `F3` / `Mayús+F3`, coincidir mayúsculas).
- Barra lateral de índice con los títulos del documento (`Ctrl+Mayús+O`).
- El formato se conserva al guardar: solo cambian las líneas editadas; las viñetas, el estilo de énfasis, la alineación
  de tablas, CRLF/LF y el BOM se mantienen.
- Extras de GitHub: avisos (`[!NOTE]` … `[!CAUTION]`), diagramas Mermaid, fórmulas KaTeX, notas al pie, emojis por
  código.
- Recarga el archivo cuando otro programa lo cambia (pregunta antes si hay cambios sin guardar).
- Recuperación ante cierres: el trabajo sin guardar se respalda cada 20 segundos y se ofrece en el próximo arranque.
- Corrector ortográfico (menú Ver).
- Instancia única: abrir un `.md` trae al frente la ventana abierta (`Archivo → Nueva ventana` sigue abriendo otra).
- Revisión diaria de actualizaciones contra las versiones publicadas en GitHub (Ayuda → Buscar actualizaciones).
- Barra de herramientas y menús nativos, al estilo de Apple; paleta clara y oscura renovada.
- El instalador muestra e instala la licencia (GPL-2.0-only). Firma de código opcional en `build.ps1`.

## 1.0.0 — 2026-11-10

- Primera versión: editor visual, código Markdown con vista previa en vivo y vista GitHub; interfaz en inglés y
  español; tema claro y oscuro; exportar a HTML; imprimir/PDF; archivos recientes; instalador por usuario con
  asociación de `.md`; manuales en inglés y español.
