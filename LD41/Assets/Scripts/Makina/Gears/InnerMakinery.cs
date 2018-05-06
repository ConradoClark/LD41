using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class InnerMakinery : MakineryGear
{
    private Makinery _makinery;
    private Makina _makina;
    private MakineryGear _gearStep;
    public InnerMakinery(Makinery makinery, Makina makina)
    {
        _makinery = makinery;
        _makina = makina;
        _makinery.Reset();
    }

    public override object Current
    {
        get
        {
            return _gearStep;
        }
    }

    public override bool MoveNext()
    {
        return _makina.RunMakineryStep(_makinery, out _gearStep) != Makina.MakineryStepResult.Done;
    }

    public override void Reset()
    {
        _makinery.Reset();
    }
}
