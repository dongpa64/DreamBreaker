using UnityEngine;

public enum PlayerForm
{
    None,
    Penguin
}

public class PlayerFormManager : MonoBehaviour
{
    public PlayerForm currentForm = PlayerForm.None;

    public GameObject penguinModel;

    public GameObject xrCameraRig;          // OVRCameraRig
    public Camera thirdPersonCam;           // Third-person camera
    public FollowTarget camFollower;        // FollowTarget 스크립트 (optional)

    void Start()
    {
        SetForm(currentForm);
    }

    public void ChangeForm(PlayerForm newForm)
    {
        currentForm = newForm;
        SetForm(newForm);
    }

    private void SetForm(PlayerForm form)
    {
        bool isPenguin = form == PlayerForm.Penguin;

        if (penguinModel != null)
            penguinModel.SetActive(isPenguin);

        SwitchCamera(isPenguin);
    }

    private void SwitchCamera(bool toThirdPerson)
    {
        // XR Origin 끄지 말기!
        // xrCameraRig.SetActive(!toThirdPerson); ❌ 제거
        thirdPersonCam.gameObject.SetActive(toThirdPerson);

        if (toThirdPerson && camFollower != null)
        {
            camFollower.target = penguinModel.transform;
        }

        // 추가: XR 카메라 비활성화 (카메라만!)
        Camera xrCam = xrCameraRig.GetComponentInChildren<Camera>(true);
        if (xrCam != null)
            xrCam.enabled = !toThirdPerson;

        Debug.Log("Camera switched. 3rd person: " + toThirdPerson);
    }
}

