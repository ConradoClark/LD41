using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Makina : MonoBehaviour
{
    private Makinery[] Makinarium { get; set; }
    private List<int> ActiveMakineries { get; set; }
    private Dictionary<string, Queue<int>> MakineryQueues { get; set; }
    private List<int> _removeList = new List<int>();
    public int MakinaSize = 5000;

    public enum MakineryStepResult
    {
        Waiting,
        Step,
        Done
    }

    private void Awake()
    {
        Makinarium = new Makinery[MakinaSize];
        ActiveMakineries = new List<int>();
        MakineryQueues = new Dictionary<string, Queue<int>>();
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Makinarium == null) return;
        ActiveMakineries.Sort();
        _removeList.Clear();
        foreach (var makineryIndex in ActiveMakineries.ToList())
        {
            Makinery m = Makinarium[makineryIndex];
            MakineryGear step;
            MakineryStepResult result = RunMakineryStep(m, out step);

            if (result == MakineryStepResult.Done)
            {
                Makinarium[makineryIndex] = null;
                _removeList.Add(makineryIndex);
            }
        }
        ActiveMakineries.RemoveAll(r => _removeList.Contains(r));
    }

    public MakineryStepResult RunMakineryStep(Makinery m, out MakineryGear gearStep)
    {        
        bool hasQueue = m.QueueName != null;

        if (hasQueue)
        {
            AddToMakineryQueue(m);
            if (m.AssignedPriority != MakineryQueues[m.QueueName].Peek())
            {
                gearStep = MakinaStatic.InactiveGear;
                return MakineryStepResult.Waiting;
            }
        }
        gearStep = m.Step();

        if (gearStep == null)
        {
            if (hasQueue)
            {
                MakineryQueues[m.QueueName].Dequeue();
            }
            return MakineryStepResult.Done;
        }

        // Loops back to the end of the queue if looping or inactive
        if (hasQueue)
        {
            if (gearStep == MakinaStatic.InactiveGear)
            {
                MakineryQueues[m.QueueName].Dequeue();
            }
            if (gearStep == MakinaStatic.LoopStepGear)
            {
                MakineryQueues[m.QueueName].Enqueue(m.AssignedPriority);
            }
        }
        return MakineryStepResult.Step;
    }

    public void AddMakinery(params Makinery[] makineries)
    {
        foreach (var makinery in makineries)
        {
            if (makinery.Priority < 1) continue;
            makinery.AssignedPriority = Makinarium.Skip(makinery.Priority - 1).Select((m, ix) => new { m = m, ix = ix }).FirstOrDefault(r => r.m == null).ix + makinery.Priority - 1;
            if (Makinarium[makinery.AssignedPriority] == null)
            {
                Makinarium[makinery.AssignedPriority] = makinery;
                ActiveMakineries.Add(makinery.AssignedPriority);
            }
        }
    }

    private void AddToMakineryQueue(Makinery makinery)
    {
        if (!MakineryQueues.ContainsKey(makinery.QueueName))
        {
            MakineryQueues[makinery.QueueName] = new Queue<int>();
        }
        if (!MakineryQueues[makinery.QueueName].Contains(makinery.AssignedPriority))
        {
            MakineryQueues[makinery.QueueName].Enqueue(makinery.AssignedPriority);
            makinery.FireQueued();
        }        
    }

    public void RemoveMakinery(Makinery makinery)
    {
        if (Makinarium[makinery.AssignedPriority] == makinery)
        {
            Makinarium[makinery.AssignedPriority] = null;
            ActiveMakineries.Remove(makinery.AssignedPriority);
            if (makinery.QueueName != null)
            {
                if (MakineryQueues.ContainsKey(makinery.QueueName))
                {
                    MakineryQueues[makinery.QueueName] = new Queue<int>(MakineryQueues[makinery.QueueName].Except(new[] { makinery.AssignedPriority }));
                }
            }
        }
    }

    public int GetQueuePosition(Makinery makinery)
    {
        if (makinery.QueueName == null) return 0;
        if (!MakineryQueues.ContainsKey(makinery.QueueName)) return -1;
        var queuePos = MakineryQueues[makinery.QueueName].Select((i, ix) => new { i = i, ix = ix })
            .Where(m=>m.i == makinery.AssignedPriority)
            .FirstOrDefault();
        if (queuePos == null) return -2;
        return queuePos.ix;
    }
}
