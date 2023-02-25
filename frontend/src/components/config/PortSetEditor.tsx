import { IconButton, Stack, TextField } from "@fluentui/react";
import { useReducer, useRef, useState } from "react";
import { Editor } from "../Editor";
import { ItemList } from "../ItemList";
import { PortSet } from "src/models/portsets";

export interface PortSetEditorProps {
    isOpen: boolean
    record: PortSet | undefined
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
        useReducer(updateOrigins, [])
    let [portSetName, setPortSetName] = useState('');
    let [error, setError] = useState<any>(null);
    let originalValue = useRef<PortSet>({ id: '', name: '', origins: [] });

    // The text field for adding an origin.
    const [originName, updateOriginName] = useState('');

    return (
        <Editor
            isOpen={props.isOpen}
            isBlocking={false}
            headerText={"Edit port set"}
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
                        onKeyPress={(ev) => {
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
            <ItemList
                compact={true}
                columns={[{
                    key: 'name',
                    name: 'Name',
                    minWidth: 100,
                    onRender: (item) => <span>{item}</span>
                }]}
                getRowAriaLabel={(r: string) => r}
                items={props.record?.origins || []}
                error={error}>
            </ItemList>
        </Editor>)
};
