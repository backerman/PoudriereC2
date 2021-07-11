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
    getConfigFiles: () => ConfigFileMetadata[]
    getConfigFile: (id: string) => ConfigFileMetadata | null
    updateConfigFile: (meta: ConfigFileMetadata) => void
}

export type { ConfigFileMetadata, ConfigFileRepository }