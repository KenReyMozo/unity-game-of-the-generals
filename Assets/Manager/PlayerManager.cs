using UnityEngine;
using Photon.Pun;
using System.IO;
using Photon.Realtime;

public class PlayerManager : MonoBehaviour
{
    public PhotonView PV;
    [SerializeField] GameObject playerPrefab;
    Board board;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        SpawnPlayerOnline();
    }

    void SpawnPlayerOnline()
    {
        if (PV.IsMine)
        {
            GameObject player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), Vector3.zero, Quaternion.identity, 0, new object[] { PV.ViewID });
            PlayerView[] playerViews = player.GetComponents<PlayerView>();
            foreach (PlayerView view in playerViews)
            {
                view.PlayerManager = this;
            }
        }
    }

    public void RespawnPlayerOnline(GameObject obj)
    {
        PhotonNetwork.Destroy(obj);
        SpawnPlayerOnline();
    }
}
