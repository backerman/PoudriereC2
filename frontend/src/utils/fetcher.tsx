import { AxiosError, AxiosInstance, AxiosRequestConfig } from 'axios';

export const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL || '';

export type FunctionResult = {
    result: string;
    error?: string;
    guid?: string;
}

export type FetcherArgs = Omit<Partial<AxiosRequestConfig>, 'headers' | 'url'>;

export function makeFetcher(axios: AxiosInstance) {
    async function myFetcher<ResponseData>(url: string, args?: FetcherArgs) {
        const res = await axios.request<ResponseData>({
            url: url,
            baseURL: baseUrl,
            headers: {
                'Accept': 'application/json'
            },
            ...args,
        }).catch((err: AxiosError<FunctionResult>) => {
            if (err.response) {
                throw new Error(err.response.data.error);
            } else {
                throw err.message;
            }
        });
        return res.data;    
    }
    return myFetcher
}
