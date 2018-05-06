using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class MakineryGear : IEnumerator
{
    public abstract object Current { get; }

    public abstract bool MoveNext();

    public abstract void Reset();
}
