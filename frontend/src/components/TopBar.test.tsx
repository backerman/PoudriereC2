import userEvent from '@testing-library/user-event'
import { act, render, screen, waitFor, within } from '@testing-library/react';
import { ITopBarProps, TopBar } from './TopBar';
import { initializeIcons } from '@fluentui/react';

initializeIcons();

const userProps : ITopBarProps = {
    user: {
        name: "Dr. Robert Ford",
        upn: "robert.ford@example.com",
        initials: "RF"
    }
}

it('renders the top bar', async () => {
    const logoutCallback = jest.fn();

    await act(async () => {
        render(<TopBar
            {...userProps}
            onLogout={logoutCallback} />);
    });

    expect.hasAssertions();
    await waitFor(async () => {
        const menu = screen.getByLabelText("User menu");
        expect(menu).toBeVisible();
        expect(menu).toHaveTextContent(userProps.user.name);
        expect(menu).toHaveTextContent(userProps.user.upn);
        expect(logoutCallback).not.toHaveBeenCalled();
    });
});

it('calls the logout callback when the logout button is clicked', async () => {
    const logoutCallback = jest.fn();

    await act(async () => {
        render(<TopBar
            {...userProps}
            onLogout={logoutCallback} />);
    });

    expect.hasAssertions();
    const menu = screen.getByLabelText("User menu");
    expect(menu).toHaveAttribute("aria-expanded", "false");
    await userEvent.click(menu);
    expect(menu).toHaveAttribute("aria-expanded", "true");
    const logoutButton = screen.getByRole("menuitem", { name: "Sign out"});
    await waitFor(() => {
        expect(logoutButton).toBeInTheDocument();
    })
    await userEvent.click(logoutButton);
    expect(logoutCallback).toHaveBeenCalled();
});