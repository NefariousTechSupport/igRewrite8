/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary
{
	public class CBuildDependencyAttribute : igBaseDependenciesAttribute
	{
		public string _value;
		public bool _replaceExtension;
		public void GenerateBuildDependancies(igIGZSaver saver, object value)
		{
			string[] depStrings = _value.Split(';');
			for(int i = 0; i < depStrings.Length; i++)
			{
				string cSharpValue = depStrings[i].Replace("%s", "{0}");
				string depName = string.Format(cSharpValue, value).Replace('\\', '/');
				saver.AddBuildDependency(depName);
			}
		}
	}
}