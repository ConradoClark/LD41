using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefeatAllEnemies : IGoal
{
    private LevelGrid _levelGrid;
    public DefeatAllEnemies(LevelGrid grid)
    {
        _levelGrid = grid;
    }

    public GoalProgress GetGoalProgress()
    {
        var r = _levelGrid.GetActiveGridObjects(GridObject.GridObjectType.Enemy);
        string str = r.Length == 1 ? "1 Enemy Left" : string.Format("{0} Enemies Left", r.Length);
        return new GoalProgress("Defeat all Enemies!", str, r.Length == 0);
    }
}
