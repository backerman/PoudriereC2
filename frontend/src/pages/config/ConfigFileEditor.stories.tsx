import { ConfigFileRepository } from 'src/models/configs';
import { getSampleDataSource, sampleData } from 'src/models/configs.sample';
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
                dataSource={dataSource}
                onDismiss={() => updateArgs({isOpen: false})}
                recordId={args.recordId} />
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

const dataSource: ConfigFileRepository = getSampleDataSource();

export const Editor : StoryObj<typeof ConfigFileEditor> = {
    args: {
        dataSource: dataSource,
        recordId: sampleData[1].id
    },
}