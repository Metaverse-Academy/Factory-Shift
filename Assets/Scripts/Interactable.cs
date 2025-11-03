using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    [SerializeField] private string prompt = "Press [E] to interact";

    public string GetPrompt() => prompt;

    public void Interact(GameObject interactor)
    {
        Debug.Log($"{name} was interacted with by {interactor.name}");
    }
}
