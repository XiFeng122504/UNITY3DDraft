using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //  主角，需要跟随的角色
    public Transform target;

    //  相对位置偏移
    public Vector3 offset;

    //  平滑速度
    public float smoothSpeed = 0.125f;


    /// <summary>
    /// 每一帧执行一次，但是在update后执行
    /// </summary>
    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;

        Vector3 smoothPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothPosition;

        transform.LookAt(target);

    }

}
