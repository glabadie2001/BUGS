using UnityEngine;
using System.Collections.Generic;
using Gabadie.GFSM;

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
    Vector2 direction;

    public PlayerAttackEvent(Player _player, Attack _attack)
    {
        player = _player;
        attack = _attack;
        direction = InputManager.Inst.lastInput.move.normalized;
    }
    
    public override void Execute()
    {
        // If no direction input, default to facing direction
        if (direction == Vector2.zero)
            direction = new Vector2(-player.transform.localScale.x, 0);
        else
            direction = direction.normalized; // Ensure the direction is normalized

        // Calculate spawn position by rotating around the player at a fixed radius
        Vector3 spawnPosition = player.transform.position + (Vector3)(direction * attack.radius);

        // Calculate rotation to face attack direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // Instantiate attack prefab
        AttackParticle spawnedAttack = GameObject.Instantiate(attack.prefab, spawnPosition, rotation).GetComponent<AttackParticle>();

        spawnedAttack.target = player;
    }
}

public class PlayerTransitionEvent : Event
{
    Player player;
    InputFrame input;
    State target;

    public PlayerTransitionEvent(Player _player, State _target)
    {
        player = _player;
        target = _target;
    }

    public override void Execute()
    {
        player.animator.PlayAnim(target.Name);
    }
}