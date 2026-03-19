using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField]private float moveSpeed = 1f;
    private Rigidbody rb;
    private Vector2 InputVector;
    private Transform camTransform;

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (Camera.main != null) camTransform = Camera.main.transform;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        MovePlayer();
    }
    /// <summary>
    /// 处理玩家移动输入
    /// </summary>
    public void onMove(InputAction.CallbackContext context)
    {
        InputVector = context.ReadValue<Vector2>();
    }
    /// <summary>
    /// 根据相机的方向确定Player移动方向
    /// </summary>
    private void MovePlayer()
    {
        Vector3 forward = camTransform.forward;
        Vector3 right = camTransform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        Vector3 moveDirection = forward * InputVector.y + right * InputVector.x;
        rb.velocity = moveDirection * moveSpeed;
    }
}