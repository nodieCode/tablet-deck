// The deck's languages. Text is translated where it's shown: labels, panel titles, status
// words and the deck's own messages. Layout files, keys and voice phrases stay English, so
// nothing a button presses or listens for changes. Star Citizen's own terms (Quantum, SCM,
// NAV, VTOL, ESP, ATC, mobiGlas) stay as the game shows them.
//
// T maps English text to [Español, Français, Deutsch, Català]. "{0}" in a text is filled in by t().
// Text with no entry shows in English, so your own layouts keep the labels you gave them.
// The translations were made with AI; corrections from native speakers are welcome.
(() => {
  const LANGUAGES = [["en", "English"], ["es", "Español"], ["fr", "Français"], ["de", "Deutsch"], ["ca", "Català"]];
  const COLUMN = { es: 0, fr: 1, de: 2, ca: 3 };
  const known = (l) => Object.prototype.hasOwnProperty.call(COLUMN, l);

  const T = {
    // ---------- the deck's own text ----------
    "Deck settings": ["Ajustes del deck", "Réglages du deck", "Deck-Einstellungen", "Configuració del deck"],
    "Close": ["Cerrar", "Fermer", "Schließen", "Tancar"],
    "Layout": ["Diseño", "Disposition", "Layout", "Disposició"],
    "Language": ["Idioma", "Langue", "Sprache", "Idioma"],
    "Each tablet and phone keeps its own choice.": ["Cada tablet y móvil guarda su propia elección.", "Chaque tablette et téléphone garde son propre choix.", "Jedes Tablet und Handy merkt sich seine eigene Wahl.", "Cada tauleta i mòbil mantenen la seva elecció pròpia."],
    "Display": ["Pantalla", "Affichage", "Anzeige", "Pantalla"],
    "Night mode": ["Modo nocturno", "Mode nuit", "Nachtmodus", "Mode nocturn"],
    "Dims the deck for a dark room.": ["Atenúa el deck para una habitación oscura.", "Assombrit le deck pour une pièce sombre.", "Dimmt das Deck für einen dunklen Raum.", "Atenua el deck per a una habitació fosca."],
    "Large text": ["Texto grande", "Grand texte", "Große Schrift", "Text gran"],
    "Bigger, bolder labels, easier to read at a glance.": ["Etiquetas más grandes y gruesas, fáciles de leer de un vistazo.", "Libellés plus grands et plus gras, lisibles d'un coup d'œil.", "Größere, fettere Beschriftungen, auf einen Blick lesbar.", "Etiquetes més grans i gruixudes, més fàcils de llegir d'un cop d'ull."],
    "High contrast": ["Alto contraste", "Contraste élevé", "Hoher Kontrast", "Contrast alt"],
    "Black and white with strong colours, over any theme.": ["Blanco y negro con colores intensos, sobre cualquier tema.", "Noir et blanc aux couleurs vives, avec n'importe quel thème.", "Schwarz-Weiß mit kräftigen Farben, für jedes Theme.", "Blanc i negre amb colors intensos, sigui quin sigui el tema."],
    "Two-finger swipe": ["Deslizar con dos dedos", "Balayage à deux doigts", "Mit zwei Fingern wischen", "Lliscar amb dos dits"],
    "Swipe sideways over a half to flip its panels. Presses then wait a moment.": ["Desliza de lado sobre una mitad para pasar sus paneles. Las pulsaciones esperan un momento.", "Balayez latéralement une moitié pour changer de panneau. Les appuis attendent alors un instant.", "Seitlich über eine Hälfte wischen, um ihre Panels zu blättern. Antippen reagiert dann etwas verzögert.", "Llisca lateralment sobre una meitat per canviar de panell. Les pulsacions esperen un moment."],
    "Button lights": ["Luces de botones", "Voyants des boutons", "Tastenbeleuchtung", "Llums dels botons"],
    "Light buttons and show ON/OFF while something is on. They follow your taps, not the game.": ["Ilumina los botones y muestra ON/OFF mientras algo está activo. Siguen tus pulsaciones, no el juego.", "Allume les boutons et affiche ON/OFF quand quelque chose est actif. Ils suivent vos appuis, pas le jeu.", "Lässt Tasten leuchten und zeigt AN/AUS, solange etwas an ist. Sie folgen deinem Antippen, nicht dem Spiel.", "Il·lumina els botons i mostra ON/OFF mentre alguna cosa està activa. Segueixen les teves pulsacions, no el joc."],
    "Reset button lights": ["Reiniciar luces", "Réinitialiser les voyants", "Beleuchtung zurücksetzen", "Restableix els llums dels botons"],
    "Turns all lit buttons off, e.g. after a respawn or when they don't match the game.": ["Apaga todos los botones iluminados, p. ej. tras reaparecer o si no coinciden con el juego.", "Éteint tous les boutons allumés, par ex. après une réapparition ou s'ils ne correspondent plus au jeu.", "Schaltet alle leuchtenden Tasten aus, z. B. nach einem Respawn oder wenn sie nicht zum Spiel passen.", "Apaga tots els botons il·luminats, per exemple després de reaparèixer o si no coincideixen amb el joc."],
    "Button lights reset": ["Luces reiniciadas", "Voyants réinitialisés", "Beleuchtung zurückgesetzt", "Llums dels botons restablerts"],
    "Open on PC": ["Abrir en el PC", "Ouvrir sur le PC", "Am PC öffnen", "Obrir al PC"],
    "Opens the site in your PC's browser.": ["Abre la web en el navegador de tu PC.", "Ouvre le site dans le navigateur de votre PC.", "Öffnet die Seite im Browser deines PCs.", "Obre el web al navegador del teu PC."],
    "Theme": ["Tema", "Thème", "Theme", "Tema"],
    "Ideas or problems?": ["¿Ideas o problemas?", "Des idées ou un problème ?", "Ideen oder Probleme?", "Idees o problemes?"],
    "Feedback & ideas on GitHub": ["Comentarios e ideas en GitHub", "Avis et idées sur GitHub", "Feedback & Ideen auf GitHub", "Comentaris i idees a GitHub"],
    "Voice commands": ["Comandos de voz", "Commandes vocales", "Sprachbefehle", "Ordres de veu"],
    "Voice commands list": ["Lista de comandos de voz", "Liste des commandes vocales", "Liste der Sprachbefehle", "Llista d'ordres de veu"],
    "Hold the mic button and say one of these. Most Star Citizen controls are toggles, so \"engines on\" and \"engines off\" press the same key.": ["Mantén pulsado el micrófono y di uno de estos (en inglés). La mayoría de controles de Star Citizen alternan, así que \"engines on\" y \"engines off\" pulsan la misma tecla.", "Maintenez le micro et dites l'une de ces phrases (en anglais). La plupart des commandes de Star Citizen basculent, donc \"engines on\" et \"engines off\" appuient sur la même touche.", "Halte das Mikro gedrückt und sag einen davon (auf Englisch). Die meisten Star-Citizen-Befehle schalten um, daher drücken \"engines on\" und \"engines off\" dieselbe Taste.", "Mantén premut el botó del micròfon i digues una d'aquestes ordres (en anglès). La majoria de controls de Star Citizen alternen entre dos estats; per això «engines on» i «engines off» premen la mateixa tecla."],
    "Search, e.g. shields": ["Buscar, p. ej. shields", "Rechercher, par ex. shields", "Suchen, z. B. shields", "Cerca, p. ex., escuts"],
    "Hold to talk": ["Pulsa y habla", "Tenir pour parler", "Halten & sprechen", "Mantén premut per parlar"],
    "Settings": ["Ajustes", "Réglages", "Einstellungen", "Configuració"],
    "Fullscreen": ["Pantalla completa", "Plein écran", "Vollbild", "Pantalla completa"],
    "Previous panel": ["Panel anterior", "Panneau précédent", "Vorheriges Panel", "Panell anterior"],
    "Next panel": ["Panel siguiente", "Panneau suivant", "Nächstes Panel", "Panell següent"],
    "Turn your phone sideways": ["Gira el móvil", "Tournez votre téléphone", "Dreh dein Handy quer", "Gira el mòbil"],
    "Tap to continue": ["Toca para continuar", "Touchez pour continuer", "Tippen zum Fortfahren", "Toca per continuar"],
    "Star Citizen control deck": ["Panel de control para Star Citizen", "Deck de commande pour Star Citizen", "Steuerdeck für Star Citizen", "Deck de control de Star Citizen"],
    "› HUD systems": ["› Sistemas HUD", "› Systèmes HUD", "› HUD-Systeme", "› Sistemes HUD"],
    "› Hull profile": ["› Perfil del casco", "› Profil de coque", "› Rumpfprofil", "› Perfil del casc"],
    "› PC uplink": ["› Enlace con el PC", "› Liaison PC", "› PC-Verbindung", "› Connexió amb el PC"],
    "Loading layout…": ["Cargando diseño…", "Chargement de la disposition…", "Layout wird geladen…", "Carregant la disposició…"],
    "Connecting…": ["Conectando…", "Connexion…", "Verbinde…", "Connectant…"],
    "Connected": ["Conectado", "Connecté", "Verbunden", "Connectat"],
    "Reconnecting…": ["Reconectando…", "Reconnexion…", "Verbinde neu…", "Reconnectant…"],
    "Not paired": ["Sin emparejar", "Non appairé", "Nicht gekoppelt", "Sense aparellar"],
    "Demo": ["Demo", "Démo", "Demo", "Demostració"],
    "Error": ["Error", "Erreur", "Fehler", "Error"],
    "Star Citizen not focused": ["Star Citizen no está en primer plano", "Star Citizen n'est pas au premier plan", "Star Citizen ist nicht im Vordergrund", "Star Citizen no és en primer pla"],
    "Heard: {0}": ["Oído: {0}", "Entendu : {0}", "Gehört: {0}", "S'ha sentit: {0}"],
    "Didn't catch that": ["No te he entendido", "Je n'ai pas compris", "Nicht verstanden", "No t'he entès"],
    "{0}: test mode, not opened": ["{0}: modo de prueba, no se abrió", "{0} : mode test, non ouvert", "{0}: Testmodus, nicht geöffnet", "{0}: mode de prova, no s'ha obert"],
    "Opened {0} on the PC": ["{0} abierto en el PC", "{0} ouvert sur le PC", "{0} am PC geöffnet", "S'ha obert {0} al PC"],
    "With the app, this opens {0} on your PC": ["Con la app, esto abre {0} en tu PC", "Avec l'appli, ceci ouvre {0} sur votre PC", "Mit der App öffnet das {0} auf deinem PC", "Amb l'aplicació, això obre {0} al teu PC"],
    "Voice commands work with Tablet Deck on your PC": ["Los comandos de voz funcionan con Tablet Deck en tu PC", "Les commandes vocales fonctionnent avec Tablet Deck sur votre PC", "Sprachbefehle funktionieren mit Tablet Deck auf deinem PC", "Les ordres de veu funcionen amb Tablet Deck al teu PC"],
    "Demo: taps play their animations; with the app, they press keys in Star Citizen": ["Demo: los toques muestran sus animaciones; con la app, pulsan teclas en Star Citizen", "Démo : les appuis jouent leurs animations ; avec l'appli, ils appuient sur des touches dans Star Citizen", "Demo: Antippen spielt die Animationen ab; mit der App drückt es Tasten in Star Citizen", "Demostració: els tocs mostren les animacions; amb l'aplicació, premen tecles a Star Citizen"],
    "Loading…": ["Cargando…", "Chargement…", "Lädt…", "Carregant…"],
    "Couldn't load the list from the PC.": ["No se pudo cargar la lista desde el PC.", "Impossible de charger la liste depuis le PC.", "Die Liste konnte nicht vom PC geladen werden.", "No s'ha pogut carregar la llista des del PC."],
    "Voice commands are turned off on the PC. Turn them on in the Tablet Deck window's Settings.": ["Los comandos de voz están desactivados en el PC. Actívalos en Ajustes de la ventana de Tablet Deck.", "Les commandes vocales sont désactivées sur le PC. Activez-les dans les réglages de la fenêtre Tablet Deck.", "Sprachbefehle sind am PC ausgeschaltet. Schalte sie in den Einstellungen des Tablet-Deck-Fensters ein.", "Les ordres de veu estan desactivades al PC. Activa-les a la configuració de la finestra de Tablet Deck."],
    "Voice commands can't run on this PC{0}. The Tablet Deck window shows what to do.": ["Los comandos de voz no pueden funcionar en este PC{0}. La ventana de Tablet Deck indica qué hacer.", "Les commandes vocales ne peuvent pas fonctionner sur ce PC{0}. La fenêtre Tablet Deck indique quoi faire.", "Sprachbefehle laufen auf diesem PC nicht{0}. Das Tablet-Deck-Fenster zeigt, was zu tun ist.", "Les ordres de veu no poden funcionar en aquest PC{0}. La finestra de Tablet Deck t'indica què has de fer."],
    "The PC is still starting voice commands. Try again in a moment.": ["El PC aún está iniciando los comandos de voz. Inténtalo en un momento.", "Le PC démarre encore les commandes vocales. Réessayez dans un instant.", "Der PC startet die Sprachbefehle noch. Versuch es gleich noch einmal.", "El PC encara està iniciant les ordres de veu. Torna-ho a provar d'aquí a un moment."],
    "Nothing matches that.": ["No hay coincidencias.", "Aucun résultat.", "Keine Treffer.", "No hi ha coincidències."],
    "Pair this device": ["Emparejar este dispositivo", "Appairer cet appareil", "Dieses Gerät koppeln", "Aparella aquest dispositiu"],
    "Open {0} on your gaming PC and type the pairing code its window shows.": ["Abre {0} en tu PC de juego y escribe el código de emparejamiento que muestra su ventana.", "Ouvrez {0} sur votre PC de jeu et saisissez le code d'appairage affiché dans sa fenêtre.", "Öffne {0} auf deinem Gaming-PC und gib den Kopplungscode aus seinem Fenster ein.", "Obre {0} al PC de joc i escriu el codi d'aparellament que mostra la finestra."],
    "Pairing code": ["Código de emparejamiento", "Code d'appairage", "Kopplungscode", "Codi d'aparellament"],
    "e.g. 1a2b3c4d": ["p. ej. 1a2b3c4d", "par ex. 1a2b3c4d", "z. B. 1a2b3c4d", "p. ex., 1a2b3c4d"],
    "Connect": ["Conectar", "Connecter", "Verbinden", "Connecta"],
    "Quicker: scan the QR code in the Tablet Deck window with this device's camera.": ["Más rápido: escanea el código QR de la ventana de Tablet Deck con la cámara de este dispositivo.", "Plus rapide : scannez le code QR de la fenêtre Tablet Deck avec l'appareil photo de cet appareil.", "Schneller: Scanne den QR-Code im Tablet-Deck-Fenster mit der Kamera dieses Geräts.", "Més ràpid: escaneja el codi QR de la finestra de Tablet Deck amb la càmera d'aquest dispositiu."],
    "Too many wrong codes. Wait a minute, then try again.": ["Demasiados códigos incorrectos. Espera un minuto y vuelve a intentarlo.", "Trop de codes erronés. Attendez une minute, puis réessayez.", "Zu viele falsche Codes. Warte eine Minute und versuch es dann erneut.", "Hi ha hagut massa codis incorrectes. Espera un minut i torna-ho a provar."],
    "That code didn't match. Check the code in the Tablet Deck window on your PC and try again.": ["Ese código no coincide. Comprueba el código en la ventana de Tablet Deck de tu PC y vuelve a intentarlo.", "Ce code ne correspond pas. Vérifiez le code dans la fenêtre Tablet Deck de votre PC et réessayez.", "Der Code stimmt nicht. Prüfe den Code im Tablet-Deck-Fenster auf deinem PC und versuch es erneut.", "Aquest codi no coincideix. Comprova el codi a la finestra de Tablet Deck del PC i torna-ho a provar."],
    "Type the pairing code first.": ["Escribe primero el código de emparejamiento.", "Saisissez d'abord le code d'appairage.", "Gib zuerst den Kopplungscode ein.", "Primer escriu el codi d'aparellament."],
    "Layout error ({0})": ["Error de diseño ({0})", "Erreur de disposition ({0})", "Layout-Fehler ({0})", "Error de disposició ({0})"],
    "PC's choice: {0}": ["Elección del PC: {0}", "Choix du PC : {0}", "Wahl des PCs: {0}", "Opció del PC: {0}"],
    "{0} (yours)": ["{0} (tuyo)", "{0} (le vôtre)", "{0} (deins)", "{0} (teva)"],
    "Hold": ["Mantener", "Maintenir", "Halten", "Mantén premut"],
    "Long press": ["Pulsación larga", "Appui long", "Lang drücken", "Pulsació llarga"],

    // ---------- notepad ----------
    "Cargo notes": ["Notas de carga", "Notes de cargo", "Frachtnotizen"],
    "Pen": ["Lápiz", "Stylo", "Stift"],
    "Eraser": ["Borrador", "Gomme", "Radierer"],
    "Undo": ["Deshacer", "Annuler", "Rückgängig"],
    "Clear": ["Borrar", "Effacer", "Löschen"],
    "Clear page?": ["¿Borrar página?", "Effacer la page ?", "Seite löschen?"],
    "Previous page": ["Página anterior", "Page précédente", "Vorherige Seite"],
    "Next page": ["Página siguiente", "Page suivante", "Nächste Seite"],
    "Type": ["Escribir", "Taper", "Tippen"],
    "Draw or type here. Notes stay on this device.": ["Dibuja o escribe aquí. Las notas se quedan en este dispositivo.", "Dessinez ou tapez ici. Les notes restent sur cet appareil.", "Hier zeichnen oder tippen. Die Notizen bleiben auf diesem Gerät."],

    // ---------- theme taglines ----------
    "Classic interstellar lines": ["Líneas interestelares clásicas", "Lignes interstellaires classiques", "Klassische interstellare Linien", "Línies interestel·lars clàssiques"],
    "Built rough, flown hard": ["Construido a lo bruto, pilotado a tope", "Construit brut, piloté dur", "Rau gebaut, hart geflogen", "Construït a l'ample, pilotat a sac"],
    "Military precision": ["Precisión militar", "Précision militaire", "Militärische Präzision", "Precisió militar"],
    "Frontline hardware": ["Equipo de primera línea", "Matériel de première ligne", "Frontausrüstung", "Equipament de primera línia"],
    "Luxury in the void": ["Lujo en el vacío", "Le luxe dans le vide", "Luxus im All", "Luxe en el buit"],
    "Industrial workhorses": ["Caballos de batalla industriales", "Bêtes de somme industrielles", "Industrielle Arbeitstiere", "Cavalls de batalla industrials"],
    "Open skies, big haulers": ["Cielos abiertos, grandes cargueros", "Grands espaces, gros cargos", "Weiter Himmel, große Frachter", "Cels oberts, grans transportistes"],
    "Alien craft, merchant spirit": ["Naves alienígenas, espíritu mercante", "Vaisseaux extraterrestres, esprit marchand", "Fremde Schiffe, Händlergeist", "Naus alienígenes, esperit mercant"],
    "Xi'an elegance": ["Elegancia xi'an", "Élégance xi'an", "Xi'an-Eleganz", "Elegància Xi'an"],
    "Utility first": ["Lo práctico primero", "L'utilité d'abord", "Nutzen zuerst", "Primer, la utilitat"],
    "Frontier newcomer": ["Recién llegado de la frontera", "Nouveau venu de la frontière", "Neuling an der Grenze", "Nouvingut de la frontera"],
    "Reborn Tevarin designs": ["Diseños tevarin renacidos", "Designs tevarin renaissants", "Wiedergeborene Tevarin-Designs", "Dissenys tevarin renaixents"],

    // ---------- layouts, tabs, panels and headers ----------
    "Mining & Salvage": ["Minería y recuperación", "Minage et récupération", "Bergbau & Bergung", "Mineria i desballestament"],
    "Hauling & Exploration": ["Transporte y exploración", "Transport et exploration", "Transport & Erkundung", "Transport i exploració"],
    "Star Citizen (tabbed grid)": ["Star Citizen (cuadrícula)", "Star Citizen (grille)", "Star Citizen (Raster)", "Star Citizen (graella amb pestanyes)"],
    "Combat": ["Combate", "Combat", "Kampf", "Combat"],
    "Landing": ["Aterrizaje", "Atterrissage", "Landung", "Aterratge"],
    "Power": ["Energía", "Énergie", "Energie", "Energia"],
    "QT/Systems": ["QT/Sistemas", "QT/Systèmes", "QT/Systeme", "QT/Sistemes"],
    "Operator mode": ["Modo operador", "Mode opérateur", "Bedienmodus", "Mode d'operador"],
    "Operator Modes": ["Modos de operador", "Modes opérateur", "Bedienmodi", "Modes d'operador"],
    "Emergency": ["Emergencia", "Urgence", "Notfall", "Emergència"],
    "Flight Assist": ["Asistencia de vuelo", "Assistance de vol", "Flugassistenz", "Assistència de vol"],
    "Shields": ["Escudos", "Boucliers", "Schilde", "Escuts"],
    "Targeting": ["Objetivos", "Ciblage", "Zielerfassung", "Selecció d'objectius"],
    "Crew/Doors": ["Tripulación/Puertas", "Équipage/Portes", "Crew/Türen", "Tripulació/Portes"],
    "On Foot": ["A pie", "À pied", "Zu Fuß", "A peu"],
    "Emotes": ["Gestos", "Emotes", "Emotes", "Gestos"],
    "Emotes 1": ["Gestos 1", "Emotes 1", "Emotes 1", "Gestos 1"],
    "Emotes 2": ["Gestos 2", "Emotes 2", "Emotes 2", "Gestos 2"],
    "Power/Landing": ["Energía/Aterrizaje", "Énergie/Atterrissage", "Energie/Landung", "Energia/Aterratge"],
    "Prospector Flight": ["Prospector: vuelo", "Prospector : vol", "Prospector: Flug", "Vol del Prospector"],
    "Vulture Flight": ["Vulture: vuelo", "Vulture : vol", "Vulture: Flug", "Vulture: vol"],
    "Modes": ["Modos", "Modes", "Modi", "Modes"],
    "Targets": ["Objetivos", "Cibles", "Ziele", "Objectius"],
    "Crew": ["Tripulación", "Équipage", "Crew", "Tripulació"],
    "Ship_systems": ["Sistemas_nave", "Systèmes_vaisseau", "Schiffssysteme", "Sistemes_nau"],
    "Power_management": ["Gestión_energía", "Gestion_énergie", "Energieverwaltung", "Gestió_energia"],
    "Laser": ["Láser", "Laser", "Laser", "Làser"],
    "Modules": ["Módulos", "Modules", "Module", "Mòduls"],
    "Ship": ["Nave", "Vaisseau", "Schiff", "Nau"],
    "Doors": ["Puertas", "Portes", "Türen", "Portes"],
    "Landing mode": ["Modo aterrizaje", "Mode atterrissage", "Landemodus", "Mode d'aterratge"],
    "Docking mode": ["Modo atraque", "Mode amarrage", "Andockmodus", "Mode d'acoblament"],
    "ENABLED": ["ACTIVO", "ACTIF", "AKTIV", "ACTIVAT"],
    "Max": ["Máx", "Max", "Max", "Màx."],
    "Incr": ["Más", "Plus", "Mehr", "Augm."],
    "Decr": ["Menos", "Moins", "Weniger", "Red."],
    "Min": ["Mín", "Min", "Min", "Mín."],
    "Weapons": ["Armas", "Armes", "Waffen", "Armes"],
    "Thrusters": ["Propulsores", "Propulseurs", "Triebwerke", "Propulsors"],

    // ---------- status words ----------
    "ON": ["ON", "ON", "AN", "ON"],
    "OFF": ["OFF", "OFF", "AUS", "OFF"],
    "ONLINE": ["EN LÍNEA", "EN LIGNE", "ONLINE", "EN LÍNIA"],
    "DEPLOYED": ["DESPLEGADO", "DÉPLOYÉ", "AUSGEFAHREN", "DESPLEGAT"],
    "RETRACTED": ["RETRAÍDO", "RENTRÉ", "EINGEFAHREN", "REPLEGAT"],
    "DEPLOYING…": ["DESPLEGANDO…", "DÉPLOIEMENT…", "FÄHRT AUS…", "DESPLEGANT…"],
    "RETRACTING…": ["RETRAYENDO…", "RENTRÉE…", "FÄHRT EIN…", "REPLEGANT…"],
    "HOLD TO ENGAGE": ["MANTÉN PARA ACTIVAR", "MAINTENIR POUR ENGAGER", "HALTEN ZUM AKTIVIEREN", "MANTÉN PREMUT PER ACTIVAR"],
    "SPOOLING…": ["CARGANDO…", "CHARGEMENT…", "LÄDT…", "CARREGANT…"],
    "ENGAGED": ["ACTIVADO", "ENGAGÉ", "AKTIVIERT", "ACTIVAT"],
    "OPEN": ["ABIERTO", "OUVERT", "OFFEN", "OBERT"],
    "CLOSED": ["CERRADO", "FERMÉ", "ZU", "TANCAT"],
    "OPENING…": ["ABRIENDO…", "OUVERTURE…", "ÖFFNET…", "OBRINT…"],
    "CLOSING…": ["CERRANDO…", "FERMETURE…", "SCHLIESST…", "TANCANT…"],
    "ACQUIRING…": ["ADQUIRIENDO…", "ACQUISITION…", "ERFASSE…", "ADQUIRINT…"],
    "LOCKED": ["BLOQUEADO", "VERROUILLÉ", "VERRIEGELT", "FIXAT"],
    "HOLD TO ARM": ["MANTÉN PARA ARMAR", "TENIR POUR ARMER", "GEDRÜCKT HALTEN", "MANTÉN PREMUT PER ARMAR"],
    "ARMING…": ["ARMANDO…", "ARMEMENT…", "WIRD SCHARF…", "ARMANT…"],
    "ARMED": ["ARMADO", "ARMÉ", "SCHARF", "ARMAT"],

    // ---------- button labels ----------
    "3rd Person": ["3.ª persona", "3e personne", "3. Person", "3a persona"],
    "Agree": ["De acuerdo", "D'accord", "Zustimmen", "Assentir"],
    "Angry": ["Enfadado", "En colère", "Wütend", "Enfadat"],
    "At Ease": ["Descanso", "Repos", "Rührt euch", "Descans"],
    "Attention": ["Firmes", "Garde-à-vous", "Stillgestanden", "Ferms"],
    "Back": ["Atrás", "Arrière", "Hinten", "Enrere"],
    "Beam Spacing +": ["Separación haz +", "Écart faisceaux +", "Strahlabstand +", "Separació del feix +"],
    "Beam Spacing −": ["Separación haz −", "Écart faisceaux −", "Strahlabstand −", "Separació del feix −"],
    "Blah": ["Bla", "Bla-bla", "Bla", "Bla, bla"],
    "Bored": ["Aburrido", "Ennuyé", "Gelangweilt", "Avorrit"],
    "Bottom": ["Abajo", "Bas", "Unten", "Inferior"],
    "Bow": ["Reverencia", "Révérence", "Verbeugen", "Reverència"],
    "Burp": ["Eructar", "Roter", "Rülpsen", "Rot"],
    "Cheer": ["Animar", "Acclamer", "Jubeln", "Celebrar"],
    "Chicken": ["Gallina", "Poule mouillée", "Hühnchen", "Gallina"],
    "Clap": ["Aplaudir", "Applaudir", "Klatschen", "Aplaudir"],
    "Close All Doors": ["Cerrar todas las puertas", "Fermer toutes les portes", "Alle Türen schließen", "Tanca totes les portes"],
    "Close Doors": ["Cerrar puertas", "Fermer les portes", "Türen schließen", "Tanca les portes"],
    "Come": ["Ven", "Viens", "Komm", "Vine"],
    "Contacts": ["Contactos", "Contacts", "Kontakte", "Contactes"],
    "Cry": ["Llorar", "Pleurer", "Weinen", "Plorar"],
    "Dance": ["Bailar", "Danser", "Tanzen", "Ballar"],
    "Decoy": ["Señuelo", "Leurre", "Täuschkörper", "Contramesures"],
    "Disagree": ["En desacuerdo", "Pas d'accord", "Ablehnen", "Negar"],
    "Docking Camera": ["Cámara de atraque", "Caméra d'amarrage", "Andockkamera", "Càmera d'acoblament"],
    "Drop Item": ["Soltar objeto", "Lâcher l'objet", "Gegenstand ablegen", "Deixa anar l'objecte"],
    "Engage Auto Land": ["Aterrizaje automático", "Atterrissage auto", "Auto-Landung", "Activa l'aterratge automàtic"],
    "Engage Quantum": ["Activar Quantum", "Engager Quantum", "Quantum aktivieren", "Activar Quantum"],
    "Exit Seat": ["Salir del asiento", "Quitter le siège", "Sitz verlassen", "Surt del seient"],
    "Failure": ["Fracaso", "Échec", "Fehlschlag", "Fracàs"],
    "Fewer Missiles": ["Menos misiles", "Moins de missiles", "Weniger Raketen", "Menys míssils"],
    "Fire Laser": ["Disparar láser", "Tirer au laser", "Laser feuern", "Dispara el làser"],
    "Flex": ["Sacar músculo", "Muscles", "Muskeln zeigen", "Fer múscul"],
    "Flight": ["Vuelo", "Vol", "Flug", "Vol"],
    "Flight Mode": ["Modo vuelo", "Mode vol", "Flugmodus", "Mode de vol"],
    "Flight Ready": ["Listo para volar", "Prêt au vol", "Flugbereit", "Preparat per volar"],
    "Flirt": ["Coquetear", "Flirter", "Flirten", "Flirtejar"],
    "Fracture / Extract": ["Fracturar / Extraer", "Fracturer / Extraire", "Brechen / Abbauen", "Fracturar / Extreure"],
    "Front": ["Frente", "Avant", "Vorne", "Davant"],
    "G-Force Safety": ["Seguridad G", "Sécurité G", "G-Sicherung", "Seguretat de forces G"],
    "G-Safety": ["Seguridad G", "Sécurité G", "G-Sicherung", "Seguretat G"],
    "Gasp": ["Asombro", "Stupeur", "Keuchen", "Esbufec"],
    "Gloat": ["Regodearse", "Jubiler", "Triumphieren", "Presumir"],
    "Greet": ["Saludar", "Saluer", "Grüßen", "Saludar"],
    "Guns": ["Armas", "Canons", "Waffen", "Canons"],
    "Guns Mode": ["Modo armas", "Mode canons", "Waffenmodus", "Mode de canons"],
    "Inspect Item": ["Inspeccionar objeto", "Inspecter l'objet", "Gegenstand prüfen", "Inspeccionar objecte"],
    "Inventory": ["Inventario", "Inventaire", "Inventar", "Inventari"],
    "Landing Gear": ["Tren de aterrizaje", "Train d'atterrissage", "Fahrwerk", "Tren d'aterratge"],
    "Laser Power +": ["Potencia láser +", "Puissance laser +", "Laserleistung +", "Potència del làser +"],
    "Laser Power −": ["Potencia láser −", "Puissance laser −", "Laserleistung −", "Potència del làser −"],
    "Laugh": ["Reír", "Rire", "Lachen", "Riure"],
    "Launch": ["Lanzar", "Lancer", "Abfeuern", "Llançament"],
    "Launch Missile": ["Lanzar misil", "Lancer un missile", "Rakete abfeuern", "Llança un míssil"],
    "Launch Missile (Cinematic)": ["Lanzar misil (cinemático)", "Lancer un missile (cinématique)", "Rakete abfeuern (Kino)", "Llança un míssil (cinemàtic)"],
    "Lead / Lag Pips": ["Marcas de avance", "Repères d'avance", "Vorhaltemarken", "Indicadors d'avançament"],
    "Left": ["Izquierda", "Gauche", "Links", "Esquerra"],
    "Lights": ["Luces", "Phares", "Licht", "Llums"],
    "Lock All Doors": ["Bloquear todas las puertas", "Verrouiller toutes les portes", "Alle Türen verriegeln", "Bloquejar totes les portes"],
    "Lock Doors": ["Bloquear puertas", "Verrouiller les portes", "Türen verriegeln", "Bloquejar les portes"],
    "Look Behind": ["Mirar atrás", "Regarder derrière", "Nach hinten sehen", "Mirar enrere"],
    "Look Behind (hold)": ["Mirar atrás (mantener)", "Regarder derrière (maintenir)", "Nach hinten sehen (halten)", "Mirar enrere (mantenir)"],
    "Master Mode: NAV": ["Modo maestro: NAV", "Mode maître : NAV", "Hauptmodus: NAV", "Mode mestre: NAV"],
    "Master Mode: SCM": ["Modo maestro: SCM", "Mode maître : SCM", "Hauptmodus: SCM", "Mode mestre: SCM"],
    "Master: NAV": ["Maestro: NAV", "Maître : NAV", "Haupt: NAV", "Mestre: NAV"],
    "Master: SCM": ["Maestro: SCM", "Maître : SCM", "Haupt: SCM", "Mestre: SCM"],
    "Match Speed": ["Igualar velocidad", "Aligner la vitesse", "Tempo angleichen", "Iguala la velocitat"],
    "Match Target Speed": ["Igualar vel. objetivo", "Aligner sur la cible", "Zieltempo angleichen", "Iguala la velocitat de l'objectiu"],
    "Max Shields": ["Escudos máx.", "Boucliers max", "Schilde max", "Escuts al màxim"],
    "Max Thrusters": ["Propulsores máx.", "Propulseurs max", "Triebwerke max", "Propulsors al màxim"],
    "Max Weapons": ["Armas máx.", "Armes max", "Waffen max", "Armes al màxim"],
    "Min Shields": ["Escudos mín.", "Boucliers min", "Schilde min", "Escuts al mínim"],
    "Min Thrusters": ["Propulsores mín.", "Propulseurs min", "Triebwerke min", "Propulsors al mínim"],
    "Min Weapons": ["Armas mín.", "Armes min", "Waffen min", "Armes al mínim"],
    "Mining": ["Minería", "Minage", "Bergbau", "Mineria"],
    "Mining Mode": ["Modo minería", "Mode minage", "Bergbaumodus", "Mode de mineria"],
    "Missile Mode": ["Modo misiles", "Mode missiles", "Raketenmodus", "Mode de míssils"],
    "Missiles": ["Misiles", "Missiles", "Raketen", "Míssils"],
    "Modifier Left": ["Modificador izq.", "Modificateur gauche", "Modifikator links", "Modificador esq."],
    "Modifier Right": ["Modificador der.", "Modificateur droite", "Modifikator rechts", "Modificador dret"],
    "Module 1": ["Módulo 1", "Module 1", "Modul 1", "Mòdul 1"],
    "Module 2": ["Módulo 2", "Module 2", "Modul 2", "Mòdul 2"],
    "Module 3": ["Módulo 3", "Module 3", "Modul 3", "Mòdul 3"],
    "Noise": ["Ruido", "Brouilleur", "Störsignal", "Soroll"],
    "Open All Doors": ["Abrir todas las puertas", "Ouvrir toutes les portes", "Alle Türen öffnen", "Obrir totes les portes"],
    "Open Doors": ["Abrir puertas", "Ouvrir les portes", "Türen öffnen", "Obrir les portes"],
    "Oprt Mode": ["Modo oper.", "Mode opér.", "Bedienmodus", "Mode oper."],
    "Panic Decoys": ["Señuelos de pánico", "Leurres d'urgence", "Panik-Täuschkörper", "Contramesures d'emergència"],
    "Point": ["Señalar", "Pointer", "Zeigen", "Assenyalar"],
    "Power On/Off": ["Encender/apagar", "Marche/arrêt", "Energie an/aus", "Energia: on/off"],
    "Power: Engines": ["Energía: motores", "Énergie : moteurs", "Energie: Triebwerke", "Energia: motors"],
    "Power: Reset": ["Energía: reiniciar", "Énergie : réinit.", "Energie: zurücksetzen", "Energia: restablir"],
    "Power: Shields": ["Energía: escudos", "Énergie : boucliers", "Energie: Schilde", "Energia: escuts"],
    "Power: Weapons": ["Energía: armas", "Énergie : armes", "Energie: Waffen", "Energia: armes"],
    "Prev Any": ["Anterior: cualquiera", "Précédent : tous", "Vorheriges: alle", "Anterior qualsevol"],
    "Prev Any Target": ["Objetivo anterior", "Cible précédente", "Vorheriges Ziel", "Objectiu anterior"],
    "Prev Attacker": ["Atacante anterior", "Attaquant précédent", "Vorheriger Angreifer", "Atacant anterior"],
    "Prev Friendly": ["Aliado anterior", "Allié précédent", "Vorheriger Verbündeter", "Aliat anterior"],
    "Prev Hostile": ["Hostil anterior", "Hostile précédent", "Vorheriger Feind", "Hostil anterior"],
    "Prev In View": ["Anterior a la vista", "Précédent en vue", "Vorheriges in Sicht", "Anterior visible"],
    "Prev Subtarget": ["Subobjetivo anterior", "Sous-cible précédente", "Vorheriges Teilziel", "Subobjectiu anterior"],
    "Quantum Jump (hold)": ["Salto Quantum (mantener)", "Saut Quantum (maintenir)", "Quantensprung (halten)", "Salt Quantum (mantén premut)"],
    "Quantum Mode": ["Modo Quantum", "Mode Quantum", "Quantum-Modus", "Mode Quantum"],
    "Remote Turret 1": ["Torreta remota 1", "Tourelle distante 1", "Fernturm 1", "Torreta remota 1"],
    "Remote Turret 2": ["Torreta remota 2", "Tourelle distante 2", "Fernturm 2", "Torreta remota 2"],
    "Remote Turret 3": ["Torreta remota 3", "Tourelle distante 3", "Fernturm 3", "Torreta remota 3"],
    "Request Docking": ["Solicitar atraque", "Demander l'amarrage", "Andocken anfragen", "Sol·licita acoblament"],
    "Request Landing": ["Solicitar aterrizaje", "Demander l'atterrissage", "Landung anfragen", "Sol·licita aterratge"],
    "Reset": ["Reiniciar", "Réinitialiser", "Zurücksetzen", "Restableix"],
    "Reset All": ["Reiniciar todo", "Tout réinitialiser", "Alles zurücksetzen", "Restablir-ho tot"],
    "Reset Limit": ["Reiniciar límite", "Réinit. limite", "Limit zurücksetzen", "Restablir el límit"],
    "Reset Speed Limit": ["Reiniciar límite de velocidad", "Réinit. limite de vitesse", "Tempolimit zurücksetzen", "Restableix el límit de velocitat"],
    "Right": ["Derecha", "Droite", "Rechts", "Dreta"],
    "Rude": ["Grosero", "Grossier", "Unhöflich", "Mal educat"],
    "Salute": ["Saludo militar", "Salut militaire", "Salutieren", "Salutació militar"],
    "Salvage": ["Recuperación", "Récupération", "Bergung", "Desballestament"],
    "Salvage Mode": ["Modo recuperación", "Mode récupération", "Bergungsmodus", "Mode de desballestament"],
    "Scan Mode": ["Modo escaneo", "Mode scan", "Scanmodus", "Mode d'escaneig"],
    "Scanning": ["Escaneo", "Scan", "Scannen", "Escaneig"],
    "Self-D": ["Autodest.", "Autodest.", "Selbstzerst.", "Autodestr."],
    "Shields +": ["Escudos +", "Boucliers +", "Schilde +", "Escuts +"],
    "Shields −": ["Escudos −", "Boucliers −", "Schilde −", "Escuts −"],
    "Shields Back": ["Escudos atrás", "Boucliers arrière", "Schilde hinten", "Escuts posteriors"],
    "Shields Bottom": ["Escudos abajo", "Boucliers bas", "Schilde unten", "Escuts inferiors"],
    "Shields Front": ["Escudos frente", "Boucliers avant", "Schilde vorne", "Escuts frontals"],
    "Shields Left": ["Escudos izquierda", "Boucliers gauche", "Schilde links", "Escuts esquerres"],
    "Shields Reset": ["Escudos: reiniciar", "Boucliers : réinit.", "Schilde zurücksetzen", "Restableix escuts"],
    "Shields Right": ["Escudos derecha", "Boucliers droite", "Schilde rechts", "Escuts drets"],
    "Shields Top": ["Escudos arriba", "Boucliers haut", "Schilde oben", "Escuts superiors"],
    "Sit": ["Sentarse", "S'asseoir", "Hinsetzen", "Seure"],
    "Sleep": ["Dormir", "Dormir", "Schlafen", "Dormir"],
    "Slide to eject": ["Desliza para eyectar", "Glisser pour s'éjecter", "Schieben zum Ausstoßen", "Llisca per ejectar"],
    "Smell": ["Oler", "Renifler", "Riechen", "Olorar"],
    "Speed Limiter": ["Limitador de velocidad", "Limiteur de vitesse", "Tempolimit", "Limitador de velocitat"],
    "Speed Range +": ["Rango de velocidad +", "Plage de vitesse +", "Tempobereich +", "Rang de velocitat +"],
    "Speed Range −": ["Rango de velocidad −", "Plage de vitesse −", "Tempobereich −", "Rang de velocitat −"],
    "Speed Range Down": ["Bajar rango de velocidad", "Baisser la plage de vitesse", "Tempobereich runter", "Reduir el rang de velocitat"],
    "Speed Range Up": ["Subir rango de velocidad", "Augmenter la plage de vitesse", "Tempobereich rauf", "Augmentar el rang de velocitat"],
    "Star Map": ["Mapa estelar", "Carte stellaire", "Sternenkarte", "Mapa estel·lar"],
    "Switch mode": ["Cambiar modo", "Changer de mode", "Modus wechseln", "Canviar de mode"],
    "Taunt": ["Provocar", "Provoquer", "Verhöhnen", "Provocar"],
    "Threaten": ["Amenazar", "Menacer", "Drohen", "Amenaçar"],
    "Thrusters +": ["Propulsores +", "Propulseurs +", "Triebwerke +", "Propulsors +"],
    "Thrusters −": ["Propulsores −", "Propulseurs −", "Triebwerke −", "Propulsors −"],
    "Toggle All Doors": ["Alternar todas las puertas", "Basculer toutes les portes", "Alle Türen umschalten", "Alternar totes les portes"],
    "Toggle Doors": ["Alternar puertas", "Basculer les portes", "Türen umschalten", "Alternar les portes"],
    "Toggle ESP": ["Alternar ESP", "Basculer ESP", "ESP umschalten", "Alternar ESP"],
    "Toggle Flight Ready": ["Listo para volar", "Prêt au vol", "Flugbereit", "Alternar preparació de vol"],
    "Top": ["Arriba", "Haut", "Oben", "Superior"],
    "Turret 1": ["Torreta 1", "Tourelle 1", "Turm 1", "Torreta 1"],
    "Turret 2": ["Torreta 2", "Tourelle 2", "Turm 2", "Torreta 2"],
    "Turret 3": ["Torreta 3", "Tourelle 3", "Turm 3", "Torreta 3"],
    "Unlock All Doors": ["Desbloquear todas las puertas", "Déverrouiller toutes les portes", "Alle Türen entriegeln", "Desbloquejar totes les portes"],
    "Unlock Doors": ["Desbloquear puertas", "Déverrouiller les portes", "Türen entriegeln", "Desbloquejar les portes"],
    "Wait": ["Espera", "Attends", "Warte", "Esperar"],
    "Walk": ["Caminar", "Marcher", "Gehen", "Caminar"],
    "Wave": ["Agitar la mano", "Faire signe", "Winken", "Saludar amb la mà"],
    "Weapons +": ["Armas +", "Armes +", "Waffen +", "Armes +"],
    "Weapons −": ["Armas −", "Armes −", "Waffen −", "Armes −"],
    "Whistle": ["Silbar", "Siffler", "Pfeifen", "Xiular"],
  };

  // The tablet's own language if there's a translation, else English; a choice in Deck
  // settings wins and is kept per device.
  let lang = "en";
  try { lang = localStorage.getItem("lang") || ""; } catch { /* no storage: default below */ }
  if (!known(lang) && lang !== "en") lang = (navigator.language || "en").slice(0, 2).toLowerCase();
  if (!known(lang)) lang = "en";
  document.documentElement.lang = lang;

  // Translates English text; "{0}" takes the first extra argument. Phone layouts' names end
  // in " (iPhone)", which stays after the translated name.
  function t(text, ...args) {
    let out = text;
    if (lang !== "en" && typeof text === "string") {
      const row = T[text];
      if (row) out = row[COLUMN[lang]];
      else if (text.endsWith(" (iPhone)") && T[text.slice(0, -9)]) out = `${T[text.slice(0, -9)][COLUMN[lang]]} (iPhone)`;
    }
    return args.length ? String(out).replace("{0}", args[0]) : out;
  }

  // Translates the page's fixed text (Deck settings, the intro, the voice list) outside the
  // deck itself, which is drawn with t(). The English originals are kept, so switching
  // languages again starts from them.
  const originals = new WeakMap();
  const ATTRS = ["aria-label", "placeholder", "title"];
  function apply(root = document.body) {
    const skip = (el) => el.closest && el.closest("#stage, #grid, #tabs, #cards, #layoutPick, #langPick, #pcLinkBtns, #voiceList, #toast, #title, #statusText, .js-status, .js-intro-maker, script, style");
    const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT);
    for (let node = walker.nextNode(); node; node = walker.nextNode()) {
      if (skip(node.parentElement)) continue;
      if (!originals.has(node)) originals.set(node, node.nodeValue);
      const original = originals.get(node), key = original.trim();
      if (key && T[key]) node.nodeValue = original.replace(key, t(key));
      else node.nodeValue = original;
    }
    for (const el of root.querySelectorAll(ATTRS.map((a) => `[${a}]`).join(","))) {
      if (skip(el)) continue;
      for (const a of ATTRS) {
        if (!el.hasAttribute(a)) continue;
        const key = `data-en-${a}`;
        if (!el.hasAttribute(key)) el.setAttribute(key, el.getAttribute(a));
        el.setAttribute(a, t(el.getAttribute(key)));
      }
    }
  }

  function set(next) {
    lang = known(next) ? next : "en";
    try { localStorage.setItem("lang", lang); } catch { /* not kept */ }
    document.documentElement.lang = lang;
  }

  window.DeckI18n = { languages: LANGUAGES, get lang() { return lang; }, set, t, apply };
})();
