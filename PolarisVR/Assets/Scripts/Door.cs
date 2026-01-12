using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{

    // Door state
    private bool isOpen = false;

    // Triggers needed to open/close door
    public List<CellTrigger> linkedTriggers = new List<CellTrigger>(); 

    // Door collider
    private BoxCollider doorCollider;

    // Door panels
    public GameObject leftDoorPanel;
    public GameObject rightDoorPanel;
    public GameObject bottomDoorPanel;


    private Vector3 leftPanelStartPos;
    private Vector3 rightPanelStartPos;
    private Vector3 bottomPanelStartPos;

    public float openSpeed = 2.0f;

    void Start()
    {
        // Store initial positions
        leftPanelStartPos = leftDoorPanel.transform.position;
        rightPanelStartPos = rightDoorPanel.transform.position;
        bottomPanelStartPos = bottomDoorPanel.transform.position;

        // Get door collider
        doorCollider = GetComponent<BoxCollider>();
    }

    void Update()
    {
        CheckTriggers();
        AnimateDoor();
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
            if (doorCollider != null)
                doorCollider.enabled = false;
        }
    }
    public void CloseDoor()
    {
        if (isOpen)
        {
            isOpen = false;
            if (doorCollider != null)
                doorCollider.enabled = true;
        }
    }

    void AnimateDoor()
    {
        // Target panel positions
        Vector3 leftTargetPos = leftPanelStartPos + new Vector3(-1.0f, 0.5f, 0);
        Vector3 rightTargetPos = rightPanelStartPos + new Vector3(1.0f, 0.5f, 0);
        Vector3 bottomTargetPos = bottomPanelStartPos + new Vector3(0, -1.0f, 0);

        // Smoothly move panels
        if (isOpen)
        {
            leftDoorPanel.transform.position = Vector3.Lerp(leftDoorPanel.transform.position, leftTargetPos, Time.deltaTime * openSpeed);
            rightDoorPanel.transform.position = Vector3.Lerp(rightDoorPanel.transform.position, rightTargetPos, Time.deltaTime * openSpeed);
            bottomDoorPanel.transform.position = Vector3.Lerp(bottomDoorPanel.transform.position, bottomTargetPos, Time.deltaTime * openSpeed);
        }
        else
        {
            leftDoorPanel.transform.position = Vector3.Lerp(leftDoorPanel.transform.position, leftPanelStartPos, Time.deltaTime * openSpeed);
            rightDoorPanel.transform.position = Vector3.Lerp(rightDoorPanel.transform.position, rightPanelStartPos, Time.deltaTime * openSpeed);
            bottomDoorPanel.transform.position = Vector3.Lerp(bottomDoorPanel.transform.position, bottomPanelStartPos, Time.deltaTime * openSpeed);
        }
    }

}
