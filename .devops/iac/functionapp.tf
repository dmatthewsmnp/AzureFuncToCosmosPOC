# Create storage account for function app
resource "azurerm_storage_account" "storage" {
  name                     = "fxpocstorage"
  location                 = azurerm_resource_group.rg.location
  resource_group_name      = azurerm_resource_group.rg.name
  account_tier             = "Standard"
  account_replication_type = "LRS"
  lifecycle {
    ignore_changes = [
      tags
    ]
  }
}

# Create app service plan for function app
resource "azurerm_app_service_plan" "plan" {
  name                = "fx-poc-svcplan"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  kind                = "FunctionApp"

  sku {
    tier = "Dynamic"
    size = "Y1"
  }

  lifecycle {
    ignore_changes = [
      tags
    ]
  }
}

# Create function app:
resource "azurerm_function_app" "funcapp" {
  name                       = "fx-poc-functions"
  location                   = azurerm_resource_group.rg.location
  resource_group_name        = azurerm_resource_group.rg.name
  app_service_plan_id        = azurerm_app_service_plan.plan.id
  storage_account_name       = azurerm_storage_account.storage.name
  storage_account_access_key = azurerm_storage_account.storage.primary_access_key
  version                    = "~4"

  app_settings = {

    # Static values required for publish:
    FUNCTIONS_WORKER_RUNTIME = "dotnet-isolated"
    WEBSITE_RUN_FROM_PACKAGE = "1"

    # Dynamic values from other resources:
    APPINSIGHTS_INSTRUMENTATIONKEY = azurerm_application_insights.appinsights.instrumentation_key
    CosmosDBConnection             = "AccountEndpoint=${azurerm_cosmosdb_account.dbacct.endpoint};AccountKey=${azurerm_cosmosdb_account.dbacct.primary_master_key};"
    DBName                         = "fx-poc-db"

  }

  site_config {
    dotnet_framework_version = "v6.0"
  }

  lifecycle {
    ignore_changes = [
      tags
    ]
  }
}