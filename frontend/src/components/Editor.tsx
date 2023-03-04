import { DefaultButton, IPanelProps, Panel, PanelType, PrimaryButton, Stack } from "@fluentui/react";
import React from "react";

export interface EditorProps extends IPanelProps {
    /// Callback to be called when the user clicks the Save button.
    onSubmit: () => void;
    /// Callback to be called when the user clicks the Cancel button or the panel is
    /// otherwise dismissed.
    onDismiss: () => void;
}

const buttonStyles = { root: { marginRight: 8 } };

export function Editor(props: EditorProps): JSX.Element {
    const { onDismiss,
        onSubmit,
        type = PanelType.medium,
        ...rest } = props;

    const onRenderFooterContent = () => {
        return (
            <div>
                <PrimaryButton onClick={onSubmit} styles={buttonStyles}>
                    Save
                </PrimaryButton>
                <DefaultButton onClick={onDismiss}>Cancel</DefaultButton>
            </div>
        )
    };

    return (
        <Panel
            closeButtonAriaLabel={"Close"}
            onRenderFooterContent={onRenderFooterContent}
            isFooterAtBottom={true}
            onDismiss={onDismiss}
            type={type}
            {...rest}>
            <Stack verticalAlign="start">
                {props.children}
            </Stack>
        </Panel>)
}