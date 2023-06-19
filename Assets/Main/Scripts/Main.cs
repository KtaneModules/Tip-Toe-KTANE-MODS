using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class Main : MonoBehaviour
{
	[SerializeField]
	KMBombInfo Bomb;
	[SerializeField]
	KMAudio Audio;

	#region Edgework
	int psPortNum;
	int rcaPortNum;
	int dviPortNum;
	int rjPortNum;
	int parallelPortNum;
	int serialPortNum;

	#endregion
	KMSelectable[] buttons;
	Cell[,] Grid;

	static int ModuleIdCounter = 1;
	int ModuleId;
	private bool ModuleSolved;

	void Awake()
	{
		buttons = GetComponent<KMSelectable>().Children;
		Grid = new Cell[10, 10];

		for (int i = 0; i < 100; i++)
		{
			int row = i / 10;
			int col = i % 10;
			Grid[row, col] = new Cell(row, col, buttons[i]);

		}
	}

	void GetEdgework()
    {
		List<string> ports = Bomb.GetPorts().ToList();

		psPortNum = ports.Count(x => x.ToUpper() == "PS2");
		rcaPortNum = ports.Count(x => x.ToUpper() == "STEREORCA");
		dviPortNum = ports.Count(x => x.ToUpper() == "DVI");
		rjPortNum = ports.Count(x => x.ToUpper() == "RJ45");
		parallelPortNum = ports.Count(x => x.ToUpper() == "PARALLEL");
		serialPortNum = ports.Count(x => x.ToUpper() == "SERIAL");

	}

	void SetSafeRow1()
	{
	}
	void Start()
	{
		GetEdgework();

		Debug.Log("PS num " + psPortNum);
		Debug.Log("rca num " + rcaPortNum);
		Debug.Log("dvid num " + dviPortNum);
		Debug.Log("rj num " + rjPortNum);
		Debug.Log("parallel num " + parallelPortNum);
		Debug.Log("serial num " + serialPortNum);
	}

	void Update()
	{

	}

	void Logging(string log)
    {
		if (log == "")
		{
			return;
		}

		Debug.LogFormat($"[Tip Toe #{ModuleId}] {log}");
	}

#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string Command)
	{
		yield return null;
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		yield return null;
	}
}
