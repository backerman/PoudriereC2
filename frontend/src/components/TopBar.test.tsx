import userEvent from '@testing-library/user-event'
import { act, render, screen, waitFor } from '@testing-library/react';
import { TopBar } from './TopBar';
import { initializeIcons } from '@fluentui/react';
import { MsalReactTester } from 'msal-react-tester';
import { AccountInfo } from '@azure/msal-common';
import { MsalProvider } from '@azure/msal-react';

initializeIcons();

const userAcctInfo: AccountInfo = {
    "homeAccountId": "58906f55-c091-4c30-9ffe-068d2e52d3f1.3a74f1fe-841a-468b-845c-10e65df01a9b",
    "environment": "login.windows.net",
    "tenantId": "58906f55-c091-4c30-9ffe-068d2e52d3f1",
    "username": "robert.ford@example.com",
    "localAccountId": "879bdbd4-7e35-4a3f-8d51-1558c82f365f",
    "name": "Robert Ford"
};

let msalTester: MsalReactTester;

beforeEach(() => {
    msalTester = new MsalReactTester("Popup", userAcctInfo);
    msalTester.spyMsal();
});

afterEach(() => {
    // reset msal-react-tester
    msalTester.resetSpyMsal();
});

it('renders the top bar', async () => {
    const logoutCallback = jest.fn();
    msalTester.isLogged();

    await act(async () => {
        render(
            <MsalProvider instance={msalTester.client}>
                <TopBar
                    onLogout={logoutCallback} />
            </MsalProvider>
        );
    });

    expect.hasAssertions();
    await waitFor(async () => {
        const menu = screen.getByLabelText("User menu");
        const userInitials: string =
            userAcctInfo.name?.split(' ').map((n) => n[0]).join('')
            || 'If the initials are undefined, something has gone horribly wrong';
        expect(menu).toBeVisible();
        expect(menu).toHaveTextContent(userAcctInfo.username);
        expect(menu).toHaveTextContent(userAcctInfo.name!);
        expect(menu).toHaveTextContent(userInitials)
        expect(logoutCallback).not.toHaveBeenCalled();
    });
});

it('calls the logout callback when the logout button is clicked', async () => {
    const logoutCallback = jest.fn();
    msalTester.isLogged();

    await act(async () => {
        render(
            <MsalProvider instance={msalTester.client}>
                <TopBar
                    onLogout={logoutCallback} />
            </MsalProvider>
        );
    });

    expect.hasAssertions();
    const menu = screen.getByLabelText("User menu");
    // If skipHover isn't set, user-event will think pointer-events has been
    // set to none further up the DOM tree, and will throw an error.
    const user = userEvent.setup({ skipHover: true });
    expect(menu).toHaveAttribute("aria-expanded", "false");
    await user.click(menu);
    expect(menu).toHaveAttribute("aria-expanded", "true");
    const logoutButton = screen.getByRole("menuitem", { name: "Sign out" });
    await waitFor(() => {
        expect(logoutButton).toBeInTheDocument();
    });
    await user.click(logoutButton);
    expect(logoutCallback).toHaveBeenCalled();
});