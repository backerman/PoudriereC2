import { Dropdown, IDropdownOption, TextField } from "@fluentui/react";
import { useCallback, useEffect, useReducer, useState } from "react";
import { Editor } from "@/components/Editor";
import { ConfigFileMetadata, ConfigFileRepository } from "src/models/configs";

export type ConfigFileEditorProps =
    {
        isOpen: boolean
        record: ConfigFileMetadata
        recordId: string
        onSubmit?: (formData: ConfigFileMetadata) => void
        onDismiss: () => void
    }

const fileTypeChoices: IDropdownOption<string>[] = [
    { key: "poudriereconf", text: "poudriere.conf" },
    { key: "makeconf", text: "make.conf" },
    { key: "srcconf", text: "src.conf" }
];

export function ConfigFileEditor(props: ConfigFileEditorProps): JSX.Element {

    function updateState<K extends keyof ConfigFileMetadata>(state: ConfigFileMetadata,
        action: { field: K, value: ConfigFileMetadata[K] } | ConfigFileMetadata): ConfigFileMetadata {
        let newState = { ...state };
        if (!("field" in action)) {
            // Must be ConfigFileMetadata.
            newState = action;
        } else {
            switch (typeof action.value) {
                case "boolean":
                case "string":
                case "undefined":
                    newState[action.field] = action.value;
                    break;
                default:
                    throw Error("Unexpected type passed");
            }
        }
        return newState;
    }
    let [mostRecentPropsRecord, setMostRecentPropsRecord] = useState(props.record);
    let [configFileData, setState] =
        useReducer(updateState, {} as ConfigFileMetadata);
    // The state isn't reinitialized when the props change, so do that
    // manually.
    if (props.record != mostRecentPropsRecord) {
        setState(props.record);
        setMostRecentPropsRecord(props.record);
    }

    const onTextChange = useCallback((fieldName: keyof ConfigFileMetadata) => {
        return (event: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>, newValue?: string) => {
            setState(updateState(configFileData, { field: fieldName, value: (newValue || '') }));
        }
    }, [configFileData]);

    return (
        <Editor
            isOpen={props.isOpen}
            isBlocking={false}
            headerText={"Edit configuration"}
            onDismiss={props.onDismiss}
            onSubmit={() => { props.onSubmit && props.onSubmit(configFileData) }}>
            <TextField
                label="GUID"
                value={configFileData.id || ''}
                contentEditable={false}
                onChange={onTextChange("id")} />
            <TextField
                label="Name"
                value={configFileData.name || ''}
                onChange={onTextChange("name")} />
            <Dropdown
                label="File type"
                placeholder="Select a file type"
                selectedKey={configFileData.fileType || ''}
                options={fileTypeChoices}
                onChange={(_, val) => setState({ field: "fileType", value: val?.key.toString() })} />
            <TextField
                label="Jail"
                value={configFileData.jail || ''}
                onChange={onTextChange("jail")} />
            <TextField
                label="Port set"
                value={configFileData.portSet || ''}
                onChange={onTextChange("portSet")} />
            <TextField
                label="Ports tree"
                value={configFileData.portsTree || ''}
                onChange={onTextChange("portsTree")} />
        </Editor>)
};
