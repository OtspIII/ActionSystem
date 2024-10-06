using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //A class meant to be put on the camera.
    //Makes the camera follow the target. A pretty crappy camera script--you should make a better one.
    
    public GameObject Target;
    
    void Update()
    {
        if (Target == null) return;
        Vector3 where = Target.transform.position;
        where.z = -1;
        transform.position = where;
    }
}
