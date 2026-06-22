const {
  app,
  BrowserWindow,
  Tray,
  Menu,
  ipcMain,
  nativeImage,
  desktopCapturer,
  session,
} = require("electron");
const path = require("path");
const { mouse, keyboard, Button, Key } = require("@nut-tree-fork/nut-js");
const { serverConfig } = require("./config");

app.commandLine.appendSwitch('disable-ipv6');
app.commandLine.appendSwitch('disable-http2');
app.commandLine.appendSwitch('ignore-certificate-errors');

// ── Disable nut-js delays for real-time control ───────────────────────────────
mouse.config.autoDelayMs = 0;
keyboard.config.autoDelayMs = 0;

const isDev =
  !app.isPackaged || process.env.FLOWDESK_AGENT_DEV === "1";
const DEV_URL = serverConfig.serverUrl;

// ── Single instance lock ──────────────────────────────────────────────────────
const gotLock = app.requestSingleInstanceLock();
if (!gotLock) {
  app.quit();
  process.exit(0);
}

// ── State ─────────────────────────────────────────────────────────────────────
let tray = null;
let loginWindow = null;
let agentWindow = null;
let isConnected = false;
let isSessionActive = false;

// ── Accept self-signed dev cert for localhost ─────────────────────────────────
app.on(
  "certificate-error",
  (event, _webContents, url, _error, _certificate, callback) => {
    if (
      url.startsWith(DEV_URL) ||
      url.includes("localhost") ||
      url.includes("127.0.0.1")
    ) {
      event.preventDefault();
      callback(true);
    } else {
      callback(false);
    }
  },
);

// ── Auto start ────────────────────────────────────────────────────────────────
function setAutoStart() {
  app.setLoginItemSettings({
    openAtLogin: true,
    openAsHidden: true,
    name: "FlowDesk Agent",
  });
}

// ── Key map: string name -> nut-js Key ────────────────────────────────────────
const KEY_MAP = {
  enter: Key.Return,
  return: Key.Return,
  backspace: Key.Backspace,
  delete: Key.Delete,
  escape: Key.Escape,
  tab: Key.Tab,
  space: Key.Space,
  arrowup: Key.Up,
  arrowdown: Key.Down,
  arrowleft: Key.Left,
  arrowright: Key.Right,
  home: Key.Home,
  end: Key.End,
  pageup: Key.PageUp,
  pagedown: Key.PageDown,
  ctrl: Key.LeftControl,
  alt: Key.LeftAlt,
  shift: Key.LeftShift,
  command: Key.LeftSuper,
  meta: Key.LeftSuper,
  f1: Key.F1,
  f2: Key.F2,
  f3: Key.F3,
  f4: Key.F4,
  f5: Key.F5,
  f6: Key.F6,
  f7: Key.F7,
  f8: Key.F8,
  f9: Key.F9,
  f10: Key.F10,
  f11: Key.F11,
  f12: Key.F12,
};

function resolveKey(keyStr) {
  if (!keyStr) return null;
  const lower = keyStr.toLowerCase();
  if (KEY_MAP[lower] !== undefined) return KEY_MAP[lower];
  const upper = keyStr.toUpperCase();
  if (Key[upper] !== undefined) return Key[upper];
  return null;
}

// ── Get tray icon based on OS ───────────────────────────────────────────────
function getIconPath() {
  const iconFile =
    process.platform === "darwin" ? "trayTemplate.png" : "tray.ico";
  const iconPath = path.join(__dirname, "assets", iconFile);
  const icon = nativeImage.createFromPath(iconPath);
  return icon.isEmpty() ? nativeImage.createEmpty() : icon;
}

// ── IPC: OS control via nut-js ────────────────────────────────────────────────

ipcMain.on("agent:mouse-move", async (_, { x, y }) => {
  try {
    await mouse.setPosition({ x, y });
  } catch {
    /* ignore transient nut-js errors */
  }
});

ipcMain.on(
  "agent:mouse-click",
  async (_, { x, y, button, double: isDouble }) => {
    try {
      await mouse.setPosition({ x, y });
      const btn = button === "right" ? Button.RIGHT : Button.LEFT;
      isDouble ? await mouse.doubleClick(btn) : await mouse.click(btn);
    } catch {
      /* ignore transient nut-js errors */
    }
  },
);

ipcMain.on("agent:key-press", async (_, { key, modifier }) => {
  try {
    const resolvedKey = resolveKey(key);
    if (!resolvedKey) return;
    const resolvedMod = resolveKey(modifier);
    if (resolvedMod) {
      await keyboard.pressKey(resolvedMod);
      await keyboard.type(resolvedKey);
      await keyboard.releaseKey(resolvedMod);
    } else {
      await keyboard.type(resolvedKey);
    }
  } catch {
    /* ignore transient nut-js errors */
  }
});

ipcMain.on("agent:scroll", async (_, { x, y, direction }) => {
  try {
    await mouse.setPosition({ x, y });
    direction === "up" ? await mouse.scrollUp(3) : await mouse.scrollDown(3);
  } catch {
    /* ignore transient nut-js errors */
  }
});

