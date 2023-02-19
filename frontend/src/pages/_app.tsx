// import '@/styles/globals.css'
import { Configuration, InteractionType, PublicClientApplication } from '@azure/msal-browser';
import { MsalAuthenticationResult, MsalAuthenticationTemplate, MsalProvider } from '@azure/msal-react';
import { initializeIcons, ThemeProvider } from '@fluentui/react';
import type { AppProps } from 'next/app';
import Layout from '../components/Layout';

initializeIcons();

const msalConfig: Configuration = {
  auth: {
    clientId: process.env.NEXT_PUBLIC_AAD_CLIENT_ID || 'this space intentionally left blank',
    authority: `https://login.microsoftonline.com/${process.env.NEXT_PUBLIC_AAD_TENANT_ID}`,
  }
}

export const msalInstance = new PublicClientApplication(msalConfig);

const AuthError = ({ error }: MsalAuthenticationResult) => {
  return (<div>
    <p>An error occurred: {error?.message}</p>
  </div>);
}

const AuthInProgress = () => {
  return (<div>
    <p>Authenticating...</p>
  </div>);
};

export default function App({ Component, pageProps }: AppProps) {
  return (
    <MsalProvider instance={msalInstance}>
      <ThemeProvider>
        <Layout>
          <MsalAuthenticationTemplate
            interactionType={InteractionType.Popup}
            errorComponent={AuthError}
            loadingComponent={AuthInProgress}>
            <Component {...pageProps} />
          </MsalAuthenticationTemplate>
        </Layout>
      </ThemeProvider>
    </MsalProvider>
  );

}
