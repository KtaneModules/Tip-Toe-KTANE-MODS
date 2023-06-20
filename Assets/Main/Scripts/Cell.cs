using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell {
    public bool Safe { get; set; }
    public bool Flicker { get; set; }
    public int[] FlickerTimes { get; private set; }
    public bool[] AlreadyFlickered { get; set; }
    public int Row { get; private set; }
    public int Col { get; private set; }
    public KMSelectable Button { get; private set; }

    public Cell Up { get;  set; }
    public Cell Right { get;  set; }
    public Cell Down { get;  set; }
    public Cell Left { get;  set; }
    public bool Viseted { get; set; }
    public int Heuristic { get; set; }
    public int FinalCost { get; set; }
    public int G { get; set; }

    public Cell Parent { get; set; }

    private MeshRenderer m;
    Material white;
    Material orange;


    public Cell(int row, int col, KMSelectable button, Material white)
    {
        Row = row;
        Col = col;
        Button = button;
        this.white = white;

        if (button != null)
        {
            m = Button.gameObject.GetComponent<MeshRenderer>();
            orange = m.material;
        }

        FlickerTimes = new int[4];
        AlreadyFlickered = new bool[4];
    }

    public bool Adjacent(Cell c)
    {
        return
            Up == c ||
            Left == c ||
            Down == c ||
            Right == c;
    }

    public void SetWhite(bool t)
    {
        m.material = t ? white : orange;
    }

    public override string ToString()
    {
        return $"({Row}, {Col})";
    }
}
