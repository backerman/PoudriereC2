import { act, render, screen, waitFor } from '@testing-library/react';
import { initializeIcons } from '@fluentui/react';
import { PortSets } from './PortSets';
import { sampleData } from '@/models/portsets.sample';
import { SWRConfig } from 'swr';
import MockAdapter from 'axios-mock-adapter';
import axios from 'axios';
import { AuthConfigContext } from '../AuthConfigContext';

initializeIcons();

const mock = new MockAdapter(axios, {
    onNoMatch: "throwException",
})

function countAllMockRequests() {
    let keys = Object.keys(mock.history);
    return keys.reduce((acc, cur) => acc + mock.history[cur].length, 0);
}

beforeEach(() => {
    mock.reset();
});

it('renders a portset list successfully', async () => {
    expect.assertions(4);
    expect(mock.history.get.length).toEqual(0);
    mock.onGet().reply(200, sampleData);
    await act(async () => {
        render(
            <AuthConfigContext.Provider value={{ functionsScope: '', isDevelopment: true }}>
                <SWRConfig value={{ provider: () => new Map() }}>
                    <PortSets />
                </SWRConfig>
            </AuthConfigContext.Provider>);
    });
    const samplePortSetName = screen.getByText("Yog-Sothoth");
    expect(samplePortSetName).toBeInTheDocument();
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
    expect(mock.history.get.length).toEqual(1);
});

it('renders errors successfully', async () => {
    expect.hasAssertions();
    expect(mock.history.get.length).toEqual(0);
    mock.onGet().reply(500, { error: "LOL no" });
    await act(async () => {
        render(
            <AuthConfigContext.Provider value={{functionsScope: '', isDevelopment: true}}>
            <SWRConfig value={{ provider: () => new Map() }}>
                <PortSets />
            </SWRConfig>
            </AuthConfigContext.Provider>);
    });

    await waitFor(() => {
        const testElement = screen.getByRole("alert");
        expect(testElement).toHaveTextContent("LOL no");
    });
    expect(mock.history.get.length).toEqual(1);
});
