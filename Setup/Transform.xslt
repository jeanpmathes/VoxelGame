<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:wix="http://schemas.microsoft.com/wix/2006/wi"
                version="1.0" xmlns="http://schemas.microsoft.com/wix/2006/wi">

    <xsl:output method="xml" indent="yes"/>

    <xsl:key name="ExeToRemove"
             match="wix:Component[ substring( wix:File/@Source, string-length( wix:File/@Source ) - 3 ) = '.exe' ]"
             use="@Id"/>

    <xsl:template match="@*|node()">
        <xsl:copy>
            <xsl:apply-templates select="@*|node()"/>
        </xsl:copy>
    </xsl:template>

    <xsl:template match="wix:Component">
        <xsl:copy>
            <xsl:attribute name="Win64">yes</xsl:attribute>
            <xsl:apply-templates select="@*|node()"/>
        </xsl:copy>
    </xsl:template>

    <xsl:template match="*[ self::wix:Component or self::wix:ComponentRef ][ key( 'ExeToRemove', @Id ) ]"/>
</xsl:stylesheet>
