using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using mk = CardGameMakineryConstants;


public class GoalIcon : MonoBehaviour
{
    public SpriteRenderer GoalIconSpriteRenderer;
    public Sprite IncompleteState;
    public Sprite CompleteState;

    public Makinery Complete()
    {
        Makinery mkComplete = new Makinery(mk.Priority.UIAnimations);
        mkComplete.AddRoutine(() => MkComplete());
        return mkComplete;
    }

    private IEnumerator<MakineryGear> MkComplete()
    {
        if (GoalIconSpriteRenderer.sprite == CompleteState) yield break;
        Makinery inner = new Makinery(mk.Priority.UIAnimations);
        inner.AddRoutine(
            MkLerp.LerpFloat(
            (f) => GoalIconSpriteRenderer.material.SetFloat("_Luminance", f),
            () => GoalIconSpriteRenderer.material.GetFloat("_Luminance"),
            0.5f,
            () => 0.4f,
            MkEasing.EasingFunction.SineEaseIn
            ));

        yield return new InnerMakinery(inner, Toolbox.Instance.MainMakina);
        GoalIconSpriteRenderer.sprite = CompleteState;

        inner = new Makinery(mk.Priority.UIAnimations);
        inner.AddRoutine(
            MkLerp.LerpFloat(
            (f) => GoalIconSpriteRenderer.material.SetFloat("_Luminance", f),
            () => GoalIconSpriteRenderer.material.GetFloat("_Luminance"),
            0.5f,
            () => 0f,
            MkEasing.EasingFunction.SineEaseOut
            ));

        yield return new InnerMakinery(inner, Toolbox.Instance.MainMakina);
    }

    public void Reset()
    {
        GoalIconSpriteRenderer.sprite = IncompleteState;
        GoalIconSpriteRenderer.material.SetFloat("_Luminance", 1f);
    }
}
