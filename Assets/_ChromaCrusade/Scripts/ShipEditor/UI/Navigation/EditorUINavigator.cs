using System.Collections;
using UnityEngine;

public class EditorUINavigator : UINavigator, IUINavigator
{
    protected EditorNavVisualizer editorVisualizer;

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


    public void DoDelayedNav(NavItem target)
    {
        if(delayedNavRoutine != null) StopCoroutine(delayedNavRoutine);
        delayedNavRoutine = StartCoroutine(DelayedNavigate(target));
    }

    Coroutine delayedNavRoutine;
    IEnumerator DelayedNavigate(NavItem target)
    {
        while (EditorState.Scrolling)
            yield return null;

        NavToItem(target);
    }
}
