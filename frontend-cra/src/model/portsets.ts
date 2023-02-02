
export type PortSet = {
    id: string;
    name: string;
    origins: string[];
}

export interface PortSetRepository {
    getPortSets: () => Promise<PortSet[]>
    getPortSet: (id: string) => Promise<PortSet | undefined>
    // addPackage: (id: string, origin: string) => Promise<void>
    // removePackage: (id: string, origin: string) => Promise<void>
    updatePortSet: (id: string, packageSet: PortSet) => Promise<void>
}

export function getDataSource(data: PortSet[]): PortSetRepository {
    return {
        getPortSets: async () => data,
        getPortSet: async (id) => data.find((ps) => ps.id === id),
        updatePortSet: async (id, pset) => {
            const index = data.findIndex(ps => ps.id === id);
            if (index !== -1) {
                data[index] = pset;
                // force a re-render
                data = data.slice();
            }
        }
    }
}