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

# Create client queue:
resource "azurerm_servicebus_queue" "sbclientqueue" {
  name                = "fx_poc_client_queue"
  namespace_id        = azurerm_servicebus_namespace.sbnamespace.id
  enable_partitioning = true
}

# Create sending authorization rule for use by on-prem IntegrationEventRelay (may be replaced by RBAC later):
resource "azurerm_servicebus_queue_authorization_rule" "sbclientqueueauthwriter" {
  name     = "fx_poc_client_queue_authwriter"
  queue_id = azurerm_servicebus_queue.sbclientqueue.id

  listen = false
  send   = true
  manage = false
}