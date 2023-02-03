import { DefaultButton, Panel, PrimaryButton, Stack } from "@fluentui/react";
import React from "react";

export interface EditorProps {
    isOpen: boolean;
    isBlocking: boolean;
    headerText: string;
    onSubmit: () => void;
    onDismiss: () => void;
    children?: React.ReactNode;
}

const buttonStyles = { root: { marginRight: 8 } };

export function Editor(props: EditorProps): JSX.Element {
    const onRenderFooterContent = () => {
        return (
            <div>
                <PrimaryButton onClick={props.onSubmit} styles={buttonStyles}>
                    Save
                </PrimaryButton>
                <DefaultButton onClick={props.onDismiss}>Cancel</DefaultButton>
            </div>
        )
    };

    return (
        <Panel
            isOpen={props.isOpen}
            isBlocking={props.isBlocking}
            headerText={props.headerText}
            closeButtonAriaLabel={"Close"}
            onRenderFooterContent={onRenderFooterContent}
            isFooterAtBottom={true}
            onDismiss={props.onDismiss}>
            <Stack verticalAlign="start">
                {props.children}
            </Stack>
        </Panel>)

}

export default Editor;