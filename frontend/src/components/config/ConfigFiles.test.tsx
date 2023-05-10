import { initializeIcons } from '@fluentui/react';
import { act, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { sampleData } from 'src/models/configs.sample';
import { sampleData as samplePortset } from 'src/models/portsets.sample';
import { sampleData as samplePortstree } from 'src/models/portstrees.sample';
import { ConfigFiles } from './ConfigFiles';
import { SWRConfig } from 'swr';
import { ConfigFileMetadata } from '@/models/configs';
import { AuthConfigContext } from '../AuthConfigContext';
import { MsalReactTester } from 'msal-react-tester';
import { MsalProvider } from '@azure/msal-react';
import { getMock } from '@/utils/mockUtils';

initializeIcons();

const { mock, countAllMockRequests } = getMock();
let msalTester: MsalReactTester;

beforeEach(() => {
    mock.reset();
    msalTester = new MsalReactTester();
    msalTester.spyMsal();
});

afterEach(() => {
    msalTester.resetSpyMsal();
});

it('renders a file list successfully', async () => {
    await msalTester.isLogged();
    expect.assertions(5); // FIXME one more than expected
    expect(mock.history.get.length).toEqual(0);
    mock.onGet().reply(200, sampleData)
    await act(async () => {
        render(
            <MsalProvider instance={msalTester.client}>
                <AuthConfigContext.Provider value={{ functionsScope: '', isDevelopment: true }}>
                    <SWRConfig value={{ provider: () => new Map() }}>
                        <ConfigFiles />
                    </SWRConfig>
                </AuthConfigContext.Provider>
            </MsalProvider>)
    });
    await msalTester.waitForRedirect(); // this command seems to add one
    const sampleFilenameElement = screen.getByText("this is a test");
    expect(sampleFilenameElement).toBeInTheDocument();
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
    expect(mock.history.get.length).toEqual(1);
});

it('renders errors successfully', async () => {
    await msalTester.isLogged();
    const asyncCalled = jest.fn();
    expect.hasAssertions();
    expect(mock.history.get.length).toEqual(0);
    mock.onGet().reply(500, { error: "LOL no" });
    await act(async () => {
        render(
            <MsalProvider instance={msalTester.client}>
                <AuthConfigContext.Provider value={{ functionsScope: '', isDevelopment: true }}>
                    <SWRConfig value={{ provider: () => new Map() }}>
                        <ConfigFiles />
                    </SWRConfig>
                </AuthConfigContext.Provider>
            </MsalProvider>)
    });
    await msalTester.waitForRedirect();
    await waitFor(() => {
        const testElement = screen.getByRole("alert");
        expect(testElement).toHaveTextContent('LOL no');
        asyncCalled();
    });
    expect(mock.history.get.length).toEqual(1);
    expect(asyncCalled).toBeCalled();
});

it('has a working editor', async () => {
    await msalTester.isLogged();
    expect.hasAssertions(); // can't do at least n assertions.
    expect(mock.history.get.length).toEqual(0);
    let changeSubmitted = false;
    mock.onPut().reply(200, { result: 'OK' });
    mock.onGet(/\/api\/configurationfiles\/[-a-f0-9]+\/options/).reply(200, [{ name: "test", value: "testing" }]);
    mock.onGet('/api/configurationfiles').reply((config) => {
        return [200, sampleData.map((cf: ConfigFileMetadata) => {
            if (cf.name === "this is a test" && changeSubmitted) {
                return {
                    ...cf,
                    name: "frodo baggins",
                };
            }
            return cf;
        }), {}];
    });
    mock.onGet('/api/portsets').reply(200, samplePortset);
    mock.onGet('/api/portstrees').reply(200, samplePortstree);
    mock.onGet('/api/jails').reply(200, [{ name: 'test', type: 'null', requiresParameter: 'none' }]);

    const user = userEvent.setup();
    await act(async () => {
        render(
            <MsalProvider instance={msalTester.client}>
                <AuthConfigContext.Provider value={{ functionsScope: '', isDevelopment: true }}>
                    <SWRConfig value={{ provider: () => new Map() }}>
                        <ConfigFiles />
                    </SWRConfig>
                </AuthConfigContext.Provider>
            </MsalProvider>)
    });
    await msalTester.waitForRedirect();
    expect(mock.history.get.length).toEqual(1);
    expect(countAllMockRequests()).toEqual(1);
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
    expect(countAllMockRequests()).toBeGreaterThanOrEqual(2);
});
