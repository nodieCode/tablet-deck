// Tablet Deck's app window. It starts TabletDeckServer.exe (server.js packed by
// scripts/build.js) hidden, with --ui, and shows its address, pairing code, QR code,
// status and log instead of a console window. Minimizing hides it to a tray icon; closing
// it stops the server, which releases any held keys first.
//
//   TabletDeck.exe [--minimized] [server options such as --dry-run or --port 8791]
//
// The server reports status as "@ui {json}" lines on stdout (see ui() in server.js); every
// other line goes to the log. "quit" on its stdin stops it. Only the installed app uses
// this window; running from source keeps the console.
//
// Updates: unless turned off, the window asks GitHub for the latest release when it starts
// and once a day. A newer version shows a banner; "Update now" downloads the installer,
// checks it against the release's SHA-256 file and runs it quietly. The installer asks this
// window to quit (TabletDeck.Quit), installs, and starts Tablet Deck again.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Win32;

static class Program
{
    [DllImport("user32.dll")]
    static extern bool SetProcessDPIAware();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern int RegisterWindowMessage(string name);

    [DllImport("user32.dll")]
    static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    // Sent to every window when Tablet Deck is opened a second time; the running copy shows itself.
    public static readonly int ShowWindowMessage = RegisterWindowMessage("TabletDeck.ShowWindow");
    // Sent by the installer (installer/TabletDeck.iss) before it updates or removes the files.
    public static readonly int QuitMessage = RegisterWindowMessage("TabletDeck.Quit");

    [STAThread]
    static void Main(string[] args)
    {
        bool first;
        using (var mutex = new Mutex(true, "Local\\TabletDeck.Window", out first))
        {
            if (!first)
            {
                PostMessage((IntPtr)0xffff, ShowWindowMessage, IntPtr.Zero, IntPtr.Zero); // HWND_BROADCAST
                return;
            }
            SetProcessDPIAware();
            ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072; // TLS 1.2, which GitHub requires
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => Fail(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => Fail(e.ExceptionObject as Exception);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try { Application.Run(new DeckWindow(args)); }
            catch (Exception ex) { Fail(ex); }
            GC.KeepAlive(mutex);
        }
    }

    // Where Fail saves what went wrong, next to config.json.
    public static readonly string CrashLog = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TabletDeck", "crash.log");
    static bool failing;

    // Any error nothing else caught: saved to crash.log, shown, and Tablet Deck closes. The
    // server notices its window is gone and stops, releasing any held keys.
    static void Fail(Exception ex)
    {
        if (failing) return;
        failing = true;
        string text = ex == null ? "unknown error" : ex.ToString();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(CrashLog));
            File.AppendAllText(CrashLog, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + "  Tablet Deck " + Application.ProductVersion
                + " on " + Environment.OSVersion + "\r\n" + text + "\r\n\r\n");
        }
        catch (Exception) { }
        try
        {
            MessageBox.Show("Tablet Deck ran into a problem and has to close:\n\n" + (ex == null ? text : ex.Message) +
                "\n\nThe details are saved in " + CrashLog + ". Please attach that file to an issue on GitHub " +
                "(github.com/nodieCode/tablet-deck/issues).", "Tablet Deck", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception) { }
        Environment.Exit(1);
    }
}

// The window's languages. Text is written in English and translated where it's shown;
// what the server sends (log lines, voice errors) stays English. The choice is kept in the
// registry; "Automatic" follows Windows' display language. The translations were made with
// AI; corrections from native speakers are welcome.
static class Lang
{
    const string Key = @"Software\Tablet Deck";
    // [code, name] in the order the Settings window lists them; "" is Automatic.
    public static readonly string[][] Choices = {
        new[] { "", "Automatic" }, new[] { "en", "English" }, new[] { "es", "Español" }, new[] { "fr", "Français" }, new[] { "de", "Deutsch" }, new[] { "ca", "Català" },
    };
    static readonly string[] Columns = { "es", "fr", "de", "ca" };
    static int column = -1; // in the tables' rows; -1 is English

