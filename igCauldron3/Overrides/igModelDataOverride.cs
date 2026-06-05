/*
	Copyright (c) 2022-2026, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Render;
using igLibrary.Core;
using ImGuiNET;
using System.Diagnostics;

namespace igCauldron3
{
	/// <summary>
	/// UI override for rendering igModelData
	/// </summary>
	public class igModelDataOverride : InspectorDrawOverride
	{
		private static bool _initialised = false;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
		private static igMetaField _field_materialHandle;
		private static igMetaField _field_lod;
		private static igMetaField _field_enabled;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.


		/// <summary>
		/// Constructor
		/// </summary>
		public igModelDataOverride()
		{
			_t = typeof(igModelData);
		}


		/// <summary>
		/// Sets up the metafields used by this override
		/// </summary>
		private static void SetupMetaFields()
		{
			if (_initialised)
			{
				return;
			}

			igMetaObject? drawCallMeta = igArkCore.GetObjectMeta(nameof(igModelDrawCallData));

			Debug.Assert(drawCallMeta != null);

			_field_materialHandle = drawCallMeta.GetFieldByName(nameof(igModelDrawCallData._materialHandle))!;
			_field_lod            = drawCallMeta.GetFieldByName(nameof(igModelDrawCallData._lod))!;
			_field_enabled        = drawCallMeta.GetFieldByName(nameof(igModelDrawCallData._enabled))!;

			Debug.Assert(_field_materialHandle != null);
			Debug.Assert(_field_lod            != null);
			Debug.Assert(_field_enabled        != null);

			_initialised = true;
		}



		/// <summary>
		/// Renders the ui
		/// </summary>
		/// <param name="dirFrame">The directory manager frame</param>
		/// <param name="id">the id to render with</param>
		/// <param name="obj">the object</param>
		/// <param name="meta">the type of the object</param>
		public override void Draw2(DirectoryManagerFrame dirFrame, string id, igObject obj, igMetaObject meta)
		{
			igModelData modelData = (igModelData)obj;

			SetupMetaFields();

			if (ImGui.TreeNode("Draw Calls"))
			{
				for (int d = 0; d < modelData._drawCalls._count; d++)
				{
					igModelDrawCallData drawCallData = modelData._drawCalls[d];

					if (ImGui.TreeNode($"Draw Call {d}"))
					{
						RenderField(id, drawCallData, _field_materialHandle);
						RenderField(id, drawCallData, _field_lod);
						RenderField(id, drawCallData, _field_enabled);

						ImGui.TreePop();
					}
				}
				ImGui.TreePop();
			}
		}
	}
}