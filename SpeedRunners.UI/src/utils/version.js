// 浏览器端版本检测工具（纯前端，不依赖 Node.js 模块）

// 本地版本号缓存 key
const storageKey = "currentVersion";
// 首次访问标记 key
const firstVisitKey = "app_first_visit";
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

        // 缓存版本号
        save(serverVersion);

        // 标记已访问
        markVisited();

        // 有新版本且需要自动刷新
        if (isNew && isReload) {
          console.log(`[Version] 检测到新版本: ${serverVersion}，${UPDATE_DELAY / 1000}秒后自动刷新...`);
          setTimeout(() => {
            reload();
          }, UPDATE_DELAY);
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

  const localVersion = localStorage.getItem(storageKey);

  // 首次访问：本地无版本号，保存服务器版本，不刷新
  if (!localVersion || localVersion === "undefined" || localVersion === "null") {
    return false;
  }

  // 对比版本号
  return serverVersion.toString() !== localVersion;
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
  // 添加时间戳强制刷新
  const separator = window.location.href.indexOf("?") > -1 ? "&" : "?";
  window.location.href = window.location.href + separator + "_v=" + Date.now();
}

/**
 * 清除浏览器缓存（保留登录相关数据）
 */
function clearCache() {
  // 保留的 keys（登录态等）
  const preserveKeys = ["token", "userInfo", "locale"];
  const preserved = {};

  // 保存需要保留的数据
  preserveKeys.forEach(key => {
    const value = localStorage.getItem(key);
    if (value) {
      preserved[key] = value;
    }
  });

  // 清除 sessionStorage
  sessionStorage.clear();

  // 恢复保留的数据
  Object.keys(preserved).forEach(key => {
    localStorage.setItem(key, preserved[key]);
  });
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