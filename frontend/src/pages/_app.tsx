// import '@/styles/globals.css'
import { Configuration, InteractionType, PublicClientApplication } from '@azure/msal-browser';
import { MsalAuthenticationResult, MsalAuthenticationTemplate, MsalProvider } from '@azure/msal-react';
import { createTheme, FontSizes, IDetailsRowStyles, initializeIcons, ThemeProvider } from '@fluentui/react';
import type { AppProps } from 'next/app';
import Layout from '../components/Layout';
import { AuthConfigContext } from '@/components/AuthConfigContext';

initializeIcons();

const apiScope = process.env.NEXT_PUBLIC_API_SCOPE || 'scope not configured';
const isDevelopment = process.env.NEXT_PUBLIC_IS_DEVELOPMENT ?
  ['true', 'yes'].includes(process.env.NEXT_PUBLIC_IS_DEVELOPMENT.toLowerCase()) :
  false;

const msalConfig: Configuration = {
  auth: {
    clientId: process.env.NEXT_PUBLIC_AAD_CLIENT_ID || 'this space intentionally left blank',
    authority: `https://login.microsoftonline.com/${process.env.NEXT_PUBLIC_AAD_TENANT_ID}`
  },
  cache: {
    cacheLocation: 'localStorage',
    storeAuthStateInCookie: false
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

// Make a theme to fix font sizes.
const detailsRowStyles: Partial<IDetailsRowStyles> = {
  cell: {
    fontSize: FontSizes.size14
  },
  isRowHeader: {
    fontSize: FontSizes.size14
  }
}
const myTheme = createTheme({
  components: {
    DetailsRow: {
      styles: detailsRowStyles
    }
  }
});

export default function App({ Component, pageProps }: AppProps) {
  return (
    <MsalProvider instance={msalInstance}>
      <AuthConfigContext.Provider value={{
        isDevelopment: isDevelopment,
        functionsScope: apiScope
      }}>
        <ThemeProvider theme={myTheme}>
          <Layout>
            <MsalAuthenticationTemplate
              interactionType={InteractionType.Popup}
              authenticationRequest={{
                scopes: ['profile'],
                extraScopesToConsent: [apiScope]
              }}
              errorComponent={AuthError}
              loadingComponent={AuthInProgress}>
              <Component {...pageProps} />
            </MsalAuthenticationTemplate>
          </Layout>
        </ThemeProvider>
      </AuthConfigContext.Provider>
    </MsalProvider>
  );
}
