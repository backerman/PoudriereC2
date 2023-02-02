import { act, getByLabelText, render, screen, waitFor } from '@testing-library/react';
import { initializeIcons } from '@fluentui/react';
import userEvent from '@testing-library/user-event'
import { getSampleDataSource } from '../model/portsets.sample';
import { PortSets } from './PortSets';
import { PortSet, PortSetRepository } from '../model/portsets';

initializeIcons();

it('renders a portset list successfully', async () => {
    expect.assertions(2);
    const sampleData = getSampleDataSource();

    await act(async () => {
        render(<PortSets dataSource={sampleData}/>);
    });
    const samplePortSetName = screen.getByText("Yog-Sothoth"); 
    expect(samplePortSetName).toBeInTheDocument();
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
});

const erroringDataSource : PortSetRepository = {
    getPortSets: async function (): Promise<PortSet[]> {
        throw new Error('Function not implemented.');
    },
    getPortSet: function (id: string): Promise<PortSet | undefined> {
        throw new Error('Function not implemented.');
    },
    updatePortSet: function (id: string, packageSet: PortSet): Promise<void> {
        throw new Error('Function not implemented.');
    }
}

it('renders errors successfully', async () => {
    expect.hasAssertions();
    await act(async () => {
        render(<PortSets dataSource={erroringDataSource}/>);
    });

    await waitFor(() => {
        const testElement = screen.getByRole("alert");
        expect(testElement).toHaveTextContent("Error retrieving data.");
    });
});
