using UnityEngine;
using System.Collections.Generic;

public class EventManager : MonoBehaviour
{
    public static EventManager Inst;
    public Queue<Event> eventQueue = new();

    private void Awake()
    {
        //Singleton boilerplate
        if (Inst == null)
            Inst = this;
        else if (Inst != this)
            Destroy(this);
    }

    private void Update()
    {
        while (eventQueue.Count > 0)
        {
            eventQueue.Dequeue().Execute();
        }
    }

    public void Send(Event e)
    {
        eventQueue.Enqueue(e);
    }
}

public abstract class Event
{
    public abstract void Execute();
}

public class PlayerAttackEvent : Event
{
    Player player;
    Attack attack;

    public PlayerAttackEvent(Player _player, Attack _attack)
    {
        player = _player;
        attack = _attack;
    }

    public override void Execute()
    {
        player.animator.PlayAttack(attack.animTrigger);
        //player.controller.Attack(attack);
    }
}

public class PlayerTransitionEvent : Event
{
    Player player;
    PlayerState target;

    public PlayerTransitionEvent(Player _player, PlayerState _target)
    {
        player = _player;
        target = _target;
    }

    public override void Execute()
    {
        player.animator.
        player.controller.Transition(target, InputManager.Inst.lastInput);
    }
}