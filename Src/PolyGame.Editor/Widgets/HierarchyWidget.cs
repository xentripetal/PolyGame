using System.Numerics;
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;

namespace PolyGame.Editor.Widgets;

public class HierarchyWidget : EditorWindow
{
    public HierarchyWidget()
    {
        IsShown = true;
    }

    private enum HierarchyLevelColoring
    {
        Mono,
        Color,
        Multi
    }

    private readonly uint[] levelColorPalette =
    [ // 0xAABBGGRR
        0x8F0000FF,
        0x8F00FF00,
        0x8FFF0000,
        0x8FFFFF00,
        0x8FFF00FF,
        0x8F00FFFF,
        0x8F800080,
        0x8F008080,
    ];

    private uint levelColor = 0xffcf7334;
    private uint prefabLevelColor = 0x8526D65F;
    private uint sceneLevelColor = 0xffcf7334;
    private byte levelAlpha = 0xFF;
    private byte monochromeBrightness = 0xac;
    private bool reverseColoring = false;

    private bool showHidden;
    private bool focused;
    private string searchString = string.Empty;
    private bool windowHovered;

    protected override string Name
    {
        get => "Hierarchy";
    }

    public override void DrawContent(PolyWorld world, GraphicsDevice device)
    {
        focused = ImGui.IsWindowFocused();
        ImGui.InputTextWithHint("##SearchBar", $"{FontAwesome.MagnifyingGlass} Search...", ref searchString, 1024);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, 0xff1c1c1c);
        ImGui.BeginChild("LayoutContent");
        windowHovered = ImGui.IsWindowHovered();

