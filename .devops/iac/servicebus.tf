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

resource "azurerm_servicebus_queue" "sbclientqueue" {
  name                = "fx_poc_client_queue"
  namespace_id        = azurerm_servicebus_namespace.sbnamespace.id
  enable_partitioning = true
}

resource "azurerm_servicebus_queue_authorization_rule" "sbclientqueueauthrule" {
  name     = "fx_poc_client_queue_authrule"
  queue_id = azurerm_servicebus_queue.sbclientqueue.id

  listen = true
  send   = false
  manage = false
}