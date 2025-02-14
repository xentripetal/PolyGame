using Hexa.NET.ImGui;
using Hexa.NET.ImPlot;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using Vector2 = System.Numerics.Vector2;

namespace PolyGame.Editor.Widgets;

public class FPSTracker : EditorWindow
{
    public FPSTracker()
    {
        isShown = true;
    }
    
    protected override string Name { get => "FPS"; }
    public RingBuffer History = new(512);
    public RingBuffer RollingAvg = new(512);
    public RingBuffer Window = new(16);

    public override void DrawContent(PolyWorld world, GraphicsDevice device)
    {
        var time = world.MustGetResource<GameTime>();
        var fps = 1 / time.ElapsedGameTime.TotalSeconds;
        History.Add((float) fps);
        Window.Add((float) fps);
        RollingAvg.Add(Window.Avg);
        ImGui.Text($"Avg: {Math.Round(Window.Avg, 0)}, Min: {Math.Round(Window.Min, 0)}, Max: {Math.Round(Window.Max, 0)}");
        ImGui.Text($"Cur: {Math.Round(fps, 0)} Frame Time: {Math.Round(time.ElapsedGameTime.TotalMilliseconds, 2)}ms");
        
        ImPlot.SetNextAxesToFit();
        if (ImPlot.BeginPlot("History", new Vector2(-1, -1), ImPlotFlags.NoInputs))
        {
            ImPlot.PlotLine("FPS", ref History.Values[0], History.Length, 1, 0, ImPlotLineFlags.None, History.Head); 
            ImPlot.PlotLine("Avg Fps", ref RollingAvg.Values[0], RollingAvg.Length, 1, 0, ImPlotLineFlags.None, RollingAvg.Head); 
            ImPlot.EndPlot();
        }
    }
}
