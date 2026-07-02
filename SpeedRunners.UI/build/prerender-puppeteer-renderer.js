const PuppeteerRenderer = require("@prerenderer/renderer-puppeteer");

class HeaderPuppeteerRenderer extends PuppeteerRenderer {
  async handleRequestInterception(page, baseURL) {
    await page.setRequestInterception(true);

    page.on("request", req => {
      if (this._rendererOptions.skipThirdPartyRequests && !req.url().startsWith(baseURL)) {
        req.abort();
        return;
      }

      const headers = Object.assign({}, req.headers());
      const extraHeaders = this._rendererOptions.headers || {};
      Object.keys(extraHeaders).forEach(name => {
        headers[name.toLowerCase()] = extraHeaders[name];
      });

      req.continue({ headers });
    });
  }
}

module.exports = HeaderPuppeteerRenderer;
