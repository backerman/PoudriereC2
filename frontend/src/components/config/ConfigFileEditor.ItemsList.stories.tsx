import { initializeIcons } from "@fluentui/react";
import { ConfigFileEditorItemsList } from "./ConfigFileEditor.ItemsList";
import { Meta, StoryObj } from "@storybook/react";

initializeIcons();

const meta: Meta<typeof ConfigFileEditorItemsList> = {
    component: ConfigFileEditorItemsList,
    title: "components/config/ConfigFileEditor.ItemsList",
    argTypes: {
        deleteClicked: {
            action: "deleteClicked"
        }
    }
};
export default meta;

type Story = StoryObj<typeof ConfigFileEditorItemsList>;

export const SomeItems: Story = {
    args: {
        items: [
            { name: "foo", value: "bar" },
            { name: "baz", value: "quux" }
        ]
    }
}
