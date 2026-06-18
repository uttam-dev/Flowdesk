const { contextBridge, ipcRenderer } = require('electron');
const { serverConfig } = require('./config');

contextBridge.exposeInMainWorld('agent', {
  getServerConfig: () => ({ ...serverConfig }),

  // ── OS control ───────────────────────────────────────────────────────────
  mouseMove    : (x, y)                    => ipcRenderer.send('agent:mouse-move',  { x, y }),
  mouseClick   : (x, y, button, isDouble)  => ipcRenderer.send('agent:mouse-click', { x, y, button, double: isDouble }),
  keyPress     : (key, modifier)           => ipcRenderer.send('agent:key-press',   { key, modifier }),
  scroll       : (x, y, direction)         => ipcRenderer.send('agent:scroll',      { x, y, direction }),
  getScreenSize: ()                        => ipcRenderer.invoke('agent:get-screen-size'),

  // ── Auth flow ────────────────────────────────────────────────────────────
  // Called from login.html after successful login
  authSuccess: (payload) => ipcRenderer.send('agent:auth-success', payload),

  // Called from agent.js when refresh token is also expired → show login again
  requestReLogin: () => ipcRenderer.send('agent:re-login'),

  // ── Status reporting (updates tray icon) ─────────────────────────────────
  reportConnected     : () => ipcRenderer.send('agent:connected'),
  reportDisconnected  : () => ipcRenderer.send('agent:disconnected'),
  reportSessionStarted: () => ipcRenderer.send('agent:session-started'),
  reportSessionEnded  : () => ipcRenderer.send('agent:session-ended'),

  // ── Receive start signal from main process ───────────────────────────────
  onStart: (callback) => ipcRenderer.on('agent:start', (_, data) => callback(data)),
});
