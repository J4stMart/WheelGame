using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 10f;
    public Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void FixedUpdate()
    {
        Vector3 targetPosition = target.position + offset;
        float distance = Vector3.Distance(targetPosition, transform.position);

        transform.position = Vector3.Lerp(transform.position, target.position + offset, (distance / 5) * smoothSpeed * Time.deltaTime);
    }
}
