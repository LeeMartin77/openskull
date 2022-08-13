import { defineConfig } from 'vite';
import { svelte } from '@sveltejs/vite-plugin-svelte';
// This is annoying, we can just ignore it though
// @ts-ignore ts(1259)
import path from 'node:path';

const proxy = {
  '/api': {
       target: 'http://localhost:5248',
       changeOrigin: true,
       secure: false,      
       ws: true,
   }
}

// https://vitejs.dev/config/
export default defineConfig({
  preview: {
    port: 3000
  },
  server: {
    port: 3000,
    proxy
  },
  resolve: {
    alias: {
      src: path.resolve('./src')
    }
  },
  plugins: [svelte()]
});
