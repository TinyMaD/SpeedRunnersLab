// 用于 Node.js 构建时创建版本文件
const fs = require("fs");
const path = require("path");

/**
 * 创建版本文件
 * @param {string} filePath - 文件路径（默认：public/verify.json）
 * @param {number} version - 版本号（默认：当前时间戳）
 * @param {function} result - 回调函数
 */
function create(filePath = "public/verify.json", version = new Date().getTime(), result) {
  // 确保目录存在
  const dir = path.dirname(filePath);
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }

  fs.writeFile(filePath, JSON.stringify({ version }), err => {
    if (result) {
      result({ isOK: !err, error: err });
    }
  });
}

module.exports = { create };
