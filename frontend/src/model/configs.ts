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

export type { ConfigFileMetadata }