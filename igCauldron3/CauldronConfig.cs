/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace igCauldron3
{
	/// <summary>
	/// The settings file
	/// </summary>
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
		private const int CurrentVersion = 2;

		public int _version = CurrentVersion;
		public List<GameConfig> _games = new List<GameConfig>();

		// A lot of this json stuff should be rewritten to not rely on reflection
		public class VersionChecker
		{
			public int _version;
		}

		// Per game config
		public class GameConfig
		{
			public string _path = string.Empty;
			public string _updatePath = string.Empty;
			[JsonConverter(typeof(StringEnumConverter))] public igArkCore.EGame _game = igArkCore.EGame.EV_None;
			[JsonConverter(typeof(StringEnumConverter))] public IG_CORE_PLATFORM _platform;
		}


		/// <summary>
		/// Load the configuration file
		/// </summary>
		/// <exception cref="ApplicationException">Thrown when it fails to read the config</exception>
		public static void ReadConfig()
		{
			if(File.Exists(GameConfigFilePath))
			{
				string json = File.ReadAllText(GameConfigFilePath);

				int version = JsonConvert.DeserializeObject<VersionChecker>(json)._version;
				if (version == CurrentVersion)
				{
					_config = JsonConvert.DeserializeObject<CauldronConfig>(json);
				}

				if(_config == null) throw new ApplicationException($"Failed to load config. Try deleting \"{GameConfigFilePath}\" and try again.");
			}
			else
			{
				_config = new CauldronConfig();
			}
		}


		/// <summary>
		/// Writing config
		/// </summary>
		public static void WriteConfig()
		{
			string json = JsonConvert.SerializeObject(_config);
			File.WriteAllText(GameConfigFilePath, json);
		}
	}
}