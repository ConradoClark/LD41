using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardQueueCounter : MonoBehaviour {

    public SpriteRenderer CounterSpriteRenderer;
    public TextMeshPro TextMeshPro;
    public Vector2 OffsetToCard;
    private Transform _cardTransform;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (_cardTransform == null) return;
        transform.position = _cardTransform.position + (Vector3)OffsetToCard;
    }

    public void Show(Transform cardPosition, int queuePos)
    {
        gameObject.SetActive(true);
        _cardTransform = cardPosition;
        transform.position = _cardTransform.position + (Vector3)OffsetToCard;
        TextMeshPro.SetText(queuePos.ToString());
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
