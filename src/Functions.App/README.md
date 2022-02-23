# Coding/implementation items to-do or consider

1. Set unhandled exception and validation filters (may require small middleware if Mvc-style UseExceptionHandler() and ConfigureApiBehaviorOptions.InvalidModelStateResponseFactory don't work in AzFx) to ensure consistent responses from API, and cut down on duplicating code in every function
2. Possible to do top-level parameter validation in AzFx (like ApiController - for example put a \[RegularExpression\] attribute on a parameter and have it automatically validated)? Otherwise, relying on developer to remember to validate any non-strongly-typed parameters
