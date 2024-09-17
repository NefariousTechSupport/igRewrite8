namespace igLibrary.Core
{
	public class igBuildDependencyAttribute : igObject
	{
		public bool _value = true;
		public void GenerateBuildDependancies(igIGZSaver saver, string value)
		{
			string depName = value.Replace('\\', '/');
			if(!saver._buildDependancies.Any(x => x.Item2 == depName))
			{
				saver._buildDependancies.Add((Path.GetFileNameWithoutExtension(depName), depName));
			}
		}
	}
}