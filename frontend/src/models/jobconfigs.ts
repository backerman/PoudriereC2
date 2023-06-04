export interface JobConfig {
    id?: string,
    deleted: boolean,
    name: string,
    poudriereConf?: string,
    poudriereConfName?: string,
    portSet?: string,
    portSetName?: string,
    portsTree?: string,
    portsTreeName?: string,
    jail?: string,
    jailName?: string
}
