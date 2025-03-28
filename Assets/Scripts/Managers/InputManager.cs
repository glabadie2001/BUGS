using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputManager))]
public class InputManager : MonoBehaviour
{
    public static InputManager Inst;

    public string[] actions = {
        "Move",
        "Look",
        "Jump",
        "Attack",
        "Dash"
     };
    Dictionary<string, InputAction> actionMap;

    private void Awake()
    {
        if (Inst == null)
            Inst = this;
        else if (Inst != this)
            Destroy(this);


        foreach (var action in actions)
        {
            actionMap.Add(action, InputSystem.actions.FindAction(action));
        }
    }

    public InputFrame ProcessInput()
    {
        Vector2 movement = actionMap["Move"].ReadValue<Vector2>();
        // Normalize input if it exceeds 1 in combined length (for diagonal movement)
        if (movement.magnitude > 1f)
            movement.Normalize();

        return new InputFrame(movement, actionMap["Jump"].WasPerformedThisFrame(), actionMap["Attack"].WasPerformedThisFrame());
    }
}

public struct InputFrame
{
    public Vector2 move;
    public bool jump;
    public bool attack;

    public InputFrame(Vector2 _move, bool _jump, bool _attack)
    {
        move = _move;
        jump = _jump;
        attack = _attack;
    }
}
