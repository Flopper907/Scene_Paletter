using Godot;
using System;

namespace Addons.ScenePaletter.Dialogs;

public partial class DeleteDialog : Page<DeleteDialogData>
{
    [Export] public Button cancelButton;
    [Export] public Button deleteButton;

    public override void Initialize()
    {
        cancelButton.Pressed += data.cancelAction;
        deleteButton.Pressed += data.deleteAction;
    }
}

public struct DeleteDialogData
{
    public Action cancelAction;
    public Action deleteAction;

    public DeleteDialogData(Action cancelAction, Action deleteAction)
    {
        this.cancelAction = cancelAction;
        this.deleteAction = deleteAction;
    }
}