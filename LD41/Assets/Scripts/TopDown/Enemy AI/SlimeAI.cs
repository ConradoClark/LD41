using Assets.Scripts.TopDown;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeAI : MonoBehaviour
{

    private Queue<long> _moveCoroutines = new Queue<long>();
    private static long _move = 0;
    private bool _looping;
    private bool _isMoving;
    private LevelGrid _levelGrid;
    private MainCharacter _mainCharacter;
    private Vector2 _currentDirection;
    private Vector2 _currentSpeed;
    // Use this for initialization
    void Start()
    {
        Toolbox.TryGetLevelGrid(out _levelGrid);
        Toolbox.TryGetMainCharacter(out _mainCharacter);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector2.SmoothDamp(transform.position,
            _levelGrid.CenterInGrid(transform.position),
            ref _currentSpeed,
            1.0f,
            1.0f,
            Time.deltaTime);

        if (isActiveAndEnabled && !_looping)
        {
            _looping = true;
            StartCoroutine(Loop());
        }
    }

    IEnumerator Loop()
    {
        while (isActiveAndEnabled)
        {
            var dir = Mathf.RoundToInt(Random.Range(0f,1f) * 8f);
            Vector2 dirVector = Vector2.right;
            switch (dir)
            {
                case (int)Direction.Up:
                    dirVector = Vector2.up;
                    break;
                case (int)Direction.Right:
                    dirVector = Vector2.right;
                    break;
                case (int)Direction.Down:
                    dirVector = Vector2.down;
                    break;
                case (int)Direction.Left:
                    dirVector = Vector2.left;
                    break;
                case 5:
                    dirVector = Vector2.zero;
                    break;
                default:
                    var dir2 = Mathf.RoundToInt(Random.Range(0f, 1f));
                    Vector3 testV = _mainCharacter.CharacterTransform.position - this.transform.position;
                    // horizontal
                    if (dir2 == 0 || testV.y < 0.1f)
                    {
                        dirVector = testV.x < 0 ? Vector2.left : Vector2.right;
                    }
                    else
                    {
                        dirVector = testV.y < 0 ? Vector2.down : Vector2.up;
                    }
                    break;
            }

            _currentDirection = dirVector;
            _moveCoroutines.Enqueue(++_move);
            yield return StartCoroutine(Move(dirVector, _move));
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator Move(Vector2 translation, long coroutineId)
    {
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
        while (time < max)
        {
            var lerp = Mathf.SmoothStep(0, 1, time);
            this.transform.position =
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
                this.transform.position =
                     Vector2.Lerp(endingPos, startingPos, Mathf.Min(lerp, 1f));

                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        _isMoving = false;
        _moveCoroutines.Dequeue();
    }
}
