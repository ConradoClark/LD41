using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardUI : MonoBehaviour {

    public class OnHelpChangedEventHandler : EventArgs
    {
        public string Title;
        public string Description;
    }
    public TextMeshPro Stats;
    public TextMeshPro HelpTitle;
    public TextMeshPro HelpDescription;
    public TextMeshPro DiscardCount;
    public TextMeshPro CardCount;
    private List<PawSlot> PawSlots = new List<PawSlot>();
    public event EventHandler<OnHelpChangedEventHandler> OnHelpChanged;
    public event EventHandler<EventArgs> OnHelpReset;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void AddPawSlot(PawSlot slot)
    {
        PawSlots.Add(slot);
        PawSlots.Sort(new Comparison<PawSlot>((p, p2) => p.Order.CompareTo(p2.Order)));
    }

    public PawSlot[] GetPawSlots()
    {
        return PawSlots.ToArray();
    }
    
    public void UpdateHelp(string title, string description)
    {
        if (OnHelpChanged == null) return;

        OnHelpChanged.Invoke(this, new OnHelpChangedEventHandler()
        {
            Title = title,
            Description = description
        });
    }

    public void ResetHelp()
    {
        if (OnHelpReset == null) return;
        OnHelpReset.Invoke(this, new EventArgs());
    }
}
