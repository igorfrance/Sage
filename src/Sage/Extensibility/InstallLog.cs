﻿namespace Sage.Extensibility
{
	using System;
	using System.Collections.Generic;
	using System.Xml;

	using Kelp.Core.Extensions;

	internal class InstallLog
	{
		public InstallLog(XmlElement element)
		{
			string dateString = element.GetAttribute("dateTime");
			string resultString = element.GetAttribute("result");

			if (!string.IsNullOrWhiteSpace(dateString))
			{
				DateTime dateTime;
				if (DateTime.TryParse(dateString, out dateTime))
					this.Date = dateTime;
			}

			if (!string.IsNullOrWhiteSpace(resultString))
			{
				InstallState result;
				if (Enum.TryParse(resultString, true, out result))
					this.Result = result;
			}

			this.Files = new List<InstallFile>();
			foreach (XmlElement fileElem in element.SelectNodes("files/file"))
			{
				InstallFile file = this.AddFile(fileElem.GetAttribute("path"));
				string stateString = fileElem.GetAttribute("state");
				if (!string.IsNullOrWhiteSpace(stateString))
				{
					FileState state;
					if (Enum.TryParse(resultString, true, out state))
						file.State = state;
				}
			}
		}

		public InstallLog(DateTime date)
		{
			this.Date = date;
			this.Files = new List<InstallFile>();
		}

		public DateTime? Date { get; private set; }

		public Exception Error { get; set; }

		public InstallState Result { get; set; }

		public List<InstallFile> Files { get; private set; }

		public InstallFile AddFile(string path)
		{
			InstallFile file = new InstallFile { Path = path };
			this.Files.Add(file);
			return file;
		}

		public XmlElement ToXml(XmlDocument ownerDoc)
		{
			XmlElement logElement = ownerDoc.CreateElement("install");
			logElement.SetAttribute("dateTime", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss"));
			logElement.SetAttribute("result", this.Result.ToString());

			XmlElement filesElement = logElement.AppendElement("files");
			foreach (InstallFile file in this.Files)
			{
				XmlElement fileElement = filesElement.AppendElement("file");
				fileElement.SetAttribute("path", file.Path);
				fileElement.SetAttribute("state", file.State.ToString().ToLower());
			}

			if (this.Error != null)
			{
				XmlElement errorElement = logElement.AppendElement("exception");
				errorElement.SetAttribute("message", this.Error.InnermostExceptionMessage());
				errorElement.SetAttribute("type", this.Error.InnermostExceptionTypeName());
				errorElement.InnerText = this.Error.InnermostExceptionStackTrace();
			}

			return logElement;
		}
	}
}