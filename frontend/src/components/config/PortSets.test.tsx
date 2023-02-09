import { act, getByLabelText, render, screen, waitFor } from '@testing-library/react';
import { initializeIcons } from '@fluentui/react';
import userEvent from '@testing-library/user-event'
import { getErroringDataSource, getSampleDataSource } from 'src/models/portsets.sample';
import { PortSets } from './PortSets';
import { PortSet, PortSetRepository } from 'src/models/portsets';

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

it('renders errors successfully', async () => {
    expect.hasAssertions();
    const erroringDataSource = getErroringDataSource();
    await act(async () => {
        render(<PortSets dataSource={erroringDataSource}/>);
    });

    await waitFor(() => {
        const testElement = screen.getByRole("alert");
        expect(testElement).toHaveTextContent("Error retrieving data.");
    });
});
