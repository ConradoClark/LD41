using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.CardGame.CardLogic
{
    public class MoveOneCard : ICard
    {

        private TopDown.Direction _direction;
        public MoveOneCard(TopDown.Direction direction)
        {
            _direction = direction;
        }
        public IEnumerator DoLogic(MonoBehaviour unity, EventHandler<EventArgs> onAfterUse)
        {
            MainCharacter mainCharacter;
            if (Toolbox.TryGetMainCharacter(out mainCharacter))
            {
                mainCharacter.Move(_direction);
            }
            if (onAfterUse == null) yield break;
            onAfterUse.Invoke(this, new EventArgs());
            yield break;
        }
    }
}
