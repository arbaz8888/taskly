import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/auth': 'http://localhost:5013',
      '/tasks': 'http://localhost:5013',
      '/hello': 'http://localhost:5013'
    }
  },
  test: {
    environment: 'jsdom',
    setupFiles: './src/setupTests.ts',
    globals: true // ok to keep; makes describe/it global if you want
  }
})
