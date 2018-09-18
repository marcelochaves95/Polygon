using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudManager : MonoBehaviour {

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
        Gizmos.DrawLine(Vector3.zero, Vector3.forward);
    }
}
