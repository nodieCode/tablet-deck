# Changelog

What changed in each version of Tablet Deck, newest first. Each version's
section becomes the notes of its
[GitHub release](https://github.com/nodieCode/tablet-deck/releases), so add one
before tagging a release.

## Unreleased

- **中文 (Chinese):** the deck can show its text in Chinese, alongside Español,
  Français and Deutsch. Each tablet and phone picks its language under Layout
  in Deck settings, starting from the device's own language.

## 1.6.0 (2026-10-26)

- **Español, Français and Deutsch:** the deck and the app window can show
  their text in Spanish, French or German. Each tablet and phone picks its
  language under Layout in Deck settings, starting from the device's own
  language; the window follows Windows, or the new Language choice in
  Settings. Button labels, panels and status words are translated; keys, voice
  phrases (still English) and your own layouts' labels stay as they are. The
  translations were made with AI, so corrections from native speakers are
  welcome on GitHub.

## 1.5.0 (2026-10-25)

- **Button lights can be turned off** under Display in Deck settings. The deck
  can't read the game, so lit buttons only follow your taps; with this off,
  buttons just press their keys, without ON/OFF.
- **The window tells you when Windows treats your network as Public**, the most
  common reason a tablet can't connect, with a link to Windows' network
  settings. The warning goes away by itself once it's Private.
- **Feedback & ideas** links in the window's System panel, its tray menu and at
  the bottom of Deck settings, to GitHub's issue forms.
- The app window no longer shows a dotted focus ring around Settings when it
  opens; it only appears when you move around with Tab.

## 1.4.0 (2026-10-25)

- **Share layouts:** the layout editor can **Export** a layout as a file and
  **Import** one from a friend. An import opens unsaved, so you can check its
  buttons and keys before saving it with the usual checks.
- **Ship layouts for the Prospector and the Vulture:** each starts with a panel
  of that ship's controls, made from the deck's own buttons, on tablets and
  phones.
- **Large text** and **High contrast** switches under Display in Deck settings,
  for reading the deck at a glance. High contrast works over any theme.

## 1.3.1 (2026-10-24)

- **Sharper text in the app window:** text is drawn with ClearType on solid
  panels, each letter on a whole pixel, and the smallest labels are a size
  larger. The stars now pass behind the panels instead of showing through them.

## 1.3.0 (2026-10-24)

- **The new app window is back, and now starts:** HUD panels with corner
  brackets over drifting stars that follow the mouse (click an empty spot to
  send a pulse), telemetry of tablets, where keys go and the last minute's
  presses, a dark title bar, and the same look for Settings (now in panels with
  on/off switches), Voice commands and Install keybind profile. The stars only
  move while the window is in front.
- **If Tablet Deck ever runs into a problem, it says so:** a message with the
  reason, and the details saved in `crash.log` in the Settings folder, instead
  of not starting.

## 1.2.1 (2026-10-24)

- **Back to the 1.1.0 app window.** 1.2.0's new look kept Tablet Deck from
  starting on some PCs, so this release brings back the previous window while
  that's fixed. Everything else from 1.2.0 stays.

## 1.2.0 (2026-10-24)

- **A new look for the app window:** HUD panels with corner brackets over a
  field of drifting stars that follows the mouse; click an empty spot to send a
  pulse through them. The stars only move while the window is in front, and
  stand still when Windows' "Show animations" is off.
- **Telemetry** in the app window: tablets connected, where keys go, and the
  presses of the last minute, including those held back because Star Citizen
  wasn't in front.
- The app window has a dark title bar on Windows 10 and 11.
- **Settings, Voice commands and Install keybind profile** have the same look.
  Settings is now sorted into panels (Pairing, Keys, Voice, Push-to-talk,
  Links) with on/off switches.

## 1.1.0 (2026-10-24)

- **The QR code picks the right network:** VPN and virtual adapters (WSL,
  Hyper-V, VirtualBox, NordLynx, Tailscale and others) are skipped, so the QR
  code uses your Wi-Fi or Ethernet. The new `host` setting chooses the address
  yourself, and running from source lists each address with its adapter.
- **Wrong pairing codes are limited:** after 5 in a minute, that device is
  ignored for a minute, and the tablet says to wait.
- Fixed a held combo button (like Look Behind) that could leave a key stuck
  down when released very quickly, and a quick hold that could let go of a key
  another button was still holding.
- The tabbed grid layout's settings button now has the sliders icon and says
  **Settings**, like the HUD's, instead of the theme's name, so it's clear how
  to get to the other layouts from there.
- **A new pairing screen:** a clear card with a big code field, a red error
  when the code doesn't match, and a hint to scan the QR code instead. The
  deck's buttons stay hidden until the device is paired. It also no longer
  looks greyed out.
- **Lit buttons:** buttons that switch something on stay lit until it's
  switched off. Open Doors stays lit until Close Doors, Landing Gear shows
  Deployed, and the power toggles, lights, VTOL, flight ready, speed limiter,
  ESP, G-safety and door locks light up while on. The state is kept on the PC,
  so it survives paging and reloads, matches on every tablet and follows voice
  commands. **Reset button lights** in Deck settings turns them all off.
- **Deck settings:** the sliders sheet is now organised in sections (Layout,
  Display, Open on PC, Theme), with real on/off switches for night mode and
  two-finger swipe.
- The mic and **?** buttons only show when voice commands can actually run on
  the PC, and the voice list says why when they can't.
- Messages on the tablet appear above the page names instead of covering them.
- "Hold to engage" no longer runs into the quantum charge marks, and status
  lines like "Retracted" have room under their label.
- The installer now includes `THIRD-PARTY-NOTICES.txt` with the licence of the
  Node.js runtime inside `TabletDeckServer.exe`.

## 1.0.0 (2026-10-22)

The first public release.

- **A HUD-style control deck for any tablet or phone.** It runs in the browser,
  so iPad, Android and iPhone all work, with no app or account. Scan the QR
  code in the PC window to pair. Each button presses the real key on your PC,
  and by default only while Star Citizen is the window in front.
- **12 manufacturer themes**, press animations and a night mode.
- **Layouts:** the Tablet Deck HUD, plus the **Combat**, **Mining & Salvage**
  and **Hauling & Exploration** starter layouts, for tablets and phones. Each
  device picks its own under **Layout** in the sliders sheet. You can turn on
  a two-finger swipe there to flip panels.
- **Layout editor** (**Edit layout** in the window): drag and resize buttons,
  and change their label, keys, press mode, colour, icon, animation and voice
  phrases, with undo.
- **Voice commands:** hold the mic on the tablet, or set a push-to-talk key or
  mouse button on the PC. Over 300 phrases like "gear up", "guns mode", "full
  power to shields", "previous hostile" and "emote wave". The **?** next to the
  mic lists them all, and the window has a **Voice strictness** slider.
- **Follows your keybinds:** buttons read your Star Citizen keybinds, so
  rebinding in-game moves them too. The optional Tablet Deck Keyboard profile
  covers actions with no default key, and **Install keybind profile...** puts
  it in your game folders.
- **Open on PC:** buttons in the sliders sheet open UEX, Erkul, SPViewer and
  SCMDB in the PC's browser. Change the list in **Settings**.
- **A Windows app:** one installer, a window with the QR code, connected
  tablets and a log of presses, a tray icon, optional **Start with Windows**,
  and **Settings** for the pairing code, port, tap length, voice and more.
- **Updates:** Tablet Deck tells you when a new version is out and installs it
  when you click **Update now**. Turn the check off in the window if you like.
- Lets itself through Windows Firewall on private networks only. Settings live
  in `%APPDATA%\TabletDeck`, and uninstalling asks before deleting them.
