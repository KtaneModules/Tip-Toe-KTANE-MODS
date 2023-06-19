﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell{
    public bool Condition { get; set; }
    public int Row { get; private set; }
    public int Col { get; private set; }
    public KMSelectable Button { get; private set; }

    public Cell(int row, int col, KMSelectable button)
    {
        Row = row;
        Col = col;
        Button = button;
    }
}
