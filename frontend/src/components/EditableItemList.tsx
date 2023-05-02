import { IColumn, IDetailsColumnFieldProps, Selection } from "@fluentui/react";
import { ItemList, ItemListProperties } from "./ItemList";
import { ReactNode, useState } from "react";

export interface EditableItemListProperties extends ItemListProperties {
    columns: IEditableColumn[];
}

export interface IEditableColumn extends IColumn {
    onRenderFieldEditable?: (props?: IDetailsColumnFieldProps) => JSX.Element | null;
}

export function EditableItemList(props: EditableItemListProperties): JSX.Element {

    const selectedRow = useState<Selection>();
    function renderItemColumn(item?: any, index?: number | undefined, column?: IColumn | undefined) : ReactNode {
        return <></>;
    }
    
    return <ItemList
        onRenderItemColumn={renderItemColumn}
        {...props}
    />;
}
