using System.Reflection;

namespace igLibrary.Core
{
	/// <summary>
	/// Represents the metadata for a field
	/// </summary>
	public class igMetaField : igObject
	{
		/// <summary>
		/// Different ways to copy a field when copying an object
		/// </summary>
		public enum CopyType
		{
			kCopyByValue,
			kCopyByReference,
			kCopyByNoCopy,
			kCopyByDefault,
			kCopyTypeMax
		}


		/// <summary>
		/// Different ways to reset a field when resetting an object
		/// </summary>
		public enum ResetType
		{
			kResetByValue,
			kResetByReference,
			kResetByNoReset,
			kResetByDefault,
			kResetTypeMax
		}


		/// <summary>
		/// Different ways to check for equality in the same field between two objects
		/// </summary>
		public enum IsAlikeCompareType
		{
			kIsAlikeCompareValue,
			kIsAlikeCompareReference,
			kIsAlikeCompareNoCompare,
			kIsAlikeCompareDefault,
			kIsAlikeCompareTypeMax
		}


		/// <summary>
		/// Decoded properties about the field, alchemy stores this as a bitfield
		/// </summary>
		public struct Properties
		{
			public CopyType _copyMethod;
			public ResetType _resetMethod;
			public IsAlikeCompareType _isAlikeCompareMethod;
			public CopyType _itemsCopyMethod;
			public CopyType _keysCopyMethod;
			public bool _persistent;
			public bool _hasInvariance;
			public bool _hasPoolName;
			public bool _mutable;
			public bool _implicitAlignment;


			/// <summary>
			/// Constructor for igMetaField properties
			/// </summary>
			public Properties()
			{
				_copyMethod = CopyType.kCopyByDefault;
				_resetMethod = ResetType.kResetByDefault;
				_isAlikeCompareMethod = IsAlikeCompareType.kIsAlikeCompareDefault;
				_itemsCopyMethod = CopyType.kCopyByDefault;
				_keysCopyMethod = CopyType.kCopyByDefault;
				_persistent = true;
				_hasInvariance = false;
				_hasPoolName = false;
				_mutable = false;
				_implicitAlignment = true;
			}

			//I'm so sorry
			internal int getArkStorage() => (int)_copyMethod | (int)_resetMethod << 2 | (int)_isAlikeCompareMethod << 4 | (int)_itemsCopyMethod << 6 | (int)_keysCopyMethod << 8 | bts(_persistent, 10) | bts(_hasInvariance, 11) | bts(_hasPoolName, 12) | bts(_mutable, 13) | bts(_implicitAlignment, 14);
			private int bts(bool value, byte shift) => (value ? 1 : 0) << shift;
			private bool stb(int value, byte shift) => ((value >> shift) & 1) != 0;
			internal void setArkStorage(int value)
			{
				_copyMethod = (CopyType)((value >> 0) & 3);
				_resetMethod = (ResetType)((value >> 2) & 3);
				_isAlikeCompareMethod = (IsAlikeCompareType)((value >> 4) & 3);
				_itemsCopyMethod = (CopyType)((value >> 6) & 3);
				_keysCopyMethod = (CopyType)((value >> 8) & 3);
				_persistent = stb(value, 10);
				_hasInvariance = stb(value, 11);
				_hasPoolName = stb(value, 12);
				_mutable = stb(value, 13);
				_implicitAlignment = stb(value, 14);
			}
		}
		public string? _fieldName;
		public ushort _offset;
		public Dictionary<IG_CORE_PLATFORM, ushort> _offsets = new Dictionary<IG_CORE_PLATFORM, ushort>();
		public igBaseMeta _parentMeta;
		public Properties _properties = new Properties();
		public igObjectList? _attributes = new igObjectList();
		public object? _default;
		public FieldInfo? _fieldHandle;
		[Obsolete("This exists for the reflection system, use _parentMeta instead.")] public int _parentMetaObjectIndex = -1;
		[Obsolete("This exists for the reflection system, do not use.")] public int _typeIndex = -1;
		[Obsolete("This exists for the reflection system, do not use.")] public int _internalIndex = -1;
		[Obsolete("This exists for the reflection system, use GetSize() instead.")] public ushort _size = 0;

