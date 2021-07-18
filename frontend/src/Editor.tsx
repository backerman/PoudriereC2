import { DefaultButton, Panel, PrimaryButton, Stack } from "@fluentui/react";
import React from "react";

export interface EditorProps {
    isOpen: boolean;
    isBlocking: boolean;
    headerText: string;
    onSubmit: () => void;
    onDismiss: () => void;
}

const buttonStyles = { root: { marginRight: 8 } };

class Editor<P extends EditorProps, S> extends React.Component<P, S> {
    onRenderFooterContent = () => {
        return (
            <div>
                <PrimaryButton onClick={this.props.onSubmit} styles={buttonStyles}>
                    Save
                </PrimaryButton>
                <DefaultButton onClick={this.props.onDismiss}>Cancel</DefaultButton>
            </div>
        )
    };

    render() {
        return (
            <Panel
                isOpen={this.props.isOpen}
                isBlocking={this.props.isBlocking}
                headerText={this.props.headerText}
                closeButtonAriaLabel={"Close"}
                onRenderFooterContent={this.onRenderFooterContent}
                isFooterAtBottom={true}
                onDismiss={this.props.onDismiss}>
                <Stack verticalAlign="start">
                    {this.props.children}
                </Stack>
            </Panel>)
    };
}

export default Editor;