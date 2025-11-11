# Copilot Query Examples for FileScrubber MCP

This document provides example queries you can use with GitHub Copilot or other AI assistants to interact with the FileScrubber MCP Server and the example files in the `Examples/` directory.

## Table of Contents

- [JSON File Examples](#json-file-examples)
  - [simple.json](#simplejson)
  - [medium.json](#mediumjson)
  - [complex.json](#complexjson)
- [XML File Examples](#xml-file-examples)
  - [simple.xml](#simplexml)
  - [medium.xml](#mediumxml)
  - [complex.xml](#complexxml)
- [YAML File Examples](#yaml-file-examples)
  - [simple.yaml](#simpleyaml)
  - [medium.yaml](#mediumyaml)
  - [complex.yaml](#complexyaml)
- [XSLT Transformation Examples](#xslt-transformation-examples)
  - [simple_text.xslt](#simple_textxslt)
  - [employee_report.xslt](#employee_reportxslt)
- [Scriban Template Examples](#scriban-template-examples)
  - [simple_file_list.sbn](#simple_file_listsbn)
  - [file_list_report.sbn](#file_list_reportsbn)
  - [company_employee_report.sbn](#company_employee_reportsbn)
  - [uri_fetched_employee_report.sbn](#uri_fetched_employee_reportsbn)

---

## JSON File Examples

### simple.json

**File Structure:**

```json
{
  "name": "John Doe",
  "age": 30,
  "email": "john.doe@example.com",
  "active": true
}
```

#### Example Queries

**Basic Read:**

```
Read the Examples/simple.json file
```

**Query with JSONPath:**

```
Search Examples/simple.json for the person's email address
```

```
Use #fscrub_parser_search_json to get the name from Examples/simple.json using JSONPath $.name
```

**Multiple Fields:**

```
Get the name and email from Examples/simple.json
```

```
Search Examples/simple.json for all fields using JSONPath $.*
```

**Conditional Query:**

```
Check if the person is active in Examples/simple.json
```

---

### medium.json

**File Structure:**

```json
{
  "company": "Tech Solutions Inc",
  "employees": [
    {
      "id": 1,
      "name": "Alice Smith",
      "department": "Engineering",
      "position": "Senior Developer",
      "salary": 95000,
      "skills": ["C#", "JavaScript", "SQL"]
    },
    ...
  ],
  "founded": 2010,
  "headquarters": {...}
}
```

#### Example Queries

**Find All Employees:**

```
Get all employees from Examples/medium.json
```

```
Use #fscrub_parser_search_json to search Examples/medium.json for all employees using $.employees[*]
```

**Filter by Department:**

```
Find all Engineering employees in Examples/medium.json
```

```
Search Examples/medium.json for employees in the Engineering department using JSONPath $.employees[?(@.department == 'Engineering')]
```

**Get Specific Fields:**

```
Get just the names and positions of all employees from Examples/medium.json
```

```
List all employee names from Examples/medium.json using JSONPath $.employees[*].name
```

**Salary Queries:**

```
Find employees with salary greater than 80000 in Examples/medium.json
```

```
Use #fscrub_parser_search_json to find high earners in Examples/medium.json using $.employees[?(@.salary > 80000)]
```

**Skills Search:**

```
Find all employees who have Docker as a skill in Examples/medium.json
```

```
Search for employees with JavaScript skills in Examples/medium.json
```

**Company Info:**

```
Get the company headquarters information from Examples/medium.json
```

```
Use JSONPath $.headquarters to get office location from Examples/medium.json
```

---

### complex.json

**File Structure:**

```json
{
  "apiVersion": "v1",
  "metadata": {...},
  "configuration": {
    "database": {...},
    "cache": {...},
    "services": [...]
  },
  "monitoring": {...},
  "security": {...}
}
```

#### Example Queries

**Service Discovery:**

```
List all microservices from Examples/complex.json
```

```
Use #fscrub_parser_search_json to get all service names from Examples/complex.json using $.configuration.services[*].name
```

**Resource Queries:**

```
Find all services with more than 2 replicas in Examples/complex.json
```

```
Search Examples/complex.json for services using JSONPath $.configuration.services[?(@.replicas > 2)]
```

**Database Configuration:**

```
Get all database configurations from Examples/complex.json
```

```
Find database replicas in Examples/complex.json using $.configuration.database.replicas[*]
```

**Monitoring Alerts:**

```
List all critical alerts from Examples/complex.json
```

```
Use JSONPath $.monitoring.alerts[?(@.severity == 'critical')] to find critical alerts in Examples/complex.json
```

**Security Settings:**

```
Get the authentication configuration from Examples/complex.json
```

**Nested Searches:**

```
Find all health check paths in Examples/complex.json
```

```
Search for all service endpoints in Examples/complex.json using $.configuration.services[*].endpoints[*]
```

---

## XML File Examples

### simple.xml

**File Structure:**

```xml
<person>
  <name>John Doe</name>
  <age>30</age>
  <email>john.doe@example.com</email>
  <active>true</active>
</person>
```

#### Example Queries

**Basic Read:**

```
Read Examples/simple.xml
```

**XPath Queries:**

```
Use #fscrub_parser_search_xml to get the person's name from Examples/simple.xml using XPath //person/name
```

```
Search Examples/simple.xml for the email using XPath //email/text()
```

**Attribute and Element:**

```
Get all child elements of person from Examples/simple.xml
```

```
Use XPath //person/* to get all person fields from Examples/simple.xml
```

---

### medium.xml

**File Structure:**

```xml
<company>
  <name>Tech Solutions Inc</name>
  <employees>
    <employee id="1">
      <name>Alice Smith</name>
      <department>Engineering</department>
      ...
    </employee>
  </employees>
</company>
```

#### Example Queries

**Employee Searches:**

```
Get all employee names from Examples/medium.xml
```

```
Use #fscrub_parser_search_xml to find all employees in Examples/medium.xml using XPath //employee
```

**Attribute Queries:**

```
Get all employee IDs from Examples/medium.xml using XPath //employee/@id
```

**Department Filter:**

```
Find Engineering employees in Examples/medium.xml using XPath //employee[department='Engineering']
```

**Nested Elements:**

```
Get all skills from Examples/medium.xml using XPath //skills/skill
```

```
List skills for employee with id=1 in Examples/medium.xml using XPath //employee[@id='1']//skill
```

**Company Information:**

```
Get headquarters city from Examples/medium.xml using XPath //headquarters/city/text()
```

---

### complex.xml

**File Structure:**

```xml
<configuration>
  <metadata>...</metadata>
  <database>...</database>
  <services>...</services>
  <monitoring>...</monitoring>
</configuration>
```

#### Example Queries

**Service Queries:**

```
Find all microservice names from Examples/complex.xml
```

```
Use XPath //service/@name to get service names from Examples/complex.xml
```

**Conditional Searches:**

```
Find services with more than 2 replicas in Examples/complex.xml using XPath //service[replicas > 2]
```

**Database Configuration:**

```
Get all database nodes from Examples/complex.xml using XPath //database//*
```

**Alert Searches:**

```
Find critical alerts in Examples/complex.xml using XPath //alert[severity='critical']
```

---

## YAML File Examples

### simple.yaml

**File Structure:**

```yaml
name: John Doe
age: 30
email: john.doe@example.com
active: true
```

#### Example Queries

**Basic Read:**

```
Read Examples/simple.yaml
```

**JSONPath Queries:**

```
Use #fscrub_parser_search_yaml to get the name from Examples/simple.yaml using $.name
```

```
Get all fields from Examples/simple.yaml using JSONPath $.*
```

---

### medium.yaml

**File Structure:**

```yaml
company: Tech Solutions Inc
employees:
  - id: 1
    name: Alice Smith
    department: Engineering
    ...
```

#### Example Queries

**Employee Lists:**

```
Get all employees from Examples/medium.yaml
```

```
Use #fscrub_parser_search_yaml to search for employees in Examples/medium.yaml using $.employees[*]
```

**Department Filters:**

```
Find Engineering employees in Examples/medium.yaml using JSONPath $.employees[?(@.department == 'Engineering')]
```

**Skill Searches:**

```
Get all employee skills from Examples/medium.yaml
```

```
Find employees with C# skills in Examples/medium.yaml
```

---

### complex.yaml

**File Structure:**

```yaml
apiVersion: v1
metadata:
  name: enterprise-system
configuration:
  database:
    primary:
      host: db-primary.example.com
      port: 5432
```

#### Example Queries

**Configuration Queries:**

```
Get database configuration from Examples/complex.yaml
```

```
Use #fscrub_parser_search_yaml to find all databases with port 5432 in Examples/complex.yaml using $..[?(@.port == '5432')]
```

**Service Discovery:**

```
List all services from Examples/complex.yaml
```

```
Get service names from Examples/complex.yaml using $.configuration.services[*].name
```

**Monitoring Settings:**

```
Find all critical alerts in Examples/complex.yaml
```

```
Get Prometheus configuration from Examples/complex.yaml using $.monitoring.metrics.prometheus
```

---

## XSLT Transformation Examples

### simple_text.xslt

**Purpose:** Transforms simple.xml to plain text format

#### Example Queries

**Basic Transformation:**

```
Transform Examples/simple.xml using Examples/simple_text.xslt
```

```
Use #fscrub_parser_transform_xml to transform Examples/simple.xml with Examples/simple_text.xslt
```

**Transform and Save:**

```
Transform Examples/simple.xml with Examples/simple_text.xslt and save to output/person.txt
```

```
Use #fscrub_parser_transform_xml to convert Examples/simple.xml to text using Examples/simple_text.xslt and save to Documents/person_profile.txt
```

---

### employee_report.xslt

**Purpose:** Transforms medium.xml to styled HTML employee report

#### Example Queries

**Generate HTML Report:**

```
Transform Examples/medium.xml using Examples/employee_report.xslt to create an employee report
```

```
Use #fscrub_parser_transform_xml to generate an HTML employee report from Examples/medium.xml using Examples/employee_report.xslt
```

**Save to File:**

```
Transform Examples/medium.xml with Examples/employee_report.xslt and save as Documents/employee_report.html
```

**Preview Transformation:**

```
Show me what the transformed output looks like when applying Examples/employee_report.xslt to Examples/medium.xml
```

---

## Scriban Template Examples

### simple_file_list.sbn

**Purpose:** Simple file listing template

#### Example Queries

**Basic Usage:**

```
Use the simple_file_list.sbn template to list files in the current directory
```

**With File List Data:**

```
Get a file list from the Examples directory and render it using Examples/simple_file_list.sbn
```

```
Use #fscrub_file_list to get files from Examples/ then use #fscrub_scriban_process_template with Examples/simple_file_list.sbn to create a file listing
```

**Complete Workflow:**

```
List all .cs files recursively, then process with Examples/simple_file_list.sbn template and save to Documents/code_files.md
```

---

### file_list_report.sbn

**Purpose:** Comprehensive file listing report with metadata

#### Example Queries

**Generate File Report:**

```
Create a file listing report for the entire project using Examples/file_list_report.sbn
```

```
Use #fscrub_file_list to get all files recursively, then use #fscrub_scriban_process_template with Examples/file_list_report.sbn to generate a detailed report
```

**Specific Directory:**

```
Generate a file report for the Services directory using the file_list_report.sbn template
```

**Save Output:**

```
Create a file listing report for all C# files and save to Documents/csharp_files_report.md using Examples/file_list_report.sbn
```

**Complete Example:**

```
Get a recursive file list from Examples/, process it with Examples/file_list_report.sbn template, and save to Documents/examples_report.md
```

---

### company_employee_report.sbn

**Purpose:** Generates formatted employee report from company JSON data

#### Example Queries

**Fetch and Process URI Data:**

```
Use #fscrub_uri_get to fetch data from https://raw.githubusercontent.com/MCPRUNNER/filescrubberMCP/refs/heads/main/Examples/medium.json and then use #fscrub_scriban_process_template with Examples/company_employee_report.sbn to generate a company employee report and save to Documents/company_report.txt
```

**Step-by-Step:**

```
1. Fetch employee data from GitHub using #fscrub_uri_get from https://raw.githubusercontent.com/MCPRUNNER/filescrubberMCP/refs/heads/main/Examples/medium.json
2. Process the JSON data with #fscrub_scriban_process_template using Examples/company_employee_report.sbn
3. Save output to Documents/employee_directory.txt
```

**Direct Process:**

```
Get https://raw.githubusercontent.com/MCPRUNNER/filescrubberMCP/refs/heads/main/Examples/medium.json via URI and render it using Examples/company_employee_report.sbn template
```

---

### uri_fetched_employee_report.sbn

**Purpose:** Comprehensive employee analytics report with department summaries and statistics

#### Example Queries

**Fetch and Generate Analytics Report:**

```
Use #fscrub_uri_get to fetch https://raw.githubusercontent.com/MCPRUNNER/filescrubberMCP/refs/heads/main/Examples/medium.json and process with #fscrub_scriban_process_template using Examples/uri_fetched_employee_report.sbn to create an analytics report saved to Documents/employee_analytics.txt
```

**Complete Workflow:**

```
1. Use #fscrub_uri_get to retrieve employee data from https://raw.githubusercontent.com/MCPRUNNER/filescrubberMCP/refs/heads/main/Examples/medium.json
2. Apply #fscrub_scriban_process_template with Examples/uri_fetched_employee_report.sbn template
3. Save detailed analytics report to Documents/company_analytics_report.txt
```

**Preview Output:**

```
Fetch medium.json from GitHub and show me the rendered output from Examples/uri_fetched_employee_report.sbn template
```

---

## Advanced Combined Queries

### JSON to Report

```
Search Examples/medium.json for all Engineering employees, then create a report using a template
```

### Multiple File Analysis

```
Read all JSON files in Examples/ and show me the structure of each
```

### Data Extraction and Transformation

```
Extract employee data from Examples/medium.json, transform to XML format, then apply Examples/employee_report.xslt
```

### File Discovery and Documentation

```
List all example files, get their metadata, and generate a documentation file using the file_list_report.sbn template
```

### Complex Pipeline

```
1. Search Examples/complex.yaml for all services
2. Extract service configuration
3. Create a formatted report using a Scriban template
```

---

## Tips for Effective Queries

1. **Be Specific:** Mention the exact file path and tool name when possible
2. **Use Tool References:** Reference tools with `#fscrub_*` for clarity
3. **Specify Output:** Indicate whether you want results displayed or saved to a file
4. **JSONPath Syntax:** Use standard JSONPath syntax (e.g., `$.employees[*]`, `$..name`)
5. **XPath Syntax:** Use standard XPath syntax (e.g., `//employee`, `//employee[@id='1']`)
6. **Chain Operations:** Combine multiple tools for complex workflows
7. **Test Incrementally:** Start with simple queries and build complexity

---

## Common Query Patterns

### Pattern 1: Search and Display

```
Search [file] for [data] using [query]
```

### Pattern 2: Transform and Save

```
Transform [xml-file] with [xslt-file] and save to [output-path]
```

### Pattern 3: List and Report

```
List files from [directory] and generate report using [template] saved to [output]
```

### Pattern 4: Extract and Process

```
Extract [data] from [file], process with [template/transformation], save to [output]
```

---

## See Also

- [SEARCH_EXAMPLES.md](SEARCH_EXAMPLES.md) - Detailed search query syntax and examples
- [SCRIBAN_TEMPLATES_README.md](SCRIBAN_TEMPLATES_README.md) - Scriban template documentation
- [README.md](../README.md) - Main documentation with tool reference
