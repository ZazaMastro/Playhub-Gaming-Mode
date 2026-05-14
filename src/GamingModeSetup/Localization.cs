using System.Globalization;

namespace GamingModeSetup;

public static class L
{
    public static readonly (string Code, string NativeName, string EnglishName)[] SupportedLanguages =
    [
        ("it", "Italiano", "Italian"),
        ("en", "English", "English"),
        ("es", "Español", "Spanish"),
        ("fr", "Français", "French"),
        ("de", "Deutsch", "German"),
        ("pt", "Português", "Portuguese")
    ];

    public static string CurrentLanguage { get; private set; } = "en";

    private static readonly Dictionary<string, Dictionary<string, string>> Strings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["en"] = new()
        {
            ["language.title"] = "Choose your language",
            ["setup.title"] = "Gaming Mode Setup",
            ["notice.title"] = "Decky plugin required",
            ["notice.body"] = "Install the Gaming Mode plugin through Decky Loader before running this setup. The Windows installer configures only the companion app, local agent, shortcuts, and startup mode.",
            ["notice.note"] = "If the plugin is missing, Steam controls will not be available.",
            ["next"] = "Next",
            ["back"] = "Back",
            ["options"] = "Install options",
            ["default.startup"] = "Default startup",
            ["default.startup.desc"] = "Sets the default shell at startup.",
            ["desktop.mode"] = "Desktop Mode",
            ["gaming.mode"] = "Gaming Mode",
            ["cursor.autohide"] = "Hide idle mouse pointer",
            ["cursor.autohide.desc"] = "Hides the pointer after a short idle delay.",
            ["desktop.shortcut"] = "Add a desktop shortcut",
            ["desktop.shortcut.desc"] = "Creates a desktop shortcut.",
            ["launch.after"] = "Open Gaming Mode after installation",
            ["launch.after.desc"] = "Launches the companion app after setup.",
            ["install"] = "Install",
            ["update"] = "Update",
            ["uninstall"] = "Uninstall",
            ["close"] = "Close",
            ["installing"] = "Installing...",
            ["uninstalling"] = "Uninstalling...",
            ["ready"] = "Ready to install.",
            ["installed"] = "Gaming Mode is installed.",
            ["remove.question"] = "Remove Gaming Mode from this PC?",
            ["safety"] = "Hold Shift at sign-in to force Desktop Mode.",
            ["location"] = "Install location"
        },
        ["it"] = new()
        {
            ["language.title"] = "Scegli la lingua",
            ["setup.title"] = "Setup Gaming Mode",
            ["notice.title"] = "Plugin Decky richiesto",
            ["notice.body"] = "Installa il plugin Gaming Mode da Decky Loader prima di eseguire questo setup. L'installer Windows configura solo companion app, agent locale, collegamenti e modalità di avvio.",
            ["notice.note"] = "Se il plugin non è installato, i controlli da Steam non saranno disponibili.",
            ["next"] = "Avanti",
            ["back"] = "Indietro",
            ["options"] = "Opzioni di installazione",
            ["default.startup"] = "Avvio predefinito",
            ["default.startup.desc"] = "Imposta la shell predefinita all'avvio.",
            ["desktop.mode"] = "Modalità Desktop",
            ["gaming.mode"] = "Modalità Gaming",
            ["cursor.autohide"] = "Nascondi il puntatore inattivo",
            ["cursor.autohide.desc"] = "Nasconde il puntatore dopo un breve periodo di inattività.",
            ["desktop.shortcut"] = "Aggiungi un collegamento sul desktop",
            ["desktop.shortcut.desc"] = "Crea un collegamento sul desktop.",
            ["launch.after"] = "Apri Gaming Mode a fine installazione",
            ["launch.after.desc"] = "Avvia la companion app al termine del setup.",
            ["install"] = "Installa",
            ["update"] = "Aggiorna",
            ["uninstall"] = "Disinstalla",
            ["close"] = "Chiudi",
            ["installing"] = "Installazione...",
            ["uninstalling"] = "Disinstallazione...",
            ["ready"] = "Pronto per l'installazione.",
            ["installed"] = "Gaming Mode è installato.",
            ["remove.question"] = "Rimuovere Gaming Mode da questo PC?",
            ["safety"] = "Tieni premuto Shift all'accesso per forzare la Modalità Desktop.",
            ["location"] = "Percorso installazione"
        },
        ["es"] = new()
        {
            ["language.title"] = "Elige el idioma",
            ["setup.title"] = "Instalador de Gaming Mode",
            ["notice.title"] = "Plugin Decky requerido",
            ["notice.body"] = "Instala el plugin Gaming Mode desde Decky Loader antes de ejecutar este instalador. El instalador de Windows solo configura la app complementaria, el agente local, los accesos directos y el modo de inicio.",
            ["notice.note"] = "Si falta el plugin, los controles desde Steam no estarán disponibles.",
            ["next"] = "Siguiente",
            ["back"] = "Atrás",
            ["options"] = "Opciones de instalación",
            ["default.startup"] = "Inicio predeterminado",
            ["default.startup.desc"] = "Define la shell predeterminada al iniciar.",
            ["desktop.mode"] = "Modo Escritorio",
            ["gaming.mode"] = "Modo Gaming",
            ["cursor.autohide"] = "Ocultar puntero inactivo",
            ["cursor.autohide.desc"] = "Oculta el puntero tras un breve periodo de inactividad.",
            ["desktop.shortcut"] = "Añadir acceso directo en el escritorio",
            ["desktop.shortcut.desc"] = "Crea un acceso directo en el escritorio.",
            ["launch.after"] = "Abrir Gaming Mode al terminar",
            ["launch.after.desc"] = "Inicia la app complementaria al finalizar.",
            ["install"] = "Instalar",
            ["update"] = "Actualizar",
            ["uninstall"] = "Desinstalar",
            ["close"] = "Cerrar",
            ["installing"] = "Instalando...",
            ["uninstalling"] = "Desinstalando...",
            ["ready"] = "Listo para instalar.",
            ["installed"] = "Gaming Mode está instalado.",
            ["remove.question"] = "¿Quitar Gaming Mode de este PC?",
            ["safety"] = "Mantén Shift al iniciar sesión para forzar el modo Escritorio.",
            ["location"] = "Ubicación"
        },
        ["fr"] = new()
        {
            ["language.title"] = "Choisir la langue",
            ["setup.title"] = "Installation de Gaming Mode",
            ["notice.title"] = "Plugin Decky requis",
            ["notice.body"] = "Installez le plugin Gaming Mode via Decky Loader avant de lancer cette installation. L'installateur Windows configure uniquement l'application compagnon, l'agent local, les raccourcis et le mode de démarrage.",
            ["notice.note"] = "Si le plugin est absent, les contrôles depuis Steam ne seront pas disponibles.",
            ["next"] = "Suivant",
            ["back"] = "Retour",
            ["options"] = "Options d'installation",
            ["default.startup"] = "Démarrage par défaut",
            ["default.startup.desc"] = "Définit le shell par défaut au démarrage.",
            ["desktop.mode"] = "Mode Bureau",
            ["gaming.mode"] = "Mode Gaming",
            ["cursor.autohide"] = "Masquer le pointeur inactif",
            ["cursor.autohide.desc"] = "Masque le pointeur après une courte période d'inactivité.",
            ["desktop.shortcut"] = "Ajouter un raccourci Bureau",
            ["desktop.shortcut.desc"] = "Crée un raccourci sur le Bureau.",
            ["launch.after"] = "Ouvrir Gaming Mode à la fin",
            ["launch.after.desc"] = "Lance l'application compagnon à la fin du setup.",
            ["install"] = "Installer",
            ["update"] = "Mettre à jour",
            ["uninstall"] = "Désinstaller",
            ["close"] = "Fermer",
            ["installing"] = "Installation...",
            ["uninstalling"] = "Désinstallation...",
            ["ready"] = "Prêt à installer.",
            ["installed"] = "Gaming Mode est installé.",
            ["remove.question"] = "Supprimer Gaming Mode de ce PC ?",
            ["safety"] = "Maintenez Shift à la connexion pour forcer le mode Bureau.",
            ["location"] = "Emplacement"
        },
        ["de"] = new()
        {
            ["language.title"] = "Sprache auswählen",
            ["setup.title"] = "Gaming Mode Setup",
            ["notice.title"] = "Decky-Plugin erforderlich",
            ["notice.body"] = "Installiere das Gaming Mode Plugin über Decky Loader, bevor dieses Setup ausgeführt wird. Das Windows-Setup konfiguriert nur Begleit-App, lokalen Agent, Verknüpfungen und Startmodus.",
            ["notice.note"] = "Wenn das Plugin fehlt, sind die Steam-Steuerelemente nicht verfügbar.",
            ["next"] = "Weiter",
            ["back"] = "Zurück",
            ["options"] = "Installationsoptionen",
            ["default.startup"] = "Standardstart",
            ["default.startup.desc"] = "Legt die Standard-Shell beim Start fest.",
            ["desktop.mode"] = "Desktop-Modus",
            ["gaming.mode"] = "Gaming-Modus",
            ["cursor.autohide"] = "Inaktiven Mauszeiger ausblenden",
            ["cursor.autohide.desc"] = "Blendet den Zeiger nach kurzer Inaktivität aus.",
            ["desktop.shortcut"] = "Desktop-Verknüpfung hinzufügen",
            ["desktop.shortcut.desc"] = "Erstellt eine Desktop-Verknüpfung.",
            ["launch.after"] = "Gaming Mode nach Installation öffnen",
            ["launch.after.desc"] = "Startet die Begleit-App nach dem Setup.",
            ["install"] = "Installieren",
            ["update"] = "Aktualisieren",
            ["uninstall"] = "Deinstallieren",
            ["close"] = "Schließen",
            ["installing"] = "Installation...",
            ["uninstalling"] = "Deinstallation...",
            ["ready"] = "Bereit zur Installation.",
            ["installed"] = "Gaming Mode ist installiert.",
            ["remove.question"] = "Gaming Mode von diesem PC entfernen?",
            ["safety"] = "Shift bei der Anmeldung halten, um den Desktop-Modus zu erzwingen.",
            ["location"] = "Installationsort"
        },
        ["pt"] = new()
        {
            ["language.title"] = "Escolha o idioma",
            ["setup.title"] = "Instalador Gaming Mode",
            ["notice.title"] = "Plugin Decky obrigatório",
            ["notice.body"] = "Instale o plugin Gaming Mode através do Decky Loader antes de executar este instalador. O instalador do Windows configura apenas a app companheira, o agente local, os atalhos e o modo de arranque.",
            ["notice.note"] = "Se o plugin não estiver instalado, os controlos no Steam não estarão disponíveis.",
            ["next"] = "Seguinte",
            ["back"] = "Voltar",
            ["options"] = "Opções de instalação",
            ["default.startup"] = "Arranque predefinido",
            ["default.startup.desc"] = "Define a shell predefinida no arranque.",
            ["desktop.mode"] = "Modo Desktop",
            ["gaming.mode"] = "Modo Gaming",
            ["cursor.autohide"] = "Ocultar ponteiro inativo",
            ["cursor.autohide.desc"] = "Oculta o ponteiro após um curto período de inatividade.",
            ["desktop.shortcut"] = "Adicionar atalho no desktop",
            ["desktop.shortcut.desc"] = "Cria um atalho no desktop.",
            ["launch.after"] = "Abrir Gaming Mode no fim",
            ["launch.after.desc"] = "Inicia a app companheira no fim do setup.",
            ["install"] = "Instalar",
            ["update"] = "Atualizar",
            ["uninstall"] = "Desinstalar",
            ["close"] = "Fechar",
            ["installing"] = "A instalar...",
            ["uninstalling"] = "A desinstalar...",
            ["ready"] = "Pronto para instalar.",
            ["installed"] = "Gaming Mode está instalado.",
            ["remove.question"] = "Remover Gaming Mode deste PC?",
            ["safety"] = "Mantenha Shift no início de sessão para forçar o modo Desktop.",
            ["location"] = "Local"
        }
    };

    public static string T(string key)
    {
        foreach (var language in GetLanguageCandidates())
        {
            if (Strings.TryGetValue(language, out var local) && local.TryGetValue(key, out var value))
            {
                return value;
            }
        }

        return Strings["en"].TryGetValue(key, out var fallback) ? fallback : key;
    }

    public static void SetLanguage(string code)
    {
        var language = NormalizeLanguage(code);
        CurrentLanguage = language;
        var culture = CultureInfo.GetCultureInfo(language);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    private static IEnumerable<string> GetLanguageCandidates()
    {
        yield return CurrentLanguage;
        yield return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        yield return "en";
    }

    private static string NormalizeLanguage(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return "en";
        }

        if (Strings.ContainsKey(code))
        {
            return code;
        }

        var twoLetter = code.Split('-', StringSplitOptions.RemoveEmptyEntries)[0];
        return Strings.ContainsKey(twoLetter) ? twoLetter : "en";
    }
}
