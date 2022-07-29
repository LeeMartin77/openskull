import { defineConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'

// https://vitejs.dev/config/
export default defineConfig({
  preview: {
    port: 3000
  },
  server: {
    port: 3000
  },
  plugins: [svelte()]
})
