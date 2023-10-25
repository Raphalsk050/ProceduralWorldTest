using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class ThreadTest : MonoBehaviour
{
    private Queue<TestThreadInfo<float>> _testThreadInfoQueue = new();
    private Action<float> _testCallback;

    private void Start()
    {
        _testCallback = delegate { };
        
    }

    //sended to another thread to compute 
    public void ResquestThreadTestInfo(Action<float> callback, float initialTime)
    {
        ThreadStart threadOneStart = delegate
        {
            TestInfoThread(callback, initialTime);
        };
        new Thread(threadOneStart).Start();

        /*ThreadStart threadTwoStart = delegate
        {
            TestInfoThreadTwo(callback);
        };
        new Thread(threadTwoStart).Start();*/
    }

    public void TestInfoThread(Action<float> callback, float currentTime)
    {
        float counter = 0;
        float initialTime = currentTime;
        for (int i = 0; i < 10000000; i++)
        {
            counter = Mathf.Sqrt(counter)*Mathf.Pow(Mathf.PI, counter);
        }

        lock (_testThreadInfoQueue)
        {
            _testThreadInfoQueue.Enqueue(new TestThreadInfo<float>(callback,initialTime));
        }
    }

    public void TestInfoThreadTwo(Action<float> callback)
    {
        float counter = 0;
        float totalTime = 0;
        for (int i = 0; i < 10000000; i++)
        {
            counter = Mathf.Sqrt(counter) * Mathf.Pow(Mathf.PI, counter);
            totalTime = 1;
        }

        lock (_testThreadInfoQueue)
        {
            _testThreadInfoQueue.Enqueue(new TestThreadInfo<float>(callback, totalTime));
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResquestThreadTestInfo(_testCallback, Time.time);
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            float counter = 0;
            float initialTime = Time.time;
            for (int i = 0; i < 10000000; i++)
            {
                counter = Mathf.Sqrt(counter) * Mathf.Pow(Mathf.PI, counter);
            }
            Debug.Log(Time.time - initialTime);
        }

        //wait until the specific thread finish work
        if (_testThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < _testThreadInfoQueue.Count; i++)
            {
                TestThreadInfo<float> threadInfo = _testThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
                float totalTime = Time.time - threadInfo.parameter;
                Debug.Log(totalTime);
            }
        }
    }
}

struct TestThreadInfo<T>
{
    public readonly Action<T> callback;
    public readonly T parameter;

    public TestThreadInfo(Action<T> callback, T parameter)
    {
        this.callback = callback;
        this.parameter = parameter;
    }
}
