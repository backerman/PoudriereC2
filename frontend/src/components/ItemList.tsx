import { ConstrainMode, DetailsList, DetailsListLayoutMode, IDetailsListProps, IDetailsRowProps, MessageBar, MessageBarType, ShimmeredDetailsList } from "@fluentui/react";
import React, { useState } from "react";
import styles from './ItemList.module.css';

export interface ItemListProperties extends IDetailsListProps {
    error?: string
    children?: React.ReactNode;
    enableShimmer?: boolean;
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
            <div className={styles.rowDeleted}>
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
            {props.error}
        </MessageBar>;

    const {
        items,
        compact,
        error,
        layoutMode = DetailsListLayoutMode.justified,
        onRenderRow = renderRowDefault,
        enableShimmer,
        ...others } = props;
    return (
        <div className={"ItemList"}>
            {error && !errorBarClosed && errorBar}
            <ShimmeredDetailsList
                {...others}
                enableShimmer={enableShimmer || false}
                items={items}
                layoutMode={layoutMode}
                compact={compact ?? false}
                onRenderRow={onRenderRow}
                constrainMode={ConstrainMode.unconstrained}
            />
        </div>
    );
}