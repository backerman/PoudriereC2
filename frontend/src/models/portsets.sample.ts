import { getDataSource, PortSet, PortSetRepository } from "./portsets"

const sampleList: PortSet[] = [
    {
        id: "3bb71b44-869a-493c-aa82-aaf9f337fc50",
        name: "Yog-Sothoth",
        origins: [
            "devel/binutils",
            "lang/python38",
            "net-mgmt/zabbix5-frontend",
            "sysutils/py-azure-cli",
            "textproc/bat"
        ]
    }
]

const erroringDataSource : PortSetRepository = {
    getPortSets: async function (): Promise<PortSet[]> {
        throw new Error('Function not implemented.');
    },
    getPortSet: function (id: string): Promise<PortSet | undefined> {
        throw new Error('Function not implemented.');
    },
    updatePortSet: function (id: string, packageSet: PortSet): Promise<void> {
        throw new Error('Function not implemented.');
    }
}

export function getSampleDataSource() {
    return getDataSource(sampleList);
}

export function getErroringDataSource() {
    return erroringDataSource;
}