ipcMain.handle("agent:get-screen-size", () => {
  try {
    const { width, height } =
      require("electron").screen.getPrimaryDisplay().size;
    return { width, height };
  } catch {
    return { width: 1920, height: 1080 };
  }
});

// ── Windows management ────────────────────────────────────────────────────────

function createLoginWindow() {
  if (loginWindow) {
    loginWindow.show();
    loginWindow.focus();
    return;
  }

  loginWindow = new BrowserWindow({
    width: 500,
    height: 700,
    resizable: false,
    center: true,
    icon: getIconPath(),
    title: "FlowDesk Agent",
    autoHideMenuBar: true,
    webPreferences: {
      preload: path.join(__dirname, "preload.js"),
      contextIsolation: true,
      nodeIntegration: false,
      sandbox: false,
      session: session.defaultSession,
    },
  });
loginWindow.webContents.openDevTools({ mode: 'detach' });
  loginWindow.loadFile(path.join(__dirname, "renderer", "login.html"));
  loginWindow.on("closed", () => {
    loginWindow = null;
  });
}

function createAgentWindow(payload) {
  if (agentWindow) {
    agentWindow.webContents.send("agent:start", payload);
    return;
  }

  agentWindow = new BrowserWindow({
    width: isDev ? 960 : 1,
    height: isDev ? 720 : 1,
    show: isDev,
    skipTaskbar: !isDev,
    title: isDev ? "FlowDesk Agent (Debug)" : "FlowDesk Agent",
    webPreferences: {
      preload: path.join(__dirname, "preload.js"),
      contextIsolation: true,
      nodeIntegration: false,
      sandbox: false,
      session: session.defaultSession,
    },
  });

  agentWindow.loadFile(path.join(__dirname, "renderer", "agent.html"));

  agentWindow.webContents.once("did-finish-load", () => {
    if (isDev) {
      agentWindow.show();
      agentWindow.webContents.openDevTools({ mode: "detach" });
    }
    agentWindow.webContents.send("agent:start", payload);
  });

  agentWindow.on("closed", () => {
    agentWindow = null;
  });
}

// ── Tray ──────────────────────────────────────────────────────────────────────
function createTray() {
  tray = new Tray(getIconPath());
  tray.setToolTip("FlowDesk Agent");
  updateTrayMenu();
}

function updateTrayMenu() {
  if (!tray) return;

  const status = isConnected
    ? isSessionActive
      ? "🟡Session Active"
      : "🟢Connected"
    : "🔴Disconnected";

  tray.setContextMenu(
    Menu.buildFromTemplate([
      { label: "FlowDesk Agent", enabled: false },
      { label: status, enabled: false },
      { type: "separator" },
      { label: "Login / Switch Account", click: createLoginWindow },
      { type: "separator" },
      {
        label: "Quit",
        click: () => {
          app.isQuitting = true;
          app.quit();
        },
      },
    ]),
  );
}

// ── IPC: auth flow ────────────────────────────────────────────────────────────

ipcMain.on("agent:auth-success", (_, payload) => {
  if (loginWindow) loginWindow.hide();
  createAgentWindow(payload);
});

ipcMain.on("agent:re-login", () => {
  if (agentWindow) {
    agentWindow.destroy();
    agentWindow = null;
  }
  createLoginWindow();
});

// ── IPC: status updates ───────────────────────────────────────────────────────

ipcMain.on("agent:connected", () => {
  isConnected = true;
  isSessionActive = false;
  updateTrayMenu();
});
ipcMain.on("agent:disconnected", () => {
  isConnected = false;
  isSessionActive = false;
  updateTrayMenu();
});
ipcMain.on("agent:session-started", () => {
  isSessionActive = true;
  updateTrayMenu();
});
ipcMain.on("agent:session-ended", () => {
  isSessionActive = false;
  updateTrayMenu();
});

// ── App lifecycle ─────────────────────────────────────────────────────────────

app.whenReady().then(() => {
  session.defaultSession.setCertificateVerifyProc((req, cb) => {
    if (
      req.hostname === "localhost" ||
      req.hostname === "127.0.0.1" ||
      (DEV_URL && req.url && req.url.startsWith(DEV_URL))
    ) {
      cb(0);
    } else {
      cb(-1);
    }
  });

  session.defaultSession.setDisplayMediaRequestHandler((request, callback) => {
    desktopCapturer
      .getSources({ types: ["screen"] })
      .then((sources) => {
        if (sources.length === 0) {
          callback();
          return;
        }
        callback({ video: sources[0], audio: "loopback" });
      })
      .catch(() => {
        callback();
      });
  });

  setAutoStart();
  createTray();
  createLoginWindow();
});

app.on("window-all-closed", (e) => {
  if (!app.isQuitting) e.preventDefault();
});

app.on("before-quit", () => {
  app.isQuitting = true;
});

app.on("second-instance", () => {
  createLoginWindow();
});
