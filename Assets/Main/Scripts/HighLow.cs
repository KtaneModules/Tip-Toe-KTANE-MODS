using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighLow {

    public int Higher { get; private set; }
    public int Lower { get; private set; }

    public HighLow(int higher, int lower)
    {
        Higher = higher;
        Lower = lower;
    }
}
