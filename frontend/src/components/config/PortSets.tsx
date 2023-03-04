import { ActionButton, CommandBar, ContextualMenu, DefaultButton, Dialog, DialogFooter, DialogType, IColumn, ICommandBarItemProps, IDialogContentProps, IModalProps, ITextField, PrimaryButton, Selection, TextField } from "@fluentui/react";
import { useRef, useState } from "react";
import { useBoolean } from '@fluentui/react-hooks';
import { PortSet } from "src/models/portsets";
import { ItemList } from "../ItemList";
import { sortBy } from "src/utils/utils";
import { PortSetEditor } from "./PortSetEditor";
import { fetcher, FunctionResult } from "@/utils/fetcher";
import useSWR from 'swr';

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

    const addContentProps: IDialogContentProps = {
        type: DialogType.normal,
        title: 'Create port set',
        subText: 'Please specify the name of the port set to be created.'
    }

    const deleteConfimProps: IDialogContentProps = {
        type: DialogType.normal,
        title: 'Delete port sets',
        subText: 'Are you sure you want to delete the selected port sets?'
    }

    const commandBarItems: ICommandBarItemProps[] = [
        {
            key: 'add',
            text: 'Add',
            iconProps: { iconName: 'Add' },
            onClick: () => showAddDialog()
        },
        {
            key: 'delete',
            text: 'Delete',
            iconProps: { iconName: 'Delete' },
            ariaDescription: 'Delete selected port sets',
            disabled: deleteButtonDisabled,
            onClick: () => showDeleteDialog()
        }
    ]

    const draggableProps : IModalProps = {
        dragOptions: {
            moveMenuItemText: 'Move',
            closeMenuItemText: 'Close',
            menu: ContextualMenu
        }
    }

    return (
        <div className={"PortSets"}>
            <PortSetEditor
                createNewRecord={creatingNewRecord}
                isOpen={editorIsOpen}
                onDismiss={closeEditor}
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
                        }, { revalidate: false})
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
                    closeEditor();
                }} />
            <CommandBar
                items={commandBarItems} />
            <Dialog
                hidden={addDialogHidden}
                modalProps={draggableProps}
                dialogContentProps={addContentProps}
                onDismiss={hideAddDialog}>
                <TextField label={"Name"} componentRef={addNameRef}/>
                <DialogFooter>
                    <DefaultButton onClick={() => {
                        setActiveRecord({
                            ...emptyPortSet,
                            name: addNameRef.current?.value || ''
                        });
                        setCreatingNewRecord();
                        openEditor();
                        hideAddDialog();
                    }} text="Select ports"/>
                    <ActionButton onClick={hideAddDialog} text="Cancel" />
                </DialogFooter>
            </Dialog>
            <Dialog
                hidden={deleteDialogHidden}
                modalProps={draggableProps}
                dialogContentProps={deleteConfimProps}
                onDismiss={hideDeleteDialog} >
                <DialogFooter>
                    <PrimaryButton onClick={
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
                        }
                    } text="Delete" />
                    <DefaultButton onClick={hideDeleteDialog} text="Cancel" />
                </DialogFooter>
            </Dialog>
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