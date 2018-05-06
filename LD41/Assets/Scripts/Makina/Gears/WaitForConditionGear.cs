using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class WaitForConditionGear : MakineryGear
{
    private Func<bool> _condition;
    public WaitForConditionGear(Func<bool> condition)
    {
        _condition = condition;
    }

    public override object Current
    {
        get
        {
            return _condition();
        }
    }

    public override bool MoveNext()
    {
        return _condition != null && !_condition();
    }

    public override void Reset()
    {
    }
}