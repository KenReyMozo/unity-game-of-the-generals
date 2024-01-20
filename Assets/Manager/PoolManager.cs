using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    [SerializeField] PoolItem FXPrefab;

    [SerializeField] int defaultCapacity = 10;
    [SerializeField] int maxCapacity = 50;


    ObjectPool<PoolItem> fxPool;
    public ObjectPool<PoolItem> FXPool { get => fxPool; private set => fxPool = value; }

    void Start()
    {
        InitializePool();
    }

    void InitializePool()
    {
        FXPool = new ObjectPool<PoolItem>(
            CreateFX,
            GetFX,
            ReleaseFX,
            DestroyFX,
            false,
            defaultCapacity,
            maxCapacity
            );
    }

    PoolItem CreateFX()
    {
        PoolItem obj = Instantiate(FXPrefab);
        obj.SetPoolManager(this);
        return obj;
    }
    void GetFX(PoolItem obj)
    {
        obj.gameObject.SetActive(true);
    }
    void ReleaseFX(PoolItem obj)
    {
        obj.gameObject.SetActive(false);
    }
    void DestroyFX(PoolItem obj)
    {
        Destroy(obj);
    }

    public PoolItem GetFXItem()
    {
        PoolItem item = FXPool.Get();
        return item;
    }
    public PoolItem GetFXItem(Vector3 position)
    {
        PoolItem item = FXPool.Get();
        item.transform.position = position;
        return item;
    }
    public void ReleaseFXItem(PoolItem item)
    {
        FXPool.Release(item);
    }
}