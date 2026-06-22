// const SERVER_URL = "http://localhost:5293";
// const SERVER_URL = "https://localhost:7161";
// const SERVER_URL = "https://216.24.57.9";
const SERVER_URL = "https://api.flowdesk.uttamprajapati.me";

const serverConfig = Object.freeze({
  serverUrl: SERVER_URL,
  apiUrl: `${SERVER_URL}/api`,
  hubUrl: `${SERVER_URL}/hubs/request`,
});

module.exports = Object.freeze({
  serverConfig,
});
