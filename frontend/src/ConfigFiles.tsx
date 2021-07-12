import React, { useCallback, useEffect, useState } from 'react';
import { DetailsList, DetailsListLayoutMode, IColumn, IDetailsRowProps } from '@fluentui/react';
import { ConfigFileMetadata, ConfigFileRepository } from './model/configs';
import { sortBy } from './utils';
import './ConfigFiles.css';
import { ConfigFileEditor } from './ConfigFileEditor';
import { useBoolean } from '@fluentui/react-hooks';

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
    } /*,
    {
        key: 'deleted',
        name: 'Deleted',
        fieldName: 'jail',
        minWidth: 20,
        maxWidth: 50,
        isResizable: true,
        targetWidthProportion: 0.1

    } */
]

function renderItemColumn(item: ConfigFileMetadata, index?: number, column?: IColumn) {
    if (column === undefined) {
        return <span></span>;
    }

    const fieldContent = item[column.fieldName as keyof ConfigFileMetadata] as string;
    switch (column.key) {
        case 'deleted':
            return item.deleted ? <span>⭕</span> : <span>&nbsp;</span>;
        default:
            return <span>{fieldContent}</span>;
    }
}

function renderRow(props?: IDetailsRowProps, defaultRender?: (props?: IDetailsRowProps)
    => JSX.Element | null): JSX.Element | null {
    if (defaultRender === undefined) {
        return null;
    }
    if (props === undefined) {
        return defaultRender(props);
    }
    let row = props.item as ConfigFileMetadata;
    if (row.deleted) {
        return (
            <div className="rowDeleted">
                {defaultRender(props)}
            </div>);
    } else {
        return defaultRender(props);
    }
}

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
        useEffect(() => {
            let isMounted = true;
            async function fetchData() {
                props.dataSource.getConfigFiles()
                    .then(
                        (items: ConfigFileMetadata[]) => {
                            if (isMounted)
                                setItemList(items.filter(itemsFilter).sort(sortBy('name')))
                        }
                    );
            };
            fetchData();
            return () => { isMounted = false; }
        }, [itemsFilter, props.dataSource, itemList]);
        return (<div className={"ConfigFiles"}>
            <ConfigFileEditor
                dataSource={props.dataSource}
                isOpen={editorIsOpen}
                recordId={activeRecord}
                onDismiss={closeEditor}
                onSubmit={(meta) => {
                    props.dataSource.updateConfigFile(meta);
                    closeEditor();
                }}
            />
            <DetailsList
                ariaLabel={"List of configuration files"}
                items={itemList}
                columns={columns}
                getKey={(f: ConfigFileMetadata) => f.id}
                layoutMode={DetailsListLayoutMode.justified}
                compact={false}
                onRenderItemColumn={renderItemColumn}
                onRenderRow={renderRow}
                onItemInvoked={(item: ConfigFileMetadata) => {
                    setActiveRecord(item.id);
                    openEditor();
                }}
            /></div>)
    }