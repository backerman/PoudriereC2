import { FunctionResult, fetcher } from "@/utils/fetcher";
import { IColumn, ITextField, Selection } from "@fluentui/react";
import { useBoolean } from '@fluentui/react-hooks';
import { useRef, useState } from "react";
import { PortSet } from "src/models/portsets";
import useSWR from 'swr';
import { ItemList } from "../ItemList";
import { ConfigCommandBar } from "./ConfigCommandBar";
import { PortSetEditor } from "./PortSetEditor";

const columns: IColumn[] = [
    {
        key: 'name',
        name: 'Name',
        fieldName: 'name',
        isSorted: true,
        isSortedDescending: false,
        minWidth: 75,
        isResizable: true,
        targetWidthProportion: 0.75,
        isCollapsible: false,
        isRowHeader: true
    },
    {
        key: 'numPorts',
        name: '# ports',
        fieldName: 'portSet',
        minWidth: 50,
        isResizable: true,
        targetWidthProportion: 0.25,
        isCollapsible: false,
        onRender: (ps: PortSet) => ps.origins.length
    }
]

function computeMutations(oldPs: PortSet, newPs: PortSet) {
    const toAdd = newPs.origins.filter(o => !oldPs.origins.includes(o));
    const toDelete = oldPs.origins.filter(o => !newPs.origins.includes(o));
    return { toAdd, toDelete };
}

const emptyPortSet: PortSet = {
    id: '',
    name: '',
    origins: []
}

export function PortSets(): JSX.Element {
    const [editorIsOpen, { setTrue: openEditor, setFalse: closeEditor }] = useBoolean(false);
    const [activeRecord, setActiveRecord] = useState<PortSet | undefined>(undefined);
    const { data, error, isLoading, mutate } = useSWR<PortSet[]>('/api/portsets', fetcher);
    const [addDialogHidden, { setTrue: hideAddDialog, setFalse: showAddDialog }] = useBoolean(true);
    const [deleteDialogHidden, { setTrue: hideDeleteDialog, setFalse: showDeleteDialog }] = useBoolean(true);
    // Whether the delete button is enabled; it should be enabled
    // iff at least one item is selected.
    const [deleteButtonDisabled, setDeleteButtonDisabled] = useState(true);

    const [creatingNewRecord, { setTrue: setCreatingNewRecord, setFalse: clearCreatingNewRecord }] = useBoolean(false);
    const addNameRef = useRef<ITextField>(null);
    const [selection] = useState(new Selection({
        getKey: (ps) => (ps as PortSet).id,
        onSelectionChanged: () => {
            setDeleteButtonDisabled(selection.getSelectedCount() === 0);
        }
    }));

    // Putting them here to add them with spread operator
    const addDeleteParams = {
        addDialogHidden,
        hideAddDialog,
        showAddDialog,
        deleteDialogHidden,
        hideDeleteDialog,
        showDeleteDialog
    }

    return (
        <div className={"PortSets"}>
            <PortSetEditor
                createNewRecord={creatingNewRecord}
                isOpen={editorIsOpen}
                onDismiss={() => {
                    clearCreatingNewRecord();
                    closeEditor();
                }}
                record={activeRecord}
                onSubmit={async (ps: PortSet) => {
                    if (!activeRecord) {
                        console.log("Error: no active record exists")
                    } else if (activeRecord.id !== ps.id) {
                        console.log("Error: active record id does not match submitted record id")
                    } else if (creatingNewRecord) {
                        await mutate(async () => {
                            const result = await fetcher<FunctionResult>('/api/portsets', {
                                method: 'POST',
                                body: JSON.stringify({
                                    ...ps,
                                    // The server will assign a GUID to the new port set.
                                    id: undefined
                                })
                            });
                            if (result.error) {
                                throw new Error(result.error);
                            }
                            if (!result.guid) {
                                throw new Error("No GUID returned from server");
                            }
                            // Return the new portset array.
                            return data?.concat({
                                ...ps,
                                id: result.guid
                            });
                        }, { revalidate: false })
                    } else {
                        const { toAdd, toDelete } = computeMutations(activeRecord, ps);
                        const actions = [
                            { action: 'add', ports: toAdd },
                            { action: 'delete', ports: toDelete }
                        ].filter(a => a.ports.length > 0);
                        var result: FunctionResult;
                        await mutate(
                            async () => {
                                result = await fetcher<FunctionResult>(`/api/portsets/${ps.id}/members`, {
                                    method: 'PATCH',
                                    body: JSON.stringify(actions)
                                });
                                if (result.error) {
                                    throw new Error(result.error);
                                }
                                // Return the updated list of port sets
                                return data?.map((r) => r.id === ps.id ? ps : r);
                            });
                    }
                    clearCreatingNewRecord();
                    closeEditor();
                }} />
            <ConfigCommandBar
                {...addDeleteParams}
                addConfirmButtonText={"Select ports"}
                addNameRef={addNameRef}
                deleteButtonDisabled={deleteButtonDisabled}
                pluralItemName={"port sets"}
                singularItemName={"port set"}
                onAddConfirmClick={() => {
                    setActiveRecord({
                        ...emptyPortSet,
                        name: addNameRef.current?.value || ''
                    });
                    setCreatingNewRecord();
                    openEditor();
                    hideAddDialog();
                }}
                onDeleteConfirmClick={
                    async () => {
                        await mutate(
                            async () => {
                                const pses = selection.getSelection() as PortSet[];
                                hideDeleteDialog();
                                for (const ps of pses) {
                                    const result = await fetcher<FunctionResult>(`/api/portsets/${ps.id}`, {
                                        method: 'DELETE'
                                    });
                                    if (result.error) {
                                        throw new Error(result.error);
                                    }
                                };
                                return data?.filter((r) => !pses.includes(r));
                            });
                    }} />
            <ItemList
                ariaLabelForGrid={"List of port sets"}
                getRowAriaLabel={(r: PortSet) => r.name}
                columns={columns}
                items={data || []}
                selection={selection}
                enableShimmer={isLoading}
                error={error?.toString()}
                getKey={(r: PortSet) => { return r?.id; }}
                onItemInvoked={(item: PortSet) => {
                    setActiveRecord(item);
                    openEditor();
                }}
            />
        </div>
    )
}
