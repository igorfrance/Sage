﻿namespace Sage.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Web;
	using System.Xml;
	using System.Xml.Schema;

	using Kelp.Core.Extensions;

	using Sage.Extensibility;
	using Sage.Modules;

	using log4net;
	using Sage.ResourceManagement;

	/// <summary>
	/// Implements the configuration container for configurable properties of this project.
	/// </summary>
	public class ProjectConfiguration
	{
		private const string RewriteOnFile = "Rewrite.ON";
		private const string SystemConfigName = "System.config";
		private const string ProjectConfigName = "Project.config";
		private const string ConfigSchemaPath = "sageresx://sage/resources/schemas/projectconfiguration.xsd";

		private static readonly ILog log = LogManager.GetLogger(typeof(ProjectConfiguration).FullName);

		private static volatile ProjectConfiguration systemConfig;
		private static volatile ProjectConfiguration projectConfig;

		private ProjectConfiguration()
		{
			this.Modules = new List<ModuleConfiguration>();
			this.Categories = new Dictionary<string, CategoryInfo>();
			this.Locales = new Dictionary<string, LocaleInfo>();
			this.MetaViews = new MetaViewDictionary();
			this.DeveloperIps = new List<IpAddress>();
			this.AssetPrefixes = new Dictionary<string, string>();
			this.Links = new LinkConfiguration();
			this.Routing = new RoutingConfiguration();
			this.PathTemplates = new PathTemplates();
			this.ScriptLibraries = new Dictionary<string, ScriptLibraryInfo>();
			this.Deliverables = new List<string>();

			this.SharedCategory = "shared";
			this.DefaultLocale = "default";
			this.DefaultCategory = "default";
			this.AreResourcesPreGenerated = false;
			this.MergeResources = false;
		}

		/// <summary>
		/// Gets the current global <see cref="ProjectConfiguration"/>.
		/// </summary>
		public static ProjectConfiguration Current
		{
			get
			{
				return projectConfig;
			}
		}

		/// <summary>
		/// Gets the physical path of the currently executing assembly.
		/// </summary>
		public static string AssemblyPath
		{
			get
			{
				return Path.GetDirectoryName(
					Assembly.GetExecutingAssembly()
						.CodeBase
						.Replace("file:///", string.Empty)
						.Replace("/", "\\"));
			}
		}

		public XmlElement ConfigurationElement { get; private set; }

		public string Name { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the resources have been pre-generated.
		/// </summary>
		/// <value>
		/// <c>true</c> if the resources have been pre-generated; otherwise, <c>false</c>.
		/// </value>
		public bool AreResourcesPreGenerated
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the default category to fall back to if not specific category has been specified.
		/// </summary>
		public string DefaultCategory
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the default locale to fall back to if not specific locale has been specified.
		/// </summary>
		public string DefaultLocale
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a value indicating whether the resources (script and styles) should be merged.
		/// </summary>
		/// <value>
		/// <c>true</c> if the resources should be merged; otherwise, <c>false</c>.
		/// </value>
		public bool MergeResources
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a value indicating whether the current project runs in multi-category mode.
		/// </summary>
		/// <value>
		/// <c>true</c> if the current project runs in multi-category mode; otherwise, <c>false</c>.
		/// </value>
		public bool MultiCategory
		{
			get;
			private set;
		}

		/// <summary>
		/// Path templates for various system-required files.
		/// </summary>
		public PathTemplates PathTemplates
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the routing configuration for the current project.
		/// </summary>
		public RoutingConfiguration Routing
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the resource path configuration variable; this is the base path for all resources.
		/// </summary>
		public string AssetPath
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the name of the shared category.
		/// </summary>
		public string SharedCategory
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets the base pattern for constructing URLs.
		/// </summary>
		public string UrlRewritePrefix
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether URL rewriting is on.
		/// </summary>
		/// <value><c>true</c> if URL rewriting is on; otherwise, <c>false</c>.</value>
		public bool UrlRewritingOn
		{
			get
			{
				if (PathResolver.ApplicationPhysicalPath != null)
					return File.Exists(Path.Combine(PathResolver.ApplicationPhysicalPath, RewriteOnFile));

				return false;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating the current project has been configured for debugging.
		/// </summary>
		/// <value><c>true</c> if debugging is on; otherwise, <c>false</c>.</value>
		public bool IsDebugMode
		{
			get;
			private set;
		}

		public Dictionary<string, ScriptLibraryInfo> ScriptLibraries
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the dictionary of asset resource mappings; where keys are asset aliases, and values are the actual paths to these assets.
		/// </summary>
		public Dictionary<string, string> AssetPrefixes
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the list of locales available within categories.
		/// </summary>
		/// <remarks>
		/// The keys in this dictionaries are the keys of the categories ('running', 'football'...) and the values are 
		/// comma-separated lists of locale identifiers.
		/// </remarks>
		/// <see cref="CategoryInfo"/>
		public Dictionary<string, CategoryInfo> Categories
		{
			get;
			private set;
		}

		public List<string> Deliverables { get; private set; }

		/// <summary>
		/// Gets the list of IP addresses or address ranges to be considered as developers.
		/// </summary>
		public List<IpAddress> DeveloperIps
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the <see cref="NameValueCollection"/> of name/pattern link values as parsed from the configuration node.
		/// </summary>
		public LinkConfiguration Links
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the dictionary of defined locales.
		/// </summary>
		/// <see cref="LocaleInfo"/>
		public Dictionary<string, LocaleInfo> Locales
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the collection of global, shared meta views as defined in the configuration file.
		/// </summary>
		/// <see cref="MetaViewInfo"/>
		public MetaViewDictionary MetaViews
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the dictionary that defines how module names map to module classes that implement them.
		/// </summary>
		public IList<ModuleConfiguration> Modules
		{
			get;
			private set;
		}

		public static ProjectConfiguration Create(string configPath)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(configPath));

			XmlDocument document = ResourceManager.LoadXmlDocument(configPath);
			return Create(document);
		}

		public static ProjectConfiguration Create(XmlDocument configDoc)
		{
			Contract.Requires<ArgumentNullException>(configDoc != null);

			var result = new ProjectConfiguration();
			result.Parse(configDoc);

			return result;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="clientIpAddress"/> is configured as a developer IP address.
		/// </summary>
		/// <param name="clientIpAddress">The client IP address to test.</param>
		/// <returns>
		/// <c>true</c> if the specified <paramref name="clientIpAddress"/> is configured as a developer IP address; otherwise, <c>false</c>.
		/// </returns>
		public bool IsDeveloperIp(string clientIpAddress)
		{
			return this.DeveloperIps.Where(a => a.Matches(clientIpAddress)).Count() != 0;
		}

		/// <summary>
		/// Gets a value indicating whether the specified <paramref name="locale"/> uses a latin character subset.
		/// </summary>
		/// <param name="locale">The name of the locale to verify</param>
		/// <returns>
		/// <c>true</c> if the specified locale uses a latin character subset, otherwise <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// If the argument <paramref name="locale"/> is empty or a <c>null</c>.
		/// </exception>
		public bool IsLatinLocale(string locale)
		{
			if (locale == null)
				throw new ArgumentNullException("locale");

			bool result = false;

			LocaleInfo info;
			if (this.Locales.TryGetValue(locale, out info))
				result = info.IsLatinCharset;

			return result;
		}

		internal static void Initialize()
		{
			string projectConfigPath = Path.Combine(AssemblyPath, ProjectConfigName);
			string systemConfigPath = Path.Combine(AssemblyPath, SystemConfigName);

			if (!File.Exists(projectConfigPath) && !File.Exists(systemConfigPath))
				throw new SageHelpException(ProblemType.MissingConfigurationFile);
		
			systemConfig = new ProjectConfiguration();
			if (File.Exists(systemConfigPath))
				systemConfig.Parse(systemConfigPath);

			projectConfig = new ProjectConfiguration();
			if (File.Exists(systemConfigPath))
				projectConfig.Parse(systemConfigPath);

			if (File.Exists(projectConfigPath))
				projectConfig.Parse(projectConfigPath);

			if (projectConfig.Locales.Count == 0)
				throw new SageHelpException(ProblemType.ConfigurationMissingLocales);

			if (projectConfig.MultiCategory && projectConfig.Categories.Count == 0)
				throw new SageHelpException(ProblemType.ConfigurationMissingCategories);
		}

		internal void Parse(string configPath)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(configPath));

			XmlDocument document = ResourceManager.LoadXmlDocument(configPath);
			Parse(document);
		}

		/// <summary>
		/// Parses the specified configuration document.
		/// </summary>
		/// <param name="configDoc">The XML configuration document to parse.</param>
		internal void Parse(XmlDocument configDoc)
		{
			Contract.Requires<ArgumentNullException>(configDoc != null);

			ResourceManager.ValidateDocument(configDoc, ConfigSchemaPath);
			XmlNamespaceManager nm = XmlNamespaces.Manager;

			XmlElement configNode = configDoc.SelectSingleElement("/p:configuration", nm);

			this.ConfigurationElement = configNode.SelectSingleElement("p:project", nm);
			this.Categories = new Dictionary<string, CategoryInfo>();
			this.Locales = new Dictionary<string, LocaleInfo>();

			string nodeValue;
			XmlElement projectNode = this.ConfigurationElement;
			XmlElement routingNode = projectNode.SelectSingleElement("p:routing", nm);
			XmlElement linksNode = projectNode.SelectSingleElement("p:links", nm);
			XmlElement pathsNode = projectNode.SelectSingleElement("p:paths", nm);

			nodeValue = projectNode.GetAttribute("name");
			if (!string.IsNullOrEmpty(nodeValue))
				this.Name = nodeValue;

			nodeValue = projectNode.GetAttribute("sharedCategory");
			if (!string.IsNullOrEmpty(nodeValue))
				this.SharedCategory = nodeValue;

			nodeValue = projectNode.GetAttribute("defaultLocale");
			if (!string.IsNullOrEmpty(nodeValue))
				this.DefaultLocale = nodeValue;

			nodeValue = projectNode.GetAttribute("defaultCategory");
			if (!string.IsNullOrEmpty(nodeValue))
				this.DefaultCategory = nodeValue;

			nodeValue = projectNode.GetAttribute("resourcesPregenerated");
			if (!string.IsNullOrEmpty(nodeValue))
				this.AreResourcesPreGenerated = nodeValue.ContainsAnyOf("yes", "1", "true");

			nodeValue = projectNode.GetAttribute("multiCategory");
			if (!string.IsNullOrEmpty(nodeValue))
				this.MultiCategory = nodeValue.ContainsAnyOf("yes", "1", "true");

			nodeValue = projectNode.GetAttribute("mergeResources");
			if (!string.IsNullOrEmpty(nodeValue))
				this.MergeResources = nodeValue.ContainsAnyOf("yes", "1", "true");

			nodeValue = projectNode.GetAttribute("debugMode");
			if (!string.IsNullOrEmpty(nodeValue))
				this.IsDebugMode = nodeValue.ContainsAnyOf("yes", "1", "true");

			if (pathsNode != null)
			{
				this.PathTemplates.Parse(pathsNode);

				XmlNode node = pathsNode.SelectSingleNode("p:AssetPath", nm);
				if (node != null)
				{
					nodeValue = node.InnerText;
					if (!string.IsNullOrEmpty(nodeValue))
						this.AssetPath = nodeValue;
				}
			}

			if (routingNode != null)
				this.Routing.ParseConfiguration(routingNode);

			if (linksNode != null)
			{
				this.Links.ParseConfiguration(linksNode);
				nodeValue = linksNode.GetAttribute("rewritePrefix");
				if (!string.IsNullOrEmpty(nodeValue))
					this.UrlRewritePrefix = nodeValue;
			}

			foreach (XmlElement moduleNode in projectNode.SelectNodes("p:modules/p:module", nm))
			{
				ModuleConfiguration moduleConfig = new ModuleConfiguration(moduleNode);
				this.Modules.Add(moduleConfig);
			}

			foreach (XmlElement libraryNode in projectNode.SelectNodes("p:scripts/p:library", nm))
			{
				ScriptLibraryInfo info = new ScriptLibraryInfo(libraryNode);
				this.ScriptLibraries.Add(info.Name, info);
			}

			foreach (XmlElement locale in projectNode.SelectNodes("p:globalization/p:locale", nm))
			{
				var name = locale.GetAttribute("name");
				var info = new LocaleInfo(locale);
				if (this.Locales.ContainsKey(name))
					this.Locales[name] = info;
				else
					this.Locales.Add(name, info);
			}

			if (this.MultiCategory)
			{
				foreach (XmlElement category in projectNode.SelectNodes("p:categories/p:category", nm))
				{
					var info = new CategoryInfo(category, this.Locales);
					this.Categories.Add(info.Name, info);
					if (this.Categories.ContainsKey(info.Name))
						this.Categories[info.Name] = info;
					else
						this.Categories.Add(info.Name, info);
				}
			}
			else
			{
				this.Categories.Add(this.DefaultCategory, new CategoryInfo(this.DefaultCategory, this.Locales));
			}

			foreach (XmlElement viewNode in projectNode.SelectNodes("p:metaViews/p:view", nm))
			{
				var name = viewNode.GetAttribute("name");
				var info = new MetaViewInfo(viewNode);
				if (this.MetaViews.ContainsKey(name))
					this.MetaViews[name] = info;
				else
					this.MetaViews.Add(name, info);
			}

			foreach (XmlElement elem in projectNode.SelectNodes("p:developers/p:ip", nm))
			{
				IpAddress address = new IpAddress(elem);
				this.DeveloperIps.Add(address);
			}

			foreach (XmlElement elem in projectNode.SelectNodes("p:assets/p:prefix", nm))
			{
				var key = elem.GetAttribute("key");
				var value = elem.GetAttribute("value");
				if (this.AssetPrefixes.ContainsKey(key))
					this.AssetPrefixes[key] = value;
				else
					this.AssetPrefixes.Add(key, value);
			}

			foreach (XmlElement binaryNode in projectNode.SelectNodes("p:deliverables/p:binary", nm))
			{
				this.Deliverables.Add(binaryNode.InnerText);
			}
		}
	}
}