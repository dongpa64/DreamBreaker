using UnityEngine;
using Photon.Pun;

public class PlayerCharacter : MonoBehaviourPun
{
    [SerializeField] GameObject cameraRig;
    private void Awake()
    {       
        if(photonView.IsMine)
        {
            cameraRig.gameObject.SetActive(true);
        }

    }
}