<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:mod="http://www.cycle99.com/projects/sage/modules"
	xmlns:sage="http://www.cycle99.com/projects/sage"
	xmlns:kelp="http://www.cycle99.com/projects/kelp"
	xmlns:set="http://www.cycle99.com/projects/sage/xslt/extensions/set"
	xmlns:xhtml="http://www.w3.org/1999/xhtml"
	xmlns="http://www.w3.org/1999/xhtml"
	exclude-result-prefixes="sage mod kelp set xhtml">

	<xsl:include href="sageres://modules.xslt" />
	<xsl:include href="sageresx://sage/resources/xslt/logic.xsl" />

	<xsl:variable name="view" select="/sage:view"/>
	<xsl:variable name="request" select="$view/sage:request"/>
	<xsl:variable name="response" select="$view/sage:response"/>
	<xsl:variable name="address" select="$view/sage:request/sage:address"/>
	<xsl:variable name="useragent" select="$view/sage:request/sage:useragent"/>

	<xsl:output method="xml" version="1.0" standalone="yes" omit-xml-declaration="yes"
		encoding="utf-8" media-type="text/xml" indent="yes"
		doctype-system="about:legacy-compat"/>

	<xsl:template match="sage:view">
		<xsl:choose>
			<xsl:when test="count($response/sage:model/node()) = 0">
				<html>
					VOID MODEL
				</html>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="$response/sage:model/node()"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="sage:basehref">
		<base href="{$request/@basehref}"/>
	</xsl:template>

	<xsl:template match="sage:version">
		<xsl:value-of select="$request/sage:assembly/@version"/>
	</xsl:template>

	<xsl:template match="sage:literal">
		<xsl:apply-templates select="node()"/>
	</xsl:template>

	<xsl:template match="xhtml:html">
		<html>
			<xsl:apply-templates select="@*"/>
			<xsl:attribute name="data-thread">
				<xsl:value-of select="$request/@thread" />
			</xsl:attribute>
			<xsl:apply-templates select="node()"/>
		</html>
	</xsl:template>

	<xsl:template match="xhtml:head">
		<xsl:variable name="styles" select="$response/sage:resources/sage:head/xhtml:link | xhtml:link"/>
		<xsl:variable name="scripts" select="$response/sage:resources/sage:head/xhtml:script | xhtml:script"/>
		<head>
			<xsl:apply-templates select="@*"/>
			<xsl:apply-templates select="node()[local-name() != 'script' and local-name() != 'link']"/>
			<xsl:apply-templates select="set:distinct($styles, '@href', true())"/>
			<xsl:apply-templates select="set:distinct($scripts, '@src', true())"/>
		</head>
	</xsl:template>

	<xsl:template match="xhtml:body">
		<body>
			<xsl:apply-templates select="@*"/>
			<xsl:apply-templates select="node()"/>
			<xsl:apply-templates select="$response/sage:resources/sage:body/xhtml:link"/>
			<xsl:apply-templates select="$response/sage:resources/sage:body/xhtml:script"/>
			<xsl:apply-templates select="." mode="execute-libraries"/>
		</body>
	</xsl:template>

	<xsl:template match="xhtml:script[starts-with(@src, 'kelp://')]">
		<xsl:for-each select="document(@src)/*/kelp:resource">
			<xsl:choose>
				<xsl:when test="@exists = 'false'">
					<xsl:comment> File not found: <xsl:value-of select="@path"/> </xsl:comment>
				</xsl:when>
				<xsl:otherwise>
					<script type="text/javascript" language="javascript" src="{@src}"></script>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="xhtml:link[starts-with(@href, 'kelp://')]">
		<xsl:for-each select="document(@href)/*/kelp:resource">
			<xsl:choose>
				<xsl:when test="@exists = 'false'">
					<xsl:comment>
						File not found: <xsl:value-of select="@path"/>
					</xsl:comment>
				</xsl:when>
				<xsl:otherwise>
					<link type="text/css" rel="stylesheet" href="{@src}" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="sage:link">
		<a>
			<xsl:apply-templates select="@*[name() != 'ref' and name() != 'values' and name() != 'escape']"/>
			<xsl:apply-templates select="node()"/>
		</a>
	</xsl:template>

	<xsl:template match="sage:resource[@type='script']">
		<script type="text/javascript" language="javascript" src="{@path}"></script>
	</xsl:template>

	<xsl:template match="sage:resource[@type='style']">
		<link type="text/css" rel="stylesheet" href="{@path}" />
	</xsl:template>

	<xsl:template match="@xml:base | @xml:space"/>

	<xsl:template match="xhtml:*">
		<xsl:element name="{local-name()}">
			<xsl:apply-templates select="@*"/>
			<xsl:apply-templates select="node()"/>
		</xsl:element>
	</xsl:template>

	<xsl:template match="*">
		<xsl:element name="{name()}" namespace="{namespace-uri()}">
			<xsl:apply-templates select="@*"/>
			<xsl:apply-templates select="node()"/>
		</xsl:element>
	</xsl:template>

	<xsl:template match="@*">
		<xsl:attribute name="{name()}">
			<xsl:value-of select="."/>
		</xsl:attribute>
	</xsl:template>

	<xsl:template match="text()">
		<xsl:value-of select="."/>
	</xsl:template>

	<xsl:template match="*" mode="execute-libraries"/>

</xsl:stylesheet>
