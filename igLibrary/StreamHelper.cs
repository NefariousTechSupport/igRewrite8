using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;

namespace igLibrary
{
	//Heavily edited and renamed version of https://github.com/AdventureT/TrbMultiTool/blob/opengl/TrbMultiTool/TrbMultiTool/EndiannessAwareBinaryReader.cs

	public class StreamHelper : BinaryReader
	{
		public enum Endianness
		{
			Little,
			Big,
		}

		public Endianness _endianness = Endianness.Little;
		public byte bitPosition = 0;

		//THIS MAKES IT NOT THREAD SAFE
		private readonly byte[] _integerBuffer = new byte[8];

		public StreamHelper(byte[] input) : base(new MemoryStream(input)) {}
		public StreamHelper(Stream input) : base(input) {}
		public StreamHelper(byte[] input, Encoding encoding) : base(new MemoryStream(input), encoding) {}
		public StreamHelper(Stream input, Encoding encoding) : base(input, encoding) {}
		public StreamHelper(byte[] input, Encoding encoding, bool leaveOpen) : base(new MemoryStream(input), encoding, leaveOpen) {}
		public StreamHelper(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen) {}
		public StreamHelper(byte[] input, Endianness endianness) : base(new MemoryStream(input)) => _endianness = endianness;
		public StreamHelper(Stream input, Endianness endianness) : base(input) => _endianness = endianness;
		public StreamHelper(byte[] input, Encoding encoding, Endianness endianness) : base(new MemoryStream(input), encoding) => _endianness = endianness;
		public StreamHelper(Stream input, Encoding encoding, Endianness endianness) : base(input, encoding) => _endianness = endianness;
		public StreamHelper(byte[] input, Encoding encoding, bool leaveOpen, Endianness endianness) : base(new MemoryStream(input), encoding, leaveOpen) => _endianness = endianness;
		public StreamHelper(Stream input, Encoding encoding, bool leaveOpen, Endianness endianness) : base(input, encoding, leaveOpen) => _endianness = endianness;

		public static uint Align(uint value, uint alignment, uint basePos = 0)
		{
			return basePos + (((value - basePos + (alignment - 1)) / alignment) * alignment);
		}
		public void Align(uint alignment) => Seek(Align(Tell(), alignment, 0));
		public void Align(uint basePos, uint alignment) => Seek(Align(Tell(), alignment, basePos));
		public void Seek(ulong offset, SeekOrigin origin = SeekOrigin.Begin) => Seek((long)offset, origin);
		public void Seek(long offset, SeekOrigin origin = SeekOrigin.Begin) => BaseStream.Seek(offset, origin);
		public uint Tell() => (uint)BaseStream.Position;
		public ulong Tell64() => (ulong)BaseStream.Position;

		public override string ReadString()
		{
			List<byte> data = new List<byte>();
			while (true)
			{
				var newByte = ReadByte();
				if (newByte == 0) break;
				data.Add(newByte);
			}
			return System.Text.Encoding.UTF8.GetString(data.ToArray());
		}
		public string ReadStringUntil(char character)
		{
			var sb = new StringBuilder();
			while (true)
			{
				var newByte = ReadByte();
				if (newByte == 0 || newByte == (byte)character) break;
				sb.Append((char)newByte);
			}
			return sb.ToString();
		}
		public string ReadLine() => ReadStringUntil('\n');

		public char ReadUnicodeChar()
		{
			byte newByte = 0;
			byte newByte2 = 0;
			try
			{
				newByte = ReadByte();
				newByte2 = ReadByte();
			}
			catch (EndOfStreamException)
			{
				return (char)0x00;
			}
			if (newByte == 0 && newByte2 == 0) return (char)0x00;
			string convertedChar;

			if (_endianness == Endianness.Big) convertedChar = Encoding.Unicode.GetString(new byte[] { newByte2, newByte });
			else convertedChar = Encoding.Unicode.GetString(new byte[] { newByte, newByte2 });
			return convertedChar[0];
		}
		public string ReadUnicodeString()
		{
			var sb = new StringBuilder();
			while (true)
			{
				byte newByte;
				byte newByte2;

				try
				{
					newByte = ReadByte();
					newByte2 = ReadByte();
				}
				catch (EndOfStreamException)
				{
					break;
				}
				if (newByte == 0 && newByte2 == 0) break;
				string convertedChar;
				if (_endianness == Endianness.Big) convertedChar = Encoding.Unicode.GetString(new byte[] { newByte2, newByte });
				else convertedChar = Encoding.Unicode.GetString(new byte[] { newByte, newByte2 });
				sb.Append(convertedChar);
			}
			return sb.ToString();
		}

