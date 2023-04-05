import { useRef, useState } from 'react';
import { ActionButton, CommandBar, ContextualMenu, DefaultButton, Dialog, DialogFooter, DialogType, IColumn, ICommandBarItemProps, IDialogContentProps, IModalProps, ITextField, PrimaryButton, Selection, TextField } from '@fluentui/react';
import { useBoolean } from '@fluentui/react-hooks';
import { ItemList } from 'src/components/ItemList';
import useSWR from 'swr';
import { FunctionResult, fetcher } from '@/utils/fetcher';
import { PortsTree } from '@/models/portstrees';
import { PortsTreeEditor } from './PortsTreeEditor';

const columns: IColumn[] = [
    {
        key: 'name',
        name: 'Name',
        fieldName: 'name',
        isSorted: true,
        isSortedDescending: false,
        minWidth: 100,
        maxWidth: 300,
        isResizable: true,
        targetWidthProportion: 0.2,
        isCollapsible: false,
        isRowHeader: true,
    },
    {
        key: 'method',
        name: 'Method',
        fieldName: 'method',
        minWidth: 100,
        maxWidth: 100,
        isResizable: true,
        targetWidthProportion: 0.2,
        isCollapsible: true
    },
    {
        key: 'url',
        name: 'URL',
        fieldName: 'url',
        minWidth: 150,
        maxWidth: 500,
        isResizable: true,
        targetWidthProportion: 0.5,
        isCollapsible: true
    }
]

const draggableProps: IModalProps = {
    dragOptions: {
        moveMenuItemText: 'Move',
        closeMenuItemText: 'Close',
        menu: ContextualMenu
    }
}

export function PortsTrees(): JSX.Element {
    const [editorIsOpen, { setTrue: openEditor, setFalse: closeEditor }] = useBoolean(false);
    const [activeRecord, setActiveRecord] = useState<PortsTree | undefined>(undefined);
    const { data, error, isLoading, mutate } = useSWR<PortsTree[]>('/api/portstrees', fetcher);
    const [addDialogHidden, { setTrue: hideAddDialog, setFalse: showAddDialog }] = useBoolean(true);
    const [deleteDialogHidden, { setTrue: hideDeleteDialog, setFalse: showDeleteDialog }] = useBoolean(true);
    // Whether the delete button is enabled; it should be enabled
    // iff at least one item is selected.
    const [deleteButtonDisabled, setDeleteButtonDisabled] = useState(true);

    const [creatingNewRecord, { setTrue: setCreatingNewRecord, setFalse: clearCreatingNewRecord }] = useBoolean(false);
    const addNameRef = useRef<ITextField>(null);
    const [selection] = useState(new Selection({
        getKey: (ps) => (ps as PortsTree).id || '',
        onSelectionChanged: () => {
            setDeleteButtonDisabled(selection.getSelectedCount() === 0);
        }
    }));

    const addContentProps: IDialogContentProps = {
        type: DialogType.normal,
        title: 'Create port set',
        subText: 'Please specify the name of the ports tree to be created.'
    }

    const deleteConfirmProps: IDialogContentProps = {
        type: DialogType.normal,
        title: 'Delete port sets',
        subText: 'Are you sure you want to delete the selected ports tree?'
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
            ariaDescription: 'Delete selected ports tree',
            disabled: deleteButtonDisabled,
            onClick: () => showDeleteDialog()
        }
    ]

    return (<div className={"PortsTrees"}>
        <PortsTreeEditor
            isOpen={editorIsOpen}
            creatingNewRecord={creatingNewRecord}
            record={activeRecord || {} as PortsTree}
            onSubmit={async (tree: PortsTree) => {
                if (!activeRecord) {
                    console.log("Error: no active record exists")
                } else if (creatingNewRecord) {
                    await mutate(
                        async () => {
                            // FIXME check for errors
                            const result = await fetcher<FunctionResult>('/api/portstrees', {
                                method: 'POST',
                                body: JSON.stringify(tree)
                            });
                            if (result.error) {
                                throw new Error(result.error);
                            }
                            if (!result.guid) {
                                throw new Error("No GUID returned from server");
                            }
                            return data?.concat({
                                ...tree,
                                id: result.guid
                            });
                        }, { revalidate: false });
                } else if (activeRecord.id !== tree.id) {
                    console.log("Error: active record id does not match submitted record id")
                } else {
                    await mutate(
                        async () => {
                            // FIXME check for errors
                            await
                                fetcher('/api/portstrees/' + tree.id,
                                    {
                                        method: 'PUT',
                                        body: JSON.stringify(tree)
                                    });
                            return data?.map((r) => r.id === tree.id ? tree : r);
                        });
                }
                closeEditor();
            }}
            onDismiss={closeEditor} />
        <CommandBar
            items={commandBarItems} />
        <Dialog
            hidden={addDialogHidden}
            modalProps={draggableProps}
            dialogContentProps={addContentProps}
            onDismiss={() => {
                hideAddDialog();
                clearCreatingNewRecord();
            }}>
            <TextField label={"Name"} componentRef={addNameRef} />
            <DialogFooter>
                <DefaultButton onClick={() => {
                    setActiveRecord({
                        id: undefined,
                        name: addNameRef.current?.value || '',
                        method: 'null',
                        url: undefined
                    });
                    setCreatingNewRecord();
                    openEditor();
                    hideAddDialog();
                }} text="Configure ports tree" />
                <ActionButton onClick={hideAddDialog} text="Cancel" />
            </DialogFooter>
        </Dialog>
        <Dialog
            hidden={deleteDialogHidden}
            modalProps={draggableProps}
            dialogContentProps={deleteConfirmProps}
            onDismiss={hideDeleteDialog} >
            <DialogFooter>
                <PrimaryButton onClick={
                    async () => {
                        await mutate(
                            async () => {
                                const pses = selection.getSelection() as PortsTree[];
                                hideDeleteDialog();
                                for (const ps of pses) {
                                    const result = await fetcher<FunctionResult>(`/api/portstrees/${ps.id}`, {
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
            enableShimmer={isLoading}
            ariaLabelForGrid={"List of ports trees"}
            getRowAriaLabel={(r: PortsTree) => r.name}
            error={error?.toString()}
            items={data || []}
            selection={selection}
            columns={columns}
            getKey={data ? (f: PortsTree) => {
                const key = f.id || 'undefined';
                return key;
            } : undefined }
            onItemInvoked={(item: PortsTree) => {
                setActiveRecord(item);
                openEditor();
            }}
        />
    </div>)
}
