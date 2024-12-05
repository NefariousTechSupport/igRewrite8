using ImGuiNET;
using igLibrary.Core;
using igLibrary;
using igLibrary.Gfx;
using System.Diagnostics;

namespace igCauldron3
{
	public class ConfigFrame : Frame
	{
		public ConfigFrame(Window wnd) : base(wnd)
		{
			CauldronConfig.ReadConfig();
		}
		private string GetGameName(igArkCore.EGame game)
		{
			for(int i = 0; i < Names.GameNames.Length; i++)
			{
				if(Names.GameNames[i].Item1 == game)
				{
					return Names.GameNames[i].Item2;
				}
			}
			return "Select a Game";
		}
		private string GetPlatformName(IG_CORE_PLATFORM platform)
		{
			for(int i = 0; i < Names.PlatformNames.Length; i++)
			{
				if(Names.PlatformNames[i].Item1 == platform)
				{
					return Names.PlatformNames[i].Item2;
				}
			}
			return "Select a Platform";
		}
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
						for(int p = 0; p < Names.GameNames.Length; p++)
						{
							ImGui.PushID(p);
							if(ImGui.Selectable(Names.GameNames[p].Item2, game._game == Names.GameNames[p].Item1))
							{
								game._game = Names.GameNames[p].Item1;
							}
							if(game._game == Names.GameNames[p].Item1)
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
						for(int p = 0; p < Names.PlatformNames.Length; p++)
						{
							ImGui.PushID(p);
							if(ImGui.Selectable(Names.PlatformNames[p].Item2, game._platform == Names.PlatformNames[p].Item1))
							{
								game._platform = Names.PlatformNames[p].Item1;
							}
							if(game._platform == Names.PlatformNames[p].Item1)
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