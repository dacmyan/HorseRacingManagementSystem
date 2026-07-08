import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'https://hrms-backend-a4dwfmgmgfagf7ax.southeastasia-01.azurewebsites.net',
        changeOrigin: true,
      },
      '/hubs': {
        target: 'https://hrms-backend-a4dwfmgmgfagf7ax.southeastasia-01.azurewebsites.net',
        ws: true,
        changeOrigin: true,
      },
    },
  },
})
