import { StoryObj } from "@storybook/react";
import { TopBar } from "./TopBar";
import { initializeIcons } from '@fluentui/react';
import { MockMsalInstance } from "@/utils/FakeAuthInfo";
import { MsalProvider } from "@azure/msal-react";

initializeIcons();

export default {
    component: TopBar,
    render: (props: any) =>
        <MsalProvider instance={MockMsalInstance}>
            <TopBar {...props} />
        </MsalProvider>
} as StoryObj<typeof TopBar>;

export const Bar: StoryObj<typeof TopBar> = {
    name: 'TopBar'
};