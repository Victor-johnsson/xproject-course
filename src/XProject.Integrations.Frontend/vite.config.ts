import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

// export let serverPort: number
// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
    const env = loadEnv(mode, process.cwd(), '');

    // serverPort = parseInt(env.VITE_PORT)
    console.log('backend http', process.env.YARP_HTTP)
    console.log('backend https', process.env.YARP_HTTPS)
    return {
        plugins: [react()],
        server: {
            port: parseInt(env.VITE_PORT),
             proxy: {
                 '/api': {
                    target: process.env.YARP_HTTPS || process.env.YARP_HTTP,
                    // target: process.env.BACKEND_HTTPS || process.env.BACKEND_HTTP,
                     changeOrigin: true,
                     secure: false,
                 }
             }
        },
        build: {
            outDir: 'dist',
            rollupOptions: {
                input: './index.html'
            }
        }
    }
})

