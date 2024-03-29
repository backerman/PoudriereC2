import { useCallback, useRef, useState } from 'react';
import { IColumn, ITextField, Selection } from '@fluentui/react';
import { ConfigFileMetadata, ConfigOption, ConfigOptionUpdate } from 'src/models/configs';
import { ConfigFileEditor } from './ConfigFileEditor';
import { useBoolean } from '@fluentui/react-hooks';
import { ItemList } from 'src/components/ItemList';
import useSWR from 'swr';
import { FunctionResult } from 'src/utils/fetcher';
import { ConfigCommandBar } from './ConfigCommandBar';
import { useAzureFunctionsOAuth } from "@/utils/apiAuth";

type ConfigFilesProps = {
    showDeleted?: boolean;
}

const columns: IColumn[] = [
    {
        key: 'fileType',
        name: 'Type',
        fieldName: 'fileType',
        minWidth: 50,
        maxWidth: 100,
        isResizable: true,
        targetWidthProportion: 0.2,
    },
    {
        key: 'name',
        name: 'Name',
        fieldName: 'name',
        isSorted: true,
        isSortedDescending: false,
        minWidth: 100,
        maxWidth: 1500,
        isResizable: true,
        targetWidthProportion: 0.2,
        isCollapsible: false,
        isRowHeader: true,
    },
    {
        key: 'portSet',
        name: 'Port set',
        fieldName: 'portSet',
        minWidth: 75,
        maxWidth: 250,
        isResizable: true,
        targetWidthProportion: 0.2,
        isCollapsible: true,
        onRender: (item: ConfigFileMetadata) =>
            item.portSetName
    },
    {
        key: 'portsTree',
        name: 'Ports tree',
        fieldName: 'portsTree',
        minWidth: 75,
        maxWidth: 250,
        isResizable: true,
        targetWidthProportion: 0.2,
        isCollapsible: true,
        onRender: (item: ConfigFileMetadata) =>
            item.portsTreeName
    },
    {
        key: 'jail',
        name: 'Jail',
        fieldName: 'jail',
        minWidth: 100,
        maxWidth: 250,
        isResizable: true,
        targetWidthProportion: 1,
        isCollapsible: true,
        onRender: (item: ConfigFileMetadata) =>
            item.jailName
    }
]

