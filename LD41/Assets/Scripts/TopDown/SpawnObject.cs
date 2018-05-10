using Assets.Scripts.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using mk = CardGameMakineryConstants;

public class SpawnObject : MonoBehaviour
{
    public float TimeUntilSpawn { get; set; }
    private const float iconVanishTime = 0.25f;
    private const float moveObjectTime = 2f;
    public Vector3 Destination { get; set; }
    public SpriteRenderer Icon;
    public SpriteRenderer Arrow;
    public SpriteRenderer SpawnedObject;
    public GridObject GridObject;
    private Makinery _floatArrow, _animateIcon, _vanishObjects, _moveObject, _moveObjectInner;
    private LevelGrid _levelGrid;
    private int _originalLayerOrder;

    private void Init()
    {
        _originalLayerOrder = SpawnedObject.sortingOrder;
        SpawnedObject.sortingOrder = Arrow.sortingOrder + 1;
        GridObject.transform.position = Destination + new Vector3(0, 7f);
        Icon.transform.position = Destination + Icon.transform.localPosition;
        Arrow.transform.position = Destination + Arrow.transform.localPosition;

        _floatArrow = new Makinery(mk.Priority.UIAnimations) { Looping = true };
        _floatArrow.AddRoutine(MkLerp.LerpFloat(
            (f) => Arrow.transform.position = new Vector3(Arrow.transform.position.x, f, Arrow.transform.position.z),
            () => Arrow.transform.position.y,
            0.25f,
            () => Arrow.transform.position.y + .35f,
            MkEasing.EasingFunction.QuadraticEaseIn
            ));
        _floatArrow.AddRoutine(MkLerp.LerpFloat(
           (f) => Arrow.transform.position = new Vector3(Arrow.transform.position.x, f, Arrow.transform.position.z),
           () => Arrow.transform.position.y,
           0.25f,
           () => Arrow.transform.position.y - .35f,
           MkEasing.EasingFunction.QuadraticEaseOut
           ));

        _animateIcon = new Makinery(mk.Priority.UIAnimations) { Looping = true };
        _animateIcon.AddRoutine(MkLerp.LerpFloat(
            (f) => Icon.transform.localScale = new Vector3(f, f, Icon.transform.localScale.z),
            () => Icon.transform.localScale.x,
            1f,
            () => 1.1f,
            MkEasing.EasingFunction.QuadraticEaseIn
            ));
        _animateIcon.AddRoutine(MkLerp.LerpFloat(
           (f) => Icon.transform.localScale = new Vector3(f, f, Icon.transform.localScale.z),
           () => Icon.transform.localScale.x,
           1f,
           () => 1.1f,
           MkEasing.EasingFunction.QuadraticEaseOut
           ));

        _moveObjectInner = new Makinery(mk.Priority.UIAnimations);
        _moveObjectInner.AddRoutine(MkLerp.LerpFloat(
            (f) => GridObject.transform.position = new Vector3(GridObject.transform.position.x, f, GridObject.transform.position.z),
            () => GridObject.transform.position.y,
            moveObjectTime,
            () => Destination.y,
            MkEasing.EasingFunction.QuarticEaseIn
            ));

        _moveObject = new Makinery(mk.Priority.UIAnimations);
        _moveObject.AddRoutine(() => MkMoveObject());

        _vanishObjects = new Makinery(mk.Priority.UIAnimations);
        _vanishObjects.AddRoutine(MkLerp.LerpFloat(
            (f) =>
            {
                Arrow.material.SetFloat("_Opacity", f);
                Icon.material.SetFloat("_Opacity", f);
            },
            () => Arrow.material.GetFloat("_Opacity"),
            iconVanishTime,
            () => 0f,
            MkEasing.EasingFunction.CubicEaseInOut
            ));
    }

    private IEnumerator<MakineryGear> MkMoveObject()
    {
        GridObject.Active = false;
        _levelGrid.FlashTile(_levelGrid.Vector2ToGrid(Destination), 2f, TimeUntilSpawn + moveObjectTime, Colors.MidRed);

        yield return new WaitForSecondsGear(TimeUntilSpawn);
        yield return new InnerMakinery(_moveObjectInner, Toolbox.Instance.MainMakina);

        GridObject.transform.position = Destination;
        _levelGrid.FlashTile(_levelGrid.Vector2ToGrid(Destination), 5f, 0.25f, Colors.White);

        yield return new InnerMakinery(_vanishObjects, Toolbox.Instance.MainMakina);
        Arrow.material.SetFloat("_Opacity", 0f);
        Icon.material.SetFloat("_Opacity", 0f);

        SpawnedObject.sortingOrder = _originalLayerOrder;
        GridObject.Active = true;

        Toolbox.Instance.LevelGrid.TriggerGridEvent(LevelGrid.GridEvents.CrushAttack,
            GridObject, _levelGrid.Vector2ToGrid(Destination)
            , new Dictionary<string, object>()
            {
                { "Damage", GridObject.Weight},
            });

        Toolbox.Instance.MainMakina.RemoveMakinery(_floatArrow);
        Toolbox.Instance.MainMakina.RemoveMakinery(_animateIcon);
    }

    public void Spawn(Vector3 Destination, LevelGrid grid)
    {        
        this.Destination = Destination;
        Init();
        _levelGrid = grid;
        Toolbox.Instance.MainMakina.AddMakinery(_floatArrow, _animateIcon, _moveObject);
    }
}
