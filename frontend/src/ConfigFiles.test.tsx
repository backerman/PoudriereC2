import { act, getByLabelText, render, screen, waitFor } from '@testing-library/react';
import { initializeIcons } from '@fluentui/react';
import userEvent from '@testing-library/user-event'
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

it('has a working editor', async () => {
    expect.hasAssertions(); // can't do at least n assertions.
    const sampleData = getSampleDataSource();

    await act(async () => {
        render(<ConfigFiles dataSource={sampleData}/>);
    });
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();

    userEvent.dblClick(screen.getByText("this is a test"));
    const editor = screen.getByRole("dialog");
    expect(editor).toBeInTheDocument();

    // Change the text.
    const nameField = getByLabelText(editor, "Name");
    await waitFor(() => {
        expect(nameField).toHaveValue("this is a test");
    });
    userEvent.clear(nameField);
    userEvent.type(nameField, "frodo baggins");
    expect(nameField).toHaveValue("frodo baggins");
    userEvent.click(screen.getByText("Save"));
    expect(editor).not.toBeVisible();

    // Verify update propagated to list.
    await waitFor(() => {
        expect(screen.queryByText("this is a test")).not.toBeInTheDocument();
    });

    expect(screen.queryByText("frodo baggins")).toBeInTheDocument();
});