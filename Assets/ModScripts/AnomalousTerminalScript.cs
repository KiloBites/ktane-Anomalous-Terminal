using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Random;
using static UnityEngine.Debug;

public class AnomalousTerminalScript : MonoBehaviour 
{

	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMBombModule Module;
	public KMColorblindMode Colorblind;

	public VCRDisplay MainVCRDisplay;
	public TerminalBIOS Bios;
	public Motherboard Board;
	public Material[] ScreenColorMats;
	public MeshRenderer ModuleScreen;

	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved;

	void Awake()
    {
		moduleId = moduleIdCounter++;

		Module.OnActivate += Activate;

		var motherboardScale = Board.transform.localScale;

		Board.transform.localScale = new Vector3(motherboardScale.x, 0.05f, motherboardScale.z);
    }

	void Activate()
	{
        ModuleScreen.material = ScreenColorMats[0];
        MainVCRDisplay.KillTexts();
		Bios.InitializeBIOS(false);
	}

	
	void Start()
    {
		ModuleScreen.material = ScreenColorMats[1];
		MainVCRDisplay.InitializeStartup();
    }

	public void DoLog(string msg) => Log($"[Anomalous Terminal #{moduleId}] {msg}");

	// Twitch Plays


#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} something";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string command)
    {
		string[] split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		yield return null;
    }

	IEnumerator TwitchHandleForcedSolve()
    {
		yield return null;
    }


}