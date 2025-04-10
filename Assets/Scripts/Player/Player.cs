using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerController controller;
    public PlayerAnimator animator;
    public Health health;

    private void Update()
    {
        InputFrame input = InputManager.Inst.lastInput;

        if (input.move.x != 0)
            transform.localScale = new Vector3(-Mathf.Sign(input.move.x), transform.localScale.y, transform.localScale.z);

        if(controller.fsm.Poll(Time.deltaTime))
        {
            EventManager.Inst.Send(new PlayerTransitionEvent(this, controller.fsm.State));
        }

        if (input.attack)
        {
            EventManager.Inst.Send(new PlayerAttackEvent(this, controller.basicAttack));
        }
    }
}