    // The saved choice: "", "en", "es", "fr", "de" or "ca".
    public static string Saved
    {
        get
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(Key))
                    return key == null ? "" : (key.GetValue("Language") as string ?? "");
            }
            catch (Exception) { return ""; }
        }
    }

    static Lang() { Use(Saved); }

    public static void Use(string code)
    {
        if (code == "") code = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        column = Array.IndexOf(Columns, code);
    }

    public static void Save(string code)
    {
        try { using (var key = Registry.CurrentUser.CreateSubKey(Key)) key.SetValue("Language", code); }
        catch (Exception) { }
        Use(code);
    }

    public static string T(string text)
    {
        string[] row;
        return column >= 0 && text != null && table.TryGetValue(text, out row) ? row[column] : text;
    }

    // T with "{0}", "{1}" filled in.
    public static string F(string text, params object[] args)
    {
        return string.Format(T(text), args);
    }

    // English -> [Español, Français, Deutsch, Català].
    static readonly Dictionary<string, string[]> table = new Dictionary<string, string[]>
    {
        // ---- the main window ----
        { "STAR CITIZEN CONTROL DECK", new[] { "PANEL DE CONTROL PARA STAR CITIZEN", "DECK DE COMMANDE POUR STAR CITIZEN", "STEUERDECK FÜR STAR CITIZEN", "DECK DE CONTROL DE STAR CITIZEN" } },
        { "STARTING", new[] { "INICIANDO", "DÉMARRAGE", "STARTET", "INICIANT" } },
        { "ONLINE", new[] { "EN LÍNEA", "EN LIGNE", "ONLINE", "EN LÍNIA" } },
        { "TEST MODE", new[] { "MODO PRUEBA", "MODE TEST", "TESTMODUS", "MODE DE PROVA" } },
        { "STOPPED", new[] { "DETENIDO", "ARRÊTÉ", "GESTOPPT", "ATURAT" } },
        { "UPTIME", new[] { "ACTIVO", "DURÉE", "LAUFZEIT", "TEMPS ACTIU" } },
        { "CONNECT", new[] { "CONECTAR", "CONNEXION", "VERBINDEN", "CONNECTA" } },
        { "LINK", new[] { "ENLACE", "LIEN", "LINK", "ENLLAÇ" } },
        { "TELEMETRY", new[] { "TELEMETRÍA", "TÉLÉMÉTRIE", "TELEMETRIE", "TELEMETRIA" } },
        { "VOICE", new[] { "VOZ", "VOIX", "STIMME", "VEU" } },
        { "SYSTEM", new[] { "SISTEMA", "SYSTÈME", "SYSTEM", "SISTEMA" } },
        { "LOG", new[] { "REGISTRO", "JOURNAL", "PROTOKOLL", "REGISTRE" } },
        { "NO NETWORK FOUND", new[] { "NO SE ENCONTRÓ RED", "AUCUN RÉSEAU TROUVÉ", "KEIN NETZWERK GEFUNDEN", "NO S'HA TROBAT CAP XARXA" } },
        { "OFFLINE", new[] { "SIN CONEXIÓN", "HORS LIGNE", "OFFLINE", "SENSE CONNEXIÓ" } },
        { "WAITING FOR THE SERVER", new[] { "ESPERANDO AL SERVIDOR", "EN ATTENTE DU SERVEUR", "WARTE AUF DEN SERVER", "ESPERANT EL SERVIDOR" } },
        { "SCAN WITH YOUR TABLET'S CAMERA", new[] { "ESCANEA CON TU TABLET", "SCANNEZ AVEC VOTRE TABLETTE", "MIT DER TABLET-KAMERA SCANNEN", "ESCANEJA AMB LA CÀMERA DE LA TAULETA" } },
        { "ADDRESS", new[] { "DIRECCIÓN", "ADRESSE", "ADRESSE", "ADREÇA" } },
        { "No network found. Connect this PC to Wi-Fi or a network.", new[] { "No se encontró ninguna red. Conecta este PC a una wifi o a una red.", "Aucun réseau trouvé. Connectez ce PC au Wi-Fi ou à un réseau.", "Kein Netzwerk gefunden. Verbinde diesen PC mit WLAN oder einem Netzwerk.", "No s'ha trobat cap xarxa. Connecta aquest PC a la Wi-Fi o a una xarxa." } },
        { "Windows treats \"{0}\" as Public: tablets can't connect.", new[] { "Windows trata \"{0}\" como pública: las tablets no pueden conectarse.", "Windows considère \"{0}\" comme public : les tablettes ne peuvent pas se connecter.", "Windows behandelt \"{0}\" als öffentlich: Tablets können sich nicht verbinden.", "Windows considera «{0}» una xarxa pública: les tauletes no es poden connectar." } },
        { "ALSO", new[] { "TAMBIÉN", "AUSSI", "AUCH", "TAMBÉ" } },
        { "PAIRING CODE", new[] { "CÓDIGO DE EMPAREJAMIENTO", "CODE D'APPAIRAGE", "KOPPLUNGSCODE", "CODI D'APARELLAMENT" } },
        { "TABLETS", new[] { "TABLETS", "TABLETTES", "TABLETS", "TAULETES" } },
        { "CONNECTED", new[] { "CONECTADAS", "CONNECTÉES", "VERBUNDEN", "CONNECTADES" } },
        { "KEYS GO TO", new[] { "TECLAS A", "TOUCHES VERS", "TASTEN AN", "LES TECLES VAN A" } },
        { "LOG ONLY", new[] { "SOLO REGISTRO", "JOURNAL SEUL", "NUR PROTOKOLL", "NOMÉS REGISTRE" } },
        { "ANY WINDOW", new[] { "CUALQUIER VENTANA", "TOUTE FENÊTRE", "JEDES FENSTER", "QUALSEVOL FINESTRA" } },
        { "TEST MODE: NO KEYS SENT", new[] { "MODO PRUEBA: SIN TECLAS", "MODE TEST : AUCUNE TOUCHE", "TESTMODUS: KEINE TASTEN", "MODE DE PROVA: NO S'ENVIEN TECLES" } },
        { "ONLY WHILE IT'S IN FRONT", new[] { "SOLO EN PRIMER PLANO", "SI AU PREMIER PLAN", "NUR IM VORDERGRUND", "NOMÉS QUAN ÉS EN PRIMER PLA" } },
        { "FOCUS CHECK IS OFF", new[] { "SIN CONTROL DE FOCO", "CONTRÔLE DU FOCUS COUPÉ", "FOKUSPRÜFUNG AUS", "COMPROVACIÓ DE FOCUS DESACTIVADA" } },
        { "PRESSES", new[] { "PULSACIONES", "APPUIS", "TASTENDRÜCKE", "PULSACIONS" } },
        { "LAST: {0}", new[] { "ÚLTIMA: {0}", "DERNIER : {0}", "ZULETZT: {0}", "DARRERA: {0}" } },
        { "NONE YET", new[] { "NINGUNA AÚN", "AUCUN POUR L'INSTANT", "NOCH KEINE", "ENCARA CAP" } },
        { "{0} BLOCKED", new[] { "{0} BLOQUEADAS", "{0} BLOQUÉS", "{0} BLOCKIERT", "{0} BLOQUEJADES" } },
        { "STRICTNESS", new[] { "EXIGENCIA", "EXIGENCE", "STRENGE", "PRECISIÓ" } },
        { "Lower accepts more but may mishear. Higher is stricter.", new[] { "Bajo: acepta más, pero se equivoca más. Alto: más estricto.", "Bas : accepte plus, se trompe plus. Haut : plus strict.", "Niedrig: nimmt mehr an, irrt sich eher. Hoch: strenger.", "Un valor baix accepta més ordres, però pot confondre-les. Un valor alt és més estricte." } },
        { "Starting...", new[] { "Iniciando...", "Démarrage...", "Startet...", "Iniciant…" } },
        { "Starting the server...", new[] { "Iniciando el servidor...", "Démarrage du serveur...", "Server wird gestartet...", "Iniciant el servidor…" } },
        { "Running", new[] { "En marcha", "En marche", "Läuft", "En execució" } },
        { "Running in test mode (dry run): no keys are sent", new[] { "En modo de prueba (dry run): no se envían teclas", "En mode test (dry run) : aucune touche n'est envoyée", "Läuft im Testmodus (Dry Run): Es werden keine Tasten gesendet", "En mode de prova: no s'envien tecles" } },
        { "Stopped: {0}", new[] { "Detenido: {0}", "Arrêté : {0}", "Gestoppt: {0}", "Aturat: {0}" } },
        { "{0} is missing. Reinstall Tablet Deck.", new[] { "Falta {0}. Reinstala Tablet Deck.", "{0} est introuvable. Réinstallez Tablet Deck.", "{0} fehlt. Installiere Tablet Deck neu.", "Falta {0}. Torna a instal·lar Tablet Deck." } },
        { "Couldn't start the server: {0}", new[] { "No se pudo iniciar el servidor: {0}", "Impossible de démarrer le serveur : {0}", "Der Server konnte nicht starten: {0}", "No s'ha pogut iniciar el servidor: {0}" } },
        { "The server stopped (exit code {0}). See the log below.", new[] { "El servidor se detuvo (código {0}). Mira el registro de abajo.", "Le serveur s'est arrêté (code {0}). Voir le journal ci-dessous.", "Der Server wurde beendet (Code {0}). Siehe das Protokoll unten.", "El servidor s'ha aturat (codi de sortida {0}). Consulta el registre de sota." } },
        { "Off", new[] { "Desactivado", "Désactivé", "Aus", "Desactivat" } },
        { "Ready: {0} phrases, {1}", new[] { "Listo: {0} frases, {1}", "Prêt : {0} phrases, {1}", "Bereit: {0} Sätze, {1}", "Preparat: {0} frases, {1}" } },
        { ", push-to-talk: {0}", new[] { ", pulsar para hablar: {0}", ", appuyer pour parler : {0}", ", Push-to-Talk: {0}", ", prémer per parlar: {0}" } },
        { "Turned off in settings", new[] { "Desactivado en los ajustes", "Désactivé dans les réglages", "In den Einstellungen ausgeschaltet", "Desactivat a la configuració" } },
        { "Restarting...", new[] { "Reiniciando...", "Redémarrage...", "Startet neu...", "Reiniciant…" } },
        { "Unavailable: {0}", new[] { "No disponible: {0}", "Indisponible : {0}", "Nicht verfügbar: {0}", "No disponible: {0}" } },
        { "Last heard: \"{0}\", {1}% sure. Pressed {2}.", new[] { "Último oído: \"{0}\", {1} % seguro. Pulsado: {2}.", "Dernière écoute : \"{0}\", sûr à {1} %. Appuyé : {2}.", "Zuletzt gehört: \"{0}\", {1} % sicher. Gedrückt: {2}.", "S'ha sentit: «{0}», certesa del {1}%. S'ha premut {2}." } },
        { "Last heard: \"{0}\", {1}% sure. Below {2}%, so it was ignored.", new[] { "Último oído: \"{0}\", {1} % seguro. Por debajo del {2} %, así que se ignoró.", "Dernière écoute : \"{0}\", sûr à {1} %. Sous {2} %, donc ignoré.", "Zuletzt gehört: \"{0}\", {1} % sicher. Unter {2} %, daher ignoriert.", "S'ha sentit: «{0}», certesa del {1}%. Per sota del {2}%, així que s'ha ignorat." } },
        { "Last heard: something that isn't one of the commands.", new[] { "Último oído: algo que no es uno de los comandos.", "Dernière écoute : quelque chose qui n'est pas une commande.", "Zuletzt gehört: etwas, das kein Befehl ist.", "S'ha sentit una frase que no correspon a cap ordre." } },
        { "Tablet Deck (stopped)", new[] { "Tablet Deck (detenido)", "Tablet Deck (arrêté)", "Tablet Deck (gestoppt)", "Tablet Deck (aturat)" } },
        { " (test mode)", new[] { " (modo prueba)", " (mode test)", " (Testmodus)", " (mode de prova)" } },
        { "Windows treats \"{0}\" as a Public network, so tablets can't connect. In Windows: Settings > Network & Internet > {0} > set it to Private.", new[] { "Windows trata \"{0}\" como red pública, así que las tablets no pueden conectarse. En Windows: Configuración > Red e Internet > {0} > ponla como privada.", "Windows considère \"{0}\" comme un réseau public, donc les tablettes ne peuvent pas se connecter. Dans Windows : Paramètres > Réseau et Internet > {0} > réglez-le sur Privé.", "Windows behandelt \"{0}\" als öffentliches Netzwerk, daher können Tablets sich nicht verbinden. In Windows: Einstellungen > Netzwerk und Internet > {0} > auf Privat stellen.", "Windows considera «{0}» una xarxa pública, de manera que les tauletes no es poden connectar. A Windows: Configuració > Xarxa i Internet > {0} > estableix-la com a privada." } },
        { "Tablets can't connect", new[] { "Las tablets no pueden conectarse", "Les tablettes ne peuvent pas se connecter", "Tablets können sich nicht verbinden", "Les tauletes no es poden connectar" } },
        { "Windows treats \"{0}\" as a Public network. Open Tablet Deck to fix it.", new[] { "Windows trata \"{0}\" como red pública. Abre Tablet Deck para arreglarlo.", "Windows considère \"{0}\" comme un réseau public. Ouvrez Tablet Deck pour corriger cela.", "Windows behandelt \"{0}\" als öffentliches Netzwerk. Öffne Tablet Deck, um das zu beheben.", "Windows considera «{0}» una xarxa pública. Obre Tablet Deck per solucionar-ho." } },
        { "Restarting the server for the new settings...", new[] { "Reiniciando el servidor con los nuevos ajustes...", "Redémarrage du serveur avec les nouveaux réglages...", "Server startet mit den neuen Einstellungen neu...", "Reiniciant el servidor amb la configuració nova…" } },
        { "Couldn't save the settings: {0}", new[] { "No se pudieron guardar los ajustes: {0}", "Impossible d'enregistrer les réglages : {0}", "Die Einstellungen konnten nicht gespeichert werden: {0}", "No s'ha pogut desar la configuració: {0}" } },
        { "Tablet Deck isn't running", new[] { "Tablet Deck no está en marcha", "Tablet Deck n'est pas lancé", "Tablet Deck läuft nicht", "Tablet Deck no s'està executant" } },
        { "Start it first; the layout editor is part of the server.", new[] { "Inícialo primero; el editor de diseños forma parte del servidor.", "Lancez-le d'abord ; l'éditeur de disposition fait partie du serveur.", "Starte es zuerst; der Layout-Editor gehört zum Server.", "Inicia'l primer: l'editor de disposicions forma part del servidor." } },
        { "Tablet Deck is still running", new[] { "Tablet Deck sigue en marcha", "Tablet Deck tourne toujours", "Tablet Deck läuft weiter", "Tablet Deck encara s'està executant" } },
        { "Double-click the tray icon to open it, or right-click it to quit.", new[] { "Haz doble clic en el icono de la bandeja para abrirlo, o clic derecho para salir.", "Double-cliquez sur l'icône de la zone de notification pour l'ouvrir, ou faites un clic droit pour quitter.", "Doppelklicke auf das Symbol im Infobereich, um es zu öffnen, oder klicke rechts, um es zu beenden.", "Fes doble clic a la icona de l'àrea de notificació per obrir-lo, o clic dret per sortir-ne." } },
        // ---- the main window's buttons, switches and links ----
        { "Copy link", new[] { "Copiar enlace", "Copier le lien", "Link kopieren", "Copia l'enllaç" } },
        { "Copied!", new[] { "¡Copiado!", "Copié !", "Kopiert!", "Copiat!" } },
        { "Settings", new[] { "Ajustes", "Réglages", "Einstellungen", "Configuració" } },
        { "Edit layout", new[] { "Editar diseño", "Éditer le deck", "Layout bearbeiten", "Edita la disposició" } },
        { "Restart", new[] { "Reiniciar", "Redémarrer", "Neu starten", "Reinicia" } },
        { "Commands", new[] { "Comandos", "Commandes", "Befehle", "Ordres" } },
        { "Start with Windows (in the tray)", new[] { "Iniciar con Windows (en la bandeja)", "Lancer avec Windows (réduit)", "Mit Windows starten (im Infobereich)", "Inicia amb Windows (a l'àrea de notificació)" } },
        { "Check for updates automatically", new[] { "Buscar actualizaciones a diario", "Chercher les mises à jour", "Automatisch nach Updates suchen", "Comprova les actualitzacions automàticament" } },
        { "Settings folder", new[] { "Carpeta de ajustes", "Dossier des réglages", "Einstellungsordner", "Carpeta de configuració" } },
        { "Check for updates now", new[] { "Buscar actualizaciones ahora", "Rechercher les mises à jour", "Jetzt nach Updates suchen", "Comprova si hi ha actualitzacions ara" } },
        { "Feedback & ideas", new[] { "Comentarios e ideas", "Avis et idées", "Feedback & Ideen", "Comentaris i idees" } },
        { "Set it to Private ›", new[] { "Ponla como privada ›", "La passer en privé ›", "Auf Privat stellen ›", "Estableix-la com a privada ›" } },
        { "What's new", new[] { "Novedades", "Nouveautés", "Neuigkeiten", "Novetats" } },
        { "Update now", new[] { "Actualizar ahora", "Mettre à jour", "Jetzt aktualisieren", "Actualitza ara" } },
        { "Later", new[] { "Más tarde", "Plus tard", "Später", "Més tard" } },
        { "Close", new[] { "Cerrar", "Fermer", "Schließen", "Tanca" } },
        { "Show Tablet Deck", new[] { "Mostrar Tablet Deck", "Afficher Tablet Deck", "Tablet Deck anzeigen", "Mostra Tablet Deck" } },
        { "Quit", new[] { "Salir", "Quitter", "Beenden", "Surt" } },
        { "Opens the deck in this PC's browser", new[] { "Abre el deck en el navegador de este PC", "Ouvre le deck dans le navigateur de ce PC", "Öffnet das Deck im Browser dieses PCs", "Obre el deck al navegador d'aquest PC" } },
        { "Copies the address with the pairing code, to send to a tablet", new[] { "Copia la dirección con el código de emparejamiento, para enviarla a una tablet", "Copie l'adresse avec le code d'appairage, à envoyer à une tablette", "Kopiert die Adresse mit dem Kopplungscode, um sie an ein Tablet zu schicken", "Copia l'adreça amb el codi d'aparellament per enviar-la a una tauleta" } },
        { "Pairing code, port, focus check, tap length, voice language and the Star Citizen keybind profile", new[] { "Código de emparejamiento, puerto, control de foco, duración del toque, idioma de voz y el perfil de teclas de Star Citizen", "Code d'appairage, port, contrôle du focus, durée d'appui, langue vocale et profil de touches Star Citizen", "Kopplungscode, Port, Fokusprüfung, Tippdauer, Sprachbefehl-Sprache und das Star-Citizen-Tastenprofil", "Codi d'aparellament, port, comprovació de focus, durada de pulsació, idioma de veu i perfil de tecles de Star Citizen" } },
        { "Opens the layout editor in your browser: move buttons, change their keys, labels and icons", new[] { "Abre el editor de diseños en tu navegador: mueve botones y cambia sus teclas, nombres e iconos", "Ouvre l'éditeur de disposition dans votre navigateur : déplacez les boutons, changez leurs touches, libellés et icônes", "Öffnet den Layout-Editor im Browser: Tasten verschieben, ihre Tasten, Beschriftungen und Symbole ändern", "Obre l'editor de disposicions al navegador: mou els botons i canvia'n les tecles, etiquetes i icones" } },
        { "Everything you can say, and the button each one presses", new[] { "Todo lo que puedes decir y el botón que pulsa cada frase", "Tout ce que vous pouvez dire, et le bouton que chaque phrase active", "Alles, was du sagen kannst, und welche Taste jeder Befehl drückt", "Tot el que pots dir i el botó que prem cada ordre" } },
        { "Looks for a new version on GitHub when Tablet Deck starts and once a day. Updates only install when you click Update now.", new[] { "Busca una versión nueva en GitHub al iniciar Tablet Deck y una vez al día. Solo se instala cuando pulsas Actualizar ahora.", "Cherche une nouvelle version sur GitHub au démarrage de Tablet Deck et une fois par jour. Rien ne s'installe sans un clic sur Mettre à jour.", "Sucht beim Start von Tablet Deck und einmal am Tag auf GitHub nach einer neuen Version. Installiert wird nur, wenn du auf Jetzt aktualisieren klickst.", "Cerca una versió nova a GitHub en iniciar Tablet Deck i un cop al dia. Les actualitzacions només s'instal·len quan prems Actualitza ara." } },
        { "Opens the folder with config.json and your own layouts", new[] { "Abre la carpeta con config.json y tus propios diseños", "Ouvre le dossier contenant config.json et vos propres dispositions", "Öffnet den Ordner mit config.json und deinen eigenen Layouts", "Obre la carpeta amb config.json i les teves disposicions" } },
        { "Report a problem or suggest an idea on GitHub", new[] { "Informa de un problema o propón una idea en GitHub", "Signalez un problème ou proposez une idée sur GitHub", "Melde ein Problem oder schlag eine Idee auf GitHub vor", "Informa d'un problema o proposa una idea a GitHub" } },
        { "Pick your network there and set its network profile to Private", new[] { "Elige ahí tu red y pon su perfil de red como privado", "Choisissez votre réseau et réglez son profil réseau sur Privé", "Wähle dort dein Netzwerk und stelle sein Netzwerkprofil auf Privat", "Tria la teva xarxa i estableix-ne el perfil com a privat" } },
        // ---- updates ----
        { "Couldn't check for updates", new[] { "No se pudo buscar actualizaciones", "Impossible de rechercher les mises à jour", "Konnte nicht nach Updates suchen", "No s'han pogut comprovar les actualitzacions" } },
        { "GitHub didn't answer.", new[] { "GitHub no respondió.", "GitHub n'a pas répondu.", "GitHub hat nicht geantwortet.", "GitHub no ha respost." } },
        { "Tablet Deck is up to date", new[] { "Tablet Deck está actualizado", "Tablet Deck est à jour", "Tablet Deck ist aktuell", "Tablet Deck està actualitzat" } },
        { "You have version {0}, the latest.", new[] { "Tienes la versión {0}, la más reciente.", "Vous avez la version {0}, la plus récente.", "Du hast Version {0}, die neueste.", "Tens la versió {0}, la més recent." } },
        { "Update not ready yet", new[] { "La actualización aún no está lista", "Mise à jour pas encore prête", "Update noch nicht bereit", "L'actualització encara no està preparada" } },
        { "Version {0} is out, but its installer isn't attached yet. Try again in a few minutes.", new[] { "La versión {0} ya salió, pero su instalador aún no está adjunto. Inténtalo en unos minutos.", "La version {0} est sortie, mais son installateur n'est pas encore joint. Réessayez dans quelques minutes.", "Version {0} ist erschienen, aber der Installer ist noch nicht angehängt. Versuch es in ein paar Minuten erneut.", "La versió {0} ja ha sortit, però encara no hi ha l'instal·lador adjunt. Torna-ho a provar d'aquí a uns minuts." } },
        { "Tablet Deck {0} is available (you have {1}).", new[] { "Tablet Deck {0} está disponible (tienes la {1}).", "Tablet Deck {0} est disponible (vous avez la {1}).", "Tablet Deck {0} ist verfügbar (du hast {1}).", "Tablet Deck {0} està disponible (tens la versió {1})." } },
        { "Tablet Deck {0} is available", new[] { "Tablet Deck {0} está disponible", "Tablet Deck {0} est disponible", "Tablet Deck {0} ist verfügbar", "Tablet Deck {0} està disponible" } },
        { "Open Tablet Deck and click Update now.", new[] { "Abre Tablet Deck y pulsa Actualizar ahora.", "Ouvrez Tablet Deck et cliquez sur Mettre à jour.", "Öffne Tablet Deck und klicke auf Jetzt aktualisieren.", "Obre Tablet Deck i prem Actualitza ara." } },
        { "Downloading Tablet Deck {0}...", new[] { "Descargando Tablet Deck {0}...", "Téléchargement de Tablet Deck {0}...", "Tablet Deck {0} wird heruntergeladen...", "Baixant Tablet Deck {0}…" } },
        { "Checking the download...", new[] { "Comprobando la descarga...", "Vérification du téléchargement...", "Download wird geprüft...", "Comprovant la baixada…" } },
        { "Installing Tablet Deck {0}. Allow the Windows prompt to continue.", new[] { "Instalando Tablet Deck {0}. Acepta el aviso de Windows para continuar.", "Installation de Tablet Deck {0}. Autorisez la demande de Windows pour continuer.", "Tablet Deck {0} wird installiert. Bestätige die Windows-Abfrage, um fortzufahren.", "Instal·lant Tablet Deck {0}. Accepta l'avís de Windows per continuar." } },
        { "the download failed ({0})", new[] { "la descarga falló ({0})", "le téléchargement a échoué ({0})", "der Download ist fehlgeschlagen ({0})", "la baixada ha fallat ({0})" } },
        { "the download didn't match its checksum, so it wasn't installed", new[] { "la descarga no coincidía con su suma de comprobación, así que no se instaló", "le téléchargement ne correspondait pas à sa somme de contrôle, il n'a donc pas été installé", "der Download passte nicht zu seiner Prüfsumme und wurde daher nicht installiert", "la baixada no coincideix amb la suma de verificació i no s'ha instal·lat" } },
        { "couldn't check the download ({0})", new[] { "no se pudo comprobar la descarga ({0})", "impossible de vérifier le téléchargement ({0})", "der Download konnte nicht geprüft werden ({0})", "no s'ha pogut comprovar la baixada ({0})" } },
        { "the Windows permission prompt was cancelled", new[] { "se canceló el aviso de permisos de Windows", "la demande d'autorisation de Windows a été annulée", "die Windows-Berechtigungsabfrage wurde abgebrochen", "s'ha cancel·lat la sol·licitud de permisos de Windows" } },
        { "the installer finished without updating", new[] { "el instalador terminó sin actualizar", "l'installateur s'est terminé sans mettre à jour", "der Installer wurde ohne Update beendet", "l'instal·lador ha acabat sense actualitzar" } },
        { "the installer stopped (code {0}). Was the Windows prompt cancelled?", new[] { "el instalador se detuvo (código {0}). ¿Se canceló el aviso de Windows?", "l'installateur s'est arrêté (code {0}). La demande de Windows a-t-elle été annulée ?", "der Installer wurde beendet (Code {0}). Wurde die Windows-Abfrage abgebrochen?", "l'instal·lador s'ha aturat (codi {0}). Has cancel·lat l'avís de Windows?" } },
        { "Update failed: {0}", new[] { "La actualización falló: {0}", "Échec de la mise à jour : {0}", "Update fehlgeschlagen: {0}", "L'actualització ha fallat: {0}" } },
        { "Updated to Tablet Deck {0}.", new[] { "Actualizado a Tablet Deck {0}.", "Mis à jour vers Tablet Deck {0}.", "Auf Tablet Deck {0} aktualisiert.", "S'ha actualitzat a Tablet Deck {0}." } },
        // ---- Voice commands window ----
        { "Voice commands", new[] { "Comandos de voz", "Commandes vocales", "Sprachbefehle", "Ordres de veu" } },
        { "Voice commands (not running)", new[] { "Comandos de voz (no activos)", "Commandes vocales (inactives)", "Sprachbefehle (nicht aktiv)", "Ordres de veu (no actives)" } },
        { "Voice commands ({0})", new[] { "Comandos de voz ({0})", "Commandes vocales ({0})", "Sprachbefehle ({0})", "Ordres de veu ({0})" } },
        { "Voice commands ({0} of {1})", new[] { "Comandos de voz ({0} de {1})", "Commandes vocales ({0} sur {1})", "Sprachbefehle ({0} von {1})", "Ordres de veu ({0} de {1})" } },
        { "COMMANDS", new[] { "COMANDOS", "COMMANDES", "BEFEHLE", "ORDRES" } },
        { "Hold the mic button on the tablet and say one of these. Most Star Citizen controls are toggles, so \"engines on\" and \"engines off\" press the same key.", new[] { "Mantén el micro en la tablet y di uno (en inglés). Casi todos los controles alternan: \"engines on\" y \"engines off\" pulsan la misma tecla.", "Maintenez le micro sur la tablette et dites-en un (en anglais). La plupart basculent : \"engines on\" et \"engines off\" appuient sur la même touche.", "Halte das Mikro auf dem Tablet und sag einen davon (Englisch). Die meisten Befehle schalten um: \"engines on\" und \"engines off\" drücken dieselbe Taste.", "Mantén premut el botó del micròfon de la tauleta i digues una d'aquestes ordres (en anglès). La majoria de controls de Star Citizen alternen entre dos estats, així que «engines on» i «engines off» premen la mateixa tecla." } },
        { "Search, e.g. shields or quantum", new[] { "Buscar, p. ej. shields o quantum", "Rechercher, par ex. shields ou quantum", "Suchen, z. B. shields oder quantum", "Cerca, p. ex., shields o quantum" } },
        { "Say", new[] { "Di", "Dites", "Sag", "Digues" } },
        { "Presses", new[] { "Pulsa", "Appuie sur", "Drückt", "Prem" } },
        // ---- Install keybind profile window ----
        { "Install keybind profile", new[] { "Instalar perfil de teclas", "Installer le profil de touches", "Tastenprofil installieren", "Instal·la el perfil de tecles" } },
        { "STAR CITIZEN COPIES", new[] { "COPIAS DE STAR CITIZEN", "COPIES DE STAR CITIZEN", "STAR-CITIZEN-KOPIEN", "CÒPIES DE STAR CITIZEN" } },
        { "Copies the Tablet Deck Keyboard profile into Star Citizen, so buttons for actions with no default key work. Tick the copies of the game to install it into.", new[] { "Copia el perfil Tablet Deck Keyboard en Star Citizen, para que funcionen los botones de acciones sin tecla por defecto. Marca las copias del juego donde instalarlo.", "Copie le profil Tablet Deck Keyboard dans Star Citizen, pour que les boutons des actions sans touche par défaut fonctionnent. Cochez les copies du jeu où l'installer.", "Kopiert das Profil Tablet Deck Keyboard in Star Citizen, damit Tasten für Aktionen ohne Standardbelegung funktionieren. Hake die Spielkopien an, in die es soll.", "Copia el perfil Tablet Deck Keyboard a Star Citizen perquè funcionin els botons d'accions que no tenen cap tecla predeterminada. Marca les instal·lacions del joc on el vols copiar." } },
        { "Choose game folder...", new[] { "Elegir carpeta del juego...", "Choisir le dossier du jeu...", "Spielordner wählen...", "Tria la carpeta del joc…" } },
        { "STATUS", new[] { "ESTADO", "ÉTAT", "STATUS", "ESTAT" } },
        { "Install", new[] { "Instalar", "Installer", "Installieren", "Instal·la" } },
        { "The profile file is missing ({0}). Reinstall Tablet Deck.", new[] { "Falta el archivo del perfil ({0}). Reinstala Tablet Deck.", "Le fichier du profil est introuvable ({0}). Réinstallez Tablet Deck.", "Die Profildatei fehlt ({0}). Installiere Tablet Deck neu.", "Falta el fitxer del perfil ({0}). Torna a instal·lar Tablet Deck." } },
        { "Star Citizen wasn't found in the usual places. Click \"Choose game folder...\" and pick the StarCitizen folder (the one with LIVE in it).", new[] { "No se encontró Star Citizen en los sitios habituales. Pulsa \"Elegir carpeta del juego...\" y elige la carpeta StarCitizen (la que contiene LIVE).", "Star Citizen est introuvable aux emplacements habituels. Cliquez sur \"Choisir le dossier du jeu...\" et choisissez le dossier StarCitizen (celui qui contient LIVE).", "Star Citizen wurde an den üblichen Orten nicht gefunden. Klicke auf \"Spielordner wählen...\" und wähle den Ordner StarCitizen (der mit LIVE darin).", "No s'ha trobat Star Citizen als llocs habituals. Prem «Tria la carpeta del joc…» i selecciona la carpeta StarCitizen (la que conté LIVE)." } },
        { "Found {0} copy of the game. Missing one? Click \"Choose game folder...\".", new[] { "Se encontró {0} copia del juego. ¿Falta alguna? Pulsa \"Elegir carpeta del juego...\".", "{0} copie du jeu trouvée. Il en manque une ? Cliquez sur \"Choisir le dossier du jeu...\".", "{0} Spielkopie gefunden. Fehlt eine? Klicke auf \"Spielordner wählen...\".", "S'ha trobat {0} instal·lació del joc. En falta alguna? Prem «Tria la carpeta del joc…»." } },
        { "Found {0} copies of the game. Missing one? Click \"Choose game folder...\".", new[] { "Se encontraron {0} copias del juego. ¿Falta alguna? Pulsa \"Elegir carpeta del juego...\".", "{0} copies du jeu trouvées. Il en manque une ? Cliquez sur \"Choisir le dossier du jeu...\".", "{0} Spielkopien gefunden. Fehlt eine? Klicke auf \"Spielordner wählen...\".", "S'han trobat {0} instal·lacions del joc. En falta alguna? Prem «Tria la carpeta del joc…»." } },
        { "not installed", new[] { "no instalado", "non installé", "nicht installiert", "no instal·lat" } },
        { "already up to date", new[] { "ya actualizado", "déjà à jour", "bereits aktuell", "ja està actualitzat" } },
        { "older copy, will be replaced", new[] { "copia antigua, se reemplazará", "ancienne copie, sera remplacée", "ältere Kopie, wird ersetzt", "còpia antiga, se substituirà" } },
        { "Pick the StarCitizen folder (the one with LIVE in it).", new[] { "Elige la carpeta StarCitizen (la que contiene LIVE).", "Choisissez le dossier StarCitizen (celui qui contient LIVE).", "Wähle den Ordner StarCitizen (der mit LIVE darin).", "Tria la carpeta StarCitizen (la que conté LIVE)." } },
        { "That folder doesn't look like Star Citizen. Pick the StarCitizen folder, the one with LIVE in it.", new[] { "Esa carpeta no parece de Star Citizen. Elige la carpeta StarCitizen, la que contiene LIVE.", "Ce dossier ne ressemble pas à Star Citizen. Choisissez le dossier StarCitizen, celui qui contient LIVE.", "Dieser Ordner sieht nicht nach Star Citizen aus. Wähle den Ordner StarCitizen, den mit LIVE darin.", "Aquesta carpeta no sembla de Star Citizen. Tria la carpeta StarCitizen, la que conté LIVE." } },
        { "Added. Tick the copies of the game to install the profile into, then click Install.", new[] { "Añadido. Marca las copias del juego donde instalar el perfil y pulsa Instalar.", "Ajouté. Cochez les copies du jeu où installer le profil, puis cliquez sur Installer.", "Hinzugefügt. Hake die Spielkopien an, in die das Profil soll, und klicke auf Installieren.", "Afegida. Marca les instal·lacions del joc on vols posar el perfil i prem Instal·la." } },
        { "Nothing ticked. Tick the copies of the game to install the profile into.", new[] { "No hay nada marcado. Marca las copias del juego donde instalar el perfil.", "Rien n'est coché. Cochez les copies du jeu où installer le profil.", "Nichts angehakt. Hake die Spielkopien an, in die das Profil soll.", "No n'has marcat cap. Marca les instal·lacions del joc on vols posar el perfil." } },
        { "Installed into {0}. ", new[] { "Instalado en {0}. ", "Installé dans {0}. ", "Installiert in {0}. ", "Instal·lat a {0}." } },
        { "Now, in Star Citizen: Options > Keybindings > Control Profiles, load \"Tablet Deck Keyboard\". ", new[] { "Ahora, en Star Citizen: Opciones > Asignación de teclas > Perfiles de control, carga \"Tablet Deck Keyboard\". ", "Ensuite, dans Star Citizen : Options > Raccourcis > Profils de contrôle, chargez \"Tablet Deck Keyboard\". ", "Jetzt in Star Citizen: Optionen > Tastenbelegung > Steuerungsprofile, \"Tablet Deck Keyboard\" laden. ", "Ara, a Star Citizen: Options > Keybindings > Control Profiles, carrega «Tablet Deck Keyboard»." } },
        { "This replaces your current custom binds, so note any you want to keep and set them again afterwards.", new[] { "Esto sustituye tus asignaciones personalizadas actuales, así que apunta las que quieras conservar y vuelve a ponerlas después.", "Cela remplace vos raccourcis personnalisés actuels : notez ceux que vous voulez garder et refaites-les ensuite.", "Das ersetzt deine aktuellen eigenen Belegungen. Notiere dir, was du behalten willst, und stelle es danach wieder ein.", "Això substituirà les teves assignacions de tecles personalitzades. Anota les que vulguis conservar i torna-les a configurar després." } },
        { "Couldn't copy into {0}. Try running Tablet Deck as administrator, or copy {1} by hand.", new[] { "No se pudo copiar en {0}. Prueba a ejecutar Tablet Deck como administrador o copia {1} a mano.", "Impossible de copier dans {0}. Essayez de lancer Tablet Deck en administrateur, ou copiez {1} à la main.", "Kopieren nach {0} fehlgeschlagen. Starte Tablet Deck als Administrator oder kopiere {1} von Hand.", "No s'ha pogut copiar a {0}. Prova d'executar Tablet Deck com a administrador o copia {1} manualment." } },
        // ---- Settings window ----
        { "Tablet Deck settings", new[] { "Ajustes de Tablet Deck", "Réglages de Tablet Deck", "Tablet-Deck-Einstellungen", "Configuració de Tablet Deck" } },
        { "PAIRING", new[] { "EMPAREJAMIENTO", "APPAIRAGE", "KOPPLUNG", "APARELLAMENT" } },
        { "Pairing code", new[] { "Código de emparejamiento", "Code d'appairage", "Kopplungscode", "Codi d'aparellament" } },
        { "New random code", new[] { "Nuevo código", "Nouveau code", "Neuer Code", "Codi aleatori nou" } },
        { "Letters and numbers. Tablets already paired keep working until they reload; new ones need the new code.", new[] { "Letras y números. Las tablets ya emparejadas funcionan hasta recargar; las nuevas usan el código nuevo.", "Lettres et chiffres. Les tablettes appairées marchent jusqu'à leur rechargement, les nouvelles ont besoin du code.", "Buchstaben und Ziffern. Gekoppelte Tablets laufen bis zum Neuladen weiter; neue brauchen den neuen Code.", "Lletres i números. Les tauletes ja aparellades continuaran funcionant fins que es recarreguin; les noves necessitaran el codi nou." } },
        { "Port", new[] { "Puerto", "Port", "Port", "Port" } },
        { "Set with --port when Tablet Deck was started, so it can't be changed here.", new[] { "Fijado con --port al iniciar Tablet Deck; no se puede cambiar aquí.", "Défini avec --port au lancement ; non modifiable ici.", "Beim Start mit --port gesetzt, daher hier nicht änderbar.", "S'ha fixat amb --port en iniciar Tablet Deck i aquí no es pot modificar." } },
        { "Changing it restarts the server. Tablets then need the new address or QR code.", new[] { "Reinicia el servidor. Las tablets necesitarán la nueva dirección o el QR.", "Redémarre le serveur. Les tablettes devront reprendre l'adresse ou le QR.", "Startet den Server neu. Tablets brauchen dann neue Adresse oder QR-Code.", "Si el canvies, el servidor es reiniciarà. Les tauletes necessitaran l'adreça nova o el codi QR." } },
        { "KEYS", new[] { "TECLAS", "TOUCHES", "TASTEN", "TECLES" } },
        { "Only press keys while Star Citizen is in front", new[] { "Pulsar teclas solo con Star Citizen en primer plano", "Seulement si Star Citizen est devant", "Tasten nur drücken, wenn Star Citizen vorne ist", "Prem les tecles només quan Star Citizen sigui en primer pla" } },
        { "Keys never reach another window, like a browser or chat, by accident.", new[] { "Las teclas nunca llegan por error a otra ventana, como un navegador o un chat.", "Les touches n'atteignent jamais par erreur une autre fenêtre, comme un navigateur ou un chat.", "Tasten landen nie versehentlich in einem anderen Fenster, etwa einem Browser oder Chat.", "Les tecles no arribaran accidentalment a cap altra finestra, com un navegador o un xat." } },
        { "Tap length", new[] { "Duración del toque", "Durée d'appui", "Tippdauer", "Durada de pulsació" } },
        { "milliseconds a tap holds the key. Raise it if the game misses taps.", new[] { "milisegundos que un toque mantiene la tecla. Súbelo si el juego pierde toques.", "millisecondes de maintien par appui. À augmenter si le jeu rate des appuis.", "Millisekunden, die ein Tippen die Taste hält. Erhöhen, wenn das Spiel Tipps verpasst.", "mil·lisegons que es manté premuda una tecla. Augmenta-la si el joc no detecta algunes pulsacions." } },
        { "Voice language", new[] { "Idioma de voz", "Langue vocale", "Sprache für Befehle", "Idioma de veu" } },
        { "Needs that English speech recognizer installed in Windows (Settings > Time & Language > Speech).", new[] { "Necesita ese reconocedor de voz en inglés instalado en Windows (Configuración > Hora e idioma > Voz).", "Nécessite la reconnaissance vocale anglaise de Windows (Paramètres > Heure et langue > Voix).", "Braucht die englische Spracherkennung von Windows (Einstellungen > Zeit und Sprache > Sprache).", "Cal tenir instal·lat el reconeixedor de veu en anglès a Windows (Configuració > Hora i idioma > Veu)." } },
        { "Emotes also answer to one word, like \"wave\"", new[] { "Gestos también con una palabra, como \"wave\"", "Emotes aussi avec un seul mot, comme \"wave\"", "Emotes reagieren auch auf ein Wort, etwa \"wave\"", "Els gestos també responen a una sola paraula, com «wave»" } },
        { "Without this, emotes need \"emote\" first (\"emote wave\"), so a stray word doesn't set one off.", new[] { "Sin esto, los gestos necesitan \"emote\" delante (\"emote wave\"), para que una palabra suelta no active uno.", "Sans cela, les emotes demandent \"emote\" d'abord (\"emote wave\"), pour qu'un mot isolé n'en déclenche pas.", "Ohne das brauchen Emotes zuerst \"emote\" (\"emote wave\"), damit ein einzelnes Wort keins auslöst.", "Si no actives això, els gestos necessiten «emote» al davant («emote wave»), per evitar que una paraula aïllada els activi." } },
        { "PUSH-TO-TALK", new[] { "PULSAR PARA HABLAR", "APPUYER POUR PARLER", "PUSH-TO-TALK", "PRÉMER PER PARLAR" } },
        { "Key on this PC", new[] { "Tecla en este PC", "Touche sur ce PC", "Taste an diesem PC", "Tecla d'aquest PC" } },
        { "None", new[] { "Ninguna", "Aucune", "Keine", "Cap" } },
        { "None: click to set", new[] { "Ninguna: clic para elegir", "Aucune : cliquez pour choisir", "Keine: zum Festlegen klicken", "Cap: prem per configurar" } },
        { "Press a key or button...", new[] { "Pulsa una tecla o botón...", "Appuyez sur une touche ou un bouton...", "Drück eine Taste oder einen Knopf...", "Prem una tecla o un botó…" } },
        { "Mode", new[] { "Modo", "Mode", "Modus", "Mode" } },
        { "Hold to talk", new[] { "Mantener para hablar", "Maintenir pour parler", "Halten zum Sprechen", "Mantén premut per parlar" } },
        { "Press to start, again to stop", new[] { "Pulsar para empezar y otra vez para parar", "Appuyer pour démarrer, encore pour arrêter", "Drücken zum Starten, nochmal zum Beenden", "Prem per començar i torna a prémer per aturar" } },
        { "Play a sound when it starts and stops listening", new[] { "Sonido al empezar y dejar de escuchar", "Un son quand l'écoute démarre et s'arrête", "Ton beim Start und Ende des Zuhörens", "Reprodueix un so en començar i acabar d'escoltar" } },
        { "Pick a key Star Citizen doesn't use, like a mouse side button, a numpad key or F13-F24: the key still reaches the game.", new[] { "Elige una tecla que Star Citizen no use, como un botón lateral del ratón, una tecla del teclado numérico o F13-F24: la tecla sigue llegando al juego.", "Choisissez une touche que Star Citizen n'utilise pas, comme un bouton latéral de souris, une touche du pavé numérique ou F13-F24 : la touche arrive quand même au jeu.", "Wähle eine Taste, die Star Citizen nicht nutzt, etwa eine Maus-Seitentaste, eine Ziffernblocktaste oder F13-F24: Die Taste kommt trotzdem im Spiel an.", "Tria una tecla que Star Citizen no utilitzi, com un botó lateral del ratolí, una tecla del teclat numèric o F13-F24: la tecla també arribarà al joc." } },
        { "LINKS", new[] { "ENLACES", "LIENS", "LINKS", "ENLLAÇOS" } },
        { "Websites the tablet can open on this PC", new[] { "Webs que la tablet puede abrir en este PC", "Sites que la tablette peut ouvrir", "Websites für das Tablet", "Webs que la tauleta pot obrir en aquest PC" } },
        { "One per line: Name | https://address. The tablet only sees the names, so it can't open anything else.", new[] { "Una por línea: Nombre | https://dirección. La tablet solo ve los nombres, así que no puede abrir nada más.", "Un par ligne : Nom | https://adresse. La tablette ne voit que les noms, elle ne peut donc rien ouvrir d'autre.", "Eine pro Zeile: Name | https://adresse. Das Tablet sieht nur die Namen und kann daher nichts anderes öffnen.", "Un per línia: Nom | https://adreça. La tauleta només en veu els noms, de manera que no pot obrir res més." } },
        { "Install keybind profile...", new[] { "Instalar perfil de teclas...", "Installer le profil de touches...", "Tastenprofil installieren...", "Instal·la el perfil de tecles…" } },
        { "Language", new[] { "Idioma", "Langue", "Sprache", "Idioma" } },
        { "Automatic", new[] { "Automático", "Automatique", "Automatisch", "Automàtic" } },
        { "Save", new[] { "Guardar", "Enregistrer", "Speichern", "Desa" } },
        { "Cancel", new[] { "Cancelar", "Annuler", "Abbrechen", "Cancel·la" } },
        { "This link isn't right:\n\n{0}\n\nWrite each one as: Name | https://address (names up to 24 characters).", new[] { "Este enlace no es correcto:\n\n{0}\n\nEscribe cada uno así: Nombre | https://dirección (nombres de hasta 24 caracteres).", "Ce lien n'est pas correct :\n\n{0}\n\nÉcrivez chacun ainsi : Nom | https://adresse (noms de 24 caractères maximum).", "Dieser Link stimmt nicht:\n\n{0}\n\nSchreib jeden so: Name | https://adresse (Namen bis 24 Zeichen).", "Aquest enllaç no és correcte:\n\n{0}\n\nEscriu-los així: Nom | https://adreça (noms de fins a 24 caràcters)." } },
        { "Up to 12 links, please.", new[] { "Como máximo 12 enlaces, por favor.", "12 liens au maximum, s'il vous plaît.", "Höchstens 12 Links, bitte.", "Com a màxim, 12 enllaços." } },
        { "The pairing code can only have letters and numbers (up to 32).", new[] { "El código de emparejamiento solo puede tener letras y números (hasta 32).", "Le code d'appairage ne peut contenir que des lettres et des chiffres (32 au maximum).", "Der Kopplungscode darf nur Buchstaben und Ziffern haben (bis zu 32).", "El codi d'aparellament només pot contenir lletres i números (fins a 32)." } },
        { "The voice language should look like en-US.", new[] { "El idioma de voz debe tener la forma en-US.", "La langue vocale doit ressembler à en-US.", "Die Sprache für Befehle sollte wie en-US aussehen.", "L'idioma de veu ha de tenir el format en-US." } },
    };
}

