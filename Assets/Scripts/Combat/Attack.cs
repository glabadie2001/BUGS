using UnityEngine;

[CreateAssetMenu(fileName = "Attack", menuName = "BUGS/Attack", order = 0)]
public class Attack : ScriptableObject
{
    [SerializeField]
    string animTrigger;

    [SerializeField]
    AttackFrame[] hitFrames;

    public int Length => hitFrames.Length;

    public AttackFrame this[int index]
    {
        get => hitFrames[index];
    }
}

public struct AttackFrame
{
    public bool active;
}
