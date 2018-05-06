using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class EndRoutineGear : MakineryGear
{
    public override object Current
    {
        get
        {
            return null;
        }
    }

    public override bool MoveNext()
    {
        return false;
    }

    public override void Reset()
    {
    }
}