		public string ReadUnicodeString(uint size)
		{
			var sb = new StringBuilder();
			for (int i = 0; i < size; i++)
			{
				byte newByte;
				byte newByte2;

				try
				{
					newByte = ReadByte();
					newByte2 = ReadByte();
				}
				catch (EndOfStreamException)
				{
					break;
				}
				if (newByte == 0 && newByte2 == 0) break;
				string convertedChar;
				if (_endianness == Endianness.Big) convertedChar = Encoding.Unicode.GetString(new byte[] { newByte2, newByte });
				else convertedChar = Encoding.Unicode.GetString(new byte[] { newByte, newByte2 });
				sb.Append(convertedChar);
			}
			return sb.ToString();
		}

		public override byte ReadByte() => ReadByte((uint)BaseStream.Position);
		public byte ReadByte(uint offset)
		{
			BaseStream.Seek(offset, SeekOrigin.Begin);
			BaseStream.Read(_integerBuffer, 0, 1);
			return _integerBuffer[0];
		}
		public void WriteBoolean(bool data) => WriteBoolean(data, (uint)BaseStream.Position);
		public void WriteBoolean(bool data, uint offset) => WriteByte((byte)(data ? 1 : 0), offset);
		public void WriteByte(byte data) => WriteByte(data, (uint)BaseStream.Position);
		public void WriteByte(byte data, uint offset)
		{
			BaseStream.Seek(offset, SeekOrigin.Begin);
			_integerBuffer[0] = data;
			BaseStream.Write(_integerBuffer, 0x00, 0x01);
		}
		public void WriteSByte(sbyte data) => WriteByte(unchecked((byte)data), (uint)BaseStream.Position);
		public void WriteSByte(sbyte data, uint offset) => WriteByte(unchecked((byte)data), offset);

		public string ReadString(uint offset)
		{
			BaseStream.Seek(offset, SeekOrigin.Begin);
			return ReadString();
		}
		public void WriteString(string data, uint offset)
		{
			BaseStream.Seek(offset, SeekOrigin.Begin);
			WriteString(data);
		}
		public void WriteString(string data)
		{
			BaseStream.Write(System.Text.Encoding.ASCII.GetBytes(data));
			BaseStream.WriteByte(0x00);
		}

		public byte[] ReadBytes(uint count) => ReadBytes((int)count);
		public void WriteBytes(byte[] data)
		{
			BaseStream.Write(data);
		}
		public byte[] ReadFromOffset(int count, uint offset)
		{
			BaseStream.Seek(offset, SeekOrigin.Begin);
			return ReadBytes(count);
		}
		public unsafe object ReadStruct(Type t)
		{
			return GetType().GetMethod("ReadStruct", new Type[0]).MakeGenericMethod(t).Invoke(this, null);
		}
		public unsafe T ReadStruct<T>() where T : struct
		{
			byte[] data = ReadBytes(Marshal.SizeOf(typeof(T)));

			if((_endianness == StreamHelper.Endianness.Big && BitConverter.IsLittleEndian) || (_endianness == StreamHelper.Endianness.Little && !BitConverter.IsLittleEndian))
			{
				foreach (var field in typeof(T).GetFields())
				{
					var fieldType = field.FieldType;
					if (field.IsStatic)
						// don't process static fields
						continue;

					if (fieldType == typeof(string))
						// don't swap bytes for strings
						continue;

					var offset = Marshal.OffsetOf(typeof(T), field.Name).ToInt32();

					// handle enums
					if (fieldType.IsEnum)
						fieldType = Enum.GetUnderlyingType(fieldType);

					// check for sub-fields to recurse if necessary
					var subFields = fieldType.GetFields().Where(subField => subField.IsStatic == false).ToArray();

					var effectiveOffset = offset;

					if (subFields.Length == 0)
					{
						Array.Reverse(data, effectiveOffset, Marshal.SizeOf(fieldType));
					}
				}
				if(typeof(T).IsPrimitive)
				{
					Array.Reverse(data, 0, data.Length);
				}
			}

			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			T read = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
			handle.Free();

			return read;
		}
		public unsafe object ReadStructArray(Type t, uint count)
		{
			return GetType().GetMethod("ReadStructArray", new Type[1]{typeof(uint)}).MakeGenericMethod(t).Invoke(this, new object[1]{count});
		}
		public T[] ReadStructArray<T>(uint count) where T : struct
		{
			T[] items = new T[count];
			for(uint i = 0; i < count; i++)
			{
				items[i] = ReadStruct<T>();
			}
			return items;
		}
		public unsafe void WriteStruct<T>(T data)
		{
			byte[] bytes = new byte[Marshal.SizeOf<T>()];
			IntPtr ptr = IntPtr.Zero;
			ptr = Marshal.AllocHGlobal(Marshal.SizeOf<T>());
			Marshal.StructureToPtr(data, ptr, false);
			Marshal.Copy(ptr, bytes, 0, bytes.Length);
			Marshal.FreeHGlobal(ptr);

			if((_endianness == StreamHelper.Endianness.Big && BitConverter.IsLittleEndian) || (_endianness == StreamHelper.Endianness.Little && !BitConverter.IsLittleEndian))
			{
				foreach (var field in typeof(T).GetFields())
				{
					var fieldType = field.FieldType;
					if (field.IsStatic)
						// don't process static fields
						continue;

					if (fieldType == typeof(string))
						// don't swap bytes for strings
						continue;

					var offset = Marshal.OffsetOf(typeof(T), field.Name).ToInt32();

					// handle enums
					if (fieldType.IsEnum)
						fieldType = Enum.GetUnderlyingType(fieldType);

					// check for sub-fields to recurse if necessary
					var subFields = fieldType.GetFields().Where(subField => subField.IsStatic == false).ToArray();

					var effectiveOffset = offset;

					if (subFields.Length == 0)
					{
						Array.Reverse(bytes, effectiveOffset, Marshal.SizeOf(fieldType));
					}
				}
				if(typeof(T).IsPrimitive)
				{
					Array.Reverse(bytes, 0, bytes.Length);
				}
			}

			WriteBytes(bytes);
		}
		public override float ReadSingle() => ReadSingle(_endianness);
		public float ReadSingle(Endianness endianness) => BitConverter.ToSingle(ReadForEndianness(sizeof(float), endianness), 0);
		public void WriteSingle(float data) => WriteSingle(data, _endianness);
		public void WriteSingle(float data, Endianness endianness) => WriteForEndianness(BitConverter.GetBytes(data), endianness);
		public override double ReadDouble() => ReadDouble(_endianness);
		public double ReadDouble(Endianness endianness) => BitConverter.ToSingle(ReadForEndianness(sizeof(float), endianness), 0);
		public void WriteDouble(double data) => WriteDouble(data, _endianness);
		public void WriteDouble(double data, Endianness endianness) => WriteForEndianness(BitConverter.GetBytes(data), endianness);
		public override Half ReadHalf() => ReadHalf(_endianness);
		public Half ReadHalf(Endianness endianness) => BitConverter.ToHalf(ReadForEndianness(2, endianness), 0);

