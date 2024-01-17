using UnityEngine;
using Photon.Pun;

public class PlayerView : MonoBehaviourPunCallbacks
{
    [SerializeField] protected GameObject[] toDisable;
    [SerializeField] public PhotonView PV;
}
