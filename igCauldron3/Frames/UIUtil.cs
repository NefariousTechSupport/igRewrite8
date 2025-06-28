using igLibrary.Core;
using ImGuiNET;

namespace igCauldron3
{
	/// <summary>
	/// UI Utilities
	/// </summary>
	public static class UIUtil
	{
		/// <summary>
		/// Platform names
		/// </summary>
		public static readonly (IG_CORE_PLATFORM, string)[] sPlatformNames = new (IG_CORE_PLATFORM, string)[]
		{
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT,   "Select a Platform"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ANDROID,   "Android 32-bit"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN,     "iOS 32-bit"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64,   "iOS 64-bit"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_LINUX,     "Linux"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_LGTV,      "LG Smart TV"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_OSX,       "Mac OS 32-bit"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_MARMALADE, "Marmalade"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_NGP,       "PSVita"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS3,       "PS3"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS4,       "PS4"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_RASPI,     "Raspberry Pi"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WII,       "Wii"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_CAFE,      "Wii U"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN32,     "Windows 32-bit"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN64,     "Windows 64-bit"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WP8,       "Windows Phone"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_XENON,     "Xbox 360"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_DURANGO,   "Xbox One")
		};


		/// <summary>
		/// Game names
		/// </summary>
		public static readonly (igArkCore.EGame, string)[] sGameNames = new (igArkCore.EGame, string)[]
		{
			(igArkCore.EGame.EV_None,                    "Select a Game"),
			(igArkCore.EGame.EV_SkylandersSuperchargers, "Skylanders Superchargers 1.6.X"),
			(igArkCore.EGame.EV_SkylandersImaginators,   "Skylanders Imaginators 1.1.X")
		};



		/// <summary>
		/// Lookup the name of a game based on the enum
		/// </summary>
		/// <param name="game">The game to grab the string for</param>
		/// <returns>The name for the game</returns>
		private static string GetGameName(igArkCore.EGame game)
		{
			for (int i = 0; i < sGameNames.Length; i++)
			{
				if (sGameNames[i].Item1 == game)
				{
					return sGameNames[i].Item2;
				}
			}
			return "Select a Game";
		}


		/// <summary>
		/// Lookup the name of a platform based on the enum
		/// </summary>
		/// <param name="platform">The platform to grab the string for</param>
		/// <returns>The name for the platform</returns>
		private static string GetPlatformName(IG_CORE_PLATFORM platform)
		{
			for (int i = 0; i < sPlatformNames.Length; i++)
			{
				if (sPlatformNames[i].Item1 == platform)
				{
					return sPlatformNames[i].Item2;
				}
			}
			return "Select a Platform";
		}



		/// <summary>
		/// Renders a combo box of enums with reThe label to display</param>
		/// <param name="names">The lookup table of names</param>
		/// <param name="value">the value</param>
		public static void EnumComboBox<T>(string label, (T, string)[] names, ref T value) where T : Enum
		{
			string preview = string.Empty;
			for (int i = 0; i < names.Length; i++)
			{
				if (names[i].Item1.Equals(value))
				{
					preview = names[i].Item2;
				}
			}

			ImGui.Text(label);
			ImGui.SameLine();

			if (ImGui.BeginCombo($"##{label}", preview))
			{
				for (int p = 0; p < names.Length; p++)
				{
					ImGui.PushID(p);
					if (ImGui.Selectable(names[p].Item2, names[p].Item1.Equals(value)))
					{
						value = names[p].Item1;
					}
					if (names[p].Item1.Equals(value))
					{
						ImGui.SetItemDefaultFocus();
					}
					ImGui.PopID();
				}
				ImGui.EndCombo();
			}
		}



		/// <summary>
		/// Render a text input field
		/// </summary>
		/// <param name="label">The text to show</param>
		/// <param name="id">The id to use</param>
		/// <param name="val">The string value for the user to edit</param>
		public static void RenderTextField(string label, string id, ref string val)
		{
			ImGui.Text(label);
			ImGui.SameLine();
			ImGui.PushID(id);
			ImGui.InputText(string.Empty, ref val, 512);
			ImGui.PopID();
		}



		/// <summary>
		/// Render a number input field
		/// </summary>
		/// <param name="label">The text to show</param>
		/// <param name="id">The id to use</param>
		/// <param name="val">The integer value for the user to edit</param>
		public static void RenderIntField(string label, string id, ref int val, int min, int max)
		{
			ImGui.Text(label);
			ImGui.SameLine();
			ImGui.PushID(id);
			ImGui.InputInt(string.Empty, ref val);
			ImGui.PopID();

			if (val > max)
			{
				val = max;
			}
			else if (val < min)
			{
				val = min;
			}
		}
	}
}