using System.Text;

namespace igLibrary.Gfx
{
	public class igMetaImageInfo : igObject
	{
		private static Lazy<igMetaImageInfo> _metaImageInfoLazy = new Lazy<igMetaImageInfo>(() => new igMetaImageInfo());
		public static igMetaImageInfo _metaImageInfo => _metaImageInfoLazy.Value;
		public igStringMetaImageHashTable _metaImagesTable = new igStringMetaImageHashTable() { _autoRehash = true };
		private igObjectDirectory? _metaimageDirInternal = null;
		public igMetaImageList _metaImages = new igMetaImageList();
		public igObjectDirectory _metaimageDir {
			get
			{
				if(_metaimageDirInternal != null) return _metaimageDirInternal;
				_metaimageDirInternal = igObjectStreamManager.Singleton.Load("metaimages");
				return _metaimageDirInternal;
			}
		}
		public static void RegisterFormat(igMetaImage metaimage)
		{
			_metaImageInfo._metaImagesTable.Add(metaimage._name, metaimage);
			_metaImageInfo._metaimageDir.AddObject(metaimage, default, new igName(metaimage._name));
			_metaImageInfo._metaImages.Append(metaimage);
		}
		public static igMetaImage? FindFormat(string name)
		{
			uint hash = igHash.Hash(name);
			for(int i = 0; i < _metaImageInfo._metaimageDir._nameList._count; i++)
			{
				if(_metaImageInfo._metaimageDir._nameList[i]._hash == hash) return _metaImageInfo._metaimageDir._objectList[i] as igMetaImage;
			}
			return null;
		}
		public static void Debug()
		{
#if DEBUG
			StringBuilder sb = new StringBuilder();
			igObjectDirectory dir = igObjectStreamManager.Singleton.Load("metaimages");
			for(int i = 0; i < dir._objectList._count; i++)
			{
				sb.Append(dir._nameList[i]._hash.ToString("X08"));
				sb.Append(' ');
				sb.Append(dir._nameList[i]._string);
				sb.Append('\n');
			}
			File.WriteAllText("metaimages.debug.log", sb.ToString());
#endif
		}
	}
	public class igStringMetaImageHashTable : igTUHashTable<igMetaImage, string> {}
}