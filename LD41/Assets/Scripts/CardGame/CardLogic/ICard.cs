using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.CardGame.CardLogic
{
    public interface ICard
    {
        IEnumerator DoLogic(EventHandler<EventArgs> onAfterUse);
    }
}
