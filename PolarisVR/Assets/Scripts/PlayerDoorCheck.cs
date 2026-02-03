using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDoorCheck : MonoBehaviour
{
    public bool IsPlayerInsideDoor = false;

    private int doorCount = 0;

    public void EnterDoor()
    {
        doorCount++;
        IsPlayerInsideDoor = true;
    }

    public void ExitDoor()
    {
        doorCount = Mathf.Max(0, doorCount - 1);
        IsPlayerInsideDoor = doorCount > 0;


    }
}