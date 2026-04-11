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
		/// <summary>
		/// Constructor for config frame
		/// </summary>
		/// <param name="wnd">The window to parent it to</param>
		public ConfigFrame(Window wnd) : base(wnd)
		{
			CauldronConfig.ReadConfig();
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
					UIUtil.RenderTextField("Game Path", "gp", ref game._path);
					UIUtil.RenderTextField("Update Path", "up", ref game._updatePath);

					UIUtil.EnumComboBox("Game",     UIUtil.sGameNames,     ref game._game);
					UIUtil.EnumComboBox("Platform", UIUtil.sPlatformNames, ref game._platform);

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
						CPrecacheFileLoader.LoadInitScript(game._game, debug);

						_wnd._frames.Remove(this);
						_wnd._frames.Add(new DirectoryManagerFrame(_wnd));
						_wnd._frames.Add(new ArchiveFrame(_wnd));
						_wnd._frames.Add(new ArchiveEditorFrame(_wnd, igFileContext.Singleton.LoadArchive("archives:/loosefiles.pak")));

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
	}
}