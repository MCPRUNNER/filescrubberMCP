---
applyTo: "**/*.dtsx"
---

# GitHub Copilot Instructions for DTSX Packages

## DTSX Package Files (\*.dtsx)

When working with DTSX package files (`.dtsx`), follow these guidelines and use the official Microsoft documentation for SQL Server Integration Services (SSIS).

### Reference Documentation

- **SSIS Documentation**: https://docs.microsoft.com/en-us/sql/integration-services/sql-server-integration-services
- **DTSX File Format**: https://docs.microsoft.com/en-us/sql/integration-services/packages/package-file-format

### Key Guidelines

1. **Conditional Checks for Optional Properties**
   - Always check if a property exists before accessing nested properties
   - Use appropriate XML conditional checks to guard against null references
   ```xml
   <PropertyExpression>
     <Expression>ISNULL(@[User::MyVariable]) ? "DefaultValue" : @[User::MyVariable]</Expression>
   </PropertyExpression>
   -->
   ```
2. **Math Operations**
   - Use standard arithmetic operators: `+`, `-`, `*`, `/`
   - Wrap expressions in parentheses for clarity: `(value / divisor)`
   - Use built-in SSIS functions for rounding: `FLOOR()`, `CEILING()`, `ROUND()`
   - **Note**: There is no built-in sum function - use loops or aggregations to calculate sums
