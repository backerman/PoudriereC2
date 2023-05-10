import { PortsTree } from '@/models/portstrees';
import { FunctionResult } from '@/utils/fetcher';
import { IColumn, ITextField, Selection } from '@fluentui/react';
import { useBoolean } from '@fluentui/react-hooks';
import { useRef, useState } from 'react';
import { ItemList } from 'src/components/ItemList';
import useSWR from 'swr';
import { ConfigCommandBar } from './ConfigCommandBar';
import { PortsTreeEditor } from './PortsTreeEditor';
import { useAzureFunctionsOAuth } from '@/utils/apiAuth';

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

export function PortsTrees(): JSX.Element {
    const [editorIsOpen, { setTrue: openEditor, setFalse: closeEditor }] = useBoolean(false);
    const [activeRecord, setActiveRecord] = useState<PortsTree | undefined>(undefined);
    const { fetcher, keyIfTokenReady } = useAzureFunctionsOAuth();
    const { data, error, isLoading, mutate } = useSWR<PortsTree[]>(keyIfTokenReady('/api/portstrees'), fetcher);
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

    // Putting them here to add them with spread operator
    const addDeleteParams = {
        addNameRef,
        addDialogHidden,
        hideAddDialog,
        showAddDialog,
        deleteDialogHidden,
        hideDeleteDialog,
        showDeleteDialog,
        deleteButtonDisabled
    }

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
                            const result = await fetcher<FunctionResult>('/api/portstrees', {
                                method: 'POST',
                                data: tree
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
                                fetcher(`/api/portstrees/${tree.id}`,
                                    {
                                        method: 'PUT',
                                        data: tree
                                    });
                            return data?.map((r) => r.id === tree.id ? tree : r);
                        });
                }
                closeEditor();
            }}
            onDismiss={closeEditor} />
        <ConfigCommandBar
            {...addDeleteParams}
            addConfirmButtonText='Configure ports tree'
            singularItemName='ports tree'
            pluralItemName='ports trees'
            onAddConfirmClick={() => {
                setActiveRecord({
                    id: undefined,
                    name: addNameRef.current?.value || '',
                    method: 'null',
                    url: undefined
                });
                setCreatingNewRecord();
                openEditor();
                hideAddDialog();
            }}
            onDeleteConfirmClick={async () => {
                await mutate(
                    async () => {
                        const pses = selection.getSelection() as PortsTree[];
                        hideDeleteDialog();
                        for (const ps of pses) {
                            const result = await fetcher<FunctionResult>(
                                `/api/portstrees/${ps.id}`,
                                {
                                    method: 'DELETE'
                                });
                            if (result.error) {
                                throw new Error(result.error);
                            }
                        };
                        return data?.filter((r) => !pses.includes(r));
                    });
            }}
        />
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
            } : undefined}
            onItemInvoked={(item: PortsTree) => {
                setActiveRecord(item);
                openEditor();
            }}
        />
    </div>)
}
