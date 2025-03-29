using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField]
    Animator playerAnim;
    [SerializeField]
    Animator attackAnim;

    public void PlayAnim(string trigger)
    {
        attackAnim.SetTrigger(trigger);
    }

    public void PlayAttack(string trigger)
    {
        attackAnim.SetTrigger(trigger);
    }
}