// The house colours and fonts (docs/DESIGN.md) and the HUD's drawing pieces: thin-lined
// panels with corner brackets and "// HEADING" titles, letter-spaced labels, rulers.
// Sizes are in 96-dpi pixels; HudView scales everything for the screen.
static class Hud
{
    [DllImport("dwmapi.dll")]
    static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    static extern int SetWindowTheme(IntPtr hwnd, string app, string idList);

    // A dark title bar (Windows 10 2004 and later use attribute 20, earlier builds 19).
    public static void DarkTitleBar(Form form)
    {
        int on = 1;
        try
        {
            if (DwmSetWindowAttribute(form.Handle, 20, ref on, 4) != 0) DwmSetWindowAttribute(form.Handle, 19, ref on, 4);
        }
        catch (Exception) { }
    }

    // Dark scrollbars on a list or text box (Windows 10 1809 and later).
    public static void DarkScrollbars(Control c)
    {
        c.HandleCreated += delegate { try { SetWindowTheme(c.Handle, "DarkMode_Explorer", null); } catch (Exception) { } };
    }

    public static readonly Color Back = Color.FromArgb(5, 9, 15);
    public static readonly Color Panel = Color.FromArgb(11, 23, 38);
    public static readonly Color PanelHi = Color.FromArgb(18, 37, 59);
    public static readonly Color Line = Color.FromArgb(31, 58, 88);
    public static readonly Color Accent = Color.FromArgb(79, 179, 255);
    public static readonly Color Ink = Color.FromArgb(232, 238, 245);
    public static readonly Color Dim = Color.FromArgb(138, 160, 184);
    public static readonly Color Good = Color.FromArgb(91, 227, 125);
    public static readonly Color Warn = Color.FromArgb(255, 179, 71);
    public static readonly Color Bad = Color.FromArgb(255, 107, 107);

    // Bahnschrift (Windows 10 and later) has the squared, instrument-panel look of the deck's
    // fonts; older Windows falls back to Segoe UI.
    static readonly string Display = Installed("Bahnschrift") ? "Bahnschrift" : "Segoe UI";
    static readonly Dictionary<string, Font> fonts = new Dictionary<string, Font>();
    static readonly StringFormat typographic = MakeTypographic();

    static bool Installed(string family)
    {
        try
        {
            using (var f = new FontFamily(family))
                return f.Name == family && f.IsStyleAvailable(FontStyle.Regular) && f.IsStyleAvailable(FontStyle.Bold);
        }
        catch (Exception) { return false; }
    }

    static StringFormat MakeTypographic()
    {
        var f = (StringFormat)StringFormat.GenericTypographic.Clone();
        f.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoWrap;
        return f;
    }

    static Font Get(string family, float px, FontStyle style)
    {
        string key = family + px + style;
        Font f;
        if (fonts.TryGetValue(key, out f)) return f;
        // A family or style this PC lacks falls back to Segoe UI, then to the default font.
        foreach (var name in new[] { family, "Segoe UI" })
        {
            try { return fonts[key] = new Font(name, px, style, GraphicsUnit.Pixel); }
            catch (Exception) { }
        }
        return fonts[key] = new Font(SystemFonts.DefaultFont.FontFamily, px, style, GraphicsUnit.Pixel);
    }

    public static Font Head(float px) { return Get(Display, px, FontStyle.Bold); }
    public static Font Label(float px) { return Get(Display, px, FontStyle.Regular); }
    public static Font Body(float px) { return Get("Segoe UI", px, FontStyle.Regular); }
    public static Font Mono(float px, bool bold) { return Get("Consolas", px, bold ? FontStyle.Bold : FontStyle.Regular); }

    // Screen pixels per 96-dpi pixel, read from the screen itself so no window has to exist yet.
    public static float ScreenZoom()
    {
        try { using (var g = Graphics.FromHwnd(IntPtr.Zero)) return Math.Max(1f, g.DpiX / 96f); }
        catch (Exception) { return 1f; }
    }

    public static Color Fade(Color c, int alpha) { return Color.FromArgb(Math.Max(0, Math.Min(255, alpha)), c); }

    // Letter-spaced text, the HUD's labels and headings; returns its width.
    public static float Spaced(Graphics g, string text, Font font, Color color, float x, float y, float spacing)
    {
        float start = x, scale;
        using (var m = g.Transform) scale = m.Elements[0]; // screen pixels per unit
        float top = (float)Math.Round(y * scale) / scale;
        using (var brush = new SolidBrush(color))
            foreach (char ch in text)
            {
                string s = ch.ToString();
                // Each letter starts on a whole screen pixel; fractional positions smear small text.
                g.DrawString(s, font, brush, (float)Math.Round(x * scale) / scale, top, typographic);
                x += g.MeasureString(s, font, PointF.Empty, typographic).Width + spacing;
            }
        return x - start - (text.Length > 0 ? spacing : 0);
    }

    public static float SpacedWidth(Graphics g, string text, Font font, float spacing)
    {
        float w = 0;
        foreach (char ch in text) w += g.MeasureString(ch.ToString(), font, PointF.Empty, typographic).Width + spacing;
        return w - (text.Length > 0 ? spacing : 0);
    }

    public static float Width(Graphics g, string text, Font font)
    {
        return g.MeasureString(text, font, PointF.Empty, typographic).Width;
    }

    // One line of text, cut short with "..." when it doesn't fit.
    public static void Text(Graphics g, string text, Font font, Color color, RectangleF box)
    {
        using (var brush = new SolidBrush(color))
        using (var f = (StringFormat)typographic.Clone())
        {
            f.Trimming = StringTrimming.EllipsisCharacter;
            f.LineAlignment = StringAlignment.Center;
            g.DrawString(text, font, brush, box, f);
        }
    }

    // Text that wraps inside the box.
    public static void Wrapped(Graphics g, string text, Font font, Color color, RectangleF box)
    {
        using (var brush = new SolidBrush(color))
        using (var f = new StringFormat { Trimming = StringTrimming.EllipsisWord })
            g.DrawString(text, font, brush, box, f);
    }

    public static void Dot(Graphics g, Color color, float cx, float cy)
    {
        using (var glow = new SolidBrush(Fade(color, 60))) g.FillEllipse(glow, cx - 6, cy - 6, 12, 12);
        using (var dot = new SolidBrush(color)) g.FillEllipse(dot, cx - 3.5f, cy - 3.5f, 7, 7);
    }

    // Corner brackets: short accent strokes at the four corners of a box.
    public static void Brackets(Graphics g, RectangleF r, Color color, float arm)
    {
        using (var pen = new Pen(color, 1.5f))
        {
            g.DrawLines(pen, new[] { new PointF(r.Left, r.Top + arm), new PointF(r.Left, r.Top), new PointF(r.Left + arm, r.Top) });
            g.DrawLines(pen, new[] { new PointF(r.Right - arm, r.Top), new PointF(r.Right, r.Top), new PointF(r.Right, r.Top + arm) });
            g.DrawLines(pen, new[] { new PointF(r.Left, r.Bottom - arm), new PointF(r.Left, r.Bottom), new PointF(r.Left + arm, r.Bottom) });
            g.DrawLines(pen, new[] { new PointF(r.Right - arm, r.Bottom), new PointF(r.Right, r.Bottom), new PointF(r.Right, r.Bottom - arm) });
        }
    }

    // A row of ruler ticks, every fourth one longer.
    public static void Ruler(Graphics g, float x, float y, float width, Color color)
    {
        using (var pen = new Pen(color, 1f))
            for (int i = 0; i * 6 <= width; i++)
                g.DrawLine(pen, x + i * 6, y, x + i * 6, y + (i % 4 == 0 ? 6 : 3));
    }

    // A panel: a translucent fill the stars show through, a thin border, accent corner
    // brackets, its "// TITLE" and a ruler in the top-right corner.
    public static void PanelBox(Graphics g, RectangleF r, string title)
    {
        using (var fill = new SolidBrush(Fade(Panel, 168))) g.FillRectangle(fill, r);
        using (var border = new Pen(Line, 1f)) g.DrawRectangle(border, r.X, r.Y, r.Width, r.Height);
        Brackets(g, r, Fade(Accent, 200), 9);
        float w = Spaced(g, "// " + Lang.T(title), Head(11), Accent, r.X + 14, r.Y + 9, 1.6f);
        using (var pen = new Pen(Fade(Line, 255), 1f)) g.DrawLine(pen, r.X + 22 + w, r.Y + 16, r.Right - 70, r.Y + 16);
        Ruler(g, r.Right - 62, r.Y + 11, 48, Fade(Accent, 110));
    }
}

// The main window's face, drawn by hand: a starfield that drifts behind the panels, shifts
// with the mouse (near stars more than far ones) and lights up along a ring sent out by a
// click, plus the window's own buttons, switches, slider and links. The owner draws the
// panels in DrawContent onto an opaque layer, so text gets ClearType; that layer is cached
// and only redrawn after Redraw(), and the stars are drawn over it outside Covers. The stars
// move only while Running (the window is in front) and Windows' "Show animations" is on,
// so nothing animates behind Star Citizen.
class HudView : Control
{
    public enum Kind { Button, Primary, Switch, Slider, Link }

    // Something to click. Box is in 96-dpi pixels, like everything drawn here.
    public class Item
    {
        public Kind Kind;
        public RectangleF Box;
        public string Text = "", Tip = "";
        public bool Visible = true, Enabled = true, On;
        public bool Mono; // links: the address, in a monospaced font
        public int Value, Min, Max = 100;
        public MethodInvoker Click;   // buttons and links; switches after On flips
        public MethodInvoker Changed; // the slider, on every step
        public Action<MouseButtons> OtherButton; // a middle or side mouse button pressed on it
    }

    [DllImport("user32.dll")]
    static extern bool SystemParametersInfo(int action, int param, ref int value, int winIni);

    class Star { public float X, Y, Depth, Size, Phase, Glow; public int Alpha; }
    class Pulse { public float X, Y, Age; }

