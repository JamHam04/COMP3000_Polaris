using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{

    // Door state
    private bool isOpen = false;



   public void OpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;
            gameObject.SetActive(false);
            Debug.Log("Door opened");
        }
    }
    public void CloseDoor()
    {
        if (isOpen)
        {
            isOpen = false;
            gameObject.SetActive(true);
            Debug.Log("Door closed");
        }
    }
}
