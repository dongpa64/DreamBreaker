using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.Events;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public Button joinButton;         // 입장 버튼
    public TMP_Text statusText;           // 안내 메시지
    UnityEvent joinedRoomEvent;
    void Start()
    {
        statusText.text = "Photon login...";
        joinButton.interactable = false;
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Photon 서버 접속 성공
    public override void OnConnectedToMaster()
    {
        statusText.text = "Photon Linked!";
        joinButton.interactable = true;
    }

    // 서버 접속 해제
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (statusText != null)
            statusText.text = $"Disconnect: {cause}";
        joinButton.interactable = false;
    }

    // 입장 버튼 클릭 시 호출
    public void OnClickJoin()
    {
        joinButton.interactable = false;
        statusText.text = "Random Matching...";
        PhotonNetwork.JoinRandomRoom(); // 빈 방이 있으면 입장, 없으면 새 방 생성
    }

    // 빈 방이 없을 때 새 방 생성
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        statusText.text = "No Empty Room, Create New Room...";
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(null, options);
    }

    // 룸에 입장 성공
    public override void OnJoinedRoom()
    {
        statusText.text = $"Success! ({PhotonNetwork.CurrentRoom.Name})\nStay Until Another Player...";
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            photonView.RPC("GotoTutorial", RpcTarget.All);
    }

    // 다른 플레이어가 입장했을 때(2명 됐을 때)
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        statusText.text = "Second Player Login!\nGo!";
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            photonView.RPC("Perspective", RpcTarget.All);
    }
}
