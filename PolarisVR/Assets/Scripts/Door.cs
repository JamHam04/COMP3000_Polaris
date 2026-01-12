using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{

    // Door state
    private bool isOpen = false;

    // Triggers needed to open/close door
    public List<CellTrigger> linkedTriggers = new List<CellTrigger>(); 

    void Update()
    {
        CheckTriggers();
    }

    // Check if all linked triggers are active
    public void CheckTriggers()
    {
        foreach (CellTrigger trigger in linkedTriggers)
        {
            if (!trigger.IsTriggerActive)
            {
                CloseDoor();
                return;
            }
        }
        OpenDoor();
    }

    public void OpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;
            //gameObject.SetActive(false);
            //doorCollider.enabled = false;
            // Animate door opening

        }
    }
    public void CloseDoor()
    {
        if (isOpen)
        {
            isOpen = false;
            //doorCollider.enabled = true;
            //gameObject.SetActive(true);
        }
    }
}