		public override short ReadInt16() => ReadInt16(_endianness);
		public short ReadInt16(uint offset) => ReadInt16(offset, _endianness);
		public short ReadInt16(uint offset, Endianness endianness)
		{
			BaseStream.Seek(offset, SeekOrigin.Begin);
			return ReadInt16(endianness);
		}

		public override int ReadInt32() => ReadInt32(_endianness);
		public int ReadInt32(uint offset) => ReadInt32(offset, _endianness);
		public int ReadInt32(uint offset, Endianness endianness)
		{
			BaseStream.Seek(offset, SeekOrigin.Begin);
			return ReadInt32(endianness);
		}

		public override ushort ReadUInt16() => ReadUInt16(_endianness);
		public ushort ReadUInt16(uint offset) => ReadUInt16(offset, _endianness);
		public ushort ReadUInt16(uint offset, Endianness endianness)
		{
			BaseStream.Seek(offset, SeekOrigin.Begin);
			return ReadUInt16(endianness);
		}

		public override uint ReadUInt32() => ReadUInt32(_endianness);
		public uint ReadUInt32(uint offset) => ReadUInt32(offset, _endianness);
		public uint ReadUInt32(uint offset, Endianness endianness)
		{
			BaseStream.Seek(offset, SeekOrigin.Begin);
			return ReadUInt32(endianness);
		}

