using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.TopDown;
using System.Linq;
using System;

public class MainCharacter : MonoBehaviour
{
    public CharacterStats Stats;
    public Transform CharacterTransform;
    public SpriteRenderer CharacterSpriteRenderer;
    public Animator Animator;
    public GridObject GridObject;    
    private Queue<long> _moveCoroutines = new Queue<long>();
    private static long _move = 0;
    private LevelGrid _levelGrid;
    private Vector2 _currentSpeed;
    private Vector2 _currentDirection = Vector2.down;
    private int _currentHealth;
    private bool _takingDamage;
    public bool IsMoving { get; private set; }
    public bool IsIncapacitated { get; private set; }
    
    // Use this for initialization
    void Start()
    {
        Toolbox.TryGetLevelGrid(out _levelGrid);
        _levelGrid.OnGridAction += _levelGrid_OnGridAction;
        _currentHealth = Stats.Health;
    }

    private void _levelGrid_OnGridAction(object sender, LevelGrid.GridActionEventArgs e)
    {
        HandleDamage(e);
    }

    void HandleDamage(LevelGrid.GridActionEventArgs e)
    {
        if (e.Action == LevelGrid.GridEvents.EnemyAttack && !_takingDamage
            && e.TargetTile.SequenceEqual(_levelGrid.Vector2ToGrid(CharacterTransform.position)))
        {
            int damage = e.Values.ContainsKey(LevelGrid.GridValues.Damage) ? (int)e.Values[LevelGrid.GridValues.Damage] : 1;
            Vector2 pushBack = e.Values.ContainsKey(LevelGrid.GridValues.Push) ?
                (Vector2) e.Values[LevelGrid.GridValues.Push] : Vector2.zero;
            StartCoroutine(TakeDamage(damage));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsMoving)
        {
            CharacterTransform.position = Vector2.SmoothDamp(CharacterTransform.position,
                _levelGrid.CenterInGrid(CharacterTransform.position),
                ref _currentSpeed,
                1.0f,
                1.0f,
                Time.deltaTime);
            // LERP TO GRID
        }
    }

    public Vector2 GetDirection()
    {
        return _currentDirection;
    }

    public void UpdateStatsText()
    {
        CardUI cardUI;
        if (Toolbox.TryGetCardUI(out cardUI))
        {
            cardUI.Stats.text = string.Join("\n", new[] {
                Stats.Draw.ToString().PadLeft(2,'0'),
                Stats.Attack.ToString().PadLeft(2,'0'),
                Stats.Defense.ToString().PadLeft(2,'0'),
                Stats.Shuffle.ToString().PadLeft(2,'0'),
                Stats.Discard.ToString().PadLeft(2,'0'),
                Stats.Agility.ToString().PadLeft(2,'0'),
                Stats.PawSize.ToString().PadLeft(2,'0'),
            });
        }
    }

    public IEnumerator<MakineryGear> Move(Direction direction)
    {
        yield return null;
        Vector2 dirVector = Vector2.right;
        switch (direction)
        {
            case (Direction.Right):                
                dirVector = Vector2.right;
                break;
            case (Direction.Left):
                dirVector = Vector2.left;
                break;
            case (Direction.Up):
                dirVector = Vector2.up;
                break;
            case (Direction.Down):
                dirVector = Vector2.down;
                break;
        }
        _moveCoroutines.Enqueue(++_move);
        Makinery move = new Makinery(50);
        move.AddRoutine(() => Move(dirVector, _move, direction));
        yield return new InnerMakinery(move, Toolbox.Instance.MainMakina);
    }

    IEnumerator<MakineryGear> Move(Vector2 translation, long coroutineId, Direction direction)
    {
        while (_moveCoroutines.Peek() != coroutineId)
        {
            yield return new WaitForFrameCountGear();
        }

        if (CharacterTransform == null) yield break;
        IsMoving = true;
        Animator.SetBool("walking", true);
        Vector2 startingPos = CharacterTransform.transform.position;
        Vector2 endingPos = startingPos + translation;

        _currentDirection = translation;
        Animator.SetInteger("direction", (int)direction);

        int[] gridEndingPos = _levelGrid.Vector2ToGrid(endingPos);
        bool isBlocking = _levelGrid.IsBlocking(gridEndingPos[0], gridEndingPos[1]);
        if (isBlocking)
        {
            endingPos = startingPos + (translation / 4);
        }

        float max = isBlocking ? 0.50f : 1f;
        float time = 0f;
        while (time < max)
        {
            var lerp = Mathf.SmoothStep(0, 1, time);
            CharacterTransform.transform.position =
                 Vector2.Lerp(startingPos, endingPos, Mathf.Min(lerp, 1f));

            time += Time.deltaTime;
            yield return new WaitForFrameCountGear();
        }

        if (isBlocking)
        {
            time = 0.5f;
            while (time < 1f)
            {
                var lerp = Mathf.SmoothStep(0, 1, time);
                CharacterTransform.transform.position =
                     Vector2.Lerp(endingPos, startingPos, Mathf.Min(lerp, 1f));

                time += Time.deltaTime;
                yield return new WaitForFrameCountGear();
            }
        }

        IsMoving = false;
        Animator.SetBool("walking", false);
        _moveCoroutines.Dequeue();
    }

    IEnumerator TakeDamage(int damage)
    {
        _takingDamage = true;
        //GridObject.Blocking = true;
        IsIncapacitated = true;
        _currentHealth--;

        for (int i = 0; i < 5; i++)
        {
            CharacterSpriteRenderer.material.SetFloat("_Opacity", 0f);
            yield return new WaitForSeconds(0.1f);
            CharacterSpriteRenderer.material.SetFloat("_Opacity", 1f);
            yield return new WaitForSeconds(0.1f);
        }

        IsIncapacitated = false;
        //GridObject.Blocking = false;
        _takingDamage = false;
    }
}
