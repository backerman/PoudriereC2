import { makePortable, sortBy } from './utils';
import * as fc from 'fast-check';

it("successfully sorts a list of objects", () => {
    const arb = fc.string({ maxLength: 1 }).chain(sortKey =>
        fc.tuple(fc.string(), fc.string()).map(
            ([a, b]) => {
                let obja: Record<string, string> = {};
                let objb: Record<string, string> = {};
                obja[sortKey] = a;
                objb[sortKey] = b;
                return { key: sortKey, values: [obja, objb] };
            }));

    fc.assert(
        fc.property(arb,
            (testCase) => {
                const sortA = testCase.values[0];
                const sortB = testCase.values[1];
                const sortFunc = sortBy<Record<string, string>, string>(testCase.key);
                let expected: number;
                if (sortA[testCase.key] < sortB[testCase.key])
                    expected = -1;
                else if (sortA[testCase.key] > sortB[testCase.key])
                    expected = 1;
                else
                    expected = 0;
                const actual = sortFunc(sortA, sortB);
                return actual === expected;
            }));
})

it("removes non-portable characters from a string", () => {
    const arb = fc.unicodeString({minLength: 1});
    fc.assert(
        fc.property(arb,
            (testCase) => {
                const actual = makePortable(testCase);
                const hasNonPortableCharacters = /[^-a-z0-9]/g.test(actual);
                return !hasNonPortableCharacters
            }));
})
