using System.Text.Json.Nodes;

namespace igLibrary.Mods
{
	public sealed class AlchemyLaboratoryModManifest : ModManifest
	{
		public struct ModifiedFile
		{
			public string _path;
			public string? _archive;
			public ModifiedFile()
			{
				_path = string.Empty;
				_archive = null;
			}
		}

		private List<ModifiedFile> _modifiedFiles = new List<ModifiedFile>();

		public List<ModifiedFile> ModifiedFiles => _modifiedFiles;

		protected override ManifestError Parse(JsonObject manifestJson)
		{
			// Parsing modified list
			{
				if (!manifestJson.TryGetPropertyValue("modified", out JsonNode? modifiedNode))
				{
					Logging.Error("Failed to locate modified property");
					return ManifestError.LabMissingModifiedProperty;
				}
				if (modifiedNode is not JsonArray modifiedArray)
				{
					return ManifestError.LabMalformedModifiedProperty;
				}

				// Parsing each item
				foreach(JsonNode? fileNode in modifiedArray)
				{
					if (fileNode == null)
					{
						Logging.Error("File node is null?");
						return ManifestError.UnknownError;
					}

					if (fileNode is not JsonObject fileObject)
					{
						Logging.Error("modified item must be a json object");
						return ManifestError.LabMalformedModifiedItem;
					}

					string path;
					{
						if (!fileObject.TryGetPropertyValue("path", out JsonNode? pathValue))
						{
							Logging.Error("Failed to locate path property");
							return ManifestError.LabMissingPathProperty;
						}
						if (!fileObject!.AsValue().TryGetValue<string>(out path!))
						{
							Logging.Error("Path malformed, must be a string");
							return ManifestError.LabMalformedPathProperty;
						}
					}

					string? archive;
					{
						if (!fileObject.TryGetPropertyValue("archive", out JsonNode? archiveValue))
						{
							Logging.Error("Failed to locate archive property");
							return ManifestError.LabMissingArchiveProperty;
						}
						if (!archiveValue!.AsValue().TryGetValue<string>(out archive))
						{
							Logging.Error("archive malformed, must be a string");
							return ManifestError.LabMalformedArchiveProperty;
						}
					}

					ModifiedFile file = new ModifiedFile();
					file._path = path;
					file._archive = archive;
					_modifiedFiles.Add(file);
				}
			}

			return ManifestError.Success;
		}
	}
}