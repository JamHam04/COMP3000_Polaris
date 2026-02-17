using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollider : MonoBehaviour
{

    public Transform headsetPosition;

    // Collider Settings
    public float padding = 0.3f;
    public float minHeight = 0.5f;
    public float maxHeight = 2.2f;
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

        // Height calculation
        float height = Mathf.Clamp(headsetY + padding, minHeight, maxHeight); // Clamp to prevent going too low/high

        characterController.height = height; // Set height
        characterController.radius = height * radiusFactor; // Set radius

        // Center calculation
        Vector3 localHeadOffset = transform.InverseTransformPoint(headsetPosition.position);

        characterController.center = new Vector3(localHeadOffset.x, height * 0.5f, localHeadOffset.z); // Set center
    }
}
