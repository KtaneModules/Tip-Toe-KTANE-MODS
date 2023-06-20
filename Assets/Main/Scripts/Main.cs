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


	[SerializeField]
	Material white;
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

	Cell currentPos;

	bool row6CellSafe;
	List<int> row9Safe;

	List<KeyValuePair<string, HighLow>> row6List;
	List<Cell> flickeringCells;

	static int ModuleIdCounter = 1;
	int ModuleId;
	private bool ModuleSolved = false;
	int startingMinutes;
	int currentMinutes = -1;
	void Awake()
	{
		ModuleId = ModuleIdCounter++;
		buttons = GetComponent<KMSelectable>().Children;
		Grid = new Cell[10, 10];
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
		startingMinutes = (int)Bomb.GetTime() / 60 - 1;
		GetEdgework();

		ResetModule();

		foreach (KMSelectable s in buttons)
		{ 
			s.OnInteract += delegate () { KeypadPress(s); return false; };
		}

		PrintGrid(true);
	}

	void ResetModule()
	{
		for (int i = 0; i < 100; i++)
		{
			int row = i / 10;
			int col = i % 10;
			Grid[row, col] = new Cell(row, col, buttons[i], white);
		}

		for (int i = 0; i < 10; i++)
        {
			for (int j = 0; j < 10; j++)
            {
				Cell c = Grid[i, j];

				c.Up = i == 0 ? null : Grid[i - 1, j];
				c.Down = i == 9 ? null : Grid[i + 1, j];
				c.Left = j == 0 ? null : Grid[i, j - 1];
				c.Right = j == 9 ? null : Grid[i, j + 1];
			}
        }

		flickeringCells = new List<Cell>();
		currentPos = new Cell(-1, -1, null, null);
		row6CellSafe = false;
		SetSafeRow1(); //9
		SetConditionTrue(8); //8
		SetSafeRow3(); //7
		SetConditionTrue(6); //6
		SetSafeRow5(); //5
		SetSafeRow6(); //4
		SetConditionTrue(2); //2
		SetSafeRow9(); //1
		SetSafeRow10(); //0

		foreach (Cell c in Grid)
        {
			if (c.Flicker)
            {
				flickeringCells.Add(c);
            }
        }

		foreach (Cell c in flickeringCells)
        {
			do
			{
				for (int i = 0; i < c.AlreadyFlickered.Length; i++)
                {
					c.FlickerTimes[i] = SetFlickeringTime(i);
                }

			} while (c.FlickerTimes.Distinct().Count() != c.FlickerTimes.Count());
		}

		Cell cell = flickeringCells[0];

		//Debug.Log($"Flickering at {cell.Row},{cell.Col}");
		//Debug.Log($"Times at {string.Join(" ", cell.FlickerTimes.Select(x => "" + x).ToArray())}");

	}

	private void PrintGrid(bool before)
	{
		string[,] g1 = new string[10, 10];
		string log = $"Grid {(before ? "before" : "after")} reaching row 6";
		for (int i = 0; i < 10; i++)
        {
			log += "\n";

			for (int j = 0; j < 10; j++)
			{
				string answer = Grid[i, j].Safe ? "T " : "F ";
				log += answer;
				g1[i, j] = answer;
			}
		}

		Logging(log);

		if (before)
        {
			string g2 = "FFFFFFFFFF\nFFFFFFFFFF\nTTTTTTTTTT\nFFFFFFFFFF\nFFFFFFTTTF\nTFTFFTTTTF\nTTTTTTTTTT\nFFTTTTFTFF\nTTTTTTTTTT\nFTFFFTFFTF";
			SameGrid(g1, g2);
		}
	}
		

	void SameGrid(string[,] g1, string temp)
	{
		string[] g2 = temp.Split('\n');

		for (int i = 2; i < 10; i++)
        {
			for (int j = 0; j < 10; j++)
            {
				if (g1[i, j].Trim() != "" + g2[i][j])
                {
					Debug.Log($"Different at ({i},{j})");
                }
            }
		}
	}




	void Update()
	{
		if (ModuleSolved)
		{
			return;
		}

		int minutes = (int)Bomb.GetTime() / 60;
		int seconds = (int)Bomb.GetTime() % 60;

		if (minutes >= startingMinutes)
        {
			return;
        }

		if (currentMinutes == -1 || currentMinutes > minutes)
        {
			currentMinutes = minutes; 
			foreach (Cell c in flickeringCells)
            {
				for (int i = 0; i < c.AlreadyFlickered.Length; i++)
                {
					c.AlreadyFlickered[i] = false;
                }
			}
		}

		foreach (Cell c in flickeringCells)
        {
			for (int i = 0; i < c.AlreadyFlickered.Length; i++)
			{
				if (!c.AlreadyFlickered[i] && c.FlickerTimes[i] == seconds)
                {
					StartCoroutine(Flicker(c));
					c.AlreadyFlickered[i] = true;
				}
			}
		}
	}

	void SetSafeRow1()
	{
		if (rcaPortNum > 1)
		{
			Grid[9, 0].Safe = true;
		}

		if (serialNumberDigits.Last() % 2 == 0)
		{
			Grid[9, 1].Safe = true;
		}

		if (litIndicators.Count > unlitIndicators.Count)
		{

			Grid[9, 2].Safe = true;
		}

		if (dBatteryCount == 0)
		{
			Grid[9, 3].Safe = true;
		}

		if (indicators.Contains("BOB") || indicators.Contains("FRK"))
		{
			Grid[9, 4].Safe = true;
		}

		if (Bomb.GetPortPlates().Any(x => x.Contains("Parallel") && x.Contains("Serial")))
		{
			Grid[9, 5].Safe = true;
		}

		if (psPortNum == 0 && dviPortNum == 0)
		{
			Grid[9, 6].Safe = true;
		}

		if (dBatteryCount > aBatteryCount)
		{
			Grid[9, 7].Safe = true;
		}

		if (serialNumberLetters.Count == 3)
		{
			Grid[9, 8].Safe = true;
		}

		if (serialNumberDigits.Sum() < 10)
		{
			Grid[9, 9].Safe = true;
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
			Grid[7, GetIndex(num[i])].Safe = true;
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

		List<string> list = new List<string>();



		foreach (KeyValuePair<string, int> kv in d)
		{ 
			if (list.Count == 3)
            {
				break;
            }

			if (kv.Value > 0)
			{
				list.Add(kv.Key);
			}
		}

		row6List = new List<KeyValuePair<string, HighLow>>();

		if (list.Count == 0)
		{
			Grid[5, 0].Safe = Grid[5, 9].Safe = true;
			row6List.Add(new KeyValuePair<string, HighLow>("NONE", new HighLow(0, 9)));
		}

		else
        {
			foreach (string s in list)
            {
				int high = -1;
				int low = -1;
                switch (s)
                {
					case "PS":
						foreach (int num in serialNumberDigits)
                        {
							Grid[5, GetIndex(num)].Safe = true;
						}

						high = GetIndex(serialNumberDigits.Last());
						low = GetIndex(serialNumberDigits.First());
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
				Grid[5, high].Safe = Grid[5, low].Safe = true;
				row6List.Add(new KeyValuePair<string, HighLow>(s, new HighLow(high, low)));
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
					Grid[4, kv.Value.Lower].Safe = true;
					break;

				case "CLR":
				case "FRQ":
				case "IND":
				case "SIG":
				case "SND":
				case "NONE":
					Grid[4, kv.Value.Higher].Safe = true;
					break;
			}
		}
	}

	void SetSafeRow7()
	{
		Cell c = currentPos;
		
		for (int i = 0; i < serialNumber.Length; i++)
        {
			if ((c.Col + 1) % 10 % 3 == 0)
            {
				if (c.Col == 0)
                {
					c = Grid[4, 9];
                }
				else
                {
					c = Grid[4, c.Col - 1];
                }
            }

			else if ((c.Col + 1) % 10 % 2 == 0)
            {
				c = Grid[4, (c.Col + 1) % 10];
            }

			else
            {
				Grid[3, c.Col].Safe = true;
				return;
            }
		}

		Grid[3, c.Col].Safe = true;
	}

	void SetSafeRow9()
	{
		row9Safe = new List<int>();

		do
		{
			row9Safe.Add(Rnd.Range(0, 10));
		} while (row9Safe.Distinct().Count() != 5);

		row9Safe = row9Safe.Distinct().ToList();

		for (int i = 0; i < 10; i++)
        {
			if (row9Safe.Contains(i))
            {
				Grid[1, i].Safe = true;
            }
			else
            {
				Grid[1, i].Flicker = true;
			}
		}
	}

	void SetSafeRow10()
	{
		List<int> list = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };

		foreach (int i in row9Safe)
        {
			list.Remove(i);
        }

		//There will only be one set of vertically-adjacent squares in rows 9 and 10 that are safe.
		int index = row9Safe[Rnd.Range(0, row9Safe.Count)];

		while (list.Count != 2)
        {
			list.RemoveAt(Rnd.Range(0, list.Count));
        }

		list.Add(index);

		for (int i = 0; i < 10; i++)
		{
			if (list.Contains(i))
			{
				Grid[0, i].Safe = true;
			}
			else
			{
				Grid[0, i].Flicker = true;
			}
		}
	}

	int SetFlickeringTime(int range)
	{
		int num;
		switch (range)
		{
			case 0:
				num = Rnd.Range(0, 15);
				break;
			case 1:
				num = Rnd.Range(15, 30);
				break;
			case 2:
				num = Rnd.Range(30, 45);
				break;
			default:
				num = Rnd.Range(45, 60);
				break;
		}

		return num;
	}

	void KeypadPress(KMSelectable s)
	{
		s.AddInteractionPunch(.5f);
		if (ModuleSolved)
        {
			return;
        }
		Cell c = GetCell(s);

		string log = $"Pressed {c}.";

		//string log = $"Pressed ({(c.Row + 1) % 10},{(c.Col + 1) % 10}).";

		//if player is not set, check to see if cell pressed is in the first row
		if (currentPos.Row == -1 && currentPos.Col == -1)
        {
			if (c.Row != 9)
			{
				log += " This is not in the first row. Strike!";
				Logging(log);
				Strike();
				return;
			}

			//check cell is safe
			if (!c.Safe)
            {
				log += " This is not safe. Strike!";
				Logging(log);
				Strike();
				return;
			}

			currentPos = c;
			Logging(log);
			return;
		}

		//check to see if the cell is orthongal from current pos
		if (!currentPos.Adjacent(c))
        {
			log += " This is not adjacent. Strike!";
			Logging(log);
			Strike();
			return;
		}

		//check cell is safe
		if (!c.Safe)
		{
			log += " This is not safe. Strike!";
			Logging(log);
			Strike();
			return;
		}

		currentPos = c;
		Logging(log);

		//if current cell is in 6th row, make all cells in that row safe
		if (!row6CellSafe && c.Row == 4)
        {
			row6CellSafe = true;
			SetConditionTrue(4);
			SetSafeRow7();
			
			PrintGrid(false);
		}

		//current cell is in 10th row solve module
		if (c.Row == 0)
        {
			Solve();
		}

	}

	IEnumerator Flicker(Cell c)
    {
		c.SetWhite(true);
		yield return new WaitForSeconds(1f);
		c.SetWhite(false);
	}

	Cell GetCell(KMSelectable s)
	{
		foreach (Cell c in Grid)
		{ 
			if (c.Button == s)
            {
				return c;
            }

		}

		return null;
	}

	void Strike()
	{
		GetComponent<KMBombModule>().HandleStrike();
		ResetModule();
	}

	void Solve()
    {
		GetComponent<KMBombModule>().HandlePass();
		ModuleSolved = true;
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
			Grid[row, i].Safe = true;
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
	private readonly string TwitchHelpMessage = @"Use `!{0} row col` to press the button at that location. Only can process one press per command";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string Command)
	{
		string[] commands = Command.Trim().Split(' ');
		yield return null;

		if (commands.Length != 2)
		{
			yield return "sendtochaterror Invalid amount of commands.";
			yield break;
		}

		foreach (string s in commands)
        {
			int num;

			bool b = int.TryParse(s, out num);

			if (!b)
            {
				yield return "sendtochaterror Commands contains characters that are not numbers.";
				yield break;
			}
			else if(num < 0 || num > 9)
            {
				yield return "sendtochaterror Commands contains numbers that are less than 0 or greater than 9.";
				yield break;
			}
		}

		int row = int.Parse(commands[0]) - 1 == -1 ? 9 : int.Parse(commands[0]) - 1;
		int col = int.Parse(commands[1]) - 1 == -1 ? 9 : int.Parse(commands[1]) - 1;

		KeypadPress(Grid[row, col].Button);
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		yield return null;

		Dictionary<Cell, List<Cell>> d = new Dictionary<Cell, List<Cell>>();

		List<int> startingCells = new List<int>();
		List<int> endingCells = new List<int>();
		for (int i = 0; i < 10; i++)
        {
			if (Grid[9, i].Safe)
            {
				startingCells.Add(i);
			}

			if (Grid[4, i].Safe)
            {
				endingCells.Add(i);
            }
        }

		//int startingCellIndex = -1;
		//int endingCellIndex = -1;

		List<Cell> path = null;

		path = FindPath(Grid[9, 1], Grid[4, 6]);

		/*
		 		for (int i = 0; i < startingCells.Count; i++)
        {
			for (int j = 0; j < endingCells.Count; j++)
			{
				Cell start = Grid[9,i];
				Cell end = Grid[4, j];

				Debug.Log("this for loop is being seen");
				path = FindPath(start, end);

				if (path != null)
                {
					break;
                }
			}

			if (path != null)
			{
				break;
			}
		}
		 */


		if (path != null)
        {
			Debug.Log(path.Count);
			Debug.Log(string.Join(" ", path.Select(x => x.ToString()).ToArray()));
		}
		else
        {
			Debug.Log("Bullshit has happen that has lead a path to not be found");
        }

	}

	List<Cell> FindPath(Cell start, Cell end)
    {
		SetHeristic(end);

		List<Cell> open = new List<Cell>();
		List<Cell> closed = new List<Cell>();

		start.G = 0;
		
		Cell currentCell = start;
		closed.Add(start);

		int count = 0;

		while (!open.Contains(end)) //change to when end is reached
        {
			count++;

			if (count == 100)
            {
				Debug.Log("An infiinte loop has occured");
				break;
            }

			Debug.Log("Current cell is " + currentCell.ToString());

			List<Cell> neighbors = new List<Cell>();

			neighbors.Add(currentCell.Up);
			neighbors.Add(currentCell.Left);
			neighbors.Add(currentCell.Down);
			neighbors.Add(currentCell.Right);


			Debug.Log("BEFORE neighbors are " + string.Join(" ", neighbors.Select(x => x == null ? "poop" : x.ToString()).ToArray()));

			neighbors = GetRidOfBadNeighbors(neighbors, closed);

			Debug.Log("neighbors are " + string.Join(" ", neighbors.Select(x => x.ToString()).ToArray()));

			foreach (Cell c in neighbors)
            {
				if (!open.Contains(c))
                {
					c.Parent = currentCell;
					open.Add(c);
					c.G = c.Parent.G + 1;
					c.FinalCost = c.G + c.Heuristic;
                }

				else
                {
					int potentialCost = c.Parent.G + 1;
					if (c.FinalCost > potentialCost)
                    {
						c.Parent = currentCell;
						c.G = potentialCost;
						c.FinalCost = c.G + c.Heuristic;
					}
				}
            }

			open.Remove(currentCell);
			closed.Add(currentCell);

			currentCell = open.OrderBy(x => x.FinalCost).ToList().Last();
			Debug.Log("Cells in open list " + string.Join(" ", open.Select(x => x.ToString()).ToArray()));
			neighbors.Clear();
		}
	
		if (!open.Contains(end))
        {
			return null;
        }

		Debug.Log($"Start at " + start.ToString());
		Debug.Log($"End at " + end.ToString());

		List<Cell> path = new List<Cell>();

		Cell current = end;
		while (!path.Contains(start))
        {
			path.Add(current);
			current = current.Parent;
        }
		path.Reverse();

		return path;
	}

	void SetHeristic(Cell end)
    {
		foreach (Cell c in Grid)
        {
			c.Heuristic = Math.Abs(end.Row - c.Row) + Math.Abs(end.Col - c.Col);
			c.Parent = null;
			c.FinalCost = 0;
			c.G = 0;
        }
    }

	List<Cell> GetRidOfBadNeighbors(List<Cell> list, List<Cell> closed)
    {
		List<Cell> newList = new List<Cell>();

		foreach (Cell c in list)
		{ 
			if (c != null && !closed.Contains(c) && c.Safe)
            {
				newList.Add(c);
            }
		}
		
		return newList;
	}

}
