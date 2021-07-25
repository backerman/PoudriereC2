// import React from 'react';
import { Meta } from '@storybook/react';
import { PortSets } from './PortSets';
import { getDataSource, PortSet, PortSetRepository } from '../model/portsets';
import { getSampleDataSource } from '../model/portsets.sample'
import { initializeIcons } from '@fluentui/react/lib/Icons';
initializeIcons();


export default {
  title: 'Config/PortSets',
  component: PortSets
} as Meta;

const dataSource = getSampleDataSource();
const emptyDataSource = getDataSource([]);

function sleep(time: number){
    return new Promise((resolve)=>setTimeout(resolve,time)
  )
}

const erroringDataSource : PortSetRepository = {
    getPortSets: async function (): Promise<PortSet[]> {
        await sleep(1000);
        throw new EvalError('Function not implemented.');
    },
    getPortSet: async function (id: string): Promise<PortSet | undefined> {
        throw new EvalError('Function not implemented.');
    },
    setPortSet: async function (id: string, packageSet: PortSet): Promise<void> {
        throw new EvalError('Function not implemented.');
    }
}

export const Empty = () => <PortSets dataSource={emptyDataSource} />;
export const Contents = () => <PortSets dataSource={dataSource} />;
export const Error = () => <PortSets dataSource={erroringDataSource} />;
