using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] LayerMask environmentMask;

    [Header("Dependencies")]
    [SerializeField]
    Rigidbody2D rb;

    [Header("Combat")]
    [SerializeField] BoxCollider2D hitbox;
    [SerializeField] bool attacking;
    public Attack basicAttack;

    [Header("Movement")]
    [SerializeField] float speed = 5f;
    [SerializeField] float airMult = 0.25f;
    [SerializeField] float crouchThreshold = -0.8f;
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float jumpChargeRate = 2.5f;
    [SerializeField] float maxJumpCharge = 5f;
    [SerializeField] float jumpCharge = 0f;

    [SerializeField] float groundRayLength = 1.01f;

    [Header("State Machine")]
    [SerializeField] PlayerState currentState;
    public PlayerState State => currentState;

    Dictionary<string, PlayerState> states = new();
    Transition[] transitions;

    bool Grounded => Physics2D.Raycast(transform.position, Vector3.down, groundRayLength, environmentMask).collider != null;

    public void Transition(PlayerState target, InputFrame input)
    {
        currentState.Exit(input);
        currentState = target;
        currentState.Init(input);
    }

    public void CheckTransitions(InputFrame input)
    {
        foreach (Transition transition in transitions)
        {
            if (transition.Try(this, input))
            {
                EventManager.Inst.Send(new PlayerTransitionEvent(transition.Target))
                break;
            }
        }
    }

    void AddState(PlayerState state)
    {
        states.Add(state.name, state);
    }

    void InitStates()
    {
        AddState(new PlayerState("Idle",
            _update: (InputFrame input, float deltaTime) =>
            {
                rb.linearVelocity = Vector2.zero;
            }
        ));

        AddState(new PlayerState("Walk",
            _update: (InputFrame input, float deltaTime) =>
            {
                // Only modify the horizontal components of velocity
                Vector2 currentVelocity = rb.linearVelocity;
                Vector2 horizontalVelocity = new Vector3(input.move.x * speed, 0f);
                rb.linearVelocity = new Vector2(horizontalVelocity.x, currentVelocity.y);
            }
        ));

        AddState(new PlayerState("Crouch",
            _init: (InputFrame input) => jumpCharge = 0f,
            _update: (InputFrame input, float deltaTime) =>
            {
                rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, deltaTime);
                jumpCharge = Mathf.Min(maxJumpCharge, jumpCharge + (jumpChargeRate * deltaTime));
            },
            _exit: (InputFrame input) => {
                if (!input.jumpDown)
                    jumpCharge = 0f;
            }
        ));

        AddState(new PlayerState("Jump",
            _init: (InputFrame input) =>
            {
                rb.AddForce(Vector2.up * (jumpForce + jumpCharge), ForceMode2D.Impulse);
            },
            _exit: (InputFrame input) =>
            {
                jumpCharge = 0f;
            }
        ));

        AddState(new PlayerState("Fall",
            _update: (InputFrame input, float deltaTime) =>
            {
                rb.linearVelocity = new Vector2(input.move.x * speed * airMult, rb.linearVelocity.y);
            }
        ));
    }

    void InitTransitions()
    {
        //TODO: Input bindings
        transitions = new Transition[] {
            new Transition(states["Idle"], states["Walk"], (player, input) => input.move != Vector2.zero),
            new Transition(states["Walk"], states["Idle"], (player, input) => input.move == Vector2.zero),

            new Transition(states["Crouch"], (player, input) => player.Grounded && input.move.y < crouchThreshold),
            new Transition(states["Crouch"], states["Idle"], (player, input) => input.move.y > crouchThreshold),

            new Transition(states["Jump"], (player, input) => player.Grounded && input.jumpDown),
            new Transition(states["Fall"], (player, input) => !player.Grounded),

            new Transition(states["Fall"], states["Idle"], (player, input) => player.Grounded),

            //TODO: Will fix if reducing velocity to 0 for a frame becomes an issue.
            //new Transition(fall, walk, controller => controller.IsGrounded() && controller.lastInput.move != Vector2.zero),
        };
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        InitStates();
        InitTransitions();

        currentState = states["Idle"];
    }

    void FixedUpdate()
    {
        currentState.Update(InputManager.Inst.lastInput, Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        if (Grounded)
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.red;

        Gizmos.DrawLine(transform.position, transform.position + (Vector3.down * groundRayLength));
    }
}
