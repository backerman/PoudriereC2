import { DetailsList, DetailsListLayoutMode, IDetailsListProps, IDetailsRowProps, MessageBar, MessageBarType } from "@fluentui/react";
import React from "react";

export interface ItemListProperties extends IDetailsListProps {
    error: any
}

interface ItemListState {
    items: any[]
    errorBarClosed: boolean
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

class ItemList<P extends ItemListProperties> extends React.Component<P, ItemListState> {
    errorBar: JSX.Element;

    constructor(props: P) {
        super(props);

        this.errorBar =
            <MessageBar
                messageBarType={MessageBarType.error}
                isMultiline={false}
                onDismiss={this.closeErrorBar}
                dismissButtonAriaLabel="Close">
                Error retrieving data.
            </MessageBar>;
        this.state = {
            items: props.items,
            errorBarClosed: false
        };
    }

    closeErrorBar() {
        this.setState({ errorBarClosed: true });
    }

    render() {
        const {
            compact,
            error,
            layoutMode = DetailsListLayoutMode.justified,
            onRenderRow = renderRowDefault,
            ...others } = this.props;
        return (
            <div className={"ConfigItems"}>
                {error && !this.state.errorBarClosed && this.errorBar}
                <DetailsList
                    {...others}
                    layoutMode={layoutMode}
                    compact={compact ?? false}
                    onRenderRow={onRenderRow}
                />
            </div>
        );
    }
}

export default ItemList;