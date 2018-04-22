using Assets.Scripts.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class InteractiveHelp : MonoBehaviour
{

    private CardUI _cardUI;
    private const int HelpTitleDefaultSize = 4;
    private const string HelpPrefix = "HELP";
    private Queue<long> _titleCoroutines = new Queue<long>();
    private Queue<long> _descriptionCoroutines = new Queue<long>();
    private long coroutineId;
    public string DefaultDescription;
    private string currentTitle;
    private string currentDescription;
    private long cancel;

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
        //coroutineId++;
        //_titleCoroutines.Enqueue(coroutineId);
        //_descriptionCoroutines.Enqueue(coroutineId);
        //StartCoroutine(SetText(coroutineId, _titleCoroutines, _cardUI.HelpTitle, HelpPrefix, 4));
        //StartCoroutine(SetText(coroutineId, _descriptionCoroutines, _cardUI.HelpDescription, DefaultDescription));
        //StartCoroutine(SetTitle(coroutineId, HelpPrefix));
        //StartCoroutine(SetDescription(coroutineId, DefaultDescription));
    }

    private void _cardUI_OnHelpChanged(object sender, CardUI.OnHelpChangedEventHandler e)
    {
        if (e == null) return;
        string helpTitle = HelpPrefix + e.Title;
        if (currentTitle == helpTitle && currentDescription == e.Description) return;

        cancel = coroutineId;
        coroutineId++;
        _titleCoroutines.Enqueue(coroutineId);
        _descriptionCoroutines.Enqueue(coroutineId);

        currentTitle = helpTitle;
        currentDescription = e.Description;
        StartCoroutine(SetText(coroutineId, _titleCoroutines, _cardUI.HelpTitle, helpTitle, 4));
        StartCoroutine(SetText(coroutineId, _descriptionCoroutines, _cardUI.HelpDescription, e.Description));
    }

    IEnumerator SetText(long coroutineId, Queue<long> coroutineQueue, TextMeshPro textComponent, string content, int minLength=0)
    {
        while(coroutineQueue.Count > 1)
        {
            if (coroutineId <= cancel)
            {
                coroutineQueue.Dequeue();
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }
        int frequency = 2;
        int currentFrequency = 0;
        while (coroutineQueue.Count > 0 && coroutineQueue.Peek() != coroutineId)
        {
            if (coroutineId <= cancel)
            {
                coroutineQueue.Dequeue();
                yield break;
            }

            yield return 0;
        }
        textComponent.text = textComponent.text.Substring(0, minLength);

        if (!string.IsNullOrEmpty(content) && textComponent.text.Length < content.Length)
        {
            string strNewDescription = content.Substring(minLength);
            while (!string.IsNullOrEmpty(strNewDescription) && textComponent.text.Length < content.Length)
            {
                if (coroutineId <= cancel) break;
                string str = strNewDescription.First() == '<' && strNewDescription.IndexOf(">") > 0 ? strNewDescription.Substring(0, strNewDescription.IndexOf(">") + 1) : strNewDescription.First().ToString();
                strNewDescription = strNewDescription.Substring(str.Length);
                textComponent.text += str;
                if (currentFrequency++ % frequency == 0)
                {
                    yield return new WaitForSeconds(0.01f);
                }
            }
        }
        coroutineQueue.Dequeue();
    }


    // Update is called once per frame
    void Update()
    {

    }
}
