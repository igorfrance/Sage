﻿/**
 * Open Source Initiative OSI - The MIT License (MIT):Licensing
 * [OSI Approved License]
 * The MIT License (MIT)
 *
 * Copyright (c) 2011 Igor France
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */
namespace Sage.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Reflection;
	using System.Web.Mvc;
	using System.Web.Routing;
	using System.Xml;

	using Kelp;
	using Kelp.Extensions;

	using log4net;
	using Sage.Extensibility;
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

		private static readonly ILog log = LogManager.GetLogger(typeof(SageController).FullName);
		private static readonly List<FilterViewXml> xmlFilters = new List<FilterViewXml>();

		private readonly ControllerMessages messages = new ControllerMessages();
		private readonly IModuleFactory moduleFactory = new SageModuleFactory();
		private readonly Dictionary<string, ViewInfo> viewInfo = new Dictionary<string, ViewInfo>();

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
						FilterViewXml del;
						try
						{
							del = (FilterViewXml) Delegate.CreateDelegate(typeof(FilterViewXml), methodInfo);
						}
						catch
						{
							log.ErrorFormat("The method {0} on type {1} marked with attribute {2} doesn't match the required delegate {3}, and will therefore not be registered as an XML filter method",
								methodInfo.Name, type.FullName, typeof(ViewXmlFilterAttribute).Name, typeof(FilterViewXml).Name);

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
			this.messages = new ControllerMessages();
			this.ViewData["messages"] = this.messages;
			this.IsShared = this.GetType().GetCustomAttributes(typeof(SharedControllerAttribute), true).Count() != 0;
		}

		/// <summary>
		/// Gets the <see cref="SageContext"/> with which this controller runs.
		/// </summary>
		public SageContext Context { get; private set; }

		/// <summary>
		/// Gets or sets the list of files that this document consists of or depends on.
		/// </summary>
		public List<string> Dependencies { get; protected set; }

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
		/// Gets a value indicating whether this is a shared controller.
		/// </summary>
		/// <remarks>
		/// Shared controller is useful with multi-ategory projects. A shared controller's view are opened from
		/// <see cref="PathResolver.SharedViewPath"/> instead of from <see cref="PathResolver.ViewPath"/>. This makes
		/// it possible to have create views that are shared across all gategories in a multi-category project.
		/// </remarks>
		internal bool IsShared { get; private set; }

		/// <summary>
		/// Sets the HTTP status to not found (404) and returns an <see cref="EmptyResult"/>.
		/// </summary>
		/// <returns>Empty result</returns>
		public ActionResult PageNotFound()
		{
			Context.Response.StatusCode = 404;
			return new EmptyResult();
		}

		/// <summary>
		/// Processes the view configuration associated with the specified <paramref name="viewName"/>, 
		/// and returns an <see cref="ActionResult"/>.
		/// </summary>
		/// <param name="viewName">The of the view that should be rendered to response.</param>
		/// <returns>The action result.</returns>
		public ActionResult SageView(string viewName)
		{
			ViewInfo info = new ViewInfo(this, viewName);
			if (info.Exists)
			{
				ViewInput result = this.ProcessView(info);
				return this.View(result.Action, result);
			}

			log.FatalFormat("The specified view name '{0}' doesn't exist.", viewName);
			return this.PageNotFound();
		}

		/// <summary>
		/// Processes the view configuration associated with the specified <paramref name="viewName"/>, 
		/// and returns a <see cref="ViewInput"/> instance that contains the result.
		/// </summary>
		/// <param name="viewName">The name of the view to process.</param>
		/// <returns>
		/// An object that contains the result of processing the view configuration
		/// </returns>
		public virtual ViewInput ProcessView(string viewName)
		{
			return ProcessView(new ViewInfo(this, viewName));
		}

		/// <summary>
		/// Processes the view configuration associated with the specified <paramref name="viewInfo"/>, 
		/// and returns a <see cref="ViewInput"/> instance that contains the result.
		/// </summary>
		/// <param name="viewInfo">The object that contains information about the view.</param>
		/// <returns>
		/// An object that contains the result of processing the view configuration
		/// </returns>
		public virtual ViewInput ProcessView(ViewInfo viewInfo)
		{
			ViewConfiguration config = ViewConfiguration.Create(this, viewInfo);
			return config.ProcessRequest();
		}

		/// <inheritdoc/>
		public virtual IModule CreateModule(XmlElement moduleElement)
		{
			return moduleFactory.CreateModule(moduleElement);
		}

		/// <summary>
		/// Wraps the previously processed view configuration input XML with the standard XML envelope that contains 
		/// information about the current request, and the resources referenced by the modules and libraries in use by the 
		/// the view.
		/// </summary>
		/// <param name="viewContext">The view context that contains the <see cref="ViewInput"/> that resulted from
		/// previously processing the view configuration.</param>
		/// <returns>
		/// The actual XML document that will be used as input for the final XSLT transform.
		/// </returns>
		public virtual XmlDocument PrepareViewXml(ViewContext viewContext)
		{
			ViewInput input = viewContext.ViewData.Model as ViewInput;

			string action = "action";
			if (input != null)
			{
				action = input.ViewConfiguration.Name;
			}
			else if (this.ViewData["Action"] != null)
			{
				action = this.ViewData["Action"].ToString();
			}

			XmlDocument result = new XmlDocument();
			XmlElement viewRoot = result.AppendElement("sage:view", XmlNamespaces.SageNamespace);
			viewRoot.SetAttribute("controller", this.ControllerName);
			viewRoot.SetAttribute("action", action);
			viewRoot.AppendElement(this.Context.ToXml(result));

			XmlElement responseNode = viewRoot.AppendElement("sage:response", XmlNamespaces.SageNamespace);

			if (input != null && input.ConfigNode != null)
			{
				var inputResources = input.Resources;
				if (inputResources.Count != 0)
				{
					XmlElement resourceRoot = responseNode.AppendElement("sage:resources", XmlNamespaces.SageNamespace);

					List<Resource> headResources = inputResources.Where(r => r.Location == Sage.ResourceLocation.Head).ToList();
					List<Resource> bodyResources = inputResources.Where(r => r.Location == Sage.ResourceLocation.Body).ToList();
					List<Resource> dataResources = inputResources.Where(r => r.Location == Sage.ResourceLocation.Data).ToList();

					foreach (Resource resource in dataResources)
					{
						resourceRoot.AppendChild(resource.ToXml(result, this.Context));
					}

					if (headResources.Count != 0)
					{
						XmlNode headNode = resourceRoot.AppendElement("sage:head", XmlNamespaces.SageNamespace);
						foreach (Resource resource in headResources)
							headNode.AppendChild(resource.ToXml(result, this.Context));
					}

					if (bodyResources.Count != 0)
					{
						XmlNode bodyNode = resourceRoot.AppendElement("sage:body", XmlNamespaces.SageNamespace);
						foreach (Resource resource in bodyResources)
							bodyNode.AppendChild(resource.ToXml(result, this.Context));
					}
				}

				responseNode
					.AppendElement("sage:model", XmlNamespaces.SageNamespace)
					.AppendChild(result.ImportNode(input.ConfigNode, true));
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
		/// Gets the view info corresponding to this controller and the specified <paramref name="viewName"/>.
		/// </summary>
		/// <param name="viewName">The name of the view for which to get the info.</param>
		/// <returns>
		/// An object that contains information about the view template and configuration file that correspond to 
		/// this controller and the vew with the specified <paramref name="viewName"/>.
		/// </returns>
		public virtual ViewInfo GetViewInfo(string viewName)
		{
			if (!viewInfo.ContainsKey(viewName))
				viewInfo.Add(viewName,
					new ViewInfo(this, viewName));

			return viewInfo[viewName];
		}

		/// <summary>
		/// Gets the last modification date for the specified <paramref name="viewName"/>.
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
		/// <param name="viewName">The name of the action for which to retrieve the last modification date.</param>
		/// <returns>
		/// The last modification date for the view with the specified <paramref name="viewName"/>.
		/// </returns>
		public virtual DateTime? GetLastModificationDate(string viewName)
		{
			ViewInfo info = GetViewInfo(viewName);
			return info.LastModified;
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
		/// Filters the specified <paramref name="viewXml"/> by invoking all <see cref="FilterViewXml"/> delegates
		/// that are accessible by the project at the time of initialization.
		/// </summary>
		/// <param name="viewContext">The view context under which this code is executed.</param>
		/// <param name="viewXml">The XML document to filter.</param>
		/// <returns>
		/// The filtered version of the specified <paramref name="viewXml"/>.
		/// </returns>
		protected virtual XmlDocument FilterViewXml(ViewContext viewContext, XmlDocument viewXml)
		{
			foreach (FilterViewXml filter in xmlFilters)
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
		protected void AddMessage(MessageType type, string messageText, params string[] formatValues)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(messageText));

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
		protected void AddMessagePhrase(MessageType type, string phraseId, params string[] formatValues)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(phraseId));

			var phrase = this.Context.Resources.GetPhrase(phraseId);
			var text = string.Format(phrase, formatValues);
			var message = new ControllerMessage { Type = type, Text = text };
			this.messages.Add(message);
		}

		/// <summary>
		/// Initializes the controller with a new <see cref="SageContext"/> instance.
		/// </summary>
		/// <param name="requestContext">The request context.</param>
		protected override void Initialize(RequestContext requestContext)
		{
			base.Initialize(requestContext);
			this.Context = new SageContext(this.ControllerContext);
		}
	}
}
