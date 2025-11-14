<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  
  <!-- Parameter for report generation date -->
  <xsl:param name="reportDate" select="'Not Specified'"/>
  <xsl:param name="reportTitle" select="'Employee Directory Report'"/>
  
  <!-- Output as HTML -->
  <xsl:output method="html" indent="yes" encoding="UTF-8"/>
  
  <!-- Root template -->
  <xsl:template match="/">
    <html>
      <head>
        <title>Employee Report - <xsl:value-of select="company/name"/></title>
        <style>
          body {
            font-family: Arial, sans-serif;
            margin: 20px;
            background-color: #f5f5f5;
          }
          .header {
            background-color: #2c3e50;
            color: white;
            padding: 20px;
            border-radius: 5px;
            margin-bottom: 20px;
          }
          .report-info {
            background-color: #34495e;
            color: #ecf0f1;
            padding: 10px 20px;
            border-radius: 3px;
            margin-top: 10px;
            font-size: 0.9em;
          }
          .company-info {
            background-color: white;
            padding: 15px;
            border-radius: 5px;
            margin-bottom: 20px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
          }
          table {
            width: 100%;
            border-collapse: collapse;
            background-color: white;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            border-radius: 5px;
            overflow: hidden;
          }
          th {
            background-color: #3498db;
            color: white;
            padding: 12px;
            text-align: left;
          }
          td {
            padding: 10px 12px;
            border-bottom: 1px solid #ddd;
          }
          tr:hover {
            background-color: #f0f0f0;
          }
          .engineering {
            background-color: #e8f4f8;
          }
          .sales {
            background-color: #f8e8e8;
          }
          .skills {
            font-size: 0.9em;
            color: #666;
          }
          .skill-tag {
            display: inline-block;
            background-color: #ecf0f1;
            padding: 2px 8px;
            margin: 2px;
            border-radius: 3px;
            font-size: 0.85em;
          }
        </style>
      </head>
      <body>
        <div class="header">
          <h1><xsl:value-of select="$reportTitle"/></h1>
          <div class="report-info">
            <strong>Report Generated:</strong> <xsl:value-of select="$reportDate"/>
          </div>
        </div>
        
        <div class="company-info">
          <h2><xsl:value-of select="company/name"/></h2>
          <p><strong>Founded:</strong> <xsl:value-of select="company/founded"/></p>
          <p>
            <strong>Headquarters:</strong> 
            <xsl:value-of select="company/headquarters/city"/>, 
            <xsl:value-of select="company/headquarters/state"/> 
            <xsl:value-of select="company/headquarters/zipCode"/>, 
            <xsl:value-of select="company/headquarters/country"/>
          </p>
          <p><strong>Total Employees:</strong> <xsl:value-of select="count(company/employees/employee)"/></p>
        </div>

        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Name</th>
              <th>Department</th>
              <th>Position</th>
              <th>Salary</th>
              <th>Skills</th>
            </tr>
          </thead>
          <tbody>
            <xsl:apply-templates select="company/employees/employee">
              <xsl:sort select="department"/>
              <xsl:sort select="name"/>
            </xsl:apply-templates>
          </tbody>
        </table>
        
        <div class="company-info" style="margin-top: 20px;">
          <h3>Department Summary</h3>
          <p>
            <strong>Engineering:</strong> 
            <xsl:value-of select="count(company/employees/employee[department='Engineering'])"/> employees
          </p>
          <p>
            <strong>Sales:</strong> 
            <xsl:value-of select="count(company/employees/employee[department='Sales'])"/> employees
          </p>
          <p>
            <strong>Average Salary:</strong> 
            $<xsl:value-of select="format-number(sum(company/employees/employee/salary) div count(company/employees/employee), '#,##0')"/>
          </p>
        </div>
        
        <div class="company-info" style="margin-top: 20px; text-align: center; font-size: 0.85em; color: #7f8c8d;">
          <p>Report generated on <xsl:value-of select="$reportDate"/> | <xsl:value-of select="company/name"/></p>
        </div>
      </body>
    </html>
  </xsl:template>
  
  <!-- Employee template -->
  <xsl:template match="employee">
    <tr>
      <xsl:attribute name="class">
        <xsl:choose>
          <xsl:when test="department='Engineering'">engineering</xsl:when>
          <xsl:when test="department='Sales'">sales</xsl:when>
        </xsl:choose>
      </xsl:attribute>
      <td><xsl:value-of select="@id"/></td>
      <td><strong><xsl:value-of select="name"/></strong></td>
      <td><xsl:value-of select="department"/></td>
      <td><xsl:value-of select="position"/></td>
      <td>$<xsl:value-of select="format-number(salary, '#,##0')"/></td>
      <td class="skills">
        <xsl:apply-templates select="skills/skill"/>
      </td>
    </tr>
  </xsl:template>
  
  <!-- Skill template -->
  <xsl:template match="skill">
    <span class="skill-tag"><xsl:value-of select="."/></span>
    <xsl:if test="position() != last()"> </xsl:if>
  </xsl:template>
  
</xsl:stylesheet>
