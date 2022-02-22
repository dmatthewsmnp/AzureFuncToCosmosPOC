# Set up required providers and backend
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 2.97.0"
    }
  }
  required_version = ">= 1.1.6"
}

# Create resource group "rg" for deployment
resource "azurerm_resource_group" "rg" {
  name     = local.application_name
  location = "Canada Central"
  tags     = var.tags
}

# Provider housekeeping (no configuration options currently required)
provider "azurerm" {
  features {}
}
