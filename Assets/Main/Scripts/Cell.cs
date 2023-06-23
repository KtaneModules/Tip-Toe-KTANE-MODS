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

    public Color32 White { get; private set; }
    public Color32 Orange { get; set; }


    public Cell Parent { get; set; }

    private MeshRenderer mmeshRenderer;


    public Cell(int row, int col, KMSelectable button)
    {
        Row = row;
        Col = col;
        Button = button;

        Transform transform = null;

        if (button != null)
        {
            mmeshRenderer = Button.transform.GetComponent<MeshRenderer>();
            transform = Button.transform.Find("Colorblind Text");
        }


        if (transform != null)
        {
            text = transform.GetComponent<TextMesh>();
        }

        FlickerTimes = new int[4];
        AlreadyFlickered = new bool[4];

        White = new Color32(240, 240, 240, 255);
    }

    public bool Adjacent(Cell c)
    {
        return
            Up == c ||
            Left == c ||
            Down == c ||
            Right == c;
    }

    public IEnumerator Fade(float time, Color targetColor, bool fadeBack)
    {
        float elaspedTime = 0f;

        Color c = mmeshRenderer.material.color;

        while (elaspedTime < time)
        {
            SetMaterialColor(Color.Lerp(c, targetColor, elaspedTime/time));
            elaspedTime += Time.deltaTime;
            yield return null;
        }

        if (fadeBack)
        {
            yield return Fade(time, Orange, false);
        }
    }

    public void FadeWhite()
    {

    }

    public void SetRed(bool t, bool colorBlind, Color32 orangeColor)
    {
        SetMaterialColor(t ? new Color32(240, 20, 50, 255) : orangeColor);
    }

    public void SetGreen()
    {
        SetMaterialColor(new Color32(0, (byte)Rnd.Range(230, 250), (byte)Rnd.Range(40, 90), 255));
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

    void SetMaterialColor(Color c)
    {
        mmeshRenderer.material.color = c;
    }
}
