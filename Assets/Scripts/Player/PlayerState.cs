using System.Linq;
using System;
using Gabadie.GFSM;

public enum PlayerStateType
{
    Idle,
    Walk,
    Crouch,
    Jump,
    Fall,
    Any // Represents transitions from any state
}

//[System.Serializable]
//public struct Transition
//{
//    public PlayerState Source { get; private set; }
//    public PlayerState Target { get; private set; }

//    Func<PlayerController, InputFrame, bool>[] condition;
//    bool fromAny;

//    public Transition(PlayerState _source, PlayerState _target, params Func<PlayerController, InputFrame, bool>[] _condition)
//    {
//        Source = _source;
//        Target = _target;
//        condition = _condition; 
//        fromAny = false;
//    }

//    public Transition(PlayerState _target, params Func<PlayerController, InputFrame, bool>[] _condition)
//    {
//        Source = PlayerState.Any;
//        Target = _target;
//        condition = _condition;
//        fromAny = true;
//    }

//    public bool Try(PlayerController controller, InputFrame input)
//    {
//        if (!Source.complete) return false;

//        if (((fromAny && controller.State != Target) || controller.State == Source) && condition.All(condition => condition(controller, input)))
//        {
//            //controller.Transition(target, input);
//            return true;
//        }
//        return false;
//    }
//}
