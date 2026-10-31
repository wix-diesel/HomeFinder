import { defineConfig } from 'vitest/config'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@auth-provider': fileURLToPath(new URL('./src/services/auth/msalAuthProvider.ts', import.meta.url)),
    },
  },
  build: {
    // バンドル対象の依存関係からライセンス情報を収集し、アプリ内で表示できる JSON を出力する。
    license: {
      fileName: 'licenses.json',
    },
  },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: ['./tests/setup.ts'],
    include: ['tests/unit/**/*.{test,spec}.ts'],
  },
})
