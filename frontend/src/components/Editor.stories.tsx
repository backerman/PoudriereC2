import { useArgs } from "@storybook/preview-api";
import { StoryObj } from "@storybook/react";
import Editor, { EditorProps } from "./Editor";

import { initializeIcons } from '@uifabric/icons';
import { PrimaryButton } from "@fluentui/react";
initializeIcons();

export default {
    component: Editor,
    render: (args: EditorProps) => {
        // eslint-disable-next-line react-hooks/rules-of-hooks
        const [_args, updateArgs, _resetArgs] = useArgs();
        return (
            <div>
                <PrimaryButton
                    disabled={args.isOpen}
                    onClick={() => updateArgs({isOpen: true})}>
                    Open editor
                </PrimaryButton>
                <Editor
                    {...args}
                    onDismiss={() => updateArgs({ isOpen: false })}
                />
            </div>)
    },
    args: {
        headerText: "Editor dialog",
        isBlocking: false,
        isOpen: false
    }
} as StoryObj<typeof Editor>;

export const JustAnEditor: StoryObj<typeof Editor> = {
    name: 'Editor',
    args: {
        children:
            <p>O hai!</p>
    }
}