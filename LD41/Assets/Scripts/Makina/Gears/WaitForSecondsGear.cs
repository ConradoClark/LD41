using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class WaitForSecondsGear : MakineryGear
{
    private float _seconds;
    private float _currentTime;
    private float _timeScale;

    public WaitForSecondsGear(float seconds, float timeScale = 1f)
    {
        _seconds = seconds;
        _currentTime = 0f;
        _timeScale = timeScale;
    }
    public override object Current
    {
        get
        {
            return _currentTime;
        }
    }

    public override bool MoveNext()
    {
        _currentTime += Time.deltaTime * _timeScale;
        return _currentTime < _seconds;
    }

    public override void Reset()
    {
        _currentTime = 0f;
    }
}
