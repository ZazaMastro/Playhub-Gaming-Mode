using System.Globalization;

namespace GamingMode;

public static class L
{
    private static readonly Dictionary<string, Dictionary<string, string>> Strings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["en"] = new()
        {
            ["app.title"] = "Gaming Mode",
            ["action.gaming"] = "Switch to Gaming Mode",
            ["action.desktop"] = "Switch to Desktop Mode",
            ["default.startup"] = "Default startup",
            ["desktop.mode"] = "Desktop Mode",
            ["gaming.mode"] = "Gaming Mode",
            ["config"] = "Config",
            ["splash.logo"] = "Startup logo",
            ["agent.starting"] = "Starting local agent...",
            ["agent.ready"] = "Agent ready.",
            ["agent.unreachable"] = "Agent is not reachable on localhost:47991.",
            ["status.current"] = "Current",
            ["status.default"] = "Default",
            ["status.shell"] = "Shell",
            ["status.ready"] = "Ready.",
            ["status.noStatus"] = "Agent returned no status.",
            ["error.unreachable"] = "Agent is not reachable on localhost:47991.",
            ["error.unreadable"] = "Agent response could not be read."
        },
        ["it"] = new()
        {
            ["app.title"] = "Gaming Mode",
            ["action.gaming"] = "Passa alla modalità Gaming",
            ["action.desktop"] = "Passa alla modalità Desktop",
            ["default.startup"] = "Avvio predefinito",
            ["desktop.mode"] = "Modalità Desktop",
            ["gaming.mode"] = "Modalità Gaming",
            ["config"] = "Config",
            ["splash.logo"] = "Logo di avvio",
            ["agent.starting"] = "Avvio dell'agent locale...",
            ["agent.ready"] = "Agent pronto.",
            ["agent.unreachable"] = "Agent non raggiungibile su localhost:47991.",
            ["status.current"] = "Attuale",
            ["status.default"] = "Predefinita",
            ["status.shell"] = "Shell",
            ["status.ready"] = "Pronto.",
            ["status.noStatus"] = "L'agent non ha restituito uno stato.",
            ["error.unreachable"] = "Agent non raggiungibile su localhost:47991.",
            ["error.unreadable"] = "La risposta dell'agent non può essere letta."
        },
        ["es"] = new()
        {
            ["app.title"] = "Gaming Mode",
            ["action.gaming"] = "Cambiar al modo Gaming",
            ["action.desktop"] = "Cambiar al modo Escritorio",
            ["default.startup"] = "Inicio predeterminado",
            ["desktop.mode"] = "Modo Escritorio",
            ["gaming.mode"] = "Modo Gaming",
            ["config"] = "Config",
            ["splash.logo"] = "Logo de inicio",
            ["agent.starting"] = "Iniciando agente local...",
            ["agent.ready"] = "Agente listo.",
            ["agent.unreachable"] = "No se puede contactar con el agente en localhost:47991.",
            ["status.current"] = "Actual",
            ["status.default"] = "Predeterminado",
            ["status.shell"] = "Shell",
            ["status.ready"] = "Listo.",
            ["status.noStatus"] = "El agente no devolvió ningún estado.",
            ["error.unreachable"] = "No se puede contactar con el agente en localhost:47991.",
            ["error.unreadable"] = "No se pudo leer la respuesta del agente."
        },
        ["fr"] = new()
        {
            ["app.title"] = "Gaming Mode",
            ["action.gaming"] = "Passer en mode Gaming",
            ["action.desktop"] = "Passer en mode Bureau",
            ["default.startup"] = "Démarrage par défaut",
            ["desktop.mode"] = "Mode Bureau",
            ["gaming.mode"] = "Mode Gaming",
            ["config"] = "Config",
            ["splash.logo"] = "Logo de démarrage",
            ["agent.starting"] = "Démarrage de l'agent local...",
            ["agent.ready"] = "Agent prêt.",
            ["agent.unreachable"] = "Agent injoignable sur localhost:47991.",
            ["status.current"] = "Actuel",
            ["status.default"] = "Défaut",
            ["status.shell"] = "Shell",
            ["status.ready"] = "Prêt.",
            ["status.noStatus"] = "L'agent n'a renvoyé aucun état.",
            ["error.unreachable"] = "Agent injoignable sur localhost:47991.",
            ["error.unreadable"] = "La réponse de l'agent ne peut pas être lue."
        },
        ["de"] = new()
        {
            ["app.title"] = "Gaming Mode",
            ["action.gaming"] = "In den Gaming-Modus wechseln",
            ["action.desktop"] = "In den Desktop-Modus wechseln",
            ["default.startup"] = "Standardstart",
            ["desktop.mode"] = "Desktop-Modus",
            ["gaming.mode"] = "Gaming-Modus",
            ["config"] = "Config",
            ["splash.logo"] = "Startlogo",
            ["agent.starting"] = "Lokalen Agent starten...",
            ["agent.ready"] = "Agent bereit.",
            ["agent.unreachable"] = "Agent ist auf localhost:47991 nicht erreichbar.",
            ["status.current"] = "Aktuell",
            ["status.default"] = "Standard",
            ["status.shell"] = "Shell",
            ["status.ready"] = "Bereit.",
            ["status.noStatus"] = "Agent hat keinen Status geliefert.",
            ["error.unreachable"] = "Agent ist auf localhost:47991 nicht erreichbar.",
            ["error.unreadable"] = "Agent-Antwort konnte nicht gelesen werden."
        },
        ["pt"] = new()
        {
            ["app.title"] = "Gaming Mode",
            ["action.gaming"] = "Mudar para o modo Gaming",
            ["action.desktop"] = "Mudar para o modo Desktop",
            ["default.startup"] = "Arranque predefinido",
            ["desktop.mode"] = "Modo Desktop",
            ["gaming.mode"] = "Modo Gaming",
            ["config"] = "Config",
            ["splash.logo"] = "Logótipo de arranque",
            ["agent.starting"] = "A iniciar agente local...",
            ["agent.ready"] = "Agente pronto.",
            ["agent.unreachable"] = "Agente indisponível em localhost:47991.",
            ["status.current"] = "Atual",
            ["status.default"] = "Predefinido",
            ["status.shell"] = "Shell",
            ["status.ready"] = "Pronto.",
            ["status.noStatus"] = "O agente não devolveu estado.",
            ["error.unreachable"] = "Agente indisponível em localhost:47991.",
            ["error.unreadable"] = "Não foi possível ler a resposta do agente."
        },
        ["pt-BR"] = new()
        {
            ["app.title"] = "Gaming Mode",
            ["action.gaming"] = "Mudar para o modo Gaming",
            ["action.desktop"] = "Mudar para o modo Desktop",
            ["default.startup"] = "Inicialização padrão",
            ["desktop.mode"] = "Modo Desktop",
            ["gaming.mode"] = "Modo Gaming",
            ["config"] = "Config",
            ["splash.logo"] = "Logo de inicialização",
            ["agent.starting"] = "Iniciando agente local...",
            ["agent.ready"] = "Agente pronto.",
            ["agent.unreachable"] = "Agente indisponível em localhost:47991.",
            ["status.current"] = "Atual",
            ["status.default"] = "Padrão",
            ["status.shell"] = "Shell",
            ["status.ready"] = "Pronto.",
            ["status.noStatus"] = "O agente não retornou um status.",
            ["error.unreachable"] = "Agente indisponível em localhost:47991.",
            ["error.unreadable"] = "Não foi possível ler a resposta do agente."
        },
        ["nl"] = new()
        {
            ["app.title"] = "Gaming Mode",
            ["action.gaming"] = "Overschakelen naar Gaming Mode",
            ["action.desktop"] = "Overschakelen naar Desktop Mode",
            ["default.startup"] = "Standaard opstartmodus",
            ["desktop.mode"] = "Desktop Mode",
            ["gaming.mode"] = "Gaming Mode",
            ["config"] = "Config",
            ["splash.logo"] = "Opstartlogo",
            ["agent.starting"] = "Lokale agent starten...",
            ["agent.ready"] = "Agent gereed.",
            ["agent.unreachable"] = "Agent is niet bereikbaar op localhost:47991.",
            ["status.current"] = "Huidig",
            ["status.default"] = "Standaard",
            ["status.shell"] = "Shell",
            ["status.ready"] = "Gereed.",
            ["status.noStatus"] = "Agent heeft geen status teruggegeven.",
            ["error.unreachable"] = "Agent is niet bereikbaar op localhost:47991.",
            ["error.unreadable"] = "Agentantwoord kon niet worden gelezen."
        },
        ["uk"] = new()
        {
            ["app.title"] = "Gaming Mode",
            ["action.gaming"] = "Перейти в ігровий режим",
            ["action.desktop"] = "Перейти в режим робочого столу",
            ["default.startup"] = "Запуск за замовчуванням",
            ["desktop.mode"] = "Режим робочого столу",
            ["gaming.mode"] = "Ігровий режим",
            ["config"] = "Налаштування",
            ["splash.logo"] = "Логотип запуску",
            ["agent.starting"] = "Запуск локального агента...",
            ["agent.ready"] = "Агент готовий.",
            ["agent.unreachable"] = "Агент недоступний на localhost:47991.",
            ["status.current"] = "Поточний",
            ["status.default"] = "За замовчуванням",
            ["status.shell"] = "Shell",
            ["status.ready"] = "Готово.",
            ["status.noStatus"] = "Агент не повернув стан.",
            ["error.unreachable"] = "Агент недоступний на localhost:47991.",
            ["error.unreadable"] = "Не вдалося прочитати відповідь агента."
        },
        ["zh"] = new()
        {
            ["app.title"] = "Gaming Mode",
            ["action.gaming"] = "切换到游戏模式",
            ["action.desktop"] = "切换到桌面模式",
            ["default.startup"] = "默认启动模式",
            ["desktop.mode"] = "桌面模式",
            ["gaming.mode"] = "游戏模式",
            ["config"] = "配置",
            ["splash.logo"] = "启动标志",
            ["agent.starting"] = "正在启动本地代理...",
            ["agent.ready"] = "代理已就绪。",
            ["agent.unreachable"] = "无法连接 localhost:47991 上的代理。",
            ["status.current"] = "当前",
            ["status.default"] = "默认",
            ["status.shell"] = "Shell",
            ["status.ready"] = "就绪。",
            ["status.noStatus"] = "代理没有返回状态。",
            ["error.unreachable"] = "无法连接 localhost:47991 上的代理。",
            ["error.unreadable"] = "无法读取代理响应。"
        },
        ["ja"] = new()
        {
            ["app.title"] = "Gaming Mode",
            ["action.gaming"] = "Gaming Mode に切り替え",
            ["action.desktop"] = "Desktop Mode に切り替え",
            ["default.startup"] = "既定の起動モード",
            ["desktop.mode"] = "Desktop Mode",
            ["gaming.mode"] = "Gaming Mode",
            ["config"] = "設定",
            ["splash.logo"] = "起動ロゴ",
            ["agent.starting"] = "ローカルエージェントを起動中...",
            ["agent.ready"] = "エージェントの準備ができました。",
            ["agent.unreachable"] = "localhost:47991 のエージェントに接続できません。",
            ["status.current"] = "現在",
            ["status.default"] = "既定",
            ["status.shell"] = "Shell",
            ["status.ready"] = "準備完了。",
            ["status.noStatus"] = "エージェントから状態が返されませんでした。",
            ["error.unreachable"] = "localhost:47991 のエージェントに接続できません。",
            ["error.unreadable"] = "エージェントの応答を読み取れませんでした。"
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
