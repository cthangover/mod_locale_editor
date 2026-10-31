# Traducciones — escribe los textos que le faltan a un idioma

La ventana de traducciones muestra todos los textos que a un idioma le faltan en algún mod, y escribe lo que tecleas en
el archivo de localización del mod que es dueño del texto. El juego muestra el texto nuevo al instante. Se abre desde el
botón **Traducciones** de la barra de herramientas del juego; está en los juegos que incluyen las herramientas de
desarrollo (`cthangover`) y en `rpg_plus`.

## Abrirla

Inicia el juego, abre la barra de herramientas y elige **Traducciones**. La ventana lee los archivos de localización de
todos los mods instalados; unos segundos después muestra cuántos textos faltan.

## Traducir un texto

1. Elige el **idioma** arriba a la izquierda (por ejemplo, `ko`). La lista muestra solo los textos que ese idioma no tiene.
2. Si quieres, elige un **mod** para ver solo sus textos.
3. Haz clic en una fila a la izquierda. A la derecha aparece:
   - **Clave** — el identificador que pide el juego, por ejemplo `rpg_plus/map/village`;
   - **Mod** — el mod dueño del texto;
   - **Archivo** — el archivo de localización donde irá la línea, por ejemplo `locale/ko.properties`;
   - **Texto en otro idioma** — el mismo texto en inglés o en ruso (o el literal de un escenario) como pista.
4. Escribe la traducción y pulsa **Guardar** (o Intro, o Ctrl+S). La línea se escribe en el archivo. El juego vuelve a
   leer sus textos y la fila desaparece de la lista. **Revisar de nuevo** lee los archivos otra vez.

Solo cambia una línea del archivo: las demás claves, los comentarios y los finales de línea quedan como estaban.

## Adónde va el texto

- Una clave declarada en otros idiomas del mod va al archivo de este idioma: `ui_en.properties` → `ui_ko.properties`;
  `en.properties` → `ko.properties`.
- Una clave que usa un escenario pero que ningún archivo declara va a `scenarios_<idioma>.properties` del mod.
- Un mod en zip es de solo lectura. Sus filas aparecen, pero **Guardar** está desactivado y la ventana lo indica. Para
  traducirlo, copia el mod a la carpeta de mods (los mods en carpeta sí se pueden escribir).

## Por qué falta un texto

La ventana no decide nada por sí misma: muestra lo que encuentra el atlas de localización (`locale_atlas`). El atlas es
el verificador; su informe completo con todas las advertencias está en `tests/artifacts/locale_atlas/report.md` (solo
para desarrolladores). Una traducción que falta es un hueco, no un fallo de la compilación: el juego muestra el idioma
que tiene el texto.

## Conviene saber

- La ventana trabaja con los mods del juego en el que se abrió: un mod que no está instalado no aparece en la lista.
- Un mod nuevo creado con **Nuevo mod** (gestor de mods) recibe su título en inglés y en el idioma del jugador; los demás
  idiomas aparecen aquí como títulos que faltan.
- Los textos ya declarados no se ofrecen. Para cambiar una traducción, edita el archivo o usa el editor del objeto o del
  personaje (sus nombres se escriben por idioma en sus formularios).
