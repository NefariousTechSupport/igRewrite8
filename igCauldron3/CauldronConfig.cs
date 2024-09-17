using igLibrary.Core;
using Newtonsoft.Json;

namespace igCauldron3
{
	public class CauldronConfig
	{
		public static string ConfigFolder
		{
			get
			{
				string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NefariousTechSupport", "igCauldron");
				Directory.CreateDirectory(path);
				return path;
			}
		}
		private static string GameConfigFilePath => Path.Combine(ConfigFolder, "gameconfig.json");
		public static string ImGuiConfigFilePath => Path.Combine(ConfigFolder, "imgui.ini");
		public static CauldronConfig _config { get; private set; }

		public int _version = 1;
		public List<GameConfig> _games = new List<GameConfig>();

		public class GameConfig
		{
			public string _path = string.Empty;
			public string _updatePath = string.Empty;
			public igArkCore.EGame _game = igArkCore.EGame.EV_SkylandersSuperchargers;	//This'll be replaced once multiple games work
			public IG_CORE_PLATFORM _platform;
		}

		public static void ReadConfig()
		{
			if(File.Exists(GameConfigFilePath))
			{
				string json = File.ReadAllText(GameConfigFilePath);
				_config = JsonConvert.DeserializeObject<CauldronConfig>(json);
				if(_config == null) throw new ApplicationException($"Failed to load config. Try deleting \"{GameConfigFilePath}\" and trying again.");
			}
			else
			{
				_config = new CauldronConfig();
			}
		}
		public static void WriteConfig()
		{
			string json = JsonConvert.SerializeObject(_config);
			File.WriteAllText(GameConfigFilePath, json);
		}
	}
}