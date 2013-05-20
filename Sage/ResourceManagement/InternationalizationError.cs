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

	/// <summary>
	/// Represents an error that occurs during globalization.
	/// </summary>
	public class InternationalizationError : Exception
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="InternationalizationError"/> class.
        /// </summary>
		public InternationalizationError()
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="InternationalizationError"/> class, using the specified <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message.</param>
		public InternationalizationError(string message)
			: base(message)
		{
		}
	}
}
