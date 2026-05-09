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
            ["action.gaming"] = "Passa alla modalita Gaming",
            ["action.desktop"] = "Passa alla modalita Desktop",
            ["default.startup"] = "Avvio predefinito",
            ["desktop.mode"] = "Modalita Desktop",
            ["gaming.mode"] = "Modalita Gaming",
            ["config"] = "Config",
            ["agent.starting"] = "Avvio dell'agent locale...",
            ["agent.ready"] = "Agent pronto.",
            ["agent.unreachable"] = "Agent non raggiungibile su localhost:47991.",
            ["status.current"] = "Attuale",
            ["status.default"] = "Predefinita",
            ["status.shell"] = "Shell",
            ["status.ready"] = "Pronto.",
            ["status.noStatus"] = "L'agent non ha restituito uno stato.",
            ["error.unreachable"] = "Agent non raggiungibile su localhost:47991.",
            ["error.unreadable"] = "La risposta dell'agent non puo essere letta."
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
            ["agent.starting"] = "Iniciando agente local...",
            ["agent.ready"] = "Agente listo.",
            ["agent.unreachable"] = "No se puede contactar el agente en localhost:47991.",
            ["status.current"] = "Actual",
            ["status.default"] = "Predeterminado",
            ["status.shell"] = "Shell",
            ["status.ready"] = "Listo.",
            ["status.noStatus"] = "El agente no devolvio estado.",
            ["error.unreachable"] = "No se puede contactar el agente en localhost:47991.",
            ["error.unreadable"] = "No se pudo leer la respuesta del agente."
        },
        ["fr"] = new()
        {
            ["app.title"] = "Gaming Mode",
            ["action.gaming"] = "Passer en mode Gaming",
            ["action.desktop"] = "Passer en mode Bureau",
            ["default.startup"] = "Demarrage par defaut",
            ["desktop.mode"] = "Mode Bureau",
            ["gaming.mode"] = "Mode Gaming",
            ["config"] = "Config",
            ["agent.starting"] = "Demarrage de l'agent local...",
            ["agent.ready"] = "Agent pret.",
            ["agent.unreachable"] = "Agent injoignable sur localhost:47991.",
            ["status.current"] = "Actuel",
            ["status.default"] = "Defaut",
            ["status.shell"] = "Shell",
            ["status.ready"] = "Pret.",
            ["status.noStatus"] = "L'agent n'a renvoye aucun etat.",
            ["error.unreachable"] = "Agent injoignable sur localhost:47991.",
            ["error.unreadable"] = "La reponse de l'agent ne peut pas etre lue."
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
            ["action.gaming"] = "Mudar para modo Gaming",
            ["action.desktop"] = "Mudar para modo Desktop",
            ["default.startup"] = "Arranque predefinido",
            ["desktop.mode"] = "Modo Desktop",
            ["gaming.mode"] = "Modo Gaming",
            ["config"] = "Config",
            ["agent.starting"] = "A iniciar agente local...",
            ["agent.ready"] = "Agente pronto.",
            ["agent.unreachable"] = "Agente indisponivel em localhost:47991.",
            ["status.current"] = "Atual",
            ["status.default"] = "Predefinido",
            ["status.shell"] = "Shell",
            ["status.ready"] = "Pronto.",
            ["status.noStatus"] = "O agente nao devolveu estado.",
            ["error.unreachable"] = "Agente indisponivel em localhost:47991.",
            ["error.unreadable"] = "Nao foi possivel ler a resposta do agente."
        }
    };

    public static string T(string key)
    {
        var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        if (Strings.TryGetValue(language, out var local) && local.TryGetValue(key, out var value))
        {
            return value;
        }

        return Strings["en"].TryGetValue(key, out var fallback) ? fallback : key;
    }
}
