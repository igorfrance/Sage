﻿// <auto-generated>Marked as auto-generated so StyleCop will ignore BDD style tests</auto-generated>
/**
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

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
namespace Sage.Test
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Kelp;
	using Kelp.Extensions;
	using Kelp.ResourceHandling;

	using Machine.Specifications;

	using log4net;

	/// <summary>
	/// This class serves to hold temporary code snippets that can be run using the test runner.
	/// </summary>
	[Tags(Categories.TestBench), Ignore("This is for local ")]
	public class TestBench
	{
		static readonly ILog log = LogManager.GetLogger(typeof(TestBench).FullName);

		private It CheckForDuplicateReferences = () =>
		{
			IEnumerable<IGrouping<string, Util.Reference>> referenceGroups =
				Kelp.Util.FindConflictingReferences(@"G:\cycle99\projects\git\sage\Sage.Tools\bin\Debug");

			foreach (var group in referenceGroups)
			{
				log.WarnFormat("Possible conflicts for {0}:", group.Key);
				log.WarnFormat("=".Repeat(80));
				foreach (var reference in group)
				{
					log.WarnFormat("{0} references {1}",
						reference.Assembly.Name.PadLeft(25),
						reference.ReferencedAssembly.FullName);
				}
				log.WarnFormat("=".Repeat(80));
			}
		};

		private It TestTheMimeExtensionHelper = () =>
		{
			string mimeType = Kelp.Http.Util.GetMimeType("myimage.jpg");
			log.DebugFormat("Mime type is: " + mimeType);
		};
	}
}
