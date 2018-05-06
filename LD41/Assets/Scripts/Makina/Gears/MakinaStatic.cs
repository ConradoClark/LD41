using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class MakinaStatic
{
    public static InactiveGear InactiveGear;
    public static LoopStepGear LoopStepGear;
    public static EndRoutineGear EndRoutineGear;

    static MakinaStatic()
    {
        InactiveGear = new InactiveGear();
        LoopStepGear = new LoopStepGear();
        EndRoutineGear = new EndRoutineGear();
    }
}