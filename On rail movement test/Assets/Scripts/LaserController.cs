using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserController : MonoBehaviour
{
    private LineRenderer lr;
    private float widthAtStart;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        widthAtStart = lr.startWidth;
    }

    void Update()
    {
        lr.startWidth -= widthAtStart / (1.0f / Time.deltaTime);
        lr.endWidth -= widthAtStart / (1.0f / Time.deltaTime);

        if(lr.startWidth <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
