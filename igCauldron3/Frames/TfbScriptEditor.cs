using igCauldron3.Frames.TextEdit;
using igCauldron3.Frames.TextEdit.Syntax;
using igCauldron3.Utils;
using igLibrary.Core;
using igLibrary.Gfx.GX2Utils;
using igLibrary.Tfb.Script;
using ImGuiNET;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.Devices;
using Newtonsoft.Json.Serialization;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design.Behavior;
using System.Windows.Forms.VisualStyles;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
namespace igCauldron3.Frames
{

    /// <summary>
    /// 
    /// Stuff to note:
    /// 
    /// - i have not made this readable yet, the code rn is hilariously unreadable and messy (going to fix this)
    /// 
    /// - you might notice I use the very messy variable "indentCount" to indent code
    ///   I'm planning to modify the text editor itself
    ///   instead to automatically indent lines when it reads '{', and remove indentation
    ///   when reading '}' (maybe)
    ///
    /// - Feel free to question my ways of displaying tfbScript, I want feedback
    /// - No, I'm not ever changing "int" to "IntMeasurement", or "color" to "ColorMeasurement" etc
    /// 
    /// - I'm split between making OpAbstractFlow a "goto" command with a prompt you can click
    ///   to jump to where it lands in the script, or to do what Ghidra does with unconditional branches
    ///   how do I program that help
    /// 
    /// </summary>

    public class TfbScriptEditor : Frame
    {
        private OpCodeList codeList;
        private OpCreateVariableList? varList;
        private igObjectDirectory currentdir;
        private string scriptName;
        private int linecount = 0;
        private TextEditor editor;
        /// <summary>
        /// colors used for colorcoding i guess :)
        /// colors used for colorcoding
        /// </summary>
        private Vector4 Grey = new Vector4(255f, 255f, 255f, 0.5f);
        private Vector4 Red = new Vector4(1f, 0f, 0f, 1f);
        private Vector4 Green = new Vector4(154f, 205f, 50f, 1f);
        private Vector4 Blue = new Vector4(0f, 0f, 1f, 1f);
        private Dictionary<OpSpawn, string> spawnedobjects = new Dictionary<OpSpawn, string>(); // for opspawn
        private int amountOfSpawns = 0; // for opspawn
        private string callordefinemacro = "define";
        private string? variablessection;
        private string scriptsection;
        private Dictionary<OpAbstractCreateVariable, string> localvariables = new Dictionary<OpAbstractCreateVariable, string>(); // to fix float variables by turning ints into floats
                                                                                                                                  // if the variable ever gets assigned with a float value
        private Dictionary<List<OpAbstractCreateVariable>, string> scriptDependencies = new Dictionary<List<OpAbstractCreateVariable>, string>();
        private Dictionary<string, string> localvarstrings = new Dictionary<string, string>();
        private Dictionary<OpUserBehavior, string> behaviors = new Dictionary<OpUserBehavior, string>();
        private int amountOfBehaviors = 0;
        private Dictionary<OpFlowBuiltInBehavior, string> branchTargets = new Dictionary<OpFlowBuiltInBehavior, string>(); // for OpAbstractFlow
        string script = "tfbscript test";
        public TfbScriptEditor(Window wnd, igObjectDirectory currentdir2, tfbScriptInfo inputscript, Dictionary<List<OpAbstractCreateVariable>, string> dependencies) : base(wnd)
        {
            codeList = inputscript._opList;
            varList = inputscript._masterVarList;
            scriptName = inputscript._name.Split('/').Last();
            currentdir = currentdir2;
            scriptDependencies = dependencies;
            StringBuilder scriptbuilder = new StringBuilder();
            if (varList != null)
            {
                scriptbuilder.AppendLine("Variables Section");
                variablessection = ParseVariables(varList);
            }
            scriptsection = ParseScriptObjects(codeList);
            if (varList != null)
            {
                foreach (var kvp in localvariables)
                {
                    scriptbuilder.AppendLine(kvp.Value + " [" + kvp.Key._varName + "]");
                    localvarstrings.TryAdd("[" + kvp.Key._varName + "]", kvp.Value);
                }
            }
            scriptbuilder.AppendLine("Script Section");
            scriptbuilder.Append(scriptsection);
            script = scriptbuilder.ToString();
            editor = new TextEditor
            {
                AllText = script,
                SyntaxHighlighter = (varList != null) ? new CStyleHighlighter(localvarstrings) : new CStyleHighlighter(),
            };
            editor.SetColor(PaletteIndex.Custom, 0xff0000ff);
            editor.SetColor(PaletteIndex.Custom + 1, 0xff00ffff);
            editor.SetColor(PaletteIndex.Custom + 2, 0xffffffff);
            editor.SetColor(PaletteIndex.Custom + 3, 0xff808080);


        }
        public override void Render()
        {
            ImGui.Begin(scriptName);
            if (ImGui.Button("Reset"))
            {
                editor.AllText = script;
            }
            ImGui.SameLine();
            ImGui.SameLine();
            if (ImGui.Button("Close")) Close();
            ImGui.Text(
                $"Cur:{editor.CursorPosition} SEL: {editor.Selection.Start} - {editor.Selection.End}"
            );
            editor.Render("EditWindow");
            ImGui.End();
        }

