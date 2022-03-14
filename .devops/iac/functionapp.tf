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
  identity {
    type = "SystemAssigned"
  }

  app_settings = {

    # Static values required for publish:
    FUNCTIONS_WORKER_RUNTIME = "dotnet-isolated"
    WEBSITE_RUN_FROM_PACKAGE = "1"

    # Dynamic values from other resources:
    APPINSIGHTS_INSTRUMENTATIONKEY = azurerm_application_insights.appinsights.instrumentation_key
    CosmosDBConnection             = "AccountEndpoint=${azurerm_cosmosdb_account.dbacct.endpoint};AccountKey=${azurerm_cosmosdb_account.dbacct.primary_master_key};" # TODO: REMOVE ONCE RBAC WORKING
    CosmosDBEndpoint               = azurerm_cosmosdb_account.dbacct.endpoint
    DBName                         = "fx-poc-db"
    AzureWebJobsServiceBus         = join("", [regex("^Endpoint=sb://.+\\.windows.net/;", azurerm_servicebus_namespace.sbnamespace.default_primary_connection_string), "Authentication=ManagedIdentity"])
    ServiceBusTopic                = replace(azurerm_servicebus_topic.mpm_client.name, "~", "/")
    ServiceBusSubscription         = azurerm_servicebus_subscription.dih_mpm_client_sub.name
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

# Grant function app Reader role on Service Bus MPM subscription (to read incoming events)
resource "azurerm_role_assignment" "funcapp_mpmsub_role" {
  scope                = azurerm_servicebus_subscription.dih_mpm_client_sub.id
  role_definition_name = "Azure Service Bus Data Receiver"
  principal_id         = azurerm_function_app.funcapp.identity.0.principal_id
}

# Grant function app Sender role on Service Bus DIH topic (to write outbound events)
resource "azurerm_role_assignment" "funcapp_dihtopic_role" {
  scope                = azurerm_servicebus_topic.dih_client.id
  role_definition_name = "Azure Service Bus Data Sender"
  principal_id         = azurerm_function_app.funcapp.identity.0.principal_id
}

# resource "azurerm_role_assignment" "funcapp_client_container_role" {
#   scope                = azurerm_cosmosdb_sql_container.client.id
#   role_definition_name = "DihOdsClientFxRole"
#   principal_id         = azurerm_function_app.funcapp.identity.0.principal_id
# }

# TODO: Grant role "Cosmos DB Built-in Data Contributor" (role definition ID 00000000-0000-0000-0000-000000000002) to
# azurerm_function_app.funcapp.identity.0.principal_id. Terraform does not yet support assigning CosmosDB roles, see
# issue: https://github.com/hashicorp/terraform-provider-azurerm/issues/13907
