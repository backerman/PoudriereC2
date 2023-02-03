import { IColumn } from "@fluentui/react";
import { ComponentMeta, ComponentStory } from "@storybook/react";
import { initializeIcons } from '@uifabric/icons';
import 'temporal-polyfill/global';
import { ItemList } from "./ItemList";

initializeIcons();

interface ISampleItem {
    id: number;
    name: string;
    date: Temporal.PlainDate;
    flag: boolean;
}

function columnWithDefaults({key, name, fieldName, minWidth, ...overrides}: IColumn): IColumn {
    return {
        key,
        name,
        fieldName,
        minWidth,
        isResizable: true,
        ...overrides
    }
}

const sampleColumns : IColumn[] = [
    {
        key: "id",
        name: "ID",
        fieldName: "id",
        minWidth: 25,
        maxWidth: 50,
    },
    {
        key: "name",
        name: "Name",
        fieldName: "name",
        minWidth: 200
    },
    {
        key: "date",
        name: "Date",
        fieldName: "date",
        minWidth: 150,
        maxWidth: 150,
        onRender: (item: ISampleItem) => {
            return item.date.toString();
        },
    },
    {
        key: "flag",
        name: "Flag",
        fieldName: "flag",
        minWidth: 150,
        maxWidth: 150
    }
].map(columnWithDefaults)

const sampleItems: ISampleItem[] = [
    {
        id: 1,
        name: "Item 1",
        date: Temporal.PlainDate.from("2022-01-01"),
        flag: true
    },
    {
        id: 2,
        name: "Item 1",
        date: Temporal.PlainDate.from("1979-02-01"),
        flag: true
    },
    {
        id: 3,
        name: "Item 1",
        date: Temporal.PlainDate.from("1800-12-31"),
        flag: true
    },
    {
        id: 4,
        name: "Item 1",
        date: Temporal.PlainDate.from("2269-09-09"),
        flag: true
    },
    {
        id: 9001,
        name: "孫悟空",
        date: Temporal.PlainDate.from("1934-04-18"),
        flag: true
    }
]

export default {
    title: "ItemList",
    component: ItemList,
} as ComponentMeta<typeof ItemList>;

const Template: ComponentStory<typeof ItemList> =
    (args) =>
        <ItemList
            {...args}
            />;

export const Empty = Template.bind({});
Empty.args = {
    items: [],
    error: undefined
}

export const SomeItems = Template.bind({});
SomeItems.args = {
    items: sampleItems,
    columns: sampleColumns,
    error: undefined
}
export const HasError = Template.bind({});
HasError.args = {
    items: sampleItems,
    columns: sampleColumns,
    error: "As I was walking down the stair, I met a man who wasn't there."
}