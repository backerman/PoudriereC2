import { IComboBox, IComboBoxOption, TextField } from "@fluentui/react";
import { useBoolean } from '@fluentui/react-hooks';
import { useEffect, useReducer } from "react";
import { Editor } from "@/components/Editor";
import { ComboBoxWithFetcher } from "./ComboBoxWithFetcher";
import { PortSet } from "@/models/portsets";
import { PortsTree } from "@/models/portstrees";
import { Jail } from "@/models/jails";
import { JobConfig } from "@/models/jobconfigs";
import { ConfigFileMetadata } from "@/models/configs";

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
    let [submitted, { setFalse: clearSubmitted, setTrue: setSubmitted }] = useBoolean(false);
    let [jobConfig, setJobConfig] =
        useReducer(updateMetadataState, {} as JobConfig);

    // Reset state when props change.
    useEffect(() => {
        setJobConfig(props.record);
        clearSubmitted();
    }, [props.record, clearSubmitted])

    const onTextChange = (fieldName: keyof JobConfig) => {
        return (event: React.FormEvent<any>, newValue?: string) => {
            clearSubmitted();
            setJobConfig({ field: fieldName, value: (newValue || '') });
        }
    };

    const onComboBoxChange = (fieldName: keyof JobConfig) => {
        return (event: React.FormEvent<IComboBox>, option?: IComboBoxOption) => {
            clearSubmitted();
            let newVal: string | undefined;
            if (option === undefined || option.key === undefined || option.key === '') {
                newVal = undefined;
            } else {
                newVal = String(option.key);
            }
            setJobConfig({ field: fieldName, value: newVal });
        }
    };

    const errorMessage = (fieldName: keyof JobConfig, fieldNameHumanReadable: string = fieldName.charAt(0).toLocaleUpperCase() + fieldName.slice(1)) =>
        submitted && (jobConfig[fieldName] === undefined || jobConfig[fieldName] === '') ? `${fieldNameHumanReadable} is required` : undefined

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
                setSubmitted();
                if (jobConfig.name === undefined || jobConfig.name === '') {
                    return;
                }
                if (jobConfig.jail === undefined || jobConfig.jail === '') {
                    return;
                }
                if (jobConfig.portSet === undefined || jobConfig.portSet === '') {
                    return;
                }
                if (jobConfig.portsTree === undefined || jobConfig.portsTree === '') {
                    return;
                }
                if (jobConfig.poudriereConf === undefined || jobConfig.poudriereConf === '') {
                    return;
                }
                return props.onSubmit ? props.onSubmit(jobConfig) : null;
            }}>
            <TextField
                label="GUID"
                required={true}
                value={jobConfig.id || ''}
                readOnly={true} />
            <TextField
                label="Name"
                required={true}
                errorMessage={errorMessage("name")}
                data-testid="config-file-name"
                value={jobConfig.name || ''}
                onChange={onTextChange("name")} />
            <ComboBoxWithFetcher<ConfigFileMetadata>
                dataUrl="/api/configurationfiles"
                filter={(item) => item.fileType === "poudriereconf"}
                required={true}
                errorMessage={errorMessage("poudriereConf", "poudriere.conf")}
                label="poudriere.conf"
                selectedKey={jobConfig.poudriereConf || ''}
                onChange={onComboBoxChange('poudriereConf')}
                onInputValueChange={(val: string) => {
                    if (val === '') {
                        // Text box blanked; clear jail selection.
                        setJobConfig({ field: "poudriereConf", value: undefined });
                    }
                }}
            />
            <ComboBoxWithFetcher<Jail>
                dataUrl="/api/jails"
                required={true}
                errorMessage={errorMessage("jail")}
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
                required={true}
                errorMessage={errorMessage("portSet", "Port set")}
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
                required={true}
                errorMessage={errorMessage("portsTree", "Ports tree")}
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
        </Editor>)
};
