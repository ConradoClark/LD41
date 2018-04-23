using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public int Health;
    private int _currentHealth;
    private LevelGrid _grid;
    public SpriteRenderer SpriteRenderer;
    private bool _takingDamage;

	// Use this for initialization
	void Start () {
        Toolbox.TryGetLevelGrid(out _grid);
        _grid.OnGridAction += _grid_OnGridAction;
        _currentHealth = Health;
    }

    private void _grid_OnGridAction(object sender, LevelGrid.GridActionEventArgs e)
    {
        if (e.Action == LevelGrid.GridEvents.HeroAttack && !_takingDamage
            && e.TargetTile.SequenceEqual(_grid.Vector2ToGrid(transform.position)))
        {
            int damage = 1;
            if (e.Values.ContainsKey("Damage"))
            {
                damage = (int) e.Values["Damage"];
            }
            StartCoroutine(TakeDamage(damage));
        }
    }

    IEnumerator TakeDamage(int damage)
    {
        _takingDamage = true;
        _currentHealth--;

        for (int i = 0; i < 5; i++)
        {
            SpriteRenderer.material.SetFloat("_Opacity", 0f);
            yield return new WaitForSeconds(0.1f);
            SpriteRenderer.material.SetFloat("_Opacity", 1f);
            yield return new WaitForSeconds(0.1f);
        }

        if (_currentHealth <= 0)
        {
            yield return Die();
        }

        _takingDamage = false;
    }

    IEnumerator Die()
    {
        float time = 0f;
        while (time < 1f)
        {
            var lerp = Mathf.Lerp(0f, -1f, time * time);
            SpriteRenderer.material.SetFloat("_Saturation", lerp);
            time += Time.deltaTime*5;
            yield return new WaitForEndOfFrame();
        }
        time = 0f;
        while (time < 1f)
        {
            var lerp = Mathf.Lerp(1f, 0f, time * time);
            SpriteRenderer.material.SetFloat("_Opacity", lerp);
            time += Time.deltaTime * 5;
            yield return new WaitForEndOfFrame();
        }
        gameObject.SetActive(false);
    }

    public bool IsTakingDamage()
    {
        return _takingDamage;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
