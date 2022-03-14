# Create Service Bus namespace:
resource "azurerm_servicebus_namespace" "sbnamespace" {
  name                = "fx-poc-servicebus-namespace"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  sku                 = "Standard"

  lifecycle {
    ignore_changes = [
      tags
    ]
  }
}

# Create mpm/client topic
resource "azurerm_servicebus_topic" "mpm_client" {
  name         = "mpm~client~v1"
  namespace_id = azurerm_servicebus_namespace.sbnamespace.id
}

# Create dih/client topic
resource "azurerm_servicebus_topic" "dih_client" {
  name         = "dih~client~v1"
  namespace_id = azurerm_servicebus_namespace.sbnamespace.id
}

# Create mpm/client topic subscription:
resource "azurerm_servicebus_subscription" "dih_mpm_client_sub" {
  name               = "dih_mpm_client_sub"
  topic_id           = azurerm_servicebus_topic.mpm_client.id
  max_delivery_count = 1
}

# Create sending authorization rule for use by on-prem IntegrationEventRelay (may be replaced by RBAC later):
resource "azurerm_servicebus_topic_authorization_rule" "mpm_client_send_rule" {
  name     = "mpm_client_send_key"
  topic_id = azurerm_servicebus_topic.mpm_client.id

  listen = false
  send   = true
  manage = false
}