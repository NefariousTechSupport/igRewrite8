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


		/// <summary>
		/// Decode the command stream with the graphics objects
		/// </summary>
		/// <param name="graphicsObjects">The graphics objects</param>
		public void Decode(igGraphicsObjectSet graphicsObjects)
		{
			IG_CORE_PLATFORM platform = igRegistry.GetRegistry()._platform;
			StreamHelper stream = new StreamHelper(_memory.Buffer, igAlchemyCore.isPlatformBigEndian(platform) ? StreamHelper.Endianness.Big : StreamHelper.Endianness.Little);
			DecodeIGZ(platform, stream, graphicsObjects);
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

	public class igMemoryCommandStreamList : igTObjectList<igMemoryCommandStream>{}
}