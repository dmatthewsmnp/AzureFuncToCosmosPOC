resource "azurerm_resource_group_template_deployment" "cosmosdb_client_container_role" {
  name                = "var.cosmosdb_client_container_role"
  resource_group_name = azurerm_resource_group.rg.name
  tags                = var.tags

  template_content = file("dihOdsRole.json")
  parameters_content = jsonencode({
    "roleName"            = { value = "DihOdsClientFxRole" }
    "cosmosDbAccountId"   = { value = azurerm_cosmosdb_account.dbacct.id }
    "cosmosDbAccountName" = { value = azurerm_cosmosdb_account.dbacct.name }
    "cosmosDbName"        = { value = azurerm_cosmosdb_sql_database.db.name }
    "cosmosContainerName" = { value = azurerm_cosmosdb_sql_container.client.name }
    "fxPrincipalId"       = { value = azurerm_function_app.funcapp.identity.0.principal_id }
  })

  deployment_mode = "Incremental"
}