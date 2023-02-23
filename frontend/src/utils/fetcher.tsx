export const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL || '';

export const fetcher = (url: RequestInfo | URL, ...args: any[]) => fetch(`${baseUrl}${url}`, ...args)
    .then(async (res) => {
        return res;
    })
    .then((res) => {
        if (res.ok) {
            return res.json();
        } else {
            throw new Error(res.statusText);
        }
    });
