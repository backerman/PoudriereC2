// Load fake authentication info from local storage

import { AuthenticationResult, AuthenticationScheme, AzureCloudInstance, BrowserConfiguration, ExternalTokenResponse, INavigationClient, INetworkModule, IPublicClientApplication, Logger, LogLevel, ProtocolMode } from '@azure/msal-browser';
import { ITokenCache } from '@azure/msal-browser/dist/cache/ITokenCache';

const fakeAuthenticationResult: AuthenticationResult = {
    // All GUIDs are fake to protect the guilty
    authority: 'https://login.microsoftonline.com/27ec19dd-88e4-4625-8aa6-38c3cd6c468e',
    uniqueId: 'a60f0049-2d09-4757-a94b-dc9d86efe8b8',
    tenantId: '27ec19dd-88e4-4625-8aa6-38c3cd6c468e',
    scopes: ['user.read'],
    account: {
        'homeAccountId': '27ec19dd-88e4-4625-8aa6-38c3cd6c468e.3a74f1fe-841a-468b-845c-10e65df01a9b',
        'environment': 'login.windows.net',
        'tenantId': '27ec19dd-88e4-4625-8aa6-38c3cd6c468e',
        'username': 'robert.ford@example.com',
        'localAccountId': '879bdbd4-7e35-4a3f-8d51-1558c82f365f',
        'name': 'Robert Ford'
    },
    idToken: 'token-goes-here',
    accessToken: 'token-goes-here',
    idTokenClaims: {},
    fromCache: false,
    expiresOn: null,
    tokenType: 'Bearer',
    correlationId: '2a0012ef-a0a7-4308-8094-35aa900f6a91'
};

const fakeTokenCache: ITokenCache = {
    loadExternalTokens: () => fakeAuthenticationResult
};

const fakeLogger = new Logger({
    loggerCallback: (level: LogLevel, message: string, containsPii: boolean) => {
        console.log(`[MSAL] ${level} (PII ${containsPii}): ${message}`)
    },
});

const fakeBrowserConfiguration: BrowserConfiguration = {
    auth: {
        clientId: 'fake-client-id',
        authority: 'https://login.microsoftonline.com/27ec19dd-88e4-4625-8aa6-38c3cd6c468e',
        knownAuthorities: ['login.microsoftonline.com'],
        redirectUri: 'http://localhost:3000',
        navigateToLoginRequestUrl: true,
        clientCapabilities: [],
        authorityMetadata: '',
        postLogoutRedirectUri: '',
        azureCloudOptions: {
            azureCloudInstance: AzureCloudInstance.AzurePublic
        },
        protocolMode: ProtocolMode.AAD,
        cloudDiscoveryMetadata: '',
        skipAuthorityMetadataCache: false
    },
    cache: {
        cacheLocation: 'localStorage',
        storeAuthStateInCookie: false,
        secureCookies: true
    },
    system: {
        allowNativeBroker: false,
        allowRedirectInIframe: false,
        iframeHashTimeout: 1000,
        asyncPopups: false,
        cryptoOptions: { },
        loadFrameTimeout: 6000,
        loggerOptions: {},
        nativeBrokerHandshakeTimeout: 1000,
        navigateFrameWait: 0,
        navigationClient: {} as INavigationClient,
        networkClient: {} as INetworkModule,
        pollIntervalMilliseconds: 1000,
        preventCorsPreflight: false,
        redirectNavigationTimeout: 1000,
        tokenRenewalOffsetSeconds: 300,
        windowHashTimeout: 1000
    },
    telemetry: {
        application: {
            appName: 'Fakey McFakeface',
            appVersion: '1.0.0'
        }
    }
}

const MockMsalInstance: IPublicClientApplication = {
    initialize: () => Promise.resolve(),
    acquireTokenPopup: () => Promise.resolve(fakeAuthenticationResult),
    acquireTokenRedirect: () => Promise.resolve(),
    acquireTokenSilent: () => Promise.resolve(fakeAuthenticationResult),
    acquireTokenByCode: () => Promise.resolve(fakeAuthenticationResult),
    addEventCallback: () => '',
    removeEventCallback: () => { },
    addPerformanceCallback: () => '',
    removePerformanceCallback: () => true,
    enableAccountStorageEvents: () => Promise.resolve(),
    disableAccountStorageEvents: () => Promise.resolve(),
    getAccountByHomeId: () => null,
    getAccountByLocalId: () => null,
    getAccountByUsername: () => null,
    getAllAccounts: () => [fakeAuthenticationResult.account!],
    handleRedirectPromise: () => Promise.resolve(fakeAuthenticationResult),
    loginPopup: () => Promise.resolve(fakeAuthenticationResult),
    loginRedirect: () => Promise.resolve(),
    logout: () => Promise.resolve(),
    logoutRedirect: () => Promise.resolve(),
    logoutPopup: () => Promise.resolve(),
    ssoSilent: () => Promise.resolve(fakeAuthenticationResult),
    getTokenCache: () => fakeTokenCache,
    getLogger: () => fakeLogger,
    setLogger: () => { },
    setActiveAccount: () => { },
    getActiveAccount: () => fakeAuthenticationResult.account,
    initializeWrapperLibrary: () => { },
    setNavigationClient: () => { },
    getConfiguration: () => fakeBrowserConfiguration
}

export { MockMsalInstance };
