/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Core;

namespace igLibrary
{
	public class CBehaviorComponentDataDependencyProvider : igDependencyProvider
	{
		public override void GetBuildDependencies(igObject obj, out igStringRefList? output)
		{
			output = null;
			CBehaviorComponentData behavior = (CBehaviorComponentData)obj;
			if(!string.IsNullOrEmpty(behavior._behaviorFile))
			{
				output = new igStringRefList();
				string animDep = behavior._behaviorFile;
				if(animDep.StartsWith("behaviors:"))
				{
					animDep = "data:/anims" + behavior._behaviorFile.Substring(10).Replace('\\', '/');
				}
				animDep = Path.TrimEndingDirectorySeparator(animDep);
				animDep = Path.ChangeExtension(animDep, ".hka");
				//output.Append(animDep);
			}
		}
	}
}