using ImGuiNET;
using igLibrary.Core;
using igLibrary;
using igLibrary.Gfx;
using System.Diagnostics;

namespace igCauldron3
{
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
		public ConfigFrame(Window wnd) : base(wnd)
		{
			CauldronConfig.ReadConfig();
		}
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

					ImGui.Text("Platform");
					ImGui.SameLine();
					ImGui.PushID("platform");
					bool comboing = ImGui.BeginCombo(string.Empty, GetPlatformName(game._platform));
					ImGui.PopID();
					if(comboing)
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
						igArkCore.ReadFromFile(igArkCore.EGame.EV_SkylandersSuperchargers);
						IG_CORE_PLATFORM platform = igRegistry.GetRegistry()._platform;

						igFileContext.Singleton.LoadArchive("app:/archives/loosefiles.pak");

						if(full)
						{
							CPrecacheManager._Instance.PrecachePackage($"generated/shaders/shaders_{igAlchemyCore.GetPlatformString(platform)}", EMemoryPoolID.MP_DEFAULT);
	
							CPrecacheManager._Instance.PrecachePackage($"generated/packageXmls/permanent_{igAlchemyCore.GetPlatformString(platform)}", EMemoryPoolID.MP_DEFAULT);
							CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/essentialui", EMemoryPoolID.MP_DEFAULT);
							CPrecacheManager._Instance.PrecachePackage("generated/UI/legal", EMemoryPoolID.MP_DEFAULT);
							CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/gamestartup", EMemoryPoolID.MP_DEFAULT);
							CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/permanentdeveloper", EMemoryPoolID.MP_DEFAULT);
							CPrecacheManager._Instance.PrecachePackage("generated/SoundBankData", EMemoryPoolID.MP_DEFAULT);
	
							CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/permanent", EMemoryPoolID.MP_DEFAULT);
							CPrecacheManager._Instance.PrecachePackage("generated/maps/zoneinfos", EMemoryPoolID.MP_DEFAULT);
							CPrecacheManager._Instance.PrecachePackage("generated/packageXmls/permanent_2015", EMemoryPoolID.MP_DEFAULT);
							CPrecacheManager._Instance.PrecachePackage("generated/UI/Domains/JuiceDomain_Mobile", EMemoryPoolID.MP_DEFAULT);
							CPrecacheManager._Instance.PrecachePackage("generated/UI/Domains/JuiceDomain_FrontEnd", EMemoryPoolID.MP_DEFAULT);
						}

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