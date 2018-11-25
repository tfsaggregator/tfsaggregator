<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:wix="http://schemas.microsoft.com/wix/2006/wi"
                xmlns="http://schemas.microsoft.com/wix/2006/wi"
                exclude-result-prefixes="xsl wix">

  <xsl:output method="xml" indent="yes" omit-xml-declaration="yes" />

  <xsl:strip-space elements="*"/>

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <!--Give Component ID prefix -->
  <xsl:template match="wix:Component/@Id">
    <xsl:attribute name="{name()}">
      <xsl:value-of select="concat('F2019_', .)" />
    </xsl:attribute>
  </xsl:template>

  <!--Give File ID prefix -->
  <xsl:template match="wix:File/@Id">
    <xsl:attribute name="{name()}">
      <xsl:value-of select="concat('F2019_', .)" />
    </xsl:attribute>
  </xsl:template>

</xsl:stylesheet>