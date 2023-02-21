import { initializeIcons } from '@fluentui/react/lib/Icons';
import { StoryObj } from '@storybook/react';
import { sampleData } from 'src/models/configs.sample';
import { ConfigFiles } from './ConfigFiles';
import { rest } from 'msw';

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

export const ConfigFilesPopulated: StoryObj<typeof ConfigFiles> = {
    args: {
        showDeleted: true,
    },
    parameters: {
        msw: {
            handlers: [
                rest.get('/api/configurationfiles/metadata', (req, res, ctx) => {
                    return res(
                        ctx.json(sampleData)
                    )
                })
            ]
        }
    }
}