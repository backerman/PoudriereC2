import { IColumn } from "@fluentui/react";
import { useState } from "react";
import { useBoolean } from '@fluentui/react-hooks';
import { PortSet } from "src/models/portsets";
import { ItemList } from "../ItemList";
import { sortBy } from "src/utils/utils";
import { PortSetEditor } from "./PortSetEditor";
import { fetcher, FunctionResult } from "@/utils/fetcher";
import useSWR from 'swr';

const columns: IColumn[] = [
    {
        key: 'name',
        name: 'Name',
        fieldName: 'name',
        isSorted: true,
        isSortedDescending: false,
        minWidth: 75,
        isResizable: true,
        targetWidthProportion: 0.75,
        isCollapsible: false,
        isRowHeader: true
    },
    {
        key: 'numPorts',
        name: '# ports',
        fieldName: 'portSet',
        minWidth: 50,
        isResizable: true,
        targetWidthProportion: 0.25,
        isCollapsible: false,
        onRender: (ps: PortSet) => ps.origins.length
    }
]

function computeMutations(oldPs: PortSet, newPs: PortSet) {
    const toAdd = newPs.origins.filter(o => !oldPs.origins.includes(o));
    const toDelete = oldPs.origins.filter(o => !newPs.origins.includes(o));
    return { toAdd, toDelete };
}

export function PortSets(): JSX.Element {
    const [editorIsOpen, { setTrue: openEditor, setFalse: closeEditor }] = useBoolean(false);
    const [activeRecord, setActiveRecord] = useState<PortSet|undefined>(undefined);
    const { data, error, isLoading, mutate } = useSWR<PortSet[]>('/api/portsets', fetcher);
    return (
        <div className={"PortSets"}>
            <PortSetEditor
                isOpen={editorIsOpen}
                onDismiss={closeEditor}
                record={activeRecord}
                onSubmit={async (ps: PortSet) => {
                    if (!activeRecord) {
                        console.log("Error: no active record exists")
                    } else if (activeRecord.id !== ps.id) {
                        console.log("Error: active record id does not match submitted record id")
                    } else {
                        const { toAdd, toDelete } = computeMutations(activeRecord, ps);
                        const actions = [
                            { action: 'add', ports: toAdd },
                            { action: 'delete', ports: toDelete}
                        ].filter(a => a.ports.length > 0);
                        var result: FunctionResult;
                        await mutate(
                            async () => {
                                result = await fetcher<FunctionResult>(`/api/portsets/${ps.id}`, {
                                    method: 'PATCH',
                                    body: JSON.stringify(actions)
                                });
                                if (result.error) {
                                    throw new Error(result.error);
                                }
                                // Return the updated list of port sets
                                return data?.map((r) => r.id === ps.id ? ps : r); 
                            });
                    }
                    closeEditor();    
                }} />
            <ItemList
                ariaLabelForGrid={"List of port sets"}
                getRowAriaLabel={(r: PortSet) => r.name}
                columns={columns}
                items={data || []}
                enableShimmer={isLoading}
                error={error?.toString()}
                getKey={(r: PortSet) => r?.id}
                onItemInvoked={(item: PortSet) => {
                    setActiveRecord(item);
                    openEditor();
                }}
            />
        </div>
    )
}