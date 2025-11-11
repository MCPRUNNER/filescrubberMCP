# Search Examples for FileScrubber MCP Parser Tools

This document provides comprehensive examples of search queries for each parser tool, including JSONPath, XPath, and other query syntaxes.

---

## Tool Overview

| MCP Tool                    | File Format | Query Language | Query Parameter | Description                           |
| --------------------------- | ----------- | -------------- | --------------- | ------------------------------------- |
| `fscrub_parse_json`         | JSON        | JSONPath       | `jsonPath`      | Query JSON files with JSONPath        |
| `fscrub_parse_xml`          | XML         | XPath          | `xPath`         | Query XML files with XPath            |
| `fscrub_parse_yaml`         | YAML        | JSONPath       | `jsonPath`      | Query YAML files (converted to JSON)  |
| `fscrub_parse_csv`          | CSV         | JSONPath       | `jsonPath`      | Query CSV files (converted to JSON)   |
| `fscrub_parse_excel`        | Excel/XLSX  | JSONPath       | `jsonPath`      | Query Excel files (converted to JSON) |
| `fscrub_transform_xml_xslt` | XML         | XSLT           | `xsltFilePath`  | Transform XML with XSLT stylesheet    |

**Key Points:**

- **JSONPath** is used for JSON, YAML, CSV, and Excel files
- **XPath** is used for XML files
- **XSLT** is used for XML transformations
- All tools support `indented` formatting and `showKeyPaths` options (except XSLT transform)

---

## Table of Contents

