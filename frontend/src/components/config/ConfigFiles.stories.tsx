import { initializeIcons } from '@fluentui/react/lib/Icons';
import { StoryObj } from '@storybook/react';
import { sampleData, getSampleDataSource } from 'src/models/configs.sample';
import { ConfigFiles } from './ConfigFiles';

initializeIcons();

export default {
    component: ConfigFiles,
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
} as StoryObj<typeof ConfigFiles>;

const dataSource = getSampleDataSource();

export const ConfigFilesPopulated: StoryObj<typeof ConfigFiles> = {
    args: {
        dataSource: dataSource,
        showDeleted: true,
    }
}