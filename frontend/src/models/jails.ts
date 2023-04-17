export type JailMethod =
    | 'allbsd'
    | 'ftp'
    | 'ftpArchive'
    | 'http'
    | 'freebsdci'
    | 'url'

export type Jail = {
    id?: string;
    name: string;
    version: string;
    architecture?: string;
    method?: string;
    url?: string;
}
