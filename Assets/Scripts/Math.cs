using UnityEngine;

public class Math : MonoBehaviour {

	public static Vector3 CrossProduct (Vector3 v1, Vector3 v2) {
        float x, y, z;
        x = v1.y * v2.z - v2.y * v1.z;
        y = (v1.x * v2.z - v2.x * v1.z) * -1;
        z = v1.x * v2.y - v2.x * v1.y;
        Vector3 cross = new Vector3(x, y, z);
        return cross;
    }
}
