using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public InputField nicknameInput;  // 닉네임 입력란
    public Button joinButton;         // 입장 버튼
    public Text statusText;           // 안내 메시지

    void Start()
    {
        statusText.text = "Photon 접속 중...";
        joinButton.interactable = false;
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Photon 서버 접속 성공
    public override void OnConnectedToMaster()
    {
        statusText.text = "Photon 서버 접속 완료!";
        joinButton.interactable = true;
    }

    // 서버 접속 해제
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (statusText != null)
            statusText.text = $"접속 끊김: {cause}";
        joinButton.interactable = false;
    }

    // 입장 버튼 클릭 시 호출
    public void OnClickJoin()
    {
        string nickname = nicknameInput.text.Trim();

        if (nickname.Length == 0)
        {
            statusText.text = "닉네임을 입력하세요.";
            return;
        }

        PhotonNetwork.NickName = nickname;
        joinButton.interactable = false;
        statusText.text = "룸 자동 매칭 중...";
        PhotonNetwork.JoinRandomRoom(); // 빈 방이 있으면 입장, 없으면 새 방 생성
    }

    // 빈 방이 없을 때 새 방 생성
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        statusText.text = "빈 방 없음, 새 방 생성 중...";
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(null, options);
    }

    // 룸에 입장 성공
    public override void OnJoinedRoom()
    {
        statusText.text = $"룸 입장 성공! ({PhotonNetwork.CurrentRoom.Name})\n상대방 입장 대기 중...";
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            photonView.RPC("GotoTutorial", RpcTarget.All);
    }

    // 다른 플레이어가 입장했을 때(2명 됐을 때)
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        statusText.text = "두 번째 플레이어 입장!\n튜토리얼 맵 이동!";
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            photonView.RPC("GotoTutorial", RpcTarget.All);
    }

    [PunRPC]
    void GotoTutorial()
    {
        if (statusText != null)
            statusText.text = "튜토리얼 맵 이동!";
        PhotonNetwork.LoadLevel("TutorialMapScene");
    }
}
