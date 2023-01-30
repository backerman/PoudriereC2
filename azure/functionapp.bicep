@description('The location into which to deploy resources.')
param location string = resourceGroup().location

@description('The name of the function application; must be globally unique.')
param funcName string

@description('The connection string for the PostgreSQL database.')
param connectionString string

@description('Log analytics workspace to use for App Insights.')
param logAnalyticsWorkspaceId string

var storageAccountName = '${uniqueString(resourceGroup().id)}func'

resource funcStorage 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${funcName}-appinsights'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspaceId
    Request_Source: 'rest'
  }
}

resource funcAsp 'Microsoft.Web/serverfarms@2022-03-01' = {
  location: location
  name: '${funcName}-asp'
  kind: 'windows'
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
}

resource azureFunction 'Microsoft.Web/sites@2022-03-01' = {
  name: funcName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: funcAsp.id
    httpsOnly: true
    siteConfig: {
      use32BitWorkerProcess: false
      appSettings: [
        {
          name: 'PostgresConnection'
          value: connectionString
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${funcStorage.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${funcStorage.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: toLower(funcName)
        }
        {
          name: 'CURRENT_STACK'
          value: 'dotnet-isolated'
        }
      ]
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
    }    
  }
}
