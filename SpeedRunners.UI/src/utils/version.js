// 浏览器端版本检测工具（纯前端，不依赖 Node.js 模块）

// 当前运行代码的版本号：构建时由 vue.config.js 通过 VUE_APP_VERSION 注入，
// 与同一次构建写入的 verify.json 取同一个时间戳。用「编译进 bundle 的版本」
// 与服务器 verify.json 比对，才能真实反映「当前页面跑的代码 vs 线上最新代码」，
// 不再依赖 localStorage（localStorage 记录的是“上次见过的服务器版本”，会与实际运行代码脱钩）
const CURRENT_VERSION = (typeof process !== "undefined" && process.env && process.env.VUE_APP_VERSION) || "";
// 本地版本号缓存 key（仅作调试留痕，不参与新版判断）
const storageKey = "currentVersion";
// 首次访问标记 key
const firstVisitKey = "app_first_visit";
// 防止「刷新后仍是旧代码」导致无限刷新的计数 key（放 sessionStorage，仅本会话有效）
const reloadGuardKey = "version_reload_guard";
// 同一目标版本最多自动刷新次数，超过则停止刷新并告警（兜底 CDN 仍命中旧缓存的情况）
const MAX_RELOAD = 1;
// 更新延迟时间（毫秒）
const UPDATE_DELAY = 3000;

/**
 * 获取版本号（升级版）
 * 检测到新版本后延迟自动刷新
 * @param {string} path - 版本文件路径
 * @param {boolean} isReload - 是否自动刷新（默认true）
 * @returns {Promise<{version: number, new: boolean}>}
 */
function getPro(path = "verify.json", isReload = true) {
  return new Promise((resolve, reject) => {
    get(path)
      .then(res => {
        const serverVersion = res.version;
        const isNew = isNewAvailable(serverVersion);

        // 缓存版本号（仅调试留痕）
        save(serverVersion);

        // 标记已访问
        markVisited();

        if (isNew && isReload) {
          // 仅当本会话尚未为该目标版本反复刷新时才自动刷新，避免 CDN 仍吐旧缓存时无限刷新
          if (canReload(serverVersion)) {
            markReloadAttempt(serverVersion);
            console.log(`[Version] 检测到新版本: ${serverVersion}（当前 ${CURRENT_VERSION}），${UPDATE_DELAY / 1000}秒后自动刷新...`);
            setTimeout(() => {
              reload();
            }, UPDATE_DELAY);
          } else {
            console.warn(`[Version] 已尝试刷新到 ${serverVersion} 但仍在运行旧代码（当前 ${CURRENT_VERSION}），疑似 CDN/边缘缓存未更新，已停止自动刷新以避免死循环。`);
          }
        } else if (!isNew) {
          // 已运行到最新版，清掉刷新计数，为下次发版复位
          clearReloadGuard();
        }

        resolve({ version: serverVersion, new: isNew });
      })
      .catch(err => {
        console.error("[Version] 获取版本号失败:", err);
        reject(err);
      });
  });
}

/**
 * 获取服务器版本号（防 CDN 缓存）
 * @param {string} path - 版本文件路径
 * @param {number} retries - 重试次数
 * @returns {Promise<{version: number}>}
 */
function get(path = "verify.json", retries = 3) {
  return new Promise((resolve, reject) => {
    const url = `${window.location.origin}/${path}?_t=${Date.now()}`;

    const xhr = new XMLHttpRequest();
    xhr.open("GET", url, true);

    // 防 CDN 缓存的请求头
    xhr.setRequestHeader("Cache-Control", "no-cache, no-store, must-revalidate");
    xhr.setRequestHeader("Pragma", "no-cache");
    xhr.setRequestHeader("Expires", "0");

    xhr.onreadystatechange = () => {
      if (xhr.readyState === 4) {
        if (xhr.status === 200) {
          try {
            const data = JSON.parse(xhr.responseText);
            resolve(data);
          } catch (e) {
            reject(new Error("解析版本数据失败"));
          }
        } else {
          // 失败时重试
          if (retries > 0) {
            console.log(`[Version] 请求失败，剩余重试次数: ${retries - 1}`);
            setTimeout(() => {
              get(path, retries - 1).then(resolve).catch(reject);
            }, 1000);
          } else {
            reject(new Error(`获取版本号失败: ${xhr.status}`));
          }
        }
      }
    };

    xhr.onerror = () => {
      if (retries > 0) {
        setTimeout(() => {
          get(path, retries - 1).then(resolve).catch(reject);
        }, 1000);
      } else {
        reject(new Error("网络请求失败"));
      }
    };

    xhr.send();
  });
}