        public string ParseVariables(OpCreateVariableList varList)
        {
            StringBuilder externalsb = new StringBuilder();
            StringBuilder sb = new StringBuilder();
            ImGui.Text("Variable Section");
            linecount++;
            for (int i = 0; i < varList._count; i++)
            {
                linecount++;
                if (varList[i]._varContentsType is OpDefineStructure defineStructure)
                {
                    localvariables.TryAdd(varList[i], "struct " + defineStructure._name);
                    externalsb.AppendLine("struct " + defineStructure._name + " [" + varList[i]._varName + "]");
                    continue;
                }
                else if (varList[i]._varContentsType is OpDefineMacro defineMacro)
                {
                    localvariables.TryAdd(varList[i], "macro " + defineMacro._name);
                    externalsb.AppendLine("macro " + defineMacro._name + " [" + varList[i]._varName + "]");
                    continue;
                }
                igExternalReferenceSystem.Singleton._globalSet.MakeReference((igMetaObject)varList[i]._varContentsType, null, out igHandleName varContentsName);
                igObject? VarContents = igExternalReferenceSystem.Singleton._globalSet.ResolveReference(varContentsName, null);
                if (VarContents is igMetaObject contents)
                {
                    switch (contents._name)
                    {
                        case "IntMeasurement":
                            localvariables.Add(varList[i], "int");
                            sb.Append("int ");
                            break;
                        //case "FloatMeasurement": //does not work
                        //    sb.Append("float ");
                        //    break;
                        case "ColorMeasurement":
                            localvariables.Add(varList[i], "color");
                            sb.Append("color ");
                            break;
                        case "tfbActorInfo":
                            localvariables.Add(varList[i], "actor");
                            sb.Append("actor ");
                            break;
                        case "ActorWaypoint":
                            localvariables.Add(varList[i], "actorwaypoint"); //maybe "waypoint" to not confuse with actor
                            sb.Append("actorwaypoint ");
                            break;
                        case "tfbSoundInfo":
                            localvariables.Add(varList[i], "sound");
                            sb.Append("sound ");
                            break;
                        case "tfbSpriteInfo":
                            localvariables.Add(varList[i], "sprite");
                            sb.Append("sprite ");
                            break;
                        case "tfbParticleInfo":
                            localvariables.Add(varList[i], "particle");
                            sb.Append("particle ");
                            break;
                        case "AnimationInfo":
                            localvariables.Add(varList[i], "animation");
                            sb.Append("animation ");
                            break;
                        case "ScriptColorInfo":
                            localvariables.Add(varList[i], "scriptcolorinfo");
                            sb.Append("scriptcolorinfo ");
                            break;
                        case "Slider":
                            localvariables.Add(varList[i], "slider");
                            sb.Append("slider ");
                            break;
                        case "ScriptController":
                            localvariables.Add(varList[i], "scriptcontroller");
                            sb.Append("scriptcontroller ");
                            break;
                        case "ValueInfo":
                            localvariables.Add(varList[i], "valueinfo");
                            sb.Append("valueinfo ");
                            break;
                        case "tfbLightInfo":
                            localvariables.Add(varList[i], "light");
                            sb.Append("light ");
                            break;
                        case "StringInfo":
                            localvariables.Add(varList[i], "string");
                            sb.Append("string ");
                            break;
                        default:
                            externalsb.AppendLine("// contents type not supported yet: " + contents._name);
                            break;
                    }
                    sb.Append("[" + varList[i]._varName + "]");
                    externalsb.AppendLine(sb.ToString());
                    sb.Clear();

                    ////    // The container type shouldnt be visible to the user but it's still important
                    ////    // Depending on what the _varContentsType is, the game contains it at runtime in a specific _varContainerType
                    ////    // other factors can affect this too
                    ////    // This has to be handled when saving, by the script editor automatically
                    ////    // todo: Lookup the container type for all content types.
                    ////    // todo: Lookup all OpCreateVariable _varContents types
                    ////}
                }
            }
            return externalsb.ToString();
        }
        private string SetupRHS(RHSReferenceStack RHS)
        {
            StringBuilder sb = new StringBuilder();
            igExternalReferenceSystem.Singleton._globalSet.MakeReference(RHS._type, null, out igHandleName name);
            igObject? EXNMTest = igExternalReferenceSystem.Singleton._globalSet.ResolveReference(name, null);
            if (EXNMTest == null) return "";
            if (EXNMTest is igMetaObject ex)
            {
                if (ex._name == "FloatMeasurement")
                    sb.Append(BitConverter.Int32BitsToSingle(RHS._value).ToString(CultureInfo.InvariantCulture));
                else if (ex._name == "IntMeasurement")
                    sb.Append(RHS._value);
                else if (ex._name == "ColorMeasurement")
                {
                    sb.Append(RHS._value.ToString("X8"));
                }
                else if (ex._name == "ScreenMeasurement")
                {
                    sb.Append("(x: " + ((uint)(RHS._value) >> 16) + " y: " + ((uint)(RHS._value) & 0xFFFF) + ")");
                }
                else
                {
                    sb.AppendLine(new string(' ', indentCount * 3) + "// rhsreferencestack: rhsvalue type not implemented, name:" + ex._name);
                }
            }
            return sb.ToString();
        }
        private string SetupRHS(ValueRHSVariant RHS, bool changeVal = false)
        {
            StringBuilder sb = new StringBuilder();
            if (RHS._varOp1._count != 0 && RHS._varOp1._value == 0)
            {
                string rhsobjects = ReadRHSObjects(RHS._varOp1);
                sb.Append(rhsobjects);
                return sb.ToString();
            }
            int rhscount = 1;
            if (RHS._arithOperator is not ValueRHSVariant.ArithOp.dotdotdot)
            {
                rhscount = 2;
            }

            for (int r = 0; r < rhscount; r++)
            {
                RHSValueStack.enumRHSfunc rhsfuncEnum;
                if (r == 0 && !changeVal) rhsfuncEnum = RHS._varOp1._funcRHS;
                else rhsfuncEnum = RHS._varOp2._funcRHS;
                switch (rhsfuncEnum)
                {
                    case RHSValueStack.enumRHSfunc.kRHSfuncInteger:
                        sb.Append("(int)");
                        break;
                    case RHSValueStack.enumRHSfunc.kRHSfuncNone:
                        // intentionally nothing
                        break;
                    case RHSValueStack.enumRHSfunc.kRHSfuncRandom:
                        sb.Append("random");
                        break;
                    case RHSValueStack.enumRHSfunc.kRHSfuncRound:
                        sb.Append("round");
                        break;
                    case RHSValueStack.enumRHSfunc.kRHSfuncSine:
                        sb.Append("sin");
                        break;
                    case RHSValueStack.enumRHSfunc.kRHSfuncCosine:
                        sb.Append("cos");
                        break;
                    case RHSValueStack.enumRHSfunc.kRHSfuncTangent:
                        sb.Append("tan");
                        break;
                    case RHSValueStack.enumRHSfunc.kRHSfuncArc_sine:
                        sb.Append("arcsin");
                        break;
                    case RHSValueStack.enumRHSfunc.kRHSfuncArc_cosine:
                        sb.Append("arccos");
                        break;
                    case RHSValueStack.enumRHSfunc.kRHSfuncArc_tangent:
                        sb.Append("arctan");
                        break;
                    case RHSValueStack.enumRHSfunc.kRHSfuncSquare_root:
                        sb.Append("sqrt");
                        break;
                    case RHSValueStack.enumRHSfunc.kRHSfuncAbsolute_value:
                        sb.Append("abs");
                        break;
                    default:
                        sb.AppendLine(new string(' ', indentCount * 3) + $"// Unhandled RHS func: {RHS._varOp1._funcRHS}");
                        break;
                }

                RHSValueStack rhsValueS;
                if (r == 0 && !changeVal) rhsValueS = RHS._varOp1;
                else rhsValueS = RHS._varOp2;
                igExternalReferenceSystem.Singleton._globalSet.MakeReference(rhsValueS._type, null, out igHandleName name);
                igObject? EXNMTest = igExternalReferenceSystem.Singleton._globalSet.ResolveReference(name, null);
                if (EXNMTest == null) continue;
                if (EXNMTest is igMetaObject ex)
                {
                    if (ex._name == "FloatMeasurement")
                        if (changeVal && r == 1) ;
                        else sb.Append(BitConverter.Int32BitsToSingle(rhsValueS._value).ToString(CultureInfo.InvariantCulture));
                    else if (ex._name == "IntMeasurement")
                        if (changeVal && r == 1) ;
                        else sb.Append(rhsValueS._value);
                    else if (ex._name == "ColorMeasurement")
                    {
                        sb.Append(rhsValueS._value.ToString("X8"));
                    }
                    else if (ex._name == "ScreenMeasurement")
                    {
                        sb.Append("(x: " + ((uint)(rhsValueS._value) >> 16) + " y: " + ((uint)(rhsValueS._value) & 0xFFFF) + ")");
                    }
                    else
                    {
                        sb.AppendLine(new string(' ', indentCount * 3) + "// OpSetValue: rhsvalue type not implemented, name:" + ex._name);
                    }
                }
                if (rhsValueS._funcRHS is RHSValueStack.enumRHSfunc.kRHSfuncSquare)
                    sb.Append("^2");
                if (rhscount == 2)
                {
                    if (!changeVal && r == 0)
                    {
                        switch (RHS._arithOperator)
                        {
                            case ValueRHSVariant.ArithOp.plus:
                                sb.Append(" + ");
                                break;
                            case ValueRHSVariant.ArithOp.minus:
                                sb.Append(" - ");
                                break;
                            case ValueRHSVariant.ArithOp.times:
                                sb.Append(" * ");
                                break;
                            case ValueRHSVariant.ArithOp.divide:
                                sb.Append(" / ");
                                break;
                            default:
                                sb.Append(" ? ");
                                break;
                        }
                    }

                }
            }
            return sb.ToString();
        }
        private string ReadRHSObjects(RHSValueStack rh)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < rh._count; i++)
            {
                if (rh[i] != null && rh[i] is AbstractPlacement || rh[i] is ScriptStructure)
                {
                    igNamedObject named = rh[i];
                    string name = "";
                    if (rh[i] is ScriptStructure || named._name.Contains(' '))
                    {
                        name = "'" + named._name + "'";
                    }
                    else
                    {
                        name = named._name;
                    }
                    if (currentdir._objectList.Contains(rh[i]))
                    {
                        sb.Append(name);
                    }
                    else
                    {
                        sb.Append("global::" + name);
                    }
                    if (i != (rh._count - 1)) sb.Append(".");
                    continue;
                }
                switch (rh[i])
                {
                    case OpControl:
                        sb.Append("[^controlled]");
                        break;
                    case OpSlideValue:
                        sb.Append("[^slider]");
                        break;
                    case OpCheckFOV checkfov:
                        string fovlhs = SetupLHS(checkfov._LHS, codeList, i);
                        sb.Append(fovlhs + ".filtered");
                        break;
                    case OpLoopValue:
                        sb.Append("[loop index]");
                        break;
                    case OpMacroParameter macro:
                        sb.Append("[" + macro._varName + "]");
                        break;
                    case OpSpawn spawn1:
                        sb.Append(spawnedobjects[spawn1]);
                        break;
                    case OpForEach opf:
                        string each = SetupLHS(opf._LHS, codeList, i);
                        sb.Append(each + ".current");
                        break;
                    case OpFindVariable:
                        sb.Append("[^found]");
                        break;
                    case OpStartSequence:
                        sb.Append("[^sequence]");
                        break;
                    case OpFindSubSet:
                        sb.Append("[^subset]");
                        break;
                    case OpCreateVariable opvar:
                        sb.Append("[" + opvar._varName + "]");
                        break;
                    case ValueInfo valinfo:
                        if (currentdir._objectList.Contains(valinfo))
                        {
                        }
                        else
                        {
                            sb.Append("global::");
                        }
                        string valueinfoval = "";
                        igExternalReferenceSystem.Singleton._globalSet.MakeReference(valinfo._type, null, out igHandleName name);
                        igObject? valueinfo2 = igExternalReferenceSystem.Singleton._globalSet.ResolveReference(name, null);
                        if (valueinfo2 is igMetaObject type)
                        {
                            switch (type._name)
                            {
                                case "FloatMeasurement":
                                    valueinfoval = BitConverter.Int32BitsToSingle(valinfo._value).ToString((CultureInfo.InvariantCulture));
                                    localvarstrings.TryAdd(valinfo._name, valueinfoval);
                                    break;
                                case "IntMeasurement":
                                    valueinfoval = valinfo._value.ToString();
                                    localvarstrings.TryAdd(valinfo._name, valueinfoval);
                                    break;
                            }
                        }
                        if (valinfo._name.Contains(' '))
                        {
                            sb.Append("'" + valinfo._name + "'");
                        }
                        else
                        {
                            sb.Append(valinfo._name);
                        }
                        break;
                    case ScriptReference scriptref:
                        if (scriptref._type != null)
                        {
                            string reftype = "";
                            if (scriptref._type is OpDefineStructure defs)
                            {
                                reftype = "struct " + defs._name;
                            }
                            else if (scriptref._type is OpDefineMacro defm)
                            {
                                reftype = "macro " + defm._name;
                            }
                            else
                            {
                                igExternalReferenceSystem.Singleton._globalSet.MakeReference(scriptref._type, null, out igHandleName name2);
                                igObject? scriptref2 = igExternalReferenceSystem.Singleton._globalSet.ResolveReference(name2, null);
                                if (scriptref2 is igMetaObject met)
                                {
                                    switch (met._name)
                                    {
                                        case "IntMeasurement": // floats are stored as IntMeasurements with _RHS._varOp1._type = float
                                            sb.Append("int");
                                            break;
                                        case "FloatMeasurement":
                                            sb.Append("float");
                                            break;
                                        case "ColorMeasurement":
                                            sb.Append("color");
                                            break;
                                        case "ScreenMeasurement":
                                            sb.Append("screenmeasurement");
                                            break;
                                        case "ValueInfo":
                                            sb.Append("valueinfo");
                                            break;
                                        case "tfbActorInfo":
                                            sb.Append("actor");
                                            break;
                                        case "ActorWaypoint":
                                            sb.Append("actorwaypoint");
                                            break;
                                        case "tfbSoundInfo":
                                            sb.Append("sound");
                                            break;
                                        case "tfbSpriteInfo":
                                            sb.Append("sprite");
                                            break;
                                        case "tfbParticleInfo":
                                            sb.Append("particle");
                                            break;
                                        case "AnimationInfo":
                                            sb.Append("animation");
                                            break;
                                        case "ScriptColorInfo":
                                            sb.Append("scriptcolorinfo");
                                            break;
                                        case "Slider":
                                            sb.Append("slider");
                                            break;
                                        case "ScriptController":
                                            sb.Append("scriptcontroller");
                                            break;
                                        case "tfbLightInfo":
                                            sb.Append("light");
                                            break;
                                        case "StringInfo":
                                            sb.Append("string");
                                            break;
                                    }
                                }
                                reftype = sb.ToString();
                            }
                            localvarstrings.TryAdd("[" + scriptref._name + "]", reftype);
                            sb.Clear();
                            if (currentdir._objectList.Contains(scriptref))
                            {
                                sb.Append("local::");
                            }
                            else
                            {
                                sb.Append("global::");
                            }
                            sb.Append("[" + scriptref._name + "]");
                        }
                        else
                        {
                            switch (scriptref._name)
                            {
                                case "OpTopLevelBehavior.my":
                                    sb.Append((rh._count > 1) ? "my" : "myself");
                                    break;
                                default:
                                    if (scriptref._name.Split('.').Last().Contains(' '))
                                    {
                                        sb.Append(ToCamelCase(scriptref._name.Split('.').Last()));
                                    }
                                    else
                                    {
                                        sb.Append(scriptref._name.Split('.').Last());
                                    }
                                    break;
                            }
                        }
                        break;
                    case OpUserBehavior opuser:
                        if (behaviors.ContainsKey(opuser))
                        {
                            sb.Append(behaviors[opuser]);
                        }
                        else
                        {
                            amountOfBehaviors++;
                            behaviors.Add(opuser, "behavior" + amountOfBehaviors);
                            sb.Append(behaviors[opuser]);
                        }
                        break;
                    case ReferenceVariant refvar:
                        sb.Append(refvar._name.Split('.').Last());
                        break;
                    case OpDefineMacro defmac:
                        sb.Append("(" + defmac._name + ")");
                        break;
                    case ScriptSet sset:
                        if (sset._name.Contains(' '))
                        {
                            sb.Append("'" + sset._name + "'");

                        }
                        else
                        {
                            sb.Append(sset._name);
                        }
                        break;
                    case tfbScriptObject so:
                        if (so._name.Split('.').Last().Contains(' '))
                        {
                            sb.Append(ToCamelCase(so._name.Split('.').Last()));
                        }
                        else
                        {
                            sb.Append(so._name.Split('.').Last());
                        }
                        break;
                }
                if (i != rh._count - 1) sb.Append(".");
            }
            return sb.ToString();
        }
        private string SetupLHS(ScriptObjectList LHS, OpCodeList codeList, int pc)
        {
            StringBuilder sb = new StringBuilder();
            bool useParenthesis = (LHS._count == 1);
            for (int j = 0; j < LHS._count; j++)
            {
                if (LHS[j] != null && LHS[j] is AbstractPlacement || LHS[j] is ScriptStructure)
                {
                    igNamedObject named = LHS[j];
                    string name = "";
                    if (LHS[j] is ScriptStructure || named._name.Contains(' '))
                    {
                        name = "'" + named._name + "'";
                    }
                    else
                    {
                        name = named._name;
                    }
                    if (currentdir._objectList.Contains(LHS[j]))
                    {
                        sb.Append(name);
                    }
                    else
                    {
                        sb.Append("global::" + name);
                    }
                    if (j != (LHS._count - 1)) sb.Append(".");
                    continue;
                }
                switch (LHS[j]) // some script objects use different fields for their actual names.
                {
                    case null:
                        continue;
                    case OpControl:
                        sb.Append("[^controlled]");
                        break;
                    case OpCheckFOV checkfov:
                        string fovlhs = SetupLHS(checkfov._LHS, codeList, pc);
                        sb.Append(fovlhs + ".filtered");
                        break;
                    case OpFindVariable:
                        sb.Append("[^found]");
                        break;
                    case OpLoopValue:
                        sb.Append("[loop index]");
                        break;
                    case OpCreateVariable opvar:
                        sb.Append("[" + opvar._varName + "]"); // variables should be written in brackets or other separator
                        break;
                    case OpForEach opf:
                        string each = SetupLHS(opf._LHS, codeList, pc);
                        sb.Append(each + ".current");
                        break;
                    case OpFindSubSet:
                        sb.Append("[^subset]");
                        break;
                    case OpStartSequence:
                        sb.Append("[^sequence]");
                        break;
                    case OpSlideValue:
                        sb.Append("[^slider]");
                        break;
                    case OpSpawn spawn1:
                        sb.Append(spawnedobjects[spawn1]);
                        break;
                    case ScriptSetReference scriptsetref:
                        if (scriptsetref._name.Split('.').Last().Contains(' '))
                        {
                            sb.Append(ToCamelCase(scriptsetref._name.Split('.').Last()));
                        }
                        else
                        {
                            sb.Append(scriptsetref._name.Split('.').Last());
                        }
                        break;
                    case ValueInfo valinfo:
                        if (currentdir._objectList.Contains(valinfo))
                        {
                        }
                        else
                        {
                            sb.Append("global::");
                        }
                        string valueinfoval = "";
                        igExternalReferenceSystem.Singleton._globalSet.MakeReference(valinfo._type, null, out igHandleName name);
                        igObject? valueinfo2 = igExternalReferenceSystem.Singleton._globalSet.ResolveReference(name, null);
                        if (valueinfo2 is igMetaObject type)
                        {
                            switch (type._name)
                            {
                                case "FloatMeasurement":
                                    valueinfoval = BitConverter.Int32BitsToSingle(valinfo._value).ToString((CultureInfo.InvariantCulture));
                                    localvarstrings.TryAdd(valinfo._name, valueinfoval);
                                    break;
                                case "IntMeasurement":
                                    valueinfoval = valinfo._value.ToString();
                                    localvarstrings.TryAdd(valinfo._name, valueinfoval);
                                    break;
                            }
                        }
                        if (valinfo._name.Contains(' '))
                        {
                            sb.Append("'" + valinfo._name + "'");
                        }
                        else
                        {
                            sb.Append(valinfo._name);
                        }
                        break;
                    case ScriptReference scriptref:
                        if (scriptref._type != null)
                        {
                            string reftype = "";
                            if (scriptref._type is OpDefineStructure defs)
                            {
                                reftype = "struct " + defs._name;
                            }
                            else if (scriptref._type is OpDefineMacro defm)
                            {
                                reftype = "macro " + defm._name;
                            }
                            else
                            {
                                igExternalReferenceSystem.Singleton._globalSet.MakeReference(scriptref._type, null, out igHandleName name2);
                                igObject? scriptref2 = igExternalReferenceSystem.Singleton._globalSet.ResolveReference(name2, null);
                                if (scriptref2 is igMetaObject met)
                                {
                                    switch (met._name)
                                    {
                                        case "IntMeasurement": // floats are stored as IntMeasurements with _RHS._varOp1._type = float
                                            sb.Append("int");
                                            break;
                                        case "FloatMeasurement":
                                            sb.Append("float");
                                            break;
                                        case "ColorMeasurement":
                                            sb.Append("color");
                                            break;
                                        case "ScreenMeasurement":
                                            sb.Append("screenmeasurement");
                                            break;
                                        case "ValueInfo":
                                            sb.Append("valueinfo");
                                            break;
                                        case "tfbActorInfo":
                                            sb.Append("actor");
                                            break;
                                        case "ActorWaypoint":
                                            sb.Append("actorwaypoint");
                                            break;
                                        case "tfbSoundInfo":
                                            sb.Append("sound");
                                            break;
                                        case "tfbSpriteInfo":
                                            sb.Append("sprite");
                                            break;
                                        case "tfbParticleInfo":
                                            sb.Append("particle");
                                            break;
                                        case "AnimationInfo":
                                            sb.Append("animation");
                                            break;
                                        case "ScriptColorInfo":
                                            sb.Append("scriptcolorinfo");
                                            break;
                                        case "Slider":
                                            sb.Append("slider");
                                            break;
                                        case "ScriptController":
                                            sb.Append("scriptcontroller");
                                            break;
                                        case "tfbLightInfo":
                                            sb.Append("light");
                                            break;
                                        case "StringInfo":
                                            sb.Append("string");
                                            break;
                                    }
                                }
                                reftype = sb.ToString();
                            }
                            localvarstrings.TryAdd("[" + scriptref._name + "]", reftype);
                            sb.Clear();
                            if (currentdir._objectList.Contains(scriptref))
                            {
                                sb.Append("local::");
                            }
                            else
                            {
                                sb.Append("global::");
                            }
                            sb.Append("[" + scriptref._name + "]");
                        }
                        else
                        {
                            switch (scriptref._name.Split('.').Last())
                            {
                                case "my":
                                    sb.Append((LHS._count > 1) ? "my" : "myself");
                                    break;
                                default:
                                    if (scriptref._name.Split('.').Last().Contains(' '))
                                    {
                                        sb.Append(ToCamelCase(scriptref._name.Split('.').Last()));
                                    }
                                    else
                                    {
                                        sb.Append(scriptref._name.Split('.').Last());
                                    }
                                    break;
                            }
                        }
                        break;
                    case ColorMeasurement cm:
                        switch (cm._name)
                        {
                            case "Placement.tint":
                                sb.Append("tint");
                                break;
                            default:
                                if (cm._name.Split('.').Last().Contains(' '))
                                {
                                    sb.Append(ToCamelCase(cm._name.Split('.').Last()));
                                }
                                else
                                {
                                    sb.Append(cm._name.Split('.').Last());
                                }
                                break;
                        }
                        break;
                    case OrientationMeasurement om:
                        switch (om._name)
                        {
                            default:
                                if (om._name.Split('.').Last().Contains(' '))
                                {
                                    sb.Append(ToCamelCase(om._name.Split('.').Last()));
                                }
                                else
                                {
                                    sb.Append(om._name.Split('.').Last());
                                }
                                break;
                        }
                        break;

                    case FloatMeasurement fl:
                        switch (fl._name)
                        {
                            case "ScaleMeasurement.X":
                                sb.Append("x");
                                break;
                            case "ScaleMeasurement.Y":
                                sb.Append("y");
                                break;
                            case "ScaleMeasurement.Z":
                                sb.Append("z");
                                break;
                            default:
                                if (fl._name.Split('.').Last().Contains(' '))
                                {
                                    sb.Append(ToCamelCase(fl._name.Split('.').Last()));
                                }
                                else
                                {
                                    sb.Append(fl._name.Split('.').Last());
                                }
                                break;
                        }
                        break;
                    case IntMeasurement im:
                        switch (im._name)
                        {
                            default:
                                if (im._name.Split('.').Last().Contains(' '))
                                {
                                    sb.Append(ToCamelCase(im._name.Split('.').Last()));
                                }
                                else
                                {
                                    sb.Append(im._name.Split('.').Last());
                                }
                                break;
                        }
                        break;
                    case ScaleMeasurement sm:
                        switch (sm._name)
                        {
                            default:
                                if (sm._name.ToString().Split('.').Last().Contains(' '))
                                {
                                    sb.Append(ToCamelCase(sm._name.Split('.').Last()));
                                }
                                else
                                {
                                    sb.Append(sm._name.ToString().Split('.').Last());
                                }
                                break;
                        }
                        break;
                    case OpMacroParameter macro:
                        sb.Append("[" + macro._varName + "]");
                        break;
                    case OpCheckReference opcr:
                        string lhs = SetupLHS(opcr._LHS, codeList, pc);
                        sb.Append(lhs);
                        break;
                    case OpDefineMacro opmac:
                        sb.Append("(" + opmac._name + ")");
                        break;
                    case OpCheckValue opc:
                        string LeftHandStack = SetupLHS(opc._LHS, codeList, pc);
                        sb.Append(LeftHandStack);
                        break;
                    case ScriptSet sset:
                        if (sset._name.Split('.').Last().Contains(' '))
                        {
                            sb.Append("'" + sset._name + "'");
                        }
                        else
                        {
                            sb.Append(sset._name);
                        }
                        break;
                    case tfbScriptObject so:
                        if (so._name.Split('.').Last().Contains(' '))
                        {
                            sb.Append(ToCamelCase(so._name.Split('.').Last()));
                        }
                        else
                        {
                            sb.Append(so._name.Split('.').Last());
                        }
                        break;
                    default:
                        sb.Append(LHS[j].ToString().Split('.').Last() + "::'" + LHS[j]._name + "'"); // never use this.
                        break;
                }

                if (j != (LHS._count - 1)) sb.Append(".");
            }
            return sb.ToString();
        }
        public string ToCamelCase(string input)
        {
            StringBuilder sb = new StringBuilder();
            string[] array = input.Split(' ');
            string toCamel = "";
            foreach (var s in array)
            {
                if (s == array[0])
                {
                    sb.Append(s);
                }
                else
                {
                    sb.Append(s[0].ToString().ToUpper() + s.Substring(1));
                }
            }
            toCamel = sb.ToString();
            return toCamel;
        }
        int indentCount = 0;

        private StringBuilder returnedstring = new StringBuilder();
        private string ParseScriptObjects(OpCodeList codeList, int startOffset = 0, int CodeCount = 0)
        {
            if (CodeCount == 0)
            {
                CodeCount = codeList._count;
            }
            else if (CodeCount == 1)
            {
                CodeCount = startOffset + 1;
            }
            else
            {
                CodeCount += startOffset;
            }
            StringBuilder sb = new StringBuilder();
            for (int i = startOffset; i < CodeCount; i++)
            {
                /// Unfinished opcodes:
                /// OpFindSubSet (_lhs._otherPoint???)
                /// OpCheckFOV (not exactly sure about the cone's dimensions etc)
                /// OpFindVariable (idk when to use "force_tag", "in_script" etc)
                /// OpChangeMembership (add, exclude_all idk about). (exclude also seems weird)
                /// OpCheckMembership (same thing)
                /// 
                /// 
                /// Unimplemented opcodes:
                /// OpPreScript
                /// OpStartUp
                /// OpShutDown
                /// OpAbstractFlow
                /// OpTeleportTo
                /// 
                /// Other unimplemented Opcodes (from 3DS)
                /// OpPrint (kind of exists on wii but never used, idk if it would work)
                /// OpPlayInstance
                /// OpHudApplyDamage
                /// OpHudDisplayString
                /// 
                ///
                if (codeList[i] is OpSetValue setv)
                {
                    int rhscount = 1;
                    if (setv._RHS._arithOperator is not ValueRHSVariant.ArithOp.dotdotdot)
                    {
                        rhscount = 2;
                    }
                    string LeftHandStack = SetupLHS(setv._LHS, codeList, i);
                    sb.Append(LeftHandStack);

                    bool changeVal = true;
                    if (setv._RHS._varOp1._count != 0 && setv._LHS._count != 0)
                    {
                        if (setv._RHS._varOp1._count == setv._LHS._count)
                        {
                            for (int j = 0; j < setv._RHS._varOp1._count; j++)
                            { // if lhs and rhs are the same write -=, += or similar
                                if (setv._RHS._varOp1[j].ToString().Split('.').Last() == setv._LHS[j].ToString().Split('.').Last())
                                {
                                    if (setv._RHS._varOp1[j] is OpCreateVariable rhsOpc && setv._LHS[j] is OpCreateVariable lhsOpc)
                                    {
                                        if (rhsOpc._varName == lhsOpc._varName) continue;
                                        else
                                        {
                                            changeVal = false;
                                            break;
                                        }
                                    }
                                    else if (setv._RHS._varOp1[j] is OpCreateVariable rhsMacro && setv._LHS[j] is OpCreateVariable lhsMacro)
                                    {
                                        if (rhsMacro._varName == lhsMacro._varName) continue;
                                        else
                                        {
                                            changeVal = false;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (setv._RHS._varOp1[j]._name == setv._LHS[j]._name) continue;
                                        else
                                        {
                                            changeVal = false;
                                            break;
                                        }
                                    }
                                }
                                if (setv._RHS._varOp1[j]._name == setv._LHS[j]._name) continue;
                                else
                                {
                                    changeVal = false;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            changeVal = false;
                        }
                    }
                    else changeVal = false;
                    if (changeVal)
                    {
                        switch (setv._RHS._arithOperator)
                        {
                            case ValueRHSVariant.ArithOp.plus:
                                sb.Append(" += ");
                                break;
                            case ValueRHSVariant.ArithOp.minus:
                                sb.Append(" -= ");
                                break;
                            case ValueRHSVariant.ArithOp.times:
                                sb.Append(" *= ");
                                break;
                            case ValueRHSVariant.ArithOp.divide:
                                sb.Append(" /= ");
                                break;
                            default:
                                sb.Append(" ? "); // unknown ArithOp (this should never happen)
                                break;
                        }
                    }
                    else sb.Append(" = ");

                    for (int r = 0; r < rhscount; r++)
                    {
                        RHSValueStack.enumRHSfunc rhsfuncEnum;
                        if (r == 0)
                        {
                            if (changeVal) continue;
                            else
                            {
                                rhsfuncEnum = setv._RHS._varOp1._funcRHS;
                            }
                        }
                        else
                        {
                            rhsfuncEnum = setv._RHS._varOp2._funcRHS;
                        }
                        switch (rhsfuncEnum)
                        {
                            case RHSValueStack.enumRHSfunc.kRHSfuncInteger:
                                sb.Append("(int)");
                                break;
                            case RHSValueStack.enumRHSfunc.kRHSfuncCeiling:
                                sb.Append("ceiling");
                                break;
                            case RHSValueStack.enumRHSfunc.kRHSfuncNone:
                                // intentionally nothing
                                break;
                            case RHSValueStack.enumRHSfunc.kRHSfuncRandom:
                                sb.Append("random");
                                break;
                            case RHSValueStack.enumRHSfunc.kRHSfuncRound:
                                sb.Append("round");
                                break;
                            case RHSValueStack.enumRHSfunc.kRHSfuncSine:
                                sb.Append("sin");
                                break;
                            case RHSValueStack.enumRHSfunc.kRHSfuncCosine:
                                sb.Append("cos");
                                break;
                            case RHSValueStack.enumRHSfunc.kRHSfuncTangent:
                                sb.Append("tan");
                                break;
                            case RHSValueStack.enumRHSfunc.kRHSfuncArc_sine:
                                sb.Append("sin^-1");
                                break;
                            case RHSValueStack.enumRHSfunc.kRHSfuncArc_cosine:
                                sb.Append("cos^-1");
                                break;
                            case RHSValueStack.enumRHSfunc.kRHSfuncArc_tangent:
                                sb.Append("tan^-1");
                                break;
                            case RHSValueStack.enumRHSfunc.kRHSfuncSquare_root:
                                sb.Append("sqr");
                                break;
                            case RHSValueStack.enumRHSfunc.kRHSfuncAbsolute_value:
                                sb.Append("absolute");
                                break;
                            default:
                                returnedstring.AppendLine(new string(' ', indentCount * 3) + $"Unhandled RHS func: {setv._RHS._varOp1._funcRHS}");
                                break;
                        }

                        RHSValueStack rhsValueS; // bad code to handle which of the RHSValueStacks is currently parsed
                        if (r == 0 && !changeVal) rhsValueS = setv._RHS._varOp1;
                        else rhsValueS = setv._RHS._varOp2;
                        bool hasRhsObjects = false;
                        if (rhsValueS._count != 0 && rhsValueS._value == 0)
                        {
                            string rhsobjects = ReadRHSObjects(rhsValueS);
                            if (rhsValueS[rhsValueS._count - 1] is OpCreateVariable var1 && setv._LHS[setv._LHS._count - 1] is OpCreateVariable var2)
                            {
                                if (localvariables.ContainsKey(var1) && localvariables.ContainsKey(var2))
                                {
                                    if (localvariables[var1] == "float")
                                    {
                                        localvariables[var2] = "float";
                                    }
                                }
                            }
                            sb.Append(rhsobjects);
                            hasRhsObjects = true;
                        }
                        igExternalReferenceSystem.Singleton._globalSet.MakeReference(rhsValueS._type, null, out igHandleName name);
                        igObject? EXNMTest = igExternalReferenceSystem.Singleton._globalSet.ResolveReference(name, null);
                        if (EXNMTest == null) continue;
                        if (EXNMTest is igMetaObject ex && !hasRhsObjects)
                        {
                            if (ex._name == "FloatMeasurement")
                            {
                                if (setv._LHS[setv._LHS._count - 1] is OpCreateVariable var)
                                {
                                    if (localvariables.ContainsKey(var))
                                    {
                                        if (localvariables[var] == "int")
                                        {
                                            localvariables[var] = "float";
                                        }
                                    }
                                }
                                sb.Append(BitConverter.Int32BitsToSingle(rhsValueS._value).ToString((CultureInfo.InvariantCulture)));
                            }
                            else if (ex._name == "IntMeasurement")
                                sb.Append(rhsValueS._value);
                            else if (ex._name == "ColorMeasurement")
                            {
                                sb.Append("0x" + rhsValueS._value.ToString("X8")); // todo: eventually maybe add way to view color?
                            }
                            else if (ex._name == "ScreenMeasurement")
                            { //screen measurement is 2 ushorts, 1st half is X, second half is Y
                              //theyre multiplied by 16 ingame
                              //this is broken somehow, the values become astronomically high

                                sb.Append("(x: " + ((uint)(rhsValueS._value) >> 16) + " y: " + ((uint)(rhsValueS._value) & 0xFFFF) + ")");
                            }
                            else
                            {
                                returnedstring.AppendLine(new string(' ', indentCount * 3) + "OpSetValue: rhsvalue type not implemented, name:" + ex._name);
                            }
                        }
                        if (rhsValueS._funcRHS is RHSValueStack.enumRHSfunc.kRHSfuncSquare)
                            sb.Append("^2");
                        if (rhscount == 2)
                        {
                            if (!changeVal && r == 0)
                            {
                                switch (setv._RHS._arithOperator)
                                {
                                    case ValueRHSVariant.ArithOp.plus:
                                        sb.Append(" + ");
                                        break;
                                    case ValueRHSVariant.ArithOp.minus:
                                        sb.Append(" - ");
                                        break;
                                    case ValueRHSVariant.ArithOp.times:
                                        sb.Append(" * ");
                                        break;
                                    case ValueRHSVariant.ArithOp.divide:
                                        sb.Append(" / ");
                                        break;
                                    default:
                                        sb.Append(" ? "); // unknown ArithOp (this should never happen)
                                        break;
                                }
                            }

                        }
                    }
                    string returnTest = sb.ToString();
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + returnTest);
                    sb.Clear();

                }
                else if (codeList[i] is OpPreScript preScript)
                {
                    returnedstring.AppendLine("#prescript");
                    if (preScript._branchPC != 0)
                    {
                        ParseScriptObjects(codeList, i + 1, preScript._branchPC);
                        i += preScript._branchPC;
                    }
                    returnedstring.AppendLine("#prescript-end");
                }
                else if (codeList[i] is OpStartUp startUp)
                {
                    returnedstring.AppendLine("#startup");
                    if (startUp._branchPC != 0)
                    {
                        ParseScriptObjects(codeList, i + 1, startUp._branchPC);
                        i += startUp._branchPC;
                    }
                    returnedstring.AppendLine("#startup-end");
                }
                else if (codeList[i] is OpShutDown shutDown)
                {
                    returnedstring.AppendLine("#shutdown");
                    if (shutDown._branchPC != 0)
                    {
                        ParseScriptObjects(codeList, i + 1, shutDown._branchPC);
                        i += shutDown._branchPC;
                    }
                    returnedstring.AppendLine("#shutdown-end");
                }

                else if (codeList[i] is OpTurnTo turnto)
                { // turn to||with anim
                    string facing = SetupRHS(turnto._facingRHS);
                    string animation = SetupLHS(turnto._NP, codeList, i);
                    if (turnto._indexRHS != null)
                    {
                        string indexrhs = SetupRHS(turnto._indexRHS);
                        if (indexrhs.Contains('[') || indexrhs.Contains(']')) ;
                        else
                        {
                            indexrhs = "[" + indexrhs + "]";
                        }
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "turnTo(" + facing + ", playing: " + animation + indexrhs + ")");
                    }
                    else
                    {
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "turnTo(" + facing + ", playing: " + animation + ")");
                    }
                }
                else if (codeList[i] is OpMoveTo moveto)
                {
                    string destination = SetupLHS(moveto._LHS, codeList, i);
                    string untilRhs = SetupRHS(moveto._RHS);
                    string animation = SetupLHS(moveto._NP, codeList, i);
                    string dir = "";
                    switch (moveto._dir)
                    {
                        case SetDirection.forward:
                            dir = "forward";
                            break;
                        case SetDirection.backward:
                            dir = "backward";
                            break;
                        case SetDirection.randomly:
                            dir = "randomly";
                            break;
                    }
                    if (moveto._indexRHS != null)
                    {
                        string indexrhs = SetupRHS(moveto._indexRHS);
                        if (indexrhs.Contains('[') || indexrhs.Contains(']')) ;
                        else
                        {
                            indexrhs = "[" + indexrhs + "]";
                        }
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "moveTo(" + destination + ", " + dir + ", until within: " + untilRhs + ", playing: " + animation + indexrhs + ")");
                    }
                    else
                    {
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "moveTo(" + destination + ", " + dir + ", until within: " + untilRhs + ", playing: " + animation + ")");
                    }


                    // move X to Y anim: Z
                }
                else if (codeList[i] is OpMoveFrom movefrom)
                {   // move from||working|until beyond|with anim
                    // "move A from B dir: C until beyond D with anim: E
                    string destination = SetupLHS(movefrom._LHS, codeList, i);
                    string untilRhs = SetupRHS(movefrom._RHS);
                    string animation = SetupLHS(movefrom._NP, codeList, i);
                    string dir = "";
                    switch (movefrom._dir)
                    {
                        case SetDirection.forward:
                            dir = "forward";
                            break;
                        case SetDirection.backward:
                            dir = "backward";
                            break;
                        case SetDirection.randomly:
                            dir = "randomly";
                            break;
                    }
                    if (movefrom._indexRHS != null)
                    {
                        string indexrhs = SetupRHS(movefrom._indexRHS);
                        if (indexrhs.Contains('[') || indexrhs.Contains(']')) ;
                        else
                        {
                            indexrhs = "[" + indexrhs + "]";
                        }
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "moveFrom(" + destination + ", " + dir + ", until beyond: " + untilRhs + ", playing: " + animation + indexrhs + ")");
                    }
                    else
                    {
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "moveFrom(" + destination + ", " + dir + ", until beyond: " + untilRhs + ", playing: " + animation + ")");
                    }
                }
                else if (codeList[i] is OpStartSequence opstart)
                {
                    string lhs = SetupLHS(opstart._LHS, codeList, i);
                    if (opstart._indexRHS != null)
                    {
                        string indexrhs = SetupRHS(opstart._indexRHS);
                        if (indexrhs.Contains('[') || indexrhs.Contains(']')) ;
                        else
                        {
                            indexrhs = "[" + indexrhs + "]";
                        }
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "start " + lhs + indexrhs);
                    }
                    else
                    {
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "start " + lhs);
                    }
                    if (opstart._branchPC != 0)
                    {
                        if (codeList[i + 1] is not OpFlowBuiltInBehavior)
                        {
                            returnedstring.AppendLine(new string(' ', indentCount * 3) + "{");
                            indentCount++;
                            sb.Clear();
                            if ((i + opstart._branchPC + 1) < codeList._count)
                            {
                                ParseScriptObjects(codeList, i + 1, opstart._branchPC);
                                i += opstart._branchPC;
                                if (indentCount != 0) indentCount--;
                            }
                            returnedstring.AppendLine(new string(' ', indentCount * 3) + "}");
                        }
                        else
                        {
                            i += opstart._branchPC;
                        }
                    }
                }
                else if (codeList[i] is OpCheckMembership checkm)
                {
                    string rhs = SetupLHS(checkm._RHS, codeList, i);
                    string lhs = SetupLHS(checkm._LHS, codeList, i);
                    switch (checkm._membershipOp)
                    {
                        case membershipTest.includes:
                            sb.Append("if (" + rhs + " in " + lhs + ")");
                            break;
                        case membershipTest.excludes:
                            sb.Append("if (" + rhs + " not in " + lhs + ")");
                            break;
                        case membershipTest.intersects_with:
                            sb.Append("if (" + rhs + " intersects with " + lhs + ")");
                            break;
                        case membershipTest.includes_all:
                            sb.Append("if (" + rhs + " all in " + lhs + ")");
                            break;
                    }
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + sb.ToString());
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "{");
                    sb.Clear();
                    if (checkm._branchPC != 0)
                    {
                        indentCount++;
                        if ((i + checkm._branchPC + 1) < codeList._count)
                        {
                            ParseScriptObjects(codeList, i + 1, checkm._branchPC);
                            i += checkm._branchPC;
                            if (indentCount != 0) indentCount--;
                        }
                        else
                        {
                            returnedstring.AppendLine(new string(' ', indentCount * 3) + "end");
                            if (indentCount != 0) indentCount--;
                        }
                    }
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "}");
                    // enum membershipTest: includes, excludes, intersects_with, includes_all
                    // includes: "if RHS in LHS"
                    // excludes: "if RHS not in LHS"
                    // intersects_with: ???
                    // includes_all: "if RHS in LHS"
                }
                else if (codeList[i] is OpDefineMacroSpecialization macrospec)
                {
                    string cachedMode = callordefinemacro;
                    callordefinemacro = "define";
                    if (macrospec._publicity == OpAbstractDefinition.Publicity.published)
                    {
                        returnedstring.Append(new string(' ', indentCount * 3) + "public macro (" + macrospec._name + ") : (" + macrospec._NP[0]._name + ") ");
                    }
                    else
                    {
                        returnedstring.Append(new string(' ', indentCount * 3) + "macro (" + macrospec._name + ") : (" + macrospec._NP[0]._name + ") ");
                    }
                    bool noparameters = (codeList[i + 2]._name == "flow macro");
                    if (noparameters && codeList[i + 3] is OpFlowBuiltInBehavior)
                    {
                        returnedstring.AppendLine();
                        i += macrospec._branchPC;
                        continue;
                    }
                    if (codeList[i + macrospec._branchPC] is OpFlowBuiltInBehavior flow)
                    {
                        int parameterCount = 0;
                        if (codeList[i + 1] is OpMacroInterface macinterface && !noparameters)
                        {
                            returnedstring.Append("(");
                            parameterCount = macinterface._branchPC - 1;
                            branchTargets.TryAdd(flow, "return");
                            ParseScriptObjects(codeList, i + 2, parameterCount); // skip the OpMacroInterface
                            i += (parameterCount + 3);
                        }
                        else
                        {
                            returnedstring.AppendLine("()");
                            branchTargets.TryAdd(flow, "return");
                            i += 3;
                        }
                        if (codeList[i] is not OpFlowBuiltInBehavior) // aka if there's more than just parameters
                        {
                            returnedstring.AppendLine(new string(' ', indentCount * 3) + "{");
                            indentCount++;
                            ParseScriptObjects(codeList, i, (macrospec._branchPC - 2) - parameterCount); // i believe this is correct
                            i += (macrospec._branchPC - 3) - parameterCount;
                            if (indentCount != 0) indentCount--;
                            returnedstring.AppendLine(new string(' ', indentCount * 3) + "}");
                        }
                        branchTargets.Remove(flow);
                    }
                    callordefinemacro = cachedMode;

                }
                else if (codeList[i] is OpDefineMacro defmacro)
                {
                    string cachedMode = callordefinemacro;
                    callordefinemacro = "define";
                    if (defmacro._publicity == OpAbstractDefinition.Publicity.published)
                    {
                        returnedstring.Append(new string(' ', indentCount * 3) + "public macro " + defmacro._name + " ");
                    }
                    else
                    {
                        returnedstring.Append(new string(' ', indentCount * 3) + "macro " + defmacro._name + " ");
                    }
                    bool noparameters = (codeList[i + 2]._name == "flow macro");
                    if (noparameters && codeList[i + 3] is OpFlowBuiltInBehavior)
                    {
                        returnedstring.AppendLine();
                        i += defmacro._branchPC;
                        continue;
                    }
                    if (codeList[i + defmacro._branchPC] is OpFlowBuiltInBehavior flow)
                    {
                        int parameterCount = 0;
                        if (codeList[i + 1] is OpMacroInterface macinterface && !noparameters)
                        {
                            returnedstring.Append("(");
                            parameterCount = macinterface._branchPC - 1;
                            branchTargets.TryAdd(flow, "return");
                            ParseScriptObjects(codeList, i + 2, parameterCount); // skip the OpMacroInterface
                            i += (parameterCount + 3);

                        }
                        else
                        {
                            returnedstring.AppendLine("()");
                            branchTargets.TryAdd(flow, "return");
                            i += 3;
                        }
                        branchTargets.TryAdd(flow, "return");
                        if (codeList[i] is not OpFlowBuiltInBehavior) // aka if there's more than just parameters
                        {
                            returnedstring.AppendLine(new string(' ', indentCount * 3) + "{");
                            indentCount++;
                            ParseScriptObjects(codeList, i, (defmacro._branchPC - 2) - parameterCount); // i believe this is correct
                            i += (defmacro._branchPC - 3) - parameterCount;
                            if (indentCount != 0) indentCount--;
                            returnedstring.AppendLine(new string(' ', indentCount * 3) + "}");

                        }
                        branchTargets.Remove(flow);
                    }
                    callordefinemacro = cachedMode;

                }
                else if (codeList[i] is OpRemove remove)
                {
                    sb.Append("remove ");
                    if (remove._NP != null)
                    {
                        string removedObj = SetupLHS(remove._NP, codeList, i);
                        sb.Append(removedObj);
                    }
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + sb.ToString());
                    sb.Clear();
                }
                else if (codeList[i] is OpLoopValue loopv)
                //      when saving, after all the OpCodes in the loop, at the end of the loop there should be an OpFlowLoop
                //      this object restarts the loop
                //
                { // "OpLoopValue(OpCreateVariable, IntMeasurement::"SetVariant.Count") --> loop (variable.count)
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "loop(" + SetupRHS(loopv._RHS) + ")");
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "{");
                    indentCount++;
                    if (loopv._branchPC != 0)
                    {
                        ParseScriptObjects(codeList, i + 1, loopv._branchPC);
                        i += loopv._branchPC;
                        if (indentCount != 0) indentCount--;
                    }
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "}");
                }
                else if (codeList[i] is OpSpawn spawn)
                {
                    if (spawn._LHS[0] is null)
                    {
                        sb.Append("(broken)");
                    }
                    else
                    {
                        switch (spawn._LHS[0].ToString().Split('.').Last())
                        {
                            case "ValueInfo":
                                sb.Append("valueinfo");
                                break;
                            case "tfbActorInfo":
                                sb.Append("actor");
                                break;
                            case "ActorWaypoint":
                                sb.Append("actorwaypoint");
                                break;
                            case "tfbSoundInfo":
                                sb.Append("sound");
                                break;
                            case "tfbSpriteInfo":
                                sb.Append("sprite");
                                break;
                            case "tfbParticleInfo":
                                sb.Append("particle");
                                break;
                            case "AnimationInfo":
                                sb.Append("animation");
                                break;
                            case "ScriptColorInfo":
                                sb.Append("scriptcolorinfo");
                                break;
                            case "Slider":
                                sb.Append("slider");
                                break;
                            case "ScriptController":
                                sb.Append("scriptcontroller");
                                break;
                            case "AbstractScriptGroup":
                                break;
                            case "ScriptStructure":
                                sb.Append("struct");
                                break;
                            case "tfbLightInfo":
                                sb.Append("light");
                                break;
                            case "OpCreateVariable":
                                sb.Append("variable");
                                break;
                            case "PlacementReference":
                                if (spawn._LHS[0] is PlacementReference placref)
                                {
                                    igExternalReferenceSystem.Singleton._globalSet.MakeReference(placref._type, null, out igHandleName name);
                                    igObject? EXNMTest = igExternalReferenceSystem.Singleton._globalSet.ResolveReference(name, null);
                                    if (EXNMTest == null) break;
                                    if (EXNMTest is igMetaObject ex)
                                        sb.Append("placementreference(" + ex._name + ")");
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    string spawndatatype = sb.ToString();
                    string spawnname = "";
                    if (spawn._LHS is not null)
                    {
                        spawnname = SetupLHS(spawn._LHS, codeList, i);
                    }
                    sb.Clear();
                    string spawnpos = ""; // at:
                    if (spawn._LHS._count != 0 && spawn._LHS[0] != null && spawn._LHS[0] is AbstractPlacement) // only spawned AbstractPlacements set a position/facing
                    {
                        if (spawn._RHS._count != 0)
                        {
                            spawnpos = SetupLHS(spawn._RHS, codeList, i);
                        }
                        string spawnfacing = SetupRHS(spawn._facingRHS);
                        amountOfSpawns++;
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "spawned spawnedObj" + amountOfSpawns + " = spawn(" + spawndatatype + " " + spawnname + ", " + spawnpos + ", " + spawnfacing + ")");
                    }
                    else // if it's not an abstractplacement it has no position or facing
                    {
                        amountOfSpawns++;
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "spawned spawnedObj" + amountOfSpawns + " = spawn(" + spawndatatype + " " + spawnname + ")");
                    }
                    spawnedobjects.TryAdd(spawn, "spawnedObj" + amountOfSpawns); // add
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "{");
                    indentCount++;
                    if (spawn._branchPC != 0)
                    {
                        if ((i + spawn._branchPC + 1) < codeList._count)
                        {
                            ParseScriptObjects(codeList, i + 1, spawn._branchPC);
                            i += spawn._branchPC;
                            if (indentCount != 0) indentCount--;
                        }
                    }
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "}");

                }
                else if (codeList[i] is OpChangeMembership changeme)
                {
                    string lhs = SetupLHS(changeme._LHS, codeList, i);
                    string rhs = SetupLHS(changeme._RHS, codeList, i);
                    string combineop = "null";
                    switch (changeme._combineOp)
                    {
                        case Combiner.include:
                            combineop = "|include|";
                            break;
                        case Combiner.exclude:
                            combineop = "|exclude|";
                            break;
                        case Combiner.intersect_with:
                            combineop = "|intersect_with";
                            break;
                        case Combiner.be_replaced_by:
                            combineop = "|be_replaced_by|";
                            break;
                        case Combiner.add:
                            combineop = "|add|";
                            break;
                        case Combiner.exclude_all:
                            combineop = "|exclude_all|";
                            break;
                    }
                    if (changeme._combineOp is Combiner.include)
                    {
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + lhs + ".Include(" + rhs + ")");
                    }
                    else if (changeme._combineOp is Combiner.exclude)
                    {
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + lhs + ".Remove(" + rhs + ")");
                    }
                    else if (changeme._combineOp is Combiner.be_replaced_by)
                    {
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + lhs + ".ReplaceWith(" + rhs + ")");
                    }
                    else if (changeme._combineOp is Combiner.add)
                    {
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + lhs + ".Add(" + rhs + ")");
                    }
                    else if (changeme._combineOp is Combiner.exclude_all)
                    {
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + lhs + ".RemoveAll(" + rhs + ")");
                    }
                    else
                    {
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "//changemembership has unimplemented CombineOp (" + combineop + ") LHS:" + lhs + " RHS: " + rhs);
                    }
                }
                else if (codeList[i] is OpStopSequence stop)
                {
                    if (stop._NP != null)
                    {
                        string lhs = SetupLHS(stop._NP, codeList, i);
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "stop " + lhs);
                    }
                    else
                    {
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "stop");
                    }
                }
                else if (codeList[i] is OpMacroParameter macropar)
                {
                    bool isList = false;
                    if (macropar._RHS is RHSReferenceStack rhsrefr)
                    {
                        if (rhsrefr._data.GetCount() != 0 && rhsrefr._data[0] is ScriptSet && macropar._indexRHS is null)
                        {
                            isList = true;
                        }
                    }
                    if (macropar._varContentsType is OpDefineStructure)
                    {
                        sb.Append("struct [" + macropar._varName + "]");
                    }
                    else if (macropar._varContentsType is OpDefineMacro)
                    {
                        sb.Append("macro [" + macropar._varName + "]");
                    }
                    else
                    {
                        igExternalReferenceSystem.Singleton._globalSet.MakeReference((igMetaObject)macropar._varContentsType, null, out igHandleName varContentsName);
                        igObject? VarContents = igExternalReferenceSystem.Singleton._globalSet.ResolveReference(varContentsName, null);
                        if (VarContents is igMetaObject contents)
                        {
                            if (isList)
                            {
                                sb.Append("List<");
                            }
                            switch (contents._name)
                            {
                                case "IntMeasurement": // floats are stored as IntMeasurements with _RHS._varOp1._type = float
                                    if (macropar._RHS is ValueRHSVariant valuerhs)
                                    {
                                        igExternalReferenceSystem.Singleton._globalSet.MakeReference((igMetaObject)valuerhs._varOp1._type, null, out igHandleName varTypeHandle);
                                        igObject? varType = igExternalReferenceSystem.Singleton._globalSet.ResolveReference(varTypeHandle, null);
                                        if (varType is igMetaObject type)
                                        {
                                            if (type._name == "IntMeasurement")
                                            {
                                                sb.Append("int");
                                            }
                                            else if (type._name == "FloatMeasurement")
                                            {
                                                sb.Append("float");
                                            }
                                        }
                                    }
                                    break;
                                case "ColorMeasurement":
                                    sb.Append("color");
                                    break;
                                case "ScreenMeasurement":
                                    sb.Append("screenmeasurement");
                                    break;
                                case "ValueInfo":
                                    sb.Append("valueinfo");
                                    break;
                                case "tfbActorInfo":
                                    sb.Append("actor");
                                    break;
                                case "ActorWaypoint":
                                    sb.Append("actorwaypoint");
                                    break;
                                case "tfbSoundInfo":
                                    sb.Append("sound");
                                    break;
                                case "tfbSpriteInfo":
                                    sb.Append("sprite");
                                    break;
                                case "tfbParticleInfo":
                                    sb.Append("particle");
                                    break;
                                case "AnimationInfo":
                                    sb.Append("animation");
                                    break;
                                case "ScriptColorInfo":
                                    sb.Append("scriptcolorinfo");
                                    break;
                                case "Slider":
                                    sb.Append("slider");
                                    break;
                                case "ScriptController":
                                    sb.Append("scriptcontroller");
                                    break;
                                case "tfbLightInfo":
                                    sb.Append("light");
                                    break;
                                case "StringInfo":
                                    sb.Append("string");
                                    break;
                                case "OpUserBehavior":
                                    sb.Append("behavior");
                                    break;
                                default:
                                    sb.Append("UNIMP_MACROPAR::" + macropar.ToString().Split('.').Last() + " " + contents._name);
                                    break;

                            }
                            if (isList)
                            {
                                sb.Append(">");
                            }
                            sb.Append(" [" + macropar._varName + "]");
                        }
                    }
                    string paramdefinition = sb.ToString();
                    sb.Clear();
                    if (macropar._RHS is ValueRHSVariant valueR)
                    {
                        sb.Append(SetupRHS(valueR));
                        //if (valueR._varOp1._count == 0)
                        //{
                        //    string RHSCalc = SetupRHS(valueR);
                        //    sb.Append(RHSCalc);
                        //}
                        //else
                        //{
                        //    for (int ii = 0; ii < valueR._varOp1._count; ii++)
                        //    {
                        //        switch (valueR._varOp1[ii])
                        //        {
                        //            case OpControl:
                        //                sb.Append("[^controlled]");
                        //                break;
                        //            case OpFindSubSet:
                        //                sb.Append("[^subset]");
                        //                break;
                        //            case OpForEach opf:
                        //                string each = SetupLHS(opf._LHS, codeList, i);
                        //                sb.Append(each + ".current");
                        //                break;
                        //            case OpSlideValue:
                        //                sb.Append("[^slider]");
                        //                break;
                        //            case OpCreateVariable opv:
                        //                sb.Append("[" + opv._varName + "]");
                        //                break;
                        //            case OpMacroParameter opmacropar:
                        //                sb.Append("[" + opmacropar._varName + "]");
                        //                break;
                        //            case OpSpawn spawn1:
                        //                sb.Append(spawnedobjects[spawn1]);
                        //                break;
                        //            case ScriptSetReference: // "my" would look better as "myself"
                        //                if (valueR._varOp1[ii]._name == "my") sb.Append("myself");
                        //                break;
                        //            default:
                        //                if (valueR._varOp1[ii]._name.Split('.').Last().Contains(' '))
                        //                {
                        //                    sb.Append("(" + valueR._varOp1[ii]._name.ToString().Split('.').Last() + ")");
                        //                }
                        //                else
                        //                {
                        //                    sb.Append(valueR._varOp1[ii]._name.Split('.').Last());
                        //                }
                        //                break;
                        //        }
                        //        if (ii != (valueR._varOp1._count - 1)) sb.Append(".");
                        //    }
                        //}
                    }
                    else if (macropar._RHS is RHSReferenceStack rhsref)
                    {
                        if (rhsref._count != 0)
                        {
                            for (int o = 0; o < rhsref._count; o++)
                            {
                                switch (rhsref[o])
                                {
                                    case null:
                                        sb.Append("<actually_null>");
                                        break;
                                    case OpControl:
                                        sb.Append("[^controlled]");
                                        break;
                                    case OpFindSubSet:
                                        sb.Append("[^subset]");
                                        break;
                                    case OpForEach opf:
                                        string each = SetupLHS(opf._LHS, codeList, i);
                                        sb.Append(each + ".current");
                                        break;
                                    case OpSlideValue:
                                        sb.Append("[^slider]");
                                        break;
                                    case OpAbstractCreateVariable opv:
                                        sb.Append("[" + opv._varName + "]");
                                        break;
                                    case OpSpawn spawn1:
                                        sb.Append(spawnedobjects[spawn1]);
                                        break;
                                    case AbstractScriptVariant absvar:
                                        switch (absvar._name)
                                        {
                                            case "OpTopLevelBehavior.my":
                                                sb.Append("myself");
                                                break;
                                            case null:
                                                sb.Append(absvar.ToString().Split('.').Last());
                                                break;
                                            default:
                                                sb.Append(absvar._name.ToString().Split('.').Last());
                                                break;
                                        }
                                        break;
                                    default:
                                        sb.Append(rhsref[o]._name.ToString().Split('.').Last());
                                        break;
                                }
                                if (o != (rhsref._count - 1)) sb.Append(".");
                            }
                        }
                        else
                        {
                            string rhsrefvalue = SetupRHS(rhsref);
                            sb.Append(rhsrefvalue);
                        }
                    }
                    else if (macropar._RHS is ScriptGroupStack sgs)
                    {
                        //sb.Append(" = ");
                        string sgsString = SetupLHS(sgs, codeList, i);
                        sb.Append(sgsString);
                    }
                    else
                    {
                        sb.AppendLine("UNKNOWN RHS CONTAINER? " + macropar._RHS);
                    }
                    if (macropar._indexRHS != null)
                    {
                        string indexrhs = SetupRHS(macropar._indexRHS);
                        if (indexrhs.Contains('[') || indexrhs.Contains(']')) ;
                        else
                        {
                            indexrhs = "[" + indexrhs + "]";
                        }
                        sb.Append(indexrhs);
                    }
                    string paramvalue = sb.ToString();
                    sb.Clear();
                    if (macropar._combineOp is not Combiner.include)
                    {
                        //switch (macropar._combineOp)
                        //{
                        //    case Combiner.exclude:
                        //        sb.Append("// exclude");
                        //        break;
                        //    case Combiner.be_replaced_by:
                        //        sb.Append("// be replaced by");
                        //        break;
                        //    case Combiner.intersect_with:
                        //        sb.Append("// intersect with");
                        //        break;
                        //    case Combiner.exclude_all:
                        //        sb.Append("// exclude all");
                        //        break;
                        //    case Combiner.add:
                        //        sb.Append("// add");
                        //        break;
                        //}
                    }
                    if (callordefinemacro == "call")
                    {
                        if (paramvalue == "null")
                        {
                            sb.Append("out " + paramdefinition);
                        }
                        else
                        {
                            sb.Append(paramvalue);
                        }
                    }
                    else
                    {
                        if (paramvalue == "null")
                        {
                            sb.Append(paramdefinition);
                        }
                        else
                        {
                            sb.Append(paramdefinition + " = " + paramvalue);
                        }
                    }
                    if (codeList[i + 1] is OpMacroParameter)
                    {
                        sb.Append(", "); //if this isnt the last param
                        returnedstring.Append(sb.ToString());
                        sb.Clear();
                    }
                    else
                    {
                        sb.Append(")"); // closing the UseMacro
                        returnedstring.AppendLine(sb.ToString());
                        sb.Clear();
                    }
                }
                else if (codeList[i] is OpSetBehavior setbehavior)
                {
                    if (setbehavior._NP[0] is OpUserBehavior behavior)
                    {
                        if (behaviors.ContainsKey(behavior))
                        {
                            returnedstring.AppendLine(new string(' ', indentCount * 3) + "setBehavior(" + behaviors[behavior] + ")");
                        }
                        else
                        {
                            amountOfBehaviors++;
                            behaviors.Add(behavior, "behavior" + amountOfBehaviors);
                            returnedstring.AppendLine(new string(' ', indentCount * 3) + "setBehavior(" + behaviors[behavior] + ")");
                        }
                    }

                }
                else if (codeList[i] is OpUserBehavior behavior)
                {
                    if (behaviors.ContainsKey(behavior))
                    {
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "#" + behaviors[behavior]);
                    }
                    else
                    {
                        amountOfBehaviors++;
                        behaviors.Add(behavior, "behavior" + amountOfBehaviors);
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "#" + behaviors[behavior]);
                    }
                    if (behavior._branchPC != 0)
                    {
                        ParseScriptObjects(codeList, i + 1, behavior._branchPC);
                        i += behavior._branchPC;
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "#end" + behaviors[behavior]);
                    }
                }
                else if (codeList[i] is OpControl control)
                {
                    string lhs = SetupLHS(control._LHS, codeList, i);
                    if (control._indexRHS != null)
                    {
                        string indexrhs = SetupRHS(control._indexRHS);
                        if (indexrhs.Contains('[') || indexrhs.Contains(']')) ;
                        else
                        {
                            indexrhs = "[" + indexrhs + "]";
                        }
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "control (" + lhs + indexrhs + ")");
                    }
                    else
                    {
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "control (" + lhs + ")");
                    }
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "{");
                    sb.Clear();
                    if (control._branchPC != 0)
                    {
                        indentCount++;
                        if ((i + control._branchPC + 1) < codeList._count)
                        {
                            ParseScriptObjects(codeList, i + 1, control._branchPC);
                            i += control._branchPC;
                        }
                        if (indentCount != 0) indentCount--;
                    }
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "}");

                }
                else if (codeList[i] is OpCheckFOV checkfov)
                {   // to find it easily (placeholder)

                    // if (distance (in a cone of sight) from _fromlhs, facing _RHSfacing, angle fov, to LHS is (RelOp comparing) to RHS
                    // (ignoring or considering obstructions)
                    string fromlhs = SetupLHS(checkfov._fromLHS, codeList, i);
                    string facing = SetupRHS(checkfov._RHSfacing);
                    string fov = SetupRHS(checkfov._RHSfov);
                    string lhs = SetupLHS(checkfov._LHS, codeList, i);
                    string relop = "";
                    switch (checkfov._relOperator)
                    {
                        case RelOp.Eq:
                            relop = "== ";
                            break;
                        case RelOp.NotEq:
                            relop = "!= ";
                            break;
                        case RelOp.Less:
                            relop = "< ";
                            break;
                        case RelOp.LessOrEq:
                            relop = "<= ";
                            break;
                        case RelOp.Great:
                            relop = "> ";
                            break;
                        case RelOp.GreatOrEq:
                            relop = ">= ";
                            break;
                    }
                    string rhs = SetupRHS(checkfov._RHS);
                    string mode = "";
                    switch (checkfov._mode)
                    {
                        case obstructMode.ignore_obstructions:
                            mode = "ignore_obstructions";
                            break;
                        case obstructMode.consider_obstructions:
                            mode = "consider_obstructions";
                            break;
                    }
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "checkFOV(" + fromlhs + ", " + lhs + " " + relop + "cone(" + fov + "°, " + rhs + ", " + facing + "), " + mode + ")");
                    if (checkfov._branchPC != 0)
                    {
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "{");
                        indentCount++;
                        if ((i + checkfov._branchPC + 1) < codeList._count)
                        {
                            ParseScriptObjects(codeList, i + 1, checkfov._branchPC);
                            i += checkfov._branchPC;
                            if (indentCount != 0) indentCount--;
                        }
                        else
                        { // dont I need to write "else { end }"
                            returnedstring.AppendLine(new string(' ', indentCount * 3) + "end"); // branching outside the scripts bounds means the script ends.

                        }
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "}");
                    }
                }
                else if (codeList[i] is OpSlideValue opslide)
                {
                    string lhs = SetupLHS(opslide._LHS, codeList, i);
                    string rhs = SetupRHS(opslide._RHS);
                    string seconds = SetupRHS(opslide._secondsRHS);
                    string easeout = SetupRHS(opslide._easeOutRHS);
                    string easein = SetupRHS(opslide._easeInRHS);

                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "slidevalue " + lhs + " to " + rhs + " (" + seconds + "s, " + easeout + ", " + easein + ")");
                    //actor.slideValue(100, 1, 1) //double check this
                }
                else if (codeList[i] is OpSetReference setref)
                {
                    string lhs = SetupLHS(setref._LHS, codeList, i);
                    string rhs = ReadRHSObjects(setref._RHS);
                    if (setref._indexRHS != null)
                    {
                        string indexrhs = SetupRHS(setref._indexRHS);
                        if (indexrhs.Contains('[') || indexrhs.Contains(']')) ;
                        else
                        {
                            indexrhs = "[" + indexrhs + "]";
                        }
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + lhs + " = " + rhs + indexrhs);
                    }
                    else
                    {
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + lhs + " = " + rhs);
                    }
                }
                else if (codeList[i] is OpCheckReference checkref)
                {
                    // todo: What is the _editType and _type used for in RHSReferenceStack?
                    //       (will need to figure that out for saving)
                    if (codeList[i - 1].ToString().Split('.').Last() == "OpAbstractFlow")
                    {
                        sb.Append("elseif (");
                    }
                    else
                    {
                        sb.Append("if (");
                    }
                    string LeftHandStack = SetupLHS(checkref._LHS, codeList, i); //
                    sb.Append(LeftHandStack);
                    switch (checkref._relOperator)
                    {
                        case OpCheckReference.BoolOp.beq:
                            sb.Append(" == ");
                            break;
                        case OpCheckReference.BoolOp.bne:
                            sb.Append(" != ");
                            break;
                    }
                    if (checkref._RHS._count != 0)
                    {
                        sb.Append(ReadRHSObjects(checkref._RHS));
                    }
                    else
                    {
                        sb.Append(SetupRHS(checkref._RHS));
                    }
                    if (checkref._indexRHS != null)
                    {
                        string indexrhs = SetupRHS(checkref._indexRHS);
                        if (indexrhs.Contains('[') || indexrhs.Contains(']')) ;
                        else
                        {
                            indexrhs = "[" + indexrhs + "]";
                        }
                        sb.Append(indexrhs);
                    }
                    sb.Append(")");
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + sb.ToString());
                    sb.Clear();
                    if (checkref._branchPC != 0)
                    {
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "{");
                        indentCount++;
                        if ((i + checkref._branchPC + 1) < codeList._count)
                        {
                            ParseScriptObjects(codeList, i + 1, checkref._branchPC);
                            i += checkref._branchPC;
                            if (indentCount != 0) indentCount--;
                        }
                        else
                        { // dont I need to write "else { end }"
                            returnedstring.AppendLine(new string(' ', indentCount * 3) + "end"); // branching outside the scripts bounds means the script ends.

                        }
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "}");
                    }

                }

                else if (codeList[i] is OpCheckValue check)
                {
                    if (codeList[i - 1].ToString().Split('.').Last() == "OpAbstractFlow")
                    {
                        sb.Append("elseif (");
                    }
                    else
                    {
                        sb.Append("if (");
                    }
                    string LeftHandStack = SetupLHS(check._LHS, codeList, i);
                    sb.Append(LeftHandStack + " ");
                    switch (check._relOperator)
                    {
                        case RelOp.Eq:
                            sb.Append("== ");
                            break;
                        case RelOp.NotEq:
                            sb.Append("!= ");
                            break;
                        case RelOp.Less:
                            sb.Append("< ");
                            break;
                        case RelOp.LessOrEq:
                            sb.Append("<= ");
                            break;
                        case RelOp.Great:
                            sb.Append("> ");
                            break;
                        case RelOp.GreatOrEq:
                            sb.Append(">= ");
                            break;
                    }
                    string RightHandStack = SetupRHS(check._RHS);
                    sb.Append(RightHandStack + ")");
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + sb.ToString());
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "{");
                    sb.Clear();
                    if (check._branchPC != 0)
                    {
                        indentCount++;
                        if ((i + check._branchPC + 1) < codeList._count)
                        {
                            ParseScriptObjects(codeList, i + 1, check._branchPC);
                            i += check._branchPC;
                            if (indentCount != 0) indentCount--;
                        }
                        else
                        {
                            returnedstring.AppendLine(new string(' ', indentCount * 3) + "end");
                            if (indentCount != 0) indentCount--;
                        }
                    }
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "}");

                }
                else if (codeList[i] is OpFindVariable findvariable)
                {

                    string owner = SetupLHS(findvariable._owner, codeList, i);
                    string filter = "";
                    switch (findvariable._filter)
                    { // I've only ever seen "in_script" so I might make it the default and invisible
                        case findFilter.in_script:
                            filter = "in_script";
                            break;
                        case findFilter.as_tag:
                            filter = "as_tag";
                            break;
                        case findFilter.force_tag:
                            filter = "force_tag";
                            break;
                        case findFilter.remove_tag:
                            filter = "remove_tag";
                            break;
                        case findFilter.count_typed_tags:
                            filter = "count_typed_tags";
                            break;
                    }
                    sb.Append("[" + findvariable._varName + "]");
                    if (findvariable._indexRHS != null)
                    {
                        string indexrhs = SetupRHS(findvariable._indexRHS);
                        if (indexrhs.Contains('[') || indexrhs.Contains(']')) ;
                        else
                        {
                            indexrhs = "[" + indexrhs + "]";
                        }
                        sb.Append(indexrhs);
                    }
                    string variable = sb.ToString();
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "find " + variable + " in " + owner + " // filter: " + filter);
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "{");
                    indentCount++;
                    sb.Clear();
                    if (findvariable._branchPC != 0)
                    {
                        if ((i + findvariable._branchPC + 1) < codeList._count)
                        {
                            ParseScriptObjects(codeList, i + 1, findvariable._branchPC);
                            i += findvariable._branchPC;
                            if (indentCount != 0) indentCount--;
                        }
                        else
                        {
                            returnedstring.AppendLine(new string(' ', indentCount * 3) + "end");
                            if (indentCount != 0) indentCount--;
                        }
                    }
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "}");

                }
                else if (codeList[i] is OpFindSubSet opfindsubset)
                {
                    ValueStack listLHS = new ValueStack();
                    ValueStack conditionLHS = new ValueStack();
                    bool partoflist = true;
                    foreach (var obj in opfindsubset._LHS)
                    {
                        if (obj is null)
                        {
                            partoflist = false;
                        }
                        else
                        {
                            if (partoflist)
                            {
                                listLHS.Add(obj);
                            }
                            else
                            {
                                conditionLHS.Add(obj);
                            }

                        }
                    }
                    string relop = "";
                    switch (opfindsubset._relOperator)
                    {
                        case RelOp.Eq:
                            relop = "== ";
                            break;
                        case RelOp.NotEq:
                            relop = "!= ";
                            break;
                        case RelOp.Less:
                            relop = "< ";
                            break;
                        case RelOp.LessOrEq:
                            relop = "<= ";
                            break;
                        case RelOp.Great:
                            relop = "> ";
                            break;
                        case RelOp.GreatOrEq:
                            relop = ">= ";
                            break;
                    }
                    string listLHSstring = SetupLHS(listLHS, codeList, i);
                    string condLHSstring = SetupLHS(conditionLHS, codeList, i);
                    string rhs = SetupRHS(opfindsubset._RHS);
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "filterList(" + listLHSstring + ",");
                    indentCount++;
                    string otherpoint = "";
                    if (opfindsubset._LHS._otherPoint != null)
                    {
                        otherpoint = SetupLHS(opfindsubset._LHS._otherPoint, codeList, i);
                        if (opfindsubset._LHS[opfindsubset._LHS._count - 1] is PairedEnumMeasurement || opfindsubset._LHS[opfindsubset._LHS._count - 1] is PairedFloatMeasurement)
                        {
                            igNamedObject paired = opfindsubset._LHS[opfindsubset._LHS._count - 1] as igNamedObject;
                            string condition = "";
                            switch (paired._name.Split('.').Last())
                            {
                                case "collision":
                                    if (condLHSstring == "collision")
                                    {
                                        condition = ((int.Parse(rhs) == 1) ? "CollidesWith(" : "!CollidesWith(") + otherpoint + ")";
                                    }
                                    else
                                    {
                                        condition = condLHSstring.Substring(0, condLHSstring.LastIndexOf('.')) + ((int.Parse(rhs) == 1) ? ".CollidesWith(" : ".!CollidesWith(") + otherpoint + ")";
                                    }
                                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "condition: " + condition + ")");
                                    indentCount--;
                                    continue;
                                case "distance":
                                    if (condLHSstring == "distance")
                                    {
                                        condition = "DistanceTo(" + otherpoint + ")";
                                    }
                                    else
                                    {
                                        condition = condLHSstring.Substring(0, condLHSstring.LastIndexOf('.')) + "DistanceTo(" + otherpoint + ")";
                                    }
                                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "condition: " + condition + ")");
                                    indentCount--;
                                    continue;
                                case "separation":
                                    if (condLHSstring == "separation")
                                    {
                                        condition = "SeparationTo(" + otherpoint + ")";
                                    }
                                    else
                                    {
                                        condition = condLHSstring.Substring(0, condLHSstring.LastIndexOf('.')) + "SeparationTo(" + otherpoint + ")";
                                    }
                                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "condition: " + condition + ")");
                                    indentCount--;
                                    continue;
                                default:
                                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "condition:) // unimplemented condition (paired): " + condLHSstring);
                                    break;
                            }
                        }
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "condition: " + condLHSstring + " " + relop + " " + rhs + ") // otherpoint: " + otherpoint);
                        indentCount--;
                        continue;
                    }
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "condition: " + condLHSstring + " " + relop + " " + rhs + ")");
                    indentCount--;
                }
                else if (codeList[i] is OpFlowBehavior endflow)
                {
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "end");
                }
                else if (codeList[i] is OpForEach opforeach)
                {
                    string lhs = SetupLHS(opforeach._LHS, codeList, i);
                    string direction = "";
                    switch (opforeach._dir)
                    {
                        case SetDirection.forward:
                            direction = "forward";
                            break;
                        case SetDirection.backward:
                            direction = "backward";
                            break;
                        case SetDirection.randomly:
                            direction = "randomly";
                            break;
                    }
                    string offsetby = SetupRHS(opforeach._RHS);
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "foreach (current in " + lhs + ", " + direction + ", " + offsetby + ")");
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "{");
                    indentCount++;
                    if (opforeach._branchPC != 0)
                    {
                        ParseScriptObjects(codeList, i + 1, opforeach._branchPC);
                        i += opforeach._branchPC;
                        if (indentCount != 0) indentCount--;
                    }
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "}");
                }


                // else if (codeList[i] is OpPrint printOp)
                // this is never used ingame outside the 3ds games
                // would be neat to have, but i'm unsure if the code for it remains in the other executables

                else if (codeList[i] is OpTeleportTo optp)
                {
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "teleportTo");
                }
                else if (codeList[i] is OpDisplace opdisp)
                {
                    string displaced = SetupLHS(opdisp._NP, codeList, i);
                    sb.Clear();
                    string opdisplength;
                    if (opdisp._lenNP._varOp1._count == 0)
                    {
                        opdisplength = SetupRHS(opdisp._lenNP);
                    }
                    else
                    {
                        opdisplength = ReadRHSObjects(opdisp._lenNP._varOp1);
                    }
                    string opdispheading;
                    if (opdisp._headNP._varOp1._count == 0)
                    {
                        opdispheading = SetupRHS(opdisp._headNP);
                    }
                    else
                    {
                        opdispheading = ReadRHSObjects(opdisp._headNP._varOp1);
                    }
                    string opdisppitch;
                    if (opdisp._pitchNP._varOp1._count == 0)
                    {
                        opdisppitch = SetupRHS(opdisp._pitchNP);
                    }
                    else
                    {
                        opdisppitch = ReadRHSObjects(opdisp._pitchNP._varOp1);
                    }
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + displaced + ".moverelative(" + opdisplength + ", " + opdispheading + ", " + opdisppitch + ")");
                    sb.Clear();
                }
                else if (codeList[i] is OpIncValue opinc)
                {
                    string lhs = SetupLHS(opinc._LHS, codeList, i);
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + lhs + "++");
                }
                else if (codeList[i] is OpDecValue opdec)
                {
                    string lhs = SetupLHS(opdec._LHS, codeList, i);
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + lhs + "--");
                }
                else if (codeList[i] is OpAbstractFlow absflow && codeList[i].ToString().Split('.').Last() == "OpAbstractFlow")
                { // if i only check the type (not string), all things with basetype OpAbstractFlow will pass the check
                  // todo: completely rework this (see notes)

                    if ((i + absflow._branchPC + 1) > codeList._count)
                    {
                        sb.Append("end");
                        if (indentCount != 0) indentCount--;
                    }
                    else
                    {
                        if (codeList[i + absflow._branchPC + 1] is OpFlowLoop)
                        {
                            sb.Append("continue");
                        }
                        else if (codeList[i + absflow._branchPC + 1] is OpFlowBuiltInBehavior flow && branchTargets.ContainsKey(flow))
                        {
                            sb.Append(branchTargets[flow]);
                        }
                        else if (codeList[i + 1] is OpCheckValue || codeList[i + 1] is OpCheckReference)
                        {
                            continue;
                        }
                        else
                        {
                            sb.Append("(unimpl.) goto");
                        }
                    }
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + sb.ToString());
                    sb.Clear();
                }
                else if (codeList[i] is OpUseMacro opuse)
                {   //todo: i think this can work in more ways than I've implemented
                    string cachedMode = callordefinemacro;
                    callordefinemacro = "call";
                    if (opuse._NP._editType is OpDefineMacro opdefmac) returnedstring.Append(new string(' ', indentCount * 3) + "callmacro " + opdefmac._name + " ");
                    if (opuse._branchPC != 0)
                    {
                        if (codeList[i + 1] is OpMacroInterface macrointerface)
                        {
                            if (opuse._branchPC == 3) //if this macro doesnt have input/output and does nothing
                            {
                                returnedstring.AppendLine("()");
                                i += opuse._branchPC;
                                continue;
                            }
                            else
                            {
                                int parameterCount = macrointerface._branchPC - 1;
                                if (parameterCount == 0)
                                {
                                    returnedstring.AppendLine("()");
                                }
                                else
                                {
                                    returnedstring.Append("(");
                                    ParseScriptObjects(codeList, i + 2, parameterCount); // skip the OpMacroInterface
                                }
                                i += (parameterCount + 3); //jump over macrointerface and both flowbuiltinbehaviors
                                if (codeList[i] is not OpFlowBuiltInBehavior) // aka if there's more than just parameters
                                {
                                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "{");
                                    indentCount++;
                                    ParseScriptObjects(codeList, i, (opuse._branchPC - 2) - parameterCount); // i believe this is correct
                                    i += (opuse._branchPC - 3) - parameterCount;
                                    if (indentCount != 0) indentCount--;
                                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "}");
                                }
                            }
                        }
                    }
                    callordefinemacro = cachedMode;
                }
                else if (codeList[i] is OpDefineStructure opDefStruct)
                {
                    if (opDefStruct._publicity == OpAbstractDefinition.Publicity.published)
                    {
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "public struct " + opDefStruct._name);
                    }
                    else
                    {
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "struct " + opDefStruct._name);
                    }
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "{");
                    if (opDefStruct._branchPC != 0)
                    {
                        indentCount++;
                        if ((i + opDefStruct._branchPC + 1) < codeList._count)
                        {
                            ParseScriptObjects(codeList, i + 1, opDefStruct._branchPC - 1);
                            i += opDefStruct._branchPC;
                            if (indentCount != 0) indentCount--;
                        }
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "}");
                    }
                }
                else if (codeList[i] is OpReset reset)
                {
                    string np = SetupLHS(reset._NP, codeList, i);
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "reset " + np);
                }
                else if (codeList[i] is OpCreateVariable opCreateVar)
                {
                    igExternalReferenceSystem.Singleton._globalSet.MakeReference((igMetaObject)opCreateVar._varContainerType, null, out igHandleName varContainerName);
                    igObject? VarContainer = igExternalReferenceSystem.Singleton._globalSet.ResolveReference(varContainerName, null);
                    bool isList = false;
                    if (VarContainer is igMetaObject container && container._name.Split('.').Last() == "ScriptSet")
                    {
                        isList = true;
                    }
                    if (opCreateVar._varContentsType is OpDefineStructure defineStructure)
                    {
                        if (isList)
                        {
                            sb.Append("List<struct" + defineStructure._name + "> [" + opCreateVar._varName + "]");
                            localvariables.TryAdd(opCreateVar, "List<struct " + defineStructure._name + ">");
                        }
                        else
                        {
                            sb.Append("struct " + defineStructure._name + " [" + opCreateVar._varName + "]");
                            localvariables.TryAdd(opCreateVar, "struct " + defineStructure._name);
                        }
                    }
                    else if (opCreateVar._varContentsType is OpDefineMacro defineMacro)
                    {
                        if (isList)
                        {
                            sb.Append("List<macro " + defineMacro._name + "> [" + opCreateVar._varName + "]");
                            localvariables.TryAdd(opCreateVar, "List<macro " + defineMacro._name + ">");
                        }
                        else
                        {
                            sb.Append("macro " + defineMacro._name + " [" + opCreateVar._varName + "]");
                            localvariables.TryAdd(opCreateVar, "macro " + defineMacro._name);
                        }
                    }
                    else
                    {
                        igExternalReferenceSystem.Singleton._globalSet.MakeReference((igMetaObject)opCreateVar._varContentsType, null, out igHandleName varContentsName);
                        igObject? VarContents = igExternalReferenceSystem.Singleton._globalSet.ResolveReference(varContentsName, null);
                        if (VarContents is igMetaObject contents)
                        {
                            switch (contents._name)
                            {
                                case "IntMeasurement":
                                    if (isList)
                                    {
                                        localvariables.TryAdd(opCreateVar, "List<int>");
                                        sb.Append("List<int> [" + opCreateVar._varName + "]");
                                    }
                                    else
                                    {
                                        localvariables.TryAdd(opCreateVar, "int");
                                        sb.Append("int [" + opCreateVar._varName + "]");
                                    }
                                    break;
                                case "FloatMeasurement":
                                    if (isList)
                                    {
                                        localvariables.TryAdd(opCreateVar, "List<float>");
                                        sb.Append("List<float> [" + opCreateVar._varName + "]");
                                    }
                                    else
                                    {
                                        localvariables.TryAdd(opCreateVar, "float");
                                        sb.Append("float [" + opCreateVar._varName + "]");
                                    }
                                    break;
                                case "ColorMeasurement":
                                    if (isList)
                                    {
                                        localvariables.TryAdd(opCreateVar, "List<color>");
                                        sb.Append("List<color> [" + opCreateVar._varName + "]");
                                    }
                                    else
                                    {
                                        localvariables.TryAdd(opCreateVar, "color");
                                        sb.Append("color [" + opCreateVar._varName + "]");
                                    }
                                    break;
                                case "ValueInfo":
                                    if (isList)
                                    {
                                        localvariables.TryAdd(opCreateVar, "List<valueinfo>");
                                        sb.Append("List<valueinfo> [" + opCreateVar._varName + "]");
                                    }
                                    else
                                    {
                                        localvariables.TryAdd(opCreateVar, "valueinfo");
                                        sb.Append("valueinfo [" + opCreateVar._varName + "]");
                                    }
                                    break;
                                case "tfbActorInfo":
                                    if (isList)
                                    {
                                        localvariables.TryAdd(opCreateVar, "List<actor>");
                                        sb.Append("List<actor> [" + opCreateVar._varName + "]");
                                    }
                                    else
                                    {
                                        localvariables.TryAdd(opCreateVar, "actor");
                                        sb.Append("actor [" + opCreateVar._varName + "]");
                                    }
                                    break;
                                case "ActorWaypoint":
                                    if (isList)
                                    {
                                        localvariables.TryAdd(opCreateVar, "List<actorwaypoint>");
                                        sb.Append("List<actorwaypoint> [" + opCreateVar._varName + "]");
                                    }
                                    else
                                    {
                                        localvariables.TryAdd(opCreateVar, "actorwaypoint");
                                        sb.Append("actorwaypoint [" + opCreateVar._varName + "]");
                                    }
                                    break;
                                case "ScriptScreenInfo":
                                    if (isList)
                                    {
                                        localvariables.TryAdd(opCreateVar, "List<scriptscreeninfo>");
                                        sb.Append("List<scriptscreeninfo> [" + opCreateVar._varName + "]");
                                    }
                                    else
                                    {
                                        localvariables.TryAdd(opCreateVar, "scriptscreeninfo");
                                        sb.Append("scriptscreeninfo [" + opCreateVar._varName + "]");
                                    }
                                    break;
                                case "tfbSoundInfo":
                                    if (isList)
                                    {
                                        localvariables.TryAdd(opCreateVar, "List<sound>");
                                        sb.Append("List<sound> [" + opCreateVar._varName + "]");
                                    }
                                    else
                                    {
                                        localvariables.TryAdd(opCreateVar, "sound");
                                        sb.Append("sound [" + opCreateVar._varName + "]");
                                    }
                                    break;
                                case "tfbSpriteInfo":
                                    if (isList)
                                    {
                                        localvariables.TryAdd(opCreateVar, "List<sprite>");
                                        sb.Append("List<sprite> [" + opCreateVar._varName + "]");
                                    }
                                    else
                                    {
                                        localvariables.TryAdd(opCreateVar, "sprite");
                                        sb.Append("sprite [" + opCreateVar._varName + "]");
                                    }
                                    break;
                                case "tfbParticleInfo":
                                    if (isList)
                                    {
                                        localvariables.TryAdd(opCreateVar, "List<particle>");
                                        sb.Append("List<particle> [" + opCreateVar._varName + "]");
                                    }
                                    else
                                    {
                                        localvariables.TryAdd(opCreateVar, "particle");
                                        sb.Append("particle [" + opCreateVar._varName + "]");
                                    }
                                    break;
                                case "AnimationInfo":
                                    if (isList)
                                    {
                                        localvariables.TryAdd(opCreateVar, "List<animation>");
                                        sb.Append("List<animation> [" + opCreateVar._varName + "]");
                                    }
                                    else
                                    {
                                        localvariables.TryAdd(opCreateVar, "animation");
                                        sb.Append("animation [" + opCreateVar._varName + "]");
                                    }
                                    break;
                                case "ScriptColorInfo":
                                    if (isList)
                                    {
                                        localvariables.TryAdd(opCreateVar, "List<scriptcolorinfo>");
                                        sb.Append("List<scriptcolorinfo> [" + opCreateVar._varName + "]");
                                    }
                                    else
                                    {
                                        localvariables.TryAdd(opCreateVar, "scriptcolorinfo");
                                        sb.Append("scriptcolorinfo [" + opCreateVar._varName + "]");
                                    }
                                    break;
                                case "Slider":
                                    if (isList)
                                    {
                                        localvariables.TryAdd(opCreateVar, "List<slider>");
                                        sb.Append("List<slider> [" + opCreateVar._varName + "]");
                                    }
                                    else
                                    {
                                        localvariables.TryAdd(opCreateVar, "slider");
                                        sb.Append("slider [" + opCreateVar._varName + "]");
                                    }
                                    break;
                                case "ScriptController":
                                    if (isList)
                                    {
                                        localvariables.TryAdd(opCreateVar, "List<scriptcontroller>");
                                        sb.Append("List<scriptcontroller> [" + opCreateVar._varName + "]");
                                    }
                                    else
                                    {
                                        localvariables.TryAdd(opCreateVar, "scriptcontroller");
                                        sb.Append("scriptcontroller [" + opCreateVar._varName + "]");
                                    }
                                    break;
                                case "tfbLightInfo":
                                    if (isList)
                                    {
                                        localvariables.TryAdd(opCreateVar, "List<light>");
                                        sb.Append("List<light> [" + opCreateVar._varName + "]");
                                    }
                                    else
                                    {
                                        localvariables.TryAdd(opCreateVar, "light");
                                        sb.Append("light [" + opCreateVar._varName + "]");
                                    }
                                    break;
                                case "ScreenMeasurement":
                                    if (isList)
                                    {
                                        localvariables.TryAdd(opCreateVar, "List<screenmeasurement>");
                                        sb.Append("List<screenmeasurement> [" + opCreateVar._varName + "]");
                                    }
                                    else
                                    {
                                        localvariables.TryAdd(opCreateVar, "screenmeasurement");
                                        sb.Append("screenmeasurement [" + opCreateVar._varName + "]");
                                    }
                                    break;
                                case "StringInfo":
                                    if (isList)
                                    {
                                        localvariables.TryAdd(opCreateVar, "List<string>");
                                        sb.Append("List<string> [" + opCreateVar._varName + "]");
                                    }
                                    else
                                    {
                                        localvariables.TryAdd(opCreateVar, "string");
                                        sb.Append("string [" + opCreateVar._varName + "]");
                                    }
                                    break;
                                default:
                                    sb.Append("UNIMP_VARPAR::" + opCreateVar.ToString().Split('.').Last());
                                    break;
                            }
                        }
                    }
                    if (codeList[i + 1] is OpMacroParameter) sb.Append(", ");
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + sb.ToString());
                    sb.Clear();
                }
                else if (codeList[i] is OpStructureSection structsection)
                {
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "#start structure section");
                    if (structsection._branchPC != 0)
                    {
                        if ((i + structsection._branchPC + 1) < codeList._count)
                        {
                            ParseScriptObjects(codeList, i + 1, structsection._branchPC);
                            i += structsection._branchPC;
                        }
                    }
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "#end structure section");
                }
                else if (codeList[i] is OpMacroSection macrosection)
                {
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "#start macro section");
                    if (macrosection._branchPC != 0)
                    {
                        if ((i + macrosection._branchPC + 1) < codeList._count)
                        {
                            ParseScriptObjects(codeList, i + 1, macrosection._branchPC);
                            i += macrosection._branchPC;
                        }
                    }
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "#end macro section");
                }
                else
                {
                    if (codeList[i] is not OpFlowBuiltInBehavior && codeList[i] is not OpFlowLoop)
                    {
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "// " + codeList[i].ToString().Split('.').Last() + " (unimplemented)");
                        sb.Clear();
                    }
                }
                // for OpFlowBuiltInBehavior etc I want to calculate and write them
                // when saving the script, leaving them out of view when editing since they hold no info
                linecount++;
            }
            return returnedstring.ToString();
        }
    }
}
