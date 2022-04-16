using UnityEngine;
using UnityEditor;
using System;

public class UpdatableData : ScriptableObject
{
    public bool AutoUpdate;
    public event Action OnValuesUpdated;

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        if (AutoUpdate)
        {
            EditorApplication.update += NotifyOfUpdatedValues;
        }
    }

    public void NotifyOfUpdatedValues()
    {
        EditorApplication.update -= NotifyOfUpdatedValues;
        OnValuesUpdated?.Invoke();
    }
#endif
}
