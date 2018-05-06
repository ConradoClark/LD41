using Assets.Scripts.TopDown;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeAI : MonoBehaviour
{
    private bool _looping;

    private LevelGrid _levelGrid;
    private MainCharacter _mainCharacter;
    private Vector2 _currentDirection;
    private Vector2 _currentSpeed;
    public Enemy _enemyRef;
    // Use this for initialization
    void Start()
    {
        Toolbox.TryGetLevelGrid(out _levelGrid);
        Toolbox.TryGetMainCharacter(out _mainCharacter);
        if (_enemyRef == null)
        {
            _enemyRef = GetComponent<Enemy>();
        }
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
        while (gameObject.activeSelf)
        {
            if (!_enemyRef.IsTakingDamage())
            {
                var dir = Mathf.RoundToInt(Random.Range(0f, 1f) * 12f);
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
                        Vector3 testV = _mainCharacter.CharacterTransform.position - transform.position;
                        if (dir2 == 0 && Mathf.Abs(testV.x) > 0.1f)
                        {
                            dirVector = testV.x < 0 ? Vector2.left : Vector2.right;
                        }
                        else
                        {
                            dirVector = testV.y < 0 ? Vector2.down : Vector2.up;
                        }
                        break;
                }

                if (dirVector != Vector2.zero)
                {
                    _currentDirection = dirVector;
                    yield return StartCoroutine(_enemyRef.Move(dirVector));
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    
}
