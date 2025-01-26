/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igCauldron3.Conversion;
using igCauldron3.Utils;
using igLibrary;
using igLibrary.Core;
using igLibrary.Gfx;
using igLibrary.Graphics;
using ImGuiNET;

namespace igCauldron3
{
	/// <summary>
	/// UI override for rendering hash tables
	/// </summary>
	public class igImage2Override : InspectorDrawOverride
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public igImage2Override()
		{
			_t = typeof(igImage2);
		}


		/// <summary>
		/// Renders the ui
		/// </summary>
		/// <param name="dirFrame">The directory manager frame</param>
		/// <param name="id">the id to render with</param>
		/// <param name="obj">the object</param>
		/// <param name="meta">the type of the object</param>
		public unsafe override void Draw2(DirectoryManagerFrame dirFrame, string id, igObject obj, igMetaObject meta)
		{
			igImage2 image = (igImage2)obj;
			
			if(image._texHandle == -1)
			{
				if(image._format == null)
				{
					Logging.Error("{0} has Unsupported Texture Format", image._name);
					return;
				}

				igTContext<igBaseGraphicsDevice>._instance.CreateTexture(igResourceUsage.kUsageDefault, image);
			}

			object? castName = image._name;
			FieldRenderer.RenderField(id, "_name", castName, meta.GetFieldByName("_name")!, (value) => image._name = (string)value!);
			if(ImGui.Button("Extract"))
			{
				string defaultFile = Path.ChangeExtension(Path.GetFileName(image._name), ".png");
				string? filePath = CrossFileDialog.SaveFile("Save Image...", ".bmp;.dds;.gif;.jpg;.pbm;.png;.qoi;.tga;.tiff;.webp", defaultFile);
				if(!string.IsNullOrWhiteSpace(filePath))
				{
					FileStream fs = File.Create(filePath);
					string ext = Path.GetExtension(filePath);
					if(ext == ".dds") igImage2Exporter.ExportToDds(image, fs);
					else              TextureConversion.Export(image, fs, ext);
					fs.Close();
				}
			}
			if(ImGui.Button("Replace"))
			{
				string defaultFile = Path.ChangeExtension(Path.GetFileName(image._name), ".png");
				string? filePath = CrossFileDialog.OpenFile("Open Image...", ".bmp;.dds;.gif;.jpg;.pbm;.png;.qoi;.tga;.tiff;.webp", defaultFile);
				if(!string.IsNullOrWhiteSpace(filePath))
				{
					FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read);
					TextureConversion.Import(image, fs);
					fs.Close();

					igTContext<igBaseGraphicsDevice>._instance.FreeTexture(image._texHandle);
					image._texHandle = -1;
					igTContext<igBaseGraphicsDevice>._instance.CreateTexture(igResourceUsage.kUsageDefault, image);
				}
			}
			ImGui.Image((IntPtr)image._texHandle, new System.Numerics.Vector2(image._width, image._height), System.Numerics.Vector2.UnitY, System.Numerics.Vector2.UnitX);
		}
	}
}