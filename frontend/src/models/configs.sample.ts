import { ConfigFileMetadata, ConfigFileRepository } from "./configs";

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
      jail: 'jail1',
      portSet: 'set1',
      portsTree: 'tree42'
    }
  ]

/// List-backed configuration source for testing.
function getDataSource(data: ConfigFileMetadata[]): ConfigFileRepository {
  return {
      getConfigFiles: async () => data,
      getConfigFile: async (id: string) => data.find(f => f.id === id),
      updateConfigFile: async (meta: ConfigFileMetadata) => {
          const index = data.findIndex(f => f.id === meta.id);
          if (index !== -1) {
              data[index] = meta;
          }
      }
  };
}

const erroringDataSource : ConfigFileRepository = {
  // EvalError is used here instead of Error to prevent red lines in VSCode.
  getConfigFiles: async function (): Promise<ConfigFileMetadata[]> {
    throw new EvalError('Function not implemented.');
  },
  getConfigFile: function (id: string): Promise<ConfigFileMetadata | undefined> {
      throw new EvalError('Function not implemented.');
  },
  updateConfigFile: function (meta: ConfigFileMetadata): Promise<void> {
      throw new EvalError('Function not implemented.');
  }
}

export function getSampleDataSource(): ConfigFileRepository {
    return getDataSource(sampleData);
}

export function getErroringDataSource(): ConfigFileRepository {
  return erroringDataSource;
}