- [JSON File Searching (JSONPath)](#json-file-searching-jsonpath)
- [XML File Searching (XPath)](#xml-file-searching-xpath)
- [YAML File Searching (JSONPath)](#yaml-file-searching-jsonpath)
- [CSV File Searching (JSONPath)](#csv-file-searching-jsonpath)
- [Excel File Searching (JSONPath)](#excel-file-searching-jsonpath)
- [Quick Reference](#quick-reference)

---

## JSON File Searching (JSONPath)

### Tool: `fscrub_parse_json`

JSONPath is a query language for JSON, similar to XPath for XML.

### Basic Syntax

| Operator      | Description        | Example                         |
| ------------- | ------------------ | ------------------------------- |
| `$`           | Root object        | `$`                             |
| `.`           | Child operator     | `$.store.book`                  |
| `..`          | Recursive descent  | `$..author`                     |
| `*`           | Wildcard           | `$.store.*`                     |
| `[]`          | Subscript operator | `$.store.book[0]`               |
| `[,]`         | Union operator     | `$.store.book[0,1]`             |
| `[start:end]` | Array slice        | `$.store.book[0:2]`             |
| `[?()]`       | Filter expression  | `$.store.book[?(@.price < 10)]` |

### Example JSON File: `products.json`

```json
{
  "store": {
    "name": "Tech Store",
    "location": "New York",
    "book": [
      {
        "category": "reference",
        "author": "Nigel Rees",
        "title": "Sayings of the Century",
        "price": 8.95
      },
      {
        "category": "fiction",
        "author": "Evelyn Waugh",
        "title": "Sword of Honour",
        "price": 12.99
      },
      {
        "category": "fiction",
        "author": "Herman Melville",
        "title": "Moby Dick",
        "isbn": "0-553-21311-3",
        "price": 8.99
      }
    ],
    "bicycle": {
      "color": "red",
      "price": 19.95
    }
  }
}
```

### Example Queries

#### 1. Get all books

```json
{
  "jsonFilePath": "Examples/products.json",
  "jsonPath": "$.store.book[*]"
}
```

**Result:** Array of all book objects

#### 2. Get all authors

```json
{
  "jsonFilePath": "Examples/products.json",
  "jsonPath": "$.store.book[*].author"
}
```

**Result:** `["Nigel Rees", "Evelyn Waugh", "Herman Melville"]`

#### 3. Get the first book

```json
{
  "jsonFilePath": "Examples/products.json",
  "jsonPath": "$.store.book[0]"
}
```

#### 4. Get books cheaper than $10

```json
{
  "jsonFilePath": "Examples/products.json",
  "jsonPath": "$.store.book[?(@.price < 10)]"
}
```

#### 5. Get all prices in the store (books and bicycle)

```json
{
  "jsonFilePath": "Examples/products.json",
  "jsonPath": "$..price"
}
```

**Result:** `[8.95, 12.99, 8.99, 19.95]`

#### 6. Get books with ISBN

```json
{
  "jsonFilePath": "Examples/products.json",
  "jsonPath": "$.store.book[?(@.isbn)]"
}
```

#### 7. Get fiction category books

```json
{
  "jsonFilePath": "Examples/products.json",
  "jsonPath": "$.store.book[?(@.category == 'fiction')]"
}
```

#### 8. Get titles of all books

```json
{
  "jsonFilePath": "Examples/products.json",
  "jsonPath": "$.store.book[*].title"
}
```

#### 9. Get last two books

```json
{
  "jsonFilePath": "Examples/products.json",
  "jsonPath": "$.store.book[-2:]"
}
```

#### 10. Get everything in the store

```json
{
  "jsonFilePath": "Examples/products.json",
  "jsonPath": "$.store.*"
}
```

---

## XML File Searching (XPath)

### Tool: `fscrub_parse_xml`

XPath is a query language for selecting nodes from an XML document.

### Basic Syntax

| Expression | Description    | Example          |
| ---------- | -------------- | ---------------- |
| `/`        | Root node      | `/catalog`       |
| `//`       | Any descendant | `//book`         |
| `.`        | Current node   | `.`              |
| `..`       | Parent node    | `..`             |
| `@`        | Attribute      | `//@id`          |
| `*`        | Wildcard       | `/catalog/*`     |
| `\|`       | Union          | `//book \| //cd` |
| `[]`       | Predicate      | `//book[1]`      |

### Example XML File: `catalog.xml`

```xml
<?xml version="1.0" encoding="UTF-8"?>
<catalog>
  <book id="bk101" category="programming">
    <author>Gambardella, Matthew</author>
    <title>XML Developer's Guide</title>
    <genre>Computer</genre>
    <price>44.95</price>
    <publish_date>2000-10-01</publish_date>
  </book>
  <book id="bk102" category="fiction">
    <author>Ralls, Kim</author>
    <title>Midnight Rain</title>
    <genre>Fantasy</genre>
    <price>5.95</price>
    <publish_date>2000-12-16</publish_date>
  </book>
  <book id="bk103" category="fiction">
    <author>Corets, Eva</author>
    <title>Maeve Ascendant</title>
    <genre>Fantasy</genre>
    <price>5.95</price>
    <publish_date>2000-11-17</publish_date>
  </book>
</catalog>
```

### Example Queries

#### 1. Get all books

```json
{
  "xmlFilePath": "Examples/catalog.xml",
  "xPath": "//book"
}
```

#### 2. Get all book titles

```json
{
  "xmlFilePath": "Examples/catalog.xml",
  "xPath": "//book/title"
}
```

**Result:** All `<title>` elements

#### 3. Get text content of all titles

```json
{
  "xmlFilePath": "Examples/catalog.xml",
  "xPath": "//book/title/text()"
}
```

**Result:** `["XML Developer's Guide", "Midnight Rain", "Maeve Ascendant"]`

#### 4. Get the first book

```json
{
  "xmlFilePath": "Examples/catalog.xml",
  "xPath": "//book[1]"
}
```

#### 5. Get all book IDs (attributes)

```json
{
  "xmlFilePath": "Examples/catalog.xml",
  "xPath": "//book/@id"
}
```

**Result:** `["bk101", "bk102", "bk103"]`

#### 6. Get books with price less than 10

```json
{
  "xmlFilePath": "Examples/catalog.xml",
  "xPath": "//book[price < 10]"
}
```

#### 7. Get fiction category books

```json
{
  "xmlFilePath": "Examples/catalog.xml",
  "xPath": "//book[@category='fiction']"
}
```

#### 8. Get book by ID

```json
{
  "xmlFilePath": "Examples/catalog.xml",
  "xPath": "//book[@id='bk102']"
}
```

#### 9. Get all authors

```json
{
  "xmlFilePath": "Examples/catalog.xml",
  "xPath": "//author/text()"
}
```

#### 10. Get all prices

```json
{
  "xmlFilePath": "Examples/catalog.xml",
  "xPath": "//price/text()"
}
```

#### 11. Get books in Fantasy genre

```json
{
  "xmlFilePath": "Examples/catalog.xml",
  "xPath": "//book[genre='Fantasy']"
}
```

#### 12. Get count of books

```json
{
  "xmlFilePath": "Examples/catalog.xml",
  "xPath": "count(//book)"
}
```

#### 13. Get all elements with 'category' attribute

```json
{
  "xmlFilePath": "Examples/catalog.xml",
  "xPath": "//*[@category]"
}
```

---

## YAML File Searching (JSONPath)

### Tool: `fscrub_parse_yaml`

YAML files are converted to JSON internally, so they use JSONPath queries.

### Example YAML File: `config.yaml`

```yaml
server:
  host: localhost
  port: 8080
  ssl: true

database:
  connections:
    - name: primary
      host: db1.example.com
      port: 5432
      username: admin
    - name: replica
      host: db2.example.com
      port: 5432
      username: readonly

features:
  - name: authentication
    enabled: true
    config:
      provider: oauth2
  - name: caching
    enabled: false
    config:
      ttl: 3600

users:
  - id: 1
    name: John Doe
    email: john@example.com
    roles:
      - admin
      - developer
  - id: 2
    name: Jane Smith
    email: jane@example.com
    roles:
      - developer
```

### Example Queries

#### 1. Get server configuration

```json
{
  "yamlFilePath": "Examples/config.yaml",
  "jsonPath": "$.server"
}
```

#### 2. Get server port

```json
{
  "yamlFilePath": "Examples/config.yaml",
  "jsonPath": "$.server.port"
}
```

**Result:** `8080`

#### 3. Get all database connections

```json
{
  "yamlFilePath": "Examples/config.yaml",
  "jsonPath": "$.database.connections[*]"
}
```

#### 4. Get primary database host

```json
{
  "yamlFilePath": "Examples/config.yaml",
  "jsonPath": "$.database.connections[?(@.name == 'primary')].host"
}
```

**Result:** `"db1.example.com"`

#### 5. Get all enabled features

```json
{
  "yamlFilePath": "Examples/config.yaml",
  "jsonPath": "$.features[?(@.enabled == true)]"
}
```

#### 6. Get all user emails

```json
{
  "yamlFilePath": "Examples/config.yaml",
  "jsonPath": "$.users[*].email"
}
```

**Result:** `["john@example.com", "jane@example.com"]`

#### 7. Get users with admin role

```json
{
  "yamlFilePath": "Examples/config.yaml",
  "jsonPath": "$.users[?(@.roles[*] == 'admin')]"
}
```

#### 8. Get all feature names

```json
{
  "yamlFilePath": "Examples/config.yaml",
  "jsonPath": "$.features[*].name"
}
```

---

## CSV File Searching (JSONPath)

### Tool: `fscrub_parse_csv`

CSV files are converted to JSON arrays, where each row becomes an object with headers as keys.

### Example CSV File: `employees.csv`

```csv
EmployeeID,FirstName,LastName,Department,Salary,HireDate
101,John,Doe,Engineering,75000,2020-01-15
102,Jane,Smith,Marketing,65000,2019-06-20
103,Bob,Johnson,Engineering,80000,2018-03-10
104,Alice,Williams,Sales,70000,2021-02-28
105,Charlie,Brown,Engineering,72000,2020-11-05
```

### Example Queries

#### 1. Get all employees

```json
{
  "csvFilePath": "Examples/employees.csv",
  "jsonPath": "$[*]"
}
```

#### 2. Get all first names

```json
{
  "csvFilePath": "Examples/employees.csv",
  "jsonPath": "$[*].FirstName"
}
```

**Result:** `["John", "Jane", "Bob", "Alice", "Charlie"]`

#### 3. Get Engineering department employees

```json
{
  "csvFilePath": "Examples/employees.csv",
  "jsonPath": "$[?(@.Department == 'Engineering')]"
}
```

#### 4. Get employees with salary > 70000

```json
{
  "csvFilePath": "Examples/employees.csv",
  "jsonPath": "$[?(@.Salary > 70000)]"
}
```

#### 5. Get first employee

```json
{
  "csvFilePath": "Examples/employees.csv",
  "jsonPath": "$[0]"
}
```

#### 6. Get employee IDs

```json
{
  "csvFilePath": "Examples/employees.csv",
  "jsonPath": "$[*].EmployeeID"
}
```

#### 7. Get employees hired in 2020

```json
{
  "csvFilePath": "Examples/employees.csv",
  "jsonPath": "$[?(@.HireDate =~ /^2020/)]"
}
```

#### 8. Get Sales department employee names

```json
{
  "csvFilePath": "Examples/employees.csv",
  "jsonPath": "$[?(@.Department == 'Sales')].FirstName"
}
```

---

## Excel File Searching (JSONPath)

### Tool: `fscrub_parse_excel`

Excel files are converted to JSON with worksheet names as top-level keys, and rows as arrays of objects.

### Example Excel Structure

**Sheet: "Employees"**
| EmployeeID | Name | Department | Salary |
|------------|------|------------|--------|
| 101 | John Doe | Engineering | 75000 |
| 102 | Jane Smith | Marketing | 65000 |
| 103 | Bob Johnson | Engineering | 80000 |

**Sheet: "Departments"**
| DeptID | DeptName | Manager |
|--------|----------|---------|
| 1 | Engineering | Alice |
| 2 | Marketing | Bob |
| 3 | Sales | Charlie |

### Example Queries

#### 1. Get all employees from Employees sheet

```json
{
  "excelFilePath": "Examples/company.xlsx",
  "jsonPath": "$.Employees[*]"
}
```

#### 2. Get all employee names

```json
{
  "excelFilePath": "Examples/company.xlsx",
  "jsonPath": "$.Employees[*].Name"
}
```

#### 3. Get Engineering employees

```json
{
  "excelFilePath": "Examples/company.xlsx",
  "jsonPath": "$.Employees[?(@.Department == 'Engineering')]"
}
```

#### 4. Get all departments

```json
{
  "excelFilePath": "Examples/company.xlsx",
  "jsonPath": "$.Departments[*]"
}
```

#### 5. Get department names

```json
{
  "excelFilePath": "Examples/company.xlsx",
  "jsonPath": "$.Departments[*].DeptName"
}
```

#### 6. Get data from both sheets

```json
{
  "excelFilePath": "Examples/company.xlsx",
  "jsonPath": "$.*[*]"
}
```

#### 7. Access by column letter (alternative)

```json
{
  "excelFilePath": "Examples/company.xlsx",
  "jsonPath": "$.Employees[*].A"
}
```

_Note: Column letters (A, B, C, etc.) are available if they don't conflict with header names_

#### 8. Get employees with salary > 70000

```json
{
  "excelFilePath": "Examples/company.xlsx",
  "jsonPath": "$.Employees[?(@.Salary > 70000)]"
}
```

#### 9. Get first employee from Employees sheet

```json
{
  "excelFilePath": "Examples/company.xlsx",
  "jsonPath": "$.Employees[0]"
}
```

---

## Quick Reference

### JSONPath Common Patterns

| Pattern                      | Description     | Example                            |
| ---------------------------- | --------------- | ---------------------------------- |
| `$`                          | Root            | `$`                                |
| `$.field`                    | Direct child    | `$.users`                          |
| `$..field`                   | Recursive       | `$..email`                         |
| `$[0]`                       | First item      | `$.users[0]`                       |
| `$[-1]`                      | Last item       | `$.users[-1]`                      |
| `$[0:3]`                     | Slice (first 3) | `$.users[0:3]`                     |
| `$[*]`                       | All items       | `$.users[*]`                       |
| `$[?(@.age > 18)]`           | Filter          | `$.users[?(@.age > 18)]`           |
| `$[?(@.name)]`               | Has field       | `$.users[?(@.name)]`               |
| `$[?(@.status == 'active')]` | Equals          | `$.users[?(@.status == 'active')]` |
| `$[?(@.name =~ /^John/)]`    | Regex           | `$.users[?(@.name =~ /^John/)]`    |

### XPath Common Patterns

| Pattern                    | Description      | Example                |
| -------------------------- | ---------------- | ---------------------- |
| `/element`                 | Root child       | `/catalog`             |
| `//element`                | Anywhere         | `//book`               |
| `//@attribute`             | All attributes   | `//@id`                |
| `//element[1]`             | First match      | `//book[1]`            |
| `//element[@attr='value']` | Attribute filter | `//book[@id='101']`    |
| `//element[child='value']` | Child filter     | `//book[price > 10]`   |
| `//element/text()`         | Text content     | `//title/text()`       |
| `//element/*`              | All children     | `//book/*`             |
| `//element\|//other`       | Union            | `//book\|//cd`         |
| `//element[position()<3]`  | Position         | `//book[position()<3]` |
| `//element[last()]`        | Last             | `//book[last()]`       |

### Filter Expression Operators

**JSONPath Filters:**

- `==` - Equal
- `!=` - Not equal
- `<` - Less than
- `<=` - Less than or equal
- `>` - Greater than
- `>=` - Greater than or equal
- `=~` - Regex match
- `!` - Not

**XPath Predicates:**

- `=` - Equal
- `!=` - Not equal
- `<` - Less than
- `<=` - Less than or equal
- `>` - Greater than
- `>=` - Greater than or equal
- `and` - Logical AND
- `or` - Logical OR
- `not()` - Logical NOT

---

## Advanced Examples

### Combining File List with Templates

1. **List all C# files:**

```json
{
  "directoryPath": "C:\\Projects\\MyApp",
  "searchPattern": "*.cs",
  "recursive": true
}
```

2. **Process with template:**

```json
{
  "templateFilePath": "Examples/file_list_report.sbn",
  "jsonData": "<result_from_step_1>",
  "outputFilePath": "reports/csharp_files.md"
}
```

### Chaining Queries

1. **Extract data from Excel:**

```json
{
  "excelFilePath": "data.xlsx",
  "jsonPath": "$.Sales[?(@.Amount > 1000)]"
}
```

2. **Save to JSON file** (using `fscrub_file_write`)

3. **Query the saved JSON** with more filters

### Complex Filters

**Find users with multiple conditions:**

```json
{
  "jsonPath": "$.users[?(@.age > 18 && @.status == 'active' && @.verified == true)]"
}
```

**Find books by multiple authors:**

```json
{
  "xPath": "//book[author='Smith' or author='Jones']"
}
```

---

## Tips and Best Practices

1. **Test queries incrementally** - Start with simple paths and add filters
2. **Use `showKeyPaths: true`** - Helps understand the structure
3. **Quote strings in filters** - Use single quotes in JSONPath filters
4. **Check data types** - Numeric comparisons don't need quotes
5. **Use recursive descent (`..`)** - When you don't know exact structure
6. **Wildcard for exploration** - Use `*` to see what's available
7. **Array slicing** - Remember arrays are 0-indexed in JSONPath
8. **XPath position()** - 1-indexed in XPath

---

## Troubleshooting

### Common Issues

**JSONPath returns empty:**

- Check if path exists with simpler query
- Verify JSON structure with `jsonPath: "$"`
- Check filter syntax (use `==` not `=`)

**XPath returns empty:**

- Verify XML is well-formed
- Check for namespaces (may need namespace handling)
- Test with simpler XPath like `//` + element name

**CSV column not found:**

- Verify `hasHeaderRecord` is `true` if CSV has headers
- Check exact spelling of column names (case-sensitive)
- Headers are trimmed of whitespace

**Excel sheet not found:**

- Verify exact sheet name (case-sensitive)
- Use `jsonPath: "$"` to see all sheet names
- Check if file has multiple sheets

---

## Additional Resources

- [JSONPath Online Evaluator](https://jsonpath.com/)
- [XPath Tester](https://www.freeformatter.com/xpath-tester.html)
- [JSONPath Specification](https://goessner.net/articles/JsonPath/)
- [XPath Tutorial](https://www.w3schools.com/xml/xpath_intro.asp)

---

_Last updated: November 11, 2025_
