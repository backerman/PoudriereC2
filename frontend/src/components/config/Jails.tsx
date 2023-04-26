import { Jail } from "@/models/jails";
import { FunctionResult, fetcher } from "@/utils/fetcher";
import { IColumn, ITextField, Selection } from "@fluentui/react";
import { useBoolean } from '@fluentui/react-hooks';
import { useRef, useState } from "react";
import useSWR from 'swr';
import { ItemList } from "../ItemList";
import { JailEditor } from "./JailEditor";
import { revalidateEvents } from "swr/_internal";

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
        key: 'url',
        name: 'URL',
        fieldName: 'url',
        minWidth: 200,
        maxWidth: 400,
        isResizable: true,
        targetWidthProportion: 0.5,
        isCollapsible: true
    },
]

export function Jails(): JSX.Element {
    const [editorIsOpen, { setTrue: openEditor, setFalse: closeEditor }] = useBoolean(false);
    const [activeRecord, setActiveRecord] = useState({} as Jail);
    const { data, error, isLoading, mutate } = useSWR<Jail[], string>('/api/jails', fetcher);
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

    return (
        <div className={"Jails"}>
            <JailEditor
                isOpen={editorIsOpen}
                record={activeRecord || {} as Jail}
                onDismiss={() => {
                    closeEditor();
                    clearCreatingNewRecord();
                }}
                onSubmit={async (jail: Jail) => {
                    if (!activeRecord) {
                        errorThrown("No active record exists")
                    } else if (creatingNewRecord) {
                        // TODO
                    } else {
                        await mutate(
                            async () => {
                                await
                                    fetcher<FunctionResult>('/api/jails/' + jail.id,
                                        {
                                            method: 'PUT',
                                            body: JSON.stringify(jail)
                                        });
                                return data?.map((r) => r.id === jail.id ? jail : r);
                            }, { revalidate: false }
                        ).catch((e) => errorThrown(e?.toString()));
                    }
                    closeEditor();
                    clearCreatingNewRecord();
                }}
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
