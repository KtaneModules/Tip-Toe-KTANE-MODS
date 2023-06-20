using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell{
    public bool Safe { get; set; }
    public bool Flicker { get; set; }
    public int FlickerTime1 { get; set; }
    public int FlickerTime2 { get; set; }
    public bool AlreadyFlickered1 { get; set; }
    public bool AlreadyFlickered2 { get; set; }
    public int Row { get; private set; }
    public int Col { get; private set; }
    public KMSelectable Button { get; private set; }

    private MeshRenderer m;
    Material white;
    Material orange;


    public Cell(int row, int col, KMSelectable button, Material white, Material orange)
    {
        Row = row;
        Col = col;
        Button = button;
        this.white = white;
        this.orange = orange;

        if (button != null)
        {
            m = Button.gameObject.GetComponent<MeshRenderer>();
        }
    }

    public bool Adjacent(Cell c)
    {
        bool up = c.Row == Row - 1 && c.Col == Col;
        bool left = c.Row == Row && c.Col == Col - 1;
        bool right = c.Row == Row && c.Col == Col + 1;
        bool down = c.Row == Row + 1 && c.Col == Col;

        return up || left || right || down;
    }

    public void SetWhite(bool t)
    {
        m.material = t ? white : orange;
    }
}
