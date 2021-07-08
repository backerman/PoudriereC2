import { sortBy } from './utils';
import * as fc from 'fast-check';

// This test is probably overkill, but Maxim #37 applies.
it("successfully sorts a list of objects", () => {
    const objectsToSortArb =
        // Generate a key to hold the sorting key
        fc.string({minLength: 1}).chain(sortKey =>
            // And a random number of objects to sort
            fc.nat(25).chain((numObjectsToSort) =>
                // A list of sort key values
                fc.array(fc.string(), { minLength: numObjectsToSort, maxLength: numObjectsToSort })
                    .chain(sortKeyValues =>
                        // Generate the objects to be sorted
                        fc.array(fc.object({values: [fc.string()]}),
                            { minLength: numObjectsToSort,
                                maxLength: numObjectsToSort})
                            .map((obj) => {
                                // Put a random string into the sort key of each object
                                // and return the generated list of objects along with
                                // the sort key used.
                                return {
                                    key: sortKey,
                                    values: (
                                        obj.map((rec, i) => {
                                            rec[sortKey] = sortKeyValues[i];
                                            return rec;
                                        }))
                                }
                            }))));
    fc.assert(
        fc.property(objectsToSortArb,
            (sortees) => {
                const sortKey = sortees.key;
                const sorted = sortees.values.sort(sortBy(sortKey));
                return sorted.every((rec, i) =>
                    i === (sorted.length - 1) || // skips the last element
                    (rec[sortKey] as string) <= (sortees.values[i+1][sortKey] as string));
            }));
})