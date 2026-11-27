# Bienvenido a Vigía

Este es un **documento de ejemplo**. Cámbialo todo lo que quieras: se abre como una copia sin título, así que el original nunca se sobrescribe. Si quieres conservar tu versión, guárdala con **Guardar como**.

> [!TIP]
> Prueba los tres botones del extremo derecho de la barra — **Visual · Markdown · GitHub** — o presiona `Ctrl+1`, `Ctrl+2` y `Ctrl+3`. Es el mismo documento visto de tres maneras. ¿Solo vas a leer? Elige **Solo lectura** al lado (`Ctrl+Mayús+L`) y nada cambia por error.

## Las tres vistas

| Vista | Atajo | Qué ves |
|:--|:--:|:--|
| **Visual** | `Ctrl+1` | El documento con formato. Escribes y usas los botones; Vigía escribe las etiquetas. |
| **Markdown** | `Ctrl+2` | El texto Markdown a la izquierda y la vista previa en vivo a la derecha. |
| **GitHub** | `Ctrl+3` | Solo lectura: la página tal cual la va a mostrar GitHub. |

1. Seguramente estás en **Visual**. Selecciona una palabra y presiona `Ctrl+B`: queda en **negrita**.
2. Pasa a **Markdown**: la misma palabra ahora tiene `**` a cada lado. Esas son las etiquetas.
3. Pasa a **GitHub**: el diagrama y la fórmula de más abajo se dibujan, como en github.com.

## Texto

Texto normal, *cursiva*, **negrita**, ~~tachado~~, `código en línea` y un [enlace a GitHub](https://github.com). También andan los emoji por nombre :sparkles: :rocket: y las notas al pie.[^1]

- Una lista con viñetas
  - con un elemento anidado
- [x] Una tarea terminada
- [ ] Una tarea pendiente

> Una cita. En la vista Markdown es una línea que empieza con `>`.

## Código

```python
def saludar(nombre: str) -> str:
    return f"¡Hola, {nombre}!"
```

## Diagrama (Mermaid)

En Visual y Markdown se ve como código; en la vista GitHub se dibuja.

```mermaid
flowchart LR
    A[Escribes en Visual] --> B[Miras las etiquetas en Markdown]
    B --> C[Lo ves como en GitHub]
```

## Fórmulas (KaTeX)

La entropía de una contraseña aleatoria: $H = L \cdot \log_2 N$

$$
\sum_{k=1}^{n} k = \frac{n(n+1)}{2}
$$

## Avisos

> [!NOTE]
> Información útil que conviene saber.

> [!WARNING]
> Algo que requiere atención.

## Carpetas

Abre una carpeta entera con **Archivo → Abrir carpeta…** y sus archivos Markdown aparecen en un árbol a la izquierda, como en VS Code u Obsidian. Puedes abrir varias carpetas; cada una tiene su color, y el botón **⋯** del panel las ordena, cambia los colores y elige qué se abre al iniciar. Clic en un archivo para abrirlo; `Ctrl+Mayús+E` muestra u oculta el panel.

## Pestañas

Cada archivo se abre en su propia pestaña. `Ctrl+Tab` cambia de pestaña y `Ctrl+W` cierra una. Activa **Archivo → Reabrir las pestañas al iniciar** y el próximo arranque vuelve a abrir los mismos archivos.

---

Para que este ejemplo no aparezca más en **Archivo → Abrir reciente**, elige **Ocultar ejemplo**, justo debajo de él (o desmarca **Ayuda → Mostrar el ejemplo en Abrir reciente**). Siempre lo puedes volver a abrir desde **Ayuda → Documento de ejemplo**.

[^1]: Una nota al pie: en la vista GitHub aparece al final de la página.
