using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints;
    public PerspectiveScaler scaler;
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        // 이 씬(Perspective)이 로드되었을 때 플레이어 스폰
        // 이미 룸에 입장한 상태라면 바로 스폰, 아니라면(예: 재접속 시) 대기
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            SpawnPlayer();
        }

    }
    void SpawnPlayer()
    {
        // PhotonNetwork.LocalPlayer.ActorNumber는 1부터 시작
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        Vector3 spawnPosition;
        if (spawnPoints != null && spawnPoints.Length > 0 && playerIndex < spawnPoints.Length)
        {
            spawnPosition = spawnPoints[playerIndex].position;
        }
        else
        {
            // 스폰 포인트가 없거나 부족할 경우 기본 위치 사용
            spawnPosition = new Vector3(Random.Range(-5f, 5f), 1f, Random.Range(-5f, 5f));
        }
        
        // PhotonNetwork.Instantiate를 사용하여 네트워크 플레이어 오브젝트 생성
        var obj = PhotonNetwork.Instantiate("Player", spawnPosition, Quaternion.identity);
        var vrCam = obj.transform.Find("OVRCameraRig/TrackingSpace/CenterEyeAnchor");
        scaler.SetVrCamera(vrCam);
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount <= 1)
        {
            PhotonNetwork.LeaveRoom(); // 현재 룸을 나감
        }
    }
    public override void OnLeftRoom()
    {
    }
    public void GoToNextGameScene(string sceneName)
    {
        // 마스터 클라이언트만 씬 로드
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(sceneName);
        }
    }
}