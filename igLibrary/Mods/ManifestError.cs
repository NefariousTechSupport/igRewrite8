namespace igLibrary.Mods
{
	public enum ManifestError
	{
		Success,
		MissingVersionProperty,
		MalformedVersionProperty,
		MissingGameProperty,
		MalformedGameProperty,
		UnknownGameName,
		UnsupportedGame,
		LabMissingModifiedProperty,
		LabMalformedModifiedProperty,
		LabMalformedModifiedItem,
		LabMissingPathProperty,
		LabMalformedPathProperty,
		LabMissingArchiveProperty,
		LabMalformedArchiveProperty,
		UnknownError,
	}
}