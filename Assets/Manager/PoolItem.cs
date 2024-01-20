using UnityEngine;

public class PoolItem : MonoBehaviour
{
    PoolManager poolManager;
    [SerializeField] float lifeTime = 5f;

    public void SetPoolManager(PoolManager poolManager)
    {
        this.poolManager = poolManager;
    }

    ParticleSystem _particleSystem;
    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        _particleSystem.Play();
        Invoke(nameof(ReleaseSelf), lifeTime);
    }

    void ReleaseSelf()
    {
        poolManager.ReleaseFXItem(this);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }
    private void OnDestroy()
    {
        CancelInvoke();
    }
}
