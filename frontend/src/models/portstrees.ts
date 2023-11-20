export type PortsTreeMethod = 'git' | 'svn' | 'null';

export type PortsTree = {
    id?: string;
    name: string;
    portableName: string;
    method: PortsTreeMethod;
    url?: string;
}
