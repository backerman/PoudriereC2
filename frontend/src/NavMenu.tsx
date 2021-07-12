import { INavLink, INavLinkGroup, INavStyles, Nav } from "@fluentui/react";
import { useCallback } from "react";
import { useHistory } from 'react-router-dom';

const navStyles: Partial<INavStyles> = {
    root: {
        width: "40 em",
        boxSizing: 'border-box',
        border: '1px solid #eee',
        overflowY: 'auto',
    },
};

const navLinkGroups: INavLinkGroup[] = [
    {
        links: [
            {
                name: 'Configuration',
                url: '/config',
                icon: 'Settings',
                expandAriaLabel: 'Expand configuration section',
                collapseAriaLabel: 'Collapse configuration section',
                links: [
                    {
                        name: 'Files',
                        url: '/config/files',
                        key: 'key1',
                    },
                    {
                        name: 'Ports trees',
                        url: '/config/trees',
                        key: 'key2',
                    },
                ],
                isExpanded: false,
            },
            {
                name: 'Job history',
                url: 'http://example.com',
                key: 'key3',
                icon: 'History',
            },
            {
                name: 'Pages',
                url: 'http://msn.com',
                key: 'key4',
                target: '_blank',
            }
        ],
    },
];

export const NavMenu: React.FunctionComponent = () => {
    const history = useHistory();

    const onLinkClick = useCallback((ev?: React.MouseEvent<HTMLElement>, item?: INavLink) => {
        ev?.preventDefault();
        if (item != null) history.push(item.url);
    }, [history]);

    return (
        <Nav
            styles={navStyles}
            groups={navLinkGroups}
            onLinkClick={onLinkClick} />
    )
}