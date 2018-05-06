using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.CardGame.CardLogic
{
    public interface ICard
    {
        IEnumerator<MakineryGear> DoLogic(MonoBehaviour unity, EventHandler<EventArgs> onAfterUse);
        bool CanUse();
    }
}
