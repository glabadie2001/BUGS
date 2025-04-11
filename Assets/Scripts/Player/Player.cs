using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Dependencies")]
    public PlayerController controller;
    public PlayerAnimator animator;
    public Health health;

    [SerializeField] Transform _feet;
    public Transform attackPos;

    [Header("Prefabs")]
    [SerializeField] GameObject _jumpDust;

    private void Update()
    {
        InputFrame input = InputManager.Inst.lastInput;

        animator.SetFloat("Speed", Mathf.Abs(input.move.x));

        if (input.move.x != 0 && controller.fsm.State != "Slide")
            transform.localScale = new Vector3(-Mathf.Sign(input.move.x), transform.localScale.y, transform.localScale.z);

        if(controller.fsm.Poll(Time.deltaTime))
        {
            if (controller.fsm.State == "Jump")
            {
                Instantiate(_jumpDust, _feet.position, Quaternion.identity);
            }

            EventManager.Inst.Send(new PlayerTransitionEvent(this, controller.fsm.State));
        }

        if (input.attack)
        {
            EventManager.Inst.Send(new PlayerAttackEvent(this, controller.basicAttack));
        }
    }
}
