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
namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Xml;

	using Kelp.Core.Extensions;
	using Kelp.Extensions;

	using Sage.Configuration;

	/// <summary>
	/// Provides summary information about globalizing an XML resource.
	/// </summary>
	/// <remarks>
	/// The summary contains information about the files that the resource consists of (via <c>XInclude</c>), the
	/// phrases that the resource uses and the phrase sources.
	/// </remarks>
	public class GlobalizationSummary
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GlobalizationSummary"/> class, using the specified file path and category info.
		/// </summary>
		/// <param name="resource">The xml resource that was globalized.</param>
		/// <param name="categoryInfo">The <see cref="CategoryInfo"/> of the category the resource belongs to.</param>
		internal GlobalizationSummary(XmlResource resource, CategoryInfo categoryInfo)
		{
			this.Resource = resource;
			this.PhraseSummary = new Dictionary<string, XmlDocument>();
			this.Dependencies = new Dictionary<string, IEnumerable<string>>();
		}

		/// <summary>
		/// Gets the path information about the globalized resource.
		/// </summary>
		internal XmlResource Resource { get; private set; }

		/// <summary>
		/// Gets or sets the duration (in milliseconds) it took to globalize a resource.
		/// </summary>
		internal long Duration { get; set; }

		internal Dictionary<string, IEnumerable<string>> Dependencies { get; private set; }

		/// <summary>
		/// Gets dictionary of the phrases that were used in translating the globalized resource.
		/// </summary>
		/// <remarks>
		/// The keys are the locales to which the resource got translated, and the corresponding values
		/// are the summary XML document generated by the XSLT processor that was used in translation.
		/// </remarks>
		internal Dictionary<string, XmlDocument> PhraseSummary { get; private set; }

        /// <summary>
        /// Gets the constituent files.
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <returns></returns>
		public IEnumerable<string> GetConstituentFiles(string locale)
		{
			return this.Dependencies[locale];
		}

		/// <summary>
		/// Adds a phrase summary for the specified <paramref name="locale"/>.
		/// </summary>
		/// <param name="locale">The locale for which the summary is being added.</param>
		/// <param name="summary">The summary XML document generated by the XSLT processor that was used in translation..</param>
		internal void AddPhraseSummary(string locale, XmlDocument summary)
		{
			this.PhraseSummary.Add(locale, summary);
		}

		internal void AddDependencies(string locale, IEnumerable<string> dependencies)
		{
			this.Dependencies.Add(locale, dependencies);
		}

		/// <summary>
		/// Generates an <see cref="XmlElement"/> that contains the information from this object.
		/// </summary>
		/// <param name="owner">The owner document to use to create nodes.</param>
		/// <returns>An <see cref="XmlElement"/> that contains the information from this object.</returns>
		internal XmlElement ToXml(XmlDocument owner)
		{
			XmlElement self = owner.CreateElement("resource");
			self.SetAttribute("path", this.Resource.Name.FilePath);
			self.SetAttribute("folder", this.Resource.Name.FolderName);
			self.SetAttribute("name", this.Resource.Name.FileName);
			self.SetAttribute("time", this.Duration.ToString());

			XmlElement phraseRoot = self.AppendElement(owner.CreateElement("phrases"));
			XmlElement fileRoot = self.AppendElement(owner.CreateElement("files"));

			foreach (string locale in this.PhraseSummary.Keys)
			{
				XmlDocument summary = this.PhraseSummary[locale];
				foreach (XmlElement phraseSource in summary.SelectNodes("//xhtml:phrase", XmlNamespaces.Manager))
				{
					string phraseID = phraseSource.GetAttribute("ref");
					XmlElement phraseNode = phraseRoot.SelectSingleElement(string.Format("phrase[@id='{0}']", phraseID));
					if (phraseNode == null)
					{
						phraseNode = phraseRoot.AppendElement(owner.CreateElement("phrase"));
						phraseNode.SetAttribute("id", phraseID);
					}

					phraseNode.SetAttribute("locale-" + locale, phraseSource.GetAttribute("source"));
				}
			}

			foreach (string locale in this.PhraseSummary.Keys)
			{
				XmlElement localeNode = fileRoot.AppendElement(owner.CreateElement("locale"));
				localeNode.SetAttribute("name", locale);

				if (this.Dependencies.ContainsKey(locale))
				{
					foreach (string path in this.Dependencies[locale])
					{
						XmlElement fileElem = localeNode.AppendElement(owner.CreateElement("file"));
						fileElem.SetAttribute("path", path);
					}
				}
			}

			return self;
		}

		/// <summary>
		/// Saves the summary as a new <see cref="XmlDocument"/> to the same location in which the resource got globalized.
		/// </summary>
		internal void Save()
		{
			XmlDocument summaryDoc = new XmlDocument();
			summaryDoc.AppendChild(this.ToXml(summaryDoc));

			string summaryPath = Path.Combine(this.Resource.TargetDirectory, this.Resource.Name.ToLocale("summary"));			
			summaryDoc.Save(summaryPath);
		}
	}
}
