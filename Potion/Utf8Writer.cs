/*
	Copyright (c) 2022-2025, The Potion Contributors.
	Potion and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Text;

namespace Potion
{
	internal class Utf8Writer : StringWriter
	{
		public override Encoding Encoding => Encoding.UTF8;
	}
}