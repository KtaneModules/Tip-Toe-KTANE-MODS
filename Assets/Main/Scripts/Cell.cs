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
    public Color32 Black { get; set; }

    public Cell Parent { get; set; }

    private MeshRenderer meshRenderer;

    private bool colorBlindOn; //this is going to always be false cuz module idea person doesn't want it, but i dont want to delete the code that does it


    public Cell(int row, int col, KMSelectable button, bool colorBlindOn)
    {
        Row = row;
        Col = col;
        Button = button;
        this.colorBlindOn = colorBlindOn;
        Black = new Color32(0, 0, 0, 1);
        Transform transform = null;

        if (button != null)
        {
            meshRenderer = Button.transform.GetComponent<MeshRenderer>();
            transform = Button.transform.Find("Colorblind Text");
        }

        if (transform != null)
        {
            text = transform.GetComponent<TextMesh>();

            if (!colorBlindOn)
            {
                text.text = "";
            }

            else
            {
                text.color = new Color(0, 0, 0, 0);
            }
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

    public IEnumerator Fade(float time, Color targetColor, Color textTargetColor, bool fadeBack)
    {
        float elaspedTime = 0f;

        Color c = meshRenderer.material.color;
        Color textC = new Color(); //stop yelling at me
        if (text != null)
        {
            textC = text.color;
        }

        while (elaspedTime < time)
        {
            float t = elaspedTime / time;
            SetMaterialColor(Color.Lerp(c, targetColor, t));

            if (text != null && colorBlindOn)
            {
                SetTextColor(Color.Lerp(textC, textTargetColor, t));
            }

            elaspedTime += Time.deltaTime;
            yield return null;
        }

        SetMaterialColor(targetColor);
        SetTextColor(textTargetColor);

        if (fadeBack)
        {
            yield return Fade(time, Orange, textC, false);
        }
    }

    public void SetRed(bool t, Color32 orangeColor)
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

    private void SetTextColor(Color32 color)
    {
        if (text != null)
        {
            text.color = color;
        }
    }

    void SetMaterialColor(Color c)
    {
        meshRenderer.material.color = c;
    }

}
