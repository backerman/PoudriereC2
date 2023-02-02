import { getDataSource, PortSet } from "./portsets"

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

export function getSampleDataSource() {
    return getDataSource(sampleList);
}