resource "azurerm_cosmosdb_account" "dbacct" {
  name                = "fx-poc"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  offer_type          = "Standard"

  consistency_policy {
    consistency_level = "ConsistentPrefix"
  }

  geo_location {
    location          = azurerm_resource_group.rg.location
    failover_priority = 0
  }
}

resource "azurerm_cosmosdb_sql_database" "db" {
  name                = "${resource.azurerm_cosmosdb_account.dbacct.name}-db"
  resource_group_name = resource.azurerm_cosmosdb_account.dbacct.resource_group_name
  account_name        = resource.azurerm_cosmosdb_account.dbacct.name
}

resource "azurerm_cosmosdb_sql_container" "person" {
  name                  = "Person"
  resource_group_name   = resource.azurerm_cosmosdb_account.dbacct.resource_group_name
  account_name          = resource.azurerm_cosmosdb_account.dbacct.name
  database_name         = resource.azurerm_cosmosdb_sql_database.db.name
  partition_key_path    = "/id"
  partition_key_version = 1 # Partition key will not be over 101 bytes
}
