# FlowDesk Agent

Electron-based remote support agent for employee machines.
Works alongside the FlowDesk web app to enable AnyDesk-style remote control.

---

## Setup

```bash
cd FlowDesk.Agent
npm install
```

> robotjs requires native compilation. You need:
> - Windows: `npm install --global windows-build-tools`
> - macOS: Xcode Command Line Tools (`xcode-select --install`)

---

## Run in Development

```bash
npm start
```

---

## Build Installers

```bash
# Windows (.exe installer)
npm run build:win

# macOS (.dmg)
npm run build:mac

# Both
npm run build:all
```

Outputs go to `dist/` folder.

---

## How It Works

1. Employee installs and opens the agent
2. Agent shows a login window — employee enters FlowDesk credentials
3. Agent connects to SignalR hub with JWT token
4. Agent minimizes to system tray — runs silently in background
5. When support requests remote access:
   - Employee sees Accept/Reject popup in their **browser** (React app)
   - Employee clicks Accept
   - Agent automatically starts screen capture
   - Support sees live screen in their browser dashboard
   - Support can control mouse/keyboard
6. Support clicks End Session — everything cleans up

---

## Auto Start on Boot

Automatically configured on first login via `app.setLoginItemSettings`.

---

## File Structure

```
FlowDesk.Agent/
├── main.js          ← Electron main process (tray, IPC, robotjs)
├── preload.js       ← contextBridge (IPC bridge)
├── renderer/
│   ├── login.html   ← Employee login form
│   ├── agent.html   ← Hidden background window
│   └── agent.js     ← SignalR + WebRTC logic
├── assets/
│   ├── icon.ico     ← Windows tray icon (add your own)
│   ├── icon.icns    ← macOS app icon (add your own)
│   └── trayTemplate.png ← macOS tray icon (add your own)
└── package.json
```

---

## Icons Required

Add your own icons to `assets/`:
- `icon.ico` — 256x256 Windows icon
- `icon.icns` — macOS icon
- `trayTemplate.png` — 16x16 or 22x22 macOS tray icon (black on transparent)
- `tray.ico` — 16x16 Windows tray icon

---

## Backend SignalR Hub Methods Used

| Called by Agent | Purpose |
|---|---|
| `AgentReady(sessionId)` | Register agent, activate session |
| `SendWebRTCOffer(sessionId, sdp)` | Send offer to admin |
| `SendICECandidate(sessionId, candidate, true)` | Send ICE to admin |

| Received by Agent | Purpose |
|---|---|
| `RemoteSessionAccepted` | Start screen capture |
| `ReceiveWebRTCAnswer` | Complete WebRTC handshake |
| `ReceiveICECandidate` | Add ICE candidate |
| `ExecuteControlEvent` | Execute mouse/keyboard via robotjs |
| `RemoteSessionEnded` | Clean up session |
