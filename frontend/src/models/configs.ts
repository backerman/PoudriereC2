// Configuration types

export interface ConfigFileMetadata {
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

export interface ConfigOption {
    name: string,
    value: string
}

export type ConfigOptionUpdate = {
    action: 'add',
    options: ConfigOption[]
} | {
    action: 'delete',
    options: string[]
}