    public readonly List<Item> Items = new List<Item>();
    public Action<Graphics> DrawContent;
    // Where the moving stars pass behind: panels and text outside them. DrawContent sets
    // these; the items are covered too.
    public readonly List<RectangleF> Covers = new List<RectangleF>();
    public float Zoom = 1f; // screen pixels per 96-dpi pixel

    readonly List<Star> stars = new List<Star>();
    readonly List<Pulse> pulses = new List<Pulse>();
    readonly System.Windows.Forms.Timer frame = new System.Windows.Forms.Timer { Interval = 33 };
    readonly Stopwatch clock = Stopwatch.StartNew();
    readonly ToolTip tips = new ToolTip();
    readonly System.Windows.Forms.Timer tipDelay = new System.Windows.Forms.Timer { Interval = 500 };
    double lastFrame;
    float time;
    PointF mouse, look; // the mouse, and where the stars currently "look" (eases towards it)
    bool mouseInside;
    Bitmap backdrop, content;
    bool contentDirty = true;
    bool running, motion = true;
    Item hover, pressed, dragging, focused;
    bool showFocus;

    public HudView()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint
            | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        TabStop = true;
        var random = new Random(7);
        for (int i = 0; i < 240; i++)
        {
            float depth = i % 7 == 0 ? 1f : i % 3 == 0 ? 0.6f : 0.3f;
            stars.Add(new Star
            {
                X = (float)random.NextDouble(), Y = (float)random.NextDouble(), Depth = depth,
                Size = depth >= 1f ? 2.6f : depth >= 0.6f ? 1.9f : 1.3f,
                Phase = (float)(random.NextDouble() * Math.PI * 2),
                Alpha = (int)(120 + 135 * depth * (0.5 + 0.5 * random.NextDouble())),
            });
        }
        frame.Tick += delegate { Invalidate(); };
        tipDelay.Tick += delegate
        {
            tipDelay.Stop();
            if (hover != null && hover.Tip != "") tips.Show(Lang.T(hover.Tip), this, (int)(mouse.X * Zoom) + 14, (int)(mouse.Y * Zoom) + 22, 6000);
        };
    }

    // Design size: the client area in 96-dpi pixels.
    public SizeF Area { get { return new SizeF(ClientSize.Width / Zoom, ClientSize.Height / Zoom); } }

    // The window is in front: animate, if Windows' animations are on.
    public bool Running
    {
        get { return running; }
        set
        {
            running = value;
            motion = AnimationsOn();
            if (running && motion) { lastFrame = clock.Elapsed.TotalSeconds; frame.Start(); }
            else { frame.Stop(); pulses.Clear(); }
            Invalidate();
        }
    }

    // "Show animations in Windows" (Settings > Accessibility > Visual effects).
    static bool AnimationsOn()
    {
        int on = 1;
        try { SystemParametersInfo(0x1042, 0, ref on, 0); } // SPI_GETCLIENTAREAANIMATION
        catch (Exception) { }
        return on != 0;
    }

    // The panels changed: draw them again on the next frame.
    public void Redraw()
    {
        contentDirty = true;
        Invalidate();
    }

    public Item Add(Kind kind, string text, MethodInvoker click)
    {
        var item = new Item { Kind = kind, Text = text, Click = click };
        Items.Add(item);
        return item;
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        if (backdrop != null) { backdrop.Dispose(); backdrop = null; }
        if (ClientSize.Width > 0 && ClientSize.Height > 0) Redraw();
    }

    // ---------- drawing ----------

    protected override void OnPaint(PaintEventArgs e)
    {
        if (ClientSize.Width < 1 || ClientSize.Height < 1) return;
        double now = clock.Elapsed.TotalSeconds;
        float dt = (float)Math.Min(0.1, now - lastFrame);
        lastFrame = now;
        if (running && motion) Step(dt);

        var g = e.Graphics;
        if (contentDirty || content == null) DrawContentLayer();
        g.DrawImageUnscaled(content, 0, 0);
        g.ScaleTransform(Zoom, Zoom);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var area = Area;
        using (var open = new Region(new RectangleF(0, 0, area.Width, area.Height)))
        {
            foreach (var r in Covers) open.Exclude(RectangleF.Inflate(r, 1, 1));
            foreach (var item in Items) if (item.Visible) open.Exclude(RectangleF.Inflate(item.Box, 2, 2));
            g.Clip = open;
            DrawStars(g);
        }
    }

    void Step(float dt)
    {
        time += dt;
        var area = Area;
        var target = mouseInside ? mouse : new PointF(area.Width / 2, area.Height / 2);
        float ease = Math.Min(1f, dt * 3f);
        look = new PointF(look.X + (target.X - look.X) * ease, look.Y + (target.Y - look.Y) * ease);
        foreach (var s in stars)
        {
            s.X -= 0.0035f * s.Depth * dt;
            if (s.X < -0.02f) s.X += 1.04f;
            s.Glow = Math.Max(0, s.Glow - dt * 1.1f);
        }
        for (int i = pulses.Count - 1; i >= 0; i--)
        {
            pulses[i].Age += dt;
            if (pulses[i].Age > 1.8f) pulses.RemoveAt(i);
        }
    }

    // The still part of the background: deep space, a soft glow and a faint grid.
    Bitmap DrawBackdrop()
    {
        var bmp = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppPArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(Hud.Back);
            g.ScaleTransform(Zoom, Zoom);
            var area = Area;
            using (var path = new GraphicsPath())
            {
                var glow = new RectangleF(-area.Width * 0.25f, -area.Height * 0.6f, area.Width * 1.1f, area.Height * 1.4f);
                path.AddEllipse(glow);
                using (var brush = new PathGradientBrush(path) { CenterColor = Color.FromArgb(46, 30, 90, 150), SurroundColors = new[] { Color.FromArgb(0, 5, 9, 15) } })
                    g.FillEllipse(brush, glow);
            }
            using (var pen = new Pen(Color.FromArgb(14, Hud.Accent), 1f))
            {
                for (float x = 0; x < area.Width; x += 40) g.DrawLine(pen, x, 0, x, area.Height);
                for (float y = 0; y < area.Height; y += 40) g.DrawLine(pen, 0, y, area.Width, y);
            }
        }
        return bmp;
    }

    void DrawStars(Graphics g)
    {
        var area = Area;
        float cx = area.Width / 2, cy = area.Height / 2;
        if (look.IsEmpty) look = new PointF(cx, cy);
        foreach (var s in stars)
        {
            float x = s.X * area.Width - (look.X - cx) * 0.035f * s.Depth;
            float y = s.Y * area.Height - (look.Y - cy) * 0.035f * s.Depth;
            foreach (var p in pulses)
            {
                float d = (float)Math.Sqrt((x - p.X) * (x - p.X) + (y - p.Y) * (y - p.Y));
                if (Math.Abs(d - PulseRadius(p)) < 16) s.Glow = 1f;
            }
            float twinkle = motion ? 0.78f + 0.22f * (float)Math.Sin(time * (1.3 + s.Depth) + s.Phase) : 1f;
            int alpha = (int)(s.Alpha * twinkle + (255 - s.Alpha) * s.Glow);
            var color = s.Glow > 0 ? Blend(Hud.Ink, Hud.Accent, s.Glow) : (s.Depth >= 1f ? Hud.Ink : Color.FromArgb(170, 200, 230));
            float size = s.Size + s.Glow * 2.5f;
            // Near stars and lit ones get a soft halo.
            if (s.Depth >= 1f || s.Glow > 0)
                using (var halo = new SolidBrush(Hud.Fade(color, alpha / 5)))
                    g.FillEllipse(halo, x - size * 1.6f, y - size * 1.6f, size * 3.2f, size * 3.2f);
            using (var brush = new SolidBrush(Hud.Fade(color, alpha)))
                g.FillEllipse(brush, x - size / 2, y - size / 2, size, size);
        }
        foreach (var p in pulses)
        {
            float r = PulseRadius(p), fade = 1f - p.Age / 1.8f;
            using (var pen = new Pen(Hud.Fade(Hud.Accent, (int)(150 * fade)), 1.5f)) g.DrawEllipse(pen, p.X - r, p.Y - r, r * 2, r * 2);
            using (var pen = new Pen(Hud.Fade(Hud.Accent, (int)(60 * fade)), 1f)) g.DrawEllipse(pen, p.X - r * 0.8f, p.Y - r * 0.8f, r * 1.6f, r * 1.6f);
        }
    }

    static float PulseRadius(Pulse p) { return p.Age * 460f; }

    static Color Blend(Color a, Color b, float t)
    {
        return Color.FromArgb((int)(a.R + (b.R - a.R) * t), (int)(a.G + (b.G - a.G) * t), (int)(a.B + (b.B - a.B) * t));
    }

    void DrawContentLayer()
    {
        if (content == null || content.Width != ClientSize.Width || content.Height != ClientSize.Height)
        {
            if (content != null) content.Dispose();
            content = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppRgb);
        }
        if (backdrop == null) backdrop = DrawBackdrop();
        Covers.Clear();
        using (var g = Graphics.FromImage(content))
        {
            g.DrawImageUnscaled(backdrop, 0, 0); // opaque, so the text below can use ClearType
            g.ScaleTransform(Zoom, Zoom);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            if (DrawContent != null) DrawContent(g);
            foreach (var item in Items) if (item.Visible) DrawItem(g, item);
        }
        contentDirty = false;
    }

    void DrawItem(Graphics g, Item item)
    {
        var r = item.Box;
        bool hot = item == hover && item.Enabled, down = item == pressed && hot;
        switch (item.Kind)
        {
            case Kind.Button:
            case Kind.Primary:
                bool primary = item.Kind == Kind.Primary;
                Color fill = !item.Enabled ? Hud.Fade(Hud.Panel, 220)
                    : down ? Hud.Fade(Hud.Accent, 90)
                    : primary ? Hud.Fade(Hud.Accent, hot ? 70 : 38)
                    : hot ? Hud.PanelHi : Hud.Fade(Hud.Panel, 235);
                using (var brush = new SolidBrush(fill)) g.FillRectangle(brush, r);
                using (var pen = new Pen(!item.Enabled ? Hud.Line : primary || hot ? Hud.Accent : Hud.Line, 1f))
                    g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
                if (item.Enabled)
                    using (var pen = new Pen(Hud.Accent, 2f))
                    {
                        g.DrawLine(pen, r.X, r.Y + 1, r.X + 7, r.Y + 1);
                        g.DrawLine(pen, r.Right - 7, r.Bottom - 1, r.Right, r.Bottom - 1);
                    }
                var font = Hud.Head(11.5f);
                string text = Lang.T(item.Text).ToUpperInvariant();
                float w = Hud.SpacedWidth(g, text, font, 1.3f);
                // A longer translation gets a smaller font rather than spilling out of the button.
                for (float px = 11f; w > r.Width - 12 && px >= 9f; px -= 0.5f)
                {
                    font = Hud.Head(px);
                    w = Hud.SpacedWidth(g, text, font, 1.3f);
                }
                Hud.Spaced(g, text, font, item.Enabled ? Hud.Ink : Hud.Fade(Hud.Dim, 140),
                    r.X + (r.Width - w) / 2, r.Y + (r.Height - font.Height) / 2 + 1, 1.3f);
                break;
            case Kind.Switch:
                var track = new RectangleF(r.X, r.Y + (r.Height - 16) / 2, 34, 16);
                using (var brush = new SolidBrush(item.On ? Hud.Fade(Hud.Accent, 60) : Hud.Fade(Hud.Panel, 235))) g.FillRectangle(brush, track);
                using (var pen = new Pen(item.On || hot ? Hud.Accent : Hud.Line, 1f)) g.DrawRectangle(pen, track.X, track.Y, track.Width, track.Height);
                using (var brush = new SolidBrush(item.On ? Hud.Accent : Hud.Dim))
                    g.FillRectangle(brush, item.On ? track.Right - 13 : track.X + 3, track.Y + 3, 10, 10);
                Hud.Text(g, Lang.T(item.Text), Hud.Body(13), hot ? Hud.Ink : Hud.Fade(Hud.Ink, 225), new RectangleF(r.X + 44, r.Y, r.Width - 44, r.Height));
                break;
            case Kind.Slider:
                float mid = r.Y + r.Height / 2, t = item.Max > item.Min ? (item.Value - item.Min) / (float)(item.Max - item.Min) : 0;
                float knob = r.X + 4 + t * (r.Width - 8);
                using (var pen = new Pen(Hud.Line, 2f)) g.DrawLine(pen, r.X, mid, r.Right, mid);
                using (var pen = new Pen(item.Enabled ? Hud.Accent : Hud.Dim, 2f)) g.DrawLine(pen, r.X, mid, knob, mid);
                Hud.Ruler(g, r.X, mid + 6, r.Width, Hud.Fade(Hud.Line, 255));
                using (var brush = new SolidBrush(!item.Enabled ? Hud.Dim : hot || dragging == item ? Hud.Ink : Hud.Accent))
                    g.FillRectangle(brush, knob - 3, mid - 8, 6, 16);
                break;
            case Kind.Link:
                var linkFont = item.Mono ? Hud.Mono(14, false) : Hud.Body(12.5f);
                string label = Lang.T(item.Text);
                float lw = Hud.Width(g, label, linkFont);
                r.Width = Math.Min(lw + 2, item.Box.Width);
                Hud.Text(g, label, linkFont, hot ? Hud.Ink : Hud.Accent, item.Box);
                if (hot) using (var pen = new Pen(Hud.Ink, 1f)) g.DrawLine(pen, r.X, r.Bottom - 2, r.Right, r.Bottom - 2);
                break;
        }
        if (item == focused && showFocus && Focused)
            using (var pen = new Pen(Hud.Accent, 1f) { DashStyle = DashStyle.Dot })
                g.DrawRectangle(pen, r.X - 3, r.Y - 3, r.Width + 6, r.Height + 6);
    }

    // The QR code at whole screen pixels per module, so cameras read it easily, with a white
    // margin of four modules.
    public void DrawQr(Graphics g, RectangleF box, string[] rows)
    {
        var state = g.Save();
        g.ResetTransform();
        g.SmoothingMode = SmoothingMode.None;
        int n = rows.Length;
        int side = (int)(Math.Min(box.Width, box.Height) * Zoom);
        int module = Math.Max(1, side / (n + 8));
        int size = module * (n + 8);
        int x0 = (int)(box.X * Zoom) + ((int)(box.Width * Zoom) - size) / 2, y0 = (int)(box.Y * Zoom) + ((int)(box.Height * Zoom) - size) / 2;
        g.FillRectangle(Brushes.White, x0, y0, size, size);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < rows[y].Length; x++)
                if (rows[y][x] == '1') g.FillRectangle(Brushes.Black, x0 + (x + 4) * module, y0 + (y + 4) * module, module, module);
        g.Restore(state);
    }

    // ---------- mouse and keyboard ----------

    Item ItemAt(PointF p)
    {
        for (int i = Items.Count - 1; i >= 0; i--)
        {
            var item = Items[i];
            if (item.Visible && item.Enabled && RectangleF.Inflate(item.Box, 0, item.Kind == Kind.Slider ? 6 : 0).Contains(p)) return item;
        }
        return null;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        mouse = new PointF(e.X / Zoom, e.Y / Zoom);
        mouseInside = true;
        if (dragging != null) { SlideTo(dragging, mouse.X); return; }
        var now = ItemAt(mouse);
        if (now == hover) return;
        hover = now;
        Cursor = hover != null ? Cursors.Hand : Cursors.Default;
        tips.Hide(this);
        tipDelay.Stop();
        if (hover != null && hover.Tip != "") tipDelay.Start();
        Redraw();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        mouseInside = false;
        tips.Hide(this);
        tipDelay.Stop();
        if (hover != null) { hover = null; Cursor = Cursors.Default; Redraw(); }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();
        showFocus = false;
        var p = new PointF(e.X / Zoom, e.Y / Zoom);
        var item = ItemAt(p);
        if (e.Button != MouseButtons.Left)
        {
            if (item != null && item.OtherButton != null) item.OtherButton(e.Button);
            return;
        }
        tips.Hide(this);
        if (item == null)
        {
            // An empty spot sends a pulse through the stars.
            if (running && motion) pulses.Add(new Pulse { X = p.X, Y = p.Y });
            return;
        }
        pressed = focused = item;
        if (item.Kind == Kind.Slider) { dragging = item; SlideTo(item, p.X); }
        Redraw();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        var p = new PointF(e.X / Zoom, e.Y / Zoom);
        var item = pressed;
        pressed = null;
        dragging = null;
        if (item != null && item.Kind != Kind.Slider && ItemAt(p) == item) Activate(item);
        Redraw();
    }

    void SlideTo(Item item, float x)
    {
        float t = Math.Max(0, Math.Min(1, (x - item.Box.X - 4) / (item.Box.Width - 8)));
        SetValue(item, item.Min + (int)Math.Round(t * (item.Max - item.Min)));
    }

    void SetValue(Item item, int value)
    {
        value = Math.Max(item.Min, Math.Min(item.Max, value));
        if (value == item.Value) return;
        item.Value = value;
        if (item.Changed != null) item.Changed();
        Redraw();
    }

    void Activate(Item item)
    {
        if (item.Kind == Kind.Switch) item.On = !item.On;
        if (item.Click != null) item.Click();
        Redraw();
    }

    // Tab and Shift+Tab move between the items, Enter or Space presses one, and the arrow
    // keys move the slider.
    protected override bool ProcessDialogKey(Keys keyData)
    {
        var key = keyData & Keys.KeyCode;
        if (key != Keys.Tab || (keyData & (Keys.Control | Keys.Alt)) != 0) return base.ProcessDialogKey(keyData);
        var usable = Items.FindAll(i => i.Visible && i.Enabled);
        int at = usable.IndexOf(focused) + ((keyData & Keys.Shift) != 0 ? -1 : 1);
        if (focused == null || at < 0 || at >= usable.Count)
        {
            // Past the last item (or before the first): on to the window's other controls.
            focused = null;
            Redraw();
            return base.ProcessDialogKey(keyData);
        }
        focused = usable[at];
        showFocus = true;
        Redraw();
        return true;
    }

    // Arrow keys move the slider; Enter and Space press the focused item instead of the
    // window's default button.
    protected override bool IsInputKey(Keys keyData)
    {
        var key = keyData & Keys.KeyCode;
        if ((key == Keys.Enter || key == Keys.Space) && focused != null && showFocus) return true;
        return key == Keys.Left || key == Keys.Right || key == Keys.Up || key == Keys.Down || base.IsInputKey(keyData);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (focused == null || !focused.Visible || !focused.Enabled) return;
        if (focused.Kind == Kind.Slider)
        {
            int step = e.Shift ? 5 : 1;
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Down) SetValue(focused, focused.Value - step);
            else if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Up) SetValue(focused, focused.Value + step);
        }
        else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) Activate(focused);
    }

    // When Tab was last pressed anywhere in the app. Windows also moves focus "by Tab" when a
    // window opens, so only a real Tab press shows the focus ring.
    static DateTime lastTab = DateTime.MinValue;

    class TabWatch : IMessageFilter
    {
        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == 0x0100 && (int)m.WParam == 0x09) lastTab = DateTime.Now; // WM_KEYDOWN, VK_TAB
            return false;
        }
    }

    static HudView() { Application.AddMessageFilter(new TabWatch()); }

    // Tabbed into from another control: start at the first item, or the last with Shift.
    // Focus that comes with the window opening or a click shows no focus ring.
    protected override void Select(bool directed, bool forward)
    {
        var usable = Items.FindAll(i => i.Visible && i.Enabled);
        if (directed && usable.Count > 0 && (DateTime.Now - lastTab).TotalMilliseconds < 500)
        {
            focused = forward ? usable[0] : usable[usable.Count - 1];
            showFocus = true;
        }
        base.Select(directed, forward);
        Redraw();
    }

    protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Redraw(); }
    protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Redraw(); }
}

// The other windows' HUD look: the main window's stars, standing still, and its panels,
// buttons and switches, drawn by a HudView. Fields that take typing or picking (text
// boxes, number boxes, lists) stay real Windows controls on top, in the house colours
// inside a drawn frame. Positions are in 96-dpi pixels, like HudView's.
class HudDialog : Form
{
    // Text drawn in a panel: a caption, a hint or a status line. Change Text or Color, then Redraw.
    protected class Note
    {
        public string Text = "";
        public Color Color = Hud.Dim;
        public RectangleF Box;
        public Font Font;
        public bool Caption;
    }

    protected readonly HudView hud = new HudView();
    // A panel's title and box; set Box again in Arrange to move it.
    protected class PanelPart
    {
        public string Title;
        public RectangleF Box;
    }

