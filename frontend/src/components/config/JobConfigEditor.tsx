import {IComboBox, IComboBoxOption, PanelType, Separator, TextField } from "@fluentui/react";
import { useEffect, useReducer } from "react";
import { Editor } from "@/components/Editor";
import { ComboBoxWithFetcher } from "./ComboBoxWithFetcher";
import { PortSet } from "@/models/portsets";
import { PortsTree } from "@/models/portstrees";
import { Jail } from "@/models/jails";
import { JobConfig } from "@/models/jobconfigs";

export interface JobConfigEditorProps {
    isOpen: boolean
    creatingNewRecord: boolean
    record: JobConfig
    onSubmit?: (formData: JobConfig) => void
    onDismiss: () => void
}

function updateMetadataState<K extends keyof JobConfig>(state: JobConfig,
    action: { field: K, value: JobConfig[K] } | JobConfig): JobConfig {
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

export function JobConfigEditor(props: JobConfigEditorProps): JSX.Element {

    let [jobConfig, setJobConfig] =
        useReducer(updateMetadataState, {} as JobConfig);

    // Reset state when props change.
    useEffect(() => {
        setJobConfig(props.record);
    }, [props.record])

    const onTextChange = (fieldName: keyof JobConfig) => {
        return (event: React.FormEvent<any>, newValue?: string) => {
            setJobConfig({ field: fieldName, value: (newValue || '') });
        }
    };

    const onComboBoxChange = (fieldName: keyof JobConfig) => {
        return (event: React.FormEvent<IComboBox>, option?: IComboBoxOption) => {
            let newVal: string | undefined;
            if (option === undefined || option.key === undefined || option.key === '') {
                newVal = undefined;
            } else {
                newVal = String(option.key);
            }
            setJobConfig({ field: fieldName, value: newVal });
        }
    };

    return (
        <Editor
            isOpen={props.isOpen}
            isBlocking={false}
            headerText={`${props.creatingNewRecord ? "Create" : "Edit"} job configuration ${props.record.name || ""}`}
            onDismiss={props.onDismiss}
            onCancelButtonClicked={
                () => {
                    setJobConfig(props.record);
                    props.onDismiss();
                }
            }
            onSubmit={() => {
                return props.onSubmit ? props.onSubmit(jobConfig) : null
            }}>
            <TextField
                label="GUID"
                value={jobConfig.id || ''}
                readOnly={true} />
            <TextField
                label="Name"
                data-testid="config-file-name"
                value={jobConfig.name || ''}
                onChange={onTextChange("name")} />
            <ComboBoxWithFetcher<Jail>
                dataUrl="/api/jails"
                label="Jail"
                selectedKey={jobConfig.jail || ''}
                onChange={onComboBoxChange('jail')}
                onInputValueChange={(val: string) => {
                    if (val === '') {
                        // Text box blanked; clear jail selection.
                        setJobConfig({ field: "jail", value: undefined });
                    }
                }}
            />
            <ComboBoxWithFetcher<PortSet>
                dataUrl="/api/portsets"
                label="Port set"
                onChange={onComboBoxChange('portSet')}
                selectedKey={jobConfig.portSet || null}
                onInputValueChange={(val: string) => {
                    if (val === '') {
                        // Text box blanked; clear port set selection.
                        setJobConfig({ field: "portSet", value: undefined });
                    }
                }}
            />
            <ComboBoxWithFetcher<PortsTree>
                dataUrl="/api/portstrees"
                label="Ports tree"
                onChange={onComboBoxChange('portsTree')}
                selectedKey={jobConfig.portsTree || null}
                onInputValueChange={(val: string) => {
                    if (val === '') {
                        // Text box blanked; clear port set selection.
                        setJobConfig({ field: "portsTree", value: undefined });
                    }
                }}
            />
            <Separator />
        </Editor>)
};
