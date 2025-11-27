using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ControllerButtonTrigger : MonoBehaviour
{
    public Button targetButton; // اسحب الزر هنا من Inspector

    void Update()
    {
        // زر X في يد الـ Xbox غالبًا هو buttonSouth
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            targetButton.onClick.Invoke();
        }
    }
}