    readonly List<PanelPart> panels = new List<PanelPart>();
    readonly List<Note> notes = new List<Note>();
    readonly List<Control> fields = new List<Control>();
    readonly Dictionary<Control, RectangleF> frames = new Dictionary<Control, RectangleF>();
    // Enter and Esc, the window's default and cancel buttons.
    protected MethodInvoker Accept, Cancel;

    protected HudDialog(string title, Icon icon, int width, int height)
    {
        SuspendLayout();
        Text = Lang.T(title);
        Icon = icon;
        ShowInTaskbar = false;
        BackColor = Hud.Back;
        ForeColor = Hud.Ink;
        Font = new Font("Segoe UI", 9.75f);
        AutoScaleMode = AutoScaleMode.None; // HudView scales itself, and Place the fields
        hud.Zoom = Hud.ScreenZoom();
        ClientSize = new Size((int)(width * hud.Zoom), (int)(height * hud.Zoom));
        StartPosition = FormStartPosition.CenterParent;
        hud.Dock = DockStyle.Fill;
        hud.DrawContent = DrawDialog;
        Controls.Add(hud);
        hud.SizeChanged += delegate { Arrange(); PlaceFields(); Redraw(); };
    }

    // Resizable windows move their panels, notes and fields here (see Frame).
    protected virtual void Arrange() { }

    protected void Frame(Control c, RectangleF frame) { frames[c] = frame; }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        Hud.DarkTitleBar(this);
    }

    // Fonts and options set after Field change a field's height, so place them again now.
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Arrange();
        PlaceFields();
    }

    protected void Redraw() { hud.Redraw(); }

    protected PanelPart Panel(string title, float x, float y, float w, float h)
    {
        var panel = new PanelPart { Title = title, Box = new RectangleF(x, y, w, h) };
        panels.Add(panel);
        return panel;
    }

    // A small letter-spaced label above a field.
    protected Note Caption(string text, float x, float y)
    {
        var note = new Note { Text = Lang.T(text).ToUpperInvariant(), Box = new RectangleF(x, y, 400, 14), Font = Hud.Label(11.5f), Caption = true };
        notes.Add(note);
        return note;
    }

    // Wrapping text: help under a field, or a status message.
    protected Note Hint(string text, float x, float y, float w, float h)
    {
        var note = new Note { Text = Lang.T(text), Box = new RectangleF(x, y, w, h), Font = Hud.Body(11.5f) };
        notes.Add(note);
        return note;
    }

    protected HudView.Item Button(string text, float x, float y, float w, MethodInvoker click)
    {
        var item = hud.Add(HudView.Kind.Button, text, click);
        item.Box = new RectangleF(x, y, w, 28);
        return item;
    }

    protected HudView.Item Primary(string text, float x, float y, float w, MethodInvoker click)
    {
        var item = Button(text, x, y, w, click);
        item.Kind = HudView.Kind.Primary;
        return item;
    }

    protected HudView.Item Switch(string text, bool on, float x, float y, float w)
    {
        var item = hud.Add(HudView.Kind.Switch, text, null);
        item.On = on;
        item.Box = new RectangleF(x, y, w, 22);
        return item;
    }

    // A real control in a drawn frame (x, y, w, h is the frame). Text boxes and lists lose
    // their own border, so the frame is the only one; the frame lights up while it has focus.
    protected T Field<T>(T c, float x, float y, float w, float h) where T : Control
    {
        c.BackColor = Hud.Panel;
        c.ForeColor = Hud.Ink;
        var text = c as TextBoxBase;
        if (text != null) text.BorderStyle = BorderStyle.None;
        var list = c as ListBox;
        if (list != null) list.BorderStyle = BorderStyle.None;
        var view = c as ListView;
        if (view != null) view.BorderStyle = BorderStyle.None;
        var number = c as UpDownBase;
        if (number != null) number.BorderStyle = BorderStyle.None;
        if (list != null || view != null || (text != null && text.Multiline)) Hud.DarkScrollbars(c);
        frames[c] = new RectangleF(x, y, w, h);
        c.GotFocus += delegate { Redraw(); };
        c.LostFocus += delegate { Redraw(); };
        c.EnabledChanged += delegate { Redraw(); };
        fields.Add(c);
        hud.Controls.Add(c);
        PlaceFields();
        return c;
    }

    // Fields sit inside their frames: single-line ones centred, the rest filling it.
    void PlaceFields()
    {
        float z = hud.Zoom;
        foreach (var c in fields)
        {
            var f = frames[c];
            var text = c as TextBoxBase;
            bool oneLine = c is ComboBox || c is UpDownBase || (text != null && !text.Multiline);
            if (c is ComboBox)
                c.Bounds = new Rectangle((int)(f.X * z), (int)((f.Y + 1) * z), (int)(f.Width * z), (int)((f.Height - 2) * z));
            else if (oneLine)
            {
                int inner = Math.Max(c.Height, c.Font.Height); // single-line boxes size their own height
                c.Bounds = new Rectangle((int)((f.X + 7) * z), (int)(f.Y * z + (f.Height * z - inner) / 2), (int)((f.Width - 9) * z), inner);
            }
            else c.Bounds = new Rectangle((int)((f.X + 6) * z), (int)((f.Y + 5) * z), (int)((f.Width - 8) * z), (int)((f.Height - 9) * z));
        }
    }

    void DrawDialog(Graphics g)
    {
        foreach (var p in panels) { Hud.PanelBox(g, p.Box, p.Title); hud.Covers.Add(p.Box); }
        foreach (var n in notes) hud.Covers.Add(n.Box);
        foreach (var c in fields)
        {
            if (c is ComboBox) continue; // a flat combo box draws its own frame
            var f = frames[c];
            using (var fill = new SolidBrush(Hud.Panel)) g.FillRectangle(fill, f);
            using (var pen = new Pen(c.ContainsFocus ? Hud.Accent : c.Enabled ? Hud.Line : Hud.Fade(Hud.Line, 120), 1f))
                g.DrawRectangle(pen, f.X, f.Y, f.Width, f.Height);
        }
        foreach (var n in notes)
        {
            if (n.Caption) Hud.Spaced(g, n.Text, n.Font, n.Color, n.Box.X, n.Box.Y, 1.6f);
            else Hud.Wrapped(g, n.Text, n.Font, n.Color, n.Box);
        }
    }

    // Enter presses the default button and Esc the cancel one, unless a drawn item or a
    // field that takes them has the focus.
    protected override bool ProcessDialogKey(Keys keyData)
    {
        if (keyData == Keys.Enter && Accept != null) { Accept(); return true; }
        if (keyData == Keys.Escape && Cancel != null) { Cancel(); return true; }
        return base.ProcessDialogKey(keyData);
    }
}

class DeckWindow : Form
{
    const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    const string RunValue = "Tablet Deck";
    // The window's size in 96-dpi pixels; it scales with the screen.
    const int DesignWidth = 900, DesignHeight = 640;

    readonly string appDir = Path.GetDirectoryName(Application.ExecutablePath);
    readonly string serverArgs;
    bool startHidden;
    bool quitting;
    bool trayHintShown;
    Process server;
    string link = "";
    string settingsPath = "";
    string fatalText = "";

    // What the panels show; DrawHud draws them from these.
    readonly HudView hud = new HudView();
    string[] qrRows = new string[0];
    // Status lines are kept as functions, so they show in the current language even after
    // it changes.
    string chip = "STARTING", others = "", pairingCode = "-", version = "";
    Func<string> stateText = () => Lang.T("Starting...");
    Color stateColor = Hud.Dim;
    bool dryRun, focusCheck = true;
    int tabletCount;
    DateTime runningSince = DateTime.MinValue;
    Func<string> voiceText = () => Lang.T("Starting..."), lastHeard = () => "";
    Color voiceColor = Hud.Dim, lastHeardColor = Hud.Dim;
    // Presses from the tablets and voice, for the telemetry panel.
    readonly Queue<DateTime> recentPresses = new Queue<DateTime>();
    int pressCount, blockedCount;
    string lastPress = "";
    // Panel boxes, set by Arrange.
    RectangleF connectBox, linkBox, telemetryBox, voiceBox, systemBox, logBox, bannerBox;

    readonly HudView.Item addressLink, copyButton, settingsButton, editButton, restartButton;
    readonly HudView.Item voiceListButton, strictness, startWithWindows, autoUpdate, folderLink, checkLink, feedbackLink;
    readonly HudView.Item networkLink;
    // The adapter Windows treats as a Public network (tablets can't connect), or "".
    string publicNetwork = "";
    bool publicNetworkNoticed;
    const string FeedbackPage = "https://github.com/nodieCode/tablet-deck/issues/new/choose";
    readonly HudView.Item bannerWhatsNew, bannerUpdate, bannerLater;
    string editorUrl = ""; // the layout editor on this PC, set when the server is ready
    bool serverReady; // the server has said "ready" and takes settings on stdin
    readonly RichTextBox log = new RichTextBox();
    readonly NotifyIcon tray = new NotifyIcon();
    readonly System.Windows.Forms.Timer copiedTimer = new System.Windows.Forms.Timer();
    // Sends the slider's value once it has stopped moving, not on every step of a drag.
    readonly System.Windows.Forms.Timer strictnessTimer = new System.Windows.Forms.Timer();
    // Once a second: the uptime and the presses graph.
    readonly System.Windows.Forms.Timer second = new System.Windows.Forms.Timer { Interval = 1000 };
    readonly List<string[]> voiceCommands = new List<string[]>(); // [phrase, button label]
    VoiceCommandsWindow commandsWindow;

    // ---- updates ----
    const string LatestReleaseApi = "https://api.github.com/repos/nodieCode/tablet-deck/releases/latest";
    const string ReleasesPage = "https://github.com/nodieCode/tablet-deck/releases";
    const string DownloadPrefix = "https://github.com/nodieCode/tablet-deck/releases/download/";
    const string AppKey = @"Software\Tablet Deck";
    // This window's version, e.g. "1.1.0" (AssemblyInformationalVersion, set by scripts/build.js).
    static readonly string CurrentVersion = Application.ProductVersion;
    bool checkedAtStart;
    bool checking;
    readonly System.Windows.Forms.Timer dailyCheck = new System.Windows.Forms.Timer();
    bool bannerShown;
    Func<string> bannerText = () => "";
    Func<string> trayText = () => "Tablet Deck";
    string whatsNewUrl = "";
    string updateVersion = "", updateSetupUrl = "", updateShaUrl = "", dismissedVersion = "";
    Process installer;

    public DeckWindow(string[] args)
    {
        var passOn = new List<string>();
        foreach (var arg in args)
        {
            if (arg == "--minimized") startHidden = true;
            else passOn.Add(arg.IndexOfAny(new[] { ' ', '"' }) >= 0 ? "\"" + arg.Replace("\"", "\\\"") + "\"" : arg);
        }
        serverArgs = ("--ui " + string.Join(" ", passOn)).Trim();

        SuspendLayout();
        Text = "Tablet Deck";
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        BackColor = Hud.Back;
        ForeColor = Hud.Ink;
        Font = new Font("Segoe UI", 9.75f);
        AutoScaleMode = AutoScaleMode.None; // HudView scales itself
        hud.Zoom = Hud.ScreenZoom();
        ClientSize = new Size((int)(DesignWidth * hud.Zoom), (int)(DesignHeight * hud.Zoom));
        MinimumSize = SizeFromClientSize(ClientSize);
        StartPosition = FormStartPosition.CenterScreen;

        hud.Dock = DockStyle.Fill;
        hud.DrawContent = DrawHud;
        Controls.Add(hud);

        addressLink = hud.Add(HudView.Kind.Link, "", delegate { if (link != "") Process.Start(link); });
        addressLink.Mono = true;
        addressLink.Tip = "Opens the deck in this PC's browser";
        copyButton = hud.Add(HudView.Kind.Button, "Copy link", delegate { CopyLink(); });
        copyButton.Tip = "Copies the address with the pairing code, to send to a tablet";
        settingsButton = hud.Add(HudView.Kind.Button, "Settings", delegate { OpenSettings(); });
        settingsButton.Tip = "Pairing code, port, focus check, tap length, voice language and the Star Citizen keybind profile";
        // Edit layout and Restart share a place: one shows while running, the other when stopped.
        editButton = hud.Add(HudView.Kind.Primary, "Edit layout", delegate { OpenEditor(); });
        editButton.Visible = false;
        editButton.Tip = "Opens the layout editor in your browser: move buttons, change their keys, labels and icons";
        restartButton = hud.Add(HudView.Kind.Primary, "Restart", delegate { StartServer(); });
        restartButton.Visible = false;

        voiceListButton = hud.Add(HudView.Kind.Button, "Commands", delegate { ShowVoiceCommands(); });
        voiceListButton.Enabled = false;
        voiceListButton.Tip = "Everything you can say, and the button each one presses";
        // How sure Windows' speech recognition must be before a command counts (voiceConfidence).
        strictness = hud.Add(HudView.Kind.Slider, "", null);
        strictness.Min = 20;
        strictness.Max = 95;
        strictness.Value = 60;
        strictness.Enabled = false;
        strictness.Changed = delegate { strictnessTimer.Stop(); strictnessTimer.Start(); };
        strictness.Tip = "Lower accepts more but may mishear. Higher is stricter.";

        startWithWindows = hud.Add(HudView.Kind.Switch, "Start with Windows (in the tray)", null);
        startWithWindows.On = RunsAtStartup();
        startWithWindows.Click = delegate { SetRunAtStartup(startWithWindows.On); };
        autoUpdate = hud.Add(HudView.Kind.Switch, "Check for updates automatically", null);
        autoUpdate.On = true;
        autoUpdate.Click = delegate
        {
            SendToServer("set autoUpdate " + (autoUpdate.On ? "on" : "off"));
            if (autoUpdate.On) CheckForUpdates(false);
        };
        autoUpdate.Tip = "Looks for a new version on GitHub when Tablet Deck starts and once a day. Updates only install when you click Update now.";
        folderLink = hud.Add(HudView.Kind.Link, "Settings folder", delegate { OpenSettingsFolder(); });
        folderLink.Tip = "Opens the folder with config.json and your own layouts";
        checkLink = hud.Add(HudView.Kind.Link, "Check for updates now", delegate { CheckForUpdates(true); });
        feedbackLink = hud.Add(HudView.Kind.Link, "Feedback & ideas", delegate { Process.Start(FeedbackPage); });
        feedbackLink.Tip = "Report a problem or suggest an idea on GitHub";
        // Opens Windows' network settings, where the network can be set to Private.
        networkLink = hud.Add(HudView.Kind.Link, "Set it to Private ›", delegate { Process.Start("ms-settings:network-status"); });
        networkLink.Visible = false;
        networkLink.Tip = "Pick your network there and set its network profile to Private";

        // The update banner, above the panels when there's news.
        bannerWhatsNew = hud.Add(HudView.Kind.Button, "What's new", delegate { if (whatsNewUrl != "") Process.Start(whatsNewUrl); });
        bannerUpdate = hud.Add(HudView.Kind.Primary, "Update now", delegate { InstallUpdate(); });
        bannerLater = hud.Add(HudView.Kind.Button, "Later", delegate { dismissedVersion = updateVersion; HideBanner(); });
        foreach (var b in new[] { bannerWhatsNew, bannerUpdate, bannerLater }) b.Visible = false;

        strictnessTimer.Interval = 400;
        strictnessTimer.Tick += delegate
        {
            strictnessTimer.Stop();
            SendToServer("set voiceConfidence " + (strictness.Value / 100.0).ToString("0.00", CultureInfo.InvariantCulture));
        };

        log.ReadOnly = true;
        log.BackColor = Hud.Panel;
        log.ForeColor = Hud.Ink;
        log.BorderStyle = BorderStyle.None;
        log.Font = new Font("Consolas", 9f);
        log.DetectUrls = false;
        log.WordWrap = true;
        log.ScrollBars = RichTextBoxScrollBars.Vertical;
        Hud.DarkScrollbars(log);
        hud.Controls.Add(log);

        hud.SizeChanged += delegate { Arrange(); };
        Arrange();

        second.Tick += delegate
        {
            while (recentPresses.Count > 0 && (DateTime.Now - recentPresses.Peek()).TotalSeconds > 60) recentPresses.Dequeue();
            if (Visible && WindowState != FormWindowState.Minimized) hud.Redraw();
        };
        second.Start();

        dailyCheck.Interval = 24 * 60 * 60 * 1000;
        dailyCheck.Tick += delegate { if (autoUpdate.On) CheckForUpdates(false); };
        dailyCheck.Start();

        tray.Icon = Icon;
        tray.Text = trayText();
        tray.Visible = true;
        tray.DoubleClick += delegate { ShowFromTray(); };
        BuildTrayMenu();

        copiedTimer.Interval = 1500;
        copiedTimer.Tick += delegate { copiedTimer.Stop(); copyButton.Text = "Copy link"; hud.Redraw(); };

        ResumeLayout(false);

        if (!IsHandleCreated) CreateHandle(); // lets server output reach the window even while it starts hidden
        StartServer();
        ShowWhatsNewAfterUpdate();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        Hud.DarkTitleBar(this);
    }

    // The stars only move while this window is in front.
    protected override void OnActivated(EventArgs e) { base.OnActivated(e); hud.Running = true; }
    protected override void OnDeactivate(EventArgs e) { base.OnDeactivate(e); hud.Running = false; }

    // ---------- layout and drawing ----------

    // Places the panels and items for the window's size, in 96-dpi pixels: the header, the
    // update banner if shown, CONNECT and LINK side by side, TELEMETRY under LINK, VOICE and
    // SYSTEM below, and the log filling the rest.
    void Arrange()
    {
        var area = hud.Area;
        if (area.Width < 200 || area.Height < 200) return; // minimized
        float w = area.Width, top = 74;
        bannerBox = new RectangleF(20, top, w - 40, 38);
        if (bannerShown) top += 50;
        connectBox = new RectangleF(20, top, 280, 300);
        linkBox = new RectangleF(316, top, w - 336, 172);
        telemetryBox = new RectangleF(316, top + 184, w - 336, 116);
        voiceBox = new RectangleF(20, top + 312, w - 356, 140);
        systemBox = new RectangleF(w - 320, top + 312, 300, 140);
        logBox = new RectangleF(20, top + 464, w - 40, Math.Max(70, area.Height - top - 464 - 16));

        float x = linkBox.X + 16, right = linkBox.Right - 16, row = linkBox.Bottom - 42;
        addressLink.Box = new RectangleF(x, linkBox.Y + 46, right - x, 20);
        editButton.Box = restartButton.Box = new RectangleF(right - 128, row, 128, 28);
        settingsButton.Box = new RectangleF(right - 128 - 10 - 104, row, 104, 28);
        copyButton.Box = new RectangleF(right - 128 - 10 - 104 - 10 - 104, row, 104, 28);

        voiceListButton.Box = new RectangleF(voiceBox.Right - 112, voiceBox.Y + 30, 96, 24);
        strictness.Box = new RectangleF(voiceBox.X + 104, voiceBox.Y + 72, 190, 14);
        startWithWindows.Box = new RectangleF(systemBox.X + 16, systemBox.Y + 32, systemBox.Width - 32, 22);
        autoUpdate.Box = new RectangleF(systemBox.X + 16, systemBox.Y + 60, systemBox.Width - 32, 22);
        folderLink.Box = new RectangleF(systemBox.X + 16, systemBox.Y + 92, 128, 18);
        feedbackLink.Box = new RectangleF(systemBox.X + 152, systemBox.Y + 92, systemBox.Width - 168, 18);
        checkLink.Box = new RectangleF(systemBox.X + 16, systemBox.Y + 112, systemBox.Width - 32, 18);
        networkLink.Box = new RectangleF(linkBox.Right - 16 - 150, linkBox.Y + 67, 150, 18);

        float bx = bannerBox.Right - 8;
        foreach (var b in new[] { bannerLater, bannerUpdate, bannerWhatsNew })
        {
            float bw = b == bannerUpdate ? 120 : 100;
            if (b.Visible) { bx -= bw; b.Box = new RectangleF(bx, bannerBox.Y + 5, bw, 28); bx -= 8; }
        }

        float z = hud.Zoom;
        log.Bounds = new Rectangle((int)((logBox.X + 12) * z), (int)((logBox.Y + 30) * z), (int)((logBox.Width - 24) * z), (int)((logBox.Height - 40) * z));
        hud.Redraw();
    }

