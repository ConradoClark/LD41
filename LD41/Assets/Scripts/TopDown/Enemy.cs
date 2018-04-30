using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public int Health;
    private int _currentHealth;
    private LevelGrid _levelGrid;
    public SpriteRenderer SpriteRenderer;
    private bool _takingDamage;
    private bool _isMoving;
    private Queue<long> _moveCoroutines = new Queue<long>();
    private static long _move = 0;

    // Use this for initialization
    void Start() {
        Toolbox.TryGetLevelGrid(out _levelGrid);
        _levelGrid.OnGridAction += _grid_OnGridAction;
        _currentHealth = Health;
    }

    private void _grid_OnGridAction(object sender, LevelGrid.GridActionEventArgs e)
    {
        if (e.Action == LevelGrid.GridEvents.HeroAttack && !_takingDamage
            && e.TargetTile.SequenceEqual(_levelGrid.Vector2ToGrid(transform.position)))
        {
            int damage = 1;
            if (e.Values.ContainsKey("Damage"))
            {
                damage = (int)e.Values["Damage"];
            }
            Vector2 dir = e.Values.ContainsKey("Push") ? (Vector2)e.Values["Push"] : Vector2.zero;
            StartCoroutine(TakeDamage(damage, dir));
        }
    }

    IEnumerator TakeDamage(int damage, Vector2 pushDirection)
    {
        _takingDamage = true;
        _currentHealth--;

        StartCoroutine(Move(pushDirection, 2f, true));

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
            time += Time.deltaTime * 5;
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

    public IEnumerator Move(Vector2 translation, float speed= 1.0f, bool forced=false)
    {
        _moveCoroutines.Enqueue(++_move);
        long coroutineId = _move;
        while (_moveCoroutines.Peek() != coroutineId)
        {
            yield return new WaitForEndOfFrame();
        }

        _isMoving = true;
        Vector2 startingPos = this.transform.position;
        Vector2 endingPos = startingPos + translation;

        int[] gridEndingPos = _levelGrid.Vector2ToGrid(endingPos);

        bool isBlocking = _levelGrid.IsBlocking(gridEndingPos[0], gridEndingPos[1]);
        if (isBlocking)
        {
            endingPos = startingPos + (translation / 4);
        }

        float max = isBlocking ? 0.50f : 1f;
        float time = 0f;
        bool flash = false;
        while (time < max)
        {
            if (_takingDamage && !forced)
            {
                _isMoving = false;
                _moveCoroutines.Dequeue();
                yield break;
            }
            var lerp = Mathf.SmoothStep(0, 1, time);
            this.transform.position =
                 Vector2.Lerp(startingPos, endingPos, Mathf.Min(lerp, 1f));

            if (!flash && time > max / 3)
            {
                _levelGrid.FlashTile(gridEndingPos, 1f, new Color(219f / 255f, 51f / 255f, 51f / 255f));

                flash = true;
            }

            time += Time.deltaTime / 2 * speed;
            yield return new WaitForEndOfFrame();
        }

        if (isBlocking)
        {
            time = 0.5f;
            while (time < 1f)
            {
                var lerp = Mathf.SmoothStep(0, 1, time);
                this.transform.position =
                     Vector2.Lerp(endingPos, startingPos, Mathf.Min(lerp, 1f));

                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        _isMoving = false;
        _moveCoroutines.Dequeue();
    }

    public bool IsTakingDamage()
    {
        return _takingDamage;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
