import { INavLink, INavLinkGroup, INavStyles, Nav } from "@fluentui/react";
import { useRouter } from 'next/router'

function toggleExpand(ev?: React.MouseEvent<HTMLElement>, item?: INavLink) {
    ev?.preventDefault();
    if (item != null) {
        item?.isExpanded ? item.isExpanded = false : item.isExpanded = true;
    }
}

const navStyles: Partial<INavStyles> = {
    root: {
        width: "40 em",
        boxSizing: 'border-box',
        border: '1px solid #eee',
        overflowY: 'auto',
    },
    chevronIcon: {
        display: 'none',
    }
};

const navLinkGroups: INavLinkGroup[] = [
    {
        links: [
            {
                name: 'Configuration',
                url: '#',
                icon: 'Settings',
                expandAriaLabel: 'Expand configuration section',
                links: [
                    {
                        name: 'Files',
                        url: '/config/files',
                        key: 'key1',
                    },
                    {
                        name: 'Portsets',
                        url: '/config/portsets',
                        key: 'key2',
                    },
                ],
                isExpanded: false,
                forceAnchor: true,
                onClick: toggleExpand,
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

export function NavMenu() {
    const router = useRouter();
    const onLinkClick = (ev?: React.MouseEvent<HTMLElement>, item?: INavLink) => {
        ev?.preventDefault();
        if (item != null && item.url !== "#") router.push(item.url);
    };

    return (
        <Nav
            styles={navStyles}
            groups={navLinkGroups}
            onLinkClick={onLinkClick} />
    )
}