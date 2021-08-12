import { ConfigFileMetadata, ConfigFileRepository, getDataSource } from '../model/configs';
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

const dataSource : ConfigFileRepository = getDataSource(sampleList);

const Sample = () => {
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

export default Sample;