    void DrawHud(Graphics g)
    {
        var area = hud.Area;
        // The stars pass behind the panels and the header's text.
        hud.Covers.AddRange(new[] { connectBox, linkBox, telemetryBox, voiceBox, systemBox, logBox,
            new RectangleF(20, 10, 360, 48), new RectangleF(area.Width - 240, 12, 220, 44) });
        if (bannerShown) hud.Covers.Add(bannerBox);
        DrawHeader(g, area.Width);
        if (bannerShown)
        {
            using (var fill = new SolidBrush(Hud.Fade(Hud.Accent, 40))) g.FillRectangle(fill, bannerBox);
            using (var pen = new Pen(Hud.Accent, 1f)) g.DrawRectangle(pen, bannerBox.X, bannerBox.Y, bannerBox.Width, bannerBox.Height);
            float buttonsLeft = bannerBox.Right;
            foreach (var b in new[] { bannerWhatsNew, bannerUpdate, bannerLater }) if (b.Visible) buttonsLeft = Math.Min(buttonsLeft, b.Box.X);
            Hud.Text(g, bannerText(), Hud.Body(13.5f), Hud.Ink, new RectangleF(bannerBox.X + 14, bannerBox.Y, buttonsLeft - bannerBox.X - 24, bannerBox.Height));
        }
        DrawConnect(g);
        DrawLink(g);
        DrawTelemetry(g);
        DrawVoice(g);
        Hud.PanelBox(g, systemBox, "SYSTEM");
        Hud.PanelBox(g, logBox, "LOG");
        using (var fill = new SolidBrush(Hud.Panel))
            g.FillRectangle(fill, logBox.X + 8, logBox.Y + 26, logBox.Width - 16, logBox.Height - 32);
    }

    void DrawHeader(Graphics g, float width)
    {
        // An original mark: a hexagon frame around a pointer, like a cockpit indicator.
        var hex = new PointF[6];
        for (int i = 0; i < 6; i++) hex[i] = new PointF(38 + 15 * (float)Math.Cos(Math.PI / 3 * i + Math.PI / 6), 34 + 15 * (float)Math.Sin(Math.PI / 3 * i + Math.PI / 6));
        using (var pen = new Pen(Hud.Accent, 1.5f)) g.DrawPolygon(pen, hex);
        using (var brush = new SolidBrush(Hud.Accent))
            g.FillPolygon(brush, new[] { new PointF(38, 25), new PointF(44, 40), new PointF(38, 36), new PointF(32, 40) });
        float tw = Hud.Spaced(g, "TABLET DECK", Hud.Head(24), Hud.Accent, 64, 17, 3f);
        if (version != "") Hud.Spaced(g, "V" + version, Hud.Label(11.5f), Hud.Dim, 64 + tw + 12, 29, 1.2f);
        Hud.Spaced(g, Lang.T("STAR CITIZEN CONTROL DECK"), Hud.Label(11.5f), Hud.Dim, 65, 45, 2.4f);

        // Status on the right: a dot and one word, the uptime under it.
        var chipFont = Hud.Head(13);
        string word = Lang.T(chip);
        float cw = Hud.SpacedWidth(g, word, chipFont, 2f);
        Hud.Spaced(g, word, chipFont, stateColor, width - 22 - cw, 20, 2f);
        Hud.Dot(g, stateColor, width - 34 - cw, 28);
        if (runningSince != DateTime.MinValue)
        {
            var up = DateTime.Now - runningSince;
            string text = Lang.T("UPTIME") + " " + ((int)up.TotalHours).ToString("00") + ":" + up.Minutes.ToString("00") + ":" + up.Seconds.ToString("00");
            float uw = Hud.SpacedWidth(g, text, Hud.Mono(11, false), 1f);
            Hud.Spaced(g, text, Hud.Mono(11, false), Hud.Dim, width - 22 - uw, 42, 1f);
        }

        using (var pen = new Pen(Hud.Line, 1f)) g.DrawLine(pen, 20, 62, width - 20, 62);
        using (var pen = new Pen(Hud.Accent, 2f)) g.DrawLine(pen, 20, 62, 150, 62);
        Hud.Ruler(g, width - 200, 64, 180, Hud.Fade(Hud.Accent, 90));
    }

    void DrawConnect(Graphics g)
    {
        var r = connectBox;
        Hud.PanelBox(g, r, "CONNECT");
        var qrBox = new RectangleF(r.X + 22, r.Y + 34, 236, 236);
        if (qrRows.Length > 0) hud.DrawQr(g, qrBox, qrRows);
        else
        {
            using (var pen = new Pen(Hud.Line, 1f) { DashStyle = DashStyle.Dash }) g.DrawRectangle(pen, qrBox.X, qrBox.Y, qrBox.Width, qrBox.Height);
            string text = Lang.T(serverReady ? "NO NETWORK FOUND" : chip == "STOPPED" ? "OFFLINE" : "WAITING FOR THE SERVER");
            float w = Hud.SpacedWidth(g, text, Hud.Label(11), 1.6f);
            Hud.Spaced(g, text, Hud.Label(11), Hud.Dim, qrBox.X + (qrBox.Width - w) / 2, qrBox.Y + qrBox.Height / 2 - 6, 1.6f);
        }
        Hud.Brackets(g, RectangleF.Inflate(qrBox, 6, 6), Hud.Accent, 14);
        string caption = Lang.T("SCAN WITH YOUR TABLET'S CAMERA");
        float cw = Hud.SpacedWidth(g, caption, Hud.Label(11.5f), 1.4f);
        Hud.Spaced(g, caption, Hud.Label(11.5f), Hud.Dim, r.X + (r.Width - cw) / 2, r.Bottom - 22, 1.4f);
    }

    void DrawLink(Graphics g)
    {
        var r = linkBox;
        float x = r.X + 16, w = r.Width - 32;
        Hud.PanelBox(g, r, "LINK");
        if (serverReady)
        {
            Hud.Spaced(g, Lang.T("ADDRESS"), Hud.Label(11.5f), Hud.Dim, x, r.Y + 32, 1.6f);
            if (link == "") Hud.Text(g, Lang.T("No network found. Connect this PC to Wi-Fi or a network."), Hud.Body(13), Hud.Warn, new RectangleF(x, r.Y + 46, w, 20));
            if (publicNetwork != "")
                Hud.Text(g, Lang.F("Windows treats \"{0}\" as Public: tablets can't connect.", publicNetwork),
                    Hud.Body(12), Hud.Warn, new RectangleF(x, r.Y + 67, networkLink.Box.X - x - 8, 18));
            else if (others != "") Hud.Text(g, Lang.T("ALSO") + "  " + others, Hud.Mono(11, false), Hud.Dim, new RectangleF(x, r.Y + 68, w, 16));
        }
        else Hud.Wrapped(g, stateText(), Hud.Body(13.5f), stateColor, new RectangleF(x, r.Y + 34, w, 50));
        Hud.Spaced(g, Lang.T("PAIRING CODE"), Hud.Label(11.5f), Hud.Dim, x, r.Y + 92, 1.6f);
        Hud.Spaced(g, pairingCode, Hud.Mono(28, true), Hud.Ink, x - 1, r.Y + 106, 3f);
    }

    void DrawTelemetry(Graphics g)
    {
        var r = telemetryBox;
        Hud.PanelBox(g, r, "TELEMETRY");
        float y = r.Y + 32, h = r.Height - 46, x = r.X + 16, gap = 10;
        float total = r.Width - 32 - gap * 2;
        var tablets = new RectangleF(x, y, total * 0.24f, h);
        var keys = new RectangleF(tablets.Right + gap, y, total * 0.31f, h);
        var presses = new RectangleF(keys.Right + gap, y, r.Right - 16 - keys.Right - gap, h);

        Tile(g, tablets, "TABLETS", tabletCount.ToString(), Hud.Ink, "CONNECTED");
        string target = !serverReady ? "-" : dryRun ? "LOG ONLY" : focusCheck ? "STAR CITIZEN" : "ANY WINDOW";
        string why = !serverReady ? "" : dryRun ? "TEST MODE: NO KEYS SENT" : focusCheck ? "ONLY WHILE IT'S IN FRONT" : "FOCUS CHECK IS OFF";
        Tile(g, keys, "KEYS GO TO", target, !serverReady ? Hud.Dim : dryRun || !focusCheck ? Hud.Warn : Hud.Ink, why);

        Tile(g, presses, "PRESSES", pressCount.ToString(), Hud.Ink, lastPress != "" ? Lang.F("LAST: {0}", lastPress.ToUpperInvariant()) : "NONE YET");
        if (blockedCount > 0)
        {
            string text = Lang.F("{0} BLOCKED", blockedCount);
            float bw = Hud.SpacedWidth(g, text, Hud.Label(11), 1.2f);
            Hud.Spaced(g, text, Hud.Label(11), Hud.Warn, presses.Right - 10 - bw, presses.Y + 8, 1.2f);
        }
        // The last minute in 2-second bars.
        var graph = new RectangleF(presses.X + presses.Width * 0.45f, presses.Y + 24, presses.Width * 0.55f - 12, presses.Height - 34);
        var bars = new int[30];
        var now = DateTime.Now;
        foreach (var t in recentPresses)
        {
            int i = 29 - (int)((now - t).TotalSeconds / 2);
            if (i >= 0 && i < 30) bars[i]++;
        }
        int most = 3;
        foreach (int b in bars) most = Math.Max(most, b);
        float bar = graph.Width / 30;
        using (var brush = new SolidBrush(Hud.Fade(Hud.Accent, 200)))
            for (int i = 0; i < 30; i++)
                if (bars[i] > 0)
                {
                    float bh = Math.Max(2, graph.Height * bars[i] / most);
                    g.FillRectangle(brush, graph.X + i * bar + 1, graph.Bottom - bh, bar - 2, bh);
                }
        using (var pen = new Pen(Hud.Line, 1f)) g.DrawLine(pen, graph.X, graph.Bottom + 1, graph.Right, graph.Bottom + 1);
    }

    static void Tile(Graphics g, RectangleF r, string caption, string value, Color color, string note)
    {
        using (var fill = new SolidBrush(Hud.Fade(Hud.Back, 140))) g.FillRectangle(fill, r);
        using (var pen = new Pen(Hud.Line, 1f)) g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
        using (var pen = new Pen(Hud.Accent, 2f)) g.DrawLine(pen, r.X, r.Y + 6, r.X, r.Y + 20);
        Hud.Spaced(g, Lang.T(caption), Hud.Label(11), Hud.Dim, r.X + 10, r.Y + 8, 1.4f);
        value = Lang.T(value);
        var font = Hud.Head(value.Length > 6 ? 17 : 24);
        Hud.Text(g, value, font, color, new RectangleF(r.X + 10, r.Y + 21, r.Width - 16, 36));
        Hud.Text(g, Lang.T(note), Hud.Label(10.5f), Hud.Dim, new RectangleF(r.X + 10, r.Bottom - 18, r.Width - 16, 14));
    }

    void DrawVoice(Graphics g)
    {
        var r = voiceBox;
        float x = r.X + 16;
        Hud.PanelBox(g, r, "VOICE");
        Hud.Dot(g, voiceColor, x + 4, r.Y + 42);
        Hud.Text(g, voiceText(), Hud.Body(13), voiceColor, new RectangleF(x + 16, r.Y + 32, voiceListButton.Box.X - x - 26, 20));
        Hud.Spaced(g, Lang.T("STRICTNESS"), Hud.Label(11.5f), Hud.Dim, x, r.Y + 68, 1.6f);
        Hud.Spaced(g, strictness.Value + "%", Hud.Head(13), strictness.Enabled ? Hud.Ink : Hud.Dim, strictness.Box.Right + 14, r.Y + 67, 1f);
        Hud.Wrapped(g, Lang.T("Lower accepts more but may mishear. Higher is stricter."), Hud.Body(11), Hud.Dim,
            new RectangleF(strictness.Box.Right + 60, r.Y + 62, r.Right - strictness.Box.Right - 76, 32));
        Hud.Text(g, lastHeard(), Hud.Body(12.5f), lastHeardColor, new RectangleF(x, r.Bottom - 28, r.Width - 32, 18));
    }

    // ---------- server ----------

