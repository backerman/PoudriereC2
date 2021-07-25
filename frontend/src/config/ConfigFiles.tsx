import { useCallback, useEffect, useState } from 'react';
import { IColumn } from '@fluentui/react';
import { ConfigFileMetadata, ConfigFileRepository } from '../model/configs';
import { sortBy } from '../utils';
import './ConfigFiles.css';
import { ConfigFileEditor } from './ConfigFileEditor';
import { useBoolean } from '@fluentui/react-hooks';
import ItemList from '../ItemList';

type ConfigFilesProps = {
    dataSource: ConfigFileRepository;
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
        targetWidthProportion: 0.05,
    },
    {
        key: 'name',
        name: 'Name',
        fieldName: 'name',
        isSorted: true,
        isSortedDescending: false,
        minWidth: 50,
        maxWidth: 250,
        isResizable: true,
        targetWidthProportion: 0.05,
        isCollapsible: true
    },
    {
        key: 'portSet',
        name: 'Port set',
        fieldName: 'portSet',
        minWidth: 50,
        maxWidth: 100,
        isResizable: true,
        targetWidthProportion: 0.2,
        isCollapsible: true
    },
    {
        key: 'portsTree',
        name: 'Ports tree',
        fieldName: 'portsTree',
        minWidth: 50,
        maxWidth: 100,
        isResizable: true,
        targetWidthProportion: 0.2,
        isCollapsible: true
    },
    {
        key: 'jail',
        name: 'Jail',
        fieldName: 'jail',
        minWidth: 50,
        maxWidth: 100,
        isResizable: true,
        targetWidthProportion: 0.2,
        isCollapsible: true
    }
]

export const ConfigFiles =
    (props: ConfigFilesProps) => {
        const itemsFilter = useCallback((item: ConfigFileMetadata) => {
            if (props.showDeleted) {
                return true;
            }
            return !item.deleted;
        }, [props.showDeleted]);
        const [editorIsOpen, { setTrue: openEditor, setFalse: closeEditor }] = useBoolean(false);
        const [activeRecord, setActiveRecord] = useState('');
        const [itemList, setItemList] = useState([] as ConfigFileMetadata[]);
        let [error, setError] = useState<any>(null);
        let [itemsChanged, renderMe] = useState(0);
        useEffect(() => {
            let isMounted = true;
            async function fetchData() {
                props.dataSource.getConfigFiles()
                    .then(
                        (items: ConfigFileMetadata[]) => {
                            if (isMounted)
                                setItemList(items.filter(itemsFilter).sort(sortBy('name')));
                        }
                    ).catch((err) => {
                        setError(err);
                    });
            };
            fetchData();
            return () => { isMounted = false; }
        }, [itemsFilter, props.dataSource, itemsChanged]);

        return (<div className={"ConfigFiles"}>
            <ConfigFileEditor
                dataSource={props.dataSource}
                isOpen={editorIsOpen}
                recordId={activeRecord}
                onDismiss={closeEditor}
                onSubmit={async (meta) => {
                    await props.dataSource.updateConfigFile(meta);
                    // force rerender.
                    renderMe((x) => x + 1);
                    closeEditor();
                }} />
            <ItemList
                ariaLabel={"List of configuration files"}
                getRowAriaLabel={(r: ConfigFileMetadata) => r.name}
                error={error}
                items={itemList}
                columns={columns}
                getKey={(f: ConfigFileMetadata) => f.id}
                onItemInvoked={(item: ConfigFileMetadata) => {
                    setActiveRecord(item.id);
                    openEditor();
                }}
            />
        </div>)
    }