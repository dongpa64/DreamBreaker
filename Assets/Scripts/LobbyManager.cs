using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public int playerCount = 2;
    public string sceneName = "Perspective";
    LobbyManager Instance { get; set; }
    [Header("UI")]
    public TMP_Text statusText;           // �ȳ� �޽���

    
    void Start()
    {
        statusText.text = "Server Login...";
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    

    // Photon ���� ���� ����
    public override void OnConnectedToMaster()
    {
        statusText.text = "Photon Linked!";
        OnClickJoin();
    }

    // ���� ���� ����
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (statusText != null)
            statusText.text = $"Disconnected: {cause}";
    }

    // ���� ��ư Ŭ�� �� ȣ��
    public void OnClickJoin()
    {
        statusText.text = "Matching...";
        PhotonNetwork.JoinRandomRoom(); // �� ���� ������ ����, ������ �� �� ����
    }

    // �� ���� ���� �� �� �� ����
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        statusText.text = "Create New Room";
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom("Dream", options);
    }

    // �뿡 ���� ����
    public override void OnJoinedRoom()
    {
        statusText.text = "Wait For Another Player";
        if (PhotonNetwork.CurrentRoom.PlayerCount == playerCount)
        {
            if(PhotonNetwork.IsMasterClient) 
                photonView.RPC("MoveScene", RpcTarget.All);
        }
    }

    // �ٸ� �÷��̾ �������� ��(2�� ���� ��)
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        statusText.text = "Second Player!\nGoto Tut!";
        if (PhotonNetwork.CurrentRoom.PlayerCount == playerCount)
        {
            if (PhotonNetwork.IsMasterClient)
                photonView.RPC("MoveScene", RpcTarget.All);
        }
    }

    [PunRPC]
    void MoveScene()
    {
        if (statusText != null)
            statusText.text = "Welcome!";
        if(photonView.IsMine)
            PhotonNetwork.LoadLevel(sceneName);
    }
}
