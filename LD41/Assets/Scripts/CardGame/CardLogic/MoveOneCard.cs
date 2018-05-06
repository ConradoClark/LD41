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

        public bool CanUse()
        {
            return true;
        }

        public IEnumerator<MakineryGear> DoLogic(MonoBehaviour unity, EventHandler<EventArgs> onAfterUse)
        {
            MainCharacter mainCharacter;
            if (Toolbox.TryGetMainCharacter(out mainCharacter))
            {
                Makinery move = new Makinery(50);
                move.AddRoutine(() => mainCharacter.Move(_direction));
                yield return new InnerMakinery(move, Toolbox.Instance.MainMakina);
            }
            if (onAfterUse == null) yield break;
            onAfterUse.Invoke(this, new EventArgs());
            yield break;
        }
    }
}
