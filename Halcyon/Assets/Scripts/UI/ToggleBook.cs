using UnityEngine;
using System.Collections;

public class ToggleBook : MonoBehaviour
{
    [SerializeField] private GameObject handMenuBook;
    [SerializeField] private GameObject book;
    private Animator bookAnimator;
    [SerializeField] private bool isCurrentlyActive = false;


    void Awake()
    {
        if (book != null)
            bookAnimator = book.GetComponent<Animator>();
    }

    public void ToggleBookActiveState()
    {
        isCurrentlyActive = !isCurrentlyActive;
        if (isCurrentlyActive)
        {
            handMenuBook.SetActive(true);

        }
        else
        {
            if (bookAnimator != null)
                bookAnimator.SetTrigger("BookClosing");
        }
    }
}
