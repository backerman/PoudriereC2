import { act, render, screen, waitFor } from '@testing-library/react';
import { initializeIcons } from '@fluentui/react';
import { PortsTrees } from './PortsTrees';
import { FetchMock } from 'jest-fetch-mock/types';
import { sampleData } from '@/models/portstrees.sample';
import { SWRConfig } from 'swr';

initializeIcons();

beforeEach(() => {
    fetchMock.resetMocks();
    fetchMock.doMock();
});

it('renders a ports tree list successfully', async () => {
    expect.assertions(3);
    expect((fetch as FetchMock).mock.calls.length).toEqual(0);
    fetchMock.mockResponse(JSON.stringify(sampleData));
    await act(async () => {
        render(
            <SWRConfig value={{ provider: () => new Map() }}>
                <PortsTrees />
            </SWRConfig>);
    });
    const samplePortsTreeName = screen.getByText("Bobby Tables");
    expect(samplePortsTreeName).toBeInTheDocument();
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
});

it('renders errors successfully', async () => {
    expect.hasAssertions();
    expect((fetch as FetchMock).mock.calls.length).toEqual(0);
    fetchMock.mockReject(new Error('LOL no'));
    await act(async () => {
        render(
            <SWRConfig value={{ provider: () => new Map() }}>
                <PortsTrees />
            </SWRConfig>);
    });

    await waitFor(() => {
        const testElement = screen.getByRole("alert");
        expect(testElement).toHaveTextContent("LOL no");
    });
});
