using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
	public Terminal Terminal;
	public Motherboard Board;
	public Material[] ScreenColorMats;
	public MeshRenderer ModuleScreen;

	public AudioClip[] GlitchyShit;
	public AudioClip GlitchyErrorSound;

	public ParticleSystem Particles;

	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved;

	[NonSerialized]
	public bool IsModuleFocused;

	private bool motherboardOpen;

	private Coroutine rising, floating, dropping;

	private Vector3 originalModulePosition, motherboardScale;
	private Vector3 originalMainPosition, newMainPosition;

	void Awake()
    {
		moduleId = moduleIdCounter++;

		Module.OnActivate += Activate;

		Module.GetComponent<KMSelectable>().OnFocus += () => { IsModuleFocused = true; };
		Module.GetComponent<KMSelectable>().OnDefocus += () => { IsModuleFocused = false; };

		StatusLightButton.OnInteract += () => { StatusPress(); return false; };

		motherboardScale = Board.transform.localScale;

		Board.transform.localScale = new Vector3(motherboardScale.x, 0.05f, motherboardScale.z);

		originalModulePosition = EntireModule.localPosition;
		originalMainPosition = Main.localPosition;
		newMainPosition = new Vector3(originalMainPosition.x, 0.02f, originalMainPosition.z);
    }

	void Activate()
	{
        ModuleScreen.material = ScreenColorMats[0];
        MainVCRDisplay.KillTexts();
		Terminal.Bios.InitializeBIOS(false);
		Terminal.WaitForStartup();
	}

	
	void Start()
    {
		ModuleScreen.material = ScreenColorMats[1];
		MainVCRDisplay.InitializeStartup();
		Terminal.GeneratePrograms();
    }

	void StatusPress()
	{
		StatusLightButton.AddInteractionPunch(0.4f);
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.SelectionTick, StatusLightButton.transform);

		if (moduleSolved || !Terminal.AllProgramsCompleted() || motherboardOpen || Terminal.WaitForBootup != null)
            return;

		ToggleTheMotherboard(true);
	}

	public void RaiseModule(bool firstTime) => rising = StartCoroutine(Rise(firstTime));

	IEnumerator Rise(bool firstTime)
	{
		var oldPos = EntireModule.localPosition;
		var newPos = new Vector3(oldPos.x, firstTime ? 0.03f : 0.01f * Terminal.ProgressCount(), oldPos.z);

		var duration = 1.5f;
		var elapsed = 0f;

        if (floating != null)
        {
            StopCoroutine(floating);
            floating = null;
		}

		while (elapsed < duration)
		{
			EntireModule.localPosition = new Vector3(oldPos.x, Easing.InOutSine(elapsed, oldPos.y, newPos.y, duration), oldPos.z);
			yield return null;
			elapsed += Time.deltaTime;
		}
        floating = StartCoroutine(Floating());
        rising = null;
	}

	IEnumerator Floating()
	{
		while (true)
		{
			var oldPos = EntireModule.localPosition;
			var newPos = new Vector3(oldPos.x, oldPos.y - 0.01f, oldPos.z);

			var elapsed = 0f;
			var duration = 1.5f;

			while (elapsed < duration)
			{
				EntireModule.localPosition = new Vector3(newPos.x, Easing.InOutSine(elapsed, oldPos.y, newPos.y, duration), newPos.z);
				yield return null;
				elapsed += Time.deltaTime;
			}
			elapsed = 0;

			while (elapsed < duration)
			{
				EntireModule.localPosition = new Vector3(oldPos.x, Easing.InOutSine(elapsed, newPos.y, oldPos.y, duration), oldPos.z);
                yield return null;
                elapsed += Time.deltaTime;
            }
		}
	}

	public void SolveModule()
	{
		moduleSolved = true;
		Module.HandlePass();
	}

	public void ToggleTheMotherboard(bool opening)
	{
		if (opening)
			dropping = StartCoroutine(DoDrop());
		else
			rising = StartCoroutine(ToggleMotherboard(false));
	}

	IEnumerator DoDrop()
	{
		if (rising != null)
		{
			StopCoroutine(rising);
			rising = null;
		}
		if (floating != null)
		{
			StopCoroutine(floating);
			floating = null;
		}

		if (EntireModule.localPosition != originalModulePosition)
            rising = StartCoroutine(DropPositionBackIntoPlace());


        yield return new WaitUntil(() => rising == null);

		rising = StartCoroutine(ToggleMotherboard(true));
	}

	IEnumerator DropPositionBackIntoPlace()
	{
		Terminal.ToggleMenu(false);
		ModuleScreen.material = ScreenColorMats[0];

		var oldPos = EntireModule.localPosition;

		var duration = 0.85f;
		var elapsed = 0f;

		while (elapsed < duration)
		{
            EntireModule.localPosition = new Vector3(oldPos.x, Easing.InOutSine(elapsed, oldPos.y, originalModulePosition.y, duration), oldPos.z);
			yield return null;
			elapsed += Time.deltaTime;
        }

		EntireModule.localPosition = originalModulePosition;
		Audio.PlaySoundAtTransform("LandingImpact", transform);
        Particles.Emit(5);
        yield return new WaitForSeconds(1.5f);
		rising = null;
	}

	IEnumerator ToggleMotherboard(bool opening)
	{
		if (opening)
		{
            Audio.PlaySoundAtTransform("GlitchOpening", transform);
			MainVCRDisplay.Transition(ModuleScreen);
        }

		var elapsed = 0f;
		var duration = opening ? 10f : 3f;

		while (elapsed < duration)
		{
			if (opening)
			{
				Main.localPosition = new Vector3(originalMainPosition.x, Easing.InCirc(elapsed, originalMainPosition.y, newMainPosition.y, duration), originalMainPosition.z);
				Main.localEulerAngles = new Vector3(Easing.InOutQuint(elapsed, 0, -90, duration), 0, 0);
				Board.transform.localScale = new Vector3(0.1f, Easing.InCirc(elapsed, 0.05f, 0.1f, duration), 0.1f);
			}
			else
			{
				Main.localPosition = new Vector3(originalMainPosition.x, Easing.InCirc(elapsed, newMainPosition.y, originalMainPosition.y, duration), originalMainPosition.z);
				Main.localEulerAngles = new Vector3(Easing.InOutQuint(elapsed, -90, 0, duration), 0, 0);
                Board.transform.localScale = new Vector3(0.1f, Easing.InCirc(elapsed, 0.1f, 0.05f, duration), 0.1f);
            }
			yield return null;
			elapsed += Time.deltaTime;
		}
		Main.localPosition = opening ? newMainPosition : originalMainPosition;
		Main.localEulerAngles = opening ? new Vector3(-90, 0, 0) : Vector3.zero;
		Board.transform.localScale = new Vector3(0.1f, opening ? 0.1f : 0.05f, 0.1f);

		if (!opening)
		{
			Audio.PlaySoundAtTransform("ModuleClose", transform);
			yield return new WaitForSeconds(2);
			Terminal.Bios.InitializeBIOS(true, Board.InputSequences, Board.PinPairsShortedCorrectly);
			Terminal.WaitForStartup();
			motherboardOpen = false;
		}
		else
		{
			ModuleScreen.material = ScreenColorMats[0];
            Board.SetupBoard();
			MainVCRDisplay.EndTransition();
			motherboardOpen = true;
        }
			

			rising = null;
	}


	public void DoLog(string msg) => Log($"[Anomalous Terminal #{moduleId}] {msg}");

	public void DoCreepyShit()
	{
		var randomClip = GlitchyShit.PickRandom();

		Audio.PlaySoundAtTransform(randomClip.name, transform);

		MainVCRDisplay.InitializeGlitch(randomClip.length, ModuleScreen);
	}

	public void CreepyErrorShit(bool caught)
	{
		Audio.PlaySoundAtTransform(GlitchyErrorSound.name, transform);
		MainVCRDisplay.InitializeError(caught, GlitchyErrorSound.length, ModuleScreen);
	}


}