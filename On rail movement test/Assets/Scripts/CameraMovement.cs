using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    [SerializeField]
    private Vector3 offset = Vector3.zero;
    [SerializeField]
    private Vector2 limits = new Vector2(5,3);
    [SerializeField]
    [Range(0,1)]
    private float smoothTime;
    [SerializeField]
    private Vector3 velocity = Vector3.zero;

    void Update()
    {
        if(!Application.isPlaying)
        {
            transform.localPosition = offset;
        }

        followTarget(target);
    }

    void LateUpdate()
    {
        Vector3 localPos = transform.localPosition;

        transform.localPosition = new Vector3(Mathf.Clamp(localPos.x, -limits.x, limits.x), Mathf.Clamp(localPos.y, -limits.y, limits.y), localPos.z);
    }

    public void followTarget(Transform target)
    {
        Vector3 localPos = transform.localPosition;
        Vector3 targetLocalPos = target.transform.localPosition;
        transform.localPosition = Vector3.SmoothDamp(localPos, new Vector3(targetLocalPos.x + offset.x, targetLocalPos.y + offset.y, localPos.z), ref velocity, smoothTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(-limits.x, -limits.y, transform.position.z), new Vector3(limits.x, -limits.y, transform.position.z));
        Gizmos.DrawLine(new Vector3(-limits.x, limits.y, transform.position.z), new Vector3(limits.x, limits.y, transform.position.z));
        Gizmos.DrawLine(new Vector3(-limits.x, -limits.y, transform.position.z), new Vector3(-limits.x, limits.y, transform.position.z));
        Gizmos.DrawLine(new Vector3(limits.x, -limits.y, transform.position.z), new Vector3(limits.x, limits.y, transform.position.z));
    }
}
