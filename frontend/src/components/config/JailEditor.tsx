import { Jail, JailMethodInfo, JailMethods } from "@/models/jails";
import { Editor } from "../Editor";
import { useReducer, useRef, useState } from "react";
import { Dropdown, IComboBox, IComboBoxOption, IDropdownOption, TextField } from "@fluentui/react";
import { ComboBoxWithFetcher } from "./ComboBoxWithFetcher";

export interface JailEditorProps {
    isOpen: boolean
    creatingNewRecord: boolean
    onSubmit?: (jail: Jail) => void
    onDismiss: () => void
    record: Jail
}

function updateState<K extends keyof Jail>(state: Jail,
    action: { field: K, value: Jail[K] } | Jail): Jail {
    let newState = { ...state };
    if (!("field" in action)) {
        // Must be Jail.
        newState = action;
    } else {
        switch (typeof action.value) {
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

const jailMethodChoices: IDropdownOption<JailMethodInfo>[] =
    JailMethods.map((method) => { return { key: method.name, text: method.name, data: method }; });

export function JailEditor(props: JailEditorProps): JSX.Element {
    const {
        isOpen,
        onDismiss,
        onSubmit,
        record,
        creatingNewRecord
    } = props;

    let [mostRecentPropsRecord, setMostRecentPropsRecord] = useState(record);
    let [jailData, setState] = useReducer(updateState, {} as Jail);
    let methodRef = useRef<JailMethodInfo | undefined>(JailMethods.find((method) => method.name === jailData.method));
    // The state isn't reinitialized when the props change, so do that
    // manually.
    if (record != mostRecentPropsRecord) {
        setState(record);
        setMostRecentPropsRecord(record);
        methodRef.current = JailMethods.find((method) => method.name === record.method)
    }

    const onTextChange = (fieldName: keyof Jail) => {
        return (_: any, newValue?: string) => {
            setState({ field: fieldName, value: (newValue || '') });
        }
    };

    const onComboBoxChange = (fieldName: keyof Jail) => {
        return (_: React.FormEvent<IComboBox>, option?: IComboBoxOption) => {
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
            isOpen={isOpen}
            isBlocking={false}
            onDismiss={onDismiss}
            headerText={`${props.creatingNewRecord ? 'Create' : 'Edit'} jail ${props.record.name}`}
            onSubmit={() => {
                if (onSubmit) {
                    switch (methodRef.current?.requiresParameter) {
                        case undefined:
                            console.error("No method set or unable to identify");
                            break;
                        case 'url':
                            jailData.path = undefined;
                            break;
                        case 'path':
                            jailData.url = undefined;
                            break;
                        case 'none':
                            jailData.url = undefined;
                            jailData.path = undefined;
                            break;
                    }
                    onSubmit(jailData);
                }
            }}>
            <TextField
                label="GUID"
                value={jailData.id || ''}
                disabled={creatingNewRecord}
                readOnly={true}
            />
            <TextField
                label="Name"
                value={jailData.name || ''}
                onChange={onTextChange('name')}
            />
            <ComboBoxWithFetcher<string>
                dataUrl="/api/freebsd/arch"
                label="Architecture"
                selectedKey={jailData.architecture || ''}
                onChange={onComboBoxChange('architecture')}
                onInputValueChange={(val: string) => {
                    if (val === '') {
                        // Text box blanked; clear port set selection.
                        setState({ field: "architecture", value: undefined });
                    }
                }}
            />
            <Dropdown
                options={jailMethodChoices}
                label="Method"
                placeholder="Select an installation method"
                selectedKey={jailData.method || ''}
                onChange={(_, val: IDropdownOption<JailMethodInfo> | undefined) => {
                    setState({ field: "method", value: val?.key.toString() });
                    methodRef.current = val?.data;
                }}
            />
            <ComboBoxWithFetcher<string>
                dataUrl={"/api/freebsd/arch/" + (jailData.architecture || '') + "/releases"}
                disabled={jailData.architecture === undefined}
                label="Version"
                noResultsMessage="The selected architecture has no available binary distributions."
                selectedKey={jailData.version || ''}
                onChange={onComboBoxChange('version')}
                onInputValueChange={(val: string) => {
                    if (val === '') {
                        // Text box blanked; clear port set selection.
                        setState({ field: "version", value: undefined });
                    }
                }}
            />
            <TextField
                label={"URL"}
                disabled={methodRef.current?.requiresParameter != 'url'}
                value={jailData.url || ''}
                onChange={onTextChange('url')}
            />
            <TextField
                label={"Path"}
                disabled={methodRef.current?.requiresParameter != 'path'}
                value={jailData.path || ''}
                onChange={onTextChange('path')}
            />
        </Editor>
    );
}
