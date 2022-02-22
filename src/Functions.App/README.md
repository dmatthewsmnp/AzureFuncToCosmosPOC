# Coding/implementation items to-do or consider

1. Set unhandled exception and validation filters (may require small middleware if Mvc-style UseExceptionHandler() and ConfigureApiBehaviorOptions.InvalidModelStateResponseFactory don't work in AzFx) to ensure consistent responses from API, and cut down on duplicating code in every function
2. Will actual CosmosClient methods *always* throw CosmosException if they fail, or do we need duplicate handling code for non-exception cases with non-2xx StatusCode?
3. Possible to do top-level parameter validation in AzFx (like ApiController - for example put a \[RegularExpression\] attribute on a parameter and have it automatically validated)? Otherwise, relying on developer to remember to validate any non-strongly-typed parameters

# General items to consider

1. If supporting PUT/PATCH, then "id" value should be part of Uri; does this mean it should be removed from model (or at least removed from *request* model)? Currently this value is both partition key and id.
