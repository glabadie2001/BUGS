using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerController controller;
    public PlayerAnimator animator;
    public Health health;

    private void Update()
    {
        controller.CheckTransitions(InputManager.Inst.lastInput);

        if (InputManager.Inst.lastInput.attack)
        {
            EventManager.Inst.Send(new PlayerAttackEvent(this, controller.basicAttack));
        }
    }
}
