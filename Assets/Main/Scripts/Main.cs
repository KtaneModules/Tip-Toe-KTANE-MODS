using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class Main : MonoBehaviour
{
	[SerializeField]
	KMBombInfo Bomb;
	[SerializeField]
	KMAudio Audio;

	private bool ZenModeActive;

	bool colorblindOn = false; //dont want to delete code that does this

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
	List<int> serialNumberAlphaDigits;

	List<string> indicators;
	List<string> litIndicators;
	List<string> unlitIndicators;


	#endregion
	KMSelectable[] buttons;
	Cell[,] Grid;

	Cell currentPos;

	bool row1CellSafe;
	bool row6CellSafe;
	List<int> row9Safe;

	List<KeyValuePair<string, HighLow>> row6List;
	List<Cell> flickeringCells;

	static int ModuleIdCounter = 1;
	int ModuleId;
	private bool ModuleSolved = false;
	int startingMinutes;
	int currentMinutes = -1;

	[SerializeField]
	AudioClip fallingClip;

	bool falling; //if the person got a strike and the falling sound effect is still playing

	private Color32[] orangeColors = new Color32[100];
	private List<Cell> visitedCells = new List<Cell>();
	void Awake()
	{
		ModuleId = ModuleIdCounter++;
		buttons = GetComponent<KMSelectable>().Children;
		Grid = new Cell[10, 10];
		//colorblindOn = GetComponent<KMColorblindMode>().ColorblindModeActive;
		for (int i = 0; i < orangeColors.Length; i++)
        {
			orangeColors[i] = new Color32(255, (byte)Rnd.Range(110, 160), 0, 255);
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
		startingMinutes = (int)Bomb.GetTime() / 60;

		if (ZenModeActive)
        {
			startingMinutes++;
        }

		else
        {
			startingMinutes--;
        }

		GetEdgework();

		ResetModule();

		foreach (KMSelectable s in buttons)
		{ 
			s.OnInteract += delegate () { KeypadPress(s); return false; };
		}

		PrintGrid(true, 6);
	}

	void ResetModule()
	{
		for (int i = 0; i < 100; i++)
		{
			int row = i / 10;
			int col = i % 10;
			Grid[row, col] = new Cell(row, col, buttons[i], colorblindOn);
			Color color = orangeColors[i];
			buttons[i].GetComponent<MeshRenderer>().material.color = color;
			Grid[row, col].Orange = color;

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
		currentPos = new Cell(-1, -1, null, colorblindOn);
		row6CellSafe = false;
		row1CellSafe = false;
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

		falling = false;
	}

	private void PrintGrid(bool before, int rowNum)
	{
		string[,] g1 = new string[10, 10];
		string log = $"Grid {(before ? "before" : "after")} reaching row {rowNum}:\n";
		for (int i = 0; i < 10; i++)
        {
			for (int j = 0; j < 10; j++)
			{
				string answer = Grid[i, j].Safe ? "T " : "F ";
				log += answer;
				g1[i, j] = answer;
			}

			log += "\n";
		}

		Logging(log);

		/*
		if (before)
        {
			string g2 = "FFFFFFFFFF\nFFFFFFFFFF\nTTTTTTTTTT\nFFFFFFFFFF\nTFTFFTFFFF\nTFTFFTTTTF\nTTTTTTTTTT\nTFTFFTFTFT\nTTTTTTTTTT\nFFTTFFFFFT";
			SameGrid(g1, g2);
		}*/

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

		if (ZenModeActive)
        {
			if (minutes <= startingMinutes)
            {
				return;
            }

			if (currentMinutes == -1 || currentMinutes < minutes)
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
		}

		else if (!ZenModeActive)
        {
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
		}

		foreach (Cell c in flickeringCells)
        {
			for (int i = 0; i < c.AlreadyFlickered.Length; i++)
			{
				if (!c.AlreadyFlickered[i] && c.FlickerTimes[i] == seconds)
                {
					FadeWhite(c);
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


		if (int.Parse("" + serialNumber.Last()) % 2 == 0)
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
		serialNumberAlphaDigits = new List<int>();

		foreach (char c in serialNumber)
		{
			serialNumberAlphaDigits.Add(Char.IsDigit(c) ? int.Parse("" + c) : c - 64);
		}

		List<int> list = serialNumberAlphaDigits.Select(x => x % 10).ToList();

		if (list.Distinct().Count() == list.Count)
		{
			list.RemoveAt(5);
		}
		else
        {
			list = list.Distinct().ToList();
		}

		for (int i = 0; i < list.Count; i++)
        {
			Grid[7, GetIndex(list[i])].Safe = true;
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
			row6List.Add(new KeyValuePair<string, HighLow>("NONE", new HighLow(9, 0)));
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

						int[] noZeros = serialNumberDigits.Where(x => x != 0).ToArray();

						high = serialNumberDigits.Contains(0) ? 9 : GetIndex(serialNumberDigits.Max());
						low = serialNumberDigits.Contains(1) ? 0 : GetIndex(noZeros.Min());
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
		
		for (int i = 0; i < serialNumberAlphaDigits.Count; i++)
        {
			int digit = serialNumberAlphaDigits[i];

			if (digit % 3 == 0)
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

			else if (digit % 2 == 0)
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

		if (ModuleSolved || falling)
        {
			return;
        }
		Cell c = GetCell(s);

		int ix = c.Row * 10 + c.Col;

		string log = $"Pressed {c}.";

		//if player is not set, check to see if cell pressed is in the first row
		if (currentPos.Row == -1 && currentPos.Col == -1)
        {
			if (c.Row != 9)
			{
				log += " This is not in the first row. Strike!";
				Logging(log);
				Strike(ix, false);
				return;
			}

			//check cell is safe
			if (!c.Safe)
            {
				log += " This is not safe. Strike!";
				Logging(log);
				Strike(ix, true);
				return;
			}

			currentPos = c;
			visitedCells.Add(c);
			Logging(log);

			return;

		}

		//check cell is safe
		if (!c.Safe)
		{
			log += " This is not safe. Strike!";
			Logging(log);
			Strike(ix, true);
			return;
		}

		//if the pressed square is in the first row, and the player is in the first row, then it's valid
		if (currentPos.Row == 9 && c.Row == 9)
        {
			visitedCells.Add(c);
			currentPos = c;
			Logging(log);
			return;
		}

		//check to see if the cell is orthongal from current pos
		if (!currentPos.Adjacent(c))
        {
			log += " This is not adjacent. Strike!";
			Logging(log);
			Strike(ix, false);
			return;
		}

		visitedCells.Add(c);
		currentPos = c;
		Logging(log);

		//if current cell is in 6th row, make all cells in that row safe
		if (!row6CellSafe && c.Row == 4)
        {
			row6CellSafe = true;
			SetConditionTrue(4);
			SetSafeRow7();
			
			PrintGrid(false, 6);
		}

		//current cell is in 10th row solve module
		if (c.Row == 0)
        {
			Solve();
		}

	}

	void FadeWhite(Cell c)
    {
		StartCoroutine(c.Fade(1, c.White, Color.black, true));
	}

	IEnumerator FlickerRed(Cell c)
    {
		c.SetRed(true, orangeColors[c.Row * 10 + c.Col]);
		yield return new WaitForSeconds(1f);
		c.SetRed(false, orangeColors[c.Row * 10 + c.Col]);
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

	void Strike(int ix, bool adjacent)
	{
		visitedCells = new List<Cell>();

		if (adjacent)
		{
			StartCoroutine(FallStrike(ix));
			
		}

		else
		{
			StartCoroutine(RedStrike(ix));
		}
	}

	IEnumerator FallStrike(int ix)
    {
		falling = true;
		Cell c = Grid[ix / 10, ix % 10];
		StartCoroutine(c.Fade(fallingClip.length, Color.black, Color.white, false));
		Audio.PlaySoundAtTransform(fallingClip.name, transform);
		yield return new WaitForSeconds(fallingClip.length);
		GetComponent<KMBombModule>().HandleStrike();
        ResetModule();
	}

	IEnumerator RedStrike(int ix)
    {
		falling = true;
		yield return FlickerRed(Grid[ix / 10, ix % 10]);
		GetComponent<KMBombModule>().HandleStrike();
        ResetModule();
	}

	void Solve()
    {
		Audio.PlaySoundAtTransform("SolveSound", transform);
		StartCoroutine(ShowPathToSolve());
		GetComponent<KMBombModule>().HandlePass();
		ModuleSolved = true;
	}

	private IEnumerator ShowPathToSolve()
    {
		var timeBetweenEach = 3.3f / visitedCells.Count;
		for (int i = 0; i < visitedCells.Count; i++)
		{
			visitedCells[i].SetGreen();
			yield return new WaitForSeconds(timeBetweenEach);
		}
		for (int i = 0; i < 10; i++)
			for (int j = 0; j < 10; j++)
			Grid[i, j].SetGreen();
		yield break;
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
	private readonly string TwitchHelpMessage = @"Use `!{0} row col` to press the button at that location. BL square is considered `1 1` while TR square is considered `10 0`. Tiles can be chained via commas. EX: `1 1, 1 2`";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string Command)
	{
        string[] commands = Command.Trim().Split(',');
		commands = commands.Select(x => x.Trim()).ToArray();
		
		string message = ValidCommands(commands);

		if (message != "")
		{
			yield return message;
			yield break;
        }
        else
        {
			for (int i = 0; i < commands.Length; i++)
			{
				string[] command = commands[i].Trim().Split(' ');
                string[] previousCommand = i == 0 ? null : commands[i - 1].Trim().Split(' ');

                Cell c = FindCellToPress(int.Parse("" + command[0]), int.Parse("" + command[1]));
                KeypadPress(c.Button);

                if (!c.Safe)
				{
                    if ((previousCommand == null || ManhattenDistance(command, previousCommand) == 1))
                    {
                        yield return new WaitForSeconds(fallingClip.length); //wait for long strike sound
                    }

                    yield return new WaitForSeconds(.1f);
                    yield return $"sendtochat Command that caused strike was \"{string.Join(" ", command)}\"";
                    yield break;
                }

				yield return new WaitForSeconds(.1f);
            }
        }

		yield return null;
	}

	private int ManhattenDistance(string[] command1, string[] command2) 
	{
		return Math.Abs(int.Parse("" + command1[0]) - int.Parse("" + command2[0])) + Math.Abs(int.Parse("" + command1[1]) - int.Parse("" + command2[1]));
    }
	private string ValidCommands(string[] commands) 
	{
		foreach (string s in commands) 
		{
			string[] command = s.Trim().Split(' ');

            if (command.Length != 2)
            {
                return $"sendtochaterror Invalid amount of numbers given for coordinate (Given \"{commands}\").";
            }

            foreach (string n in command)
            {
                int num;

                if (!int.TryParse(n, out num))
                {
                    return $"sendtochaterror Commands contains characters that are not numbers (Given \"{commands}\").";
                }
            }

            int row = int.Parse(command[0]);
            int col = int.Parse(command[1]);

            if (row < 1 || row > 10)
            {
                return $"sendtochaterror Row needs to be bewtween 1 and 10 inclusively (Given \"{commands}\").";
            }

            if (col < 0 || col > 9)
            {
                return $"sendtochaterror Column needs to be bewtween 0 and 9 inclusively (Given \"{commands}\").";
            }

            if (FindCellToPress(row, col) == null)
            {
                return $"sendtochaterror Could not process command. Please contact developer. (Given \"{commands}\")";
            }
        }

        return "";
	}

	Cell FindCellToPress(int row, int col)
    {
		foreach (Cell c in Grid)
        {
			string[] s = c.ToString().Replace("(","").Replace(")", "").Split(',');

			Debug.Log(string.Join(" ", s));

			s[0] = s[0].Trim();
			s[1] = s[1].Trim();

			if ("" + row == s[0] && "" + col == s[1])
            {
				return c;
            }
        }

		return null;
    }

	IEnumerator TwitchHandleForcedSolve()
	{
		yield return null;

		Dictionary<Cell, List<Cell>> d = new Dictionary<Cell, List<Cell>>();

		List <Cell> startingCells = new List<Cell>();
		List<Cell> ending1Cells = new List<Cell>();
		List<Cell> ending2Cells = new List<Cell>();

		for (int i = 0; i < 10; i++)
        {
			if (Grid[9, i].Safe)
            {
				startingCells.Add(Grid[9, i]);
			}

			if (Grid[4, i].Safe)
            {
				ending1Cells.Add(Grid[4, i]);
            }

			if (Grid[0, i].Safe)
            {
				ending2Cells.Add(Grid[0, i]);
			}
		}

		List<Cell> path1 = null;
		List<Cell> path2 = null;

		Cell start1;
		Cell end1 = null;

		for (int i = 0; i < startingCells.Count; i++)
        {
			for (int j = 0; j < ending1Cells.Count; j++)
			{
				start1 = startingCells[i];
				end1 = ending1Cells[i];
				
				path1 = FindPath(start1, end1);

				if (path1 != null)
                {
					break;
                }
			}

			if (path1 != null)
			{
				break;
			}
		}

		if (path1 != null)
        {
			Debug.Log(path1.Count);
			Debug.Log(string.Join(" ", path1.Select(x => x.ToString()).ToArray()));

			foreach (Cell c in path1)
            {
				KeypadPress(c.Button);
				yield return new WaitForSeconds(.1f); //here to make sure row 6 and 7 safe spaces update
			}
		}
		else
        {
			yield return "sendtochaterror An error occured. Please contact developer with log.";
			yield break;
        }

		
		Cell end2;

		for (int i = 0; i < ending2Cells.Count; i++)
        {
			end2 = ending2Cells[i];

			path2 = FindPath(end1, end2);

			if (path2 != null)
			{
				break;
            }
        }

        if (path2 != null)
        {
			path2.RemoveAt(0); //gets rid of space already on
			Debug.Log(path2.Count);
			Debug.Log(string.Join(" ", path2.Select(x => x.ToString()).ToArray()));
			foreach (Cell c in path2)
			{
				KeypadPress(c.Button);
				yield return new WaitForSeconds(.1f); //consistancy
			}
		}
		else
        {
			yield return "sendtochaterror An error occured. Please contact developer with log.";
			yield break;
		}

		while(!ModuleSolved)
        {
			yield return true;
        }
    }

    List<Cell> FindPath(Cell start, Cell end)
    {
		Debug.Log($"Start at " + start.ToString());
		Debug.Log($"End at " + end.ToString());

		SetHeristic(end);

		List<Cell> open = new List<Cell>();
		List<Cell> closed = new List<Cell>();

		start.G = 0;
		
		Cell currentCell = start;

		int count = 0;

		while (!open.Contains(end)) //change to when end is reached
        {
			count++;

			if (count == 100)
            {
				Debug.Log("An infiinte loop has occured");
				break;
            }

			List<Cell> neighbors = new List<Cell>() { currentCell.Up, currentCell.Down, currentCell.Left, currentCell.Right };

			neighbors = GetRidOfBadNeighbors(neighbors, closed);

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

			if (open.Count == 0) //there is no path to end
            {
				return null;
            }
			open = open.OrderBy(x => x.FinalCost).ToList();
			currentCell = open.First();
			//Debug.Log("Cells in open list " + string.Join(" ", open.Select(x => ($"{x.ToString()} F={x.FinalCost}")).ToArray()));
			neighbors.Clear();
		}
	
		if (!open.Contains(end))
        {
			return null;
        }

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
