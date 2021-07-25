// import React from 'react';
import { Meta } from '@storybook/react';
import { ConfigFiles } from './ConfigFiles';
import { ConfigFileMetadata, ConfigFileRepository, getDataSource } from '../model/configs';
import { initializeIcons } from '@fluentui/react/lib/Icons';
initializeIcons();


let sampleList: ConfigFileMetadata[] = [
  {
    id: 'aa5cd502-eb08-4f42-b187-b81c3d849611',
    deleted: true,
    name: 'this was a test',
    fileType: 'poudriereconf'
  },
  {
    id: 'aa5cd502-eb08-4f42-b187-b81c3d849612',
    deleted: false,
    name: 'this is a test',
    fileType: 'poudriereconf',
    jail: 'jail1',
    portSet: 'set1',
    portsTree: 'tree42'
  }
]

export default {
  title: 'Config/ConfigFiles',
  component: ConfigFiles
} as Meta;

const dataSource = getDataSource(sampleList);
const emptyDataSource = getDataSource([]);

const erroringDataSource : ConfigFileRepository = {
  // EvalError is used here instead of Error to prevent red lines in VSCode.
  getConfigFiles: async function (): Promise<ConfigFileMetadata[]> {
    throw new EvalError('Function not implemented.');
  },
  getConfigFile: function (id: string): Promise<ConfigFileMetadata | undefined> {
      throw new EvalError('Function not implemented.');
  },
  updateConfigFile: function (meta: ConfigFileMetadata): Promise<void> {
      throw new EvalError('Function not implemented.');
  }
}

export const Empty = () => <ConfigFiles dataSource={emptyDataSource} />;
export const Contents = () => <ConfigFiles dataSource={dataSource} />;
export const ContentsWithDeleted = () => <ConfigFiles dataSource={dataSource} showDeleted={true} />;
export const Error = () => <ConfigFiles dataSource={erroringDataSource} />;