/**
 * 缓存版本号到 localStorage
 * @param {number} version - 版本号
 */
function save(version) {
  if (version) {
    localStorage.setItem(storageKey, version.toString());
  }
}

/**
 * 检查是否有新版本
 * @param {number} serverVersion - 服务器版本号
 * @returns {boolean} true: 有新版本
 */
function isNewAvailable(serverVersion) {
  if (!serverVersion) {
    return false;
  }

  // 开发环境或未注入版本号时（CURRENT_VERSION 为空）不做检测，避免本地误刷
  if (!CURRENT_VERSION) {
    return false;
  }

  // 用「当前运行代码的版本」与服务器版本比对：不一致即说明本页面是旧代码
  return serverVersion.toString() !== CURRENT_VERSION.toString();
}

/**
 * 标记用户已访问过
 */
function markVisited() {
  localStorage.setItem(firstVisitKey, "true");
}

/**
 * 刷新页面（强制从服务器获取最新资源）
 */
function reload() {
  // 清除缓存后刷新
  clearCache();
  // 使用 location.reload() 触发标准刷新，配合 nginx 对 index.html 的 no-cache 头
  // 可保证每次都从源站拿最新 HTML，避免带 query 参数跳转留下的 _v 残留
  window.location.reload();
}

/**
 * 清除会话缓存（登录态在 localStorage，不受影响；保留防死循环刷新计数）
 */
function clearCache() {
  // 防死循环计数需跨 reload 存活，clear 前后手动保留
  const guard = sessionStorage.getItem(reloadGuardKey);
  sessionStorage.clear();
  if (guard !== null) {
    sessionStorage.setItem(reloadGuardKey, guard);
  }
}

/**
 * 读取防死循环刷新计数（sessionStorage）
 * @returns {{version?: string, count?: number}}
 */
function readGuard() {
  try {
    return JSON.parse(sessionStorage.getItem(reloadGuardKey)) || {};
  } catch (e) {
    return {};
  }
}

/**
 * 是否允许为该目标版本自动刷新（同一目标版本超过 MAX_RELOAD 次则拒绝，防止死循环）
 * @param {number|string} serverVersion
 * @returns {boolean}
 */
function canReload(serverVersion) {
  const guard = readGuard();
  // 目标版本变了，说明又发了新版，重新允许刷新
  if (guard.version !== serverVersion.toString()) {
    return true;
  }
  return (guard.count || 0) < MAX_RELOAD;
}

/**
 * 记录一次针对某目标版本的刷新尝试
 * @param {number|string} serverVersion
 */
function markReloadAttempt(serverVersion) {
  const guard = readGuard();
  const version = serverVersion.toString();
  const count = guard.version === version ? (guard.count || 0) + 1 : 1;
  try {
    sessionStorage.setItem(reloadGuardKey, JSON.stringify({ version, count }));
  } catch (e) {
    // sessionStorage 不可用时忽略，最坏退化为无防护
  }
}

/**
 * 清除刷新计数（已运行到最新版时调用）
 */
function clearReloadGuard() {
  try {
    sessionStorage.removeItem(reloadGuardKey);
  } catch (e) {
    // ignore
  }
}

/**
 * 手动检查更新（供外部调用）
 * @returns {Promise<boolean>} true: 有新版本并已触发刷新
 */
function checkUpdate() {
  return getPro("verify.json", true).then(res => res.new);
}

// 导出对象
const versionAPI = {
  get,
  getPro,
  save,
  isNewAvailable,
  reload,
  checkUpdate
};

export default versionAPI;
export { get, getPro, save, isNewAvailable, reload, checkUpdate };