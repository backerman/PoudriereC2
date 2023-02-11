import { PortSets } from './PortSets';
import { getDataSource, PortSet, PortSetRepository } from 'src/models/portsets';
import { getSampleDataSource } from 'src/models/portsets.sample';
import { initializeIcons } from '@fluentui/react/lib/Icons';
import { StoryObj } from '@storybook/react';

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

export default {
    component: PortSets,
} as StoryObj<typeof PortSets>;

export const Empty : StoryObj<typeof PortSets> = {
    name: 'Empty',
    render: () => <PortSets dataSource={emptyDataSource} />
}

export const Populated : StoryObj<typeof PortSets> = {
    name: 'Populated',
    render: () => <PortSets dataSource={dataSource} />
}

export const Erroring : StoryObj<typeof PortSets> = {
    name: 'Erroring',
    render: () => <PortSets dataSource={erroringDataSource} />
}