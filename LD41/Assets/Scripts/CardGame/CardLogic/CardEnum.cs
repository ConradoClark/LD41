using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.CardGame.CardLogic
{
    public enum CardEnum
    {
        MoveUp,
        MoveRight,
        MoveDown,
        MoveLeft,
        AttackSlash,
    }

    public static class CardEnumExtensions
    {
        public static ICard GetAction(this CardEnum card)
        {
            if (Toolbox.Instance.DeckManager.CardsLogic.ContainsKey(card)) return Toolbox.Instance.DeckManager.CardsLogic[card];
            return null;
        }
    }
}
