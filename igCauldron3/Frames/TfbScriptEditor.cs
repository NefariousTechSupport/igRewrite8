using igCauldron3.Frames.TextEdit;
using igCauldron3.Frames.TextEdit.Syntax;
using igCauldron3.Utils;
using igLibrary.Core;
using igLibrary.Tfb.Script;
using ImGuiNET;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.Devices;
using OpenTK.Audio.OpenAL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
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
        private OpCreateVariableList varList;
        private igObjectDirectory currentdir;
        private int linecount = 0;
        private TextEditor editor;
        /// <summary>
        /// colors used for colorcoding i guess :)
        /// </summary>
        private Vector4 Grey = new Vector4(255f, 255f, 255f, 0.5f);
        private Vector4 Red = new Vector4(1f, 0f, 0f, 1f);
        private Vector4 Green = new Vector4(154f, 205f, 50f, 1f);
        private Vector4 Blue = new Vector4(0f, 0f, 1f, 1f);
        private Dictionary<OpSpawn, string> spawnedobjects = new Dictionary<OpSpawn, string>(); // for opspawn
        private int amountOfSpawns = 0; // for opspawn
        private Dictionary<OpAbstractCreateVariable, string> localvariables = new Dictionary<OpAbstractCreateVariable, string>(); // to fix float variables by turning ints into floats
                                                                                                                                  // if the variable ever gets assigned with a float value
        private Dictionary<string, string> localvarstrings = new Dictionary<string, string>();
        private Dictionary<OpFlowBuiltInBehavior, string> branchTargets = new Dictionary<OpFlowBuiltInBehavior, string>(); // for OpAbstractFlow
        string script = "tfbscript test";
        public TfbScriptEditor(Window wnd, igObjectDirectory currentdir2, OpCodeList codeList2, OpCreateVariableList varList2, Dictionary<List<OpAbstractCreateVariable>, string> dependencies) : base(wnd)
        {
            codeList = codeList2;
            varList = varList2;
            currentdir = currentdir2;
            StringBuilder scriptbuilder = new StringBuilder();
            if (varList != null)
            {
                scriptbuilder.AppendLine("Variables Section");
                string variablessection = ParseVariables(varList);
            }
            string scriptsection = ParseScriptObjects(codeList);
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
            ImGui.Begin("Demo");
            if (ImGui.Button("Reset"))
            {
                editor.AllText = script;
            }
            ImGui.SameLine();
            if (ImGui.Button("err line"))
                editor.AppendLine("Some error text", PaletteIndex.Custom);

            ImGui.SameLine();
            if (ImGui.Button("warn line"))
                editor.AppendLine("Some warning text", PaletteIndex.Custom + 1);

            ImGui.SameLine();
            if (ImGui.Button("info line"))
                editor.AppendLine("Some info text", PaletteIndex.Custom + 2);

            ImGui.SameLine();
            if (ImGui.Button("verbose line"))
                editor.AppendLine("Some debug text", PaletteIndex.Custom + 3);
            ImGui.Text(
                $"Cur:{editor.CursorPosition} SEL: {editor.Selection.Start} - {editor.Selection.End}"
            );
            editor.Render("EditWindow");
            if (ImGui.Button("Close")) Close();
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
                    sb.Append(BitConverter.Int32BitsToSingle(RHS._value));
                else if (ex._name == "IntMeasurement")
                    sb.Append(RHS._value);
                else if (ex._name == "ColorMeasurement")
                {
                    sb.Append(RHS._value.ToString("X8"));
                }
                else if (ex._name == "ScreenMeasurement")
                {
                    sb.Append("(x: " + (RHS._value & 0xFFFF0000) * 16 + ", y: " + (RHS._value & 0x0000FFFF) * 16 + ")");
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
                        else sb.Append(BitConverter.Int32BitsToSingle(rhsValueS._value));
                    else if (ex._name == "IntMeasurement")
                        if (changeVal && r == 1) ;
                        else sb.Append(rhsValueS._value);
                    else if (ex._name == "ColorMeasurement")
                    {
                        sb.Append(rhsValueS._value.ToString("X8"));
                    }
                    else if (ex._name == "ScreenMeasurement")
                    {
                        sb.Append("(x: " + (rhsValueS._value & 0xFFFF0000) * 16 + ", y: " + (rhsValueS._value & 0x0000FFFF) * 16 + ")");
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
                switch (rh[i])
                {
                    case OpSlideValue:
                        sb.Append("[^slider]");
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
                    case OpForEach:
                        sb.Append("[^for each]");
                        break;
                    case OpFindSubSet:
                        sb.Append("[^subset]");
                        break;
                    case OpCreateVariable opvar:
                        sb.Append("[" + opvar._varName + "]");
                        break;
                    case ValueInfo valinfo:
                        string valueinfoval = "";
                        igExternalReferenceSystem.Singleton._globalSet.MakeReference(valinfo._type, null, out igHandleName name);
                        igObject? valueinfo2 = igExternalReferenceSystem.Singleton._globalSet.ResolveReference(name, null);
                        if (valueinfo2 is igMetaObject type)
                        {
                            switch (type._name)
                            {
                                case "FloatMeasurement":
                                    valueinfoval = BitConverter.Int32BitsToSingle(valinfo._value).ToString();
                                    localvarstrings.TryAdd(valinfo._name, valueinfoval);
                                    break;
                                case "IntMeasurement":
                                    valueinfoval = valinfo._value.ToString();
                                    localvarstrings.TryAdd(valinfo._name, valueinfoval);
                                    break;
                            }
                        }
                        sb.Append("'" + valinfo._name + "'");
                        break;
                    case ScriptReference sref:
                        if (sref._name.Split('.').Last() == "my") sb.Append("myself");
                        else sb.Append(sref._name.Split('.').Last());
                        break;
                    case OpUserBehavior:
                        sb.Append("opuserbehavior //unimplemented");
                        break;
                    default:
                        sb.Append("(" + rh[i]._name.Split('.').Last() + ")");
                        break;
                }
                if (i != rh._count - 1) sb.Append(".");
            }
            return sb.ToString();
        }
        private string SetupLHS(ScriptObjectList LHS, OpCodeList codeList, int pc)
        {
            StringBuilder sb = new StringBuilder();
            for (int j = 0; j < LHS._count; j++)
            {
                switch (LHS[j]) // some script objects use different fields for their actual names.
                {
                    case null:
                        continue;
                    case OpControl:
                        sb.Append("[^controlled]");
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
                    case OpForEach:
                        sb.Append("[^for each]");
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
                        sb.Append("(" + scriptsetref._name.ToString().Split('.').Last() + ")");
                        break;
                    case ScriptReference scriptref:
                        switch (scriptref._name)
                        {
                            case "OpTopLevelBehavior.my":
                                sb.Append("myself");
                                break;
                            default:
                                sb.Append("(" + scriptref._name.ToString().Split('.').Last() + ")");
                                break;
                        }
                        break;
                    case ColorMeasurement cm:
                        switch (cm._name)
                        {
                            case "Placement.tint":
                                sb.Append("tint");
                                break;
                            default:
                                sb.Append("(" + cm._name.ToString().Split('.').Last() + ")");
                                break;
                        }
                        break;
                    case OrientationMeasurement om:
                        switch (om._name)
                        {
                            case "MatrixMeasurement.orientation":
                                sb.Append("orientation");
                                break;
                            default:
                                sb.Append("(" + om._name.ToString().Split('.').Last() + ")");
                                break;
                        }
                        break;

                    case FloatMeasurement fl:
                        switch (fl._name)
                        {
                            case "ActorPhysics.gravity max speed":
                                sb.Append("(gravity max speed)");
                                break;
                            case "ScaleMeasurement.uniform":
                                sb.Append("uniform");
                                break;
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
                                sb.Append("(" + fl._name.ToString().Split('.').Last() + ")");
                                break;
                        }
                        break;
                    case IntMeasurement im:
                        switch (im._name)
                        {
                            case "Sequence.playback mode":
                                sb.Append("(playback mode)");
                                break;
                            case "ColorMeasurement.alpha":
                                sb.Append("alpha");
                                break;
                            case "SetVariant.count":
                                sb.Append("count");
                                break;
                            case "CollisionInfo.interacts with level mesh":
                                sb.Append("(interacts with level mesh)");
                                break;
                            default:
                                if (im._name.ToString().Split('.').Last().Contains(' '))
                                {
                                    sb.Append('(' + im._name.ToString().Split('.').Last() + ')');
                                }
                                else
                                {
                                    sb.Append(im._name.ToString().Split('.').Last());
                                }
                                break;
                        }
                        break;
                    case ScaleMeasurement sm:
                        switch (sm._name)
                        {
                            case "Placement.scale":
                                sb.Append("scale");
                                break;
                            default:
                                if (sm._name.ToString().Split('.').Last().Contains(' '))
                                {
                                    sb.Append('(' + sm._name.ToString().Split('.').Last() + ')');
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
                    case OpCheckValue opc:
                        string LeftHandStack = SetupLHS(opc._LHS, codeList, pc);
                        sb.Append(LeftHandStack);
                        break;
                    case tfbScriptObject so:
                        // if StreamContext.globalObjects.Contains(so)
                        // {
                        //     sb.Append("global::");
                        // }
                        // elseif (languagepak).Contains(so)
                        // {
                        //     sb.Append("lang::");
                        // }
                        // else
                        // {
                        //     sb.Append("local::");
                        // }
                        if (so._name.ToString().Split('.').Last().Contains(' '))
                        {
                            sb.Append('(' + so._name.ToString().Split('.').Last() + ')');
                        }
                        else
                        {
                            sb.Append(so._name.ToString().Split('.').Last());
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
                                sb.Append(+BitConverter.Int32BitsToSingle(rhsValueS._value));
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
                                sb.Append("(x: " + (rhsValueS._value & 0xFFFF0000) * 16 + ", y: " + (rhsValueS._value & 0x0000FFFF) * 16 + ")");
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
                else if (codeList[i] is OpPreScript)
                {

                }
                else if (codeList[i] is OpStartUp)
                {

                }
                else if (codeList[i] is OpStartSequence opstart)
                {
                    string lhs = SetupLHS(opstart._LHS, codeList, i);
                    if (opstart._indexRHS != null)
                    {
                        string indexrhs = SetupRHS(opstart._indexRHS);
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "start " + lhs + "[" + indexrhs + "]");
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
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "macro (" + macrospec._name + ") : (" + macrospec._NP[0]._name + ")");
                    bool noparameters = (codeList[i + 2]._name == "flow macro");
                    if (noparameters && codeList[i + 3] is OpFlowBuiltInBehavior)
                    {
                        i += macrospec._branchPC;
                        continue;
                    }
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "(");
                    indentCount++;
                    if (codeList[i + macrospec._branchPC] is OpFlowBuiltInBehavior flow)
                    {
                        int parameterCount = 0;
                        if (codeList[i + 1] is OpMacroInterface macinterface && !noparameters)
                        {

                            parameterCount = macinterface._branchPC - 1;
                            branchTargets.TryAdd(flow, "return");
                            ParseScriptObjects(codeList, i + 2, parameterCount); // skip the OpMacroInterface
                            i += (parameterCount + 3);
                        }
                        else
                        {
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
                        if (indentCount != 0) indentCount--;
                        branchTargets.Remove(flow);
                    }

                }
                else if (codeList[i] is OpDefineMacro defmacro)
                {
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "macro (" + defmacro._name + ")");
                    bool noparameters = (codeList[i + 2]._name == "flow macro");
                    if (noparameters && codeList[i + 3] is OpFlowBuiltInBehavior)
                    {
                        i += defmacro._branchPC;
                        continue;
                    }
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "(");
                    indentCount++;
                    if (codeList[i + defmacro._branchPC] is OpFlowBuiltInBehavior flow)
                    {
                        int parameterCount = 0;
                        if (codeList[i + 1] is OpMacroInterface macinterface && !noparameters)
                        {
                            parameterCount = macinterface._branchPC - 1;
                            branchTargets.TryAdd(flow, "return");
                            ParseScriptObjects(codeList, i + 2, parameterCount); // skip the OpMacroInterface
                            i += (parameterCount + 3);

                        }
                        else
                        {
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
                    if (indentCount != 0) indentCount--;

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
                            sb.Append("UNIMP:" + spawn._LHS[0].ToString().Split('.').Last());
                            break;
                    }
                    string spawndatatype = sb.ToString();
                    string spawnname;
                    if (spawn._LHS is not null)
                    {
                        if (spawn._LHS[0]._name.Split('.').Last() == "my")
                        {
                            spawnname = "myself";
                        }
                        else
                        {
                            spawnname = SetupLHS(spawn._LHS, codeList, i);
                        }
                    }
                    else
                    {
                        spawnname = "null";
                    }
                    sb.Clear();
                    string spawnpos = ""; // at:
                    if (spawn._LHS[0] is AbstractPlacement) // only spawned AbstractPlacements set a position/facing
                    {
                        if (spawn._RHS._count != 0)
                        {
                            if (spawn._RHS[0]._name.Split('.').Last() == "my")
                            {
                                spawnpos = "my.position";
                            }
                            else
                            {
                                spawnpos = SetupLHS(spawn._RHS, codeList, i);
                            }
                        }
                        string spawnfacing = "";
                        if (spawn._facingRHS._varOp1._count != 0)
                        {
                            if (spawn._facingRHS._varOp1[0]._name.Split('.').Last() == "my")
                            {
                                spawnfacing = "my.facing";
                            }
                        }
                        else
                        {
                            spawnfacing = SetupRHS(spawn._facingRHS);
                        }
                        amountOfSpawns++;
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "spawned spawnedObj" + amountOfSpawns + " = spawn(" + spawndatatype + "." + spawnname + ", " + spawnpos + ", " + spawnfacing + ")");
                    }
                    else // if it's not an abstractplacement it has no position or facing
                    {
                        amountOfSpawns++;
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "spawned spawnedObj" + amountOfSpawns + " = spawn(" + spawndatatype + "." + spawnname + ")");
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
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + lhs + ".Add(" + rhs + ")");
                    }
                    else if (changeme._combineOp is Combiner.exclude)
                    {
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + lhs + ".Remove(" + rhs + ")");
                    }
                    else if (changeme._combineOp is Combiner.be_replaced_by)
                    {
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + lhs + ".ReplaceWith(" + rhs + ") // (testing be_replaced_by)");
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
                    if (macropar._RHS is ValueRHSVariant valueR)
                    {
                        if (valueR._varOp1._count == 0)
                        {
                            sb.Append(" = ");
                            string RHSCalc = SetupRHS(valueR);
                            sb.Append(RHSCalc);
                        }
                        else
                        {
                            sb.Append(" = ");
                            for (int ii = 0; ii < valueR._varOp1._count; ii++)
                            {
                                switch (valueR._varOp1[ii])
                                {
                                    case OpFindSubSet:
                                        sb.Append("[^subset]");
                                        break;
                                    case OpForEach:
                                        sb.Append("[foreach]");
                                        break;
                                    case OpSlideValue:
                                        sb.Append("[^slider]");
                                        break;
                                    case OpCreateVariable opv:
                                        sb.Append("[" + opv._varName + "]");
                                        break;
                                    case OpMacroParameter opmacropar:
                                        sb.Append("[" + opmacropar._varName + "]");
                                        break;
                                    case OpSpawn spawn1:
                                        sb.Append(spawnedobjects[spawn1]);
                                        break;
                                    case ScriptSetReference: // "my" would look better as "myself"
                                        if (valueR._varOp1[ii]._name == "my") sb.Append("myself");
                                        break;
                                    default:
                                        sb.Append("(" + valueR._varOp1[ii]._name.ToString().Split('.').Last() + ")");
                                        break;
                                }
                                if (ii != (valueR._varOp1._count - 1)) sb.Append(".");
                            }
                        }
                    }
                    else if (macropar._RHS is RHSReferenceStack rhsref)
                    {
                        sb.Append(" = ");
                        if (rhsref._count != 0)
                        {
                            switch (rhsref[0])
                            {
                                case OpFindSubSet:
                                    sb.Append("[^subset]");
                                    break;
                                case OpForEach:
                                    sb.Append("[foreach]");
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
                                    sb.Append(rhsref[0]._name.ToString().Split('.').Last());
                                    break;
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
                        sb.Append(" = ");
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
                        sb.Append("[" + indexrhs + "]");
                    }
                    if (macropar._combineOp is not Combiner.include)
                    {
                        switch (macropar._combineOp)
                        {
                            case Combiner.exclude:
                                sb.Append("// exclude");
                                break;
                            case Combiner.be_replaced_by:
                                sb.Append("// be replaced by");
                                break;
                            case Combiner.intersect_with:
                                sb.Append("// intersect with");
                                break;
                            case Combiner.exclude_all:
                                sb.Append("// exclude all");
                                break;
                            case Combiner.add:
                                sb.Append("// add");
                                break;
                        }
                    }
                    if (codeList[i + 1] is OpMacroParameter) sb.Append(","); //if this isnt the last param
                    else
                    {
                        sb.Append(")"); // closing the UseMacro
                    }
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + sb.ToString());
                    sb.Clear();
                }
                else if (codeList[i] is OpControl control)
                {
                    string lhs = SetupLHS(control._LHS, codeList, i);
                    if (control._indexRHS != null)
                    {
                        string indexrhs = SetupRHS(control._indexRHS);
                        returnedstring.AppendLine(new string(' ', indentCount * 3) + "control (" + lhs + "[" + indexrhs + "]" + ")");
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
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "newopcodenewopcodenewopcode");
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "newopcodenewopcodenewopcode");
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "newopcodenewopcodenewopcode");
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "newopcodenewopcodenewopcode");
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "newopcodenewopcodenewopcode");
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "//OpCheckFOV (unimplemented):"); // check "fov"
                    string fromlhs = SetupLHS(checkfov._fromLHS, codeList, i);//from position
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
                    returnedstring.AppendLine(new string(' ', indentCount * 3) +
                        "from: " + fromlhs +
                        " facing: " + facing +
                        " fov: " + fov +
                        " lhs: " + lhs +
                        " " + relop +
                        " rhs: " + rhs +
                        " " + mode);
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
                    string indexrhs = "";
                    if (setref._indexRHS != null)
                    {
                        indexrhs = "[" + SetupRHS(setref._indexRHS) + "]";
                    }
                    sb.Append("*" + lhs + " = " + rhs + indexrhs);
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + sb.ToString());
                    sb.Clear();
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
                        sb.Append(SetupRHS(checkref._RHS));
                    }
                    if (checkref._indexRHS != null)
                    {
                        sb.Append("[" + SetupRHS(checkref._indexRHS) + "]");
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
                    indentCount++;
                    sb.Clear();
                    if (check._branchPC != 0)
                    {
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
                        sb.Append("[" + findvariable._indexRHS + "]");
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
                    string lhs = SetupLHS(opfindsubset._LHS, codeList, i);
                    string rhs = SetupRHS(opfindsubset._RHS);
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "find subset: " + lhs + " of " + rhs);
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
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "foreach (placeholder in " + lhs + ", " + direction + " " + offsetby + ")");
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
                    if (opuse._NP._editType is OpDefineMacro opdefmac) returnedstring.Append(new string(' ', indentCount * 3) + "callmacro " + opdefmac._name);
                    if (opuse._branchPC != 0)
                    {
                        if (codeList[i + 1] is OpMacroInterface macrointerface)
                        {
                            if (opuse._branchPC == 3) //if this macro doesnt have input variables
                            {
                                returnedstring.AppendLine("()");
                                i += opuse._branchPC;
                                continue;
                            }
                            else
                            {
                                returnedstring.AppendLine();
                                int parameterCount = macrointerface._branchPC - 1;
                                returnedstring.AppendLine(new string(' ', indentCount * 3) + "(");
                                indentCount++;
                                ParseScriptObjects(codeList, i + 2, parameterCount); // skip the OpMacroInterface
                                i += (parameterCount + 3);
                                if (codeList[i] is not OpFlowBuiltInBehavior) // aka if there's more than just parameters
                                {
                                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "{");
                                    indentCount++;
                                    ParseScriptObjects(codeList, i, (opuse._branchPC - 2) - parameterCount); // i believe this is correct
                                    i += (opuse._branchPC - 3) - parameterCount;
                                    if (indentCount != 0) indentCount--;
                                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "}");
                                }
                                if (indentCount != 0) indentCount--;
                            }
                        }
                    }

                }
                else if (codeList[i] is OpDefineStructure opDefStruct)
                {
                    returnedstring.AppendLine(new string(' ', indentCount * 3) + "struct " + opDefStruct._name);
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
                else if (codeList[i] is OpCreateVariable opCreateVar)
                {

                    if (opCreateVar._varContentsType is OpDefineStructure defineStructure)
                    {
                        sb.Append("struct " + defineStructure._name + " [");
                        sb.Append(opCreateVar._varName + "]");
                        localvariables.TryAdd(opCreateVar, "struct " + defineStructure._name);
                    }
                    else if (opCreateVar._varContentsType is OpDefineMacro defineMacro)
                    {
                        sb.Append("macro " + defineMacro._name + " [" + opCreateVar._varName + "]");
                        localvariables.TryAdd(opCreateVar, "macro " + defineMacro._name);
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
                                    localvariables.TryAdd(opCreateVar, "int");
                                    sb.Append("int [" + opCreateVar._varName + "]");
                                    break;
                                case "FloatMeasurement":
                                    localvariables.TryAdd(opCreateVar, "float");
                                    sb.Append("float [" + opCreateVar._varName + "]");
                                    break;
                                case "ColorMeasurement":
                                    localvariables.TryAdd(opCreateVar, "color");
                                    sb.Append("color [" + opCreateVar._varName + "]");
                                    break;
                                case "ValueInfo":
                                    localvariables.TryAdd(opCreateVar, "valueinfo");
                                    sb.Append("valueinfo [" + opCreateVar._varName + "]");
                                    break;
                                case "tfbActorInfo":
                                    localvariables.TryAdd(opCreateVar, "actor");
                                    sb.Append("actor [" + opCreateVar._varName + "]");
                                    break;
                                case "ActorWaypoint":
                                    localvariables.TryAdd(opCreateVar, "actorwaypoint");
                                    sb.Append("actorwaypoint [" + opCreateVar._varName + "]");
                                    break;
                                case "tfbSoundInfo":
                                    localvariables.TryAdd(opCreateVar, "sound");
                                    sb.Append("sound [" + opCreateVar._varName + "]");
                                    break;
                                case "tfbSpriteInfo":
                                    localvariables.TryAdd(opCreateVar, "sprite");
                                    sb.Append("sprite [" + opCreateVar._varName + "]");
                                    break;
                                case "tfbParticleInfo":
                                    localvariables.TryAdd(opCreateVar, "particle");
                                    sb.Append("particle [" + opCreateVar._varName + "]");
                                    break;
                                case "AnimationInfo":
                                    localvariables.TryAdd(opCreateVar, "animation");
                                    sb.Append("animation [" + opCreateVar._varName + "]");
                                    break;
                                case "ScriptColorInfo":
                                    localvariables.TryAdd(opCreateVar, "scriptcolorinfo");
                                    sb.Append("scriptcolorinfo [" + opCreateVar._varName + "]");
                                    break;
                                case "Slider":
                                    localvariables.TryAdd(opCreateVar, "slider");
                                    sb.Append("slider [" + opCreateVar._varName + "]");
                                    break;
                                case "ScriptController":
                                    localvariables.TryAdd(opCreateVar, "scriptcontroller");
                                    sb.Append("scriptcontroller [" + opCreateVar._varName + "]");
                                    break;
                                case "tfbLightInfo":
                                    localvariables.TryAdd(opCreateVar, "light");
                                    sb.Append("light [" + opCreateVar._varName + "]");
                                    break;
                                case "StringInfo":
                                    localvariables.TryAdd(opCreateVar, "string");
                                    sb.Append("string [" + opCreateVar._varName + "]");
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
