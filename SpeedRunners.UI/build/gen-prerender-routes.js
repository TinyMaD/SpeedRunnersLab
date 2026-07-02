const fs = require("fs");
const path = require("path");
const axios = require("axios");

const API = process.env.PRERENDER_API || "https://api.speedrunners.cn/api";
const TOP_N = Number(process.env.PRERENDER_TOP_N || 100);
const modTagsConfig = process.env.PRERENDER_MOD_TAGS == null ? "0,1,2,3,4,5,6" : process.env.PRERENDER_MOD_TAGS;
const MOD_TAGS = modTagsConfig.trim()
  ? modTagsConfig.split(",")
    .map(x => Number(x.trim()))
    .filter(x => !Number.isNaN(x))
  : [];
const HEADERS = { Origin: "https://www.speedrunners.cn" };
const proxyAgent = createProxyAgent();

function createProxyAgent() {
  const proxyUrl = process.env.HTTPS_PROXY || process.env.HTTP_PROXY || process.env.https_proxy || process.env.http_proxy;
  if (!proxyUrl) return null;
  try {
    const proxyAgentModule = require("https-proxy-agent");
    const HttpsProxyAgent = proxyAgentModule.HttpsProxyAgent || proxyAgentModule;
    return new HttpsProxyAgent(proxyUrl);
  } catch (error) {
    console.warn("[prerender] Failed to enable proxy agent:", error.message);
    return null;
  }
}

function requestOptions(extra) {
  const options = Object.assign({
    headers: HEADERS,
    timeout: 30000
  }, extra || {});
  if (proxyAgent) {
    options.proxy = false;
    options.httpAgent = proxyAgent;
    options.httpsAgent = proxyAgent;
  }
  return options;
}

function addRoute(routes, route) {
  if (route && !routes.includes(route)) {
    routes.push(route);
  }
}

(async() => {
  const routes = ["/", "/rank", "/mod", "/match", "/log", "/searchplayer"];

  try {
    const response = await axios.get(`${API}/rank/getRankList`, requestOptions());
    const players = response.data && response.data.data ? response.data.data : [];
    players.slice(0, TOP_N).forEach(player => {
      addRoute(routes, `/profile/${player.platformID}`);
    });
  } catch (error) {
    console.warn("[prerender] Failed to load rank list, profile routes skipped:", error.message);
  }

  for (const tag of MOD_TAGS) {
    try {
      const response = await axios.post(
        `${API}/asset/getModList`,
        { tag, pageNo: 1, pageSize: 10000 },
        requestOptions()
      );
      const data = response.data && response.data.data;
      const mods = data && data.list ? data.list : [];
      mods.forEach(mod => {
        addRoute(routes, `/mod/${mod.id}`);
      });
    } catch (error) {
      console.warn(`[prerender] Failed to load mod tag ${tag}, routes skipped:`, error.message);
    }
  }

  fs.writeFileSync(path.join(__dirname, "prerender-routes.json"), JSON.stringify(routes, null, 2));
  console.log(`[prerender] Route count: ${routes.length}`);
})();
