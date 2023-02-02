import { ConfigFileMetadata, getDataSource } from "./configs";

const sampleList: ConfigFileMetadata[] = [
    {
      id: 'aa5cd502-eb08-4f42-b187-b81c3d849611',
      deleted: true,
      name: 'this was a test',
      fileType: 'poudriereconf'
    },
    {
      id: 'aa5cd502-eb08-4f42-b187-b81c3d849612',
      deleted: false,
      name: 'this is a test',
      fileType: 'poudriereconf',
      jail: 'jail1',
      portSet: 'set1',
      portsTree: 'tree42'
    }
  ]

export function getSampleDataSource() {
    return getDataSource(sampleList);
}