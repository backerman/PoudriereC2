import { IColumn, IconButton } from '@fluentui/react'
import { ItemList, ItemListProperties } from '../ItemList'
import { ConfigOption } from '@/models/configs'

interface IConfigFileEditorItemsListProps extends ItemListProperties {
    items: ConfigOption[]
    deleteClicked: (item: ConfigOption) => void
}

export function ConfigFileEditorItemsList(props: IConfigFileEditorItemsListProps) {

    const columns: IColumn[] = [
        {
            key: 'name',
            name: 'Name',
            fieldName: 'name',
            minWidth: 100,
            isResizable: true,
        },
        {
            key: 'value',
            name: 'Value',
            fieldName: 'value',
            minWidth: 400,
            isResizable: true
        },
        {
            key: 'hamburger',
            name: '',
            minWidth: 50,
            isIconOnly: true,
            onRender: deleteMenuThingy
        }
    ]

    const {
        deleteClicked,
        items,
        ...others
    } = props;

    function deleteMenuThingy(item?: any, index?: number, column?: IColumn): any {
        return (
            <IconButton
                iconProps={{ iconName: 'Delete' }}
                title="Delete"
                ariaLabel="Delete"
                onClick={() => deleteClicked(item)}
            />
        )
    }
    return <ItemList
        {...others}
        items={items}
        columns={columns}
    />
}
