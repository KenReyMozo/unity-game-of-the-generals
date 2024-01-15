using UnityEngine;
using Photon.Pun;

public class PlayerView : MonoBehaviour
{
    [SerializeField] protected GameObject[] toDisable;
    [SerializeField] protected PhotonView PV;
}
