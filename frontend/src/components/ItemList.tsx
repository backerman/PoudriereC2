import { ConstrainMode, DetailsList, DetailsListLayoutMode, IDetailsListProps, IDetailsRowProps, MessageBar, MessageBarType } from "@fluentui/react";
import React, { useState } from "react";
import '/src/styles/ItemList.css';

export interface ItemListProperties extends IDetailsListProps {
    error: any
    children?: React.ReactNode;
}

function renderRowDefault(props?: IDetailsRowProps, defaultRender?: (props?: IDetailsRowProps)
    => JSX.Element | null): JSX.Element | null {
    if (defaultRender === undefined) {
        return null;
    }
    if (props === undefined) {
        return defaultRender(props);
    }
    let row = props.item;
    if (row.deleted) {
        return (
            <div className="rowDeleted">
                {defaultRender(props)}
            </div>);
    } else {
        return defaultRender(props);
    }
}

export function ItemList(props: ItemListProperties): JSX.Element {
    const [errorBarClosed, setErrorBarClosed] = useState(false);

    function closeErrorBar() {
        setErrorBarClosed(true);
    }

    const errorBar =
        <MessageBar
            messageBarType={MessageBarType.error}
            isMultiline={false}
            onDismiss={closeErrorBar}
            dismissButtonAriaLabel="Close">
            Error retrieving data.
        </MessageBar>;

    const {
        items,
        compact,
        error,
        layoutMode = DetailsListLayoutMode.justified,
        onRenderRow = renderRowDefault,
        ...others } = props;
    return (
        <div className={"ItemList"}>
            {error && !errorBarClosed && errorBar}
            <DetailsList
                {...others}
                items={items}
                layoutMode={layoutMode}
                compact={compact ?? false}
                onRenderRow={onRenderRow}
                constrainMode={ConstrainMode.unconstrained}
            />
        </div>
    );
}