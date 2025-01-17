/**
 * License: MIT - Inherited by github.com/HexaEngine/HexaEngine
 */

using Hexa.NET.ImGui;
using Hexa.NET.ImNodes;
using Hexa.NET.ImPlot;
using System.Numerics;
using Hexa.NET.ImGuizmo;
using PolyECS;
using PolyECS.Scheduling.Configs;
using PolyECS.Systems;
using PolyGame.ImGuiNet;

namespace PolyGame.Editor;

[BeforeSystem<InitImGuiRenderer>]
[AfterSystem<InitImGui>]
public partial class InitImGuiManager : AutoSystem
{
    public unsafe void Run(ImGuiContextPtr ctx, ResMut<ImGuiConfigFlags?> flags, ResMut<ImNodesContextPtr?> nodesCtx,
        ResMut<ImPlotContextPtr?> plotCtx)
    {
        // TODO I wonder if autoparam resolver could handle default values for us
        if (!flags.HasValue)
            flags.Set(ImGuiConfigFlags.NavEnableKeyboard | ImGuiConfigFlags.NavEnableGamepad |
                      ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.ViewportsEnable);
        ImGui.SetCurrentContext(ctx);
        ImGuizmo.SetImGuiContext(ctx);
        ImPlot.SetImGuiContext(ctx);
        ImNodes.SetImGuiContext(ctx);

        if (nodesCtx.IsEmpty)
            nodesCtx.Set(ImNodes.CreateContext());
        ImNodes.SetCurrentContext(nodesCtx.Value!.Value);
        ImNodes.StyleColorsDark(ImNodes.GetStyle());

        if (plotCtx.IsEmpty)
            plotCtx.Set(ImPlot.CreateContext());
        ImPlot.SetCurrentContext(plotCtx.Value!.Value);
        ImPlot.StyleColorsDark(ImPlot.GetStyle());

        var io = ImGui.GetIO();
        io.ConfigFlags |= flags.Value!.Value;
        io.ConfigViewportsNoAutoMerge = false;
        io.ConfigViewportsNoTaskBarIcon = false;
        io.ConfigDragClickToInputText = true;

        var fonts = io.Fonts;
        fonts.FontBuilderFlags = (uint)ImFontAtlasFlags.NoPowerOfTwoHeight;
        fonts.TexDesiredWidth = 2048;

        uint* glyphRanges = stackalloc uint[]
        {
            0xe005, 0xe684,
            0xF000, 0xF8FF,
            0 // null terminator
        };

        var baseDir = "../../../../PolyGame.Editor/";
        var dir = Directory.GetCurrentDirectory();
        var path = Path.GetFullPath(Path.Combine(dir, baseDir));


        using (ImGuiFontBuilder builder = new(fonts))
        {
            builder.AddFontFromFileTTF(path + "assets/shared/fonts/arial.ttf", 15)
                .SetOption(conf => conf.GlyphMinAdvanceX = 16)
                .AddFontFromFileTTF(path + "assets/shared/fonts/fa-solid-900.ttf", 14, glyphRanges)
                .AddFontFromFileTTF(path + "assets/shared/fonts/fa-brands-400.ttf", 14, glyphRanges);
            ImGuiManager.RegisterFont("Default", builder.Font);
        }


        ImGuiFontBuilder iconsRegularBuilder = new(fonts);
        iconsRegularBuilder.AddFontFromFileTTF(path + "assets/shared/fonts/arial.ttf", 15)
            .SetOption(conf => conf.GlyphMinAdvanceX = 16)
            .AddFontFromFileTTF(path + "assets/shared/fonts/fa-regular-400.ttf", 14, glyphRanges);
        ImGuiManager.RegisterFont("Icons-Regular", iconsRegularBuilder.Font);
        iconsRegularBuilder.Dispose();

        /**
        uint* glyphMaterialRanges = stackalloc uint[]
        {
            0xe003, 0xF8FF,
            0 // null terminator
        };

        ImGuiFontBuilder widgetsFontBuilder = new(fonts);
        widgetsFontBuilder.AddFontFromFileTTF(baseDir + "assets/shared/fonts/arial.ttf", size: 16)
            .SetOption(conf => conf.GlyphMinAdvanceX = 16)
            .AddFontFromFileTTF(baseDir + "assets/shared/fonts/MaterialIconsRound-Regular.otf", 20, glyphMaterialRanges);
        ImGuiManager.RegisterFont("WidgetsFont", widgetsFontBuilder.Font);
        widgetsFontBuilder.Destroy();

        ImGuiFontBuilder textEditorFontBuilder = new(fonts);
        textEditorFontBuilder.AddFontFromFileTTF(baseDir + "assets/shared/fonts/CascadiaMono.ttf", size: 16)
            .SetOption(conf => conf.GlyphMinAdvanceX = 16)
            .AddFontFromFileTTF(baseDir + "assets/shared/fonts/MaterialIconsRound-Regular.otf", 20, glyphMaterialRanges)
            ;
        ImGuiManager.RegisterFont("TextEditorFont", textEditorFontBuilder.Font);
        textEditorFontBuilder.Destroy();

        **/

        fonts.Build();

        var style = ImGui.GetStyle();
        var colors = style.Colors;

        colors[(int)ImGuiCol.Text] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
        colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
        colors[(int)ImGuiCol.WindowBg] = new Vector4(0.13f, 0.13f, 0.13f, 1.00f);
        colors[(int)ImGuiCol.ChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int)ImGuiCol.PopupBg] = new Vector4(0.19f, 0.19f, 0.19f, 0.92f);
        colors[(int)ImGuiCol.Border] = new Vector4(0.19f, 0.19f, 0.19f, 0.29f);
        colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.24f);
        colors[(int)ImGuiCol.FrameBg] = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
        colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int)ImGuiCol.TitleBg] = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.06f, 0.06f, 0.06f, 1.00f);
        colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.34f, 0.34f, 0.34f, 0.54f);
        colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.40f, 0.40f, 0.40f, 0.54f);
        colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.56f, 0.56f, 0.56f, 0.54f);
        colors[(int)ImGuiCol.CheckMark] = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.34f, 0.34f, 0.34f, 0.54f);
        colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.56f, 0.56f, 0.56f, 0.54f);
        colors[(int)ImGuiCol.Button] = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
        colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int)ImGuiCol.Header] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.00f, 0.00f, 0.00f, 0.36f);
        colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.20f, 0.22f, 0.23f, 0.33f);
        colors[(int)ImGuiCol.Separator] = new Vector4(0.48f, 0.48f, 0.48f, 0.39f);
        colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.44f, 0.44f, 0.44f, 0.29f);
        colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);
        colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.44f, 0.44f, 0.44f, 0.29f);
        colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);
        colors[(int)ImGuiCol.Tab] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int)ImGuiCol.TabHovered] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int)ImGuiCol.TabSelected] = new Vector4(0.20f, 0.20f, 0.20f, 0.36f);
        colors[(int)ImGuiCol.TabDimmed] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int)ImGuiCol.TabDimmedSelected] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int)ImGuiCol.DockingPreview] = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int)ImGuiCol.DockingEmptyBg] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.PlotLines] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.PlotHistogram] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1.00f, 1.00f, 1.00f, 0.06f);
        colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int)ImGuiCol.DragDropTarget] = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int)ImGuiCol.NavCursor] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 0.00f, 0.00f, 0.70f);
        colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(1.00f, 0.00f, 0.00f, 0.20f);
        colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.10f, 0.10f, 0.10f, 0.00f);

        style.WindowPadding = new Vector2(8.00f, 8.00f);
        style.FramePadding = new Vector2(8.00f, 6.00f);
        style.CellPadding = new Vector2(6.00f, 6.00f);
        style.ItemSpacing = new Vector2(6.00f, 6.00f);
        style.ItemInnerSpacing = new Vector2(6.00f, 6.00f);
        style.TouchExtraPadding = new Vector2(0.00f, 0.00f);
        style.IndentSpacing = 25;
        style.ScrollbarSize = 15;
        style.GrabMinSize = 12;
        style.WindowBorderSize = 1;
        style.ChildBorderSize = 1;
        style.PopupBorderSize = 1;
        style.FrameBorderSize = 1;
        style.TabBorderSize = 1;
        style.WindowRounding = 7;
        style.ChildRounding = 4;
        style.FrameRounding = 12;
        style.PopupRounding = 4;
        style.ScrollbarRounding = 9;
        style.GrabRounding = 3;
        style.LogSliderDeadzone = 4;
        style.TabRounding = 4;

        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
        {
            style.WindowRounding = 0.0f;
            style.Colors[(int)ImGuiCol.WindowBg].W = 1.0f;
        }

        //ImGuiSDL2Platform.Init(window.GetWindow(), null, null);
    }
}

