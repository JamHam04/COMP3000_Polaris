using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollider : MonoBehaviour
{

    public Transform headsetPosition;

    // Collider Settings
    public float padding = 0.3f;
    public float minHeight = 0.5f;
    public float maxHeight = 1.7f;
    public float radiusFactor = 0.25f;



    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        if (headsetPosition == null && Camera.main != null)
        {
            headsetPosition = Camera.main.transform;
        }

    }


    void FixedUpdate()
    {
        if (headsetPosition == null) return;
        UpdateCollider();
    }

    void UpdateCollider()
    {
        // Y position of headset
        float headsetY = headsetPosition.position.y - transform.position.y;
        float clampedY = Mathf.Clamp(headsetY + padding, minHeight, maxHeight);

        characterController.height = clampedY; // Set height
        characterController.radius = clampedY * radiusFactor; // Set radius

        Vector3 localHeadOffset = transform.InverseTransformPoint(headsetPosition.position);


        // Center calculation
        characterController.center = new Vector3(localHeadOffset.x, clampedY * 0.5f, localHeadOffset.z); // Set center

        // Limit headset Y position
        Vector3 localHeadsetPosition = headsetPosition.localPosition;
        localHeadsetPosition.y = Mathf.Min(localHeadsetPosition.y, maxHeight - padding);
        headsetPosition.localPosition = localHeadsetPosition;



    }
}
