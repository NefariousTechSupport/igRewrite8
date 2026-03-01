using igLibrary.Core;
using igLibrary.Tfb.Game;
using igLibrary.Tfb.Script;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace igCauldron3.Frames
{
    public class ScriptNavigatorFrame : Frame
    {
        private List<tfbScriptInfo> scripts;
        public Action<tfbScriptInfo?> _selectedscript;
        public ScriptNavigatorFrame(Window wnd, List<tfbScriptInfo> inputscripts, Action<tfbScriptInfo?> selected) : base(wnd)
        {
            scripts = inputscripts;
            scripts = scripts.OrderBy(x => x._name.Split('/').Last()).ToList();
            _selectedscript = selected;
        }

        public override void Render()
        {
            ImGui.Begin("Local scripts");
            foreach(var script in scripts)
            {
                if (ImGui.Button("Open"))
                {
                    _selectedscript.Invoke(script);
                    Close();
                    //OpCodeList codeList = script._opList;
                    //OpCreateVariableList varList = script._masterVarList;
                    //igObjectDirectory capturedDir = DirectoryManagerFrame._instance.CurrentDir!;
                    //var scriptDependencies = new Dictionary<List<OpAbstractCreateVariable>, string>(StreamContext.globalScriptDependencies);
                    //if (capturedDir._name._string != "app:/permanent/global.bld (level bld)")
                    //{
                    //    Dictionary<List<OpAbstractCreateVariable>, string>? localScriptVariables = ScriptParser.ReadDependencies(capturedDir);
                    //    foreach (var kv in localScriptVariables)
                    //    {
                    //        scriptDependencies.Add(kv.Key, kv.Value);
                    //    }
                    //}
                    
                    //Close();
                }
                ImGui.SameLine();
                ImGui.Text(script._name.Split('/').Last());
            }
            if (ImGui.Button("Close")) Close();
            ImGui.End();

        }
    }
}
