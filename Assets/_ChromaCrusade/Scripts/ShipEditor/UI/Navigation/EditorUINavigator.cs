using UnityEngine;

public class EditorUINavigator : UINavigator, IUINavigator
{
    protected EditorNavVisualizer editorVisualizer;
    public EditorState EditorState { get; set; }

    public IGridNavigator gridNav;

    public override void Init()
    {
        base.Init();

        if (editorVisualizer == null)
            editorVisualizer = visualizer as EditorNavVisualizer;


        editorVisualizer.ResetRotation();
    }

    public override void SwitchOff()
    {
        if (EditorState.navMode == NavMode.Grid) return;
        EditorState.navMode = NavMode.Grid;
        gridNav.InitGridMode();
    }
}
