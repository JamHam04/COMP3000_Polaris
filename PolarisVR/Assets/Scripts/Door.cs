using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{

    // Door state
    private bool isOpen = false;

    // Triggers needed to open/close door
    public List<CellTrigger> linkedBlueTriggers;
    public List<CellTrigger> linkedRedTriggers;
    public List<CellTrigger> linkedPurpleTriggers;


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


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerDoorCheck doorCheck = other.GetComponent<PlayerDoorCheck>();
            if (doorCheck != null)
            {
                doorCheck.EnterDoor();
            }
        }

    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerDoorCheck doorCheck = other.GetComponent<PlayerDoorCheck>();
            if (doorCheck != null)
            {
                doorCheck.ExitDoor();
            }
        }
    }

    void Start()
    {
        // Store initial positions
        leftPanelStartPos = leftDoorPanel.transform.localPosition;
        rightPanelStartPos = rightDoorPanel.transform.localPosition;
        bottomPanelStartPos = bottomDoorPanel.transform.localPosition;

        // Get door collider
        doorCollider = GetComponent<BoxCollider>();

        // Show indicators based on how many linked triggers
        
    }

    void Update()
    {
        CheckTriggers();
        AnimateDoor();
        UpdateIndicatorVisibility();
    }

    // Check if all linked triggers are active
    public void CheckTriggers()
    {
        foreach (CellTrigger trigger in linkedBlueTriggers)
        {
            if (!trigger.IsTriggerActive)
            {
                CloseDoor();
                return;
            }
        }

        foreach (CellTrigger trigger in linkedRedTriggers)
        {
            if (!trigger.IsTriggerActive)
            {
                CloseDoor();
                return;
            }
        }
        foreach (CellTrigger trigger in linkedPurpleTriggers)
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
        Vector3 leftTargetPos = leftPanelStartPos + new Vector3(-2.0f, 1.0f, 0);
        Vector3 rightTargetPos = rightPanelStartPos + new Vector3(2.0f, 1.0f, 0);
        Vector3 bottomTargetPos = bottomPanelStartPos + new Vector3(0, -2.0f, 0);

        // Smoothly move panels
        if (isOpen)
        {
            leftDoorPanel.transform.localPosition = Vector3.Lerp(leftDoorPanel.transform.localPosition, leftTargetPos, Time.deltaTime * openSpeed);
            rightDoorPanel.transform.localPosition = Vector3.Lerp(rightDoorPanel.transform.localPosition, rightTargetPos, Time.deltaTime * openSpeed);
            bottomDoorPanel.transform.localPosition = Vector3.Lerp(bottomDoorPanel.transform.localPosition, bottomTargetPos, Time.deltaTime * openSpeed);
        }
        else
        {
            leftDoorPanel.transform.localPosition = Vector3.Lerp(leftDoorPanel.transform.localPosition, leftPanelStartPos, Time.deltaTime * openSpeed);
            rightDoorPanel.transform.localPosition = Vector3.Lerp(rightDoorPanel.transform.localPosition, rightPanelStartPos, Time.deltaTime * openSpeed);
            bottomDoorPanel.transform.localPosition = Vector3.Lerp(bottomDoorPanel.transform.localPosition, bottomPanelStartPos, Time.deltaTime * openSpeed);
        }
    }

    void UpdateIndicatorVisibility()
    {
        // Blue indicators on left panel
        EnableIndicators(leftDoorPanel, linkedBlueTriggers);

        // Red indicators on right panel
        EnableIndicators(rightDoorPanel, linkedRedTriggers);

        // Purple indicators on bottom panel
        EnableIndicators(bottomDoorPanel, linkedPurpleTriggers);
    }

    void EnableIndicators(GameObject panel, List<CellTrigger> triggers)
    {
        if (panel == null) return;

        // Get all indicators
        Transform[] indicators = new Transform[panel.transform.childCount]; // Array of indicators
        for (int i = 0; i < panel.transform.childCount; i++)
            indicators[i] = panel.transform.GetChild(i); // Indicators children of panels

        // Disable all indicators initially
        foreach (Transform indicator in indicators)
            indicator.gameObject.SetActive(false);

        // Enable indicators based on how many triggers are connected to the door
        int count = Mathf.Min(triggers.Count, indicators.Length); // Number of indicators to enable
        for (int i = 0; i < count; i++)
        {
            indicators[i].gameObject.SetActive(true); // Enable indicator

            // Set emission based on if trigger active or not
            Renderer indicatorRenderer = indicators[i].GetComponent<Renderer>();
            if (indicatorRenderer != null)
            {
                if (triggers[i].IsTriggerActive)
                {
                    indicatorRenderer.material.EnableKeyword("_EMISSION");
                    indicatorRenderer.material.SetColor("_EmissionColor", Color.white * 0.5f);
                }
                else
                    indicatorRenderer.material.DisableKeyword("_EMISSION");
            }
        }
    }



}
