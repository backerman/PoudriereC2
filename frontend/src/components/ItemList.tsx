import {
    ConstrainMode,
    DetailsListLayoutMode,
    IDetailsRowProps,
    IShimmeredDetailsListProps,
    MessageBar,
    MessageBarType,
    SelectionMode,
    ShimmeredDetailsList
} from "@fluentui/react";
import React, { useState } from "react";
import styles from './ItemList.module.css';

/** Properties for {@link ItemList}. */
export interface ItemListProperties extends IShimmeredDetailsListProps {
    /** An error message to be displayed */
    error?: string
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

/** A {@link ShimmeredDetailsList} with consistent styles and an error bar. */
export function ItemList(props: ItemListProperties): JSX.Element {
    const [errorBarClosed, setErrorBarClosed] = useState(false);
    // FIXME: A second error will not be shown if the first is closed.
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
        getKey,
        layoutMode = DetailsListLayoutMode.justified,
        onRenderRow = renderRowDefault,
        enableShimmer,
        selectionMode = SelectionMode.multiple,
        ...others } = props;

    return (
        <div className={"ItemList"}>
            {error && !errorBarClosed && errorBar}
            <ShimmeredDetailsList
                {...others}
                enableShimmer={enableShimmer || false}
                items={items}
                getKey={getKey}
                setKey={"set"}
                layoutMode={layoutMode}
                compact={compact ?? false}
                onRenderRow={onRenderRow}
                constrainMode={ConstrainMode.unconstrained}
                ariaLabelForSelectionColumn="Toggle selection"
            />
        </div>
    );
}
