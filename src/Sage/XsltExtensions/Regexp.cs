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
namespace Sage.XsltExtensions
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Text.RegularExpressions;

	using Sage.Extensibility;

	/// <summary>
	/// Provides several regular expression utility methods for use in XSLT.
	/// </summary>
	[XsltExtensionObject(XmlNamespaces.Extensions.Regexp)]
	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter",
		Justification = "This is an XSLT extension class, these methods will not be used from C#.")]
	public class Regexp
	{
		/// <summary>
		/// Replaces the specified input.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <param name="expression">The expression.</param>
		/// <param name="replacement">The replacement.</param>
		/// <returns>TODO: Add documentation for replace.</returns>
		public string replace(string input, string expression, string replacement)
		{
			if (string.IsNullOrEmpty(input))
				return input;

			try
			{
				Regex expr = new Regex(expression);
				return expr.Replace(input, replacement ?? string.Empty);
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}
	}
}
