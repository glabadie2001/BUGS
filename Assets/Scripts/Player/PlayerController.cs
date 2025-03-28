using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    LayerMask environmentMask;

    [Header("Dependencies")]
    Rigidbody2D rb;

    [Header("Combat")]
    [SerializeField]
    Collider2D hitbox;
    [SerializeField]
    bool attacking;
    public Attack basicAttack;

    [Header("Movement")]
    [SerializeField]
    float speed = 5f;
    [SerializeField]
    float airMult = 0.25f;
    [SerializeField]
    float jumpForce = 5f;
    [SerializeField]
    float groundRayLength = 1.01f;

    [Header("State Machine")]
    [SerializeField]
    InputFrame lastInput;
    [SerializeField]
    PlayerState currentState;
    public PlayerState State => currentState;

    PlayerState idle;
    PlayerState walk;
    PlayerState jump;
    PlayerState fall;

    Transition[] transitions;

    bool IsGrounded()
    {
        return Physics2D.Raycast(transform.position, Vector3.down, groundRayLength, environmentMask).collider != null;
    }

    public void Transition(PlayerState target)
    {
        currentState = target;
        currentState.Init();
    }

    void InitTransitions()
    {
        idle = new PlayerState("Idle", (InputFrame info, float deltaTime) => {
            rb.linearVelocity = Vector2.zero;
        });

        walk = new PlayerState("Walk",
            (InputFrame input, float deltaTime) =>
            {
                Vector2 moveDirection = lastInput.move;

                // Only modify the horizontal components of velocity
                Vector2 currentVelocity = rb.linearVelocity;
                Vector2 horizontalVelocity = new Vector3(moveDirection.x * speed, 0f);
                rb.linearVelocity = new Vector2(horizontalVelocity.x, currentVelocity.y);
            }
        );

        jump = new PlayerState("Jump",
            () =>
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        );

        fall = new PlayerState("Fall",
            (InputFrame info, float deltaTime) => {
                Vector2 moveDirection = lastInput.move;

                rb.linearVelocity = new Vector2(moveDirection.x * speed * airMult, rb.linearVelocity.y);
            }
        );

        //TODO: Input bindings
        transitions = new Transition[] {
            new Transition(idle, walk, controller => controller.lastInput.move != Vector2.zero),
            new Transition(walk, idle, controller => controller.lastInput.move == Vector2.zero),

            new Transition(jump, controller => controller.IsGrounded() && controller.lastInput.jump),
            new Transition(fall, controller => !controller.IsGrounded()),

            new Transition(fall, idle, controller => controller.IsGrounded()),

            //TODO: Will fix if reducing velocity to 0 for a frame becomes an issue.
            //new Transition(fall, walk, controller => controller.IsGrounded() && controller.lastInput.move != Vector2.zero),
        };
    }

    public void Attack(Attack atk)
    {
        if (attacking) return;
        StartCoroutine(DoAttack(atk));
    }

    IEnumerator DoAttack(Attack atk)
    {
        attacking = true;
        int frame = 0;

        while(true)
        {
            if (frame > atk.Length) break;
            hitbox.enabled = atk[frame].active;
            frame++;
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        InitTransitions();

        currentState = idle;
    }

    private void Start()
    {
        lastInput = InputManager.Inst.ProcessInput();
    }

    void Update()
    {
        lastInput = InputManager.Inst.ProcessInput();

        foreach (Transition transition in transitions)
        {
            if (transition.Try(this))
                break;
        }

        if (lastInput.attack)
        {
            Attack(basicAttack);
        }
    }

    void FixedUpdate()
    {
        currentState.Update(lastInput, Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        if (IsGrounded())
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.red;

        Gizmos.DrawLine(transform.position, transform.position + (Vector3.down * groundRayLength));
    }
}

[System.Serializable]
public struct PlayerState
{
    public string name;
    public Action Init;
    //TODO: Is passing InputInfo redundant?
    public Action<InputFrame, float> Update;
    public Action Exit;
    public bool complete;

    public static PlayerState Any => new PlayerState("Any", () => { });

    public PlayerState(string _name, Action _init, Action<InputFrame, float> _update, Action _exit)
    {
        name = _name;
        Init = _init;
        Update = _update;
        Exit = _exit;
        complete = true;
    }

    public PlayerState(string _name, Action _init)
    {
        name = _name;
        Init = _init;
        Update = (InputFrame input, float deltaTime) => { };
        Exit = () => { };
        complete = true;
    }

    public PlayerState(string _name, Action<InputFrame, float> _update)
    {
        name = _name;
        Init = () => { };
        Update = _update;
        Exit = () => { };
        complete = true;
    }

    public static bool operator ==(PlayerState left, PlayerState right) { return left.name == right.name; }

    public static bool operator !=(PlayerState left, PlayerState right) { return !(left == right); }

    public override string ToString() { return name; }
}

[System.Serializable]
public struct Transition
{
    PlayerState source;
    PlayerState target;
    Predicate<PlayerController>[] condition;
    bool fromAny;

    public Transition(PlayerState _source, PlayerState _target, params Predicate<PlayerController>[] _condition)
    {
        source = _source;
        target = _target;
        condition = _condition;
        fromAny = false;
    }

    public Transition(PlayerState _target, params Predicate<PlayerController>[] _condition)
    {
        source = PlayerState.Any;
        target = _target;
        condition = _condition;
        fromAny = true;
    }

    public bool Try(PlayerController controller)
    {
        if (!source.complete) return false;

        if ((fromAny || controller.State == source) && condition.All(condition => condition(controller)))
        {
            controller.Transition(target);
            return true;
        }
        return false;
    }
}

