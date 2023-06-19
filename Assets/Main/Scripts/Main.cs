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

	int aBatteryCount;
	int dBatteryCount;

	int batteryCount;
	int holderCount;

	string serialNumber;
	List<int> serialNumberDigits;
	List<char> serialNumberLetters;

	List<string> indicators;
	List<string> litIndicators;
	List<string> unlitIndicators;


	#endregion
	KMSelectable[] buttons;
	Cell[,] Grid;

	List<KeyValuePair<string, HighLow>> row6List;

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
		batteryCount = Bomb.GetBatteryCount();
		holderCount = Bomb.GetBatteryHolderCount();

		aBatteryCount = 2 * (batteryCount - holderCount);
		dBatteryCount = 2 * holderCount - batteryCount;


		serialNumber = Bomb.GetSerialNumber().ToUpper();
		serialNumberDigits = Bomb.GetSerialNumberNumbers().OrderBy(x => x).ToList();

		serialNumberLetters = Bomb.GetSerialNumberLetters().ToList();

		indicators = Bomb.GetIndicators().OrderBy(x => x).ToList();

		litIndicators = Bomb.GetOnIndicators().ToList();
		unlitIndicators = Bomb.GetOffIndicators().ToList();

		List<string> ports = Bomb.GetPorts().ToList();

		psPortNum = ports.Count(x => x.ToUpper() == "PS2");
		rcaPortNum = ports.Count(x => x.ToUpper() == "STEREORCA");
		dviPortNum = ports.Count(x => x.ToUpper() == "DVI");
		rjPortNum = ports.Count(x => x.ToUpper() == "RJ45");
		parallelPortNum = ports.Count(x => x.ToUpper() == "PARALLEL");
		serialPortNum = ports.Count(x => x.ToUpper() == "SERIAL");

	}


	void Start()
	{
		GetEdgework();

		SetSafeRow1(); //9
		SetConditionTrue(8); //8
		SetSafeRow3(); //7
		SetConditionTrue(6); //6
		SetSafeRow5(); //5
		SetSafeRow6(); //4

		PrintGrid();
	}

	private void PrintGrid()
	{
		string log = "";
		for (int i = 0; i < 10; i++)
        {
			log += "\n";

			for (int j = 0; j < 10; j++)
			{
				log += Grid[i, j].Condition ? "T " : "F ";
			}
		}

		Logging(log);
	}




	void Update()
	{

	}

	void SetSafeRow1()
	{
		if (rcaPortNum > 1)
		{
			Grid[9, 0].Condition = true;
		}

		if (serialNumberDigits.Last() % 2 == 0)
		{
			Grid[9, 1].Condition = true;
		}

		if (litIndicators.Count > unlitIndicators.Count)
		{

			Grid[9, 2].Condition = true;
		}

		if (dBatteryCount == 0)
		{
			Grid[9, 3].Condition = true;
		}

		if (indicators.Contains("BOB") || indicators.Contains("FRK"))
		{
			Grid[9, 4].Condition = true;
		}

		if (Bomb.GetPortPlates().Any(x => x.Contains("Parallel") && x.Contains("Serial")))
		{
			Grid[9, 5].Condition = true;
		}

		if (psPortNum == 0 && dviPortNum == 0)
		{
			Grid[9, 6].Condition = true;
		}

		if (dBatteryCount > aBatteryCount)
		{
			Grid[9, 7].Condition = true;
		}

		if (serialNumberLetters.Count > 3)
		{
			Grid[9, 8].Condition = true;
		}

		if (serialNumberDigits.Sum() < 10)
		{
			Grid[9, 9].Condition = true;
		}
	}

	void SetSafeRow3()
	{
		List<int> num = new List<int>();

		foreach (char c in serialNumber)
		{
			num.Add(Char.IsDigit(c) ? int.Parse("" + c) : (c - 64) % 10);
		}

		if (num.Distinct().Count() == num.Count)
		{
			num.RemoveAt(5);
		}
		else
        {
			num = num.Distinct().ToList();
		}

		for (int i = 0; i < num.Count; i++)
        {
			Grid[7, GetIndex(num[i])].Condition = true;
        }
	}

	void SetSafeRow5()
	{

		Dictionary<string, int> d = new Dictionary<string, int>()
		{
			{ "PS", psPortNum},
			{ "RCA", rcaPortNum},
			{ "DVI", dviPortNum},
			{ "RJ", rjPortNum},
			{ "PARALLEL", parallelPortNum},
			{ "SERIAL", serialPortNum},
		};

		List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>();



		foreach (KeyValuePair<string, int> kv in d)
		{ 
			if (list.Count == 3)
            {
				break;
            }

			if (kv.Value > 1)
			{
				list.Add(kv);
			}
		}

		row6List = new List<KeyValuePair<string, HighLow>>();

		if (list.Count == 0)
		{
			Grid[5, 0].Condition = Grid[5, 9].Condition = true;
			row6List.Add(new KeyValuePair<string, HighLow>("NONE", new HighLow(0, 9)));
		}

		else
        {
			foreach (KeyValuePair<string, int> kv in list)
            {
				int high = -1;
				int low = -1;
                switch (kv.Key)
                {
					case "PS":
						foreach (int num in serialNumberDigits)
                        {
							Grid[5, GetIndex(num)].Condition = true;
						}
						row6List.Add(new KeyValuePair<string, HighLow>(kv.Key, new HighLow(serialNumberDigits.Last(), serialNumberDigits.First())));
						break;

					case "RCA":
						low = 0;
						high = 8;
						break;
					case "DVI":
						high = 7;
						low = 5;
						break;
					case "RJ":
						high = 6;
						low = 2;
						break;
					case "PARALLEL":
						high = 9;
						low = 3;
						break;
					case "SERIAL":
						high = 4;
						low = 1;
						break;
				}

				Grid[5, high].Condition = Grid[5, low].Condition = true;
				row6List.Add(new KeyValuePair<string, HighLow>(kv.Key, new HighLow(high, low)));
			}
		}
	}

	void SetSafeRow6()
	{
		string firstIndicator = indicators.Count == 0 ? "NONE" : indicators[0];

		foreach (KeyValuePair<string, HighLow> kv in row6List)
		{
			
			switch (firstIndicator)
			{
				case "BOB":
				case "CAR":
				case "FRK":
				case "MSA":
				case "NSA":
				case "TRN":
					Grid[4, kv.Value.Lower].Condition = true;
					break;

				case "CLR":
				case "FRQ":
				case "IND":
				case "SIG":
				case "SND":
				case "NONE":
					Grid[4, kv.Value.Higher].Condition = true;
					break;
			}
		}
	}

	void SetSafeRow7()
	{ 
		
	}

	int GetIndex(int num)
	{
		int col = num - 1;

		if (col == -1)
		{
			col = 9;
		}

		return col;
	}


	void SetConditionTrue(int row)
	{
		for (int i = 0; i < 10; i++)
		{
			Grid[row, i].Condition = true;
		}
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
