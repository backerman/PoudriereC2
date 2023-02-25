export const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL || '';

export type FunctionResult = {
    result: string;
    error?: string;
}

export async function fetcher<T = any> (url: RequestInfo | URL, ...args: any[]) {
    const res = await fetch(`${baseUrl}${url}`, ...args);
    if (res.ok) {
        return res.json() as T;
    } else {
        throw new Error(res.statusText);
    }
}