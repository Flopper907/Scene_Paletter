using Godot;
using Addons.ScenePaletter.Core;
using System;
using System.ComponentModel;

namespace Addons.ScenePaletter.Dialogs;

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

[Tool]
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