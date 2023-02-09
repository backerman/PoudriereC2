import { PortSets } from './PortSets';
import { getDataSource, PortSet, PortSetRepository } from '../model/portsets';
import { getSampleDataSource } from '../model/portsets.sample';
import { initializeIcons } from '@fluentui/react/lib/Icons';

initializeIcons();

const dataSource = getSampleDataSource();
const emptyDataSource = getDataSource([]);

function sleep(time: number) {
    return new Promise((resolve) => setTimeout(resolve, time))
}

const erroringDataSource : PortSetRepository = {
    getPortSets: async function (): Promise<PortSet[]> {
        await sleep(1000);
        throw new EvalError('Function not implemented.');
    },
    getPortSet: async function (id: string): Promise<PortSet | undefined> {
        throw new EvalError('Function not implemented.');
    },
    updatePortSet: async function (id: string, packageSet: PortSet): Promise<void> {
        throw new EvalError('Function not implemented.');
    }
}

// eslint-disable-next-line import/no-anonymous-default-export
export default {
    empty: <PortSets dataSource={emptyDataSource} />,
    contents: <PortSets dataSource={dataSource} />,
    error: <PortSets dataSource={erroringDataSource} />
}