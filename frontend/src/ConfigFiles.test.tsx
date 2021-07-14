import { act, render, screen, waitFor } from '@testing-library/react';
import { initializeIcons } from '@fluentui/react';
import { getSampleDataSource } from './model/configs.sample';
import { ConfigFiles } from './ConfigFiles';
import { ConfigFileMetadata, ConfigFileRepository } from './model/configs';

initializeIcons();

it('renders a file list successfully', async () => {
    expect.assertions(2);
    const sampleData = getSampleDataSource();

    await act(async () => {
        render(<ConfigFiles dataSource={sampleData}/>);
    });
    const sampleFilenameElement = screen.getByText("this is a test"); 
    expect(sampleFilenameElement).toBeInTheDocument();
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
});

const erroringDataSource : ConfigFileRepository = {
    getConfigFiles: async function (): Promise<ConfigFileMetadata[]> {
        throw new Error("Function not implemented.");
    },
    getConfigFile: function (id: string): Promise<ConfigFileMetadata | undefined> {
        throw new Error('Function not implemented.');
    },
    updateConfigFile: function (meta: ConfigFileMetadata): Promise<void> {
        throw new Error('Function not implemented.');
    }
}

it('renders errors successfully', async () => {
    expect.hasAssertions();
    await act(async () => {
        render(<ConfigFiles dataSource={erroringDataSource}/>);
    });

    await waitFor(() => {
        const testElement = screen.getByRole("alert");
        expect(testElement).toHaveTextContent("Error retrieving data.");
    });
});