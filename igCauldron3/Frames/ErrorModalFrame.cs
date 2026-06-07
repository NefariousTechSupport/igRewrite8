/*
	Copyright (c) 2022-2026, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using ImGuiNET;

namespace igCauldron3
{
	/// <summary>
	/// Renders a popup modal
	/// </summary>
	public sealed class ErrorModalFrame : Frame
	{
		private string _title;
		private string _body;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="wnd">Window to parent it to</param>
		public ErrorModalFrame(Window wnd, string title, string body) : base(wnd)
		{
			_title = title;
			_body  = body;
		}


		/// <summary>
		/// Renders the popup
		/// </summary>
		public override void Render()
		{
			ImGui.OpenPopup(_title);

			System.Numerics.Vector2 centre = ImGui.GetMainViewport().GetCenter();
			ImGui.SetNextWindowPos(centre, ImGuiCond.Appearing, System.Numerics.Vector2.One * 0.5f);
			bool temp = true;
			if (ImGui.BeginPopupModal(_title, ref temp, ImGuiWindowFlags.AlwaysAutoResize))
			{
				ImGui.Text(_body);

				if (ImGui.Button("Close"))
				{
					ImGui.CloseCurrentPopup();
					Close();
				}

				ImGui.EndPopup();
			}
		}
	}
}