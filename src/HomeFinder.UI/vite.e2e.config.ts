import { fileURLToPath, URL } from 'node:url';
import { defineConfig, mergeConfig } from 'vitest/config';
import appConfig from './vite.config.ts';

// 通常ビルドの設定や実行時環境変数からはテスト用認証を選択できない。
export default mergeConfig(appConfig, defineConfig({
  plugins: [{
    name: 'e2e-runtime-config',
    transformIndexHtml(html) {
      // 通常の public/env.js に含まれる開発用 API URL で上書きさせない。
      return html.replace(
        '<script src="/env.js"></script>',
        '<script>window.__APP_CONFIG__ = { VITE_API_BASE_URL: "http://127.0.0.1:5017" };</script>',
      );
    },
  }],
  resolve: {
    alias: {
      '@auth-provider': fileURLToPath(new URL('./tests/e2e/support/e2eAuthProvider.ts', import.meta.url)),
    },
  },
  build: {
    outDir: 'dist-e2e',
  },
}));