		public virtual bool IsArray => false;
		public virtual int ArrayNum => throw new NotSupportedException($"Fields of type \"{GetType().FullName}\" do not support array based operations");

		/// <summary>
		/// Returns all attributes of the given type or derived types.
		/// </summary>
		/// <typeparam name="T">The type of attribute to search for</typeparam>
		/// <returns>An array of all attributes matching the criteria</returns>
		public T[] GetAttributes<T>() where T : igObject
		{
			List<T> attrs = new List<T>();
			if(_attributes == null) return attrs.ToArray();
			for(int i = 0; i < _attributes._count; i++)
			{
				if(_attributes[i] is T attr) attrs.Add(attr);
			}
			return attrs.ToArray();
		}


		/// <summary>
		/// Returns the first attribute of the given type or derived types.
		/// </summary>
		/// <typeparam name="T">The type of attribute to search for</typeparam>
		/// <returns>The first attribute matching the criteria, or null if there is none</returns>
		public T? GetAttribute<T>() where T : igObject
		{
			if(_attributes == null) return null;
			for(int i = 0; i < _attributes._count; i++)
			{
				if(_attributes[i] is T attr) return attr;
			}
			return null;
		}


		/// <summary>
		/// Whether or not this field applies to a given platform.
		/// </summary>
		/// <param name="platform">The platform to check against</param>
		/// <returns>Whether or not this field applies to the given platform</returns>
		public bool IsApplicableForPlatform(IG_CORE_PLATFORM platform)
		{
			if(_attributes == null) return true;
			bool foundPlatformAttribute = false;
			for(int i = 0; i < _attributes._count; i++)
			{
				if(_attributes[i] is igPlatformExclusiveAttribute exclusiveAttr)
				{
					foundPlatformAttribute = true;
					if(exclusiveAttr._value == platform) return true;
				}
				else if(_attributes[i] is igPlatformExclusionAttribute exclusionAttr)
				{
					if(exclusionAttr._value == platform) return false;
				}
			}
			return !foundPlatformAttribute;
		}


		/// <summary>
		/// Dumps this field's metadata to an igArkCoreFile
		/// </summary>
		/// <param name="saver">The igArkCoreFile to save to</param>
		/// <param name="sh">The section to write the metadata to</param>
		public virtual void DumpArkData(igArkCoreFile saver, StreamHelper sh)
		{
			if(this is igPlaceHolderMetaField placeholder)
			{
				saver.SaveString(sh, placeholder._platformInfo._name);
			}
			else
			{
				saver.SaveString(sh, GetType().Name);
			}
			sh.WriteInt32(_properties.getArkStorage());
			uint templateParameterCount = GetTemplateParameterCount();
			sh.WriteUInt32(templateParameterCount);
			for(uint i = 0; i < templateParameterCount; i++)
			{
				igMetaField? templateParam = GetTemplateParameter(i);
				saver.SaveMetaField(sh, templateParam);
			}
			saver.SaveString(sh, _fieldName == "0" ? null : _fieldName);

			FieldInfo? numField = GetType().GetField("_num");
			if(numField != null)
			{
				short num = (short)numField.GetValue(this);
				sh.WriteInt16(num);
			}
			else
			{
				sh.WriteInt16((short)-1);
			}
			sh.WriteUInt16(_offset);

			saver.SaveAttributes(sh, _attributes);
		}


		/// <summary>
		/// Dump the default value of this field to the igArkCoreFile
		/// </summary>
		/// <param name="saver">The igArkCoreFile to save to</param>
		/// <param name="sh">The section to write the default value to</param>
		public virtual void DumpDefault(igArkCoreFile saver, StreamHelper sh){}


		/// <summary>
		/// Read the metadata of this field from the igArkCoreFile
		/// </summary>
		/// <param name="loader">The igArkCoreFile to load from</param>
		/// <param name="sh">The section to read the metadata from</param>
		public virtual void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			// Read the properties bitfield
			_properties.setArkStorage(sh.ReadInt32());

			// Process template arguments
			uint templateArgCount = sh.ReadUInt32();
			SetTemplateParameterCount(templateArgCount);
			for(uint i = 0; i < templateArgCount; i++)
			{
				SetTemplateParameter(i, loader.ReadMetaField(sh));
			}

