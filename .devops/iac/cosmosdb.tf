# Create CosmosDB account
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

# Create CosmosDB database
resource "azurerm_cosmosdb_sql_database" "db" {
  name                = "${azurerm_cosmosdb_account.dbacct.name}-db"
  resource_group_name = azurerm_cosmosdb_account.dbacct.resource_group_name
  account_name        = azurerm_cosmosdb_account.dbacct.name
}

# Create CosmosDB container for Person objects
resource "azurerm_cosmosdb_sql_container" "person" {
  name                  = "Person"
  resource_group_name   = azurerm_cosmosdb_account.dbacct.resource_group_name
  account_name          = azurerm_cosmosdb_account.dbacct.name
  database_name         = azurerm_cosmosdb_sql_database.db.name
  partition_key_path    = "/id"
  partition_key_version = 1 # Partition key will not be over 101 bytes
}

# Create CosmosDB container for Client objects
resource "azurerm_cosmosdb_sql_container" "client" {
  name                  = "Client"
  resource_group_name   = azurerm_cosmosdb_account.dbacct.resource_group_name
  account_name          = azurerm_cosmosdb_account.dbacct.name
  database_name         = azurerm_cosmosdb_sql_database.db.name
  partition_key_path    = "/id"
  partition_key_version = 1 # Partition key will not be over 101 bytes
}
