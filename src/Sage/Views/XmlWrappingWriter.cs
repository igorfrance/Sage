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
namespace Sage.Views
{
	using System;
	using System.Xml;

	/// <summary>
	/// Implements an <see cref="XmlWriter"/> that wraps another <see cref="XmlWriter"/>.
	/// </summary>
	public class XmlWrappingWriter : XmlWriter
	{
		private XmlWriter writer; 

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlWrappingWriter"/> class.
		/// </summary>
		/// <param name="baseWriter">The base writer this writer is wrapping.</param>
		public XmlWrappingWriter(XmlWriter baseWriter)
		{
			this.Writer = baseWriter;
		}

		/// <inheritdoc/>
		public override XmlWriterSettings Settings
		{
			get
			{
				return this.writer.Settings;
			}
		}

		/// <inheritdoc/>
		public override System.Xml.WriteState WriteState
		{
			get
			{
				return this.writer.WriteState;
			}
		}

		/// <inheritdoc/>
		public override string XmlLang
		{
			get
			{
				return this.writer.XmlLang;
			}
		}

		/// <inheritdoc/>
		public override System.Xml.XmlSpace XmlSpace
		{
			get
			{
				return this.writer.XmlSpace;
			}
		}

		/// <inheritdoc/>
		protected XmlWriter Writer
		{
			get
			{
				return this.writer;
			}

			set
			{
				this.writer = value;
			}
		}

		/// <inheritdoc/>
		public override void Close()
		{
			this.writer.Close();
		}

		/// <inheritdoc/>
		public override void Flush()
		{
			this.writer.Flush();
		}

		/// <inheritdoc/>
		public override string LookupPrefix(string ns)
		{
			return this.writer.LookupPrefix(ns);
		}

		/// <inheritdoc/>
		public override void WriteBase64(byte[] buffer, int index, int count)
		{
			this.writer.WriteBase64(buffer, index, count);
		}

		/// <inheritdoc/>
		public override void WriteCData(string text)
		{
			this.writer.WriteCData(text);
		}

		/// <inheritdoc/>
		public override void WriteCharEntity(char ch)
		{
			this.writer.WriteCharEntity(ch);
		}

		/// <inheritdoc/>
		public override void WriteChars(char[] buffer, int index, int count)
		{
			this.writer.WriteChars(buffer, index, count);
		}

		/// <inheritdoc/>
		public override void WriteComment(string text)
		{
			this.writer.WriteComment(text);
		}

		/// <inheritdoc/>
		public override void WriteDocType(string name, string pubId, string sysId, string subset)
		{
			this.writer.WriteDocType(name, pubId, sysId, subset);
		}

		/// <inheritdoc/>
		public override void WriteEndAttribute()
		{
			this.writer.WriteEndAttribute();
		}

		/// <inheritdoc/>
		public override void WriteEndDocument()
		{
			this.writer.WriteEndDocument();
		}

		/// <inheritdoc/>
		public override void WriteEndElement()
		{
			this.writer.WriteEndElement();
		}

		/// <inheritdoc/>
		public override void WriteEntityRef(string name)
		{
			this.writer.WriteEntityRef(name);
		}

		/// <inheritdoc/>
		public override void WriteFullEndElement()
		{
			this.writer.WriteFullEndElement();
		}

		/// <inheritdoc/>
		public override void WriteProcessingInstruction(string name, string text)
		{
			this.writer.WriteProcessingInstruction(name, text);
		}

		/// <inheritdoc/>
		public override void WriteRaw(string data)
		{
			this.writer.WriteRaw(data);
		}

		/// <inheritdoc/>
		public override void WriteRaw(char[] buffer, int index, int count)
		{
			this.writer.WriteRaw(buffer, index, count);
		}

		/// <inheritdoc/>
		public override void WriteStartAttribute(string prefix, string localName, string ns)
		{
			this.writer.WriteStartAttribute(prefix, localName, ns);
		}

		/// <inheritdoc/>
		public override void WriteStartDocument()
		{
			this.writer.WriteStartDocument();
		}

		/// <inheritdoc/>
		public override void WriteStartDocument(bool standalone)
		{
			this.writer.WriteStartDocument(standalone);
		}

		/// <inheritdoc/>
		public override void WriteStartElement(string prefix, string localName, string ns)
		{
			this.writer.WriteStartElement(prefix, localName, ns);
		}

		/// <inheritdoc/>
		public override void WriteString(string text)
		{
			this.writer.WriteString(text);
		}

		/// <inheritdoc/>
		public override void WriteSurrogateCharEntity(char lowChar, char highChar)
		{
			this.writer.WriteSurrogateCharEntity(lowChar, highChar);
		}

		/// <inheritdoc/>
		public override void WriteValue(bool value)
		{
			this.writer.WriteValue(value);
		}

		/// <inheritdoc/>
		public override void WriteValue(DateTime value)
		{
			this.writer.WriteValue(value);
		}

		/// <inheritdoc/>
		public override void WriteValue(decimal value)
		{
			this.writer.WriteValue(value);
		}

		/// <inheritdoc/>
		public override void WriteValue(double value)
		{
			this.writer.WriteValue(value);
		}

		/// <inheritdoc/>
		public override void WriteValue(int value)
		{
			this.writer.WriteValue(value);
		}

		/// <inheritdoc/>
		public override void WriteValue(long value)
		{
			this.writer.WriteValue(value);
		}

		/// <inheritdoc/>
		public override void WriteValue(object value)
		{
			this.writer.WriteValue(value);
		}

		/// <inheritdoc/>
		public override void WriteValue(float value)
		{
			this.writer.WriteValue(value);
		}

		/// <inheritdoc/>
		public override void WriteValue(string value)
		{
			this.writer.WriteValue(value);
		}

		/// <summary>
		/// When overridden in a derived class, writes out the given white space.
		/// </summary>
		/// <param name="ws">The string of white space characters.</param>
		public override void WriteWhitespace(string ws)
		{
			this.writer.WriteWhitespace(ws);
		}

		/// <inheritdoc/>
		protected override void Dispose(bool disposing)
		{
			((IDisposable) this.writer).Dispose();
		}
	}
}
