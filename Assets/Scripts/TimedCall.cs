using System;
using System.Collections;
using UnityEngine;

public static class TimedCall
{
    private class Runner : MonoBehaviour { }

    private static Runner runner;

    private static Runner GetRunner()
    {
        if (runner == null)
        {
            GameObject obj = new GameObject("Timer");
            UnityEngine.Object.DontDestroyOnLoad(obj);
            runner = obj.AddComponent<Runner>();
        }

        return runner;
    }

    public static void Temporary(Action start, Action end, float seconds)
    {
        GetRunner().StartCoroutine(Routine(start, end, seconds));
    }

    private static IEnumerator Routine(Action start, Action end, float seconds)
    {
        start?.Invoke();

        yield return new WaitForSeconds(seconds);

        end?.Invoke();
    }
}