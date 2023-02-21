import { initializeIcons } from '@fluentui/react';
import { act, getByLabelText, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { sampleData } from 'src/models/configs.sample';
import { ConfigFiles } from './ConfigFiles';
import { SWRConfig } from 'swr';
import { FetchMock } from 'jest-fetch-mock/types';
import { ConfigFileMetadata } from '@/models/configs';

initializeIcons();

beforeEach(() => {
    fetchMock.resetMocks();
    fetchMock.doMock();
});

it('renders a file list successfully', async () => {
    expect.assertions(4);
    expect((fetch as FetchMock).mock.calls.length).toEqual(0);
    fetchMock.mockResponse(JSON.stringify(sampleData));
    await act(async () => {
        render(<SWRConfig value={{ provider: () => new Map() }}>
            <ConfigFiles />
        </SWRConfig>)
    });
    const sampleFilenameElement = screen.getByText("this is a test");
    expect(sampleFilenameElement).toBeInTheDocument();
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
    expect((fetch as FetchMock).mock.calls.length).toEqual(1);
});

it('renders errors successfully', async () => {
    expect.hasAssertions();
    expect((fetch as FetchMock).mock.calls.length).toEqual(0);
    fetchMock.mockReject(new Error('LOL no'));
    await act(async () => {
        render(<SWRConfig value={{ provider: () => new Map() }}>
            <ConfigFiles />
        </SWRConfig>)
    });
    await waitFor(() => {
        const testElement = screen.getByRole("alert");
        expect(testElement).toHaveTextContent('LOL no');
    });
    expect((fetch as FetchMock).mock.calls.length).toEqual(1);
});

it('has a working editor', async () => {
    expect.hasAssertions(); // can't do at least n assertions.
    expect((fetch as FetchMock).mock.calls.length).toEqual(0);
    fetchMock.mockOnce(JSON.stringify(sampleData));
    const user = userEvent.setup();
    await act(async () => {
        render(<SWRConfig value={{ provider: () => new Map() }}>
            <ConfigFiles />
        </SWRConfig>)
    });
    expect((fetch as FetchMock).mock.calls.length).toEqual(1);
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();

    await user.dblClick(screen.getByText("this is a test"));
    const editor = screen.getByRole("dialog");
    expect(editor).toBeInTheDocument();

    // Change the text.
    fetchMock.mockOnce(JSON.stringify(sampleData.map((cf: ConfigFileMetadata) => {
        if (cf.name === "this is a test") {
            return {
                ...cf,
                name: "frodo baggins",
            };
        }
        return cf;
    })));
    const nameField = getByLabelText(editor, "Name");
    await waitFor(() => {
        expect(nameField).toHaveValue("this is a test");
    });
    await user.clear(nameField);
    await user.type(nameField, "frodo baggins");
    expect(nameField).toHaveValue("frodo baggins");
    await user.click(screen.getByText("Save"));
    expect(editor).not.toBeVisible();

    // Verify update propagated to list.
    await waitFor(() => {
        expect(screen.queryByText("this is a test")).not.toBeInTheDocument();
    });

    expect(screen.queryByText("frodo baggins")).toBeInTheDocument();
    expect((fetch as FetchMock).mock.calls.length).toBeGreaterThanOrEqual(2);
});