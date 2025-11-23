---
applyTo: "**/*.xslt"
---

# GitHub Copilot Instructions for XSLT Templates

## XSLT Template Files (\*.xslt)

When working with XSLT template files (`.xslt`), follow these guidelines and use the official XSLT 1.0 documentation.

### Reference Documentation

- **XSLT 1.0 Specification**: https://www.w3.org/TR/xslt
- **XPath 1.0 Specification**: https://www.w3.org/TR/xpath

### Key Guidelines

1. **Conditional Checks for Optional Elements**
   - Always check if an element exists before accessing nested elements
   - Use `<xsl:if test="element">` to guard against null references
   ```xslt
   <xsl:if test="service/healthCheck">
     <HealthCheck>
       <xsl:value-of select="service/healthCheck/path"/>
     </HealthCheck>
   </xsl:if>
   ```
2. **Math Operations**
   - Use XPath arithmetic operators: `+`, `-`, `*`, `div`, `mod`
   - Wrap expressions in parentheses for clarity: `<xsl:value-of select="(value div divisor)"/>`
   - Use `floor()`, `ceiling()`, `round()` for rounding
   - **Note**: There is no built-in sum function - use loops to calculate sums
