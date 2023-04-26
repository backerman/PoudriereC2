import { fetcher } from "@/utils/fetcher";
import { ComboBox, IComboBoxOption, IComboBoxProps, SelectableOptionMenuItemType } from "@fluentui/react"
import { useCallback } from "react";
import useSWR from 'swr';

export interface ComboBoxWithFetcherItem {
    id?: string;
    name: string;
}

export interface ComboBoxWithFetcherProps extends Omit<IComboBoxProps, "options"> {
    dataUrl: string;
    noResultsMessage?: string;
}

export function ComboBoxWithFetcher<T extends ComboBoxWithFetcherItem | string>(props: ComboBoxWithFetcherProps) : JSX.Element {
    const {dataUrl, noResultsMessage} = props;
    const { data, error, isLoading, mutate } = useSWR<T[]>(dataUrl, fetcher);
    const placeholder = useCallback(() => {
        if (isLoading) {
            return "Loading...";
        } else if (data !== undefined && data.length === 0) {
            return noResultsMessage || "No available selections";
        } else {
            return "Select an option";
        }
    }, [data, isLoading, noResultsMessage]);
    const comboOptions : IComboBoxOption[] =
        data?.map((item) => {
            if (typeof item === 'string') {
                return {
                    key: item,
                    text: item,
                    hidden: false,
                    itemType: SelectableOptionMenuItemType.Normal
                }
            } else {
                return {
                    key: item.id || 'undefined',
                    text: item.name,
                    hidden: item.id === undefined,
                    itemType: SelectableOptionMenuItemType.Normal
                }
            }
        }) || [];
    return (
    <ComboBox
        {...props}
        options={comboOptions}
        placeholder={placeholder()}
        autoComplete="on"
        useComboBoxAsMenuWidth={true}
        allowFreeInput={true}
    />)
}
