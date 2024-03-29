import { Jail } from "@/models/jails";
import { FunctionResult } from "@/utils/fetcher";
import { IColumn, ITextField, Selection } from "@fluentui/react";
import { useBoolean } from '@fluentui/react-hooks';
import { useRef, useState } from "react";
import useSWR from 'swr';
import { ItemList } from "../ItemList";
import { JailEditor } from "./JailEditor";
import { ConfigCommandBar } from "./ConfigCommandBar";
import { useAzureFunctionsOAuth } from "@/utils/apiAuth";
import { makeNamePortable } from "@/utils/utils";

const columns: IColumn[] = [
    {
        key: 'name',
        name: 'Name',
        fieldName: 'name',
        isSorted: true,
        isSortedDescending: false,
        minWidth: 100,
        maxWidth: 500,
        isResizable: true,
        targetWidthProportion: 0.5,
        isCollapsible: false,
        isRowHeader: true,
    },
    {
        key: 'version',
        name: 'OS version',
        fieldName: 'version',
        minWidth: 150,
        maxWidth: 250,
        isResizable: true,
        targetWidthProportion: 0.2,
        isCollapsible: true,
    },
    {
        key: 'architecture',
        name: 'Architecture',
        fieldName: 'architecture',
        minWidth: 100,
        maxWidth: 250,
        isResizable: true,
        targetWidthProportion: 0.2,
        isCollapsible: true,
    },
    {
        key: 'method',
        name: 'Method',
        fieldName: 'method',
        minWidth: 100,
        maxWidth: 250,
        isResizable: true,
        targetWidthProportion: 0.2,
        isCollapsible: true,
    },
    {
        key: 'url-path',
        name: 'URL/path',
        fieldName: 'url-path',
        minWidth: 200,
        maxWidth: 400,
        isResizable: true,
        targetWidthProportion: 0.5,
        isCollapsible: true,
        // Render URL or path
        onRender: (item: Jail) =>
            item.url ?? item.path
    },
]

export function Jails(): JSX.Element {
    const [editorIsOpen, { setTrue: openEditor, setFalse: closeEditor }] = useBoolean(false);
    const [activeRecord, setActiveRecord] = useState({} as Jail);

    const { fetcher, keyIfTokenReady } = useAzureFunctionsOAuth();
    const { data, error, isLoading, mutate } = useSWR<Jail[], string>(keyIfTokenReady('/api/jails'), fetcher);

    const [editorError, errorThrown] = useState<string | undefined>(undefined);
    const [addDialogHidden, { setTrue: hideAddDialog, setFalse: showAddDialog }] = useBoolean(true);
    const [deleteDialogHidden, { setTrue: hideDeleteDialog, setFalse: showDeleteDialog }] = useBoolean(true);
    // Whether the delete button is enabled; it should be enabled
    // iff at least one item is selected.
    const [deleteButtonDisabled, setDeleteButtonDisabled] = useState(true);
    const [creatingNewRecord, { setTrue: setCreatingNewRecord, setFalse: clearCreatingNewRecord }] = useBoolean(false);
    const addNameRef = useRef<ITextField>(null);
    const [selection] = useState(new Selection({
        getKey: (j) => (j as Jail).id || '',
        onSelectionChanged: () => {
            setDeleteButtonDisabled(selection.getSelectedCount() === 0);
        }
    }));

    // Putting them here to add them to ConfigCommandBar with spread operator
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

    return (
        <div className={"Jails"}>
            <JailEditor
                isOpen={editorIsOpen}
                creatingNewRecord={creatingNewRecord}
                record={activeRecord || {} as Jail}
                onDismiss={() => {
                    closeEditor();
                    clearCreatingNewRecord();
                }}
                onSubmit={async (jail: Jail) => {
                    if (!activeRecord) {
                        errorThrown("No active record exists")
                    } else if (creatingNewRecord) {
                        await mutate(
                            async () => {
                                const result = await fetcher<FunctionResult>('/api/jails',
                                    {
                                        method: 'POST',
                                        data: jail
                                    });
                                if (result.error) {
                                    throw new Error(result.error);
                                }
                                if (!result.guid) {
                                    throw new Error("No GUID returned");
                                }
                                errorThrown(undefined);
                                return data?.concat({
                                    ...jail,
                                    id: result.guid
                                });
                            }
                        )
                    } else {
                        await mutate(
                            async () => {
                                await
                                    fetcher<FunctionResult>('/api/jails/' + jail.id,
                                        {
                                            method: 'PUT',
                                            data: jail
                                        });
                                return data?.map((r) => r.id === jail.id ? jail : r);
                            }, { revalidate: false }
                        ).catch((e) => errorThrown(e?.toString()))
                            .then(() => errorThrown(undefined));
                    }
                    closeEditor();
                    clearCreatingNewRecord();
                }}
            />
            <ConfigCommandBar
                {...addDeleteParams}
                singularItemName={"jail"}
                pluralItemName={"jails"}
                addConfirmButtonText={"Configure"}
                onAddConfirmClick={async () => {
                    const name = addNameRef.current?.value || '';
                    setActiveRecord({
                        name: name,
                        portableName: makeNamePortable(name),
                    });
                    setCreatingNewRecord();
                    hideAddDialog();
                    openEditor();
                }}
                onDeleteConfirmClick={
                    async () => {
                        await mutate(async () => {
                            const sel = selection.getSelection() as Jail[];
                            hideDeleteDialog();
                            for (const j of sel) {
                                const result = await fetcher<FunctionResult>(
                                    '/api/jails/' + j.id,
                                    { method: 'DELETE' });
                                if (result.error) {
                                    throw new Error(result.error);
                                }
                            }
                            return data?.filter((r) => !sel.includes(r));
                        }).catch((e: Error) => errorThrown(e.message));
                    }
                }
            />
            <ItemList
                enableShimmer={isLoading}
                ariaLabelForGrid={"List of configuration files"}
                getRowAriaLabel={(r: Jail) => r.name}
                error={error?.toString() ?? editorError}
                items={data || []}
                columns={columns}
                selection={selection}
                getKey={data ? (j: Jail) => {
                    const key = j.id || 'undefined';
                    return key;
                } : undefined}
                onItemInvoked={(item: Jail) => {
                    setActiveRecord(item);
                    openEditor();
                }}
            />
        </div>
    );
}
