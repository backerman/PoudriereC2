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

export type { ConfigFileMetadata, ConfigFileRepository }