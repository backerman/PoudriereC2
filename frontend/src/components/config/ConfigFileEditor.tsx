import { Dropdown, IComboBox, IComboBoxOption, IDropdownOption, PanelType, Separator, TextField } from "@fluentui/react";
import { useEffect, useReducer, useState } from "react";
import { Editor } from "@/components/Editor";
import { ConfigFileMetadata, ConfigOption, ConfigOptionUpdate } from "src/models/configs";
import { ComboBoxWithFetcher } from "./ComboBoxWithFetcher";
import { PortSet } from "@/models/portsets";
import { PortsTree } from "@/models/portstrees";
import { Jail } from "@/models/jails";
import { ConfigFileEditorItemsList } from "./ConfigFileEditor.ItemsList";

export interface ConfigFileEditorProps {
    isOpen: boolean
    creatingNewRecord: boolean
    record: ConfigFileMetadata
    onSubmit?: (formData: ConfigFileMetadata, optionsMutations: ConfigOptionUpdate[], options: ConfigOption[]) => void
    onDismiss: () => void
    configOptions?: ConfigOption[]
}

const fileTypeChoices: IDropdownOption<string>[] = [
    { key: "poudriereconf", text: "poudriere.conf" },
    { key: "makeconf", text: "make.conf" },
    { key: "srcconf", text: "src.conf" }
];

function updateMetadataState<K extends keyof ConfigFileMetadata>(state: ConfigFileMetadata,
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
};

type AddOption = {
    action: 'add'
    option: ConfigOption
}

type DeleteOption = {
    action: 'delete'
    option: string
}

type StateUpdate = AddOption | DeleteOption;

type ConfigOptionsState = {
    options: ConfigOption[];
    mutations: StateUpdate[];
}

function updateOptionsState(state: ConfigOptionsState, action: StateUpdate | ConfigOption[]): ConfigOptionsState {
    const newState: ConfigOptionsState = {
        options: state.options.slice(),
        mutations: state.mutations.slice()
    }
    if (!("action" in action)) {
        // Must be an array of ConfigOptions.
        newState.options = action;
        newState.mutations = [];
    } else {
        switch (action.action) {
            case 'add':
                newState.options.push(action.option);
                break;
            case 'delete':
                const idx = newState.options.findIndex((opt) => opt.name === action.option);
                if (idx >= 0) {
                    newState.options.splice(idx, 1);
                }
                break;
        }
        newState.mutations.push({ ...action });
    }
    return newState;
}

export function ConfigFileEditor(props: ConfigFileEditorProps): JSX.Element {

    let [configFileMetadata, setMetadataState] =
        useReducer(updateMetadataState, {} as ConfigFileMetadata);
    let [configOptionsState, setConfigOptionsState] =
        useReducer(updateOptionsState, { options: props.configOptions || [], mutations: [] });

    // Reset state when props change.
    useEffect(() => {
        setMetadataState(props.record);
        setConfigOptionsState(props.configOptions || []);
    }, [props.record, props.configOptions])

    const onTextChange = (fieldName: keyof ConfigFileMetadata) => {
        return (event: React.FormEvent<any>, newValue?: string) => {
            setMetadataState({ field: fieldName, value: (newValue || '') });
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
            setMetadataState({ field: fieldName, value: newVal });
        }
    };

    return (
        <Editor
            type={PanelType.large}
            isOpen={props.isOpen}
            isBlocking={false}
            headerText={`${props.creatingNewRecord ? "Create" : "Edit"} configuration file ${props.record.name || ""}`}
            onDismiss={props.onDismiss}
            onCancelButtonClicked={
                () => {
                    setMetadataState(props.record);
                    setConfigOptionsState(props.configOptions || []);
                    props.onDismiss();
                }
            }
            onSubmit={() => {
                if (props.onSubmit) {
                    if (configFileMetadata.fileType === 'poudriereconf') {
                        // May not have a portset or ports tree.
                        setMetadataState({ field: 'portSet', value: undefined });
                        setMetadataState({ field: 'portsTree', value: undefined });
                    }
                    props.onSubmit(configFileMetadata, configOptionsState.mutations.map((mutation) => {
                        // TODO better batching
                        if (mutation.action === 'add') {
                            return { action: 'add', options: [mutation.option] };
                        } else {
                            return { action: 'delete', options: [mutation.option] };
                        }
                    }), configOptionsState.options.slice());
                }
            }}>
            <TextField
                label="GUID"
                value={configFileMetadata.id || ''}
                readOnly={true} />
            <TextField
                label="Name"
                data-testid="config-file-name"
                value={configFileMetadata.name || ''}
                onChange={onTextChange("name")} />
            <Dropdown
                label="File type"
                placeholder="Select a file type"
                selectedKey={configFileMetadata.fileType || ''}
                options={fileTypeChoices}
                onChange={(_, val) => setMetadataState({ field: "fileType", value: val?.key.toString() })} />
            <ComboBoxWithFetcher<Jail>
                dataUrl="/api/jails"
                disabled={configFileMetadata.fileType == 'poudriereconf'}
                label="Jail"
                selectedKey={configFileMetadata.jail || ''}
                onChange={onComboBoxChange('jail')}
                onInputValueChange={(val: string) => {
                    if (val === '') {
                        // Text box blanked; clear jail selection.
                        setMetadataState({ field: "jail", value: undefined });
                    }
                }}
            />
            <ComboBoxWithFetcher<PortSet>
                dataUrl="/api/portsets"
                label="Port set"
                disabled={configFileMetadata.fileType == 'poudriereconf'}
                onChange={onComboBoxChange('portSet')}
                selectedKey={configFileMetadata.portSet || null}
                onInputValueChange={(val: string) => {
                    if (val === '') {
                        // Text box blanked; clear port set selection.
                        setMetadataState({ field: "portSet", value: undefined });
                    }
                }}
            />
            <ComboBoxWithFetcher<PortsTree>
                dataUrl="/api/portstrees"
                label="Ports tree"
                disabled={configFileMetadata.fileType == 'poudriereconf'}
                onChange={onComboBoxChange('portsTree')}
                selectedKey={configFileMetadata.portsTree || null}
                onInputValueChange={(val: string) => {
                    if (val === '') {
                        // Text box blanked; clear port set selection.
                        setMetadataState({ field: "portsTree", value: undefined });
                    }
                }}
            />
            <Separator />
            <ConfigFileEditorItemsList
                items={configOptionsState.options}
                addClicked={(item) => {
                    setConfigOptionsState({ action: 'add', option: item });
                }}
                deleteClicked={(item) => {
                    setConfigOptionsState({ action: 'delete', option: item.name });
                }} />
        </Editor>)
};
