using Hexa.NET.ImGui;
using Hexa.NET.ImNodes;
using Hexa.NET.ImPlot;
using PolyGame.ImGuiNet;

namespace PolyGame.Editor;

public class EditorPlugin : IPlugin
{
    public unsafe void Apply(App app)
    {
        app.AddSystem<InitEditor>(Schedules.Startup)
            .AddSystem<DrawEditor>(Schedules.Render)
            .AddSystem<InitImGuiManager>(Schedules.Startup)
            .AddSystem<BeforeFrameImGuiManager>(Schedules.PreUpdate)
            .AddSystem<AfterFrameImGuiManager>(Schedules.PreUpdate)
            .AddSystem<SetDefaultFont>(Schedules.PreUpdate)
            .AddSystem<RemoveDefaultFont>(Schedules.PostRender);
    }
}