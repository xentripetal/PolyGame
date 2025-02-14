using Hexa.NET.ImGui;

namespace PolyGame.Editor;

public class SceneWindow
{
    private static bool isShown;
    private static bool isVisible;
    private static bool isFocused;
    private static bool isHovered;
    private static bool focus;
    private static bool unsavedDataDialogIsOpen;
    
    
    public static unsafe void Draw()
    {
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.MenuBar;
        
        if (!ImGui.Begin("Scene"u8, ref isShown, flags))
        {
            if (focus)
            {
                ImGuiP.FocusWindow(ImGuiP.GetCurrentWindow(), ImGuiFocusRequestFlags.UnlessBelowModal);
            }
            isVisible = false;
            ImGui.End();
            return;
        } 
        
        isFocused = ImGui.IsWindowFocused();
        isHovered = ImGui.IsWindowHovered(); 
         // TODO compute bounds and apply them to the scene. I don't really care about this right now. Having pop ups is fine.
        ImGui.End();
    }
}