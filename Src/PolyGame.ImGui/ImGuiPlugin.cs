using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Scheduling.Configs;
using PolyECS.Systems;
using Num = System.Numerics;

namespace PolyGame.ImGuiNet;

public enum ImGuiSets
{
    Init,
    StartFrame,
    EndFrame
}

public class ImGuiPlugin : IPlugin
{
    public void Apply(App app)
    {
        app.RegisterResource<ImGuiContextPtr>() // Resource will be created at startup, see InitImGui
            .SetResource(new ImGuiRenderer(app))
            .AddSystem<InitImGui>(Schedules.Startup)
            .AddSystem<InitImGuiRenderer>(Schedules.Startup)
            .AddSystem<StartImGuiFrame>(Schedules.PreUpdate)
            // TODO investigate how this should work in a split render world
            .AddSystem<EndImGuiFrame>(Schedules.PostRender);
    }
}

[InSet<ImGuiSets>(ImGuiSets.Init)]
public partial class InitImGui : AutoSystem
{
    public void Run(ResMut<ImGuiContextPtr?> ctx)
    {
        ctx.Set(ImGui.CreateContext());
    }
}

[AfterSystem<InitImGui>()]
[InSet<ImGuiSets>(ImGuiSets.Init)]
public partial class InitImGuiRenderer : AutoSystem
{
    public void Run(ImGuiRenderer renderer)
    {
        renderer.SetupInput();
        renderer.RebuildFontAtlas();
    }
}

[InSet<ImGuiSets>(ImGuiSets.StartFrame)]
public partial class StartImGuiFrame : AutoSystem
{
    int frameCount = 0;

    public void Run(ImGuiRenderer renderer, in GameTime gameTime)
    {
        // If a render frame was skipped, we need to reset the un-rendered previous frame
        if (ImGui.GetFrameCount() == frameCount && frameCount != 0)
        {
            ImGui.EndFrame();
        }

        renderer.BeforeLayout(gameTime);
        frameCount++;
    }
}

[InSet<ImGuiSets>(ImGuiSets.EndFrame)]
public partial class EndImGuiFrame : AutoSystem
{
    public void Run(ImGuiRenderer renderer)
    {
        renderer.AfterLayout();
    }
}

public partial class TestImGuiRender : AutoSystem
{
    public TestImGuiRender(GraphicsDevice device, ImGuiRenderer renderer)
    {
        // First, load the texture as a Texture2D (can also be done using the XNA/FNA content pipeline)
        _xnaTexture = CreateTexture(device, 300, 150, pixel =>
        {
            var red = (pixel % 300) / 2;
            return new Color(red, 1, 1);
        });

        // Then, bind it to an ImGui-friendly pointer, that we can use during regular ImGui.** calls (see below)
        _imGuiTexture = renderer.BindTexture(_xnaTexture);
    }

    public static Texture2D CreateTexture(GraphicsDevice device, int width, int height, Func<int, Color> paint)
    {
        //initialize a texture
        var texture = new Texture2D(device, width, height);

        //the array holds the color for each pixel in the texture
        Color[] data = new Color[width * height];
        for (var pixel = 0; pixel < data.Length; pixel++)
        {
            //the function applies the color according to the specified pixel
            data[pixel] = paint(pixel);
        }

        //set the color
        texture.SetData(data);

        return texture;
    }

    private Texture2D _xnaTexture;
    private ImTextureID _imGuiTexture;

    public void Run(ImGuiRenderer renderer, PolyWorld world)
    {
        //ImGuiLayout();
        renderer.AfterLayout();
    }

    // Direct port of the example at https://github.com/ocornut/imgui/blob/master/examples/sdl_opengl2_example/main.cpp
    private float f = 0.0f;

    private bool show_test_window = false;
    private bool show_another_window = false;
    private Num.Vector3 clear_color = new(114f / 255f, 144f / 255f, 154f / 255f);
    private string _textBuffer = "";

    protected virtual void ImGuiLayout()
    {
        // 1. Show a simple window
        // Tip: if we don't call ImGui.Begin()/ImGui.End() the widgets appears in a window automatically called "Debug"
        {
            ImGui.Text("Hello, world!");
            ImGui.SliderFloat("float", ref f, 0.0f, 1.0f, string.Empty);
            ImGui.ColorEdit3("clear color", ref clear_color);
            if (ImGui.Button("Test Window")) show_test_window = !show_test_window;
            if (ImGui.Button("Another Window")) show_another_window = !show_another_window;
            ImGui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)",
                1000f / ImGui.GetIO().Framerate, ImGui.GetIO().Framerate));

            ImGui.InputText("Text input", ref _textBuffer, 100);

            ImGui.Text("Texture sample");
            ImGui.Image(_imGuiTexture, new Num.Vector2(300, 150), Num.Vector2.Zero, Num.Vector2.One, Num.Vector4.One,
                Num.Vector4.One); // Here, the previously loaded texture is used
        }

        // 2. Show another simple window, this time using an explicit Begin/End pair
        if (show_another_window)
        {
            ImGui.SetNextWindowSize(new Num.Vector2(200, 100), ImGuiCond.FirstUseEver);
            ImGui.Begin("Another Window", ref show_another_window);
            ImGui.Text("Hello");
            ImGui.End();
        }

        // 3. Show the ImGui test window. Most of the sample code is in ImGui.ShowTestWindow()
        if (show_test_window)
        {
            ImGui.SetNextWindowPos(new Num.Vector2(650, 20), ImGuiCond.FirstUseEver);
            ImGui.ShowDemoWindow(ref show_test_window);
        }
    }
}