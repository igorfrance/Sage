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
namespace Sage.Extensibility
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Xml;

	using ICSharpCode.SharpZipLib.Core;
	using ICSharpCode.SharpZipLib.Zip;
	using Kelp.Extensions;
	using Kelp.IO;
	using log4net;

	using Sage.Configuration;
	using Sage.ResourceManagement;

	internal class ExtensionInfo
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ExtensionInfo).FullName);

		private readonly IOrderedEnumerable<InstallLog> orderedLogs;
		private readonly SageContext context;
		private List<Assembly> assemblies;
		private List<InstallLog> installHistory;

		private bool isLoaded;

		internal ExtensionInfo(string archiveFileName, SageContext context)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(archiveFileName));
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentException>(File.Exists(archiveFileName));

			FileStream fs = File.OpenRead(archiveFileName);
			try
			{
				using (ZipFile extensionArchive = new ZipFile(fs))
				{
					this.ArchiveFiles = new List<ExtensionFile>();
					foreach (ZipEntry entry in extensionArchive)
					{
						if (entry.IsFile)
							this.ArchiveFiles.Add(new ExtensionFile(extensionArchive, entry));
					}

					string assetPath = "assets";
					this.context = context;

					this.Name = Path.GetFileNameWithoutExtension(archiveFileName);
					this.ArchiveDate = File.GetLastWriteTime(archiveFileName).Max(File.GetCreationTime(archiveFileName));
					this.ArchiveFileName = archiveFileName;
					this.InstallLogFile = Path.ChangeExtension(this.ArchiveFileName, ".history.xml");

					ExtensionFile configFile = this.ArchiveFiles.FirstOrDefault(file => file.Name.Equals(ProjectConfiguration.ExtensionConfigName));
					if (configFile != null)
					{
						this.Config = ProjectConfiguration.Create(extensionArchive.GetInputStream(configFile.Entry));
						assetPath = (this.Config.AssetPath ?? assetPath).Replace("~/", string.Empty);
					}

					this.AssemblyFiles = this.ArchiveFiles
						.Where(v =>
							v.Name.StartsWith("bin/", StringComparison.InvariantCultureIgnoreCase) &&
							v.Name.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
						.ToList();

					this.AssetFiles = this.ArchiveFiles
						.Where(v => v.Name.StartsWith(assetPath, StringComparison.InvariantCultureIgnoreCase))
						.ToList();

					this.TargetAssets = this.AssetFiles.Select(this.GetTargetPath).ToList();
				}
			}
			finally
			{
				fs.Close();
			} 
			
			this.orderedLogs = from i in this.InstallHistory
							   orderby i.Date
							   select i;
		}

		public List<ExtensionFile> ArchiveFiles { get; private set; }

		public List<ExtensionFile> AssemblyFiles { get; private set; }

		public List<ExtensionFile> AssetFiles { get; private set; }

		public string Name { get; private set; }

		public DateTime ArchiveDate { get; private set; }

		public DateTime? LastInstallDate
		{
			get
			{
				return this.orderedLogs.Last(l => l.Result == InstallState.Installed).Date;
			}
		}

		public ProjectConfiguration Config { get; private set; }

		public List<Assembly> Assemblies
		{
			get
			{
				if (assemblies == null)
					LoadAssemblies();

				return assemblies;
			}
		}

		public List<string> TargetAssets { get; private set; }

		public List<InstallLog> InstallHistory 
		{
			get
			{
				if (this.installHistory == null)
				{
					this.installHistory = new List<InstallLog>();
					if (File.Exists(this.InstallLogFile))
					{
						XmlDocument logDoc = new XmlDocument();
						logDoc.Load(this.InstallLogFile);

						foreach (XmlElement element in logDoc.SelectNodes("/plugin/install"))
							this.installHistory.Add(new InstallLog(element));
					}
				}

				return this.installHistory;
			}
		}

		public string InstallLogFile { get; private set; }

		public string ArchiveFileName { get; private set; }

		public bool IsInstalled
		{
			get
			{
				if (this.InstallHistory.Count == 0)
					return false;

				return orderedLogs.Last().Result == InstallState.Installed;
			}
		}

		public bool IsUpdateAvailable
		{
			get
			{
				DateTime? lastInstallDate = orderedLogs.Last().Date;
				return this.ArchiveDate > lastInstallDate;
			}
		}

		public bool IsMissingResources
		{
			get
			{
				return this.TargetAssets.Any(targetAsset => !File.Exists(targetAsset));
			}
		}

		public void Update(bool forceUpdate = false)
		{
			Uninstall(true, forceUpdate);
			Install();
		}

		public void Install()
		{
			if (this.IsInstalled)
				this.Uninstall();

			InstallLog installLog = new InstallLog(DateTime.Now);

			log.DebugFormat("Installation of extension '{0}' started", this.Name);

			FileStream fs = File.OpenRead(this.ArchiveFileName);
			ZipFile extensionArchive = new ZipFile(fs);
			try
			{
				foreach (ExtensionFile file in this.ArchiveFiles)
				{
					string childPath = file.Name;
					string targetPath = context.Path.Resolve(childPath);
					string targetDir = Path.GetDirectoryName(targetPath);

					Directory.CreateDirectory(targetDir);

					InstallItem entry = installLog.AddFile(targetPath);
					entry.CrcCode = Crc32.GetHash(extensionArchive.GetInputStream(file.Entry));

					if (File.Exists(targetPath))
					{
						if (!this.HasInstalled(targetPath))
						{
							log.WarnFormat("Extension {1}: skipped installing '{0}' because a file with the same name already exists, and it doesn't originate from this extension.",
								targetPath, this.Name);

							entry.State = InstallState.NotInstalled;
							continue;
						}

						if (Crc32.GetHash(targetPath) != entry.CrcCode)
						{
							log.WarnFormat("Extension {1}: not overwriting previously installed file '{0}' because it has been changed.",
								targetPath, this.Name);

							entry.State = InstallState.NotInstalled;
							continue;
						}
					}

					file.Extract(extensionArchive, targetPath);
					entry.State = InstallState.Installed;
				}

				installLog.Result = InstallState.Installed;
			}
			catch (Exception ex)
			{
				log.Error(ex);

				installLog.Error = ex;
				installLog.Result = InstallState.NotInstalled;

				this.Rollback(installLog);
			}
			finally
			{
				fs.Close();
				extensionArchive.Close();

				SaveLog(installLog);
			}

			log.DebugFormat("Installation of extension '{0}' {1}.", this.Name, 
				installLog.Result == InstallState.Installed ? "succeeded" : "failed");
		}

		public void Refresh()
		{
			FileStream fs = File.OpenRead(this.ArchiveFileName);
			ZipFile extensionArchive = new ZipFile(fs);
			try
			{
				foreach (ExtensionFile file in this.ArchiveFiles)
				{
					string targetPath = this.GetTargetPath(file);
					if (!File.Exists(targetPath))
					{
						string directoryPath = Path.GetDirectoryName(targetPath);
						if (!Directory.Exists(directoryPath))
							Directory.CreateDirectory(directoryPath);

						file.Extract(extensionArchive, targetPath);
					}
				}
			}
			finally
			{
				extensionArchive.Close();
				fs.Close();
			}
		}

		public void Uninstall(bool isUpdateUninstall = false, bool deleteChangedFiles = false)
		{
			if (!this.IsInstalled)
				return;

			log.DebugFormat("Uninstalling extension '{0}'", this.Name);

			var installLog = orderedLogs.Last();
			this.Rollback(installLog, deleteChangedFiles);
			SaveLog(installLog);
		}

		public void LoadAssemblies()
		{
			if (this.isLoaded)
				return;

			FileStream fs = File.OpenRead(this.ArchiveFileName);
			ZipFile extensionArchive = new ZipFile(fs);
			try
			{
				this.assemblies = new List<Assembly>();
				foreach (ExtensionFile assemblyFile in this.AssemblyFiles)
				{
					Assembly extensionAssembly = Assembly.Load(assemblyFile.Read(extensionArchive));
					this.assemblies.Add(extensionAssembly);
				}

				this.isLoaded = true;
			}
			finally 
			{
				extensionArchive.Close();
				fs.Close();
			}
		}

		public bool HasInstalled(string itemPath)
		{
			InstallLog firstInstall = orderedLogs.FirstOrDefault(l => l.Items.FirstOrDefault(i => i.Path == itemPath) != null);
			if (firstInstall != null)
			{
				InstallItem item = firstInstall.Items.First(i => i.Path == itemPath);
				return item.State == InstallState.Installed;
			}

			return false;
		}

		public CacheableXmlDocument GetDictionary(string locale)
		{
			SageContext pluginContext = GetExtensionContext();
			string path = pluginContext.Path.GetDictionaryPath(locale);
			if (File.Exists(path))
				return pluginContext.Resources.LoadXml(path);

			return null;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0}", this.Name);
		}

		private SageContext GetExtensionContext()
		{
			return new SageContext(context, this.Config);
		}

		private void Rollback(InstallLog installLog, bool deleteChangedFiles = false)
		{
			log.DebugFormat("Rolling back log extension '{0}'", this.Name);

			IEnumerable<InstallItem> installedItems = installLog.Items.Where(f => f.State == InstallState.Installed);
			foreach (InstallItem installItem in installedItems)
			{
				if (!File.Exists(installItem.Path))
				{
					installItem.State = InstallState.UnInstalled;
					continue;
				}

				ExtensionFile archiveFile = GetArchiveFile(installItem.Path);

				//// CRC of the file in the current application
				string currentCrc = Crc32.GetHash(installItem.Path);
				//// CRC of the file at the time when it was installed
				string originalCrc = installItem.CrcCode;
				//// CRC of the file in the current archive
				string updatedCrc = archiveFile == null ? null : archiveFile.CrcCode;

				if (!deleteChangedFiles && (currentCrc != originalCrc && updatedCrc != null && currentCrc != updatedCrc))
				{
					log.WarnFormat("The file '{0}' has changed since it was installed, it will not be deleted", installItem.Path);
					continue;
				}

				try
				{
					File.Delete(installItem.Path);
					installItem.State = InstallState.UnInstalled;
				}
				catch (Exception ex)
				{
					log.Error(ex);
				}
			}

			installLog.Result = InstallState.UnInstalled;
		}

		private void SaveLog(InstallLog installLog)
		{
			XmlDocument logDoc = new XmlDocument();
			if (File.Exists(this.InstallLogFile))
				logDoc.Load(this.InstallLogFile);
			else
				logDoc.LoadXml(string.Format("<plugin name='{0}'></plugin>", this.Name));

			logDoc.DocumentElement.AppendChild(installLog.ToXml(logDoc));
			logDoc.Save(this.InstallLogFile);
		}

		private string GetTargetPath(ExtensionFile sourceFile)
		{
			return context.Path.Resolve(sourceFile.Name);
		}

		private ExtensionFile GetArchiveFile(string targetPath)
		{
			string childPath = targetPath.ToLower().Replace(context.Path.Resolve("~/").ToLower(), string.Empty);
			return this.ArchiveFiles.FirstOrDefault(f => f.Name.Equals(childPath, StringComparison.InvariantCultureIgnoreCase));
		}
	}
}
