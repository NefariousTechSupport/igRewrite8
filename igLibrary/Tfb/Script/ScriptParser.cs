using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igLibrary.Tfb.Script
{ // thing to read macros + structs into dictionaries before reading/saving scripts
    public class ScriptParser
    {
        /// <summary>
        /// Dictionary of (struct/macros, macro/struct name), each containing (variable, variable name)
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static Dictionary<List<OpAbstractCreateVariable>, string> ReadDependencies(igObjectDirectory dir)
        {
            var macrosAndStructs = new Dictionary<List<OpAbstractCreateVariable>, string>();
            for (int i = 0; i < dir._objectList._count; i++)
            {
                if (dir._objectList[i] is tfbScriptInfo scriptinfo)
                {
                    for (int j = 0; j < scriptinfo._opList._count; j++)
                    {
                        if (scriptinfo._opList[j] is OpDefineStructure opdefs)
                        {
                            var definestruct = new List<OpAbstractCreateVariable>();
                            for (int c = j + 1; c < j + opdefs._branchPC; c++)
                            {
                                if (scriptinfo._opList[c] is OpCreateVariable opcvar)
                                {
                                    definestruct.Add(opcvar);
                                }
                            }
                            macrosAndStructs.Add(definestruct, opdefs._name);
                            j += opdefs._branchPC;
                            continue;
                        }
                        if (scriptinfo._opList[j] is OpDefineMacro opdefmacro && scriptinfo._opList[j] is not OpUseMacro)
                        {
                            if (scriptinfo._opList[j + 1] is OpMacroInterface macinterface)
                            {
                                var definemacro = new List<OpAbstractCreateVariable>();
                                for (int c = j + 1; c < j + macinterface._branchPC; c++)
                                {
                                    if (scriptinfo._opList[c] is OpMacroParameter opmacropar)
                                    {
                                        definemacro.Add(opmacropar);
                                    }
                                }
                                macrosAndStructs.Add(definemacro, opdefmacro._name);
                                j += opdefmacro._branchPC;
                                continue;
                            }
                        }
                    }
                }
            }
            return macrosAndStructs;
        }

        public static Dictionary<string, string> ReadValueInfos(igObjectDirectory dir)
        {
            bool isGlobal = (dir._name._string == "app:/permanent/global.bld (level bld)");
            var dependencyObjs = new Dictionary<string, string>();
            for (int i = 0; i < dir._objectList._count; i++)
            {
                if (dir._objectList[i] is ValueInfo valinfo)
                {
                    igExternalReferenceSystem.Singleton._globalSet.MakeReference(valinfo._type, null, out igHandleName name);
                    igObject? valinfotype = igExternalReferenceSystem.Singleton._globalSet.ResolveReference(name, null);
                    if (valinfotype is igMetaObject vinfo)
                    {
                        switch (vinfo._name)
                        {
                            case "FloatMeasurement":
                                string valueinfoval = BitConverter.Int32BitsToSingle(valinfo._value).ToString();
                                dependencyObjs.Add(isGlobal ? "global::" : "local::" + valinfo._name, "float " + valueinfoval);
                                break;
                            case "IntMeasurement":
                                valueinfoval = valinfo._value.ToString();
                                dependencyObjs.Add(isGlobal?"global::" : "local::" + valinfo._name, "int " + valueinfoval);
                                break;
                            case "ColorMeasurement":
                                valueinfoval = valinfo._value.ToString("X8");
                                dependencyObjs.Add(isGlobal ? "global::" : "local::" + valinfo._name, "color " + valueinfoval);
                                break;
                            case "ScreenMeasurement":
                                dependencyObjs.Add(isGlobal ? "global::" : "local::" + valinfo._name, "screenmeasurement");
                                break;
                        }
                    } 
                }
                else if (dir._objectList[i] is ScriptStructure scriptstruct)
                {
                    dependencyObjs.Add(isGlobal ? "global::" : "local::" + scriptstruct._name, "struct " + scriptstruct._varContentsType._name);
                }
                if (dir._objectList[i] is ScriptSet set)
                {
                    igExternalReferenceSystem.Singleton._globalSet.MakeReference(set._list._editType, null, out igHandleName name);
                    igObject? edittype = igExternalReferenceSystem.Singleton._globalSet.ResolveReference(name, null);
                    if (edittype is igMetaObject type)
                    {
                        string temptype = "";
                        switch (type._name)
                        {
                            case "IntMeasurement":
                                temptype = "int";
                                break;
                            case "FloatMeasurement":
                                temptype = "float";
                                break;
                            case "ColorMeasurement":
                                temptype = "color";
                                break;
                            case "ScreenMeasurement":
                                temptype = "screenmeasurement";
                                break;
                            case "StringInfo":
                                temptype = "string";
                                break;
                            case "tfbSoundInfo":
                                temptype = "sound";
                                break;
                            case "tfbParticleInfo":
                                temptype = "particle";
                                break;
                            case "tfbLightInfo":
                                temptype = "light";
                                break;
                            case "ActorWaypoint":
                                temptype = "actorwaypoint";
                                break;
                            case "AnimationInfo":
                                temptype = "animation";
                                break;
                            case "tfbSpriteInfo":
                                temptype = "sprite";
                                break;
                        }
                        if (temptype != string.Empty)
                        {
                            dependencyObjs.TryAdd(isGlobal ? "global::" : "local::" + set._name, "List<" + temptype + ">");
                        }
                    }
                }
                else if (dir._objectList[i] is tfbScriptObject scriptobj)
                {
                    string temptype = "";
                    switch (dir._objectList[i].ToString().Split('.').Last())
                    {
                        case "StringInfo":
                            temptype = "string";
                            break;
                        case "tfbSoundInfo":
                            temptype = "sound";
                            break;
                        case "tfbParticleInfo":
                            temptype = "particle";
                            break;
                        case "tfbLightInfo":
                            temptype = "light";
                            break;
                        case "ActorWaypoint":
                            temptype = "actorwaypoint";
                            break;
                        case "AnimationInfo":
                            temptype = "animation";
                            break;
                        case "tfbSpriteInfo":
                            temptype = "sprite";
                            break;
                    }
                    if (temptype != string.Empty)
                    {
                        dependencyObjs.Add(isGlobal ? "global::" : "local::" + scriptobj._name, temptype);
                    }
                }
            }
            return dependencyObjs;
        }
    }
}
