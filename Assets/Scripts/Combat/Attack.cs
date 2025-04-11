using UnityEngine;

[CreateAssetMenu(fileName = "Attack", menuName = "BUGS/Attack", order = 0)]
public class Attack : ScriptableObject
{
    public GameObject prefab;
    public float radius;

    [SerializeField]
    AttackFrame[] hitFrames;

    public int Length => hitFrames.Length;

    public AttackFrame this[int index]
    {
        get => hitFrames[index];
    }
}

[System.Serializable]
public struct AttackFrame
{
    public bool active;
    public Vector2 hitOffset;
    public Vector2 hitSize;
}
