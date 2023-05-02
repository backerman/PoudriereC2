import { FontWeights, IColumn, IconButton, Stack, Text, TextField } from '@fluentui/react'
import { ItemList, ItemListProperties } from '../ItemList'
import { ConfigOption } from '@/models/configs'
import { useState } from 'react'

interface IConfigFileEditorItemsListProps extends ItemListProperties {
    items: ConfigOption[]
    deleteClicked: (item: ConfigOption) => void
    addClicked: (item: ConfigOption) => void
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
        addClicked,
        items,
        ...others
    } = props;
    const [settingName, updateSettingName] = useState('');
    const [settingValue, updateSettingValue] = useState('');
    const [settingNameError, updateSettingNameError] = useState<string | undefined>(undefined);
    const [settingValueError, updateSettingValueError] = useState<string | undefined>(undefined);

    function submitAdd() {
        // Validate that both fields are filled and the name is unique.
        if (settingName.length === 0) {
            updateSettingNameError('Name cannot be empty');
        }
        if (settingValue.length === 0) {
            updateSettingValueError('Value cannot be empty');
        }
        if (!(settingName.length && settingValue.length)) {
            return;
        }
        if (items.find(item => item.name === settingName)) {
            updateSettingNameError('Name must be unique');
        } else {
            addClicked({ name: settingName, value: settingValue });
            updateSettingName('');
            updateSettingValue('');
        }
    }

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
    return <>
        <Stack>
            <Stack.Item>
                <Text variant={'mediumPlus'} styles={{
                    root: {
                        fontWeight: FontWeights.bold
                    }
                }}>New item</Text>
            </Stack.Item>
            <Stack.Item>
                <Stack horizontal tokens={
                    {
                        childrenGap: "0 1em"
                    }
                }>
                    <Stack.Item grow={1}>
                        <TextField
                            aria-label={"Text field for adding a setting name"}
                            label={"Name"}
                            value={settingName}
                            errorMessage={settingNameError}
                            suffix="="
                            onChange={(_, newVal) => {
                                updateSettingName(newVal || '');
                                updateSettingNameError(undefined);
                                updateSettingValueError(undefined);
                            }}
                            onKeyDown={(ev) => {
                                if (ev.key === 'Enter') {
                                    submitAdd();
                                }
                            }} />
                    </Stack.Item>
                    <Stack.Item grow={3}>
                        <TextField
                            aria-label={"Text field for adding a setting value"}
                            label={"Value"}
                            resizable={true}
                            value={settingValue}
                            errorMessage={settingValueError}
                            multiline={settingValue.length > 50}
                            onChange={(_, newVal) => {
                                updateSettingValue(newVal || '');
                                updateSettingNameError(undefined);
                                updateSettingValueError(undefined);
                            }}
                            onKeyDown={(ev) => {
                                if (ev.key === 'Enter') {
                                    submitAdd();
                                }
                            }} />
                    </Stack.Item>
                    {
                        // FIXME: Alignment is somewhat of a hack.
                    }
                    <Stack.Item align={settingNameError || settingValueError ? 'center' : 'end'}>
                        <IconButton
                            ariaLabel={"Add setting"}
                            iconProps={{ iconName: "Add" }}
                            onClick={submitAdd}
                        />
                    </Stack.Item>
                </Stack>
            </Stack.Item>
            <Stack.Item>
                <ItemList
                    {...others}
                    items={items}
                    columns={columns}
                />
            </Stack.Item>
        </Stack>
    </>
}
