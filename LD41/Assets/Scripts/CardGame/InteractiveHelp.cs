using Assets.Scripts.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractiveHelp : MonoBehaviour
{

    private CardUI _cardUI;
    private const int HelpTitleDefaultSize = 4;
    private const string HelpPrefix = "HELP";
    private Queue<long> _titleCoroutines = new Queue<long>();
    private Queue<long> _descriptionCoroutines = new Queue<long>();
    private static long coroutineId;
    public string DefaultDescription;

    // Use this for initialization
    void Awake()
    {
        _cardUI = Toolbox.Instance.CardUI;
        bool cardUINotNull = new Validation(_cardUI != null, "InteractiveHelp init failed: CardUI is null.");

        if (cardUINotNull)
        {
            bool cardUITitleAssigned = new Validation(_cardUI.HelpTitle != null, "InteractiveHelp init failed: No HelpTitle assigned in CardUI.");
            bool cardUIDescriptionAssigned = new Validation(_cardUI.HelpDescription != null, "InteractiveHelp init failed: No HelpDescription assigned in CardUI.");
            if (cardUITitleAssigned && cardUIDescriptionAssigned)
            {
                _cardUI.OnHelpChanged += _cardUI_OnHelpChanged;
                _cardUI.OnHelpReset += _cardUI_OnHelpReset;
            }
        }
    }

    private void _cardUI_OnHelpReset(object sender, EventArgs e)
    {
        coroutineId++;
        _titleCoroutines.Enqueue(coroutineId);
        _descriptionCoroutines.Enqueue(coroutineId);
        StartCoroutine(SetTitle(coroutineId, HelpPrefix));
        StartCoroutine(SetDescription(coroutineId, DefaultDescription));
    }

    private void _cardUI_OnHelpChanged(object sender, CardUI.OnHelpChangedEventHandler e)
    {
        if (e == null) return;
        coroutineId++;
        _titleCoroutines.Enqueue(coroutineId);
        _descriptionCoroutines.Enqueue(coroutineId);
        StartCoroutine(SetTitle(coroutineId, e.Title));
        StartCoroutine(SetDescription(coroutineId, e.Description));
    }

    IEnumerator SetTitle(long coroutineId, string title)
    {
        int frequency = 2;
        int currentFrequency=0;
        while (_titleCoroutines.Count > 0 && _titleCoroutines.Peek() != coroutineId) yield return 0;
        while (_cardUI.HelpTitle.text.Length > HelpTitleDefaultSize)
        {
            int len = _cardUI.HelpTitle.text.Length - 1;
            if (_cardUI.HelpTitle.text.LastOrDefault() == '>' && _cardUI.HelpTitle.text.LastIndexOf('<') > 0)
            {
                len = _cardUI.HelpTitle.text.LastIndexOf('<');
            }
   
            _cardUI.HelpTitle.text = _cardUI.HelpTitle.text.Substring(0, Math.Max(0, len));

            if (currentFrequency++ % frequency == 0)
            {
                yield return new WaitForSeconds(0.01f);
            }
        }

        string strNewTitle = title;
        while (title != HelpPrefix && !string.IsNullOrEmpty(strNewTitle) && _cardUI.HelpTitle.text.Length < HelpPrefix.Length + title.Length)
        {
            string str = strNewTitle.First() == '<' && strNewTitle.IndexOf(">") > 0 ? strNewTitle.Substring(0, strNewTitle.IndexOf(">") + 1) : strNewTitle.First().ToString();
            strNewTitle = strNewTitle.Substring(str.Length);
            _cardUI.HelpTitle.text += str;
            if (currentFrequency++ % frequency == 0)
            {
                yield return new WaitForSeconds(0.01f);
            }
        }
        _titleCoroutines.Dequeue();
    }

    IEnumerator SetDescription(long coroutineId, string description)
    {
        int frequency = 2;
        int currentFrequency = 0;
        while (_descriptionCoroutines.Count > 0 && _descriptionCoroutines.Peek() != coroutineId) yield return 0;
        while (_cardUI.HelpDescription.text.Length > 0)
        {
            int len = _cardUI.HelpDescription.text.Length - 1;
            if (_cardUI.HelpDescription.text.LastOrDefault() == '>' && _cardUI.HelpDescription.text.LastIndexOf('<') > 0)
            {
                len = _cardUI.HelpDescription.text.LastIndexOf('<');
            }

            _cardUI.HelpDescription.text = _cardUI.HelpDescription.text.Substring(0, Math.Max(0, len));
            if (currentFrequency++ % frequency == 0)
            {
                yield return new WaitForSeconds(0.01f);
            }
        }

        string strNewDescription = description;
        while (!string.IsNullOrEmpty(strNewDescription) && _cardUI.HelpDescription.text.Length < HelpPrefix.Length + description.Length)
        {
            string str = strNewDescription.First() == '<' && strNewDescription.IndexOf(">") > 0 ? strNewDescription.Substring(0, strNewDescription.IndexOf(">") + 1) : strNewDescription.First().ToString();
            strNewDescription = strNewDescription.Substring(str.Length);
            _cardUI.HelpDescription.text += str;
            if (currentFrequency++ % frequency == 0)
            {
                yield return new WaitForSeconds(0.01f);
            }
        }
        _descriptionCoroutines.Dequeue();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