			// Read the field name
			_fieldName = loader.ReadString(sh);

			// Read array metafield properties
			short num = sh.ReadInt16();
			_offset = sh.ReadUInt16();
			FieldInfo? numField = GetType().GetField("_num");
			if(numField != null)
			{
				numField.SetValue(this, num);
			}

			_attributes = loader.ReadAttributes(sh);
		}


		/// <summary>
		/// Read the default value of the field from the igArkCoreFile
		/// </summary>
		/// <param name="loader">The igArkCoreFile to read the default from</param>
		/// <param name="sh">The section of the file to read the default from</param>
		public virtual void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			// Subtract 4 because the size was previously read to determine whether this is needed
			sh.BaseStream.Position -= 4;
			int size = sh.ReadInt32();
			sh.BaseStream.Position += size;
		}

		/// <summary>
		/// Reads a field from an IGZ file
		/// </summary>
		/// <param name="loader">the IGZ to read the data from, at the correct offset</param>
		/// <returns>The value of the field</returns>
		public virtual object? ReadIGZField(igIGZLoader loader) => throw new NotImplementedException();


		/// <summary>
		/// Writes a field to an IGZ file
		/// </summary>
		/// <param name="saver">The IGZ to write the data to</param>
		/// <param name="section">The section of the igz to write the data to, at the correct offset</param>
		/// <param name="value">The value to write</param>
		public virtual void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value) => throw new NotImplementedException();


		/// <summary>
		/// The dotnet type of the field
		/// </summary>
		/// <returns>The type of the field</returns>
		public virtual Type GetOutputType() => typeof(object);


		/// <summary>
		/// The size of the field (in bytes) for a given platform
		/// </summary>
		/// <param name="platform">The platform in question</param>
		/// <returns>An unsigned integer representing how big the field is in bytes</returns>
		public virtual uint GetSize(IG_CORE_PLATFORM platform) => throw new NotImplementedException();


		/// <summary>
		/// The alignment of the field (in bytes) for a given platform
		/// </summary>
		/// <param name="platform">The platfomr in question</param>
		/// <returns>An unsigned integer represnting the alignment of the field in bytes</returns>
		public virtual uint GetAlignment(IG_CORE_PLATFORM platform) => throw new NotImplementedException();


		/// <summary>
		/// Set a template parameter for this field
		/// </summary>
		/// <param name="index">Which template parameter to set</param>
		/// <param name="meta">The metafield to set it to</param>
		public virtual void SetTemplateParameter(uint index, igMetaField meta) => throw new NotImplementedException();


		/// <summary>
		/// Set the number of template parameters
		/// </summary>
		/// <param name="count">The number of template parameters</param>
		public virtual void SetTemplateParameterCount(uint count) {}


		/// <summary>
		/// Get a template parameter
		/// </summary>
		/// <param name="index">Which template parameter to get</param>
		/// <returns>The desired template parameter</returns>
		public virtual igMetaField? GetTemplateParameter(uint index) => throw new NotImplementedException();


		/// <summary>
		/// Get the number of template parameters
		/// </summary>
		/// <returns>The number of template parameters</returns>
		public virtual uint GetTemplateParameterCount() => 0;


		/// <summary>
		/// Constructs a default value for this field
		/// </summary>
		/// <param name="pool">The memory pool to construct the field for</param>
		/// <returns>The default value</returns>
		public virtual object? GetDefault(igMemoryPool pool)
		{
			FieldInfo? fi = GetType().GetField("_num");
			if(fi != null)
			{
				short num = (short)fi.GetValue(this)!;
				Array arrayDefault = Array.CreateInstance(GetOutputType().GetElementType()!, num);
				if(_default != null)
				{
					for(int i = 0; i < num; i++)
					{
						arrayDefault.SetValue(_default, i);
					}
				}
				return arrayDefault;
			}
			else return _default;
		}


		/// <summary>
		/// Create a shallow copy of this field
		/// </summary>
		/// <returns>A shallow copy of this field</returns>
		public virtual igMetaField CreateFieldCopy() => (igMetaField)this.MemberwiseClone();
	}


	/// <summary>
	/// Represents a list of igMetaFields
	/// </summary>
	public class igMetaFieldList : igTObjectList<igMetaField>{}
}