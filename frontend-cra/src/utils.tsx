function sortBy<T, K extends keyof T>(propName: K) {
    return (a: T, b: T): number => {
        const aProp = a[propName];
        const bProp = b[propName];
        if (aProp < bProp) {
            return -1;
        }
        if (aProp > bProp) {
            return 1;
        }
        return 0;
    }
}

export { sortBy };