using UnityEngine;

public class Particle : MonoBehaviour
{
    float _lifetime = 0;
    [SerializeField] float _maxLifetime = 5f;

    // Update is called once per frame
    void Update()
    {
        if (_lifetime > _maxLifetime)
            Destroy(gameObject);

        _lifetime += Time.deltaTime;
    }
}
