using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetMainCharacter : MonoBehaviour {

    public Transform Transform;
    public CharacterStats Stats;
    public Animator Animator;
    public SpriteRenderer CharacterSpriteRenderer;
    public GridObject GridObject;
    private MainCharacter _mainCharacter;
	// Use this for initialization
	void Awake () {
        if (Toolbox.TryGetMainCharacter(out _mainCharacter))
        {
            _mainCharacter.CharacterTransform = Transform;
            _mainCharacter.Stats = Stats;
            _mainCharacter.Animator = Animator;
            _mainCharacter.GridObject = GridObject;
            _mainCharacter.CharacterSpriteRenderer = CharacterSpriteRenderer;
        }
    }

    void Start()
    {
        CardUI cardUI;
        if (Toolbox.TryGetCardUI(out cardUI))
        {
            _mainCharacter.UpdateStatsText();
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
