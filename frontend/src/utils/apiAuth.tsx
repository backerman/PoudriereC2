import { AuthConfigContext } from "@/components/AuthConfigContext";
import { InteractionStatus, InteractionRequiredAuthError } from "@azure/msal-browser";
import { useMsal } from "@azure/msal-react";
import axios from "axios";
import { useContext, useEffect, useState } from "react";
import { makeFetcher } from "./fetcher";

export function useAzureFunctionsOAuth() {
    const authConfig = useContext(AuthConfigContext);
    const [accessToken, setAccessToken] = useState<string | undefined>(undefined);
    const { instance, inProgress, accounts } = useMsal();
    const axiosInstance = axios.create();
    const fetcher = makeFetcher(axiosInstance);
    axiosInstance.interceptors.request.use((config) => {
        if (!authConfig.isDevelopment) {
            // If not in development, add the access token if available. If not, return null.
            if (accessToken) {
                config.headers.Authorization = `Bearer ${accessToken}`;
            } else {
                throw new axios.Cancel('No access token provided');
            }
        }
        return config;
    });
    const keyIfTokenReady = (key: string) =>
        () => {
            if (authConfig.isDevelopment || accessToken) {
                return key;
            }
            return null;
        };

    useEffect(() => {
        const accessTokenRequest = {
            scopes: ["profile", authConfig.functionsScope],
            account: accounts[0]
        };
        if (!authConfig.isDevelopment && !accessToken && inProgress === InteractionStatus.None) {
            instance
                .acquireTokenSilent(accessTokenRequest)
                .then((accessTokenResponse) => {
                    setAccessToken(accessTokenResponse.accessToken);
                })
                .catch((error) => {
                    if (error instanceof InteractionRequiredAuthError) {
                        instance.acquireTokenRedirect(accessTokenRequest);
                    }
                    console.log(error);
                });
        }
    }, [instance, accounts, inProgress, accessToken, authConfig.functionsScope, authConfig.isDevelopment]);

    return {
        /// Bearer token to pass to Azure Functions
        accessToken,
        /// Axios instance with the bearer token set as a header
        axios: axiosInstance,
        /// Fetcher function to pass to SWR
        fetcher,
        /// Whether or not the app is running in development mode
        isDevelopment: authConfig.isDevelopment,
        /// Function to wrap key in call to SWR; will return null if waiting for token
        keyIfTokenReady
    };
}
