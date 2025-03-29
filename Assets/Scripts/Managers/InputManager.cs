using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputManager))]
public class InputManager : MonoBehaviour
{
    public static InputManager Inst;

    public InputFrame lastInput;

    public string[] actions = {
        "Move",
        "Look",
        "Jump",
        "Attack",
        "Dash"
     };
    Dictionary<string, InputAction> actionMap = new Dictionary<string, InputAction>();

    private void Awake()
    {
        //Singleton boilerplate
        if (Inst == null)
            Inst = this;
        else if (Inst != this)
            Destroy(this);

        //Initialize mapping to global actions (see ProjectSettings/Input in engine)
        foreach (var action in actions)
        {
            actionMap.Add(action, InputSystem.actions.FindAction(action));
        }
    }

    private void Start()
    {
        lastInput = ProcessInput();
    }

    private void Update()
    {
        lastInput = ProcessInput();
    }

    public InputFrame ProcessInput()
    {
        Vector2 movement = actionMap["Move"].ReadValue<Vector2>();
        if (movement.magnitude > 1f)
            movement.Normalize();

        bool jumpDown = actionMap["Jump"].WasPressedThisFrame();

        bool attackDown = actionMap["Attack"].WasPressedThisFrame();

        return new InputFrame(movement, jumpDown, attackDown);
    }
}

[System.Serializable]
public struct InputFrame
{
    public Vector2 move;
    public bool jumpDown;
    public bool attack;

    public InputFrame(Vector2 _move, bool _jumpDown, bool _attack)
    {
        move = _move;
        jumpDown = _jumpDown;
        attack = _attack;
    }
}
