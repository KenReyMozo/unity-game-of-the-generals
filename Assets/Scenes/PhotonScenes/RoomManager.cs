using UnityEngine;
using Photon.Pun;
using System.IO;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;
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
    void Start()
    {
        int playerCount = PhotonNetwork.PlayerList.Length;
        Debug.LogError("Count:"+ playerCount);
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
    }

    public void ExitGame()
    {
        // Check if the application is running in the Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
				// Quit the application when not in the Unity Editor
				Application.Quit();
#endif
    }
}
