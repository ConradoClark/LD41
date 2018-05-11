using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using mk = CardGameMakineryConstants;

public class GoalCounter : MonoBehaviour
{
    public PoolInstance GoalIconPoolInstance;
    private LevelGrid _levelGrid;
    private List<GoalIcon> _goals;
    private int _completedGoals;
    private int _numberOfGoals;
    public TextMeshPro GoalNameText;
    public TextMeshPro GoalProgressText;

    // Use this for initialization
    void Start()
    {
        _goals = new List<GoalIcon>();
        _completedGoals = 0;
        if (Toolbox.TryGetLevelGrid(out _levelGrid))
        {
            _completedGoals = _levelGrid.CompletedGoals;
            _numberOfGoals = _levelGrid.NumberOfGoals;
            CreateGoals();
            Makinery adjustGoals = new Makinery(mk.Priority.UIAnimations) { Looping = true };
            adjustGoals.AddRoutine(() => AdjustGoals());
            Toolbox.Instance.MainMakina.AddMakinery(adjustGoals);
        }
    }

    private void Update()
    {
        GoalNameText.text = _numberOfGoals > 1 && _completedGoals != _numberOfGoals ? (_completedGoals + 1) + ". " + _levelGrid.CurrentGoal : _levelGrid.CurrentGoal;
        GoalProgressText.text = _levelGrid.CurrentGoalProgress;
    }

    private void CreateGoals()
    {
        foreach (var goal in _goals)
        {
            Toolbox.Instance.Pool.Release(GoalIconPoolInstance, goal.gameObject);
        }

        for (int i = 0; i < _numberOfGoals; i++)
        {
            float xPos = (_numberOfGoals - 1 - i) * -0.45f;
            var obj = Toolbox.Instance.Pool.Retrieve(GoalIconPoolInstance);
            obj.transform.SetParent(transform);
            obj.transform.localPosition = new Vector3(xPos, 0);
            _goals.Add(obj.GetComponent<GoalIcon>());
        }
    }

    IEnumerator<MakineryGear> AdjustGoals()
    {
        if (_numberOfGoals != _levelGrid.NumberOfGoals)
        {
            _numberOfGoals = _levelGrid.NumberOfGoals;
            CreateGoals();
        }

        if (_levelGrid.CompletedGoals == _completedGoals) yield break;

        for (int i = 0; i < _goals.Count; i++)
        {
            if (_levelGrid.CompletedGoals > i)
            {
                yield return new InnerMakinery(_goals[i].Complete(), Toolbox.Instance.MainMakina);
            }
        }
        _completedGoals = _levelGrid.CompletedGoals;
    }
}
