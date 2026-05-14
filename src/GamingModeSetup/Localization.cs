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
        ("pt", "Português", "Portuguese"),
        ("pt-BR", "Português (Brasil)", "Brazilian Portuguese"),
        ("nl", "Nederlands", "Dutch"),
        ("uk", "Українська", "Ukrainian"),
        ("zh", "中文", "Chinese"),
        ("ja", "日本語", "Japanese")
    ];

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
        },
        ["pt-BR"] = new()
        {
            ["language.title"] = "Escolha o idioma",
            ["setup.title"] = "Instalador Gaming Mode",
            ["notice.title"] = "Plugin Decky obrigatório",
            ["notice.body"] = "Instale o plugin Gaming Mode pelo Decky Loader antes de executar este instalador. O instalador do Windows configura apenas o app complementar, o agente local, os atalhos e o modo de inicialização.",
            ["notice.note"] = "Se o plugin estiver ausente, os controles pelo Steam não estarão disponíveis.",
            ["next"] = "Avançar",
            ["back"] = "Voltar",
            ["options"] = "Opções de instalação",
            ["default.startup"] = "Inicialização padrão",
            ["default.startup.desc"] = "Define a shell padrão na inicialização.",
            ["desktop.mode"] = "Modo Desktop",
            ["gaming.mode"] = "Modo Gaming",
            ["cursor.autohide"] = "Ocultar ponteiro inativo",
            ["cursor.autohide.desc"] = "Oculta o ponteiro após um curto período de inatividade.",
            ["desktop.shortcut"] = "Adicionar atalho na área de trabalho",
            ["desktop.shortcut.desc"] = "Cria um atalho na área de trabalho.",
            ["launch.after"] = "Abrir Gaming Mode ao finalizar",
            ["launch.after.desc"] = "Inicia o app complementar ao final do setup.",
            ["install"] = "Instalar",
            ["update"] = "Atualizar",
            ["uninstall"] = "Desinstalar",
            ["close"] = "Fechar",
            ["installing"] = "Instalando...",
            ["uninstalling"] = "Desinstalando...",
            ["ready"] = "Pronto para instalar.",
            ["installed"] = "Gaming Mode está instalado.",
            ["remove.question"] = "Remover Gaming Mode deste PC?",
            ["safety"] = "Mantenha Shift pressionado ao entrar para forçar o modo Desktop.",
            ["location"] = "Local de instalação"
        },
        ["nl"] = new()
        {
            ["language.title"] = "Kies je taal",
            ["setup.title"] = "Gaming Mode Setup",
            ["notice.title"] = "Decky-plugin vereist",
            ["notice.body"] = "Installeer de Gaming Mode-plugin via Decky Loader voordat je deze setup uitvoert. Het Windows-installatieprogramma configureert alleen de companion-app, lokale agent, snelkoppelingen en opstartmodus.",
            ["notice.note"] = "Als de plugin ontbreekt, zijn de Steam-bedieningen niet beschikbaar.",
            ["next"] = "Volgende",
            ["back"] = "Terug",
            ["options"] = "Installatieopties",
            ["default.startup"] = "Standaard opstartmodus",
            ["default.startup.desc"] = "Stelt de standaard shell bij het opstarten in.",
            ["desktop.mode"] = "Desktop Mode",
            ["gaming.mode"] = "Gaming Mode",
            ["cursor.autohide"] = "Inactieve muisaanwijzer verbergen",
            ["cursor.autohide.desc"] = "Verbergt de aanwijzer na een korte periode van inactiviteit.",
            ["desktop.shortcut"] = "Bureaubladsnelkoppeling toevoegen",
            ["desktop.shortcut.desc"] = "Maakt een bureaubladsnelkoppeling.",
            ["launch.after"] = "Gaming Mode openen na installatie",
            ["launch.after.desc"] = "Start de companion-app na de setup.",
            ["install"] = "Installeren",
            ["update"] = "Bijwerken",
            ["uninstall"] = "Verwijderen",
            ["close"] = "Sluiten",
            ["installing"] = "Installeren...",
            ["uninstalling"] = "Verwijderen...",
            ["ready"] = "Klaar om te installeren.",
            ["installed"] = "Gaming Mode is geïnstalleerd.",
            ["remove.question"] = "Gaming Mode van deze pc verwijderen?",
            ["safety"] = "Houd Shift ingedrukt bij aanmelden om Desktop Mode te forceren.",
            ["location"] = "Installatielocatie"
        },
        ["uk"] = new()
        {
            ["language.title"] = "Виберіть мову",
            ["setup.title"] = "Встановлення Gaming Mode",
            ["notice.title"] = "Потрібен плагін Decky",
            ["notice.body"] = "Встановіть плагін Gaming Mode через Decky Loader перед запуском цього встановлювача. Встановлювач Windows налаштовує лише companion app, локальний агент, ярлики та режим запуску.",
            ["notice.note"] = "Якщо плагін відсутній, керування зі Steam буде недоступним.",
            ["next"] = "Далі",
            ["back"] = "Назад",
            ["options"] = "Параметри встановлення",
            ["default.startup"] = "Запуск за замовчуванням",
            ["default.startup.desc"] = "Встановлює shell за замовчуванням під час запуску.",
            ["desktop.mode"] = "Режим робочого столу",
            ["gaming.mode"] = "Ігровий режим",
            ["cursor.autohide"] = "Ховати неактивний курсор",
            ["cursor.autohide.desc"] = "Ховає курсор після короткого періоду неактивності.",
            ["desktop.shortcut"] = "Додати ярлик на робочий стіл",
            ["desktop.shortcut.desc"] = "Створює ярлик на робочому столі.",
            ["launch.after"] = "Відкрити Gaming Mode після встановлення",
            ["launch.after.desc"] = "Запускає companion app після завершення setup.",
            ["install"] = "Встановити",
            ["update"] = "Оновити",
            ["uninstall"] = "Видалити",
            ["close"] = "Закрити",
            ["installing"] = "Встановлення...",
            ["uninstalling"] = "Видалення...",
            ["ready"] = "Готово до встановлення.",
            ["installed"] = "Gaming Mode встановлено.",
            ["remove.question"] = "Видалити Gaming Mode з цього ПК?",
            ["safety"] = "Утримуйте Shift під час входу, щоб примусово ввімкнути режим робочого столу.",
            ["location"] = "Місце встановлення"
        },
        ["zh"] = new()
        {
            ["language.title"] = "选择语言",
            ["setup.title"] = "Gaming Mode 安装程序",
            ["notice.title"] = "需要 Decky 插件",
            ["notice.body"] = "运行此安装程序前，请先通过 Decky Loader 安装 Gaming Mode 插件。Windows 安装程序只会配置伴随应用、本地代理、快捷方式和启动模式。",
            ["notice.note"] = "如果缺少插件，Steam 中的控制项将不可用。",
            ["next"] = "下一步",
            ["back"] = "返回",
            ["options"] = "安装选项",
            ["default.startup"] = "默认启动模式",
            ["default.startup.desc"] = "设置启动时的默认 shell。",
            ["desktop.mode"] = "桌面模式",
            ["gaming.mode"] = "游戏模式",
            ["cursor.autohide"] = "隐藏闲置鼠标指针",
            ["cursor.autohide.desc"] = "短暂闲置后隐藏指针。",
            ["desktop.shortcut"] = "添加桌面快捷方式",
            ["desktop.shortcut.desc"] = "创建桌面快捷方式。",
            ["launch.after"] = "安装后打开 Gaming Mode",
            ["launch.after.desc"] = "安装完成后启动伴随应用。",
            ["install"] = "安装",
            ["update"] = "更新",
            ["uninstall"] = "卸载",
            ["close"] = "关闭",
            ["installing"] = "正在安装...",
            ["uninstalling"] = "正在卸载...",
            ["ready"] = "准备安装。",
            ["installed"] = "Gaming Mode 已安装。",
            ["remove.question"] = "要从此电脑移除 Gaming Mode 吗？",
            ["safety"] = "登录时按住 Shift 可强制进入桌面模式。",
            ["location"] = "安装位置"
        },
        ["ja"] = new()
        {
            ["language.title"] = "言語を選択",
            ["setup.title"] = "Gaming Mode セットアップ",
            ["notice.title"] = "Decky プラグインが必要です",
            ["notice.body"] = "このセットアップを実行する前に、Decky Loader から Gaming Mode プラグインをインストールしてください。Windows インストーラーは companion app、ローカルエージェント、ショートカット、起動モードのみを設定します。",
            ["notice.note"] = "プラグインがない場合、Steam からの操作は利用できません。",
            ["next"] = "次へ",
            ["back"] = "戻る",
            ["options"] = "インストールオプション",
            ["default.startup"] = "既定の起動モード",
            ["default.startup.desc"] = "起動時の既定 shell を設定します。",
            ["desktop.mode"] = "Desktop Mode",
            ["gaming.mode"] = "Gaming Mode",
            ["cursor.autohide"] = "未操作時にマウスポインターを隠す",
            ["cursor.autohide.desc"] = "短時間操作がない場合にポインターを隠します。",
            ["desktop.shortcut"] = "デスクトップショートカットを追加",
            ["desktop.shortcut.desc"] = "デスクトップショートカットを作成します。",
            ["launch.after"] = "インストール後に Gaming Mode を開く",
            ["launch.after.desc"] = "セットアップ完了後に companion app を起動します。",
            ["install"] = "インストール",
            ["update"] = "更新",
            ["uninstall"] = "アンインストール",
            ["close"] = "閉じる",
            ["installing"] = "インストール中...",
            ["uninstalling"] = "アンインストール中...",
            ["ready"] = "インストールの準備ができました。",
            ["installed"] = "Gaming Mode はインストール済みです。",
            ["remove.question"] = "この PC から Gaming Mode を削除しますか？",
            ["safety"] = "サインイン時に Shift を押し続けると Desktop Mode を強制できます。",
            ["location"] = "インストール先"
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
        var language = Strings.ContainsKey(code) ? code : "en";
        var culture = CultureInfo.GetCultureInfo(language);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    private static IEnumerable<string> GetLanguageCandidates()
    {
        var culture = CultureInfo.CurrentUICulture;
        if (!string.IsNullOrWhiteSpace(culture.Name))
        {
            yield return culture.Name;
        }

        if (culture.Name.StartsWith("pt-BR", StringComparison.OrdinalIgnoreCase))
        {
            yield return "pt-BR";
        }

        if (!string.IsNullOrWhiteSpace(culture.TwoLetterISOLanguageName))
        {
            yield return culture.TwoLetterISOLanguageName;
        }

        yield return "en";
    }
}
