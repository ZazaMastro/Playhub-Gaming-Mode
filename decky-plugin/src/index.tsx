import {
  ButtonItem,
  DropdownItem,
  PanelSection,
  PanelSectionRow,
  staticClasses,
  type SingleDropdownOption,
} from "@decky/ui";
import { definePlugin, toaster } from "@decky/api";
import { useEffect, useMemo, useState } from "react";
import { FaGamepad } from "react-icons/fa";

const API_BASE = "http://127.0.0.1:47991";

type BootMode = "Desktop" | "Gaming";

type ModeStatus = {
  defaultMode: BootMode;
};

type ApiResult = {
  ok: boolean;
  message: string;
  status?: ModeStatus;
};

const strings = {
  en: {
    mode: "Mode",
    switchGaming: "Switch to Gaming Mode",
    switchDesktop: "Switch to Desktop Mode",
    defaultStartup: "Default startup",
    desktopMode: "Desktop Mode",
    gamingMode: "Gaming Mode",
    notConnected: "Agent not connected",
    agentReturned: "Agent returned",
  },
  it: {
    mode: "Modalità",
    switchGaming: "Passa alla modalità Gaming",
    switchDesktop: "Passa alla modalità Desktop",
    defaultStartup: "Avvio predefinito",
    desktopMode: "Modalità Desktop",
    gamingMode: "Modalità Gaming",
    notConnected: "Agent non collegato",
    agentReturned: "Agent ha risposto",
  },
  es: {
    mode: "Modo",
    switchGaming: "Cambiar al modo Gaming",
    switchDesktop: "Cambiar al modo Escritorio",
    defaultStartup: "Inicio predeterminado",
    desktopMode: "Modo Escritorio",
    gamingMode: "Modo Gaming",
    notConnected: "Agente no conectado",
    agentReturned: "El agente devolvió",
  },
  fr: {
    mode: "Mode",
    switchGaming: "Passer en mode Gaming",
    switchDesktop: "Passer en mode Bureau",
    defaultStartup: "Démarrage par défaut",
    desktopMode: "Mode Bureau",
    gamingMode: "Mode Gaming",
    notConnected: "Agent non connecté",
    agentReturned: "Agent a renvoyé",
  },
  de: {
    mode: "Modus",
    switchGaming: "In den Gaming-Modus wechseln",
    switchDesktop: "In den Desktop-Modus wechseln",
    defaultStartup: "Standardstart",
    desktopMode: "Desktop-Modus",
    gamingMode: "Gaming-Modus",
    notConnected: "Agent nicht verbunden",
    agentReturned: "Agent meldete",
  },
  pt: {
    mode: "Modo",
    switchGaming: "Mudar para modo Gaming",
    switchDesktop: "Mudar para modo Desktop",
    defaultStartup: "Arranque predefinido",
    desktopMode: "Modo Desktop",
    gamingMode: "Modo Gaming",
    notConnected: "Agente não ligado",
    agentReturned: "Agente devolveu",
  },
};

function t() {
  const language = navigator.language.split("-")[0] as keyof typeof strings;
  return strings[language] ?? strings.en;
}

async function getStatus(): Promise<ModeStatus> {
  const response = await fetch(`${API_BASE}/status`);
  if (!response.ok) {
    throw new Error(`${t().agentReturned} ${response.status}`);
  }

  return await response.json();
}

async function post(path: string): Promise<ApiResult> {
  const response = await fetch(`${API_BASE}${path}`, {
    method: "POST",
  });

  if (!response.ok) {
    throw new Error(`${t().agentReturned} ${response.status}`);
  }

  return await response.json();
}

function Content() {
  const local = t();
  const [status, setStatus] = useState<ModeStatus>();
  const [busy, setBusy] = useState(false);

  const defaultOptions = useMemo<SingleDropdownOption[]>(
    () => [
      { data: "Desktop", label: local.desktopMode },
      { data: "Gaming", label: local.gamingMode },
    ],
    [local.desktopMode, local.gamingMode],
  );

  const refresh = async () => {
    try {
      setStatus(await getStatus());
    } catch (error) {
      setStatus(undefined);
      toaster.toast({
        title: "Gaming Mode",
        body: error instanceof Error ? error.message : local.notConnected,
      });
    }
  };

  const run = async (path: string, title: string) => {
    setBusy(true);
    try {
      const result = await post(path);
      toaster.toast({
        title,
        body: result.message,
      });
      if (result.status) {
        setStatus(result.status);
      } else {
        await refresh();
      }
    } catch (error) {
      toaster.toast({
        title,
        body: error instanceof Error ? error.message : local.notConnected,
      });
    } finally {
      setBusy(false);
    }
  };

  const setDefault = async (option: SingleDropdownOption) => {
    const mode = option.data as BootMode;
    await run(mode === "Gaming" ? "/default/gaming" : "/default/desktop", local.defaultStartup);
  };

  useEffect(() => {
    refresh();
    const timer = window.setInterval(refresh, 5000);
    return () => window.clearInterval(timer);
  }, []);

  return (
    <PanelSection title={local.mode}>
      <PanelSectionRow>
        <ButtonItem
          disabled={busy}
          layout="below"
          onClick={() => run("/mode/gaming/switch", local.gamingMode)}
        >
          {local.switchGaming}
        </ButtonItem>
      </PanelSectionRow>
      <PanelSectionRow>
        <ButtonItem
          disabled={busy}
          layout="below"
          onClick={() => run("/mode/desktop/switch", local.desktopMode)}
        >
          {local.switchDesktop}
        </ButtonItem>
      </PanelSectionRow>
      <PanelSectionRow>
        <DropdownItem
          label={local.defaultStartup}
          disabled={busy}
          rgOptions={defaultOptions}
          selectedOption={status?.defaultMode ?? "Desktop"}
          onChange={setDefault}
        />
      </PanelSectionRow>
    </PanelSection>
  );
}

export default definePlugin(() => {
  return {
    name: "Gaming Mode",
    titleView: <div className={staticClasses.Title}>Gaming Mode</div>,
    content: <Content />,
    icon: <FaGamepad />,
  };
});
