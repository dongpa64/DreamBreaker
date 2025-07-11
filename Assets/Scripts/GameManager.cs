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
        // �� ��(Perspective)�� �ε�Ǿ��� �� �÷��̾� ����
        // �̹� �뿡 ������ ���¶�� �ٷ� ����, �ƴ϶��(��: ������ ��) ���
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            SpawnPlayer();
        }

    }
    void SpawnPlayer()
    {
        // PhotonNetwork.LocalPlayer.ActorNumber�� 1���� ����
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        Vector3 spawnPosition;
        if (spawnPoints != null && spawnPoints.Length > 0 && playerIndex < spawnPoints.Length)
        {
            spawnPosition = spawnPoints[playerIndex].position;
        }
        else
        {
            // ���� ����Ʈ�� ���ų� ������ ��� �⺻ ��ġ ���
            spawnPosition = new Vector3(Random.Range(-5f, 5f), 1f, Random.Range(-5f, 5f));
        }
        
        // PhotonNetwork.Instantiate�� ����Ͽ� ��Ʈ��ũ �÷��̾� ������Ʈ ����
        var obj = PhotonNetwork.Instantiate("Player", spawnPosition, Quaternion.identity);
        var vrCam = obj.transform.Find("OVRCameraRig/TrackingSpace/CenterEyeAnchor");
        scaler.SetVrCamera(vrCam);
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount <= 1)
        {
            PhotonNetwork.LeaveRoom(); // ���� ���� ����
        }
    }
    public override void OnLeftRoom()
    {
    }
    public void GoToNextGameScene(string sceneName)
    {
        // ������ Ŭ���̾�Ʈ�� �� �ε�
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(sceneName);
        }
    }
}