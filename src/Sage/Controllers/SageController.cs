﻿namespace Sage.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Linq;
	using System.Reflection;
	using System.Web.Mvc;
	using System.Web.Routing;
	using System.Xml;

	using Kelp;
	using Kelp.Core.Extensions;
	using log4net;

	using Sage.Modules;
	using Sage.ResourceManagement;
	using Sage.Views;

	using XmlNamespaces = Sage.XmlNamespaces;

	/// <summary>
	/// Provides a base class for all controllers within the application.
	/// </summary>
	public abstract class SageController : Controller, IModuleFactory
	{
		internal const string ParamNameMetaView = "view";
		internal const string DefaultController = "home";
		internal const string DefaultAction = "index";

		protected Dictionary<string, ViewInfo> viewInfo = new Dictionary<string, ViewInfo>();
		private static readonly ILog log = LogManager.GetLogger(typeof(SageController).FullName);
		private static readonly List<ViewXmlFilter> xmlFilters = new List<ViewXmlFilter>();
		private readonly ControllerMessages messages = new ControllerMessages();
		private readonly IModuleFactory moduleFactory = new SageModuleFactory();

		public delegate XmlDocument ViewXmlFilter(SageController controller, ViewContext viewContext, XmlDocument viewXml);

		static SageController()
		{
			foreach (Assembly a in Application.RelevantAssemblies)
			{
				var types = from t in a.GetTypes()
							where t.IsClass && !t.IsAbstract
							select t;

				foreach (Type type in types)
				{
					foreach (MethodInfo methodInfo in type.GetMethods().Where(m => m.IsStatic && m.GetCustomAttributes(typeof(ViewXmlFilterAttribute), false).Count() != 0))
					{
						ViewXmlFilter del;
						try
						{
							del = (ViewXmlFilter) Delegate.CreateDelegate(typeof(ViewXmlFilter), methodInfo);
						}
						catch
						{
							log.ErrorFormat("The method {0} on type {1} marked with attribute {2} doesn't match the required delegate {3}, and will therefore not be registered as an XML filter method",
								methodInfo.Name, type.FullName, typeof(ViewXmlFilterAttribute).Name, typeof(ViewXmlFilter).Name);

							continue;
						}

						xmlFilters.Add(del);
					}
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SageController"/> class.
		/// </summary>
		protected SageController()
		{
			this.MetaData = new NameValueCollection();

			this.messages = new ControllerMessages();
			this.ViewData["messages"] = this.messages;
			this.IsShared = this.GetType().GetCustomAttributes(typeof(SharedControllerAttribute), true).Count() != 0;
		}

		/// <summary>
		/// Gets the <see cref="SageContext"/> current to this controller.
		/// </summary>
		public SageContext Context
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the list of files that this document consists of or depends on.
		/// </summary>
		public List<string> Dependencies
		{
			get;
			protected set;
		}

		/// <summary>
		/// Gets the name of this controller.
		/// </summary>
		public virtual string ControllerName
		{
			get
			{
				return this.GetType().Name.Replace("Controller", string.Empty).ToLower();
			}
		}

		/// <summary>
		/// Provides a dictionary for meta information that can be passed between the controller and the views.
		/// </summary>
		internal NameValueCollection MetaData
		{
			get;
			private set;
		}

		/// <summary>
		/// Indicates whether this instance is shared.
		/// </summary>
		internal bool IsShared
		{
			get;
			private set;
		}

		/// <summary>
		/// Sets the HttpStatus to Not Found (404) and returns an <see cref="EmptyResult"/>.
		/// </summary>
		public ActionResult PageNotFound()
		{
			Context.Response.StatusCode = 404;
			return new EmptyResult();
		}

		public virtual ViewInput ProcessView(ViewInfo viewInfo)
		{
			ViewConfiguration config = ViewConfiguration.Create(this, viewInfo);
			return config.ProcessRequest();
		}

		public virtual ViewInput ProcessView(string viewName)
		{
			return ProcessView(new ViewInfo(this, viewName));
		}

		public virtual IModule CreateModule(XmlElement moduleElement)
		{
			return moduleFactory.CreateModule(moduleElement);
		}

		public virtual XmlDocument GetViewXml(ViewContext viewContext)
		{
			ViewInput input = viewContext.ViewData.Model as ViewInput;

			string action = "action";
			if (input != null)
				action = input.ViewName;
			else if (this.ViewData["Action"] != null)
				action = this.ViewData["Action"].ToString();

			XmlDocument result = new XmlDocument();
			XmlElement viewRoot = result.AppendElement("sage:view", XmlNamespaces.SageNamespace);
			viewRoot.SetAttribute("controller", this.ControllerName);
			viewRoot.SetAttribute("action", action);

			XmlElement requestNode = viewRoot.AppendElement(this.Context.ToXml(result));
			XmlElement responseNode = viewRoot.AppendElement("sage:response", XmlNamespaces.SageNamespace);

			if (input != null && input.ConfigNode != null)
			{
				responseNode
					.AppendElement("sage:model", XmlNamespaces.SageNamespace)
					.AppendChild(result.ImportNode(input.ConfigNode, true));

				if ((input.Resources.Count + input.Libraries.Count) != 0)
				{
					XmlElement resourceRoot = responseNode.AppendElement("sage:resources", XmlNamespaces.SageNamespace);

					foreach (ModuleResource resource in input.Resources)
						resourceRoot.AppendChild(resource.ToXml(result, this.Context));

					foreach (string libraryName in input.Libraries)
					{
						ScriptLibraryInfo info;
						if (this.Context.Config.ScriptLibraries.TryGetValue(libraryName, out info))
						{
							resourceRoot.AppendChild(info.ToXml(result, this.Context));
						}
						else
						{
							log.ErrorFormat("The referenced script library '{0}' is undefined", libraryName);
						}
					}
				}
			}

			foreach (var key in viewContext.ViewData.Keys)
			{
				object value = viewContext.ViewData[key];
				if (value == null)
					continue;

				if (value is XmlNode)
				{
					XmlNode valueNode = (XmlNode) value;
					if (valueNode.NodeType == XmlNodeType.Document)
					{
						XmlDocument doc = (XmlDocument) valueNode;
						XmlNode importedNode = result.ImportNode(((XmlDocument) valueNode).DocumentElement, true);
						if (doc.DocumentElement != null) responseNode.AppendChild(importedNode);
					}
					else
						responseNode.AppendChild(result.ImportNode(valueNode, true));
				}
				else if (value is IXmlConvertible)
				{
					XmlElement valueElement = ((IXmlConvertible) value).ToXml(result);
					if (valueElement != null)
						responseNode.AppendChild(valueElement);
				}
				else
				{
					XmlElement elem = (XmlElement) responseNode.AppendChild(result.CreateElement("sage:value", XmlNamespaces.SageNamespace));
					elem.SetAttribute("id", key);
					elem.InnerText = value.ToString();
				}
			}

			return FilterViewXml(viewContext, result);
		}

		/// <summary>
		/// Provides a hook to initialize the controller from a unit test
		/// </summary>
		/// <param name="requestContext">The request context.</param>
		internal void InitializeForTesting(RequestContext requestContext)
		{
			this.Initialize(requestContext);
		}

		/// <summary>
		/// Gets the path info about the view template and configuration file corresponding to this controller 
		/// and the specified <paramref name="viewName"/>.
		/// </summary>
		/// <param name="viewName">Name of the action for which to get the path info.</param>
		/// <returns>The path info about the view template and configuration file corresponding to this controller 
		/// and the specified <paramref name="viewName"/></returns>
		internal virtual ViewInfo GetViewInfo(string viewName)
		{
			if (!viewInfo.ContainsKey(viewName))
				viewInfo.Add(viewName,
					new ViewInfo(this, viewName));

			return viewInfo[viewName];
		}

		/// <summary>
		/// Gets the last modification date for the specified <paramref name="actionName"/>.
		/// </summary>
		/// <remarks>
		/// Each action can and should be cached by the browsers. When subsequent requests come in, browsers will
		/// send the last modification date that they received the last time they got that file, in order for the
		/// server to figure out whether to send a new version. With sage controllers views, the last modification
		/// date is actually the latest modification date of possibly a whole series of files. Those files could be
		/// the XSLT stylesheet itself or any one of its includes, or the XML configuration file or any one of its
		/// includes. Therefore it is necessary to have this extra piece of logic to effectively determine what that
		/// latest modification date is.
		/// </remarks>
		/// <param name="actionName">The name of the action for which to retrieve the last modification date.</param>
		/// <returns>The last modified date for the specified <paramref name="actionName"/>.</returns>
		internal virtual DateTime? GetLastModificationDate(string actionName)
		{
			ViewInfo info = GetViewInfo(actionName);
			return info.LastModified;
		}

		protected virtual XmlDocument FilterViewXml(ViewContext viewContext, XmlDocument viewXml)
		{
			foreach (ViewXmlFilter filter in xmlFilters)
			{
				viewXml = filter.Invoke(this, viewContext, viewXml);
			}

			return viewXml;
		}

		/// <summary>
		/// Adds a message to this controller's message collection
		/// </summary>
		/// <param name="type">The type of the message to add.</param>
		/// <param name="messageText">The message to display.</param>
		/// <param name="formatValues">Optional format values to use for formatting the message text.</param>
		/// <exception cref="ArgumentNullException">
		/// 	<paramref name="messageText"/> is <c>null</c> or empty.
		/// </exception>
		protected void AddMessage(MessageType type, string messageText, params string[] formatValues)
		{
			if (string.IsNullOrEmpty(messageText))
				throw new ArgumentNullException("messageText");

			var text = string.Format(messageText, formatValues);
			var message = new ControllerMessage { Type = type, Text = text };
			this.messages.Add(message);
		}

		/// <summary>
		/// Adds a message to this controller's message collection, using the specified <paramref name="phraseId"/> to get the message
		/// text.
		/// </summary>
		/// <param name="type">The type of the message to add.</param>
		/// <param name="phraseId">The id of the phrase that contains the text associated with this message.</param>
		/// <param name="formatValues">Optional format values to use for formatting the phrase text.</param>
		/// <exception cref="ArgumentNullException">
		/// 	<paramref name="phraseId"/> is <c>null</c>.
		/// </exception>
		protected void AddMessagePhrase(MessageType type, string phraseId, params string[] formatValues)
		{
			if (string.IsNullOrEmpty(phraseId))
				throw new ArgumentNullException("phraseId");

			var phrase = this.Context.Resources.GetPhrase(phraseId);
			var text = string.Format(phrase, formatValues);
			var message = new ControllerMessage { Type = type, Text = text };
			this.messages.Add(message);
		}

		/// <summary>
		/// Initializes the controller.
		/// </summary>
		/// <param name="requestContext">The request context.</param>
		protected override void Initialize(RequestContext requestContext)
		{
			base.Initialize(requestContext);
			this.Context = new SageContext(requestContext.HttpContext);
		}
	}
}
