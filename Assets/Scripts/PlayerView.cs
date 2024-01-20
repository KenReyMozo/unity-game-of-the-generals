using UnityEngine;
using Photon.Pun;

public class PlayerView : MonoBehaviourPunCallbacks
{
    [SerializeField] protected GameObject[] toDisable;
    [SerializeField] public PhotonView PV;


    protected PlayerManager playerManager;
    public PlayerManager PlayerManager { get => playerManager; set => playerManager = value; }

    private void Awake()
    {
        if (PV == null)
            PV = GetComponent<PhotonView>();
    }
}