[Before<ImGuiSets>(ImGuiSets.StartFrame)]
public partial class SetDefaultFont : AutoSystem
{
    public void Run()
    {
        //ImGuiManager.PushFont("Default");
    }
}

[After<ImGuiSets>(ImGuiSets.EndFrame)]
public partial class RemoveDefaultFont : AutoSystem
{
    public void Run()
    {
        //ImGuiManager.PopFont();
    }
}

[Before<ImGuiSets>(ImGuiSets.StartFrame)]
public partial class BeforeFrameImGuiManager : AutoSystem
{
    public void Run(ImGuiContextPtr ctx, ImNodesContextPtr nodeCtx, ImPlotContextPtr plotCtx)
    {
        ImGui.SetCurrentContext(ctx);
        ImGuizmo.SetImGuiContext(ctx);
        ImPlot.SetImGuiContext(ctx);
        ImNodes.SetImGuiContext(ctx);

        ImNodes.SetCurrentContext(nodeCtx);
        ImPlot.SetCurrentContext(plotCtx);

        LayoutManager.NewFrame();
        //ImGuiSDL2Platform.NewFrame();
    }
}

[After<ImGuiSets>(ImGuiSets.StartFrame)]
public partial class AfterFrameImGuiManager : AutoSystem
{
    private bool resetLayout = true;

