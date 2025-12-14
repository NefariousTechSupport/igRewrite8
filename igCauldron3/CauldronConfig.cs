/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Core;
using ImGuiNET;
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
		private static string PrefsFilePath => Path.Combine(ConfigFolder, "prefs.json");
		public static string ImGuiConfigFilePath => Path.Combine(ConfigFolder, "imgui.ini");
		public static CauldronConfig _config { get; private set; }
		private const int CurrentVersion = 2;

		public int _version = CurrentVersion;
		public List<GameConfig> _games = new List<GameConfig>();
		[JsonIgnore] public Preferences _preferences = new Preferences();

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

		// UI Preferences
		public class Preferences
		{
			[JsonConverter(typeof(StringEnumConverter))]
			public EFontType _fontName;

			public float _fontScale = 2;

			public uint _lineSpacing;
		}

		public enum EFontType
		{
			kArial,
			kComicSans,
			kConsolas,
			kVerdana,
			kOpenDyslexic,
			kProggyClean,
			kMarkinLT,
			kProximaNova,
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

			if (File.Exists(PrefsFilePath))
			{
				string json = File.ReadAllText(PrefsFilePath);

				_config._preferences = JsonConvert.DeserializeObject<Preferences>(json);

				if(_config._preferences == null) throw new ApplicationException($"Failed to load preferences. Try deleting \"{PrefsFilePath}\" and try again.");

				ReloadFont();
			}
		}


		/// <summary>
		/// Writing config
		/// </summary>
		public static void WriteConfig()
		{
			string json = JsonConvert.SerializeObject(_config);
			File.WriteAllText(GameConfigFilePath, json);

			json = JsonConvert.SerializeObject(_config._preferences);
			File.WriteAllText(PrefsFilePath, json);
		}


		public static void ReloadFont()
		{
			switch (_config._preferences._fontName)
			{
				case EFontType.kArial:
					Styles._currentFont = Styles._arielFont;
					break;
				case EFontType.kComicSans:
					Styles._currentFont = Styles._comicSansFont;
					break;
				case EFontType.kConsolas:
					Styles._currentFont = Styles._consolasFont;
					break;
				case EFontType.kMarkinLT:
					// Unimplemented
					//Styles._currentFont = Styles._markinLtFont;
					break;
				case EFontType.kOpenDyslexic:
					Styles._currentFont = Styles._dyslexicFont;
					break;
				case EFontType.kProggyClean:
					Styles._currentFont = Styles._proggyCleanFont;
					break;
				case EFontType.kProximaNova:
					// Unimplemented
					//Styles._currentFont = Styles._proximaNovaFont;
					break;
				case EFontType.kVerdana:
					Styles._currentFont = Styles._verdanaFont;
					break;
			}

			ImGui.GetIO().FontGlobalScale = _config._preferences._fontScale;
		}
	}
}