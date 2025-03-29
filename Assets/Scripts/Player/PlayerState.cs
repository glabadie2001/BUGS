using System.Linq;
using System;
using UnityEngine;

public enum PlayerStateType
{
    Idle,
    Walk,
    Crouch,
    Jump,
    Fall,
    Any // Represents transitions from any state
}

[System.Serializable]
public struct PlayerState
{
    public string name;
    public Action<InputFrame> Init { get; }
    //TODO: Is passing InputInfo redundant?
    public Action<InputFrame, float> Update { get; }
    public Action<InputFrame> Exit { get; }
    public bool complete;

    public static PlayerState Any => new PlayerState("Any");

    public PlayerState(string _name, Action<InputFrame> _init = null, Action<InputFrame, float> _update = null, Action<InputFrame> _exit = null)
    {
        name = _name;
        Init = _init ?? ((InputFrame input) => { });
        Update = _update ?? ((InputFrame i, float dT) => { });
        Exit = _exit ?? ((InputFrame input) => { });
        complete = true;
    }

    public static bool operator ==(PlayerState left, PlayerState right) { return left.name == right.name; }

    public static bool operator !=(PlayerState left, PlayerState right) { return !(left == right); }

    public static bool operator ==(PlayerState left, string right) { return left.name == right; }
    public static bool operator !=(PlayerState left, string right) { return !(left.name == right); }

    public static bool operator ==(string left, PlayerState right) { return left == right.name; }
    public static bool operator !=(string left, PlayerState right) { return !(left == right.name); }


    public override string ToString() { return name; }
}

[System.Serializable]
public struct Transition
{
    public PlayerState Source { get; private set; }
    public PlayerState Target { get; private set; }

    Func<PlayerController, InputFrame, bool>[] condition;
    bool fromAny;

    public Transition(PlayerState _source, PlayerState _target, params Func<PlayerController, InputFrame, bool>[] _condition)
    {
        Source = _source;
        Target = _target;
        condition = _condition;
        fromAny = false;
    }

    public Transition(PlayerState _target, params Func<PlayerController, InputFrame, bool>[] _condition)
    {
        Source = PlayerState.Any;
        Target = _target;
        condition = _condition;
        fromAny = true;
    }

    public bool Try(PlayerController controller, InputFrame input)
    {
        if (!Source.complete) return false;

        if (((fromAny && controller.State != Target) || controller.State == Source) && condition.All(condition => condition(controller, input)))
        {
            //controller.Transition(target, input);
            return true;
        }
        return false;
    }
}
