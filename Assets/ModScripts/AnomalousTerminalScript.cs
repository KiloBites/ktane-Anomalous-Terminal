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

	public VCRDisplay MainVCRDisplay;
	public Material[] ScreenColorMats;
	public MeshRenderer ModuleScreen;

	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved;

	void Awake()
    {

		moduleId = moduleIdCounter++;

    }

	void Activate()
	{
        ModuleScreen.material = ScreenColorMats[0];
        MainVCRDisplay.KillTexts();
	}

	
	void Start()
    {
		ModuleScreen.material = ScreenColorMats[1];
		MainVCRDisplay.InitializeStartup();
    }

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