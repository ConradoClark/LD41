using System;
using System.Collections.Generic;
using UnityEngine;

public static class MkLerp
{
    public static Func<IEnumerator<MakineryGear>> LerpFloat(Action<float> setter,
        Func<float> getter, float seconds, Func<float> target, MkEasing.EasingFunction function, float timeScale = 1f)
    {
        return () => MkLerpFloat(setter, getter, seconds, target(), function, timeScale);
    }

    public static Func<IEnumerator<MakineryGear>> LerpInt(Action<int> setter,
        Func<int> getter, float seconds, Func<int> target, MkEasing.EasingFunction function, float timeScale = 1f)
    {
        return () => MkLerpInt(setter, getter, seconds, target(), function, timeScale);
    }

    private static IEnumerator<MakineryGear> MkLerpFloat(Action<float> setter, 
        Func<float> getter, float seconds, float target, MkEasing.EasingFunction function, float timeScale)
    {
        float initialTarget = target;
        float initialStart = getter();
        float start = initialStart;
        if (seconds > 0)
        {
            float lastAcc = 0;
            float last = start;
            float time = 0f;
            float prop = 1 / seconds;
            while (time < seconds)
            {
                var pos = getter();
                lastAcc += pos - last;
                target = initialTarget + lastAcc;
                start = initialStart + lastAcc;                
                last = MkEasing.Interpolate(time * prop, function);
                last = Mathf.Lerp(start, target, last);                
                setter(last);
                time += Time.deltaTime * timeScale;
                yield return new WaitForFrameCountGear();
            }
        }
        setter(target);
    }

    private static IEnumerator<MakineryGear> MkLerpInt(Action<int> setter,
        Func<int> getter, float seconds, int target, MkEasing.EasingFunction function, float timeScale)
    {
        int initialTarget = target;
        int initialStart = getter();
        int start = initialStart;
        if (seconds > 0)
        {
            int lastAcc = 0;
            int last = start;
            float time = 0f;
            float prop = 1 / seconds;
            while (time < seconds)
            {
                var pos = getter();
                lastAcc += pos - last;
                target = initialTarget + lastAcc;
                start = initialStart + lastAcc;
                float fLast = MkEasing.Interpolate(time * prop, function);
                last = Mathf.RoundToInt(Mathf.Lerp(start, target, fLast));
                setter(last);
                time += Time.deltaTime * timeScale;
                yield return new WaitForFrameCountGear();
            }
        }
        setter(target);
    }

}
