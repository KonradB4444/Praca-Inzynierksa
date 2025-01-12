using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    [field: Header("Ground Status")]
    [field: SerializeField] public bool isGrounded { get; private set; } = false;

    [Header("Debug")]
    [SerializeField] private bool enableDebug = false;

    void Update()
    {
        PerformGroundCheck();
    }

    private void PerformGroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);

        if (enableDebug)
        {
            DebugGroundCheck();
        }
    }

    private void DebugGroundCheck()
    {
        Color debugColor = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(groundCheckPoint.position, Vector3.down * groundCheckRadius, debugColor);
    }
}
