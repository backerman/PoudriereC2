import { AuthConfigContext } from "@/components/AuthConfigContext";
import { InteractionStatus, InteractionRequiredAuthError } from "@azure/msal-browser";
import { useMsal } from "@azure/msal-react";
import { useContext, useEffect, useState } from "react";

export function useAzureFunctionsOAuth() {
    const authConfig = useContext(AuthConfigContext);
    const [accessToken, setAccessToken] = useState<string | undefined>(undefined);
    const { instance, inProgress, accounts } = useMsal();

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

    return { accessToken, isDevelopment: authConfig.isDevelopment };
}

