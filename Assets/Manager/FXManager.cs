using UnityEngine;
using UnityEngine.Pool;

public class FXManager : MonoBehaviour
{
    public static FXManager Instance;

    [SerializeField] PoolManager WinFXPoolManager;
    [SerializeField] PoolManager LoseFXPoolManager;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void GetWinFX(Vector3 position)
    {
        WinFXPoolManager.GetFXItem(position);
    }

    public void GetLoseFX(Vector3 position)
    {
        LoseFXPoolManager.GetFXItem(position);
    }
}
