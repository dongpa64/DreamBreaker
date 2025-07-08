using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public static LobbyManager Instance { get; private set; }
    [Header("UI")]
    public Button joinButton;         // ���� ��ư
    public TMP_Text statusText;           // �ȳ� �޽���

    void Awake()
    {
        // �̱��� �ν��Ͻ� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject); // �̹� �ν��Ͻ��� ������ �ڽ��� �ı�
        }
    }
    void Start()
    {
        statusText.text = "Photon Login...";
        joinButton.interactable = false;
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    

    // Photon ���� ���� ����
    public override void OnConnectedToMaster()
    {
        statusText.text = "Photon Linked!";
        joinButton.interactable = true;
        OnClickJoin();
    }

    // ���� ���� ����
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (statusText != null)
            statusText.text = $"Disconnected: {cause}";
        joinButton.interactable = false;
    }

    // ���� ��ư Ŭ�� �� ȣ��
    public void OnClickJoin()
    {
        joinButton.interactable = false;
        statusText.text = "Matching...";
        PhotonNetwork.JoinRandomRoom(); // �� ���� ������ ����, ������ �� �� ����
    }

    // �� ���� ���� �� �� �� ����
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        statusText.text = "Create New Room...";
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom("Dream", options);
    }

    // �뿡 ���� ����
    public override void OnJoinedRoom()
    {
        statusText.text = "Success!";
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            photonView.RPC("MoveScene", RpcTarget.All);
    }

    // �ٸ� �÷��̾ �������� ��(2�� ���� ��)
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        statusText.text = "�� ��° �÷��̾� ����!\nƩ�丮�� �� �̵�!";
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            photonView.RPC("MoveScene", RpcTarget.All);
    }

    [PunRPC]
    void MoveScene()
    {
        if (statusText != null)
            statusText.text = "Go!";
        if(photonView.IsMine)
            PhotonNetwork.LoadLevel("Perspective");
    }
}
