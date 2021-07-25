import { IColumn } from "@fluentui/react";
import { useEffect, useState } from "react";
import { useBoolean } from '@fluentui/react-hooks';
import { PortSet, PortSetRepository } from "../model/portsets";
import ItemList from "../ItemList";
import { sortBy } from "../utils";
import { PortSetEditor } from "./PortSetEditor";


export interface PortSetProps {
    dataSource: PortSetRepository
}

const columns: IColumn[] = [
    {
        key: 'name',
        name: 'Name',
        fieldName: 'name',
        isSorted: true,
        isSortedDescending: false,
        minWidth: 50,
        maxWidth: 300,
        isResizable: true,
        targetWidthProportion: 0.75,
        isCollapsible: true
    },
    {
        key: 'numPorts',
        name: '# ports',
        fieldName: 'portSet',
        minWidth: 50,
        maxWidth: 100,
        isResizable: true,
        targetWidthProportion: 0.25,
        isCollapsible: true,
        onRender: (ps: PortSet) => ps.origins.length
    }
]

export const PortSets = (props: PortSetProps) => {
    const [editorIsOpen, { setTrue: openEditor, setFalse: closeEditor }] = useBoolean(false);
    const [activeRecord, setActiveRecord] = useState('');
    const [itemList, setItemList] = useState([] as PortSet[]);
    let [error, setError] = useState<any>(null);
    let [itemsChanged, renderMe] = useState(0);
    useEffect(() => {
        let isMounted = true;
        async function fetchData() {
            props.dataSource.getPortSets()
                .then(
                    (items: PortSet[]) => {
                        if (isMounted)
                            setItemList(items.sort(sortBy('name')));
                    }
                ).catch((err) => {
                    setError(err);
                });
        };
        fetchData();
        return () => { isMounted = false; }
    }, [props.dataSource, itemsChanged]);
    return (
        <div className={"PortSets"}>
            <PortSetEditor
                dataSource={props.dataSource}
                isOpen={editorIsOpen}
                onDismiss={closeEditor}
                recordId={activeRecord}
                onSubmit={async (ps) => {
                    await props.dataSource.updatePortSet(activeRecord, ps);
                    renderMe((x) => x + 1);
                    closeEditor();
                }}/>
            <ItemList
                ariaLabel={"List of port sets"}
                getRowAriaLabel={(r: PortSet) => r.name}
                columns={columns}
                items={itemList}
                error={error}
                getKey={(r: PortSet) => r.id}
                onItemInvoked={(item: PortSet) => {
                    setActiveRecord(item.id);
                    openEditor();
                }}
            />
        </div>
    )
}