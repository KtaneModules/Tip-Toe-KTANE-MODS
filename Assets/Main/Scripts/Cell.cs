using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rnd = UnityEngine.Random;

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
    public TextMesh text;

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

        Transform transform = null;

        if (button != null)
        {
            m = Button.transform.GetComponent<MeshRenderer>();
            orange = m.material;
            transform = Button.transform.Find("Colorblind Text");
        }


        if (transform != null)
        {
            text = transform.GetComponent<TextMesh>();
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

    public void SetWhite(bool t, bool colorBlind, Color32 orangeColor)
    {
        m.material.color = t ? new Color32(240, 240, 240, 255) : orangeColor;

        if (colorBlind)
            SetTextColorBlack(t);
    }

    public void SetRed(bool t, bool colorBlind, Color32 orangeColor)
    {
        m.material.color = t ? new Color32(240, 20, 50, 255) : orangeColor;
    }

    public void SetGreen()
    {
        m.material.color = new Color32(0, (byte)Rnd.Range(230, 250), (byte)Rnd.Range(40, 90), 255);
    }

    public override string ToString()
    {
        return $"({10 - Row}, {(Col + 1) % 10})";
    }

    public void ShowText(bool t)
    {
        if (text != null && !t)
        {
            text.text = "";
        }
    }

    public void SetTextColorBlack(bool t)
    {
        if (t)
        {
            text.text = "W";
            text.color = Color.black;
        }


        else
        {
            text.text = "O";
            text.color = Color.white;
        }
    }
}
