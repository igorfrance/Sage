﻿namespace Sage.Configuration
{
	using System;
	using System.Xml;

	/// <summary>
	/// Provides path templates for various system-required files.
	/// </summary>
	public class PathTemplates
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PathTemplates"/> class.
		/// </summary>
		public PathTemplates()
		{
			// initialize with default values.
			this.View = "{assetpath}/views/";
			this.Module = "{assetpath}/modules/";
			this.Extension = "{assetpath}/extensions/";
			this.CategoryConfiguration = "{assetpath}/configuration/Category.config";
			this.ViewConfiguration = "{assetpath}/views/{controller}/{action}.xml";
			this.ViewTemplate = "{assetpath}/views/{controller}/{action}";
			this.DefaultStylesheet = "{assetpath}/views/xslt/default.xsl";
			this.CategoryStylesheet = "{assetpath}/views/xslt/default.xsl";
			this.Dictionary = "{assetpath}/configuration/dictionary/{locale}.xml";
			this.GlobalizedDirectory = "_target/";
			this.GlobalizedDirectoryForNonFileResources = "~/_target/";
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PathTemplates"/> class, using the specified
		/// <paramref name="configElement"/> to parse its values from.
		/// </summary>
		/// <param name="configElement">The configuration element containing the definition of the path templates.</param>
		public PathTemplates(XmlElement configElement)
			: this()
		{
			this.Parse(configElement);
		}

		/// <summary>
		/// Gets the template for constructing paths to views.
		/// </summary>
		public string View { get; private set; }

		/// <summary>
		/// Gets the template for constructing paths to module directories.
		/// </summary>
		public string Module { get; private set; }

		/// <summary>
		/// Gets the template for constructing paths to plugin directories.
		/// </summary>
		public string Extension { get; private set; }

		/// <summary>
		/// Gets the template for constucting paths to view configuration files.
		/// </summary>
		public string ViewConfiguration { get; private set; }

		/// <summary>
		/// Gets the template for constucting paths to view template files.
		/// </summary>
		public string ViewTemplate { get; private set; }

		/// <summary>
		/// Gets the template for constucting paths to language dictionaries.
		/// </summary>
		public string Dictionary { get; private set; }

		/// <summary>
		/// Gets the path template to the default XSLT stylesheet that can be used if a view
		/// doesn't have it's own, specific stylesheet.
		/// </summary>
		public string DefaultStylesheet { get; private set; }

		/// <summary>
		/// Gets the path template to the default XSLT stylesheet specific to a category (in a multi-category project)
		/// that can be used if a view doesn't have it's own, specific stylesheet.
		/// </summary>
		public string CategoryStylesheet { get; private set; }

		/// <summary>
		/// Gets the directory in which the globalized resources are saved.
		/// </summary>
		public string GlobalizedDirectory { get; private set; }

		/// <summary>
		/// Gets the directory in which the globalized non-file resources are saved.
		/// </summary>
		public string GlobalizedDirectoryForNonFileResources { get; private set; }

		/// <summary>
		/// Gets the template for constucting paths to category configuration files.
		/// </summary>
		public string CategoryConfiguration { get; private set; }

		/// <summary>
		/// Parses the specified configuration element and copies any applicable values into the current instance.
		/// </summary>
		/// <param name="configElement">The configuration element to parse.</param>
		public void Parse(XmlElement configElement)
		{
			if (configElement == null)
				throw new ArgumentNullException("configElement");

			XmlNamespaceManager nm = XmlNamespaces.Manager;
			XmlNode testNode;
			string testValue;

			testNode = configElement.SelectSingleNode("p:View", nm);
			if (testNode != null && !string.IsNullOrEmpty(testValue = testNode.InnerText))
				this.View = testValue;

			testNode = configElement.SelectSingleNode("p:Module", nm);
			if (testNode != null && !string.IsNullOrEmpty(testValue = testNode.InnerText))
				this.Module = testValue;

			testNode = configElement.SelectSingleNode("p:Extension", nm);
			if (testNode != null && !string.IsNullOrEmpty(testValue = testNode.InnerText))
				this.Extension = testValue;

			testNode = configElement.SelectSingleNode("p:CategoryConfiguration", nm);
			if (testNode != null && !string.IsNullOrEmpty(testValue = testNode.InnerText))
				this.CategoryConfiguration = testValue;

			testNode = configElement.SelectSingleNode("p:ViewConfiguration", nm);
			if (testNode != null && !string.IsNullOrEmpty(testValue = testNode.InnerText))
				this.ViewConfiguration = testValue;

			testNode = configElement.SelectSingleNode("p:ViewTemplate", nm);
			if (testNode != null && !string.IsNullOrEmpty(testValue = testNode.InnerText))
				this.ViewTemplate = testValue;

			testNode = configElement.SelectSingleNode("p:DefaultStylesheet", nm);
			if (testNode != null && !string.IsNullOrEmpty(testValue = testNode.InnerText))
				this.DefaultStylesheet = testValue;

			testNode = configElement.SelectSingleNode("p:DefaultCategoryStylesheet", nm);
			if (testNode != null && !string.IsNullOrEmpty(testValue = testNode.InnerText))
				this.CategoryStylesheet = testValue;

			testNode = configElement.SelectSingleNode("p:Dictionary", nm);
			if (testNode != null && !string.IsNullOrEmpty(testValue = testNode.InnerText))
				this.Dictionary = testValue;

			testNode = configElement.SelectSingleNode("p:GlobalizedDirectory", nm);
			if (testNode != null && !string.IsNullOrEmpty(testValue = testNode.InnerText))
				this.GlobalizedDirectory = testValue;

			testNode = configElement.SelectSingleNode("p:GlobalizedDirectoryForNonFileResources", nm);
			if (testNode != null && !string.IsNullOrEmpty(testValue = testNode.InnerText))
				this.GlobalizedDirectoryForNonFileResources = testValue;
		}
	}
}
