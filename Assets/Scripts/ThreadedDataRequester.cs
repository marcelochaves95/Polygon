using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class ThreadedDataRequester : MonoBehaviour
{
    private static ThreadedDataRequester _instance;
    private readonly Queue<ThreadInfo> _dataQueue = new Queue<ThreadInfo>();

    private void Awake()
    {
        _instance = FindObjectOfType<ThreadedDataRequester>();
    }

    public static void RequestData(Func<object> generateData, Action<object> callback)
    {
        new Thread(DataThread).Start();

        void DataThread()
        {
            _instance.DataThread(generateData, callback);
        }
    }

    private void DataThread(Func<object> generateData, Action<object> callback)
    {
        object data = generateData();
        lock (_dataQueue)
        {
            _dataQueue.Enqueue(new ThreadInfo(callback, data));
        }
    }

    private void Update()
    {
        if (_dataQueue.Count > 0)
        {
            for (int i = 0; i < _dataQueue.Count; i++)
            {
                ThreadInfo threadInfo = _dataQueue.Dequeue();
                threadInfo.Callback(threadInfo.Parameter);
            }
        }
    }
}