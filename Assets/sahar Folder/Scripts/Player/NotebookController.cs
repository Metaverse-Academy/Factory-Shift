using UnityEngine;
using UnityEngine.InputSystem;

public class NotebookController : MonoBehaviour
{
    [Header("Pages (بالترتيب)")]
    public GameObject[] pages;
      [SerializeField] private InputActionReference nextPageAction; // زر التالي
    [SerializeField] private InputActionReference previousPageAction; // زر السابق

    private int currentPage = 0;

    private void OnEnable()
    {
        if (nextPageAction != null)
        {
            nextPageAction.action.Enable();
            nextPageAction.action.performed += ctx => OnNextPage();
        }

        if (previousPageAction != null)
        {
            previousPageAction.action.Enable();
            previousPageAction.action.performed += ctx => OnPreviousPage();
        }
    }

    void OnDisable()
    {
        if (nextPageAction != null)
        {
            nextPageAction.action.performed -= ctx => OnNextPage();
            nextPageAction.action.Disable();
        }
        if (previousPageAction != null)
        {
            previousPageAction.action.performed -= ctx => OnPreviousPage();
            previousPageAction.action.Disable();
        }
    }

    void Start()
    {
        ShowOnly(currentPage);
    }

    // هذه الدوال يستدعيها PlayerInput
    public void OnNextPage()
    {
        if (currentPage < pages.Length - 1)
        {
            currentPage++;
            ShowOnly(currentPage);
        }
    }

    public void OnPreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            ShowOnly(currentPage);
        }
    }

    void ShowOnly(int index)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == index);
        }
    }
}