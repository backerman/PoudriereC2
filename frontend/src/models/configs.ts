// Configuration types

type ConfigFileMetadata = {
    id?: string,
    deleted: boolean,
    name: string,
    portSet?: string,
    portSetName?: string,
    portsTree?: string,
    portsTreeName?: string,
    jail?: string,
    jailName?: string,
    fileType: string
}

export type { ConfigFileMetadata }
