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

export interface ITopBarProps {
    user: {
        name: string,
        upn: string,
        initials?: string
    },
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

export function TopBar(props: ITopBarProps): JSX.Element {

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
                        text={props.user.name}
                        imageInitials={props.user.initials}
                        showSecondaryText={true}
                        secondaryText={props.user.upn}
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