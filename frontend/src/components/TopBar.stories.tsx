import { StoryObj } from "@storybook/react";
import { TopBar } from "./TopBar";
import { initializeIcons } from '@fluentui/react';

initializeIcons();

export default {
    component: TopBar
} as StoryObj<typeof TopBar>;

export const Bar : StoryObj<typeof TopBar> = {
    name: 'TopBar',
    args: {
        user: {
            name: 'Dr. Robert Ford',
            upn: 'robert.ford@example.com',
            initials: 'RF'
        }
    }
};