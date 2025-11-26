using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{

    // Door state
    private bool isOpen = false;

    // Triggers needed to open/close door
    public List<CellTrigger> linkedTriggers = new List<CellTrigger>(); // Link triggers through door instead of triggers linking to door?

    // Check if all linked triggers are active:


    public void OpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;
            gameObject.SetActive(false);
        }
    }
    public void CloseDoor()
    {
        if (isOpen)
        {
            isOpen = false;
            gameObject.SetActive(true);
        }
    }
}
