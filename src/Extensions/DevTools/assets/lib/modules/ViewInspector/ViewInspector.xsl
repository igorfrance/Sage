﻿<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:mod="http://www.cycle99.com/projects/sage/modules"
	xmlns:sage="http://www.cycle99.com/projects/sage"
	xmlns:regexp="http://www.cycle99.com/projects/sage/xslt/extensions/regexp"
	xmlns:string="http://www.cycle99.com/projects/sage/xslt/extensions/string"
	xmlns:math="http://www.cycle99.com/projects/sage/xslt/extensions/math"
	xmlns="http://www.w3.org/1999/xhtml"

	exclude-result-prefixes="mod sage regexp string math">

	<xsl:template match="mod:ViewInspector">
		<xsl:param name="config" select="mod:config"/>
		<xsl:variable name="path">
			<xsl:apply-templates select="$config/mod:path"/>
		</xsl:variable>

		<xsl:variable name="cookies" select="/sage:view/sage:request/sage:cookies"/>
		<xsl:variable name="orientation" select="math:isnull($cookies/@dev.viewinspector.orientation, 'horizontal')"/>
		<xsl:variable name="layout" select="math:isnull($cookies/@dev.viewinspector.layout, 'single')"/>
		<xsl:variable name="framewidth" select="math:isnull($cookies/@dev.viewinspector.fw, 70)"/>
		<xsl:variable name="frameheight" select="math:isnull($cookies/@dev.viewinspector.fw, 70)"/>

		<div class="view-inspector {$orientation} {$layout}" data-view="{$path}" data-basehref="{/sage:view/sage:request/@basehref}">
			<xsl:apply-templates select="$config/mod:id" mode="attribute"/>
			<div class="contentcontainer">
				<xsl:attribute name="style">
					<xsl:value-of select="string:format('width: {0}%; ', $framewidth)"/>
					<xsl:value-of select="string:format('height: {0}%; ', $frameheight)"/>
				</xsl:attribute>
				<div class="contenttoolbar">
					<div class="contenttitle"></div>
					<div class="toolbar right">
						<div class="button reload" data-command="reload" title="Reload the current page"></div>
						<div class="button info" data-command="showInfo" title="About View Inspector"></div>
						<div class="button close" data-command="close" title="Close View Inspector"></div>
					</div>
				</div>
				<div class="contentframe">
					<iframe src="about:blank" frameborder="0" width="100%" height="100%"></iframe>
				</div>
				<div class="contentpreloader"></div>
			</div>
			<div class="toolscontainer">
				<xsl:attribute name="style">
					<xsl:value-of select="string:format('width: {0}%; ', 100 - $framewidth)"/>
					<xsl:value-of select="string:format('height: {0}%; ', 100 - $frameheight)"/>
				</xsl:attribute>
				<div class="viewtoolbar">
					<div class="toolbar left">
						<xsl:for-each select="mod:data/mod:meta/mod:view">
							<div class="textbutton meta {@name}" title="{.}" data-command="viewMeta" data-arguments="{@name}">
								<span><xsl:value-of select="@name"/></span>
							</div>
						</xsl:for-each>
						<div class="textbutton log" title="Show request log" data-command="viewLog">
							<span>log</span>
						</div>
					</div>
					<div class="toolbar right">
						<div class="button toggle {math:iif($layout='double', 'active', '')}"
							title="Toggle the tool frame" data-command="toggleFrame"></div>
					</div>
				</div>
				<div class="toolframe">
					<iframe src="about:blank" frameborder="0" width="100%" height="100%"></iframe>
				</div>
				<div class="toolpreloader"></div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="mod:ViewInspector/mod:config/*" mode="attribute-value">
		<xsl:apply-templates select="node()"/>
	</xsl:template>

	<xsl:template match="mod:ViewInspector/mod:config/*" mode="attribute">
		<xsl:attribute name="{local-name()}">
			<xsl:apply-templates select="." mode="attribute-value"/>
		</xsl:attribute>
	</xsl:template>

</xsl:stylesheet>