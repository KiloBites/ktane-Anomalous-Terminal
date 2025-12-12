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

	public KMSelectable StatusLightButton;

	public Transform EntireModule, Main;

	public VCRDisplay MainVCRDisplay;
	public TerminalBIOS Bios; // This is temporary. This will be replaced with a Terminal class field once everything is set.
	public Motherboard Board;
	public Material[] ScreenColorMats;
	public MeshRenderer ModuleScreen;

	public AudioClip[] GlitchyShit;

	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved;

	private Coroutine rising, wiggling;

	void Awake()
    {
		moduleId = moduleIdCounter++;

		Module.OnActivate += Activate;

		StatusLightButton.OnInteract += () => { StatusPress(); return false; };

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

	void StatusPress()
	{
		StatusLightButton.AddInteractionPunch(0.4f);

		if (moduleSolved)
			return;
	}

	IEnumerator Rise(bool firstTime)
	{
		var oldPos = EntireModule.localPosition;
		var newPos = new Vector3(oldPos.x, firstTime ? 0.03f : 0.01f * 2, oldPos.z); // Todo: Change the constant value of the otherwise ternary operator condition to use ProgressCount() from the terminal class.

		var duration = 1.5f;
		var elapsed = 0f;

		if (wiggling == null)
			wiggling = StartCoroutine(Wiggle());

		while (elapsed < duration)
		{
			EntireModule.localPosition = new Vector3(oldPos.x, Easing.InOutSine(elapsed, oldPos.y, newPos.y, duration), oldPos.z);
			yield return null;
			elapsed += Time.deltaTime;
		}

		EntireModule.localPosition = newPos;
		rising = null;
	}

	IEnumerator Wiggle()
	{
		float speed1 = 2, speed2 = 1, speed3 = Mathf.PI * 2f / 3f, maxAngle = 2.5f, variance = 0.5f;

		speed1 += Range(-variance, variance);
		speed2 += Range(-variance / 2, variance / 2);
		speed3 += Range(-variance, variance);

		while (true)
		{
			EntireModule.transform.localEulerAngles = new Vector3(Mathf.Sin((speed1 / 4) * Time.time) * maxAngle, Mathf.Sin((speed2 / 4) * Time.time) * maxAngle, Mathf.Sin((speed3 / 4) * Time.time) * maxAngle);
			yield return null;
		}
	}

	public void DoLog(string msg) => Log($"[Anomalous Terminal #{moduleId}] {msg}");

	public void DoCreepyShit()
	{
		var randomClip = GlitchyShit.PickRandom();

		Audio.PlaySoundAtTransform(randomClip.name, transform);

		MainVCRDisplay.InitializeGlitch(randomClip.length, ModuleScreen);
	}

	public void CloseUpModule(List<List<int>> numberPairs, bool[] numbersToCheck)
	{

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