import { DefaultButton, IPanelProps, Panel, PanelType, PrimaryButton, Stack } from "@fluentui/react";
import React from "react";

export interface EditorProps extends IPanelProps {
    /// Callback to be called when the user clicks the Save button.
    onSubmit: () => void;
    /// Callback to be called when the panel is dismissed without clicking the
    // Cancel button, or when dismissed and onCancelButtonClicked is not provided.
    onDismiss: () => void;
    /// Callback to be called when the user clicks the Cancel button. If not present,
    /// onDismiss will be used.
    onCancelButtonClicked?: () => void;
    /// Whether the Submit button should be disabled.
    submitDisabled?: boolean;
}

const buttonStyles = { root: { marginRight: 8 } };

export function Editor(props: EditorProps): JSX.Element {
    const { onDismiss,
        onSubmit,
        onCancelButtonClicked,
        submitDisabled,
        type = PanelType.medium,
        ...rest } = props;

    const onRenderFooterContent = () => {
        return (
            <div>
                <PrimaryButton onClick={onSubmit} styles={buttonStyles} disabled={submitDisabled}>
                    Save
                </PrimaryButton>
                <DefaultButton onClick={onCancelButtonClicked || onDismiss}>Cancel</DefaultButton>
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
