using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class WaitForFrameCountGear : MakineryGear
{
    int _totalFrames;
    int _framesLeft;
    public WaitForFrameCountGear(int frames=1)
    {
        _totalFrames = frames;
        _framesLeft = frames;
    }

    public override object Current
    {
        get
        {
            return _framesLeft;
        }
    }

    public override bool MoveNext()
    {
        return --_framesLeft > 0;
    }

    public override void Reset()
    {
        _framesLeft = _totalFrames;
    }
}
