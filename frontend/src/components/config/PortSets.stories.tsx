import { PortSets } from './PortSets';
import { PortSet } from 'src/models/portsets';
import { sampleData } from 'src/models/portsets.sample';
import { initializeIcons } from '@fluentui/react/lib/Icons';
import { StoryObj } from '@storybook/react';
import { rest } from 'msw';
import { SWRConfig } from 'swr';

initializeIcons();

export default {
    component: PortSets,
    render: () => (
        <SWRConfig value={{ provider: () => new Map() }}>
            <PortSets />
        </SWRConfig>),
} as StoryObj<typeof PortSets>;

export const Empty: StoryObj<typeof PortSets> = {
    name: 'Empty',
    parameters: {
        msw: {
            handlers: [
                rest.get('/api/portsets', (req, res, ctx) => {
                    return res(
                        ctx.delay(),
                        ctx.json([])
                    )
                })
            ]
        }
    }
}

export const Populated: StoryObj<typeof PortSets> = {
    name: 'Populated',
    parameters: {
        msw: {
            handlers: [
                rest.get('/api/portsets', (req, res, ctx) => {
                    return res(
                        ctx.delay(),
                        ctx.json(sampleData)
                    )
                })
            ]
        }
    }
}

export const Erroring: StoryObj<typeof PortSets> = {
    name: 'Erroring',
    parameters: {
        msw: {
            handlers: [
                rest.get('/api/portsets', (req, res, ctx) => {
                    return res(
                        ctx.delay(),
                        ctx.status(500),
                    )
                })
            ]
        }
    }
}