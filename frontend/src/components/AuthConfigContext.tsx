import { createContext } from 'react';

export interface AuthConfig {
    isDevelopment: boolean;
    functionsScope: string;
}

export const AuthConfigContext = createContext<AuthConfig>({} as AuthConfig);
