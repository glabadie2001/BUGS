using Gabadie.GFSM;
using UnityEngine;

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
    [SerializeField] float gravity = 1f;
    [SerializeField] float fallGravityMult = 1.5f;
    [SerializeField] float speed = 5f;
    [SerializeField] float accel = 1f;
    [SerializeField] float decel = 1f;
    [SerializeField] float airMult = 1f;
    [SerializeField] float jumpStopMult = 0.5f;
    [SerializeField] float crouchThreshold = -0.8f;
    [SerializeField] float jumpVelocity = 5f;
    [SerializeField] float jumpChargeRate = 2.5f;
    [SerializeField] float maxJumpCharge = 5f;
    [SerializeField] float jumpCharge = 0f;
    [SerializeField] float slideMult = 1.5f;
    [SerializeField] float slideSlowRate = 2.5f;

    [SerializeField] float groundRayLength = 1.01f;

    public Vector2 velocity;

    [Header("State Machine")]
    public FSM fsm = new FSM();

    public bool Grounded => Physics2D.Raycast(transform.position, Vector3.down, groundRayLength, environmentMask).collider != null;

    void InitStates()
    {
        fsm.AddState(new State("Idle",
            update: (float deltaTime) =>
            {
                rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, decel * Time.deltaTime);
            }
        ));

        fsm.AddState(new State("Run",
            update: (float deltaTime) =>
            {
                Vector2 horizontalVelocity = new Vector3(InputManager.Inst.lastInput.move.x * speed, 0f);
                rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, new Vector2(horizontalVelocity.x, 0), accel * Time.deltaTime); 
            }
        ));

        fsm.AddState(new State("Crouch",
            update: (float deltaTime) =>
            {
                jumpCharge = Mathf.Min(maxJumpCharge, jumpCharge + (jumpChargeRate * deltaTime));
            },
            onExit: (State target) => {
                if (target.Name != "Jump")
                    jumpCharge = 0f;
            }
        ));

        fsm.AddState(new State("Jump",
            onEnter: () =>
            {
                Jump();
            },
            update: (float deltaTime) =>
            {
                rb.linearVelocity = new Vector2(InputManager.Inst.lastInput.move.x * speed * airMult, rb.linearVelocity.y - gravity * Time.deltaTime);
            },
            onExit: (State target) =>
            {
                jumpCharge = 0f;
            }
        ));

        fsm.AddState(new State("Fall",
            onEnter: () =>
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpStopMult);
            },
            update: (float deltaTime) =>
            {
                rb.linearVelocity = new Vector2(InputManager.Inst.lastInput.move.x * speed * airMult, rb.linearVelocity.y - gravity * fallGravityMult * Time.deltaTime);
            }
        ));

        fsm.AddState(new State("Slide",
            onEnter: () =>
            {
                // Apply the clamped horizontal velocity while preserving vertical velocity
                Slide();

                jumpCharge = 0f;
            },
            update: (float deltaTime) =>
            {
                rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, slideSlowRate * deltaTime);
                jumpCharge = Mathf.Min(maxJumpCharge, jumpCharge + (jumpChargeRate * deltaTime));
            },
            onExit: (State target) => {
                if (target.Name != "Jump" && target.Name != "Crouch")
                    jumpCharge = 0f;
            }
        ));
    }
    
    //TODO: InputManager overhead?
    void InitTransitions()
    {
        fsm.AddTransitions(
            new Transition<State>(fsm["Idle"], fsm["Run"],
                () => InputManager.Inst.lastInput.move.x != 0),

            new Transition<State>(fsm["Run"], fsm["Idle"],
                () => InputManager.Inst.lastInput.move.x == 0),

            new Transition<State>(fsm["Slide"],
                () => fsm.State != "Crouch"
                && Grounded
                && InputManager.Inst.lastInput.crouchDown
                && InputManager.Inst.lastInput.move.x != 0),

            new Transition<State>(fsm["Crouch"],
                () => Grounded
                && InputManager.Inst.lastInput.crouchDown
                && InputManager.Inst.lastInput.move.x == 0),

            new Transition<State>(fsm["Crouch"], fsm["Idle"],
                () => InputManager.Inst.lastInput.crouchDown),

            new Transition<State>(fsm["Jump"],
                () => Grounded && InputManager.Inst.lastInput.jumpDown),

            new Transition<State>(fsm["Fall"],
                () => (!Grounded && rb.linearVelocity.y <= 0)
                || (InputManager.Inst.lastInput.jumpUp)),

            new Transition<State>(fsm["Jump"], fsm["Idle"],
                () => Grounded && rb.linearVelocity.y <= 0),

            new Transition<State>(fsm["Fall"], fsm["Idle"],
                () => Grounded 
                && rb.linearVelocity.x == 0
                && !InputManager.Inst.lastInput.crouchHeld),

            new Transition<State>(fsm["Fall"], fsm["Run"],
                () => Grounded
                && rb.linearVelocity.x != 0
                && !InputManager.Inst.lastInput.crouchHeld),

            new Transition<State>(fsm["Fall"], fsm["Crouch"],
                () => Grounded
                && InputManager.Inst.lastInput.crouchHeld
                && rb.linearVelocity.x == 0),

            new Transition<State>(fsm["Fall"], fsm["Slide"],
                () => Grounded
                && InputManager.Inst.lastInput.crouchHeld
                && rb.linearVelocity.x != 0),

            new Transition<State>(fsm["Slide"], fsm["Crouch"], () => rb.linearVelocity.x == 0),

            new Transition<State>(fsm["Slide"], fsm["Idle"], () => InputManager.Inst.lastInput.crouchDown)
        );
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity + jumpCharge);
    }

    void Slide()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * slideMult, 0);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        InitStates();
        InitTransitions();

        //Initial state
        fsm.Interrupt(fsm["Idle"]);
    }

    void FixedUpdate()
    {
        velocity = rb.linearVelocity;
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
