/*
	Copyright (c) 2022-2026, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Graphics
{
	public class igMemoryCommandStream : igCommandStream
	{
		public igMemory<byte> _memory;
		public uint _bytesWritten;


		public override void ReadIGZFields(igIGZLoader loader)
		{
			base.ReadIGZFields(loader);

			StreamHelper stream = new StreamHelper(_memory.Buffer, loader._stream._endianness);
			DecodeIGZ(loader._platform, stream);
		}


		public override void WriteIGZFields(igIGZSaver saver, igIGZSaver.SaverSection section)
		{
			MemoryStream ms = new MemoryStream();
			StreamHelper stream = new StreamHelper(ms);

			EncodeIGZ(saver._platform, stream);

			_memory = new igMemory<byte>(section._pool, (uint)ms.Length);
			Array.Copy(ms.GetBuffer(), _memory.Buffer, (uint)ms.Length);

			base.WriteIGZFields(saver, section);
		}
	}
}