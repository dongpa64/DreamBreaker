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
    public Button joinButton;         // 입장 버튼
    public TMP_Text statusText;           // 안내 메시지

    void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject); // 이미 인스턴스가 있으면 자신을 파괴
        }
    }
    void Start()
    {
        statusText.text = "Photon Login...";
        joinButton.interactable = false;
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    

    // Photon 서버 접속 성공
    public override void OnConnectedToMaster()
    {
        statusText.text = "Photon Linked!";
        joinButton.interactable = true;
        OnClickJoin();
    }

    // 서버 접속 해제
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (statusText != null)
            statusText.text = $"Disconnected: {cause}";
        joinButton.interactable = false;
    }

    // 입장 버튼 클릭 시 호출
    public void OnClickJoin()
    {
        joinButton.interactable = false;
        statusText.text = "Matching...";
        PhotonNetwork.JoinRandomRoom(); // 빈 방이 있으면 입장, 없으면 새 방 생성
    }

    // 빈 방이 없을 때 새 방 생성
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        statusText.text = "Create New Room...";
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom("Dream", options);
    }

    // 룸에 입장 성공
    public override void OnJoinedRoom()
    {
        statusText.text = "Success!";
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            photonView.RPC("MoveScene", RpcTarget.All);
    }

    // 다른 플레이어가 입장했을 때(2명 됐을 때)
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        statusText.text = "두 번째 플레이어 입장!\n튜토리얼 맵 이동!";
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
