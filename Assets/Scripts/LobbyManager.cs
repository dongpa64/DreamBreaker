using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public InputField nicknameInput;  // �г��� �Է¶�
    public Button joinButton;         // ���� ��ư
    public Text statusText;           // �ȳ� �޽���

    void Start()
    {
        statusText.text = "Photon ���� ��...";
        joinButton.interactable = false;
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Photon ���� ���� ����
    public override void OnConnectedToMaster()
    {
        statusText.text = "Photon ���� ���� �Ϸ�!";
        joinButton.interactable = true;
    }

    // ���� ���� ����
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (statusText != null)
            statusText.text = $"���� ����: {cause}";
        joinButton.interactable = false;
    }

    // ���� ��ư Ŭ�� �� ȣ��
    public void OnClickJoin()
    {
        string nickname = nicknameInput.text.Trim();

        if (nickname.Length == 0)
        {
            statusText.text = "�г����� �Է��ϼ���.";
            return;
        }

        PhotonNetwork.NickName = nickname;
        joinButton.interactable = false;
        statusText.text = "�� �ڵ� ��Ī ��...";
        PhotonNetwork.JoinRandomRoom(); // �� ���� ������ ����, ������ �� �� ����
    }

    // �� ���� ���� �� �� �� ����
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        statusText.text = "�� �� ����, �� �� ���� ��...";
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(null, options);
    }

    // �뿡 ���� ����
    public override void OnJoinedRoom()
    {
        statusText.text = $"�� ���� ����! ({PhotonNetwork.CurrentRoom.Name})\n���� ���� ��� ��...";
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            photonView.RPC("GotoTutorial", RpcTarget.All);
    }

    // �ٸ� �÷��̾ �������� ��(2�� ���� ��)
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        statusText.text = "�� ��° �÷��̾� ����!\nƩ�丮�� �� �̵�!";
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            photonView.RPC("GotoTutorial", RpcTarget.All);
    }

    [PunRPC]
    void GotoTutorial()
    {
        if (statusText != null)
            statusText.text = "Ʃ�丮�� �� �̵�!";
        PhotonNetwork.LoadLevel("TutorialMapScene");
    }
}
