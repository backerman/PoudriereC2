import { useCallback, useRef, useState } from 'react';
import { IColumn, ITextField, Selection } from '@fluentui/react';
import { useBoolean } from '@fluentui/react-hooks';
import { ItemList } from 'src/components/ItemList';
import useSWR from 'swr';
import { FunctionResult } from 'src/utils/fetcher';
import { ConfigCommandBar } from './ConfigCommandBar';
import { useAzureFunctionsOAuth } from "@/utils/apiAuth";
import { JobConfig } from '@/models/jobconfigs';
import { JobConfigEditor } from './JobConfigEditor';

type JobConfigsProps = {
    showDeleted?: boolean;
}

const columns: IColumn[] = [
    {
        key: 'name',
        name: 'Name',
        fieldName: 'name',
        isSorted: true,
        isSortedDescending: false,
        minWidth: 100,
        maxWidth: 1000,
        isResizable: true,
        targetWidthProportion: 0.2,
        isCollapsible: false,
        isRowHeader: true,
    },
    {
        key: 'poudriereConf',
        name: 'poudriere.conf',
        fieldName: 'poudriereConf',
        minWidth: 250,
        maxWidth: 500,
        isResizable: true,
        targetWidthProportion: 0.2,
        isCollapsible: true,
        onRender: (item: JobConfig) =>
            item.poudriereConfName
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
        onRender: (item: JobConfig) =>
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
        onRender: (item: JobConfig) =>
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
        onRender: (item: JobConfig) =>
            item.jailName
    }
]

export function JobConfigs(props: JobConfigsProps): JSX.Element {
    const itemsFilter = useCallback((item: JobConfig) => {
        if (props.showDeleted) {
            return true;
        }
        return !item.deleted;
    }, [props.showDeleted]);
    const [editorIsOpen, { setTrue: openEditor, setFalse: closeEditor }] = useBoolean(false);
    const { fetcher, keyIfTokenReady } = useAzureFunctionsOAuth();
    const [activeRecord, setActiveRecord] = useState({} as JobConfig);
    const { data, error, isLoading, mutate } = useSWR<JobConfig[]>(keyIfTokenReady('/api/jobconfigs'), fetcher);
    const [addDialogHidden, { setTrue: hideAddDialog, setFalse: showAddDialog }] = useBoolean(true);
    const [deleteDialogHidden, { setTrue: hideDeleteDialog, setFalse: showDeleteDialog }] = useBoolean(true);
    // Whether the delete button is enabled; it should be enabled
    // iff at least one item is selected.
    const [deleteButtonDisabled, setDeleteButtonDisabled] = useState(true);
    const [creatingNewRecord, { setTrue: setCreatingNewRecord, setFalse: clearCreatingNewRecord }] = useBoolean(false);
    const addNameRef = useRef<ITextField>(null);
    const [selection] = useState(new Selection({
        getKey: (cf) => (cf as JobConfig).id || '',
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
        <JobConfigEditor
            creatingNewRecord={creatingNewRecord}
            record={activeRecord || {} as JobConfig}
            isOpen={editorIsOpen}
            onDismiss={() => {
                closeEditor();
                clearCreatingNewRecord();
            }}
            onSubmit={async (jc: JobConfig) => {
                if (!activeRecord) {
                    console.log("Error: no active record exists")
                } else if (creatingNewRecord) {
                    await mutate(
                        async () => {
                            const result =
                                await fetcher<FunctionResult>('/api/jobconfigs',
                                    {
                                        method: 'POST',
                                        data: jc
                                    });
                            if (result.error) {
                                throw new Error(result.error);
                            }
                            if (!result.guid) {
                                throw new Error("No GUID returned from server");
                            }
                            return data?.concat({
                                ...jc,
                                id: result.guid
                            })
                        }
                    );
                } else {
                    await mutate(
                        async () => {
                            await
                                fetcher(`/api/jobconfigs/${jc.id}`,
                                    {
                                        method: 'PUT',
                                        data: jc
                                    });
                            return data?.map((cf) => cf.id === jc.id ? jc : cf);
                        }
                    );
                }
                closeEditor();
                clearCreatingNewRecord();
            }}
        />
        <ConfigCommandBar
            {...addDeleteParams}
            singularItemName='job configuration'
            pluralItemName='job configurations'
            addConfirmButtonText='Configure'
            onAddConfirmClick={async () => {
                setActiveRecord({
                    name: addNameRef.current?.value || '',
                    deleted: false,
                });
                setCreatingNewRecord();
                hideAddDialog();
                openEditor();
            }}
            onDeleteConfirmClick={async () => {
                await mutate(async () => {
                    const cfs = selection.getSelection() as JobConfig[];
                    hideDeleteDialog();
                    for (const cf of cfs) {
                        const result = await fetcher(
                            `/api/jobconfigs/${cf.id}`,
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
            ariaLabelForGrid={"List of job configs"}
            getRowAriaLabel={(r: JobConfig) => r.name}
            error={error?.toString()}
            items={(data || []).filter(itemsFilter)}
            columns={columns}
            selection={selection}
            getKey={data ? (f: JobConfig) => {
                const key = f.id || 'undefined';
                return key;
            } : undefined}
            onItemInvoked={(item: JobConfig) => {
                setActiveRecord(item);
                openEditor();
            }}
        />
    </div>)
}
