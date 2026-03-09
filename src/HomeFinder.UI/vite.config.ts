import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  server: {
    proxy: {
      '/Items': 'http://localhost:5221',
      '/Areas': 'http://localhost:5221',
      '/api': 'http://localhost:5221',
    },
  },
})