    void StartServer()
    {
        serverReady = false;
        fatalText = "";
        restartButton.Visible = false;
        copyButton.Enabled = false;
        link = "";
        addressLink.Visible = false;
        others = "";
        pairingCode = "-";
        qrRows = new string[0];
        lastHeard = () => "";
        strictness.Enabled = false;
        voiceCommands.Clear();
        voiceListButton.Enabled = false;
        if (commandsWindow != null) commandsWindow.Fill();
        SetState("STARTING", () => Lang.T("Starting the server..."), Hud.Dim);
        voiceText = () => Lang.T("Starting...");
        voiceColor = Hud.Dim;
        tabletCount = 0;
        string exe = Path.Combine(appDir, "TabletDeckServer.exe");
        if (!File.Exists(exe))
        {
            Stopped(() => Lang.F("{0} is missing. Reinstall Tablet Deck.", exe));
            return;
        }
        var psi = new ProcessStartInfo(exe, serverArgs)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = appDir,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };
        var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
        proc.OutputDataReceived += (s, e) => { if (e.Data != null) OnUi(delegate { OnLine(e.Data, false); }); };
        proc.ErrorDataReceived += (s, e) => { if (e.Data != null) OnUi(delegate { OnLine(e.Data, true); }); };
        proc.Exited += delegate { OnUi(delegate { OnServerExit(proc); }); };
        try
        {
            proc.Start();
        }
        catch (Exception ex)
        {
            Stopped(() => Lang.F("Couldn't start the server: {0}", ex.Message));
            return;
        }
        server = proc;
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();
    }

    void OnUi(MethodInvoker action)
    {
        if (IsDisposed || !IsHandleCreated) return;
        try { BeginInvoke(action); } catch (InvalidOperationException) { }
    }

    // Asks the server to release held keys and stop; ends it if it doesn't within 3 s.
    void StopServer()
    {
        var proc = server;
        server = null;
        if (proc == null || proc.HasExited) return;
        try
        {
            proc.StandardInput.WriteLine("quit");
            proc.StandardInput.Flush();
        }
        catch (Exception) { }
        if (!proc.WaitForExit(3000))
        {
            try { proc.Kill(); } catch (Exception) { }
        }
    }

    void SendToServer(string line)
    {
        var proc = server;
        if (proc == null || proc.HasExited) return;
        try
        {
            proc.StandardInput.WriteLine(line);
            proc.StandardInput.Flush();
        }
        catch (Exception) { }
    }

    void OnServerExit(Process proc)
    {
        if (quitting || proc != server) return;
        server = null;
        string fatal = fatalText;
        int code = proc.ExitCode;
        Stopped(() => fatal != "" ? fatal : Lang.F("The server stopped (exit code {0}). See the log below.", code));
        ShowFromTray();
    }

    void Stopped(Func<string> why)
    {
        serverReady = false;
        // The address means nothing now; the reason takes its place.
        link = "";
        addressLink.Visible = false;
        copyButton.Enabled = false;
        others = "";
        qrRows = new string[0];
        runningSince = DateTime.MinValue;
        SetState("STOPPED", () => Lang.F("Stopped: {0}", why()), Hud.Bad);
        restartButton.Visible = true;
        editButton.Visible = false;
        tabletCount = 0;
        voiceText = () => Lang.T("Off");
        voiceColor = Hud.Dim;
        ShowTrayText(() => Lang.T("Tablet Deck (stopped)"));
    }

    void OnLine(string line, bool error)
    {
        if (line.StartsWith("@ui "))
        {
            Dictionary<string, object> ev = null;
            try { ev = new JavaScriptSerializer().DeserializeObject(line.Substring(4)) as Dictionary<string, object>; }
            catch (Exception) { }
            if (ev != null) OnEvent(ev);
            return;
        }
        if (line.Trim() == "") return;
        AppendLog(line, error ? Hud.Warn : Hud.Ink);
    }

    void OnEvent(Dictionary<string, object> ev)
    {
        switch (Str(ev, "t"))
        {
            case "ready":
                serverReady = true;
                link = Str(ev, "link");
                editorUrl = "http://localhost:" + Str(ev, "port") + "/editor.html?t=" + Uri.EscapeDataString(Str(ev, "token"));
                editButton.Visible = true;
                settingsPath = Str(ev, "settings");
                addressLink.Text = link;
                addressLink.Visible = copyButton.Enabled = link != "";
                pairingCode = Str(ev, "token");
                var otherLinks = new List<string>();
                foreach (var o in List(ev, "others")) otherLinks.Add(Convert.ToString(o, CultureInfo.InvariantCulture));
                others = string.Join("  ", otherLinks.ToArray());
                var rows = new List<string>();
                foreach (var r in List(ev, "qr")) rows.Add(Convert.ToString(r, CultureInfo.InvariantCulture));
                qrRows = rows.ToArray();
                version = Str(ev, "version");
                Text = "Tablet Deck " + version;
                dryRun = Bool(ev, "dryRun");
                focusCheck = Bool(ev, "focusCheck");
                // A new pairing code or port restarts nothing, so the uptime keeps counting.
                if (runningSince == DateTime.MinValue) runningSince = DateTime.Now;
                string running = dryRun ? "Running in test mode (dry run): no keys are sent" : "Running";
                SetState(dryRun ? "TEST MODE" : "ONLINE", () => Lang.T(running), dryRun ? Hud.Warn : Hud.Good);
                string trayVersion = version;
                bool trayTest = dryRun;
                ShowTrayText(() => "Tablet Deck " + trayVersion + (trayTest ? Lang.T(" (test mode)") : ""));
                ShowStrictness(Num(ev, "voiceConfidence"));
                autoUpdate.On = Bool(ev, "autoUpdate");
                if (!checkedAtStart && autoUpdate.On) CheckForUpdates(false);
                checkedAtStart = true;
                break;
            case "voiceConfidence":
                ShowStrictness(Num(ev, "value"));
                break;
            case "voice-commands":
                voiceCommands.Clear();
                foreach (var pair in List(ev, "commands"))
                {
                    var p = pair as object[];
                    if (p != null && p.Length == 2) voiceCommands.Add(new[] { Convert.ToString(p[0], CultureInfo.InvariantCulture), Convert.ToString(p[1], CultureInfo.InvariantCulture) });
                }
                voiceListButton.Enabled = voiceCommands.Count > 0;
                if (commandsWindow != null) commandsWindow.Fill();
                break;
            case "heard":
                string result = Str(ev, "result"), heard = Str(ev, "text"), pressed = Str(ev, "label");
                int sure = (int)Math.Round(Num(ev, "confidence") * 100), below = strictness.Value;
                if (result == "accepted")
                {
                    lastHeard = () => Lang.F("Last heard: \"{0}\", {1}% sure. Pressed {2}.", heard, sure, pressed);
                    lastHeardColor = Hud.Good;
                }
                else if (result == "ignored")
                {
                    lastHeard = () => Lang.F("Last heard: \"{0}\", {1}% sure. Below {2}%, so it was ignored.", heard, sure, below);
                    lastHeardColor = Hud.Warn;
                }
                else
                {
                    lastHeard = () => Lang.T("Last heard: something that isn't one of the commands.");
                    lastHeardColor = Hud.Dim;
                }
                break;
            case "clients":
                int count;
                if (int.TryParse(Str(ev, "count"), out count)) tabletCount = count;
                break;
            case "press":
                if (Bool(ev, "blocked")) blockedCount++;
                else
                {
                    pressCount++;
                    recentPresses.Enqueue(DateTime.Now);
                    lastPress = Str(ev, "label");
                }
                break;
            case "voice":
                string s = Str(ev, "state"), phrases = Str(ev, "phrases"), language = Str(ev, "language"), ptt = Str(ev, "ptt"), why = Str(ev, "text");
                if (s == "ready")
                {
                    voiceText = () => Lang.F("Ready: {0} phrases, {1}", phrases, language) + (ptt != "" ? Lang.F(", push-to-talk: {0}", ptt) : "");
                    voiceColor = Hud.Good;
                }
                else if (s == "off")
                {
                    voiceText = () => Lang.T("Turned off in settings");
                    voiceColor = Hud.Dim;
                }
                else if (s == "starting")
                {
                    voiceText = () => Lang.T("Restarting...");
                    voiceColor = Hud.Dim;
                }
                else
                {
                    voiceText = () => Lang.F("Unavailable: {0}", why);
                    voiceColor = Hud.Warn;
                }
                break;
            case "network":
                publicNetwork = Str(ev, "category") == "Public" ? Str(ev, "adapter") : "";
                networkLink.Visible = publicNetwork != "";
                if (publicNetwork != "" && !publicNetworkNoticed)
                {
                    publicNetworkNoticed = true;
                    AppendLog(Lang.F("Windows treats \"{0}\" as a Public network, so tablets can't connect. In Windows: Settings > Network & Internet > {0} > set it to Private.", publicNetwork), Hud.Warn);
                    if (!Visible) Notify(Lang.T("Tablets can't connect"), Lang.F("Windows treats \"{0}\" as a Public network. Open Tablet Deck to fix it.", publicNetwork));
                }
                break;
            case "fatal":
                fatalText = Str(ev, "text");
                break;
        }
        hud.Redraw();
    }

    static string Str(Dictionary<string, object> d, string key)
    {
        object v;
        return d.TryGetValue(key, out v) && v != null ? Convert.ToString(v, CultureInfo.InvariantCulture) : "";
    }

    static bool Bool(Dictionary<string, object> d, string key)
    {
        object v;
        return d.TryGetValue(key, out v) && v is bool && (bool)v;
    }

    static double Num(Dictionary<string, object> d, string key)
    {
        object v;
        if (!d.TryGetValue(key, out v) || v == null) return 0;
        try { return Convert.ToDouble(v, CultureInfo.InvariantCulture); }
        catch (Exception) { return 0; }
    }

    static object[] List(Dictionary<string, object> d, string key)
    {
        object v;
        return d.TryGetValue(key, out v) && v is object[] ? (object[])v : new object[0];
    }

    // Moves the slider to the server's value without sending it back.
    void ShowStrictness(double value)
    {
        int percent = Math.Max(strictness.Min, Math.Min(strictness.Max, (int)Math.Round(value * 100)));
        strictness.Value = percent;
        strictness.Enabled = true;
        hud.Redraw();
    }

    void ShowVoiceCommands()
    {
        if (commandsWindow == null)
        {
            commandsWindow = new VoiceCommandsWindow(voiceCommands, Icon);
            commandsWindow.FormClosed += delegate { commandsWindow = null; };
            commandsWindow.Fill();
            commandsWindow.Show(this);
        }
        else commandsWindow.Activate();
    }

    // The header's one-word status, and the sentence shown while the server isn't running.
    void SetState(string word, Func<string> text, Color color)
    {
        chip = word;
        stateText = text;
        stateColor = color;
        hud.Redraw();
    }

    void AppendLog(string line, Color color)
    {
        // Keep the last 500 lines.
        if (log.Lines.Length > 500)
        {
            log.ReadOnly = false;
            log.Select(0, log.GetFirstCharIndexFromLine(100));
            log.SelectedText = "";
            log.ReadOnly = true;
        }
        log.SelectionStart = log.TextLength;
        log.SelectionLength = 0;
        log.SelectionColor = color;
        log.AppendText(line + "\n");
        log.ScrollToCaret();
    }

    // ---------- updates ----------

    // Shows the banner above the log. Buttons: "update" (What's new, Update now, Later),
    // "news" (What's new, Close), "busy" (none) or "info" (Close).
    void ShowBanner(Func<string> text, string buttons)
    {
        bannerText = text;
        bannerWhatsNew.Visible = buttons == "update" || buttons == "news";
        bannerUpdate.Visible = buttons == "update";
        bannerLater.Visible = buttons != "busy";
        bannerLater.Text = buttons == "update" ? "Later" : "Close";
        if (!bannerShown && WindowState == FormWindowState.Normal) Height += (int)(50 * hud.Zoom);
        bannerShown = true;
        Arrange();
    }

    void SetBannerText(Func<string> text)
    {
        bannerText = text;
        hud.Redraw();
    }

    void HideBanner()
    {
        if (bannerShown && WindowState == FormWindowState.Normal) Height = Math.Max(MinimumSize.Height, Height - (int)(50 * hud.Zoom));
        bannerShown = false;
        foreach (var b in new[] { bannerWhatsNew, bannerUpdate, bannerLater }) b.Visible = false;
        Arrange();
    }

    static WebClient NewWebClient()
    {
        var web = new WebClient { Encoding = Encoding.UTF8 };
        web.Headers[HttpRequestHeader.UserAgent] = "TabletDeck/" + CurrentVersion; // GitHub requires one
        return web;
    }

    // "1.2.0" or "v1.2.0-beta" -> 1.2.0; null if it isn't a version.
    static Version ParseVersion(string text)
    {
        string v = (text ?? "").Trim().TrimStart('v', 'V');
        int cut = v.IndexOfAny(new[] { '-', '+', ' ' });
        if (cut >= 0) v = v.Substring(0, cut);
        Version result;
        return Version.TryParse(v, out result) ? result : null;
    }

    // Asks GitHub for the latest release in the background. userAsked: report "up to date"
    // and errors too, and show a version the player dismissed with Later.
    void CheckForUpdates(bool userAsked)
    {
        if (checking || installer != null) return;
        checking = true;
        ThreadPool.QueueUserWorkItem(delegate
        {
            Dictionary<string, object> release = null;
            string error = null;
            try
            {
                using (var web = NewWebClient())
                {
                    web.Headers[HttpRequestHeader.Accept] = "application/vnd.github+json";
                    release = new JavaScriptSerializer().DeserializeObject(web.DownloadString(LatestReleaseApi)) as Dictionary<string, object>;
                }
            }
            catch (Exception ex) { error = ex.Message; }
            OnUi(delegate { checking = false; OnLatestRelease(release, error, userAsked); });
        });
    }

    void OnLatestRelease(Dictionary<string, object> release, string error, bool userAsked)
    {
        if (release == null)
        {
            if (userAsked) Notify(Lang.T("Couldn't check for updates"), error ?? Lang.T("GitHub didn't answer."));
            return;
        }
        string latest = Str(release, "tag_name").TrimStart('v', 'V');
        Version remote = ParseVersion(latest), local = ParseVersion(CurrentVersion);
        if (remote == null || local == null || remote <= local)
        {
            if (userAsked) Notify(Lang.T("Tablet Deck is up to date"), Lang.F("You have version {0}, the latest.", CurrentVersion));
            return;
        }
        // The installer and its checksum, attached to the release by .github/workflows/release.yml.
        string setupName = "TabletDeck-Setup-" + latest + ".exe";
        string setupUrl = "", shaUrl = "";
        foreach (var a in List(release, "assets"))
        {
            var asset = a as Dictionary<string, object>;
            if (asset == null) continue;
            string url = Str(asset, "browser_download_url");
            if (!url.StartsWith(DownloadPrefix, StringComparison.Ordinal)) continue;
            if (Str(asset, "name") == setupName) setupUrl = url;
            else if (Str(asset, "name") == setupName + ".sha256") shaUrl = url;
        }
        if (setupUrl == "" || shaUrl == "")
        {
            if (userAsked) Notify(Lang.T("Update not ready yet"), Lang.F("Version {0} is out, but its installer isn't attached yet. Try again in a few minutes.", latest));
            return;
        }
        if (!userAsked && latest == dismissedVersion) return;
        updateVersion = latest;
        updateSetupUrl = setupUrl;
        updateShaUrl = shaUrl;
        whatsNewUrl = Str(release, "html_url") != "" ? Str(release, "html_url") : ReleasesPage;
        ShowBanner(() => Lang.F("Tablet Deck {0} is available (you have {1}).", latest, CurrentVersion), "update");
        if (!Visible) Notify(Lang.F("Tablet Deck {0} is available", latest), Lang.T("Open Tablet Deck and click Update now."));
    }

    void Notify(string title, string text)
    {
        tray.ShowBalloonTip(5000, title, text, ToolTipIcon.Info);
    }

    void ShowTrayText(Func<string> text)
    {
        trayText = text;
        tray.Text = text();
    }

    void BuildTrayMenu()
    {
        var old = tray.ContextMenuStrip;
        var menu = new ContextMenuStrip();
        menu.Items.Add(Lang.T("Show Tablet Deck"), null, delegate { ShowFromTray(); });
        menu.Items.Add(Lang.T("Copy link"), null, delegate { CopyLink(); });
        menu.Items.Add(Lang.T("Edit layout"), null, delegate { OpenEditor(); });
        menu.Items.Add(Lang.T("Check for updates now"), null, delegate { CheckForUpdates(true); });
        menu.Items.Add(Lang.T("Feedback & ideas"), null, delegate { Process.Start(FeedbackPage); });
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(Lang.T("Quit"), null, delegate { Close(); });
        tray.ContextMenuStrip = menu;
        if (old != null) old.Dispose();
    }

    // After the language changes in Settings. The panels translate as they're drawn; the
    // tray menu and the voice commands window are built with their text, so build them again.
    void ApplyLanguage()
    {
        BuildTrayMenu();
        tray.Text = trayText();
        if (commandsWindow != null) commandsWindow.Close();
        hud.Redraw();
    }

    // Downloads the installer for updateVersion, then VerifyAndInstall.
    void InstallUpdate()
    {
        if (updateSetupUrl == "") return;
        string downloading = updateVersion;
        ShowBanner(() => Lang.F("Downloading Tablet Deck {0}...", downloading), "busy");
        string dir = UpdateFolder;
        string file = Path.Combine(dir, "TabletDeck-Setup-" + updateVersion + ".exe");
        try { Directory.CreateDirectory(dir); }
        catch (Exception ex) { UpdateFailed(() => ex.Message); return; }
        var web = NewWebClient();
        web.DownloadProgressChanged += (s, e) =>
        {
            int percent = e.ProgressPercentage;
            SetBannerText(() => Lang.F("Downloading Tablet Deck {0}...", downloading) + " " + percent + "%");
        };
        web.DownloadFileCompleted += (s, e) =>
        {
            web.Dispose();
            if (e.Error != null) { string error = e.Error.Message; UpdateFailed(() => Lang.F("the download failed ({0})", error)); }
            else VerifyAndInstall(file);
        };
        web.DownloadFileAsync(new Uri(updateSetupUrl), file);
    }

    // Checks the download against the release's SHA-256 file, then runs the installer
    // quietly. It asks this window to quit before it replaces the files, and starts
    // Tablet Deck again afterwards. Until then the deck keeps working.
    void VerifyAndInstall(string file)
    {
        SetBannerText(() => Lang.T("Checking the download..."));
        try
        {
            string expected;
            using (var web = NewWebClient())
                expected = web.DownloadString(updateShaUrl).Trim().Split(' ')[0].ToLowerInvariant();
            string actual;
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(file))
                actual = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
            if (expected.Length != 64 || actual != expected)
            {
                File.Delete(file);
                UpdateFailed(() => Lang.T("the download didn't match its checksum, so it wasn't installed"));
                return;
            }
        }
        catch (Exception ex)
        {
            UpdateFailed(() => Lang.F("couldn't check the download ({0})", ex.Message));
            return;
        }
        string installing = updateVersion;
        SetBannerText(() => Lang.F("Installing Tablet Deck {0}. Allow the Windows prompt to continue.", installing));
        try
        {
            installer = Process.Start(new ProcessStartInfo(file, "/SILENT /SUPPRESSMSGBOXES /NORESTART /RELAUNCH=1") { UseShellExecute = true });
        }
        catch (Win32Exception ex)
        {
            UpdateFailed(() => ex.NativeErrorCode == 1223 ? Lang.T("the Windows permission prompt was cancelled") : ex.Message);
            return;
        }
        // Normally the installer closes this window. If it ends first, the update didn't happen.
        installer.EnableRaisingEvents = true;
        installer.Exited += delegate
        {
            OnUi(delegate
            {
                int code = installer.ExitCode;
                installer = null;
                UpdateFailed(() => code == 0 ? Lang.T("the installer finished without updating") : Lang.F("the installer stopped (code {0}). Was the Windows prompt cancelled?", code));
            });
        };
    }

    void UpdateFailed(Func<string> why)
    {
        Func<string> text = () =>
        {
            string reason = why();
            return Lang.F("Update failed: {0}", reason) + (reason.EndsWith("?") || reason.EndsWith(".") ? "" : ".");
        };
        ShowBanner(text, "info");
        AppendLog(text(), Hud.Warn);
    }

    // Where Update now saves the installer; emptied when the window next starts.
    static readonly string UpdateFolder = Path.Combine(Path.GetTempPath(), "TabletDeck-Update");

    // After an update, offers the new version's notes once and removes the used installer.
    void ShowWhatsNewAfterUpdate()
    {
        try { if (Directory.Exists(UpdateFolder)) Directory.Delete(UpdateFolder, true); }
        catch (Exception) { }
        try
        {
            using (var key = Registry.CurrentUser.CreateSubKey(AppKey))
            {
                string last = key.GetValue("LastVersion") as string;
                if (last != null && last != CurrentVersion)
                {
                    whatsNewUrl = ReleasesPage + "/tag/v" + CurrentVersion;
                    ShowBanner(() => Lang.F("Updated to Tablet Deck {0}.", CurrentVersion), "news");
                }
                key.SetValue("LastVersion", CurrentVersion);
            }
        }
        catch (Exception) { }
    }

    void OpenEditor()
    {
        if (editorUrl == "" || !serverReady)
        {
            Notify(Lang.T("Tablet Deck isn't running"), Lang.T("Start it first; the layout editor is part of the server."));
            return;
        }
        Process.Start(editorUrl);
    }

    // ---------- settings ----------

    string ConfigFile()
    {
        return settingsPath != "" ? settingsPath
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TabletDeck", "config.json");
    }

    // The settings dialog. It starts from config.json; changes go to the running server
    // ("set name value" on stdin), or, while it's stopped, through TabletDeckServer.exe --set.
    void OpenSettings()
    {
        Dictionary<string, object> current = null;
        try { current = new JavaScriptSerializer().DeserializeObject(File.ReadAllText(ConfigFile())) as Dictionary<string, object>; }
        catch (Exception) { }
        using (var dialog = new SettingsWindow(current ?? new Dictionary<string, object>(), serverArgs.Contains("--port"), Icon,
            Path.Combine(appDir, "keybinds", "TabletDeck-StarCitizen.xml")))
        {
            var result = dialog.ShowDialog(this);
            if (result == DialogResult.OK && dialog.LanguageChanged) ApplyLanguage();
            if (result != DialogResult.OK || dialog.Changes.Count == 0) return;
            if (serverReady && server != null && !server.HasExited)
            {
                foreach (var change in dialog.Changes) SendToServer("set " + change.Key + " " + change.Value);
                if (dialog.NeedsRestart)
                {
                    AppendLog(Lang.T("Restarting the server for the new settings..."), Hud.Dim);
                    StopServer();
                    StartServer();
                }
                return;
            }
            var pairs = new List<string>();
            // Quoted, since some values have spaces (e.g. "pttKeyName=Numpad 5"); none have quotes.
            foreach (var change in dialog.Changes) pairs.Add("\"" + change.Key + "=" + change.Value.Replace("\"", "\\\"") + "\"");
            try
            {
                var psi = new ProcessStartInfo(Path.Combine(appDir, "TabletDeckServer.exe"), "--set " + string.Join(" ", pairs.ToArray()))
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    StandardErrorEncoding = Encoding.UTF8,
                };
                using (var proc = Process.Start(psi))
                {
                    string errors = proc.StandardError.ReadToEnd();
                    proc.WaitForExit(10000);
                    if (errors.Trim() != "") AppendLog(errors.Trim(), Hud.Warn);
                }
            }
            catch (Exception ex)
            {
                AppendLog(Lang.F("Couldn't save the settings: {0}", ex.Message), Hud.Warn);
                return;
            }
            if (server == null) StartServer();
        }
    }

    // ---------- buttons ----------

    void CopyLink()
    {
        if (link == "") return;
        try
        {
            Clipboard.SetText(link);
            copyButton.Text = "Copied!";
            hud.Redraw();
            copiedTimer.Stop();
            copiedTimer.Start();
        }
        catch (ExternalException) { }
    }

    void OpenSettingsFolder()
    {
        string dir = settingsPath != "" ? Path.GetDirectoryName(settingsPath)
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TabletDeck");
        Directory.CreateDirectory(dir);
        Process.Start("explorer.exe", "\"" + dir + "\"");
    }

    static bool RunsAtStartup()
    {
        using (var key = Registry.CurrentUser.OpenSubKey(RunKey))
            return key != null && key.GetValue(RunValue) != null;
    }

    static void SetRunAtStartup(bool on)
    {
        using (var key = Registry.CurrentUser.CreateSubKey(RunKey))
        {
            if (on) key.SetValue(RunValue, "\"" + Application.ExecutablePath + "\" --minimized");
            else key.DeleteValue(RunValue, false);
        }
    }

    // ---------- tray and window ----------

    void ShowFromTray()
    {
        Show();
        if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
        Activate();
    }

    // "Start with Windows" opens straight into the tray.
    protected override void SetVisibleCore(bool value)
    {
        if (startHidden)
        {
            startHidden = false;
            value = false;
        }
        base.SetVisibleCore(value);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (WindowState != FormWindowState.Minimized) return;
        Hide();
        if (!trayHintShown)
        {
            trayHintShown = true;
            tray.ShowBalloonTip(3000, Lang.T("Tablet Deck is still running"), Lang.T("Double-click the tray icon to open it, or right-click it to quit."), ToolTipIcon.Info);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Closing the window quits: the server releases held keys and stops.
        quitting = true;
        StopServer();
        tray.Visible = false;
        base.OnFormClosing(e);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == Program.ShowWindowMessage) ShowFromTray();
        else if (m.Msg == Program.QuitMessage) BeginInvoke(new MethodInvoker(Close));
        base.WndProc(ref m);
    }
}

// Everything the voice listener understands, with a search box: what to say and which
// button it presses. The list comes from the server, so it matches the current layout.
class VoiceCommandsWindow : HudDialog
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

    readonly List<string[]> commands;
    readonly PanelPart panel;
    readonly Note note, presses;
    readonly TextBox search = new TextBox();
    readonly ListView list = new ListView();

    public VoiceCommandsWindow(List<string[]> commands, Icon icon) : base("Voice commands", icon, 540, 580)
    {
        this.commands = commands;
        MinimumSize = SizeFromClientSize(new Size((int)(440 * hud.Zoom), (int)(340 * hud.Zoom)));
        StartPosition = FormStartPosition.CenterScreen;

        panel = Panel("COMMANDS", 16, 16, 508, 548);
        note = Hint("Hold the mic button on the tablet and say one of these. Most Star Citizen controls are toggles, " +
            "so \"engines on\" and \"engines off\" press the same key.", 32, 44, 476, 36);
        Field(search, 32, 86, 476, 28);
        search.TextChanged += delegate { Fill(); };
        search.HandleCreated += delegate { SendMessage(search.Handle, 0x1501, (IntPtr)1, Lang.T("Search, e.g. shields or quantum")); }; // EM_SETCUEBANNER
        Caption("Say", 38, 126);
        presses = Caption("Presses", 300, 126);
        Field(list, 32, 144, 476, 404);
        list.View = View.Details;
        list.FullRowSelect = true;
        list.HeaderStyle = ColumnHeaderStyle.None; // the captions above are the headings
        list.Columns.Add("Say", 262);
        list.Columns.Add("Presses", 200);
        ResumeLayout(false);
        Cancel = Close;
    }

    protected override void Arrange()
    {
        var area = hud.Area;
        float w = area.Width - 32, h = area.Height - 32;
        panel.Box = new RectangleF(16, 16, w, h);
        note.Box = new RectangleF(32, 44, w - 32, 36);
        Frame(search, new RectangleF(32, 86, w - 32, 28));
        presses.Box.X = 38 + (w - 44) * 0.55f;
        Frame(list, new RectangleF(32, 144, w - 32, h - 144));
        // Column widths aren't scaled with the window, so set them for its size and the screen.
        float inner = (w - 32 - 26) * hud.Zoom;
        if (list.Columns.Count == 2)
        {
            list.Columns[0].Width = (int)(inner * 0.55f);
            list.Columns[1].Width = (int)(inner * 0.45f);
        }
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        search.Focus();
    }

    // Shows the commands matching the search box, by phrase or button name.
    public void Fill()
    {
        string q = search.Text.Trim().ToLowerInvariant();
        list.BeginUpdate();
        list.Items.Clear();
        foreach (var c in commands)
            if (q == "" || c[0].Contains(q) || c[1].ToLowerInvariant().Contains(q))
                list.Items.Add(new ListViewItem(new[] { c[0], c[1] }));
        list.EndUpdate();
        Text = commands.Count == 0 ? Lang.T("Voice commands (not running)")
            : q == "" ? Lang.F("Voice commands ({0})", commands.Count)
            : Lang.F("Voice commands ({0} of {1})", list.Items.Count, commands.Count);
    }
}

// Copies the Tablet Deck Keyboard profile (keybinds\TabletDeck-StarCitizen.xml in the
// install folder) into Star Citizen's Controls\Mappings folder, for each copy of the game
// (LIVE, PTU and so on) the player ticks. Found by searching the drives, or picked by hand.
class KeybindProfileWindow : HudDialog
{
    static readonly string[] Channels = { "LIVE", "PTU", "EPTU", "HOTFIX", "TECH-PREVIEW" };
    const string ProfileName = "TabletDeck-StarCitizen.xml";

