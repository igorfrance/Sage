﻿/**
 * Copyright 2012 Igor France
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text.RegularExpressions;

	using Sage.Extensibility;

	/// <summary>
	/// Implements a resolver that can be used with embedded resources.
	/// </summary>
	[UrlResolver(Scheme = EmbeddedResourceResolver.Scheme)]
	internal class EmbeddedResourceResolver : IUrlResolver
	{
		public const string Scheme = "sageresx";

		public EntityResult GetEntity(UrlResolver parent, SageContext context, string uri)
		{
			Stream reader = GetStreamResourceReader(parent, context, uri);
			if (reader == null)
				throw new ArgumentException(string.Format("The uri '{0}' could not be resolved", uri));

			return new EntityResult { Entity = reader };
		}

		protected Stream GetStreamResourceReader(UrlResolver parent, SageContext context, string resourceUri)
		{
			string resourceName = ConvertPath(resourceUri);

			Stream result = null;
			foreach (Assembly a in Application.RelevantAssemblies)
			{
				string resourcePath = new List<string>(a.GetManifestResourceNames())
					.FirstOrDefault(r => r.ToLower().IndexOf(resourceName) != -1);

				if (!string.IsNullOrEmpty(resourcePath))
				{
					result = a.GetManifestResourceStream(resourcePath);
					break;
				}
			}

			return result;
		}

		private string ConvertPath(string path)
		{
			return Regex.Replace(path, @"^\w+://", string.Empty)
				.TrimEnd('/')
				.Replace(Scheme, string.Empty)
				.Replace("://", string.Empty)
				.Replace('/', '.')
				.Replace('\\', '.')
				.ToLower();
		}
	}
}
