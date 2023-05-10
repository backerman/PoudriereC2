import { act, render, screen, waitFor } from '@testing-library/react';
import { initializeIcons } from '@fluentui/react';
import { PortsTrees } from './PortsTrees';
import { sampleData } from '@/models/portstrees.sample';
import { SWRConfig } from 'swr';
import { getMock } from '@/utils/mockUtils';
import { AuthConfigContext } from '../AuthConfigContext';

initializeIcons();

const { mock, countAllMockRequests } = getMock();

beforeEach(() => {
    mock.reset();
});

it('renders a ports tree list successfully', async () => {
    expect.assertions(4);
    expect(countAllMockRequests()).toEqual(0);
    mock.onGet().reply(200, sampleData)
    await act(async () => {
        render(
            <AuthConfigContext.Provider value={{ functionsScope: '', isDevelopment: true }}>
                <SWRConfig value={{ provider: () => new Map() }}>
                    <PortsTrees />
                </SWRConfig>
            </AuthConfigContext.Provider>);
    });
    const samplePortsTreeName = screen.getByText("Bobby Tables");
    expect(samplePortsTreeName).toBeInTheDocument();
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
    expect(countAllMockRequests()).toEqual(1);
});

it('renders errors successfully', async () => {
    expect.hasAssertions();
    expect(countAllMockRequests()).toEqual(0);
    mock.onGet().reply(500, { error: "LOL no" });
    await act(async () => {
        render(
            <AuthConfigContext.Provider value={{ functionsScope: '', isDevelopment: true }}>
                <SWRConfig value={{ provider: () => new Map() }}>
                    <PortsTrees />
                </SWRConfig>
            </AuthConfigContext.Provider>);
    });

    await waitFor(() => {
        const testElement = screen.getByRole("alert");
        expect(testElement).toHaveTextContent("LOL no");
    });
});
