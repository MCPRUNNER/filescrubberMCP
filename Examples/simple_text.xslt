<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  
  <!-- Output as text -->
  <xsl:output method="text" encoding="UTF-8"/>
  
  <!-- Root template -->
  <xsl:template match="/">
    <xsl:text>Person Profile&#10;</xsl:text>
    <xsl:text>==============&#10;&#10;</xsl:text>
    
    <xsl:text>Name:   </xsl:text>
    <xsl:value-of select="person/name"/>
    <xsl:text>&#10;</xsl:text>
    
    <xsl:text>Age:    </xsl:text>
    <xsl:value-of select="person/age"/>
    <xsl:text> years old&#10;</xsl:text>
    
    <xsl:text>Email:  </xsl:text>
    <xsl:value-of select="person/email"/>
    <xsl:text>&#10;</xsl:text>
    
    <xsl:text>Status: </xsl:text>
    <xsl:choose>
      <xsl:when test="person/active='true'">Active User</xsl:when>
      <xsl:otherwise>Inactive User</xsl:otherwise>
    </xsl:choose>
    <xsl:text>&#10;</xsl:text>
  </xsl:template>
  
</xsl:stylesheet>