export function ConfigFiles(props: ConfigFilesProps): JSX.Element {
    const itemsFilter = useCallback((item: ConfigFileMetadata) => {
        if (props.showDeleted) {
            return true;
        }
        return !item.deleted;
    }, [props.showDeleted]);
    const [editorIsOpen, { setTrue: openEditor, setFalse: closeEditor }] = useBoolean(false);
    const { fetcher, keyIfTokenReady } = useAzureFunctionsOAuth();
    const [activeRecord, setActiveRecord] = useState({} as ConfigFileMetadata);
    const { data, error, isLoading, mutate } = useSWR<ConfigFileMetadata[]>(keyIfTokenReady('/api/configurationfiles'), fetcher)
    const { data: configOptions, mutate: mutateConfigOptions } = useSWR<ConfigOption[]>(() => {
        if (!(activeRecord && activeRecord.id)) {
            return null;
        }
        // Null if waiting for token.
        const url = keyIfTokenReady(`/api/configurationfiles/${activeRecord.id}/options`)();
        return url;
    }, fetcher);
    const [addDialogHidden, { setTrue: hideAddDialog, setFalse: showAddDialog }] = useBoolean(true);
    const [deleteDialogHidden, { setTrue: hideDeleteDialog, setFalse: showDeleteDialog }] = useBoolean(true);
    // Whether the delete button is enabled; it should be enabled
    // iff at least one item is selected.
    const [deleteButtonDisabled, setDeleteButtonDisabled] = useState(true);
    const [creatingNewRecord, { setTrue: setCreatingNewRecord, setFalse: clearCreatingNewRecord }] = useBoolean(false);
    const addNameRef = useRef<ITextField>(null);
    const [selection] = useState(new Selection({
        getKey: (cf) => (cf as ConfigFileMetadata).id || '',
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

    return (<div className={"ConfigFiles"}>
        <ConfigFileEditor
            creatingNewRecord={creatingNewRecord}
            record={activeRecord || {} as ConfigFileMetadata}
            configOptions={configOptions}
            isOpen={editorIsOpen}
            onDismiss={() => {
                closeEditor();
                clearCreatingNewRecord();
            }}
            onSubmit={async (meta: ConfigFileMetadata,
                optionMutations: ConfigOptionUpdate[],
                options: ConfigOption[]) => {
                if (!activeRecord) {
                    console.log("Error: no active record exists")
                } else if (creatingNewRecord) {
                    await mutate(
                        async () => {
                            const result =
                                // At this point we should already have an active token.
                                await fetcher<FunctionResult>('/api/configurationfiles',
                                    {
                                        method: 'POST',
                                        data: meta
                                    });
                            if (result.error) {
                                throw new Error(result.error);
                            }
                            if (!result.guid) {
                                throw new Error("No GUID returned from server");
                            }
                            return data?.concat({
                                ...meta,
                                id: result.guid
                            })
                        }
                    );
                    if (options.length > 0) {
                        await mutateConfigOptions(
                            async () => {
                                const result =
                                    await fetcher<FunctionResult>(
                                        `/api/configurationfiles/${activeRecord.id}/options`,
                                        {
                                            method: 'PUT',
                                            data: options
                                        })
                                if (result.error) {
                                    throw new Error(result.error);
                                }
                                return options;
                            }
                        );
                    }
                } else {
                    await mutate(
                        async () => {
                            await
                                fetcher(`/api/configurationfiles/${meta.id}`,
                                    {
                                        method: 'PUT',
                                        data: meta
                                    });
                            return data?.map((r) => r.id === meta.id ? meta : r);
                        }
                    );
                    if (optionMutations.length > 0) {
                        await mutateConfigOptions(
                            async () => {
                                const result =
                                    await fetcher<FunctionResult>(
                                        `/api/configurationfiles/${activeRecord.id}/options`,
                                        {
                                            method: 'PATCH',
                                            data: optionMutations
                                        })
                                if (result.error) {
                                    throw new Error(result.error);
                                }
                                return options;
                            }
                        );
                    }
                }
                closeEditor();
                clearCreatingNewRecord();
            }} />
        <ConfigCommandBar
            {...addDeleteParams}
            singularItemName='configuration file'
            pluralItemName='configuration files'
            addConfirmButtonText='Configure'
            onAddConfirmClick={async () => {
                setActiveRecord({
                    name: addNameRef.current?.value || '',
                    deleted: false,
                    fileType: 'poudriereconf'
                });
                setCreatingNewRecord();
                hideAddDialog();
                openEditor();
            }}
            onDeleteConfirmClick={async () => {
                await mutate(async () => {
                    const cfs = selection.getSelection() as ConfigFileMetadata[];
                    hideDeleteDialog();
                    for (const cf of cfs) {
                        const result = await fetcher(
                            `/api/configurationfiles/${cf.id}`,
                            { method: 'DELETE' }).catch((error: string) => {
                                throw new Error(error);
                            });
                    }
                    return data?.filter((r) => !cfs.includes(r));
                })
            }}
        />
        <ItemList
            enableShimmer={isLoading}
            ariaLabelForGrid={"List of configuration files"}
            getRowAriaLabel={(r: ConfigFileMetadata) => r.name}
            error={error?.toString()}
            items={(data || []).filter(itemsFilter)}
            columns={columns}
            selection={selection}
            getKey={data ? (f: ConfigFileMetadata) => {
                const key = f.id || 'undefined';
                return key;
            } : undefined}
            onItemInvoked={(item: ConfigFileMetadata) => {
                setActiveRecord(item);
                openEditor();
            }}
        />
    </div>)
}
