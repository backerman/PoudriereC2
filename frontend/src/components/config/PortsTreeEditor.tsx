import { Dropdown, IDropdownOption, TextField } from "@fluentui/react";
import { useReducer, useState } from "react";
import { Editor } from "@/components/Editor";
import { PortsTree, PortsTreeMethod } from "@/models/portstrees";
import { validatePortableName, validityState } from "@/utils/utils";

export type PortsTreeEditorProps =
    {
        isOpen: boolean
        creatingNewRecord: boolean
        record: PortsTree
        onSubmit: (formData: PortsTree) => void
        onDismiss: () => void
    }

const methodChoices: IDropdownOption<PortsTreeMethod>[] = [
    { key: "git", text: "Git", data: 'git' },
    { key: "svn", text: "Subversion", data: 'svn' },
    { key: "null", text: "Null", data: 'null' }
];

// FIXME make it a flat object and just do the nesting for talking to the server.
export function PortsTreeEditor(props: PortsTreeEditorProps): JSX.Element {
    function updateState<K extends keyof PortsTree>(state: PortsTree,
        action: { field: K, value: PortsTree[K] } | PortsTree): PortsTree {
        let newState = { ...state };
        if (!("field" in action)) {
            // Must be PortsTree.
            newState = action;
        } else {
            switch (typeof action.value) {
                case "string":
                    newState[action.field] = action.value;
                    break;
                default:
                    throw Error("Unexpected type passed");
            }
        }
        return newState;
    }

    let [mostRecentPropsRecord, setMostRecentPropsRecord] = useState(props.record);
    let [portsTreeData, setState] =
        useReducer(updateState, {} as PortsTree);
    // The state isn't reinitialized when the props change, so do that
    // manually.
    if (props.record != mostRecentPropsRecord) {
        setState(props.record);
        setMostRecentPropsRecord(props.record);
    }
    const validity = validityState(props.record);
    let [validityData, setValidityData] = useReducer(validity.reducer, validity.initialState);
    const isFormValid = () => {
        for (const key of validityData.keys()) {
            if (!validityData.get(key)) {
                return false;
            }
        }
        return true;
    }

    return (
        <Editor
            isOpen={props.isOpen}
            isBlocking={false}
            headerText={"Edit ports tree “" + props.record.name + "”"}
            onSubmit={() =>
                props.onSubmit(portsTreeData)}
            submitDisabled={!isFormValid()}
            onDismiss={() => {
                // Reset the state to the original value.
                setState(props.record);
                props.onDismiss();
            }}>
            <TextField
                label={"Ports tree name"}
                value={portsTreeData.name}
                onChange={(_, val) =>
                    setState({ field: "name", value: val })} />
            <TextField
                label={"Ports tree portable name"}
                value={portsTreeData.portableName}
                onGetErrorMessage={(val) => {
                    let validity = validatePortableName(val);
                    setValidityData({ field: 'portableName', value: validity.isValid });
                    return validity.errMsg;
                }}
                onChange={(_, val) =>
                    setState({ field: "portableName", value: val })} />
            <Dropdown
                label="Fetching method"
                options={methodChoices}
                selectedKey={portsTreeData.method}
                onChange={(_, val: IDropdownOption<PortsTreeMethod> | undefined) => {
                    if (val != undefined && val.data != undefined) {
                        // val is one of our options so has the data field.
                        setState({ field: 'method', value: val.data });
                    }
                }} />
            <TextField
                label="Ports tree URI"
                value={portsTreeData.url || ''}
                onChange={(_, val) =>
                    setState({ field: "url", value: val })} />
        </Editor>)
}
