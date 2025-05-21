using UnityEngine.InputSystem;

namespace Dvsilch;

public readonly struct InputInfo
{
    public ButtonMapping Button { get; init; }

    public InputAction.CallbackContext Ctx { get; init; }

    public void Deconstruct(out ButtonMapping button, out InputAction.CallbackContext ctx)
    {
        button = Button;
        ctx = Ctx;
    }
}