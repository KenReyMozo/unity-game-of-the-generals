using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;
using System.Collections;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    [SerializeField] TextMeshProUGUI loadingText;
    [SerializeField] TextMeshProUGUI cueText;

    float progress = 0;
    bool canProceed = false;

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(LoadLobbyScene());
    }
    AsyncOperation asyncOperation;
    private IEnumerator LoadLobbyScene()
    {
        asyncOperation = SceneManager.LoadSceneAsync(SceneConstants.LOBBY_SCENE);
        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            yield return null;
        }
    }

    private void Update()
    {
        UpdateLoadingText();

        if (canProceed && Input.GetKeyDown(KeyCode.Space))
        {
            canProceed = false;
            SceneManager.LoadScene(SceneConstants.LOBBY_SCENE);
        }
    }

    void UpdateLoadingText()
    {
        if (asyncOperation == null || canProceed) return;

        progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

        if (progress >= 1)
            OnFinishLoading();
        else
            loadingText.text = "Loading: " + (progress * 100f).ToString("F0") + "%";
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnFinishLoading()
    {
        canProceed = true;
        cueText.enabled = true;
        loadingText.text = "DONE";
        StopAllCoroutines();
    }
}