        var avail = ImGui.GetContentRegionAvail();
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        ImGui.BeginTable("Table", 1, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.PreciseWidths);
        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);

        ImGui.PushStyleColor(ImGuiCol.TableRowBg, 0xff1c1c1c);
        ImGui.PushStyleColor(ImGuiCol.TableRowBgAlt, 0xff232323);
        ImGuiTablePtr table = ImGui.GetCurrentTable();

        ImGui.Indent();
        ImGui.TableHeadersRow();

        ImGui.Unindent();
        // TODO drag drop target

        var q = world.QueryBuilder().Without(Ecs.ChildOf, Ecs.Wildcard).Build();
        Entity? prevEntity = null;
        q.Each((Entity en) => {
            if (prevEntity != null)
            {
                DisplayNode(prevEntity.Value, false, !prevEntity.Value.Name().Contains(searchString), drawList, table, avail, 0, false);
            }
            prevEntity = en;
        });
        if (prevEntity != null)
        {
            DisplayNode(prevEntity.Value, false, !prevEntity.Value.Name().Contains(searchString), drawList, table, avail, 0, true);
        }


        ImGui.PopStyleColor();
        ImGui.PopStyleColor();
        ImGui.EndTable();
        var space = ImGui.GetContentRegionAvail();
        ImGui.Dummy(space);

        ImGui.EndChild();

        ImGui.PopStyleColor();
    }

    private readonly List<bool> isLastInLevel = [];

    private void SetLevel(int level, bool isLast)
    {
        if (isLastInLevel.Count <= level)
        {
            isLastInLevel.Add(isLast);
        }
        else
        {
            isLastInLevel[level] = isLast;
        }
    }

    private HierarchyLevelColoring coloring = HierarchyLevelColoring.Color;


    private void DisplayNode(
        Entity entity,
        bool isRoot,
        bool searchHidden,
        ImDrawListPtr drawList,
        ImGuiTablePtr table,
        Vector2 avail,
        int level,
        bool isLast
    )
    {
        // Don't render Component entities
        if (entity.Has<flecs.EcsComponent>() || entity.Has(Ecs.Private) || entity.Has(Ecs.Module) || entity.Name().Length == 0 )
        {
            return;
        }
        SetLevel(level, isLast);

        /**
        if (element.IsHidden && !showHidden)
        {
            return;
        }
        if (element.IsHidden || searchHidden)
        {
            ImGui.BeginDisabled(true);
        }
        **/

        uint colHovered = ImGui.GetColorU32(ImGuiCol.HeaderHovered);
        uint colSelected = ImGui.GetColorU32(ImGuiCol.Header);

        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;

        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);

        ImRect rect = ImGui.TableGetCellBgRect(table, 0);
        rect.Max.X = avail.X + rect.Min.X;
        rect.Max.Y += ImGui.GetTextLineHeight();

        bool hovered = ImGui.IsMouseHoveringRect(rect.Min, rect.Max) && windowHovered;
        if (hovered)
        {
            drawList.AddRectFilled(rect.Min, rect.Max, colHovered);
        }

        var childCount = 0;
        entity.Children((_ => childCount++));

        if (childCount == 0)
        {
            flags |= ImGuiTreeNodeFlags.Leaf;
        }

        if (isRoot)
        {
            flags = ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.DefaultOpen;
        }

        if (level > 0)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                var before = coloring;
                coloring = HierarchyLevelColoring.Mono;
                DrawTreeLine(drawList, rect, level, isLast);
                coloring = before;
            }
            else
            {
                DrawTreeLine(drawList, rect, level, isLast);
            }
        }

        /**
        if (element.IsEditorSelected)
        {
            drawList.AddRectFilled(rect.Min, rect.Max, colSelected);

            var lineMin = rect.Min;
            var lineMax = new Vector2(lineMin.X + 4, rect.Max.Y);
            drawList.AddRectFilled(lineMin, lineMax, levelColor);
        }
        **/

        bool colorText = !searchHidden && !string.IsNullOrEmpty(searchString);

        if (colorText)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, 0xff0099ff);
        }

        uint col = 0x0;
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, col);
        ImGui.PushStyleColor(ImGuiCol.HeaderActive, col);

        //char icon = GetIcon(element);
        //bool isOpen = ImGui.TreeNodeEx($"{icon} {element.Name}", flags);
        bool isOpen = ImGui.TreeNodeEx($"{entity.Name()}", flags);
        ImGui.PopStyleColor();
        ImGui.PopStyleColor();

        if (colorText)
        {
            ImGui.PopStyleColor();
        }

        //element.IsEditorOpen = isOpen;
        //element.IsEditorDisplayed = true;

        //HandleInput(element, hovered);
        //DisplayNodeContextMenu(element);
        //HandleDragDrop(element);
        //DrawObjectLabels(avail, element);

        //if (searchHidden && !element.IsHidden)
        {
            //ImGui.EndDisabled();
        }

        if (isOpen)
        {
            int i = 0;
            entity.Children((child => {
                bool isLast = i >= childCount - 1;
                DisplayNode(child, false, !child.Name().Contains(searchString), drawList, table, avail, level + 1, isLast);
                i++;
            }));
            ImGui.TreePop();
        }

        //if (element.IsHidden)
        {
            //ImGui.EndDisabled();
        }
    }

    private void DrawTreeLine(ImDrawListPtr drawList, ImRect rect, int level, bool isLast, bool isLevelLower = false)
    {
        for (int i = 1; i < level; i++)
        {
            var lowerLevel = level - i;
            if (isLastInLevel[lowerLevel])
            {
                continue;
            }
            DrawTreeLine(drawList, rect, lowerLevel, false, true);
        }

        const float lineThickness = 2;
        const float lineWidth = 10;
        float indentSpacing = ImGui.GetStyle().IndentSpacing * (level - 1) + ImGui.GetTreeNodeToLabelSpacing() * 0.5f - lineThickness * 0.5f;
        Vector2 lineMin = new (rect.Min.X + indentSpacing, rect.Min.Y);
        Vector2 lineMax = new (lineMin.X + lineThickness, rect.Max.Y);
        Vector2 lineMidpoint = lineMin + (lineMax - lineMin) * 0.5f;
        Vector2 lineTMin = new (lineMax.X, lineMidpoint.Y - lineThickness * 0.5f);
        Vector2 lineTMax = new (lineMax.X + lineWidth, lineMidpoint.Y + lineThickness * 0.5f);
        if (isLast)
        {
            lineMax.Y = lineTMax.Y; // set vertical line y to horizontal line y to create a L shape
        }
        uint color = GetColorForLevel(level);
        drawList.AddRectFilled(lineMin, lineMax, color);
        if (!isLevelLower)
        {
            drawList.AddRectFilled(lineTMin, lineTMax, color);
        }
    }

    private uint GetColorForLevel(int level)
    {
        int levelNormalized = (level - 1) % levelColorPalette.Length;

        if (reverseColoring)
        {
            levelNormalized = levelColorPalette.Length - levelNormalized - 1;
        }

        if (coloring == HierarchyLevelColoring.Mono)
        {
            uint brightness = (uint)(monochromeBrightness * (1 - (levelNormalized / (float)levelColorPalette.Length)));

            return (uint)levelAlpha << 24 | brightness << 16 | brightness << 8 | brightness; // 0xAABBGGRR
        }

        /**
        if (coloring == HierarchyLevelColoring.Color)
        {
            float value = levelNormalized / (float)levelColorPalette.Length;
            float hueShift = value * 0.1f;

            ColorHSVA hsv = Color.FromABGR(levelColor).ToHSVA();

            hsv.H += hueShift;
            hsv.S *= 1 - value;
            hsv.V /= MathF.Exp(value);

            uint color = hsv.ToRGBA().PackedValue;

            return (uint)levelAlpha << 24 | color; // 0xAABBGGRR
        }**/

        // HierarchyLevelColoring.Multi just return here as fallback.

        return levelColorPalette[levelNormalized];
    }
}
