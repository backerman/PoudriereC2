import { act, render } from '@testing-library/react';
import { ConfigFileEditor } from './ConfigFileEditor';
import { initializeIcons } from '@fluentui/react';
import { sampleData } from 'src/models/configs.sample';

initializeIcons();

it('renders editor successfully', async () => {
    const recordToEdit = sampleData[0].id;
    await act(async () => {
        render(<ConfigFileEditor
            isOpen={true}
            record={sampleData[0]}
            recordId={recordToEdit}
            onDismiss={() => { return }}
            onSubmit={() => { return }} />);
    });
});