		public ulong ReadUIntN(byte n) => ReadUIntN((uint)BaseStream.Position, bitPosition, n);
		public ulong ReadUIntN(uint offset, byte n) => ReadUIntN(offset, bitPosition, n);
		public ulong ReadUIntN(uint offset, byte bitOffset, byte n)
		{
			if(n > 64) throw new ArgumentException("The number of bits to read cannot be greater than 64.");
			if(bitOffset > 7) throw new ArgumentException("The bit offset cannot be equal to or greater than 8.");

			ulong ret = 0;
			for(int i = 0; i < n; i++)
			{
				bool bit = ReadBit();
				ret |= bit ? 1u : 0u;
				ret <<= 1;
			}
			ret >>= 1;
			return ret;
		}
		public long ReadIntN(byte n) => ReadIntN((uint)BaseStream.Position, bitPosition, n);
		public long ReadIntN(uint offset, byte n) => ReadIntN(offset, bitPosition, n);
		public long ReadIntN(uint offset, byte bitOffset, byte n)
		{
			ulong unsignedn = ReadUIntN(offset, bitOffset, n);
			long signedn = (long)(unsignedn & ~(1u << n));
			signedn *= (unsignedn & (1u << n)) != 0 ? -1 : 1;
			return signedn;
		}
		public bool ReadBit() => ReadBit((uint)BaseStream.Position, bitPosition);
		public bool ReadBit(uint offset, byte bitOffset)
		{
			Seek(offset);
			bitPosition = bitOffset;
			if(bitPosition > 0) BaseStream.Position--;
			byte currentByte = ReadByte();
			int bit = (currentByte >> (7 - bitPosition)) & 1;
			bitPosition++;
			bitPosition %= 8;
			return bit == 1;
		}
		public ulong ReadUInt64(uint offset) => ReadUInt64(_endianness, offset);
		public override ulong ReadUInt64() => ReadUInt64(_endianness, BaseStream.Position);

		public short ReadInt16(Endianness endianness) => BitConverter.ToInt16(ReadForEndianness(sizeof(short), endianness), 0);

		public int ReadInt32(Endianness endianness) => BitConverter.ToInt32(ReadForEndianness(sizeof(int), endianness), 0);

		public override long ReadInt64() => ReadInt64(_endianness);
		public long ReadInt64(Endianness endianness) => BitConverter.ToInt64(ReadForEndianness(sizeof(long), endianness), 0);

		public ushort ReadUInt16(Endianness endianness) => BitConverter.ToUInt16(ReadForEndianness(sizeof(ushort), endianness), 0);

		public uint ReadUInt32(Endianness endianness) => BitConverter.ToUInt32(ReadForEndianness(sizeof(uint), endianness), 0);

		public ulong ReadUInt64(Endianness endianness) => ReadUInt64(endianness, BaseStream.Position);
		public ulong ReadUInt64(Endianness endianness, long offset)
		{
			Seek(offset);
			return BitConverter.ToUInt64(ReadForEndianness(sizeof(ulong), endianness), 0);
		}
		public void WriteUInt16(ushort data) => WriteUInt16(data, _endianness);
		public void WriteUInt16(ushort data, Endianness endianness) => WriteForEndianness(BitConverter.GetBytes(data), endianness);
		public void WriteInt16(short data) => WriteInt16(data, _endianness);
		public void WriteInt16(short data, Endianness endianness) => WriteForEndianness(BitConverter.GetBytes(data), endianness);
		public void WriteUInt32(uint data) => WriteUInt32(data, _endianness);
		public void WriteUInt32(uint data, Endianness endianness) => WriteForEndianness(BitConverter.GetBytes(data), endianness);
		public void WriteInt32(int data) => WriteInt32(data, _endianness);
		public void WriteInt32(int data, Endianness endianness) => WriteForEndianness(BitConverter.GetBytes(data), endianness);
		public void WriteUInt64(ulong data) => WriteUInt64(data, _endianness);
		public void WriteUInt64(ulong data, Endianness endianness) => WriteForEndianness(BitConverter.GetBytes(data), endianness);
		public void WriteInt64(long data) => WriteInt64(data, _endianness);
		public void WriteInt64(long data, Endianness endianness) => WriteForEndianness(BitConverter.GetBytes(data), endianness);

		private byte[] ReadForEndianness(int bytesToRead, Endianness endianness)
		{
			if(bytesToRead > _integerBuffer.Length) throw new ArgumentException($"read size must be less than {_integerBuffer.Length}");
			int res = BaseStream.Read(_integerBuffer, 0, bytesToRead);
			if(res != bytesToRead)
			{
				throw new Exception($"Read {res} instead of {bytesToRead}");
			}
			switch (endianness)
			{
				case Endianness.Little:
					if (!BitConverter.IsLittleEndian)
					{
						Array.Reverse(_integerBuffer, 0, bytesToRead);
					}
					break;

				case Endianness.Big:
					if (BitConverter.IsLittleEndian)
					{
						Array.Reverse(_integerBuffer, 0, bytesToRead);
					}
					break;
			}

			return _integerBuffer;
		}
		
		private void WriteForEndianness(byte[] bytes, Endianness endianness)
		{
			switch (endianness)
			{
				case Endianness.Little:
					if (!BitConverter.IsLittleEndian)
					{
						Array.Reverse(bytes);
					}
					break;

				case Endianness.Big:
					if (BitConverter.IsLittleEndian)
					{
						Array.Reverse(bytes);
					}
					break;
			}
			BaseStream.Write(bytes);
		}
	}
}