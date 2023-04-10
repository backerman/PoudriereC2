import { fetcher } from "@/utils/fetcher";
import { ComboBox, IComboBoxOption, IComboBoxProps, SelectableOptionMenuItemType } from "@fluentui/react"
import useSWR from 'swr';

export interface ComboBoxWithFetcherItem {
    id?: string;
    name: string;
}

export interface ComboBoxWithFetcherProps extends Omit<IComboBoxProps, "options"> {
    dataUrl: string
}

export function ComboBoxWithFetcher<T extends ComboBoxWithFetcherItem>(props: ComboBoxWithFetcherProps) : JSX.Element {
    const {dataUrl} = props;
    const { data, error, isLoading, mutate } = useSWR<T[]>(dataUrl, fetcher);
    const comboOptions : IComboBoxOption[] =
        data?.map((item) => {
            return {
                key: item.id || 'undefined',
                text: item.name,
                hidden: item.id === undefined,
                itemType: SelectableOptionMenuItemType.Normal
            }
        }) || [];
    return (
    <ComboBox
        {...props}
        options={comboOptions}
        autoComplete="on"
        useComboBoxAsMenuWidth={true}
        allowFreeInput={true}
    />)
}
