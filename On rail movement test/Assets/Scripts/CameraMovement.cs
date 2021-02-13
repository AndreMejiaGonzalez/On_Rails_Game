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
    private Vector3 limits;

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

        transform.localPosition = new Vector3(Mathf.Clamp(localPos.x, -limits.x, limits.x), Mathf.Clamp(localPos.y, -limits.y, limits.y), Mathf.Clamp(localPos.z, -limits.z, limits.z));
    }

    public void followTarget(Transform target)
    {
        Vector3 localPos = transform.localPosition;
        Vector3 targetLocalPos = target.transform.localPosition;
        transform.localPosition = new Vector3(targetLocalPos.x + offset.x, target.localPosition.y + offset.y, targetLocalPos.z + offset.z);
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
