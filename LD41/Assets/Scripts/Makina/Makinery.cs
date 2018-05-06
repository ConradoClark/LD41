using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Makinery
{
    public int Priority { get; private set; }
    public int AssignedPriority { get; set; }
    public string QueueName;
    private bool _active;
    public bool Looping;
    private List<Func<IEnumerator<MakineryGear>>> _activeRoutines;
    private List<Func<IEnumerator<MakineryGear>>> _routines;
    private Func<IEnumerator<MakineryGear>> _currentRoutine;
    private IEnumerator<MakineryGear> _currentFunction;
    private MakineryGear _currentGear;
    private bool _calledOnEnd;
    public event EventHandler<EventArgs> OnEnd;
    public event EventHandler<EventArgs> OnLoopEnd;
    public event EventHandler<EventArgs> OnQueued;

    public Makinery(int priority, bool active = true)
    {
        Priority = priority;
        _routines = new List<Func<IEnumerator<MakineryGear>>>();
        _activeRoutines = new List<Func<IEnumerator<MakineryGear>>>();
        _active = active;
    }

    public void AddRoutine(params Func<IEnumerator<MakineryGear>>[] functions)
    {
        foreach (var function in functions)
        {
            _routines.Add(function);
            _activeRoutines.Add(function);
            if (_currentRoutine == null)
            {
                _currentRoutine = function;
            }
        }
    }

    public MakineryGear Step()
    {
        if (_currentGear != null)
        {
            if (_currentGear.MoveNext())
            {
                return _currentGear;
            }
            _currentGear = null;
        }
        if (_currentRoutine == null)
        {
            if (!_calledOnEnd)
            {
                _calledOnEnd = true;
                if (OnEnd != null)
                {
                    OnEnd.Invoke(this, new EventArgs());
                }
            }
            return null;
        }
        if (!_active)
        {
            return MakinaStatic.InactiveGear;
        }
        if (_currentFunction == null)
        {
            _currentFunction = _currentRoutine();
        }
        if (!_currentFunction.MoveNext())
        {
            _activeRoutines.Remove(_currentRoutine);
            _currentRoutine = _activeRoutines.FirstOrDefault();
            if (_currentRoutine != null)
            {
                _currentFunction = _currentRoutine();
                return MakinaStatic.EndRoutineGear;
            }
        }

        if (_activeRoutines.Count == 0 && Looping)
        {
            _activeRoutines.AddRange(_routines);
            _currentRoutine = _activeRoutines.FirstOrDefault();
            _currentFunction = _currentRoutine();
            if (OnLoopEnd != null)
            {
                OnLoopEnd.Invoke(this, new EventArgs());
            }
            return MakinaStatic.LoopStepGear;
        }

        _currentGear = _currentFunction.Current;
        return _currentGear;
    }

    public void Pause()
    {
        _active = false;
    }

    public void Play()
    {
        _active = true;
    }

    public void TogglePlay()
    {
        _active = !_active;
    }

    public void Reset()
    {
        _activeRoutines.Clear();
        _activeRoutines.AddRange(_routines);
        _currentRoutine = _activeRoutines.FirstOrDefault();
        _currentFunction = null;
        _currentGear = null;
        _calledOnEnd = false;
    }

    public void FireQueued()
    {
        if (OnQueued != null)
        {
            OnQueued.Invoke(this, new EventArgs());
        }
    }
}
