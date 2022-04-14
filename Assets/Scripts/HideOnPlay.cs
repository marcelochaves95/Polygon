using UnityEngine;

public class HideOnPlay : MonoBehaviour
{
    private void Start()
    {
        gameObject.SetActive(false);
    }
}