    readonly string profile;
    readonly List<string> games = new List<string>(); // ...\StarCitizen\<channel>, same order as list
    readonly CheckedListBox list = new CheckedListBox();
    readonly Note status;

    public KeybindProfileWindow(string profile, Icon icon) : base("Install keybind profile", icon, 560, 452)
    {
        this.profile = profile;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        Panel("STAR CITIZEN COPIES", 16, 16, 528, 248);
        var intro = Hint("Copies the Tablet Deck Keyboard profile into Star Citizen, so buttons for actions with no default key work. " +
            "Tick the copies of the game to install it into.", 32, 44, 496, 36);
        intro.Color = Hud.Ink;
        Field(list, 32, 86, 496, 126);
        list.CheckOnClick = true;
        list.HorizontalScrollbar = true;
        Button("Choose game folder...", 32, 222, 210, delegate { ChooseFolder(); });

        Panel("STATUS", 16, 276, 528, 116);
        status = Hint("", 32, 304, 496, 80);

        var install = Primary("Install", 348, 408, 96, delegate { Install(); });
        Button("Close", 448, 408, 96, delegate { Close(); });
        Accept = delegate { if (install.Enabled) Install(); };
        Cancel = Close;
        ResumeLayout(false);

        if (!File.Exists(profile))
        {
            ShowStatus(Lang.F("The profile file is missing ({0}). Reinstall Tablet Deck.", profile), Hud.Bad);
            install.Enabled = false;
            return;
        }
        foreach (var game in FindGames()) AddGame(game);
        ShowStatus(games.Count == 0
            ? Lang.T("Star Citizen wasn't found in the usual places. Click \"Choose game folder...\" and pick the StarCitizen folder (the one with LIVE in it).")
            : Lang.F(games.Count == 1 ? "Found {0} copy of the game. Missing one? Click \"Choose game folder...\"."
                : "Found {0} copies of the game. Missing one? Click \"Choose game folder...\".", games.Count), Hud.Dim);
    }

    void ShowStatus(string text, Color color)
    {
        status.Text = text;
        status.Color = color;
        Redraw();
    }

    // ...\StarCitizen\<channel> folders in the usual install places, and one or two folder
    // levels down on each fixed drive (e.g. D:\Games\RSI\StarCitizen).
    static List<string> FindGames()
    {
        var found = new List<string>();
        foreach (var drive in DriveInfo.GetDrives())
        {
            if (drive.DriveType != DriveType.Fixed || !drive.IsReady) continue;
            var places = new List<string> { drive.RootDirectory.FullName };
            foreach (var top in SubFolders(drive.RootDirectory.FullName))
            {
                if (Path.GetFileName(top).Equals("Windows", StringComparison.OrdinalIgnoreCase)) continue;
                places.Add(top);
                places.AddRange(SubFolders(top));
            }
            foreach (var place in places)
                foreach (var channel in Channels)
                {
                    string dir = Path.Combine(place, "StarCitizen", channel);
                    if (Directory.Exists(dir) && !found.Contains(dir)) found.Add(dir);
                }
        }
        return found;
    }

    static string[] SubFolders(string dir)
    {
        try { return Directory.GetDirectories(dir); }
        catch (Exception) { return new string[0]; } // no access, e.g. system folders
    }

    static string Target(string game)
    {
        return Path.Combine(game, "user", "client", "0", "Controls", "Mappings", ProfileName);
    }

    // Adds a game copy to the list, ticked unless it already has this exact profile.
    void AddGame(string game)
    {
        foreach (var g in games) if (string.Equals(g, game, StringComparison.OrdinalIgnoreCase)) return;
        string state;
        bool tick;
        string target = Target(game);
        if (!File.Exists(target)) { state = "not installed"; tick = true; }
        else if (SameFile(target, profile)) { state = "already up to date"; tick = false; }
        else { state = "older copy, will be replaced"; tick = true; }
        games.Add(game);
        list.Items.Add(Path.GetFileName(game) + "  -  " + game + "  (" + Lang.T(state) + ")", tick);
    }

    static bool SameFile(string a, string b)
    {
        try { return File.ReadAllText(a).Replace("\r\n", "\n") == File.ReadAllText(b).Replace("\r\n", "\n"); }
        catch (Exception) { return false; }
    }

    // Accepts the StarCitizen folder, a channel folder inside it, or the folder above it.
    void ChooseFolder()
    {
        using (var dialog = new FolderBrowserDialog { Description = Lang.T("Pick the StarCitizen folder (the one with LIVE in it)."), ShowNewFolderButton = false })
        {
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            string picked = dialog.SelectedPath;
            var candidates = new List<string>();
            if (Array.IndexOf(Channels, Path.GetFileName(picked).ToUpperInvariant()) >= 0) candidates.Add(picked);
            foreach (var root in new[] { picked, Path.Combine(picked, "StarCitizen") })
                foreach (var channel in Channels)
                    if (Directory.Exists(Path.Combine(root, channel))) candidates.Add(Path.Combine(root, channel));
            if (candidates.Count == 0)
            {
                ShowStatus(Lang.T("That folder doesn't look like Star Citizen. Pick the StarCitizen folder, the one with LIVE in it."), Hud.Warn);
                return;
            }
            foreach (var game in candidates) AddGame(game);
            ShowStatus(Lang.T("Added. Tick the copies of the game to install the profile into, then click Install."), Hud.Dim);
        }
    }

    void Install()
    {
        var done = new List<string>();
        var failed = new List<string>();
        for (int i = 0; i < games.Count; i++)
        {
            if (!list.GetItemChecked(i)) continue;
            try
            {
                string target = Target(games[i]);
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                File.Copy(profile, target, true);
                done.Add(Path.GetFileName(games[i]));
            }
            catch (Exception ex)
            {
                failed.Add(Path.GetFileName(games[i]) + ": " + ex.Message);
            }
        }
        if (done.Count == 0 && failed.Count == 0)
        {
            ShowStatus(Lang.T("Nothing ticked. Tick the copies of the game to install the profile into."), Hud.Warn);
            return;
        }
        var text = new StringBuilder();
        if (done.Count > 0)
        {
            text.Append(Lang.F("Installed into {0}. ", string.Join(", ", done.ToArray())));
            text.Append(Lang.T("Now, in Star Citizen: Options > Keybindings > Control Profiles, load \"Tablet Deck Keyboard\". "));
            text.Append(Lang.T("This replaces your current custom binds, so note any you want to keep and set them again afterwards."));
        }
        if (failed.Count > 0)
            text.Append((done.Count > 0 ? "\n\n" : "") + Lang.F("Couldn't copy into {0}. Try running Tablet Deck as administrator, or copy {1} by hand.",
                string.Join("; ", failed.ToArray()), profile));
        ShowStatus(text.ToString(), failed.Count > 0 ? Hud.Warn : Hud.Good);
        // Refresh the states shown in the list.
        var again = new List<string>(games);
        games.Clear();
        list.Items.Clear();
        foreach (var game in again) AddGame(game);
    }
}

// Settings that would otherwise mean editing config.json. Only what changed ends up in
// Changes, as setting name -> text value (see SETTINGS in server.js).
class SettingsWindow : HudDialog
{
    public readonly List<KeyValuePair<string, string>> Changes = new List<KeyValuePair<string, string>>();
    public bool NeedsRestart; // port or voice changes take a server restart

    readonly Dictionary<string, object> current;
    readonly TextBox code = new TextBox();
    readonly NumericUpDown port = new NumericUpDown();
    readonly HudView.Item focus;
    readonly NumericUpDown tap = new NumericUpDown();
    readonly HudView.Item voiceOn;
    readonly ComboBox language = new ComboBox();
    readonly ComboBox pttMode = new ComboBox();
    readonly TextBox links = new TextBox();
    readonly ComboBox windowLanguage = new ComboBox();
    public bool LanguageChanged; // saved already; the window shows it once Settings closes

    // config.json's links as editable lines.
    string LinksText()
    {
        object v;
        var lines = new List<string>();
        if (current.TryGetValue("links", out v) && v is object[])
            foreach (var item in (object[])v)
            {
                var l = item as Dictionary<string, object>;
                if (l == null) continue;
                lines.Add(Convert.ToString(l["name"], CultureInfo.InvariantCulture) + " | " + Convert.ToString(l["url"], CultureInfo.InvariantCulture));
            }
        return string.Join("\r\n", lines.ToArray());
    }

    // The lines as the JSON the server takes, or null (with a message) if one isn't valid.
    string LinksJson()
    {
        var list = new List<Dictionary<string, string>>();
        foreach (var raw in links.Lines)
        {
            string line = raw.Trim();
            if (line == "") continue;
            int bar = line.IndexOf('|');
            string name = bar > 0 ? line.Substring(0, bar).Trim() : "";
            string url = bar > 0 ? line.Substring(bar + 1).Trim() : "";
            if (name == "" || name.Length > 24 || name.IndexOfAny(new[] { '<', '>', '"' }) >= 0
                || !System.Text.RegularExpressions.Regex.IsMatch(url, "^https://[^\\s\"<>]{3,300}$"))
            {
                MessageBox.Show(this, Lang.F("This link isn't right:\n\n{0}\n\nWrite each one as: Name | https://address (names up to 24 characters).", line), "Tablet Deck", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
            list.Add(new Dictionary<string, string> { { "name", name }, { "url", url } });
        }
        if (list.Count > 12) { MessageBox.Show(this, Lang.T("Up to 12 links, please."), "Tablet Deck", MessageBoxButtons.OK, MessageBoxIcon.Warning); return null; }
        return new JavaScriptSerializer().Serialize(list);
    }
    readonly HudView.Item pttSound, shortEmotes, pttButton;
    int pttKey;          // Windows virtual-key code, 0 = none
    string pttName = "";
    bool capturing;      // waiting for the player to press the push-to-talk key

    // ---------- push-to-talk key ----------

    void StartCapture()
    {
        if (capturing) { capturing = false; ShowPttKey(); return; } // a second click cancels
        capturing = true;
        pttButton.Text = "Press a key or button...";
        Redraw();
    }

    // While capturing, every key (Tab, Enter and Alt included) is the push-to-talk key; Esc cancels.
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (!capturing) return base.ProcessCmdKey(ref msg, keyData);
        var key = keyData & Keys.KeyCode;
        if (key == Keys.Escape) { capturing = false; ShowPttKey(); }
        else SetPttKey((int)key);
        return true;
    }

    void SetPttKey(int vk)
    {
        capturing = false;
        pttKey = vk;
        pttName = vk == 0 ? "" : KeyName(vk);
        ShowPttKey();
    }

    void ShowPttKey()
    {
        pttButton.Text = pttKey == 0 ? "None: click to set" : pttName;
        Redraw();
    }

    static string KeyName(int vk)
    {
        if (vk == 4) return "Middle mouse";
        if (vk == 5) return "Mouse 4";
        if (vk == 6) return "Mouse 5";
        string name = ((Keys)vk).ToString();
        if (name.StartsWith("NumPad")) return "Numpad " + name.Substring(6);
        if (name.StartsWith("D") && name.Length == 2 && char.IsDigit(name[1])) return name.Substring(1);
        return System.Text.RegularExpressions.Regex.Replace(name, "[^A-Za-z0-9 +-]", "");
    }

    // Two columns of panels: pairing, keys and voice on the left, push-to-talk and links on
    // the right, and the buttons along the bottom.
    public SettingsWindow(Dictionary<string, object> current, bool portFromCommandLine, Icon icon, string keybindProfile)
        : base("Tablet Deck settings", icon, 800, 584)
    {
        this.current = current;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        Panel("PAIRING", 16, 16, 376, 184);
        Caption("Pairing code", 32, 48);
        Field(code, 32, 64, 186, 28);
        code.Font = new Font("Consolas", 11f);
        code.MaxLength = 32;
        code.Text = Get("token", "");
        Button("New random code", 228, 64, 148, delegate { code.Text = RandomCode(); });
        Hint("Letters and numbers. Tablets already paired keep working until they reload; new ones need the new code.", 32, 98, 344, 32);
        Caption("Port", 32, 136);
        Field(port, 32, 152, 90, 28);
        port.Minimum = 1024;
        port.Maximum = 65535;
        port.Value = Math.Max(1024, Math.Min(65535, GetNumber("port", 8765)));
        port.Enabled = !portFromCommandLine;
        Hint(portFromCommandLine ? "Set with --port when Tablet Deck was started, so it can't be changed here."
            : "Changing it restarts the server. Tablets then need the new address or QR code.", 134, 150, 242, 34);

        Panel("KEYS", 16, 212, 376, 132);
        focus = Switch("Only press keys while Star Citizen is in front", GetBool("onlyWhenGameFocused", true), 32, 244, 344);
        focus.Tip = "Keys never reach another window, like a browser or chat, by accident.";
        Caption("Tap length", 32, 278);
        Field(tap, 32, 294, 80, 28);
        tap.Minimum = 20;
        tap.Maximum = 1000;
        tap.Value = Math.Max(20, Math.Min(1000, GetNumber("tapMs", 60)));
        Hint("milliseconds a tap holds the key. Raise it if the game misses taps.", 124, 292, 252, 34);

        Panel("VOICE", 16, 356, 376, 168);
        voiceOn = Switch("Voice commands", GetBool("voice", true), 32, 388, 344);
        voiceOn.Click = delegate { language.Enabled = shortEmotes.Enabled = voiceOn.On; Redraw(); };
        Caption("Voice language", 32, 422);
        Field(language, 32, 438, 110, 28);
        language.DropDownStyle = ComboBoxStyle.DropDown;
        language.FlatStyle = FlatStyle.Flat;
        language.Items.AddRange(new object[] { "en-US", "en-GB", "en-AU", "en-CA", "en-IN" });
        language.Text = Get("voiceLanguage", "en-US");
        Hint("Needs that English speech recognizer installed in Windows (Settings > Time & Language > Speech).", 154, 434, 222, 48);
        shortEmotes = Switch("Emotes also answer to one word, like \"wave\"", GetBool("shortEmotes", false), 32, 490, 344);
        shortEmotes.Tip = "Without this, emotes need \"emote\" first (\"emote wave\"), so a stray word doesn't set one off.";
        language.Enabled = shortEmotes.Enabled = voiceOn.On;

        // Push-to-talk from a key or mouse button on the PC (see native/voice.cs).
        Panel("PUSH-TO-TALK", 408, 16, 376, 232);
        Caption("Key on this PC", 424, 48);
        pttKey = GetNumber("pttKey", 0);
        pttName = Get("pttKeyName", "");
        pttButton = Button("", 424, 64, 250, delegate { StartCapture(); });
        pttButton.OtherButton = (b) =>
        {
            if (!capturing) return;
            int vk = b == MouseButtons.Middle ? 4 : b == MouseButtons.XButton1 ? 5 : b == MouseButtons.XButton2 ? 6 : 0;
            if (vk != 0) SetPttKey(vk);
        };
        Button("None", 682, 64, 86, delegate { SetPttKey(0); });
        Caption("Mode", 424, 104);
        Field(pttMode, 424, 120, 344, 28);
        pttMode.DropDownStyle = ComboBoxStyle.DropDownList;
        pttMode.FlatStyle = FlatStyle.Flat;
        pttMode.Items.AddRange(new object[] { Lang.T("Hold to talk"), Lang.T("Press to start, again to stop") });
        pttMode.SelectedIndex = Get("pttMode", "hold") == "toggle" ? 1 : 0;
        pttSound = Switch("Play a sound when it starts and stops listening", GetBool("pttSound", true), 424, 160, 344);
        Hint("Pick a key Star Citizen doesn't use, like a mouse side button, a numpad key or F13-F24: the key still reaches the game.", 424, 192, 344, 48);
        ShowPttKey();

        // Websites the tablet's "Open on PC" buttons open here, one per line: "Name | https://...".
        Panel("LINKS", 408, 260, 376, 264);
        Caption("Websites the tablet can open on this PC", 424, 292);
        Field(links, 424, 308, 344, 152);
        links.Multiline = true;
        links.ScrollBars = ScrollBars.Vertical;
        links.Font = new Font("Consolas", 9.5f);
        links.Text = LinksText();
        Hint("One per line: Name | https://address. The tablet only sees the names, so it can't open anything else.", 424, 468, 344, 48);

        Button("Install keybind profile...", 16, 540, 250, delegate
        {
            using (var dialog = new KeybindProfileWindow(keybindProfile, Icon)) dialog.ShowDialog(this);
        });
        // The window's own language; the tablet picks its language in Deck settings.
        Caption("Language", 282, 548);
        Field(windowLanguage, 372, 540, 180, 28);
        windowLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
        windowLanguage.FlatStyle = FlatStyle.Flat;
        foreach (var choice in Lang.Choices) windowLanguage.Items.Add(choice[0] == "" ? Lang.T(choice[1]) : choice[1]);
        string saved = Lang.Saved;
        for (int i = 0; i < Lang.Choices.Length; i++) if (Lang.Choices[i][0] == saved) windowLanguage.SelectedIndex = i;
        if (windowLanguage.SelectedIndex < 0) windowLanguage.SelectedIndex = 0;

        Primary("Save", 584, 540, 96, delegate { Save(); });
        Button("Cancel", 688, 540, 96, delegate { DialogResult = DialogResult.Cancel; });
        Accept = Save;
        Cancel = delegate { DialogResult = DialogResult.Cancel; };
        ResumeLayout(false);
    }

    string Get(string key, string fallback)
    {
        object v;
        return current.TryGetValue(key, out v) && v != null ? Convert.ToString(v, CultureInfo.InvariantCulture) : fallback;
    }

    int GetNumber(string key, int fallback)
    {
        int n;
        return int.TryParse(Get(key, ""), out n) ? n : fallback;
    }

    bool GetBool(string key, bool fallback)
    {
        object v;
        return current.TryGetValue(key, out v) && v is bool ? (bool)v : fallback;
    }

    static string RandomCode()
    {
        var bytes = new byte[4];
        using (var rng = new RNGCryptoServiceProvider()) rng.GetBytes(bytes);
        return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }

    // Checks the fields, records what changed, and closes.
    void Save()
    {
        string newCode = code.Text.Trim().ToLowerInvariant();
        if (!System.Text.RegularExpressions.Regex.IsMatch(newCode, "^[a-z0-9]{1,32}$"))
        {
            MessageBox.Show(this, Lang.T("The pairing code can only have letters and numbers (up to 32)."), "Tablet Deck", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            code.Focus();
            return;
        }
        string newLanguage = language.Text.Trim();
        if (!System.Text.RegularExpressions.Regex.IsMatch(newLanguage, "^[a-z]{2}(-[A-Za-z]{2,4})?$"))
        {
            MessageBox.Show(this, Lang.T("The voice language should look like en-US."), "Tablet Deck", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            language.Focus();
            return;
        }
        Change("token", Get("token", "").Trim().ToLowerInvariant(), newCode, false);
        if (port.Enabled) Change("port", GetNumber("port", 8765).ToString(), ((int)port.Value).ToString(), true);
        Change("onlyWhenGameFocused", OnOff(GetBool("onlyWhenGameFocused", true)), OnOff(focus.On), false);
        Change("tapMs", GetNumber("tapMs", 60).ToString(), ((int)tap.Value).ToString(), false);
        Change("voice", OnOff(GetBool("voice", true)), OnOff(voiceOn.On), true);
        Change("voiceLanguage", Get("voiceLanguage", "en-US"), newLanguage, true);
        if (GetNumber("pttKey", 0) != pttKey)
        {
            Change("pttKey", GetNumber("pttKey", 0).ToString(), pttKey.ToString(), false);
            if (pttKey != 0) Change("pttKeyName", "", pttName == "" ? "Key" : pttName, false);
        }
        Change("pttMode", Get("pttMode", "hold"), pttMode.SelectedIndex == 1 ? "toggle" : "hold", false);
        Change("pttSound", OnOff(GetBool("pttSound", true)), OnOff(pttSound.On), false);
        Change("shortEmotes", OnOff(GetBool("shortEmotes", false)), OnOff(shortEmotes.On), false);
        if (links.Text.Trim() != LinksText().Trim())
        {
            string json = LinksJson();
            if (json == null) { Changes.Clear(); NeedsRestart = false; links.Focus(); return; }
            Change("links", "", json, false);
        }
        string chosen = Lang.Choices[Math.Max(0, windowLanguage.SelectedIndex)][0];
        if (chosen != Lang.Saved)
        {
            Lang.Save(chosen);
            LanguageChanged = true;
        }
        DialogResult = DialogResult.OK;
    }

    void Change(string name, string before, string after, bool restart)
    {
        if (before == after) return;
        Changes.Add(new KeyValuePair<string, string>(name, after));
        if (restart) NeedsRestart = true;
    }

    static string OnOff(bool on)
    {
        return on ? "on" : "off";
    }
}
