export function sortBy<T, K extends keyof T>(propName: K) {
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

export function makeNamePortable(aString: string): string {
    return aString.toLowerCase().replace(/[^a-z0-9]/g, '');
}

export function validatePortableName(val: string): { errMsg: string | undefined, isValid: boolean } {
    let errMsg = undefined;
    if (val === '') {
        errMsg = "Portable name cannot be blank.";
    } else if (val.length > 63) {
        errMsg = "Portable name must be 63 characters or fewer.";
    } else if (!val.match(/^[A-Za-z0-9-]+$/)) {
        errMsg = "Portable name may only contain letters, numbers, and hyphens.";
    }
    return {errMsg, isValid: errMsg === undefined};
};

export function validityState<T>(stateObject: T) {
    function updateValidity<T, K extends keyof T = keyof T>(
        state: Map<K, boolean>,
        action: { field: K, value: boolean }): Map<K, boolean> {
        let newState = new Map(state);
        newState.set(action.field, action.value);
        return newState;
    }    
    const initialState = new Map<keyof T, boolean>();
    for (const key in stateObject) {
        initialState.set(key as keyof T, true);
    }
    return {
        reducer: updateValidity<T>,
        initialState: initialState
    }
}
