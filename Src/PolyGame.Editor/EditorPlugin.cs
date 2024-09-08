using Hexa.NET.ImGui;
using Hexa.NET.ImNodes;
using Hexa.NET.ImPlot;
using PolyGame.ImGuiNet;

namespace PolyGame.Editor;

public class EditorPlugin : IPlugin
{
    public unsafe void Apply(App app)
    {
        var ctx = ImGui.GetCurrentContext();
        ImGui.SetCurrentContext(ctx);
        ImPlot.SetImGuiContext(ctx);
        ImNodes.SetImGuiContext(ctx);
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.NavEnableKeyboard | ImGuiConfigFlags.NavEnableGamepad | ImGuiConfigFlags.ViewportsEnable;
        io.ConfigViewportsNoAutoMerge = false;
        io.ConfigViewportsNoTaskBarIcon = false;
        io.ConfigDragClickToInputText = true;
        
        
        //ImGui.DockSpaceOverViewport(null, ImGuiDockNodeFlags.PassthruCentralNode, null);

        app.AddSystem<InitEditor>(Schedules.Startup);
        app.AddSystem<DrawEditor>(Schedules.Render);
    }
}
