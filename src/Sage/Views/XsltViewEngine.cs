﻿namespace Sage.Views
{
	using System;
	using System.Web.Mvc;

	using Sage.Configuration;
	using Sage.Controllers;
	using Sage.Views;
	using Sage.Views.Meta;

	/// <summary>
	/// Implements an <see cref="IViewEngine"/> that creates XSLT views.
	/// </summary>
	public class XsltViewEngine : VirtualPathProviderViewEngine
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="XsltViewEngine"/> class.
		/// </summary>
		public XsltViewEngine()
		{
			ViewLocationFormats = new[] { "{1}/{0}" };
			PartialViewLocationFormats = ViewLocationFormats;
		}

		/// <summary>
		/// Creates the partial view.
		/// </summary>
		/// <param name="controllerContext">The controller context for which to create the partial view.</param>
		/// <param name="viewName">The name of the partial view to create.</param>
		/// <returns>The <see cref="IView"/> for the specified <see cref="ControllerContext"/>.</returns>
		protected override IView CreatePartialView(ControllerContext controllerContext, string viewName)
		{
			return CreateView(controllerContext, viewName, null);
		}

		/// <summary>
		/// Creates an <see cref="IView"/> instance, using the specified <paramref name="controllerContext"/> and <paramref name="viewName"/>.
		/// </summary>
		/// <param name="controllerContext">The controller context for which to create the view.</param>
		/// <param name="viewName">The controller/action part of the path to the view to create.</param>
		/// <param name="masterPath">This parameter is ignored.</param>
		/// <returns>The <see cref="IView"/> for the specified <paramref name="controllerContext"/>.</returns>
		protected override IView CreateView(ControllerContext controllerContext, string viewName, string masterPath)
		{
			ViewInfo viewInfo = GetViewInfo(controllerContext, viewName);
			IView result = new XsltView(viewInfo.Processor);

			if (controllerContext.Controller is SageController)
			{
				SageController controller = (SageController) controllerContext.Controller;
				ProjectConfiguration config = controller.Context.Config;
				MetaViewInfo metaViewInfo = config.MetaViews.SelectView(controller.Context);
				if (metaViewInfo != null)
				{
					result = MetaView.Create(metaViewInfo, result);
				}
			}

			return result;
		}

		/// <summary>
		/// Indicates whether a view exists
		/// </summary>
		/// <param name="controllerContext">The controller context for which to find the view.</param>
		/// <param name="viewName">The controller/action part of the view path to check.</param>
		/// <returns><c>true</c> if the specified view exists; otherwise <c>false</c>.</returns>
		protected override bool FileExists(ControllerContext controllerContext, string viewName)
		{
			ViewInfo viewInfo = GetViewInfo(controllerContext, viewName);
			return viewInfo != null && viewInfo.Exists;
		}

		/// <summary>
		/// Gets the info about the view and the corresponding content type for the specified 
		/// <paramref name="controllerContext"/> and <paramref name="viewName"/>.
		/// </summary>
		/// <param name="controllerContext">The controller context.</param>
		/// <param name="viewName">The view name.</param>
		/// <returns>the full viewPath</returns>
		/// <exception cref="ArgumentException">The view name argument is missing the mandatory '/' slash character.</exception>
		private ViewInfo GetViewInfo(ControllerContext controllerContext, string viewName)
		{
			if (controllerContext.Controller is SageController)
			{
				SageController controller = (SageController) controllerContext.Controller;
				return controller.GetViewInfo(viewName.Replace(
					controller.GetType().Name.Replace("Controller", string.Empty) + "/", string.Empty));
			}

			return null;
		}
	}
}
