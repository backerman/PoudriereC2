import { useMsal } from '@azure/msal-react';
import {
    CommandBar,
    getTheme,
    ICommandBarItemProps,
    ICommandBarStyles,
    IContextualMenuItem,
    IPersonaStyles,
    ITextStyles,
    Persona,
    PersonaSize,
    Text
} from '@fluentui/react';
import { useEffect, useState } from 'react';

export interface ITopBarProps {
    onLogout?: (ev?: React.MouseEvent<HTMLElement> | React.KeyboardEvent<HTMLElement>,
        item?: IContextualMenuItem) => boolean | void
}

const theme = getTheme();

const barStyles: ICommandBarStyles = {
    root: {
        backgroundColor: theme.palette.themePrimary,
        padding: '0.5em',
        height: '100%',
        alignItems: 'baseline'
    }
};

const barTextStyles: ITextStyles = {
    root: {
        paddingLeft: '0.5em',
        paddingRight: '0.5em',
        color: theme.palette.white
    }
};

const personaStyles: Partial<IPersonaStyles> = {
    root: {
        backgroundColor: theme.palette.themePrimary,
    },
    primaryText: {
        color: theme.palette.white,
    },
    secondaryText: {
        color: theme.palette.white,
    }
}

type AuthState = {
    name: string;
    username: string;
    initials: string;
}

export function TopBar(props: ITopBarProps): JSX.Element {
    const { accounts, instance, inProgress } = useMsal();
    const [user, setUser] = useState<AuthState>({
        name: 'The Man With No Name',
        initials: 'NN',
        username: 'manwithnoname@example.com'
    });
    useEffect(() =>{
        if (!instance.getActiveAccount() && accounts.length > 0) {
            instance.setActiveAccount(accounts[0]);
        }
        const activeAcct = instance.getActiveAccount();

        if (activeAcct) {
            setUser({
                name: activeAcct.name || '',
                username: activeAcct.username,
                initials: activeAcct.name?.split(' ').map((n) => n[0]).join('') || ''
            });
        }
    }, [accounts, instance]);

    const barItems: ICommandBarItemProps[] = [
        {
            key: 'collapseNavbar',
            ariaLabel: 'Collapse navbar',
            text: 'Collapse navbar',
            iconOnly: true,
            iconProps: {
                iconName: 'GlobalNavButton',
            },
            buttonStyles: {
                root: {
                    backgroundColor: theme.palette.themePrimary,
                },
                icon: {
                    color: theme.palette.white,
                }
            }
        },
        {
            key: 'newItem',
            text: 'Poudriere Configuration',
            commandBarButtonAs: () => {
                return (
                    <Text variant={'large'} styles={barTextStyles}>
                        Poudriere build management
                    </Text>)
            },
        }
    ];

    const barFarItems: ICommandBarItemProps[] = [
        {
            key: 'userMenu',
            ariaLabel: 'User menu',
            buttonStyles: {
                root: {
                    backgroundColor: theme.palette.themePrimary,
                },
                rootExpanded: {
                    backgroundColor: theme.palette.themePrimary,
                },
                rootHovered: {
                    backgroundColor: theme.palette.themePrimary,
                },
                rootExpandedHovered: {
                    backgroundColor: theme.palette.themePrimary,
                },
                rootPressed: {
                    backgroundColor: theme.palette.themePrimary,
                },
                rootFocused: {
                    backgroundColor: theme.palette.themePrimary,
                    // Otherwise focus outline is invisible
                    // TODO: systematically validate focus outline visibility
                    borderWidth: '1px',
                    borderStyle: 'solid',
                },
                menuIcon: {
                    color: theme.palette.white,
                },
                menuIconChecked: {
                    color: theme.palette.white,
                },
                menuIconExpanded: {
                    color: theme.palette.white,
                },
                menuIconExpandedHovered: {
                    color: theme.palette.white,
                },
                menuIconHovered: {
                    color: theme.palette.white,
                },
                menuIconPressed: {
                    color: theme.palette.white,
                }
            },
            onRenderChildren: () => {
                return (
                    <Persona
                        size={PersonaSize.size32}
                        text={user.name}
                        imageInitials={user.initials}
                        showSecondaryText={true}
                        secondaryText={user.username}
                        styles={personaStyles}
                    />
                )
            },
            subMenuProps: {
                items: [
                    {
                        key: 'signOut',
                        text: 'Sign out',
                        iconProps: { iconName: 'SignOut' },
                        onClick: props.onLogout
                    }
                ]
            }
        }
    ];

    return (
        <CommandBar
            styles={barStyles}
            items={barItems}
            farItems={barFarItems}
        />
    );
}