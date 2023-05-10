import axios from "axios";
import MockAdapter from "axios-mock-adapter";

export const getMock = () => {
    const mock = new MockAdapter(axios, {
        onNoMatch: "throwException",
    })

    const countAllMockRequests = () => {
        let keys = Object.keys(mock.history);
        return keys.reduce((acc, cur) => acc + mock.history[cur].length, 0);    
    }
    return {
        mock,
        countAllMockRequests,
    }
}
