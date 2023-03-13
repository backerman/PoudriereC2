import { CommandBar, ICommandBarItemProps, IconButton, Selection, SelectionMode, Stack, TextField } from "@fluentui/react";
import { useEffect, useReducer, useRef, useState } from "react";
import { Editor } from "../Editor";
import { ItemList } from "../ItemList";
import { PortSet } from "src/models/portsets";

export interface PortSetEditorProps {
    isOpen: boolean
    record: PortSet | undefined
    createNewRecord: boolean
    onSubmit: (formData: PortSet) => void
    onDismiss: () => void
}

export function PortSetEditor(props: PortSetEditorProps): JSX.Element {
    function updateOrigins(
        state: string[],
        mutation: { action: 'add' | 'delete', value: string }
            | string[]): string[] {
        let newState = state.slice();
        if (!("action" in mutation)) {
            // Must be an array of origins.
            newState = mutation;
        } else {
            switch (mutation.action) {
                case "add":
                    newState.push(mutation.value);
                    newState.sort();
                    break;
                case "delete":
                    const idx = newState.indexOf(mutation.value);
                    if (idx >= 0) {
                        newState.splice(idx, 1);
                    }
                    break;
            }
        }
        return newState;
    }

    let [origins, setOrigins] =
        useReducer(updateOrigins, []);
    let [portSetName, setPortSetName] = useState('');
    let [error, setError] = useState<any>(null);
    let originalValue = useRef<PortSet>({ id: '', name: '', origins: [] });

    // The text field for adding an origin.
    const [originName, updateOriginName] = useState('');

    // Whether the delete button is enabled; it should be enabled
    // iff at least one item is selected.
    const [deleteButtonDisabled, setDeleteButtonDisabled] = useState(true);

    const [selection] = useState(new Selection({
        onSelectionChanged: () => {
            setDeleteButtonDisabled(selection.getSelectedCount() === 0);
            // forceUpdate();
        }
    }));

    const portsetCommandBarItems : ICommandBarItemProps[] = [
        {
            key: 'delete',
            text: 'Delete',
            ariaDescription: 'Delete selected origins',
            disabled: deleteButtonDisabled,
            iconProps: { iconName: 'Delete' },
            onClick: () => {
                const toDelete = selection.getSelection() as string[];
                toDelete.forEach((origin) => {
                    setOrigins({ action: 'delete', value: origin });
                })
                selection.setAllSelected(false);
            }
        }
    ];

    useEffect(() => {
        if (props.record) {
            originalValue.current = props.record;
            setPortSetName(props.record.name);
            setOrigins(props.record.origins);
        }
    }, [props.record]);

    return (
        <Editor
            isOpen={props.isOpen}
            isBlocking={false}
            headerText={`${props.createNewRecord ? 'Create' : 'Edit'} port set ${portSetName}`}
            onDismiss={() => {
                // When cancel button selected, revert changes.
                setPortSetName(originalValue.current.name);
                updateOriginName('');
                setOrigins(originalValue.current.origins);
                props.onDismiss();
            }}
            onSubmit={() => {
                props.onSubmit({
                    id: props.record?.id || '',
                    name: portSetName,
                    origins: origins
                })
            }}>
            <Stack horizontal>
                <Stack.Item align="stretch" grow={true}>
                    <TextField
                        width="100%"
                        aria-label={"Text field for adding a package origin"}
                        placeholder={"Enter a package origin to add."}
                        value={originName}
                        onChange={(_, newVal) => updateOriginName(newVal || '')}
                        onKeyDown={(ev) => {
                            if (ev.key === 'Enter') {
                                setOrigins({ action: 'add', value: originName });
                                updateOriginName('');
                            }
                        }} />
                </Stack.Item>
                <IconButton
                    ariaLabel={"Add package"}
                    iconProps={{ iconName: "Add" }}
                    onClick={() => {
                        setOrigins({ action: 'add', value: originName })
                    }} />
            </Stack>
            <CommandBar
                items={portsetCommandBarItems}
            />
            <ItemList
                compact={true}
                columns={[{
                    key: 'name',
                    name: 'Name',
                    minWidth: 100,
                    onRender: (item) => <span>{item}</span>
                }]}
                getKey={(r: string) => r}
                getRowAriaLabel={(r: string) => r}
                items={origins}
                error={error}
                selection={selection}
                selectionMode={SelectionMode.multiple}
                selectionPreservedOnEmptyClick={true} />
        </Editor>)
};
