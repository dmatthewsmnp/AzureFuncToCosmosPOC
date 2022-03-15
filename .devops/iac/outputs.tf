output "cosmos_dbacct_id" {
  value = azurerm_cosmosdb_account.dbacct.id
}
output "cosmos_db_id" {
  value = azurerm_cosmosdb_sql_database.db.id
}
output "cosmosrole_id" {
  value = jsondecode(azurerm_resource_group_template_deployment.cosmosdb_client_container_role.output_content).sqlRoleId.value
}