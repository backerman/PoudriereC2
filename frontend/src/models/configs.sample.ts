import { ConfigFileMetadata } from "./configs";

export const sampleData: ConfigFileMetadata[] = [
    {
      id: 'aa5cd502-eb08-4f42-b187-b81c3d849611',
      deleted: true,
      name: 'this was a test',
      fileType: 'poudriereconf'
    },
    {
      id: 'c1fac43d-49de-4821-8ff5-8157cf8f5e29',
      deleted: false,
      name: 'this is a test',
      fileType: 'poudriereconf',
      jail: 'f11fe517-b8d7-454b-b264-7a25cdd41065',
      jailName: 'jail1',
      portSet: '3bb71b44-869a-493c-aa82-aaf9f337fc50',
      portSetName: 'Yog-Sothoth',
      portsTree: 'db2a4a2f-7191-48bd-933e-d206e68c0837',
      portsTreeName: 'Nullvad'
    }
  ]
