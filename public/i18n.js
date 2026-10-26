// The deck's languages. Text is translated where it's shown: labels, panel titles, status
// words and the deck's own messages. Layout files, keys and voice phrases stay English, so
// nothing a button presses or listens for changes. Star Citizen's own terms (Quantum, SCM,
// NAV, VTOL, ESP, ATC, mobiGlas) stay as the game shows them.
//
// T maps English text to [Español, Français, Deutsch, 中文]. "{0}" in a text is filled in by t().
// Text with no entry shows in English, so your own layouts keep the labels you gave them.
// The translations were made with AI; corrections from native speakers are welcome.
(() => {
  const LANGUAGES = [["en", "English"], ["es", "Español"], ["fr", "Français"], ["de", "Deutsch"], ["zh", "中文"]];
  const COLUMN = { es: 0, fr: 1, de: 2, zh: 3 };
  const known = (l) => Object.prototype.hasOwnProperty.call(COLUMN, l);

  const T = {
    // ---------- the deck's own text ----------
    "Deck settings": ["Ajustes del deck", "Réglages du deck", "Deck-Einstellungen", "卡组设置"],
    "Close": ["Cerrar", "Fermer", "Schließen", "关闭"],
    "Layout": ["Diseño", "Disposition", "Layout", "布局"],
    "Language": ["Idioma", "Langue", "Sprache", "语言"],
    "Each tablet and phone keeps its own choice.": ["Cada tablet y móvil guarda su propia elección.", "Chaque tablette et téléphone garde son propre choix.", "Jedes Tablet und Handy merkt sich seine eigene Wahl.", "每个平板和手机都保持自己的选择。"],
    "Display": ["Pantalla", "Affichage", "Anzeige", "显示"],
    "Night mode": ["Modo nocturno", "Mode nuit", "Nachtmodus", "夜间模式"],
    "Dims the deck for a dark room.": ["Atenúa el deck para una habitación oscura.", "Assombrit le deck pour une pièce sombre.", "Dimmt das Deck für einen dunklen Raum.", "在黑暗房间中调暗卡组。"],
    "Large text": ["Texto grande", "Grand texte", "Große Schrift", "大字体"],
    "Bigger, bolder labels, easier to read at a glance.": ["Etiquetas más grandes y gruesas, fáciles de leer de un vistazo.", "Libellés plus grands et plus gras, lisibles d'un coup d'œil.", "Größere, fettere Beschriftungen, auf einen Blick lesbar.", "更大更粗的标签，一目了然。"],
    "High contrast": ["Alto contraste", "Contraste élevé", "Hoher Kontrast", "高对比度"],
    "Black and white with strong colours, over any theme.": ["Blanco y negro con colores intensos, sobre cualquier tema.", "Noir et blanc aux couleurs vives, avec n'importe quel thème.", "Schwarz-Weiß mit kräftigen Farben, für jedes Theme.", "黑白配鲜艳色彩，适用于任何主题。"],
    "Two-finger swipe": ["Deslizar con dos dedos", "Balayage à deux doigts", "Mit zwei Fingern wischen", "双指滑动"],
    "Swipe sideways over a half to flip its panels. Presses then wait a moment.": ["Desliza de lado sobre una mitad para pasar sus paneles. Las pulsaciones esperan un momento.", "Balayez latéralement une moitié pour changer de panneau. Les appuis attendent alors un instant.", "Seitlich über eine Hälfte wischen, um ihre Panels zu blättern. Antippen reagiert dann etwas verzögert.", "横向滑动一半区域来翻转面板。按下后稍等片刻。"],
    "Button lights": ["Luces de botones", "Voyants des boutons", "Tastenbeleuchtung", "按钮灯光"],
    "Light buttons and show ON/OFF while something is on. They follow your taps, not the game.": ["Ilumina los botones y muestra ON/OFF mientras algo está activo. Siguen tus pulsaciones, no el juego.", "Allume les boutons et affiche ON/OFF quand quelque chose est actif. Ils suivent vos appuis, pas le jeu.", "Lässt Tasten leuchten und zeigt AN/AUS, solange etwas an ist. Sie folgen deinem Antippen, nicht dem Spiel.", "按钮亮起并显示开/关，当有功能激活时。它们跟随你的点击，而不是游戏状态。"],
    "Reset button lights": ["Reiniciar luces", "Réinitialiser les voyants", "Beleuchtung zurücksetzen", "重置按钮灯光"],
    "Turns all lit buttons off, e.g. after a respawn or when they don't match the game.": ["Apaga todos los botones iluminados, p. ej. tras reaparecer o si no coinciden con el juego.", "Éteint tous les boutons allumés, par ex. après une réapparition ou s'ils ne correspondent plus au jeu.", "Schaltet alle leuchtenden Tasten aus, z. B. nach einem Respawn oder wenn sie nicht zum Spiel passen.", "关闭所有亮起的按钮，例如在重生后或当它们与游戏状态不匹配时。"],
    "Button lights reset": ["Luces reiniciadas", "Voyants réinitialisés", "Beleuchtung zurückgesetzt", "按钮灯光已重置"],
    "Open on PC": ["Abrir en el PC", "Ouvrir sur le PC", "Am PC öffnen", "在PC上打开"],
    "Opens the site in your PC's browser.": ["Abre la web en el navegador de tu PC.", "Ouvre le site dans le navigateur de votre PC.", "Öffnet die Seite im Browser deines PCs.", "在PC浏览器中打开网站。"],
    "Theme": ["Tema", "Thème", "Theme", "主题"],
    "Ideas or problems?": ["¿Ideas o problemas?", "Des idées ou un problème ?", "Ideen oder Probleme?", "有问题或建议？"],
    "Feedback & ideas on GitHub": ["Comentarios e ideas en GitHub", "Avis et idées sur GitHub", "Feedback & Ideen auf GitHub", "在GitHub上反馈和建议"],
    "Voice commands": ["Comandos de voz", "Commandes vocales", "Sprachbefehle", "语音命令"],
    "Voice commands list": ["Lista de comandos de voz", "Liste des commandes vocales", "Liste der Sprachbefehle", "语音命令列表"],
    "Hold the mic button and say one of these. Most Star Citizen controls are toggles, so \"engines on\" and \"engines off\" press the same key.": ["Mantén pulsado el micrófono y di uno de estos (en inglés). La mayoría de controles de Star Citizen alternan, así que \"engines on\" y \"engines off\" pulsan la misma tecla.", "Maintenez le micro et dites l'une de ces phrases (en anglais). La plupart des commandes de Star Citizen basculent, donc \"engines on\" et \"engines off\" appuient sur la même touche.", "Halte das Mikro gedrückt und sag einen davon (auf Englisch). Die meisten Star-Citizen-Befehle schalten um, daher drücken \"engines on\" und \"engines off\" dieselbe Taste.", "按住麦克风按钮并说出其中一条。大多数星战公民控制是开关式的，所以\"引擎开启\"和\"引擎关闭\"按同一个键。"],
    "Search, e.g. shields": ["Buscar, p. ej. shields", "Rechercher, par ex. shields", "Suchen, z. B. shields", "搜索，例如护盾"],
    "Hold to talk": ["Pulsa y habla", "Tenir pour parler", "Halten & sprechen", "按住说话"],
    "Settings": ["Ajustes", "Réglages", "Einstellungen", "设置"],
    "Fullscreen": ["Pantalla completa", "Plein écran", "Vollbild", "全屏"],
    "Previous panel": ["Panel anterior", "Panneau précédent", "Vorheriges Panel", "上一面板"],
    "Next panel": ["Panel siguiente", "Panneau suivant", "Nächstes Panel", "下一面板"],
    "Turn your phone sideways": ["Gira el móvil", "Tournez votre téléphone", "Dreh dein Handy quer", "将手机横过来"],
    "Tap to continue": ["Toca para continuar", "Touchez pour continuer", "Tippen zum Fortfahren", "点击继续"],
    "Star Citizen control deck": ["Panel de control para Star Citizen", "Deck de commande pour Star Citizen", "Steuerdeck für Star Citizen", "星战公民控制面板"],
    "› HUD systems": ["› Sistemas HUD", "› Systèmes HUD", "› HUD-Systeme", "› HUD系统"],
    "› Hull profile": ["› Perfil del casco", "› Profil de coque", "› Rumpfprofil", "› 船体配置"],
    "› PC uplink": ["› Enlace con el PC", "› Liaison PC", "› PC-Verbindung", "› PC连接"],
    "Loading layout…": ["Cargando diseño…", "Chargement de la disposition…", "Layout wird geladen…", "加载布局中…"],
    "Connecting…": ["Conectando…", "Connexion…", "Verbinde…", "连接中…"],
    "Connected": ["Conectado", "Connecté", "Verbunden", "已连接"],
    "Reconnecting…": ["Reconectando…", "Reconnexion…", "Verbinde neu…", "重新连接中…"],
    "Not paired": ["Sin emparejar", "Non appairé", "Nicht gekoppelt", "未配对"],
    "Demo": ["Demo", "Démo", "Demo", "演示"],
    "Error": ["Error", "Erreur", "Fehler", "错误"],
    "Star Citizen not focused": ["Star Citizen no está en primer plano", "Star Citizen n'est pas au premier plan", "Star Citizen ist nicht im Vordergrund", "星战公民未在前台"],
    "Heard: {0}": ["Oído: {0}", "Entendu : {0}", "Gehört: {0}", "听到：{0}"],
    "Didn't catch that": ["No te he entendido", "Je n'ai pas compris", "Nicht verstanden", "没听清"],
    "{0}: test mode, not opened": ["{0}: modo de prueba, no se abrió", "{0} : mode test, non ouvert", "{0}: Testmodus, nicht geöffnet", "{0}: 测试模式，未打开"],
    "Opened {0} on the PC": ["{0} abierto en el PC", "{0} ouvert sur le PC", "{0} am PC geöffnet", "在PC上打开了{0}"],
    "With the app, this opens {0} on your PC": ["Con la app, esto abre {0} en tu PC", "Avec l'appli, ceci ouvre {0} sur votre PC", "Mit der App öffnet das {0} auf deinem PC", "使用应用会在PC上打开{0}"],
    "Voice commands work with Tablet Deck on your PC": ["Los comandos de voz funcionan con Tablet Deck en tu PC", "Les commandes vocales fonctionnent avec Tablet Deck sur votre PC", "Sprachbefehle funktionieren mit Tablet Deck auf deinem PC", "语音命令与PC上的Tablet Deck配合工作"],
    "Demo: taps play their animations; with the app, they press keys in Star Citizen": ["Demo: los toques muestran sus animaciones; con la app, pulsan teclas en Star Citizen", "Démo : les appuis jouent leurs animations ; avec l'appli, ils appuient sur des touches dans Star Citizen", "Demo: Antippen spielt die Animationen ab; mit der App drückt es Tasten in Star Citizen", "演示：点击播放动画；使用应用时，它们会在星战公民中按键"],
    "Loading…": ["Cargando…", "Chargement…", "Lädt…", "加载中…"],
    "Couldn't load the list from the PC.": ["No se pudo cargar la lista desde el PC.", "Impossible de charger la liste depuis le PC.", "Die Liste konnte nicht vom PC geladen werden.", "无法从PC加载列表。"],
    "Voice commands are turned off on the PC. Turn them on in the Tablet Deck window's Settings.": ["Los comandos de voz están desactivados en el PC. Actívalos en Ajustes de la ventana de Tablet Deck.", "Les commandes vocales sont désactivées sur le PC. Activez-les dans les réglages de la fenêtre Tablet Deck.", "Sprachbefehle sind am PC ausgeschaltet. Schalte sie in den Einstellungen des Tablet-Deck-Fensters ein.", "PC上已关闭语音命令。请在Tablet Deck窗口的设置中启用它们。"],
    "Voice commands can't run on this PC{0}. The Tablet Deck window shows what to do.": ["Los comandos de voz no pueden funcionar en este PC{0}. La ventana de Tablet Deck indica qué hacer.", "Les commandes vocales ne peuvent pas fonctionner sur ce PC{0}. La fenêtre Tablet Deck indique quoi faire.", "Sprachbefehle laufen auf diesem PC nicht{0}. Das Tablet-Deck-Fenster zeigt, was zu tun ist.", "此PC{0}无法运行语音命令。Tablet Deck窗口会显示如何操作。"],
    "The PC is still starting voice commands. Try again in a moment.": ["El PC aún está iniciando los comandos de voz. Inténtalo en un momento.", "Le PC démarre encore les commandes vocales. Réessayez dans un instant.", "Der PC startet die Sprachbefehle noch. Versuch es gleich noch einmal.", "PC仍在启动语音命令。请稍后再试。"],
    "Nothing matches that.": ["No hay coincidencias.", "Aucun résultat.", "Keine Treffer.", "没有匹配结果。"],
    "Pair this device": ["Emparejar este dispositivo", "Appairer cet appareil", "Dieses Gerät koppeln", "配对此设备"],
    "Open {0} on your gaming PC and type the pairing code its window shows.": ["Abre {0} en tu PC de juego y escribe el código de emparejamiento que muestra su ventana.", "Ouvrez {0} sur votre PC de jeu et saisissez le code d'appairage affiché dans sa fenêtre.", "Öffne {0} auf deinem Gaming-PC und gib den Kopplungscode aus seinem Fenster ein.", "在你的游戏PC上打开{0}并输入其窗口显示的配对码。"],
    "Pairing code": ["Código de emparejamiento", "Code d'appairage", "Kopplungscode", "配对码"],
    "e.g. 1a2b3c4d": ["p. ej. 1a2b3c4d", "par ex. 1a2b3c4d", "z. B. 1a2b3c4d", "例如 1a2b3c4d"],
    "Connect": ["Conectar", "Connecter", "Verbinden", "连接"],
    "Quicker: scan the QR code in the Tablet Deck window with this device's camera.": ["Más rápido: escanea el código QR de la ventana de Tablet Deck con la cámara de este dispositivo.", "Plus rapide : scannez le code QR de la fenêtre Tablet Deck avec l'appareil photo de cet appareil.", "Schneller: Scanne den QR-Code im Tablet-Deck-Fenster mit der Kamera dieses Geräts.", "更快捷：用此设备的相机扫描Tablet Deck窗口中的二维码。"],
    "Too many wrong codes. Wait a minute, then try again.": ["Demasiados códigos incorrectos. Espera un minuto y vuelve a intentarlo.", "Trop de codes erronés. Attendez une minute, puis réessayez.", "Zu viele falsche Codes. Warte eine Minute und versuch es dann erneut.", "错误码过多。请等待一分钟后重试。"],
    "That code didn't match. Check the code in the Tablet Deck window on your PC and try again.": ["Ese código no coincide. Comprueba el código en la ventana de Tablet Deck de tu PC y vuelve a intentarlo.", "Ce code ne correspond pas. Vérifiez le code dans la fenêtre Tablet Deck de votre PC et réessayez.", "Der Code stimmt nicht. Prüfe den Code im Tablet-Deck-Fenster auf deinem PC und versuch es erneut.", "代码不匹配。请检查PC上Tablet Deck窗口中的代码并重试。"],
    "Type the pairing code first.": ["Escribe primero el código de emparejamiento.", "Saisissez d'abord le code d'appairage.", "Gib zuerst den Kopplungscode ein.", "请先输入配对码。"],
    "Layout error ({0})": ["Error de diseño ({0})", "Erreur de disposition ({0})", "Layout-Fehler ({0})", "布局错误 ({0})"],
    "PC's choice: {0}": ["Elección del PC: {0}", "Choix du PC : {0}", "Wahl des PCs: {0}", "PC的选择：{0}"],
    "{0} (yours)": ["{0} (tuyo)", "{0} (le vôtre)", "{0} (deins)", "{0} (你的)"],
    "Hold": ["Mantener", "Maintenir", "Halten", "按住"],
    "Long press": ["Pulsación larga", "Appui long", "Lang drücken", "长按"],

    // ---------- theme taglines ----------
    "Classic interstellar lines": ["Líneas interestelares clásicas", "Lignes interstellaires classiques", "Klassische interstellare Linien", "经典星际线条"],
    "Built rough, flown hard": ["Construido a lo bruto, pilotado a tope", "Construit brut, piloté dur", "Rau gebaut, hart geflogen", "粗糙建造，飞行艰难"],
    "Military precision": ["Precisión militar", "Précision militaire", "Militärische Präzision", "军事精度"],
    "Frontline hardware": ["Equipo de primera línea", "Matériel de première ligne", "Frontausrüstung", "前线装备"],
    "Luxury in the void": ["Lujo en el vacío", "Le luxe dans le vide", "Luxus im All", "太空中的奢华"],
    "Industrial workhorses": ["Caballos de batalla industriales", "Bêtes de somme industrielles", "Industrielle Arbeitstiere", "工业劳力者"],
    "Open skies, big haulers": ["Cielos abiertos, grandes cargueros", "Grands espaces, gros cargos", "Weiter Himmel, große Frachter", "开放天空，大型货船"],
    "Alien craft, merchant spirit": ["Naves alienígenas, espíritu mercante", "Vaisseaux extraterrestres, esprit marchand", "Fremde Schiffe, Händlergeist", "外星飞船，商人精神"],
    "Xi'an elegance": ["Elegancia xi'an", "Élégance xi'an", "Xi'an-Eleganz", "希安优雅"],
    "Utility first": ["Lo práctico primero", "L'utilité d'abord", "Nutzen zuerst", "实用优先"],
    "Frontier newcomer": ["Recién llegado de la frontera", "Nouveau venu de la frontière", "Neuling an der Grenze", "边境新来者"],
    "Reborn Tevarin designs": ["Diseños tevarin renacidos", "Designs tevarin renaissants", "Wiedergeborene Tevarin-Designs", "重生的Tevarin设计"],

    // ---------- layouts, tabs, panels and headers ----------
    "Mining & Salvage": ["Minería y recuperación", "Minage et récupération", "Bergbau & Bergung", "采矿&打捞"],
    "Hauling & Exploration": ["Transporte y exploración", "Transport et exploration", "Transport & Erkundung", "运输&探索"],
    "Star Citizen (tabbed grid)": ["Star Citizen (cuadrícula)", "Star Citizen (grille)", "Star Citizen (Raster)", "星战公民（选项卡网格）"],
    "Combat": ["Combate", "Combat", "Kampf", "战斗"],
    "Landing": ["Aterrizaje", "Atterrissage", "Landung", "降落"],
    "Power": ["Energía", "Énergie", "Energie", "动力"],
    "QT/Systems": ["QT/Sistemas", "QT/Systèmes", "QT/Systeme", "量子/系统"],
    "Operator mode": ["Modo operador", "Mode opérateur", "Bedienmodus", "操作员模式"],
    "Operator Modes": ["Modos de operador", "Modes opérateur", "Bedienmodi", "操作员模式"],
    "Emergency": ["Emergencia", "Urgence", "Notfall", "紧急"],
    "Flight Assist": ["Asistencia de vuelo", "Assistance de vol", "Flugassistenz", "飞行辅助"],
    "Shields": ["Escudos", "Boucliers", "Schilde", "护盾"],
    "Targeting": ["Objetivos", "Ciblage", "Zielerfassung", "瞄准"],
    "Crew/Doors": ["Tripulación/Puertas", "Équipage/Portes", "Crew/Türen", "船员/舱门"],
    "On Foot": ["A pie", "À pied", "Zu Fuß", "步行"],
    "Emotes": ["Gestos", "Emotes", "Emotes", "表情"],
    "Emotes 1": ["Gestos 1", "Emotes 1", "Emotes 1", "表情1"],
    "Emotes 2": ["Gestos 2", "Emotes 2", "Emotes 2", "表情2"],
    "Power/Landing": ["Energía/Aterrizaje", "Énergie/Atterrissage", "Energie/Landung", "动力/降落"],
    "Prospector Flight": ["Prospector: vuelo", "Prospector : vol", "Prospector: Flug", "矿工飞行器：飞行"],
    "Vulture Flight": ["Vulture: vuelo", "Vulture : vol", "Vulture: Flug", "秃鹫飞行器：飞行"],
    "Modes": ["Modos", "Modes", "Modi", "模式"],
    "Targets": ["Objetivos", "Cibles", "Ziele", "目标"],
    "Crew": ["Tripulación", "Équipage", "Crew", "船员"],
    "Ship_systems": ["Sistemas_nave", "Systèmes_vaisseau", "Schiffssysteme", "飞船系统"],
    "Power_management": ["Gestión_energía", "Gestion_énergie", "Energieverwaltung", "能源管理"],
    "Laser": ["Láser", "Laser", "Laser", "激光"],
    "Modules": ["Módulos", "Modules", "Module", "模块"],
    "Ship": ["Nave", "Vaisseau", "Schiff", "飞船"],
    "Doors": ["Puertas", "Portes", "Türen", "舱门"],
    "Landing mode": ["Modo aterrizaje", "Mode atterrissage", "Landemodus", "降落模式"],
    "Docking mode": ["Modo atraque", "Mode amarrage", "Andockmodus", "对接模式"],
    "ENABLED": ["ACTIVO", "ACTIF", "AKTIV", "已启用"],
    "Max": ["Máx", "Max", "Max", "最大"],
    "Incr": ["Más", "Plus", "Mehr", "增加"],
    "Decr": ["Menos", "Moins", "Weniger", "减少"],
    "Min": ["Mín", "Min", "Min", "最小"],
    "Weapons": ["Armas", "Armes", "Waffen", "武器"],
    "Thrusters": ["Propulsores", "Propulseurs", "Triebwerke", "推进器"],

    // ---------- status words ----------
    "ON": ["ON", "ON", "AN", "开"],
    "OFF": ["OFF", "OFF", "AUS", "关"],
    "ONLINE": ["EN LÍNEA", "EN LIGNE", "ONLINE", "在线"],
    "DEPLOYED": ["DESPLEGADO", "DÉPLOYÉ", "AUSGEFAHREN", "已放下"],
    "RETRACTED": ["RETRAÍDO", "RENTRÉ", "EINGEFAHREN", "已收起"],
    "DEPLOYING…": ["DESPLEGANDO…", "DÉPLOIEMENT…", "FÄHRT AUS…", "放下中…"],
    "RETRACTING…": ["RETRAYENDO…", "RENTRÉE…", "FÄHRT EIN…", "收起中…"],
    "HOLD TO ENGAGE": ["MANTÉN PARA ACTIVAR", "MAINTENIR POUR ENGAGER", "HALTEN ZUM AKTIVIEREN", "按住以起跳"],
    "SPOOLING…": ["CARGANDO…", "CHARGEMENT…", "LÄDT…", "充能中…"],
    "ENGAGED": ["ACTIVADO", "ENGAGÉ", "AKTIVIERT", "已起跳"],
    "OPEN": ["ABIERTO", "OUVERT", "OFFEN", "开"],
    "CLOSED": ["CERRADO", "FERMÉ", "ZU", "关"],
    "OPENING…": ["ABRIENDO…", "OUVERTURE…", "ÖFFNET…", "正在打开…"],
    "CLOSING…": ["CERRANDO…", "FERMETURE…", "SCHLIESST…", "正在关闭…"],
    "ACQUIRING…": ["ADQUIRIENDO…", "ACQUISITION…", "ERFASSE…", "锁定中…"],
    "LOCKED": ["BLOQUEADO", "VERROUILLÉ", "VERRIEGELT", "已锁定"],
    "HOLD TO ARM": ["MANTÉN PARA ARMAR", "TENIR POUR ARMER", "GEDRÜCKT HALTEN", "按住待发"],
    "ARMING…": ["ARMANDO…", "ARMEMENT…", "WIRD SCHARF…", "待发准备中…"],
    "ARMED": ["ARMADO", "ARMÉ", "SCHARF", "已待发"],

    // ---------- button labels ----------
    "3rd Person": ["3.ª persona", "3e personne", "3. Person", "第三人称"],
    "Agree": ["De acuerdo", "D'accord", "Zustimmen", "同意"],
    "Angry": ["Enfadado", "En colère", "Wütend", "生气"],
    "At Ease": ["Descanso", "Repos", "Rührt euch", "稍息"],
    "Attention": ["Firmes", "Garde-à-vous", "Stillgestanden", "立正"],
    "Back": ["Atrás", "Arrière", "Hinten", "后"],
    "Beam Spacing +": ["Separación haz +", "Écart faisceaux +", "Strahlabstand +", "光束间距 +"],
    "Beam Spacing −": ["Separación haz −", "Écart faisceaux −", "Strahlabstand −", "光束间距 −"],
    "Blah": ["Bla", "Bla-bla", "Bla", "吧啦吧啦"],
    "Bored": ["Aburrido", "Ennuyé", "Gelangweilt", "无聊"],
    "Bottom": ["Abajo", "Bas", "Unten", "下"],
    "Bow": ["Reverencia", "Révérence", "Verbeugen", "鞠躬"],
    "Burp": ["Eructar", "Roter", "Rülpsen", "打嗝"],
    "Cheer": ["Animar", "Acclamer", "Jubeln", "欢呼"],
    "Chicken": ["Gallina", "Poule mouillée", "Hühnchen", "胆小鬼"],
    "Clap": ["Aplaudir", "Applaudir", "Klatschen", "鼓掌"],
    "Close All Doors": ["Cerrar todas las puertas", "Fermer toutes les portes", "Alle Türen schließen", "关闭所有舱门"],
    "Close Doors": ["Cerrar puertas", "Fermer les portes", "Türen schließen", "关闭舱门"],
    "Come": ["Ven", "Viens", "Komm", "过来"],
    "Contacts": ["Contactos", "Contacts", "Kontakte", "接触目标"],
    "Cry": ["Llorar", "Pleurer", "Weinen", "哭"],
    "Dance": ["Bailar", "Danser", "Tanzen", "跳舞"],
    "Decoy": ["Señuelo", "Leurre", "Täuschkörper", "诱饵弹"],
    "Disagree": ["En desacuerdo", "Pas d'accord", "Ablehnen", "不同意"],
    "Docking Camera": ["Cámara de atraque", "Caméra d'amarrage", "Andockkamera", "对接摄像头"],
    "Drop Item": ["Soltar objeto", "Lâcher l'objet", "Gegenstand ablegen", "放下物品"],
    "Engage Auto Land": ["Aterrizaje automático", "Atterrissage auto", "Auto-Landung", "启动自动降落"],
    "Engage Quantum": ["Activar Quantum", "Engager Quantum", "Quantum aktivieren", "启动量子跳跃"],
    "Exit Seat": ["Salir del asiento", "Quitter le siège", "Sitz verlassen", "离开座椅"],
    "Failure": ["Fracaso", "Échec", "Fehlschlag", "失败"],
    "Fewer Missiles": ["Menos misiles", "Moins de missiles", "Weniger Raketen", "减少导弹"],
    "Fire Laser": ["Disparar láser", "Tirer au laser", "Laser feuern", "发射激光"],
    "Flex": ["Sacar músculo", "Muscles", "Muskeln zeigen", "秀肌肉"],
    "Flight": ["Vuelo", "Vol", "Flug", "飞行"],
    "Flight Mode": ["Modo vuelo", "Mode vol", "Flugmodus", "飞行模式"],
    "Flight Ready": ["Listo para volar", "Prêt au vol", "Flugbereit", "飞行就绪"],
    "Flirt": ["Coquetear", "Flirter", "Flirten", "调情"],
    "Fracture / Extract": ["Fracturar / Extraer", "Fracturer / Extraire", "Brechen / Abbauen", "破碎 / 提取"],
    "Front": ["Frente", "Avant", "Vorne", "前"],
    "G-Force Safety": ["Seguridad G", "Sécurité G", "G-Sicherung", "过载保护"],
    "G-Safety": ["Seguridad G", "Sécurité G", "G-Sicherung", "过载保护"],
    "Gasp": ["Asombro", "Stupeur", "Keuchen", "倒吸气"],
    "Gloat": ["Regodearse", "Jubiler", "Triumphieren", "得意"],
    "Greet": ["Saludar", "Saluer", "Grüßen", "打招呼"],
    "Guns": ["Armas", "Canons", "Waffen", "机炮"],
    "Guns Mode": ["Modo armas", "Mode canons", "Waffenmodus", "机炮模式"],
    "Inspect Item": ["Inspeccionar objeto", "Inspecter l'objet", "Gegenstand prüfen", "查看物品"],
    "Inventory": ["Inventario", "Inventaire", "Inventar", "物品栏"],
    "Landing Gear": ["Tren de aterrizaje", "Train d'atterrissage", "Fahrwerk", "起落架"],
    "Laser Power +": ["Potencia láser +", "Puissance laser +", "Laserleistung +", "激光功率 +"],
    "Laser Power −": ["Potencia láser −", "Puissance laser −", "Laserleistung −", "激光功率 −"],
    "Laugh": ["Reír", "Rire", "Lachen", "大笑"],
    "Launch": ["Lanzar", "Lancer", "Abfeuern", "发射"],
    "Launch Missile": ["Lanzar misil", "Lancer un missile", "Rakete abfeuern", "发射导弹"],
    "Launch Missile (Cinematic)": ["Lanzar misil (cinemático)", "Lancer un missile (cinématique)", "Rakete abfeuern (Kino)", "发射导弹(运镜)"],
    "Lead / Lag Pips": ["Marcas de avance", "Repères d'avance", "Vorhaltemarken", "前置 / 滞后准星"],
    "Left": ["Izquierda", "Gauche", "Links", "左"],
    "Lights": ["Luces", "Phares", "Licht", "灯光"],
    "Lock All Doors": ["Bloquear todas las puertas", "Verrouiller toutes les portes", "Alle Türen verriegeln", "锁定所有舱门"],
    "Lock Doors": ["Bloquear puertas", "Verrouiller les portes", "Türen verriegeln", "锁定舱门"],
    "Look Behind": ["Mirar atrás", "Regarder derrière", "Nach hinten sehen", "回头看"],
    "Look Behind (hold)": ["Mirar atrás (mantener)", "Regarder derrière (maintenir)", "Nach hinten sehen (halten)", "回头看(按住)"],
    "Master Mode: NAV": ["Modo maestro: NAV", "Mode maître : NAV", "Hauptmodus: NAV", "主模式:NAV"],
    "Master Mode: SCM": ["Modo maestro: SCM", "Mode maître : SCM", "Hauptmodus: SCM", "主模式:SCM"],
    "Master: NAV": ["Maestro: NAV", "Maître : NAV", "Haupt: NAV", "主模式:NAV"],
    "Master: SCM": ["Maestro: SCM", "Maître : SCM", "Haupt: SCM", "主模式:SCM"],
    "Match Speed": ["Igualar velocidad", "Aligner la vitesse", "Tempo angleichen", "匹配速度"],
    "Match Target Speed": ["Igualar vel. objetivo", "Aligner sur la cible", "Zieltempo angleichen", "匹配目标速度"],
    "Max Shields": ["Escudos máx.", "Boucliers max", "Schilde max", "护盾最大"],
    "Max Thrusters": ["Propulsores máx.", "Propulseurs max", "Triebwerke max", "推进器最大"],
    "Max Weapons": ["Armas máx.", "Armes max", "Waffen max", "武器最大"],
    "Min Shields": ["Escudos mín.", "Boucliers min", "Schilde min", "护盾最小"],
    "Min Thrusters": ["Propulsores mín.", "Propulseurs min", "Triebwerke min", "推进器最小"],
    "Min Weapons": ["Armas mín.", "Armes min", "Waffen min", "武器最小"],
    "Mining": ["Minería", "Minage", "Bergbau", "采矿"],
    "Mining Mode": ["Modo minería", "Mode minage", "Bergbaumodus", "采矿模式"],
    "Missile Mode": ["Modo misiles", "Mode missiles", "Raketenmodus", "导弹模式"],
    "Missiles": ["Misiles", "Missiles", "Raketen", "导弹"],
    "Modifier Left": ["Modificador izq.", "Modificateur gauche", "Modifikator links", "左修饰键"],
    "Modifier Right": ["Modificador der.", "Modificateur droite", "Modifikator rechts", "右修饰键"],
    "Module 1": ["Módulo 1", "Module 1", "Modul 1", "模块 1"],
    "Module 2": ["Módulo 2", "Module 2", "Modul 2", "模块 2"],
    "Module 3": ["Módulo 3", "Module 3", "Modul 3", "模块 3"],
    "Noise": ["Ruido", "Brouilleur", "Störsignal", "干扰"],
    "Open All Doors": ["Abrir todas las puertas", "Ouvrir toutes les portes", "Alle Türen öffnen", "打开所有舱门"],
    "Open Doors": ["Abrir puertas", "Ouvrir les portes", "Türen öffnen", "打开舱门"],
    "Oprt Mode": ["Modo oper.", "Mode opér.", "Bedienmodus", "操作模式"],
    "Panic Decoys": ["Señuelos de pánico", "Leurres d'urgence", "Panik-Täuschkörper", "紧急诱饵弹"],
    "Point": ["Señalar", "Pointer", "Zeigen", "指向"],
    "Power On/Off": ["Encender/apagar", "Marche/arrêt", "Energie an/aus", "电源开/关"],
    "Power: Engines": ["Energía: motores", "Énergie : moteurs", "Energie: Triebwerke", "电源:引擎"],
    "Power: Reset": ["Energía: reiniciar", "Énergie : réinit.", "Energie: zurücksetzen", "电源:重置"],
    "Power: Shields": ["Energía: escudos", "Énergie : boucliers", "Energie: Schilde", "电源:护盾"],
    "Power: Weapons": ["Energía: armas", "Énergie : armes", "Energie: Waffen", "电源:武器"],
    "Prev Any": ["Anterior: cualquiera", "Précédent : tous", "Vorheriges: alle", "上一个:任意"],
    "Prev Any Target": ["Objetivo anterior", "Cible précédente", "Vorheriges Ziel", "上一个目标"],
    "Prev Attacker": ["Atacante anterior", "Attaquant précédent", "Vorheriger Angreifer", "上一个攻击者"],
    "Prev Friendly": ["Aliado anterior", "Allié précédent", "Vorheriger Verbündeter", "上一个友军"],
    "Prev Hostile": ["Hostil anterior", "Hostile précédent", "Vorheriger Feind", "上一个敌对目标"],
    "Prev In View": ["Anterior a la vista", "Précédent en vue", "Vorheriges in Sicht", "上一个视野内"],
    "Prev Subtarget": ["Subobjetivo anterior", "Sous-cible précédente", "Vorheriges Teilziel", "上一个子目标"],
    "Quantum Jump (hold)": ["Salto Quantum (mantener)", "Saut Quantum (maintenir)", "Quantensprung (halten)", "量子跳跃(按住)"],
    "Quantum Mode": ["Modo Quantum", "Mode Quantum", "Quantum-Modus", "量子模式"],
    "Remote Turret 1": ["Torreta remota 1", "Tourelle distante 1", "Fernturm 1", "遥控炮塔 1"],
    "Remote Turret 2": ["Torreta remota 2", "Tourelle distante 2", "Fernturm 2", "遥控炮塔 2"],
    "Remote Turret 3": ["Torreta remota 3", "Tourelle distante 3", "Fernturm 3", "遥控炮塔 3"],
    "Request Docking": ["Solicitar atraque", "Demander l'amarrage", "Andocken anfragen", "请求对接"],
    "Request Landing": ["Solicitar aterrizaje", "Demander l'atterrissage", "Landung anfragen", "请求降落"],
    "Reset": ["Reiniciar", "Réinitialiser", "Zurücksetzen", "重置"],
    "Reset All": ["Reiniciar todo", "Tout réinitialiser", "Alles zurücksetzen", "全部重置"],
    "Reset Limit": ["Reiniciar límite", "Réinit. limite", "Limit zurücksetzen", "重置限制"],
    "Reset Speed Limit": ["Reiniciar límite de velocidad", "Réinit. limite de vitesse", "Tempolimit zurücksetzen", "重置速度限制"],
    "Right": ["Derecha", "Droite", "Rechts", "右"],
    "Rude": ["Grosero", "Grossier", "Unhöflich", "粗鲁"],
    "Salute": ["Saludo militar", "Salut militaire", "Salutieren", "敬礼"],
    "Salvage": ["Recuperación", "Récupération", "Bergung", "打捞"],
    "Salvage Mode": ["Modo recuperación", "Mode récupération", "Bergungsmodus", "打捞模式"],
    "Scan Mode": ["Modo escaneo", "Mode scan", "Scanmodus", "扫描模式"],
    "Scanning": ["Escaneo", "Scan", "Scannen", "扫描中"],
    "Self-D": ["Autodest.", "Autodest.", "Selbstzerst.", "自毁"],
    "Shields +": ["Escudos +", "Boucliers +", "Schilde +", "护盾 +"],
    "Shields −": ["Escudos −", "Boucliers −", "Schilde −", "护盾 −"],
    "Shields Back": ["Escudos atrás", "Boucliers arrière", "Schilde hinten", "护盾:后"],
    "Shields Bottom": ["Escudos abajo", "Boucliers bas", "Schilde unten", "护盾:下"],
    "Shields Front": ["Escudos frente", "Boucliers avant", "Schilde vorne", "护盾:前"],
    "Shields Left": ["Escudos izquierda", "Boucliers gauche", "Schilde links", "护盾:左"],
    "Shields Reset": ["Escudos: reiniciar", "Boucliers : réinit.", "Schilde zurücksetzen", "护盾:重置"],
    "Shields Right": ["Escudos derecha", "Boucliers droite", "Schilde rechts", "护盾:右"],
    "Shields Top": ["Escudos arriba", "Boucliers haut", "Schilde oben", "护盾:上"],
    "Sit": ["Sentarse", "S'asseoir", "Hinsetzen", "坐下"],
    "Sleep": ["Dormir", "Dormir", "Schlafen", "睡觉"],
    "Slide to eject": ["Desliza para eyectar", "Glisser pour s'éjecter", "Schieben zum Ausstoßen", "滑动以弹射"],
    "Smell": ["Oler", "Renifler", "Riechen", "闻"],
    "Speed Limiter": ["Limitador de velocidad", "Limiteur de vitesse", "Tempolimit", "限速器"],
    "Speed Range +": ["Rango de velocidad +", "Plage de vitesse +", "Tempobereich +", "速度区间 +"],
    "Speed Range −": ["Rango de velocidad −", "Plage de vitesse −", "Tempobereich −", "速度区间 −"],
    "Speed Range Down": ["Bajar rango de velocidad", "Baisser la plage de vitesse", "Tempobereich runter", "速度区间调低"],
    "Speed Range Up": ["Subir rango de velocidad", "Augmenter la plage de vitesse", "Tempobereich rauf", "速度区间调高"],
    "Star Map": ["Mapa estelar", "Carte stellaire", "Sternenkarte", "星图"],
    "Switch mode": ["Cambiar modo", "Changer de mode", "Modus wechseln", "切换模式"],
    "Taunt": ["Provocar", "Provoquer", "Verhöhnen", "挑衅"],
    "Threaten": ["Amenazar", "Menacer", "Drohen", "威胁"],
    "Thrusters +": ["Propulsores +", "Propulseurs +", "Triebwerke +", "推进器 +"],
    "Thrusters −": ["Propulsores −", "Propulseurs −", "Triebwerke −", "推进器 −"],
    "Toggle All Doors": ["Alternar todas las puertas", "Basculer toutes les portes", "Alle Türen umschalten", "切换所有舱门"],
    "Toggle Doors": ["Alternar puertas", "Basculer les portes", "Türen umschalten", "切换舱门"],
    "Toggle ESP": ["Alternar ESP", "Basculer ESP", "ESP umschalten", "切换 ESP"],
    "Toggle Flight Ready": ["Listo para volar", "Prêt au vol", "Flugbereit", "切换飞行就绪"],
    "Top": ["Arriba", "Haut", "Oben", "上"],
    "Turret 1": ["Torreta 1", "Tourelle 1", "Turm 1", "炮塔 1"],
    "Turret 2": ["Torreta 2", "Tourelle 2", "Turm 2", "炮塔 2"],
    "Turret 3": ["Torreta 3", "Tourelle 3", "Turm 3", "炮塔 3"],
    "Unlock All Doors": ["Desbloquear todas las puertas", "Déverrouiller toutes les portes", "Alle Türen entriegeln", "解锁所有舱门"],
    "Unlock Doors": ["Desbloquear puertas", "Déverrouiller les portes", "Türen entriegeln", "解锁舱门"],
    "Wait": ["Espera", "Attends", "Warte", "等等"],
    "Walk": ["Caminar", "Marcher", "Gehen", "行走"],
    "Wave": ["Agitar la mano", "Faire signe", "Winken", "挥手"],
    "Weapons +": ["Armas +", "Armes +", "Waffen +", "武器 +"],
    "Weapons −": ["Armas −", "Armes −", "Waffen −", "武器 −"],
    "Whistle": ["Silbar", "Siffler", "Pfeifen", "吹口哨"],
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
