using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using mk = CardGameMakineryConstants;

public class HealthCounter : MonoBehaviour
{
    public PoolInstance HeartPoolInstance;
    private MainCharacter _mainCharacter;
    List<Heart> _hearts;
    private int _currentHealth;
    // Use this for initialization
    void Start()
    {
        _hearts = new List<Heart>();
        if (Toolbox.TryGetMainCharacter(out _mainCharacter))
        {
            for (int i = 0; i < _mainCharacter.Stats.Health; i += 2)
            {
                float xPos = i / 2 * 0.4f;
                var obj = Toolbox.Instance.Pool.Retrieve(HeartPoolInstance);
                obj.transform.SetParent(transform);
                obj.transform.localPosition = new Vector3(xPos, 0);
                _hearts.Add(obj.GetComponent<Heart>());
            }

            Makinery adjustHealth = new Makinery(mk.Priority.UIAnimations) { Looping = true };
            adjustHealth.AddRoutine(() => AdjustHealth());
            Toolbox.Instance.MainMakina.AddMakinery(adjustHealth);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator<MakineryGear> AdjustHealth()
    {
        if (_currentHealth == _mainCharacter.Stats.CurrentHealth) yield break;

        for (int ix = 0; ix < _hearts.Count; ix++)
        {
            int i = _currentHealth > _mainCharacter.Stats.CurrentHealth ? _hearts.Count - ix - 1 : ix;
            int healthTarget = _mainCharacter.Stats.CurrentHealth;

            if (healthTarget >= (i + 1) * 2)
            {
                while (_hearts[i].State != Heart.HeartState.Full)
                {
                    Makinery mk = _hearts[i].Restore();
                    yield return new InnerMakinery(mk, Toolbox.Instance.MainMakina);
                }
            }

            if (healthTarget == (i + 1) * 2 - 1)
            {
                if (_hearts[i].State == Heart.HeartState.Full)
                {
                    Makinery mk = _hearts[i].Damage();
                    yield return new InnerMakinery(mk, Toolbox.Instance.MainMakina);
                }
                else if (_hearts[i].State == Heart.HeartState.Empty)
                {
                    Makinery mk = _hearts[i].Restore();
                    yield return new InnerMakinery(mk, Toolbox.Instance.MainMakina);
                }
            }

            if (healthTarget < (i + 1) * 2 - 1)
            {
                while (_hearts[i].State != Heart.HeartState.Empty)
                {
                    Makinery mk = _hearts[i].Damage();
                    yield return new InnerMakinery(mk, Toolbox.Instance.MainMakina);
                }
            }
        }
        _currentHealth = _mainCharacter.Stats.CurrentHealth;
    }
}
