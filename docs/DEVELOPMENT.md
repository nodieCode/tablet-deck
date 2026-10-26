# Developing Tablet Deck

Everything for changing Tablet Deck itself: running it from source, editing
layouts by hand, the keybind profile, building the installer, releasing, code
signing, the website and the demo recording. For using Tablet Deck, see the
[README](../README.md).

- [Run from source](#run-from-source)
- [Test without the game](#test-without-the-game)
- [Project layout](#project-layout)
- [Editing layouts by hand](#editing-layouts-by-hand)
- [The keybind profile](#the-keybind-profile)
- [Building the installer](#building-the-installer)
- [Releasing](#releasing)
- [Code signing](#code-signing)
- [The website](#the-website)
- [Recording the demo](#recording-the-demo)

## Run from source

You need Windows 10 or 11, [Node.js](https://nodejs.org) 18 or newer, and a
tablet on the same network. No `npm install` is needed to run it: the server
uses only Node's built-in modules. The key sender (`native/keysender.cs`) and the
voice listener (`native/voice.cs`) compile themselves on first run with the C#
compiler that ships with Windows.

1. Clone the repository (or **Code → Download ZIP** and extract it).
2. Double-click `start.bat`, or run `node server.js`.
3. When Windows Firewall asks whether Node.js may use the network, allow
   **Private networks**.
4. Pair the tablet with the address, pairing code or QR code the console shows.

Running from source shows everything in the console instead of the app window,
and keeps `config.json` next to `server.js` instead of in `%APPDATA%`. The
console also prints a link to the layout editor.

Options (they work with `start.bat`, `node server.js` and the installed
`TabletDeck.exe`):

| option | does |
|---|---|
| `--dry-run` | log button presses instead of sending keys; skips the focus check |
| `--port <n>` | use another port than the one in `config.json` |
| `--ui` | status lines for the app window (used by `TabletDeck.exe`) |
| `--set name=value ...` | save settings to `config.json` and exit |

## Test without the game

### Showcase / sandbox

To show or test the deck without a phone, and without pressing keys in the
game, run a dry-run copy on another port and open the showcase page on the PC:

```
node server.js --dry-run --port 8790
```

Then open `http://localhost:8790/showcase.html?t=<pairing code>`. It shows the
deck inside an iPhone 15 Pro frame (852×393, landscape) or, with the toggle, an
iPad frame (1024×768). Clicks work as taps. The phone frame passes the iPhone's
safe-area insets (`&safe=0,59,21,59`) so the deck stays clear of the Dynamic
Island and home indicator as on the real phone.

Next to the device, a live log shows each button's label, its keys, and how it
was pressed (tap, long press, hold and release, or voice), voice results, and
whether the focus check would have blocked the press. Mode switches and panel
arrows appear as "screen only", since they send nothing to the PC. The log comes
from `/events`, which only exists in `--dry-run` and needs the pairing code.
Against a normal server the page warns that taps send real keys.

### Demo mode

`public/index.html?demo` runs the deck with no server at all: it loads the
layout from `layouts/`, and presses only play their animations. The website's
"Try it" page uses it (see [The website](#the-website)).

## Project layout

| path | what it is |
|---|---|
| `server.js` | the server: serves the deck, pairs tablets, sends keys, runs voice, the layout editor's API |
| `qr.js` | QR code generator for the console and the app window |
| `public/` | the deck (`index.html`, `app.js`, `canvas.js`, `i18n.js`, themes, icons, animations), the layout editor (`editor.*`) and the showcase |
| `layouts/` | built-in layouts; `hud-phone.json` and the starter layouts are generated |
| `keybinds/` | the Tablet Deck Keyboard profile for Star Citizen (generated) |
| `native/` | C# sources: `launcher.cs` (the app window), `keysender.cs`, `voice.cs` |
| `tools/` | generators for the phone layout, starter layouts and keybind profile, and the demo recorder |
| `scripts/` | the installer build and release notes |
| `installer/` | the Inno Setup script |
| `site/` | the website (GitHub Pages) |

## Editing layouts by hand

The layout editor covers most changes (see the README). For anything else, edit
the JSON. With the installed app, put your layout in `%APPDATA%\TabletDeck\layouts`:
a file there with the same name as a built-in one (e.g. `hud.json`) replaces it,
and one with a new name needs `layout` set in `config.json`.

Canvas layouts (`hud.json` and friends) place each element at `x`, `y`, `w`, `h`
in canvas pixels; elements in a panel are relative to their half. Element types
include `button`, `header`, `switch` (with `when` to show elements per mode),
`menu`, `slider`, `grid`, `notepad` (a pad to draw and type on; its `id` names where
its notes are kept on the device) and decorations. `star-citizen.json` is an older tabbed
grid layout.

Button fields:

| field | meaning |
|---|---|
| `id` | unique name for the button |
| `label` | text on the button; also its voice phrase |
| `keys` | Star Citizen key names joined with `+`: `r`, `lalt+n`, `np_1`, `f5`, `ralt+y` |
| `mode` | `tap` (quick press), `hold` (held while your finger is down), `timed` (held for `ms`) |
| `color` | `green`, `amber`, `red`, `blue` or blank (uses the theme's accent) |
| `icon` | icon name from `public/icons.js`; defaults to the button `id` |
| `anim` | press animation: `flash`, `ripple`, `gear`, `charge`, `burst`, `glow`, `recoil`, `streak`, `doors`, `lock`, `hazard`, `timed` |
| `voice` | extra voice phrases, or `false` to leave the button out of voice commands |
| `scAction` | Star Citizen action name; if a keybind file in `actionmaps` binds it, that key wins |
| `state` | makes the button light up while something is on: `{ "group": "doors" }` flips the group on each press, `"set": true` / `false` turns it on or off (Open/Close Doors), and `"on"` replaces the word shown while lit (default `ON`). Buttons in the same group share the state; the server keeps it (see "Lit buttons" in the README) |

Key names: `a`-`z`, `0`-`9`, `f1`-`f24`, `np_0`-`np_9`, `np_period`, `np_add`,
`np_subtract`, `np_multiply`, `np_divide`, `np_enter`, `lalt`, `ralt`, `lctrl`,
`rctrl`, `lshift`, `rshift`, `space`, `tab`, `enter`, `escape`, `backspace`,
`insert`, `delete`, `home`, `end`, `pgup`, `pgdn`, `up`, `down`, `left`, `right`.

### Generated layouts

The phone layout and the starter and ship layouts are generated from
`layouts/hud.json`, so keys and animations match. Ship layouts copy the HUD's
buttons into their own panel under new ids (`pr-…`, `vu-…`); to add a ship, add
it to `SHIPS` in `tools/make-starter-layouts.js`. After editing it:

```
node tools/make-phone-layout.js
node tools/make-starter-layouts.js
```

The phone layout is sized for an iPhone 15 Pro in landscape: a 734×372 canvas,
the 852×393 screen minus the Dynamic Island and home indicator. The page counts
any screen whose short side is under 500 points as a phone; add
`&device=tablet` or `&device=phone` to the address to override that.

### Themes

Themes are CSS variables in `public/themes.css`. Add one by copying a block
there and adding an entry to `THEMES` in `public/app.js`. Theme fonts come from
Google Fonts.

## Translations

Text is written in English and translated where it's shown, so layouts, keys
and voice phrases never change with the language.

- **The deck:** `public/i18n.js` maps each English text to
  `[Español, Français, Deutsch, Català]`. `t()` translates; `canvas.js`, `fx.js` and
  `app.js` call it for what they draw, and `apply()` translates the fixed text
  in `index.html`. Text without an entry shows in English, which is how players'
  own labels stay as they wrote them.
- **The app window:** `Lang` in `native/launcher.cs` does the same. Buttons,
  switches, links and panel titles translate when they're drawn; other text goes
  through `Lang.T()` or `Lang.F()` (with `{0}` placeholders). Status lines that
  stay on screen are kept as functions, so they follow a language change at once.

When you add a label, button, panel or message, add its translations too. To
check a layout's coverage, every `label`, `title` and `text` in `layouts/`
should have an entry (names like Prospector or mobiGlas stay English). To add a
language, add a column to both tables and an entry to `LANGUAGES` in
`i18n.js` and `Choices` in `launcher.cs`; texts must still fit their buttons,
so check the deck (`?demo`) and the window in the new language.

## The keybind profile

`keybinds/TabletDeck-StarCitizen.xml` is generated from the layouts. After you
add or change a button that has an `scAction`, rebuild it, then copy it into the
game and load it again:

```
node tools/make-profile.js
```

Every `scAction` must be listed with its Star Citizen actionmap in `ACTIONMAPS`
at the top of `tools/make-profile.js`. The script stops if an action is missing
there, if two buttons give the same action different keys, or if two actions
that are active at the same time share a key. `node tools/make-profile.js --check`
only reports whether the committed file is up to date (the installer build runs
it).

Buttons marked `"verify": true` use Star Citizen defaults that the profile
doesn't set (power management, system power toggles, SCM/NAV, eject, chat,
docking camera). The mining buttons are marked too: the profile binds them, but
their action names couldn't be confirmed against the game's files.

## Building the installer

```
npm install
npm run build
```

This makes `dist/TabletDeck-Setup-<version>.exe` (the version comes from
`package.json`). It needs [Inno Setup 6](https://jrsoftware.org/isinfo.php)
and Node.js 20 or newer. `scripts/build.js`:

1. checks the keybind profile is up to date;
2. makes the icon from `public/icon-512.png`;
3. compiles the app window (`native/launcher.cs` → `TabletDeck.exe`), the key
   sender and the voice listener, with version info;
4. packs `server.js`, `qr.js`, `public/`, `layouts/` and `keybinds/` into
   `TabletDeckServer.exe` with Node's
   [single executable application](https://nodejs.org/api/single-executable-applications.html)
   feature, and writes `THIRD-PARTY-NOTICES.txt` with that Node.js's licence (see
   [THIRD-PARTY-NOTICES.md](../THIRD-PARTY-NOTICES.md); set `NODE_LICENSE` if the
   build can't find Node's `LICENSE` file);
5. signs the files when `SIGN_CMD` is set;
6. runs Inno Setup on `installer/TabletDeck.iss` and writes the checksum.

The window runs the server hidden with `--ui` and reads its `@ui {json}` status
lines. `npm run build -- --app`, `-- --installer` and `-- --checksum` run the
stages separately, for signing services.

## Releasing

1. Set `version` in `package.json`.
2. Add a section for it at the top of `CHANGELOG.md`. It becomes the release
   notes, and the release stops without one.
3. Commit, then push a matching tag:

```
git tag v1.0.1
git push origin v1.0.1
```

The **Release** workflow (`.github/workflows/release.yml`) builds the installer
on GitHub and attaches it and its checksum to a new release. Installed copies
offer the update themselves. You can also run the workflow by hand
from the Actions tab to build without publishing. Run by hand with **Publish**
ticked, it publishes the version in `package.json` instead, tagging the commit
it ran on, for when pushing a tag isn't possible. It stops if that version is
already released.

## Code signing

Builds are unsigned until code signing is set up, so Windows SmartScreen warns
about an "unknown publisher", and antivirus tools are warier of the key sender,
which types keys into other programs. The workflow supports two ways:

- **SignPath Foundation** (free for open source): set the `SIGNPATH_API_TOKEN`
  secret and the `SIGNPATH_ORGANIZATION_ID` variable. The build then runs in two
  stages so SignPath signs the app files before they go into the installer, and
  then the installer. The artifact configurations to paste into SignPath are in
  `.signpath/artifact-configurations/`. [CODE_SIGNING.md](../CODE_SIGNING.md) is
  the signing policy, and [docs/SIGNPATH.md](SIGNPATH.md) the setup steps.
- **Your own certificate:** a `.pfx` in the `SIGNING_CERT_PFX_BASE64` and
  `SIGNING_CERT_PASSWORD` secrets.

## The website

`site/` is the website at <https://nodiecode.github.io/tablet-deck/>. The
**Website** workflow (`.github/workflows/pages.yml`) publishes it to GitHub Pages
on every push to `main` that touches the site, the deck, the layouts or the
images. `scripts/build-site.js` puts it together in `_site/`: the page, the
images from `docs/`, and the deck (`public/`) with the layouts as `try/`, set to
demo mode, so the "Try it" page always runs the current deck.

To preview it, run `node scripts/build-site.js` and serve `_site/` with any
static file server (for example `npx serve _site`).

## Recording the demo

`docs/demo.png` (the animation at the top of the README) is recorded by
`tools/record-demo.js`: it opens the deck in Microsoft Edge or Google Chrome
without a window, taps through a short script with real mouse events, and saves
the result as an animated PNG. Run a dry-run server first so no keys are pressed:

```
node server.js --dry-run --port 8790
node tools/record-demo.js "http://localhost:8790/?t=<pairing code>"
```
