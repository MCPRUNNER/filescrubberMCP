---
applyTo: "**/*.sbn"
---

# GitHub Copilot Instructions for Scriban Templates

## Scriban Template Files (*.sbn)

When working with Scriban template files (`.sbn`), follow these guidelines and use the official Scriban built-in functions documentation.

### Reference Documentation
- **Scriban Built-in Functions**: https://github.com/scriban/scriban/blob/master/doc/builtins.md
- **Scriban Language**: https://github.com/scriban/scriban/blob/master/doc/language.md
- 

### Key Guidelines

1. **Conditional Checks for Optional Properties**
   - Always check if a property exists before accessing nested properties
   - Use `{{- if property }}` to guard against null references
   ```scriban
   {{- if service.healthCheck }}
   Health Check: {{ service.healthCheck.path }}
   {{- end }}
   ```

2. **Math Operations**
   - Use direct arithmetic operators: `+`, `-`, `*`, `/`, `%`
   - Wrap expressions in parentheses for clarity: `{{ (value / divisor) }}`
   - Use `math.floor`, `math.ceil`, `math.round` for rounding
   - **Note**: `math.sum` does NOT exist - use loops to calculate sums

3. **Array Operations**
   - `array.size` - get array length
   - `array.first` - get first element
   - `array.last` - get last element
   - `array.join` - join array elements with separator
   - Use loops for aggregations like sum:
   ```scriban
   {{- total = 0 }}
   {{- for item in items }}
     {{- total = total + item.value }}
   {{- end }}
   ```

4. **String Operations**
   - `string.upcase` / `string.downcase` - change case
   - `string.capitalize` - capitalize first letter
   - `string.strip` - remove whitespace
   - `string.size` - get string length

5. **Whitespace Control**
   - Use `{{-` to strip whitespace before
   - Use `-}}` to strip whitespace after
   - Example: `{{- for item in items -}}`

6. **Object/Type Handling**
   - JSON values may need explicit type handling
   - Use direct arithmetic for numeric operations
   - Boolean values work with ternary operator: `{{ condition ? "yes" : "no" }}`

7. **Common Patterns**
   ```scriban
   # Iterate with index
   {{- for item in items }}
   {{ for.index }}: {{ item.name }}
   {{- end }}
   
   # Conditional output
   {{ property ?? "default value" }}
   
   # Ternary operator
   {{ enabled ? "✅ Enabled" : "❌ Disabled" }}
   ```

### Testing
Always test Scriban templates with actual JSON data to ensure:
- No null reference errors
- Correct type handling for math operations
- Proper whitespace control
- All required functions exist in Scriban

### Additional Resources
- Language Reference: https://github.com/scriban/scriban/tree/master/doc
- Scriban GitHub: https://github.com/scriban/scriban
