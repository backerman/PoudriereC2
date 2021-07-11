import React from 'react';
import { act, render } from '@testing-library/react';
import { ConfigFileEditor } from './ConfigFileEditor';
import { ConfigFileMetadata, getDataSource } from './model/configs';
import { initializeIcons } from '@fluentui/react';

let sampleList: ConfigFileMetadata[] = [
    {
        id: 'aa5cd502-eb08-4f42-b187-b81c3d849612',
        deleted: false,
        name: 'this is a test',
        fileType: 'poudriereconf',
        jail: 'jail1',
        portSet: 'set1',
        portsTree: 'tree42'
    }
];

const dataSource = getDataSource(sampleList);

initializeIcons();

it('renders editor successfully', async () => {
    await act(async () => {
        render(<ConfigFileEditor
            isOpen={true}
            recordId={sampleList[0].id}
            dataSource={dataSource}
            onDismiss={() => { return }}
            onSubmit={() => { return }} />);
    });
});
