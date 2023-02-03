// import '@/styles/globals.css'
import { initializeIcons, ThemeProvider } from '@fluentui/react';
import type { AppProps } from 'next/app'
import Layout from '../components/Layout'

initializeIcons();

export default function App({ Component, pageProps }: AppProps) {
  return (
    <ThemeProvider>
      <Layout>
        <Component {...pageProps} />
      </Layout>
    </ThemeProvider>
  );

}
