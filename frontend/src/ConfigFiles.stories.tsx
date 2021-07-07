// import React from 'react';
import { Meta } from '@storybook/react';

import { ConfigFiles } from './ConfigFiles';
import { ConfigFileMetadata } from './model/configs';
import { initializeIcons } from '@fluentui/react/lib/Icons';
initializeIcons();


const sampleList: ConfigFileMetadata[] = [
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

export const Empty = () => <ConfigFiles configFiles={[]} />;
export const Contents = () => <ConfigFiles configFiles={sampleList} />;
export const ContentsWithDeleted = () => <ConfigFiles configFiles={sampleList} showDeleted={true} />;
