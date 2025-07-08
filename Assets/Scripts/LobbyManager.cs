using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.Events;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public Button joinButton;         // ���� ��ư
    public TMP_Text statusText;           // �ȳ� �޽���
    UnityEvent joinedRoomEvent;
    void Start()
    {
        statusText.text = "Photon login...";
        joinButton.interactable = false;
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Photon ���� ���� ����
    public override void OnConnectedToMaster()
    {
        statusText.text = "Photon Linked!";
        joinButton.interactable = true;
    }

    // ���� ���� ����
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (statusText != null)
            statusText.text = $"Disconnect: {cause}";
        joinButton.interactable = false;
    }

    // ���� ��ư Ŭ�� �� ȣ��
    public void OnClickJoin()
    {
        joinButton.interactable = false;
        statusText.text = "Random Matching...";
        PhotonNetwork.JoinRandomRoom(); // �� ���� ������ ����, ������ �� �� ����
    }

    // �� ���� ���� �� �� �� ����
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        statusText.text = "No Empty Room, Create New Room...";
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(null, options);
    }

    // �뿡 ���� ����
    public override void OnJoinedRoom()
    {
        statusText.text = $"Success! ({PhotonNetwork.CurrentRoom.Name})\nStay Until Another Player...";
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            photonView.RPC("GotoTutorial", RpcTarget.All);
    }

    // �ٸ� �÷��̾ �������� ��(2�� ���� ��)
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        statusText.text = "Second Player Login!\nGo!";
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            photonView.RPC("Perspective", RpcTarget.All);
    }
}
