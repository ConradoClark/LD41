using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GoalProgress
{
    public string GoalName { get; private set; }
    public string Progress { get; private set; }
    public bool Completed { get; private set; }

    public GoalProgress(string goalName, string progress, bool completed)
    {
        GoalName = goalName;
        Progress = progress;
        Completed = completed;
    }
}
