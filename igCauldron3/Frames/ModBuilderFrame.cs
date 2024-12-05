using igLibrary.Core;
using igLibrary.Mods;
using ImGuiNET;

namespace igCauldron3
{
	/// <summary>
	/// UI Window for building mod files (potions)
	/// </summary>
	public class ModBuilderFrame : Frame
	{
		ModManifest? _manifest;

		/// <summary>
		/// Constructor for mod builder frame
		/// </summary>
		/// <param name="wnd">The parent window</param>
		public ModBuilderFrame(Window wnd) : base(wnd)
		{
		}


		/// <summary>
		/// Render method, called once per frame
		/// </summary>
		public override void Render()
		{
			ImGui.Begin("Mod Builder");

			igArkCore.EGame game = _manifest == null ? igArkCore.EGame.EV_None : _manifest.Game;

			if (ImGui.BeginCombo("Game", "Select game..."))
			{
				for (int g = 0; g < Names.GameNames.Length; g++)
				{
					if (ImGui.Selectable(Names.GameNames[g].Item2, game == Names.GameNames[g].Item1))
					{
						game = Names.GameNames[g].Item1;
						_manifest = ModManifest.Create(game);
					}
				}
				ImGui.EndCombo();
			}

			if (_manifest is AlchemyLaboratoryModManifest alchemyManifest)
			{
				RenderAlchemyModManifest(alchemyManifest);
			}

			if (ImGui.Button("Close"))
			{
				Close();
			}

			ImGui.End();
		}

		private void RenderAlchemyModManifest(AlchemyLaboratoryModManifest manifest)
		{
			ImGui.BeginTable("$modifiedList$", 1, ImGuiTableFlags.Borders);

			ImGui.TableSetupColumn("Modified Files");
			ImGui.TableHeadersRow();
			ImGui.TableNextRow();
			ImGui.TableSetColumnIndex(0);

			for (int i = 0; i < manifest.ModifiedFiles.Count; i++)
			{
				AlchemyLaboratoryModManifest.ModifiedFile file = manifest.ModifiedFiles[i];

				ImGui.PushID(i);

				if (ImGui.Button("-"))
				{
					manifest.ModifiedFiles.RemoveAt(i);
				}

				ImGui.SameLine();

				bool changed = ImGui.InputText(string.Empty, ref file._path, 0x100);
				if (changed)
				{
					manifest.ModifiedFiles[i] = file;
				}

				ImGui.PopID();
				
				ImGui.TableNextColumn();
			}

			if (ImGui.Button("+"))
			{
				manifest.ModifiedFiles.Add(new AlchemyLaboratoryModManifest.ModifiedFile());
			}

			ImGui.EndTable();
		}
	}
}