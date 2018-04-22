using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.TopDown;

public class MainCharacter : MonoBehaviour
{
    public CharacterStats Stats;
    public Transform CharacterTransform;
    private bool IsMoving;
    private Queue<long> _moveCoroutines = new Queue<long>();
    private static long _move = 0;
    private LevelGrid _levelGrid;
    // Use this for initialization
    void Start()
    {
        Toolbox.TryGetLevelGrid(out _levelGrid);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsMoving)
        {
            // LERP TO GRID
        }
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

    public void Move(Direction direction)
    {
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
        StartCoroutine(Move(dirVector, _move));
    }

    IEnumerator Move(Vector2 translation, long coroutineId)
    {
        while (_moveCoroutines.Peek() != coroutineId)
        {
            yield return new WaitForEndOfFrame();
        }

        if (CharacterTransform == null) yield break;
        IsMoving = true;        
        Vector2 startingPos = CharacterTransform.transform.position;
        Vector2 endingPos = startingPos + translation;

        int[] gridEndingPos = _levelGrid.Vector2ToGrid(endingPos);
        Debug.LogWarning("POS: X" + gridEndingPos[0] + " Y" + gridEndingPos[1]);
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
            yield return new WaitForEndOfFrame();
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
                yield return new WaitForEndOfFrame();
            }
        }

        IsMoving = false;
        _moveCoroutines.Dequeue();
    }
}
