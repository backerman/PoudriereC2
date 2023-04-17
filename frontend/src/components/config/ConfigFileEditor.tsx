import { Dropdown, IComboBox, IComboBoxOption, IDropdownOption, TextField } from "@fluentui/react";
import { useCallback, useReducer, useState } from "react";
import { Editor } from "@/components/Editor";
import { ConfigFileMetadata } from "src/models/configs";
import { ComboBoxWithFetcher } from "./ComboBoxWithFetcher";
import { PortSet } from "@/models/portsets";
import { PortsTree } from "@/models/portstrees";
import { Jail } from "@/models/jails";

export type ConfigFileEditorProps =
    {
        isOpen: boolean
        creatingNewRecord: boolean
        record: ConfigFileMetadata
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

    const onTextChange = (fieldName: keyof ConfigFileMetadata) => {
        return (event: React.FormEvent<any>, newValue?: string) => {
            setState({ field: fieldName, value: (newValue || '') });
        }
    };

    const onComboBoxChange = (fieldName: keyof ConfigFileMetadata) => {
        return (event: React.FormEvent<IComboBox>, option?: IComboBoxOption) => {
            let newVal: string | undefined;
            if (option === undefined || option.key === undefined || option.key === '') {
                newVal = undefined;
            } else {
                newVal = String(option.key);
            }
            setState({ field: fieldName, value: newVal });
        }
    }

    return (
        <Editor
            isOpen={props.isOpen}
            isBlocking={false}
            headerText={"Edit configuration"}
            onDismiss={props.onDismiss}
            onSubmit={() => {
                if (props.onSubmit) {
                    if (configFileData.fileType === 'poudriereconf') {
                        // May not have a portset or ports tree.
                        setState({ field: 'portSet', value: undefined });
                        setState({ field: 'portsTree', value: undefined });
                    }
                    props.onSubmit(configFileData);
                }
            }}>
            <TextField
                label="GUID"
                value={configFileData.id || ''}
                disabled={true}
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
            <ComboBoxWithFetcher<Jail>
                dataUrl="/api/jails"
                disabled={configFileData.fileType == 'poudriereconf'}
                label="Jail"
                selectedKey={configFileData.jail || ''}
                onChange={onComboBoxChange('jail')}
                onInputValueChange={(val: string) => {
                    if (val === '') {
                        // Text box blanked; clear jail selection.
                        setState({ field: "jail", value: undefined });
                    }
                }}
            />
            <ComboBoxWithFetcher<PortSet>
                dataUrl="/api/portsets"
                label="Port set"
                disabled={configFileData.fileType == 'poudriereconf'}
                onChange={onComboBoxChange('portSet')}
                selectedKey={configFileData.portSet || null}
                onInputValueChange={(val: string) => {
                    if (val === '') {
                        // Text box blanked; clear port set selection.
                        setState({ field: "portSet", value: undefined });
                    }
                }}
            />
            <ComboBoxWithFetcher<PortsTree>
                dataUrl="/api/portstrees"
                label="Ports tree"
                disabled={configFileData.fileType == 'poudriereconf'}
                onChange={onComboBoxChange('portsTree')}
                selectedKey={configFileData.portsTree || null}
                onInputValueChange={(val: string) => {
                    if (val === '') {
                        // Text box blanked; clear port set selection.
                        setState({ field: "portsTree", value: undefined });
                    }
                }}
            />
        </Editor>)
};
