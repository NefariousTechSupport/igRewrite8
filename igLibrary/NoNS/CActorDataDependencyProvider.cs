/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary
{
	public class CActorDataDependencyProvider : igDependencyProvider
	{
		public int _version = 7;
		public override void GetBuildDependencies(igObject obj, out igStringRefList? output)
		{
			CActorData actorData = (CActorData)obj;
			output = new igStringRefList();
			string path = _directory._path;
			if(_directory._path.EndsWith("_CharacterData.igz"))
			{
				string excludeRulesPath = path.ReplaceBeginning("data:/", "data:/ExcludeRules/").ReplaceEnd("_CharacterData.igz", "_rule.igz");
				output.Append(excludeRulesPath);
				return;
			}
			else if(_directory._path.StartsWith("maps:/") && !string.IsNullOrWhiteSpace(actorData._characterAnimations))
			{
				//throw new NotImplementedException();
				output.Append(string.Format("data:/anims/{0}.hka", actorData._characterAnimations));
				//string animsPath = string.Format("data:/anims/{0}+{1}.hka", actorData._characterAnimations, "this should be the zone name!");
			}

		}
	}
}