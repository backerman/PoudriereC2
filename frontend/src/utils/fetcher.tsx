export const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL || '';

export type FunctionResult = {
    result: string;
    error?: string;
    guid?: string;
}

export async function fetcher<T = any> (url: RequestInfo | URL, args?: RequestInit | undefined) {
    const res = await fetch(`${baseUrl}${url}`, {
        headers: {
            'Accept': 'application/json'
        },
        ...args,
    });
    if (res.ok) {
        return res.json() as T;
    } else {
        throw new Error(res.statusText);
    }
}
