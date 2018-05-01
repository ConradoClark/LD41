using Assets.Scripts.CardGame.CardLogic;
using Assets.Scripts.TopDown;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [Serializable]
    public struct CardAssociation
    {
        public CardEnum CardType;
        public PoolInstance PrefabPoolInstance;
    }
    public CardAssociation[] CardDatabase;
    public PoolInstance SlashInstance;

    public Dictionary<CardEnum, ICard> CardsLogic;
    public PoolInstance UsageQueuePoolInstance;

    // Use this for initialization
    void Start()
    {
        CardsLogic = new Dictionary<CardEnum, ICard>()
        {
            {CardEnum.MoveRight, new MoveOneCard(Direction.Right) },
            {CardEnum.MoveLeft, new MoveOneCard(Direction.Left) },
            {CardEnum.MoveUp, new MoveOneCard(Direction.Up) },
            {CardEnum.MoveDown, new MoveOneCard(Direction.Down) },
            {CardEnum.AttackSlash, new SlashCard(SlashInstance) },
        };
    }

    // Update is called once per frame
    void Update()
    {

    }
}
