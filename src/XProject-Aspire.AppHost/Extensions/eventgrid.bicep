
param location string = resourceGroup().location
param topic_name string = 'aspirify-xproject-integrations'
param function_app_id string = '/subscriptions/87e90c7b-ab20-4554-a894-ef30663d05f5/resourceGroups/rg-vj-aspirify/providers/Microsoft.Web/sites/functions'
param storageAccounts_externalid string = '/subscriptions/87e90c7b-ab20-4554-a894-ef30663d05f5/resourceGroups/rg-vj-aspirify/providers/Microsoft.Storage/storageAccounts/storage'

resource topics_resource_name 'Microsoft.EventGrid/topics@2025-04-01-preview' = {
  name: topic_name
  location: location
  sku: {
    name: 'Basic'
  }
  kind: 'Azure'
  identity: {
    type: 'None'
  }
  properties: {
    minimumTlsVersionAllowed: '1.2'
    inputSchema: 'CloudEventSchemaV1_0'
    publicNetworkAccess: 'Enabled'
    inboundIpRules: []
    disableLocalAuth: false
    dataResidencyBoundary: 'WithinGeopair'
  }
}

resource topics_evgt_product_counter 'Microsoft.EventGrid/topics/eventSubscriptions@2025-04-01-preview' = {
  parent: topics_resource_name
  name: 'evgs-xproject-integrations-product-count-updater'
  properties: {
    destination: {
      properties: {
        resourceId: '${function_app_id}/functions/UpdateProductCount'
        maxEventsPerBatch: 1
        preferredBatchSizeInKilobytes: 64
      }
      endpointType: 'AzureFunction'
    }
    filter: {
      includedEventTypes: [
        'OrderCreated'
      ]
      enableAdvancedFilteringOnArrays: false
    }
    labels: []
    eventDeliverySchema: 'CloudEventSchemaV1_0'
    retryPolicy: {
      maxDeliveryAttempts: 30
      eventTimeToLiveInMinutes: 1440
    }
    deadLetterDestination: {
      properties: {
        resourceId: storageAccounts_externalid
        blobContainerName: 'updateproduct'
      }
      endpointType: 'StorageBlob'
    }
  }
}

