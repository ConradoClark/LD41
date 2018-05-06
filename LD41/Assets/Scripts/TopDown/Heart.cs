using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using mk = CardGameMakineryConstants;

public class Heart : MonoBehaviour {

    public enum HeartState
    {
        Empty,
        Half,
        Full
    }
    public SpriteRenderer FullHeartSpriteRenderer;
    public HeartState State = HeartState.Empty;
    Dictionary<HeartState, float> _heartValues = new Dictionary<HeartState, float>()
    {
        { HeartState.Empty, 0f },
        { HeartState.Half, 0.5f },
        { HeartState.Full, 1f }
    };

    // Use this for initialization
    void Start() {
        FullHeartSpriteRenderer.material.SetFloat("_FadeInStripsHorizontal", 1);
    }

    // Update is called once per frame
    void Update() {

    }

    public Makinery Restore()
    {
        bool stateChanged = RestoreState();
        if (stateChanged)
        {
            return AnimateChange();
        }
        return null;
    }

    public Makinery Damage()
    {
        bool stateChanged = DamageState();
        if (stateChanged)
        {
            return AnimateChange();
        }
        return null;
    }

    Makinery AnimateChange()
    {
        Makinery blink = new Makinery(mk.Priority.UIAnimations);
        blink.AddRoutine(MkLerp.LerpFloat(
            (f) => FullHeartSpriteRenderer.material.SetFloat("_Luminance", f),
            () => FullHeartSpriteRenderer.material.GetFloat("_Luminance"),
            0.25f,
            () => 1,
            MkEasing.EasingFunction.CubicEaseOut
            ),
            MkLerp.LerpFloat(
            (f) => FullHeartSpriteRenderer.material.SetFloat("_Luminance", f),
            () => FullHeartSpriteRenderer.material.GetFloat("_Luminance"),
            0.10f,
            () => 0,
            MkEasing.EasingFunction.CircularEaseOut
            ));
        Toolbox.Instance.MainMakina.AddMakinery(blink);

        Makinery chng = new Makinery(mk.Priority.UIAnimations);
        chng.AddRoutine(MkLerp.LerpFloat(
            (f) => FullHeartSpriteRenderer.material.SetFloat("_FadeInStripsHorizontalProgress", f),
            () => FullHeartSpriteRenderer.material.GetFloat("_FadeInStripsHorizontalProgress"),
            0.15f,
            () => _heartValues[State],
            MkEasing.EasingFunction.QuadraticEaseIn
            ));
        return chng;
    }

    bool RestoreState()
    {
        switch (State)
        {
            case HeartState.Empty:
                State = HeartState.Half;
                return true;
            case HeartState.Half:
                State = HeartState.Full;
                return true;
            case HeartState.Full:
                return false;
            default:
                return false;
        }
    }

    bool DamageState()
    {
        switch (State)
        {
            case HeartState.Empty:
                return false;
            case HeartState.Half:
                State = HeartState.Empty;
                return true;
            case HeartState.Full:
                State = HeartState.Half;
                return true;
            default:
                return false;
        }
    }
}
