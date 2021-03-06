import { Dropdown, IDropdownOption, TextField } from "@fluentui/react";
import { useCallback, useEffect, useReducer } from "react";
import Editor from "./Editor";
import { ConfigFileMetadata, ConfigFileRepository } from "../model/configs";

export type ConfigFileEditorProps =
    {
        isOpen: boolean
        recordId: string
        dataSource: ConfigFileRepository
        onSubmit: (formData: ConfigFileMetadata) => void
        onDismiss: () => void
    }

const fileTypeChoices: IDropdownOption<string>[] = [
    { key: "poudriereconf", text: "poudriere.conf" },
    { key: "makeconf", text: "make.conf" },
    { key: "srcconf", text: "src.conf" }
];

export const ConfigFileEditor: React.FC<ConfigFileEditorProps> =
    (props: ConfigFileEditorProps) => {

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

        let [configFileData, setState] =
            useReducer(updateState, {} as ConfigFileMetadata)

        useEffect(() => {
            if (props.recordId) {
                props.dataSource.getConfigFile(props.recordId)
                    .then((newRecord: ConfigFileMetadata | undefined) => {
                        setState(newRecord || {} as ConfigFileMetadata);
                    });
            }
        }, [props.dataSource, props.recordId]);

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
            onSubmit={() => {props.onSubmit(configFileData)}}>
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
