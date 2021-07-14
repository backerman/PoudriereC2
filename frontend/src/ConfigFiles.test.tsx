import { act, render, screen } from '@testing-library/react';
import { initializeIcons } from '@fluentui/react';
import { getSampleDataSource } from './model/configs.sample';
import { ConfigFiles } from './ConfigFiles';
import { getDataSource } from './model/configs';

initializeIcons();

it('renders editor successfully', async () => {
    const sampleData = getDataSource([]);

    await act(async () => {
        render(<ConfigFiles dataSource={sampleData}/>);
    });
    // expect(screen.getAllByRole("columnHeader")).toHaveTextContent("Ports tree");
    // expect(screen.getAllByRole("gricdell")).toHaveTextContent("this is a test");
});
