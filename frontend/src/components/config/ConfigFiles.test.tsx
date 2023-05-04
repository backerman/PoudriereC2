import { initializeIcons } from '@fluentui/react';
import { act, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { sampleData } from 'src/models/configs.sample';
import { sampleData as samplePortset } from 'src/models/portsets.sample';
import { sampleData as samplePortstree } from 'src/models/portstrees.sample';
import { ConfigFiles } from './ConfigFiles';
import { SWRConfig } from 'swr';
import { FetchMock } from 'jest-fetch-mock/types';
import { ConfigFileMetadata } from '@/models/configs';

initializeIcons();

beforeEach(() => {
    fetchMock.resetMocks();
    fetchMock.doMock();
    // Switch to using jest.SpyOn instead of jest-mock-fetch?
    // In the meantime, ensure NEXT_PUBLIC_API_BASE_URL set for
    // VSCode.
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
    let changeSubmitted = false;
    fetchMock.doMock(async (req) => {
        const parsedUrl = new URL(req.url);
        if (req.method === 'PUT') {
            return JSON.stringify({
                result: 'OK'
            })
        }
        switch (parsedUrl.pathname) {
            case '/api/configurationfiles':
                return JSON.stringify(sampleData.map((cf: ConfigFileMetadata) => {
                    if (cf.name === "this is a test" && changeSubmitted) {
                        return {
                            ...cf,
                            name: "frodo baggins",
                        };
                    }
                    return cf;
                }));
                break;
            case '/api/configurationfiles/aa5cd502-eb08-4f42-b187-b81c3d849611/options':
                return JSON.stringify([{ name: "test", value: "testing" }]);
                break;
            case '/api/configurationfiles/c1fac43d-49de-4821-8ff5-8157cf8f5e29/options':
                return JSON.stringify([{ name: "test", value: "testing" }]);
                break;
            case '/api/portsets':
                return JSON.stringify(samplePortset);
                break;
            case '/api/portstrees':
                return JSON.stringify(samplePortstree);
            case '/api/jails':
                return JSON.stringify([{ name: 'test', type: 'null', requiresParameter: 'none' }]);
                break;
            default:
                console.log("Doing path", parsedUrl.pathname);
                throw new Error(`Unexpected URL ${req.url}`);
        }
    });
    const user = userEvent.setup();
    await act(async () => {
        render(<SWRConfig value={{ provider: () => new Map() }}>
            <ConfigFiles />
        </SWRConfig>)
    });
    expect((fetch as FetchMock).mock.calls.length).toEqual(1);
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
    await act(async () => user.dblClick(screen.getByText("this is a test")));
    const editor = screen.getByRole("dialog");
    expect(editor).toBeInTheDocument();

    // Change the text.
    const nameField = screen.getByTestId("config-file-name");
    await waitFor(() => {
        expect(nameField).toHaveValue("this is a test");
    });
    await act(async () => {
        await user.clear(nameField);
        await user.type(nameField, "frodo baggins");
    });
    expect(nameField).toHaveValue("frodo baggins");
    await act(async () => {
        changeSubmitted = true; // FIXME need to properly return new data in mutate.
        await user.click(screen.getByText("Save"));
    });
    expect(editor).not.toBeVisible();

    // Verify update propagated to list.
    await waitFor(() => {
        expect(screen.queryByText("this is a test")).not.toBeInTheDocument();
    });

    expect(screen.queryByText("frodo baggins")).toBeInTheDocument();
    expect((fetch as FetchMock).mock.calls.length).toBeGreaterThanOrEqual(2);
});
