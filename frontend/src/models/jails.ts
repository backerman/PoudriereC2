export type JailMethodInfo = {
    name: string;
    type: 'source' | 'binary' | 'either';
    requiresParameter: 'path' | 'url' | 'none';
}

export const JailMethods: JailMethodInfo[] = [
    { name: 'allbsd', type: 'binary', requiresParameter: 'none' },
    { name: 'freebsd-ci', type: 'binary', requiresParameter: 'none' },
    { name: 'ftp', type: 'binary', requiresParameter: 'none' },
    { name: 'ftp-archive', type: 'binary', requiresParameter: 'none' },
    { name: 'git', type: 'source', requiresParameter: 'url' },
    { name: 'http', type: 'binary', requiresParameter: 'none' },
    { name: 'null', type: 'binary', requiresParameter: 'none' },
    { name: 'src', type: 'either', requiresParameter: 'path' },
    { name: 'svn', type: 'source', requiresParameter: 'url' },
    { name: 'tar', type: 'binary', requiresParameter: 'path' },
    { name: 'url', type: 'either', requiresParameter: 'url' },
]

export type Jail = {
    id?: string;
    name: string;
    portableName: string;
    version?: string;
    architecture?: string;
    method?: string;
    url?: string;
    path?: string;
}
