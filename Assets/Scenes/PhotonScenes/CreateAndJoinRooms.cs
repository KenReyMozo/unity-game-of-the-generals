using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField createRoomInput, joinRoomInput;
    [SerializeField] Button createRoomBtn, joinRoomBtn;
    [SerializeField] TextMeshProUGUI errorMessage;
    [SerializeField] string sceneName = "MainScene";
    string createRoomName = "", joinRoomName = "";

    bool isLoading = false;
    public bool IsLoading { get => isLoading; set => isLoading = value; }

    void Start()
    {
        ValidateButton(createRoomBtn, createRoomName);
        ValidateButton(joinRoomBtn, joinRoomName);
    }

    public void CreateRoom()
    {
        EnableStartCreateOrJoin(false);
        PhotonNetwork.CreateRoom(createRoomInput.text);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        EnableStartCreateOrJoin(true);
        errorMessage.text = message;
    }
    public void JoinRoom()
    {
        EnableStartCreateOrJoin(false);
        PhotonNetwork.JoinRoom(joinRoomInput.text);
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        EnableStartCreateOrJoin(true);
        errorMessage.text = message;
    }

    public void OnChangeCreateRoomInput()
    {
        createRoomName = createRoomInput.text;
        ValidateButton(createRoomBtn, createRoomName);
    }
    public void OnChangeJoinRoomInput()
    {
        joinRoomName = joinRoomInput.text;
        ValidateButton(joinRoomBtn, joinRoomName);
    }

    void ValidateButton(Button button, string text)
    {
        if (text.Length <= 2)
        {
            button.interactable = false;
        }
        else
        {
            button.interactable = true;
        }
        errorMessage.text = "";
    }

    public override void OnJoinedRoom()
    {
        EnableStartCreateOrJoin(false);
        PhotonNetwork.LoadLevel(sceneName);
    }

    private void EnableStartCreateOrJoin(bool isEnabled)
    {
        IsLoading = !isEnabled;
        createRoomBtn.interactable = isEnabled;
        joinRoomBtn.interactable = isEnabled;
        createRoomInput.interactable = isEnabled;
        joinRoomInput.interactable = isEnabled;
    }
}
