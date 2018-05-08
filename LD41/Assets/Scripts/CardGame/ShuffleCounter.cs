using System.Collections;
using mk = CardGameMakineryConstants;
using UnityEngine;

public class ShuffleCounter : MonoBehaviour {

    public SpriteRenderer SpriteRendererFullIcon;
    public SpriteRenderer SpriteRendererEmptyIcon;
    private bool _canShuffle;
    private float _shuffleBar;
    private MainCharacter _mainCharacter;
    private Deck _deck;

	// Use this for initialization
	void Start () {
        _shuffleBar = 0f;
        _canShuffle = false;
        Toolbox.TryGetMainCharacter(out _mainCharacter);
        Toolbox.Instance.OnPostInit += Instance_OnPostInit;
    }

    private void Instance_OnPostInit(object sender, System.EventArgs e)
    {
        Toolbox.TryGetDeck(out _deck);
    }

    // Update is called once per frame
    void Update () {
        _shuffleBar += _mainCharacter.Stats.Shuffle * Time.deltaTime / 20;
        _shuffleBar = Mathf.Clamp(_shuffleBar, 0, 1);
        if (_shuffleBar == 1f)
        {
            if (!_canShuffle)
            {
                StartCoroutine(CanShuffleAnim());
            }
            _canShuffle = true;
        }
        if (SpriteRendererEmptyIcon != null)
        {
            SpriteRendererEmptyIcon.material.SetFloat("_Cutoff", _shuffleBar);
        }
	}

    void OnMouseOver()
    {
        if (!Toolbox.Instance.MainCharacter.IsDead && Input.GetMouseButtonUp(0))
        {
            if (_canShuffle && !Toolbox.Instance.Deck.Drawing && !Toolbox.Instance.Deck.Reorganizing)
            {
                _canShuffle = false;
                _shuffleBar = 0;
                SpriteRendererFullIcon.material.SetFloat("_Saturation", -0.4f);
                Makinery shuffle = new Makinery(mk.Priority.CardShuffle) { QueueName = mk.Queues.DeckOperation };
                shuffle.AddRoutine(() => _deck.ShufflePawIntoCardPile());
                Toolbox.Instance.MainMakina.AddMakinery(shuffle);
            }
        }
    }

    IEnumerator CanShuffleAnim()
    {
        if (SpriteRendererFullIcon == null) yield break;

        float lumi = 0.65f;
        float hue = 50;

        float time = 0f;
        while (time <= 1f)
        {
            float lumiLerp = QuintEaseInOut(time, 0, lumi, 1f);
            float hueLerp = QuintEaseInOut(time, 0, hue, 1f);
            SpriteRendererFullIcon.material.SetFloat("_Luminance", lumiLerp);
            SpriteRendererFullIcon.material.SetFloat("_Hue", hueLerp);
            time += Time.deltaTime *6f;
            yield return new WaitForEndOfFrame();
        }

        SpriteRendererFullIcon.material.SetFloat("_Luminance", lumi);
        SpriteRendererFullIcon.material.SetFloat("_Hue", hue);

        time = 0f;
        while (time <= 1f)
        {
            float lumiLerp = Mathf.Lerp(lumi, 0, time * time);
            float hueLerp = Mathf.Lerp(hue, 0, time * time);
            SpriteRendererFullIcon.material.SetFloat("_Luminance", lumiLerp);
            SpriteRendererFullIcon.material.SetFloat("_Hue", hueLerp);
            time += Time.deltaTime * 5f;
            yield return new WaitForEndOfFrame();
        }

        SpriteRendererFullIcon.material.SetFloat("_Luminance", 0f);
        SpriteRendererFullIcon.material.SetFloat("_Hue", 0);
        SpriteRendererFullIcon.material.SetFloat("_Saturation", 0);

        yield break;
    }

    float QuintEaseInOut(float t, float b, float c, float d)
    {
        if ((t /= d / 2) < 1)
            return c / 2 * t * t * t * t * t + b;
        return c / 2 * ((t -= 2) * t * t * t * t + 2) + b;
    }
}
