using igCauldron3.Conversion;
using igCauldron3.Graphics;
using igCauldron3.Utils;
using igLibrary;
using igLibrary.Core;
using igLibrary.Gfx;
using igLibrary.Graphics;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;

namespace igCauldron3
{
	public class igImage2Override : InspectorDrawOverride
	{
		public igImage2Override()
		{
			_t = typeof(igImage2);
		}

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
				string filePath = CrossFileDialog.SaveFile("Save Image...", ".bmp;.dds;.gif;.jpg;.pbm;.png;.qoi;.tga;.tiff;.webp", image._name);
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
				string filePath = CrossFileDialog.OpenFile("Open Image...", ".bmp;.dds;.gif;.jpg;.pbm;.png;.qoi;.tga;.tiff;.webp", image._name);
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