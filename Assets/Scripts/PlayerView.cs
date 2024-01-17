using UnityEngine;
using Photon.Pun;

public class PlayerView : MonoBehaviourPunCallbacks
{
    [SerializeField] protected GameObject[] toDisable;
    [SerializeField] protected PhotonView PV;
}
