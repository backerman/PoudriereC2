import { sampleData } from 'src/models/configs.sample';
import { initializeIcons } from '@fluentui/react/lib/Icons';
import { ConfigFileEditor } from './ConfigFileEditor';
import { PrimaryButton } from '@fluentui/react';
import { useArgs } from "@storybook/preview-api";
import { StoryObj } from "@storybook/react";

initializeIcons();

export default {
    component: ConfigFileEditor,
    render: (args) => {
        const [_args, updateArgs, _resetArgs] = useArgs();
        return (
            <div>
                <PrimaryButton
                    disabled={args.isOpen}
                    onClick={() => updateArgs({ isOpen: true })}>
                    Open editor
                </PrimaryButton>
                <ConfigFileEditor
                isOpen={args.isOpen}
                record={sampleData.find((item) => item.id == args.record.id) || sampleData[0]}
                onDismiss={() => updateArgs({isOpen: false})}  />
            </div>
        )
    },
    argTypes: {
        onSubmit: {
            description: 'Callback for submission of editor form',
            action: 'submitted'
        },
        recordId: {
            options: sampleData.map((item) => item.id),
            control: {
                type: 'select'
            }
        }
    }
} as StoryObj<typeof ConfigFileEditor>;

export const Editor : StoryObj<typeof ConfigFileEditor> = {
    args: {
        record: sampleData[1]
    },
}
