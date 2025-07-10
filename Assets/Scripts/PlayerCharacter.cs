using UnityEngine;
using Photon.Pun;

public class PlayerCharacter : MonoBehaviourPun
{
    
    private void Awake()
    {
        var camera = transform.Find("XR Origin");
        if(!photonView.IsMine)
            camera.gameObject.SetActive(false);

    }
}