import { act, render } from '@testing-library/react';
import { ConfigFileEditor } from './ConfigFileEditor';
import { initializeIcons } from '@fluentui/react';
import { getSampleDataSource } from './model/configs.sample';


const dataSource = getSampleDataSource();

initializeIcons();

it('renders editor successfully', async () => {
    const recordToEdit = (await dataSource.getConfigFiles())[0].id;
    await act(async () => {
        render(<ConfigFileEditor
            isOpen={true}
            recordId={recordToEdit}
            dataSource={dataSource}
            onDismiss={() => { return }}
            onSubmit={() => { return }} />);
    });
});
