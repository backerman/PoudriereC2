// import React from 'react';
import { Meta } from '@storybook/react';

import { ConfigFiles } from './ConfigFiles';
import { ConfigFileMetadata, ConfigFileRepository } from './model/configs';
import { initializeIcons } from '@fluentui/react/lib/Icons';
import { ConfigFileEditor } from './ConfigFileEditor';
import { useBoolean } from "@fluentui/react-hooks";
import { DefaultButton } from '@fluentui/react';

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

const dataSource: ConfigFileRepository = {
  getConfigFiles: function (): ConfigFileMetadata[] {
    return sampleList;
  },
  getConfigFile: function (id: string): ConfigFileMetadata | null {
    const matching = sampleList.filter((f) => f.id === id);
    if (matching.length > 0)
      return matching[0]
    else
      return null
  },
  updateConfigFile: function (meta: ConfigFileMetadata): void {
    throw new Error('Function not implemented.');
  }
}

export default {
  title: 'Config/ConfigFileEditor',
  component: ConfigFiles
} as Meta;

export const Sample = () => {
  const [ isOpen, {setTrue: openPanel, setFalse: dismissPanel } ] = useBoolean(false);
  return (
    
    <div>
      <DefaultButton onClick={openPanel}>Open the editor</DefaultButton>
      <ConfigFileEditor
        isOpen={isOpen}
        dataSource={dataSource}
        onSubmit={(cfm) => { console.log(cfm) }}
        onDismiss={dismissPanel}
        recordId={sampleList[0].id} />
    </div>);
}
