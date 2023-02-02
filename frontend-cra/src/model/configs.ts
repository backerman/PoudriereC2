// Configuration types


type ConfigFileMetadata = {
    id: string,
    deleted: boolean,
    name: string,
    portSet?: string,
    portsTree?: string,
    jail?: string,
    fileType: string
}

interface ConfigFileRepository {
    getConfigFiles: () => Promise<ConfigFileMetadata[]>
    getConfigFile: (id: string) => Promise<ConfigFileMetadata | undefined>
    updateConfigFile: (meta: ConfigFileMetadata) => Promise<void>
}

/// List-backed configuration source for testing.
export function getDataSource(data: ConfigFileMetadata[]): ConfigFileRepository {
    return {
        getConfigFiles: async () => data,
        getConfigFile: async (id: string) => data.find(f => f.id === id),
        updateConfigFile: async (meta: ConfigFileMetadata) => {
            const index = data.findIndex(f => f.id === meta.id);
            if (index !== -1) {
                data[index] = meta;
            }
        }
    };
}

export type { ConfigFileMetadata, ConfigFileRepository }