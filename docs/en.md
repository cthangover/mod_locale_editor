# Translations — type the texts a language is missing

The translations window lists every text that a language lacks in a mod, and writes what you type into the
locale file of the mod that owns the text. The game shows it at once. It opens from the in-game toolbar (the
**Translations** button) in games that include the tool: `cthangover` (developer tools), and `rpg_plus`.

## Open it

Start the game, open the toolbar and choose **Translations**. The window scans the locale files of every
installed mod; a few seconds later it shows the count of missing texts.

## Translate a text

1. Choose the **language** at the top left (for example `ko`). The list shows only the texts that this language
   lacks.
2. Optionally choose one **mod** to see only its texts.
3. Click a row on the left. The right side shows:
   - **Key** — the identifier the game asks for, for example `rpg_plus/map/village`;
   - **Mod** — the mod that owns the text;
   - **File** — the locale file the line goes into, for example `locale/ko.properties`;
   - **Text in another language** — the same text in English or Russian (or the literal a scenario uses), as a hint.
4. Type the translation and press **Save** (or Enter, or Ctrl+S). The line is written into the file. The game reloads
   its texts, and the row disappears from the list. Press **Scan again** to read the files once more.

Only the one line changes in the file: the other keys, comments and line endings stay as they were.

## Where the text goes

- A key declared in other languages of a mod goes to the same file of this language: `ui_en.properties` →
  `ui_ko.properties`; `en.properties` → `ko.properties`.
- A key a scenario uses but no file declares goes to `scenarios_<language>.properties` of the mod.
- A zip mod is read-only. Its rows are listed, but **Save** is off and the window says so. Copy the mod into the
  mods folder (folder mods can be written) to translate it.

## Why a text is missing

The window does not decide anything itself: it shows what the locale atlas (`locale_atlas`) finds. The atlas is the
checker; its full report with every warning is in `tests/artifacts/locale_atlas/report.md` (developers only). A
missing translation is a gap, never a broken build: the game shows the language that has the text.

## Good to know

- The window works on the mods of the game it was opened in: a mod that is not installed is not in the list.
- A new mod made with **New mod** (mod manager) starts with its title in English and in the player's language;
  the other languages show up here as missing titles.
- Texts that are already declared are not offered. To change a translation, edit the file, or use the editor of
  the item or character (their names are typed per language in their own forms).
