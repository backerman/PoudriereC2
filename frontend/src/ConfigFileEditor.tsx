import { DefaultButton, Dropdown, IDropdownOption, Panel, PrimaryButton, Stack, TextField } from "@fluentui/react";
import { useCallback, useReducer } from "react";
import { ConfigFileMetadata, ConfigFileRepository } from "./model/configs";

export type ConfigFileEditorProps =
    {
        isOpen: boolean
        recordId: string
        dataSource: ConfigFileRepository
        onSubmit: (formData: ConfigFileMetadata) => void
        onDismiss: () => void
    }

const buttonStyles = { root: { marginRight: 8 } };

const fileTypeChoices: IDropdownOption<string>[] = [
    { key: "poudriereconf", text: "poudriere.conf" },
    { key: "makeconf", text: "make.conf" },
    { key: "srcconf", text: "src.conf" }
];

export const ConfigFileEditor: React.FC<ConfigFileEditorProps> =
    (props: ConfigFileEditorProps) => {

        function updateState<K extends keyof ConfigFileMetadata>(state: ConfigFileMetadata,
            action: { field: K, value: string | boolean | undefined }): ConfigFileMetadata {
            let newState = { ...state };
            switch (typeof action.value) {
                case "boolean":
                    if (action.field === "deleted") {
                        newState.deleted = action.value ?? false; break;
                    }
                    break;
                case "string":
                case "undefined":
                    switch (action.field) {
                        case "id":
                            newState.id = action.value ?? ""; break;
                        case "name":
                            newState.name = action.value ?? ""; break;
                        case "portSet":
                            newState.portSet = action.value ?? ""; break;
                        case "portsTree":
                            newState.portsTree = action.value ?? ""; break;
                        case "jail":
                            newState.jail = action.value ?? ""; break;
                        case "fileType":
                            newState.fileType = action.value ?? ""; break;
                    }
                    break;
                default:
                    throw Error("Unexpected type passed");
            }
            return newState;
        }

        let [configFileData, setState] =
            useReducer(updateState, props.dataSource.getConfigFile(props.recordId)
                ?? {} as ConfigFileMetadata)
        let { onDismiss, onSubmit } = props
        const onRenderFooterContent = useCallback(
            () => {
                return (
                    <div>
                        <PrimaryButton onClick={() => onSubmit(configFileData)} styles={buttonStyles}>
                            Save
                        </PrimaryButton>
                        <DefaultButton onClick={onDismiss}>Cancel</DefaultButton>
                    </div>
                )
            },
            [onDismiss, onSubmit, configFileData]
        );
        return (
            <Panel
                isOpen={props.isOpen}
                isBlocking={true}
                headerText={"Edit configuration"}
                closeButtonAriaLabel={"Close"}
                onRenderFooterContent={onRenderFooterContent}
                isFooterAtBottom={true}
                onDismiss={props.onDismiss}>
                <Stack verticalAlign="start">
                    <TextField
                        label="GUID"
                        defaultValue={configFileData.id}
                        contentEditable={false}
                        onChange={(_, val) => setState({ field: "id", value: val })} />
                    <TextField
                        label="Name"
                        defaultValue={configFileData.name}
                        onChange={(_, val) => setState({ field: "name", value: val })} />
                    <Dropdown
                        label="File type"
                        placeholder="Select a file type"
                        selectedKey={configFileData.fileType}
                        options={fileTypeChoices}
                        onChange={(_, val) => setState({ field: "fileType", value: val?.key.toString() })} />
                    <TextField
                        label="Jail"
                        defaultValue={configFileData.jail}
                        onChange={(_, val) => setState({ field: "jail", value: val })} />
                    <TextField
                        label="Port set"
                        defaultValue={configFileData.portSet}
                        onChange={(_, val) => setState({ field: "portSet", value: val })} />
                    <TextField
                        label="Ports tree"
                        defaultValue={configFileData.portsTree}
                        onChange={(_, val) => setState({ field: "portsTree", value: val })} />
                </Stack>
            </Panel>)
    };
