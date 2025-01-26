/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using ImGuiNET;
using igLibrary.Core;
using igLibrary;
using igLibrary.Gfx;
using System.Diagnostics;

namespace igCauldron3
{
	/// <summary>
	/// Intitial configuration UI frame
	/// </summary>
	public class ConfigFrame : Frame
	{
		public (IG_CORE_PLATFORM, string)[] _platformNames = new (IG_CORE_PLATFORM, string)[]
		{
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ANDROID, "Android 32-bit"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN, "iOS 32-bit"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64, "iOS 64-bit"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_LINUX, "Linux"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_LGTV, "LG Smart TV"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_OSX, "Mac OS 32-bit"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_MARMALADE, "Marmalade"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_NGP, "PSVita"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS3, "PS3"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS4, "PS4"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_RASPI, "Raspberry Pi"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WII, "Wii"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_CAFE, "Wii U"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN32, "Windows 32-bit"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN64, "Windows 64-bit"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WP8, "Windows Phone"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_XENON, "Xbox 360"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_DURANGO, "Xbox One")
		};

		public (igArkCore.EGame, string)[] _gameNames = new (igArkCore.EGame, string)[]
		{
			(igArkCore.EGame.EV_SkylandersSuperchargers, "Skylanders Superchargers 1.6.X"),
			(igArkCore.EGame.EV_SkylandersImaginators,   "Skylanders Imaginators 1.1.X")
		};


		/// <summary>
		/// Constructor for config frame
		/// </summary>
		/// <param name="wnd">The window to parent it to</param>
		public ConfigFrame(Window wnd) : base(wnd)
		{
			CauldronConfig.ReadConfig();
		}


		/// <summary>
		/// Lookup the name of a game based on the enum
		/// </summary>
		/// <param name="game">The game to grab the string for</param>
		/// <returns>The name for the game</returns>
		private string GetGameName(igArkCore.EGame game)
		{
			for(int i = 0; i < _gameNames.Length; i++)
			{
				if(_gameNames[i].Item1 == game)
				{
					return _gameNames[i].Item2;
				}
			}
			return "Select a Game";
		}


		/// <summary>
		/// Lookup the name of a platform based on the enum
		/// </summary>
		/// <param name="platform">The platform to grab the string for</param>
		/// <returns>The name for the platform</returns>
		private string GetPlatformName(IG_CORE_PLATFORM platform)
		{
			for(int i = 0; i < _platformNames.Length; i++)
			{
				if(_platformNames[i].Item1 == platform)
				{
					return _platformNames[i].Item2;
				}
			}
			return "Select a Platform";
		}


		/// <summary>
		/// Renders the ui
		/// </summary>
		/// <exception cref="DirectoryNotFoundException">Thrown if the game directory is missing</exception>
		/// <exception cref="FileNotFoundException">Thrown if the update file is missing</exception>
		/// <exception cref="ArgumentException">If the platform specified is invalid</exception>
		public override void Render()
		{
			ImGui.Begin("Configuration");

			CauldronConfig config = CauldronConfig._config;

			for(int i = 0; i < config._games.Count; i++)
			{
				CauldronConfig.GameConfig game = config._games[i];
				if(ImGui.TreeNode(i.ToString("X08"), $"Game {i}: {game._path}"))
				{
					RenderTextField("Game Path", "gp", ref game._path);
					RenderTextField("Update Path", "up", ref game._updatePath);

					ImGui.Text("Game");
					ImGui.SameLine();
					ImGui.PushID("game");
					bool gameComboing = ImGui.BeginCombo(string.Empty, GetGameName(game._game));
					ImGui.PopID();
					if(gameComboing)
					{
						for(int p = 0; p < _gameNames.Length; p++)
						{
							ImGui.PushID(p);
							if(ImGui.Selectable(_gameNames[p].Item2, game._game == _gameNames[p].Item1))
							{
								game._game = _gameNames[p].Item1;
							}
							if(game._game == _gameNames[p].Item1)
							{
								ImGui.SetItemDefaultFocus();
							}
							ImGui.PopID();
						}
						ImGui.EndCombo();
					}

					ImGui.Text("Platform");
					ImGui.SameLine();
					ImGui.PushID("platform");
					bool platformComboing = ImGui.BeginCombo(string.Empty, GetPlatformName(game._platform));
					ImGui.PopID();
					if(platformComboing)
					{
						for(int p = 0; p < _platformNames.Length; p++)
						{
							ImGui.PushID(p);
							if(ImGui.Selectable(_platformNames[p].Item2, game._platform == _platformNames[p].Item1))
							{
								game._platform = _platformNames[p].Item1;
							}
							if(game._platform == _platformNames[p].Item1)
							{
								ImGui.SetItemDefaultFocus();
							}
							ImGui.PopID();
						}
						ImGui.EndCombo();
					}

					bool full = ImGui.Button("Load Game");
					ImGui.SameLine();
					bool debug = ImGui.Button("Debug Game");

					if(full || debug)
					{
						if(!Directory.Exists(game._path)) throw new DirectoryNotFoundException("Game folder does not exist");
						if(!string.IsNullOrWhiteSpace(game._updatePath) && !File.Exists(game._updatePath)) throw new FileNotFoundException("Update file does not exist");
						if(game._platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT || game._platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEPRECATED || game._platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_MAX) throw new ArgumentException("Invalid platform");

						Stopwatch timer = new Stopwatch();
						timer.Start();

						CauldronConfig.WriteConfig();

						igFileContext.Singleton.Initialize(game._path);
						if(!string.IsNullOrWhiteSpace(game._updatePath))
						{
							igFileContext.Singleton.InitializeUpdate(game._updatePath);
						}

						igRegistry.GetRegistry()._platform = game._platform;
						igRegistry.GetRegistry()._gfxPlatform = igGfx.GetGfxPlatformFromCore(game._platform);

						igArkCore.ReadFromXmlFile(game._game);
						CPrecacheFileLoader.LoadInitialPackages(game._game, debug);

						_wnd._frames.Remove(this);
						_wnd._frames.Add(new DirectoryManagerFrame(_wnd));
						_wnd._frames.Add(new ArchiveFrame(_wnd));
						_wnd._frames.Add(new MenuBarFrame(_wnd));

						timer.Stop();
						Logging.Info("Loaded game in {0}", timer.Elapsed.TotalSeconds);
					}
					ImGui.SameLine();
					if(ImGui.Button("Remove Game"))
					{
						config._games.RemoveAt(i);
					}

					ImGui.TreePop();
				}
			}

			if(ImGui.Button("Add Game"))
			{
				config._games.Add(new CauldronConfig.GameConfig());
			}

			ImGui.End();
		}


		/// <summary>
		/// Render one of the text fields
		/// </summary>
		/// <param name="label">The text to show</param>
		/// <param name="id">The id to use</param>
		/// <param name="val">The string value for the user to edit</param>
		private void RenderTextField(string label, string id, ref string val)
		{
			ImGui.Text(label);
			ImGui.SameLine();
			ImGui.PushID(id);
			ImGui.InputText(string.Empty, ref val, 512);
			ImGui.PopID();
		}
	}
}