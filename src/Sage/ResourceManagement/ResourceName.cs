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
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Text.RegularExpressions;

	using Sage.Configuration;

	/// <summary>
	/// Represents the path of the resource that may be globalized.
	/// </summary>
	public class ResourceName
	{
		internal static readonly Regex fixSeparators = new Regex(@"[\\/]");
		internal static readonly Regex fixScheme = new Regex(@":/(?!/)");
		private static readonly Regex suffixTest = new Regex("^[^-]+-(.*)$", RegexOptions.Compiled);

		private readonly string separator;

		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceName"/> class.
		/// </summary>
		/// <param name="filePath">The full path of a file.</param>
		/// <param name="category">The resource's category.</param>
		public ResourceName(string filePath, CategoryInfo category)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(filePath));
			Contract.Requires<ArgumentNullException>(category != null);

			this.separator = filePath.Contains("/") ? "/" : Path.DirectorySeparatorChar.ToString();

			this.FilePath = filePath;
			this.FileDirectory = fixScheme.Replace((Path.GetDirectoryName(filePath) ?? string.Empty).Replace("\\", this.separator), "://");
			this.FileName = Path.GetFileNameWithoutExtension(filePath);
			this.Extension = Path.GetExtension(filePath);

			string locale = this.DiscoverLocale(this.FileName, category);
			if (locale != null && locale != this.FileName)
			{
				this.FileName = Regex.Replace(this.FileName, string.Concat("-", locale, "$"), string.Empty, RegexOptions.IgnoreCase);
				this.Locale = locale;
			}
		}

		/// <summary>
		/// Gets the full directory name of this resource path ('g:\files' from 'g:\files\list-de.xml').
		/// </summary>
		public string FileDirectory { get; private set; }

		/// <summary>
		/// Gets the directory file name of this resource path.
		/// </summary>
		/// <value>
		/// 'files' for the resource name 'g:\files\list-de.xml'
		/// </value>
		public string FolderName
		{
			get
			{
				return Path.GetFileName(FileDirectory);
			}
		}

		/// <summary>
		/// Gets the file name part (without the locale) of this resource name.
		/// </summary>
		/// <value>
		/// <c>list</c> for the resource name <c>g:\files\list-de.xml</c>.
		/// </value>
		public string FileName { get; private set; }

		/// <summary>
		/// Gets the non-localized version of this resource name.
		/// </summary>
		public string NonLocalizedName
		{
			get
			{
				return string.Concat(this.FileName, this.Extension);
			}
		}

		/// <summary>
		/// Gets the non-localized version of the full path associated with this resource name.
		/// </summary>
		public string NonLocalizedPath
		{
			get
			{
				return string.Concat(this.FileDirectory, this.separator, this.FileName, this.Extension);
			}
		}

		/// <summary>
		/// Gets the abstract signature name of this resource name.
		/// </summary>
		/// <value>
		/// <c>g:\files\list</c> for the resource name <c>g:\files\list-de.xml</c>.
		/// </value>
		public string Signature
		{
			get
			{
				return Path.Combine(this.FileDirectory, this.FileName);
			}
		}

		/// <summary>
		/// Gets the full path string of this resource name.
		/// </summary>
		/// <value>
		/// For example, <c>g:\files\list-de.xml</c>
		/// </value>
		public string FilePath { get; private set; }

		/// <summary>
		/// Gets the file extension part of this resource name.
		/// </summary>
		/// <value>
		/// <c>.xml</c> for the resource name <c>g:\files\list-de.xml</c>
		/// </value>
		public string Extension { get; private set; }

		/// <summary>
		/// Gets the locale part of this resource name.
		/// </summary>
		/// <value>
		/// <c>de</c> for the resource name <c>g:\files\list-de.xml</c>
		/// </value>
		public string Locale { get; private set; }

		/// <summary>
		/// Gets the file name locale pattern of this resource name.
		/// </summary>
		/// <value>
		/// <c>list-{locale}.xml</c> for the resource name <c>g:\files\list-de.xml</c>
		/// </value>
		public string LocalePattern
		{
			get
			{
				return string.Concat(this.FileName, "-{locale}", this.Extension);
			}
		}

		/// <summary>
		/// Gets the file path locale pattern of this resource name.
		/// </summary>
		/// <value>
		/// <c>g:\files\list-{locale}.xml</c> for the resource name <c>g:\files\list-de.xml</c>
		/// </value>
		public string LocalePathPattern
		{
			get
			{
				return Path.Combine(this.FileDirectory, LocalePattern).Replace("\\", this.separator);
			}
		}

		/// <summary>
		/// Returns the locale-specific version of this resource name.
		/// </summary>
		/// <param name="locale">The locale to use (e.g. <c>fr</c>).</param>
		/// <returns>The version of this resource name for the specified locale.</returns>
		public string ToLocale(string locale)
		{
			return string.Concat(this.FileName, "-", locale, this.Extension);
		}

		/// <summary>
		/// Returns the locale-specific version of this resource path.
		/// </summary>
		/// <param name="locale">The locale to use (e.g. <c>fr</c>).</param>
		/// <returns>The version of this resource name for the specified locale.</returns>
		public string ToLocalePath(string locale)
		{
			return Path.Combine(this.FileDirectory, this.ToLocale(locale)).Replace("\\", this.separator);
		}

		private string DiscoverLocale(string fileName, CategoryInfo category)
		{
			string result = null;
			if (this.FileName.EndsWith("-{locale}"))
				result = "{locale}";
			else
			{
				ProjectConfiguration config = ProjectConfiguration.Current;

				Match m;
				while ((m = suffixTest.Match(fileName)).Success)
				{
					fileName = m.Groups[1].Value;
					foreach (string name in category.Locales)
					{
						LocaleInfo locale = config.Locales[name];
						foreach (string suffix in locale.ResourceNames)
						{
							if (fileName.Equals(suffix, StringComparison.InvariantCultureIgnoreCase))
								return suffix;
						}
					}
				}
			}

			return result;
		}
	}
}
