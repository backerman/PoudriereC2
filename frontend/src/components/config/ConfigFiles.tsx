import { useCallback, useState } from 'react';
import { IColumn } from '@fluentui/react';
import { ConfigFileMetadata, ConfigFileRepository } from 'src/models/configs';
import { ConfigFileEditor } from './ConfigFileEditor';
import { useBoolean } from '@fluentui/react-hooks';
import { ItemList } from 'src/components/ItemList';
import useSWR from 'swr';

const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL || '';

const fetcher = (url: RequestInfo | URL, ...args: any[]) =>
    fetch(`${baseUrl}${url}`, ...args)
        .then(async (res) => {
            return res;
        })
        .then((res) => {
            if (res.ok) {
                return res.json();
            } else {
                throw new Error(res.statusText);
            }
        });

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
    },
    {
        key: 'portsTree',
        name: 'Ports tree',
        fieldName: 'portsTree',
        minWidth: 75,
        maxWidth: 250,
        isResizable: true,
        targetWidthProportion: 0.2,
        isCollapsible: true
    },
    {
        key: 'jail',
        name: 'Jail',
        fieldName: 'jail',
        minWidth: 100,
        maxWidth: 250,
        isResizable: true,
        targetWidthProportion: 1,
        isCollapsible: true
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
    const [activeRecord, setActiveRecord] = useState('');
    const { data, error, isLoading, mutate } = useSWR<ConfigFileMetadata[]>('/api/configurationfiles/metadata', fetcher);
    return (<div className={"ConfigFiles"}>
        <ConfigFileEditor
            record={data?.find((r) => r.id === activeRecord) || {} as ConfigFileMetadata}
            isOpen={editorIsOpen}
            recordId={activeRecord}
            onDismiss={closeEditor}
            onSubmit={async (meta: ConfigFileMetadata) => {
                await mutate(
                    async () => {
                        await
                            fetcher('/api/configurationfiles/metadata',
                                {
                                    method: 'PUT',
                                    body: JSON.stringify(meta)
                                });
                        return data?.map((r) => r.id === meta.id ? meta : r);
                    });
                closeEditor();
            }} />
        <ItemList
            enableShimmer={isLoading}
            ariaLabelForGrid={"List of configuration files"}
            getRowAriaLabel={(r: ConfigFileMetadata) => r.name}
            error={error?.toString()}
            items={data || []}
            columns={columns}
            getKey={data ? (f: ConfigFileMetadata) => f.id : undefined}
            onItemInvoked={(item: ConfigFileMetadata) => {
                setActiveRecord(item.id);
                openEditor();
            }}
        />
    </div>)
}