    public void Run(ImGuiContextPtr ctx, ImNodesContextPtr nodeCtx, ImPlotContextPtr plotCtx)
    {
        ImGuizmo.BeginFrame();

        ImGui.PushStyleColor(ImGuiCol.WindowBg, Vector4.Zero);
        ImGuiManager.DockSpaceId = ImGui.DockSpaceOverViewport(null, ImGuiDockNodeFlags.PassthruCentralNode);

        if (resetLayout)
        {
            ImGuiManager.ResetLayout();
            resetLayout = false;
        }

        ImGui.PopStyleColor(1);
    }
}

public class ImGuiManager
{
    private ImGuiContextPtr guiContext;
    private ImNodesContextPtr nodesContext;
    private ImPlotContextPtr plotContext;

    private ImGuiRenderer _renderer;


    public static unsafe void ResetLayout()
    {
        ImGuiP.ClearIniSettings();

        uint rootDockspace = DockSpaceId;

        ImGuiP.DockBuilderRemoveNode(rootDockspace);
        ImGuiP.DockBuilderAddNode(rootDockspace, ImGuiDockNodeFlags.PassthruCentralNode);

        uint down;
        uint up = ImGuiP.DockBuilderSplitNode(rootDockspace, ImGuiDir.Up, 0.65f, null, &down);

        uint next;
        uint left = ImGuiP.DockBuilderSplitNode(up, ImGuiDir.Left, 0.15f, null, &next);
        ImGuiP.DockBuilderDockWindow($"{FontAwesome.Sitemap} Hierarchy", left);
        uint right = ImGuiP.DockBuilderSplitNode(next, ImGuiDir.Right, 0.25f, null, &next);
        ImGuiP.DockBuilderDockWindow("Scene"u8, next);
        ImGuiP.DockBuilderDockWindow($"{FontAwesome.Bars} Properties", right);

        uint leftBottom, rightBottom;
        ImGuiP.DockBuilderSplitNode(down, ImGuiDir.Left, 0.4f, &leftBottom, &rightBottom);

        ImGuiP.DockBuilderDockWindow($"{FontAwesome.LinesLeaning} Asset Browser", leftBottom);

        ImGuiP.DockBuilderDockWindow($"{FontAwesome.PenRuler} Material Editor", rightBottom);
        ImGuiP.DockBuilderDockWindow($"Error List", rightBottom);
        ImGuiP.DockBuilderDockWindow($"\xf120 Console", rightBottom);

        ImGuiP.DockBuilderFinish(rootDockspace);
    }

    public static uint DockSpaceId { get; set; }

    public void Dispose()
    {
        //ImGuiRenderer.Shutdown();
        //ImGuiSDL2Platform.Shutdown();

        ImGui.SetCurrentContext(null);
        ImGuizmo.SetImGuiContext(null);
        ImPlot.SetImGuiContext(null);
        ImNodes.SetImGuiContext(null);

        ImNodes.DestroyContext(nodesContext);
        ImPlot.DestroyContext(plotContext);
        ImGui.DestroyContext(guiContext);
    }

    private static readonly Dictionary<string, ImFontPtr> aliasToFont = new();
    private static int fontPushes = 0;

    public static void RegisterFont(string name, ImFontPtr ptr)
    {
        aliasToFont.Add(name, ptr);
    }

    public static void PushFont(string name = "Default")
    {
        if (aliasToFont.TryGetValue(name, out ImFontPtr fontPtr))
        {
            ImGui.PushFont(fontPtr);
            fontPushes++;
        }
    }

    public static bool PushFont(string name, bool condition)
    {
        if (condition && aliasToFont.TryGetValue(name, out ImFontPtr fontPtr))
        {
            ImGui.PushFont(fontPtr);
            fontPushes++;
            return true;
        }
        return false;
    }

    public static void PopFont()
    {
        if (fontPushes == 0)
        {
            return;
        }

        ImGui.PopFont();
        fontPushes--;
    }

    public static void DisableNav(bool v)
    {
        var io = ImGui.GetIO();
        if (v)
        {
            io.ConfigFlags &= ~ImGuiConfigFlags.NavEnableKeyboard;
            io.ConfigFlags &= ~ImGuiConfigFlags.NavEnableGamepad;
        }
        else
        {
            io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
            io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;
        }
    }
}