using UnityEngine;

public class EditorCommandFactory
{
    private readonly IEditorCommandContext context;

    public EditorCommandFactory(IEditorCommandContext context) => this.context = context;


}
