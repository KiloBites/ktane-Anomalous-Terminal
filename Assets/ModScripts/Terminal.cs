using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class Terminal : MonoBehaviour
{
    private List<SoftwareProgram> programs;

    public ColorCycleDisplay ColorCycle;
    public HexadecimalCycleDisplay HexCycle;
    public TemperatureCheckDisplay TempCheck;
    public PatternIntegrityDisplay PatternIntegrity;
    public SacrificialHouseDisplay SacrificialHouse;
    public RootedPasswordDisplay RootedPassword;
    public MainMenu MainMenu;
    public AnomalousTerminalScript Module;
    public TerminalBIOS Bios;

    [NonSerialized]
    public Coroutine CreepyShit;

    [NonSerialized]
    public Coroutine WaitForBootup;

    private List<GameObject> programObjects;

    private List<GameObject> ObtainObjects()
    {
        var finalList = new List<GameObject>();

        foreach (var program in programs)
        {
            switch (program.ProgramType)
            {
                case SoftwareProgramType.ColorCycle:
                    finalList.Add(ColorCycle.gameObject);
                    break;
                case SoftwareProgramType.HexCycle:
                    finalList.Add(HexCycle.gameObject);
                    break;
                case SoftwareProgramType.TempCheck:
                    finalList.Add(TempCheck.gameObject);
                    break;
                case SoftwareProgramType.PatternIntegrity:
                    finalList.Add(PatternIntegrity.gameObject);
                    break;
                case SoftwareProgramType.SacrificialHouse:
                    finalList.Add(SacrificialHouse.gameObject);
                    break;
                case SoftwareProgramType.RootedPassword:
                    finalList.Add(RootedPassword.gameObject);
                    break;
            }
        }

        return finalList;
    }

    private bool[] programsCompleted = new bool[4];

    public int ProgressCount() => programsCompleted.Count(x => x);
    public bool AllProgramsCompleted() => programsCompleted.All(x => x);

    private static readonly Dictionary<SoftwareProgramType, string> programTypeNames = new Dictionary<SoftwareProgramType, string>
    {
        { SoftwareProgramType.ColorCycle, "Color Cycle" },
        { SoftwareProgramType.HexCycle, "Hexadecimal Cycle" },
        { SoftwareProgramType.TempCheck, "Temperature Check" },
        { SoftwareProgramType.PatternIntegrity, "Pattern Integrity" },
        { SoftwareProgramType.SacrificialHouse, "Sacrificial House" },
        { SoftwareProgramType.RootedPassword, "Rooted Password" }
    };

    void Start()
    {
        MainMenu.AssignTerminal(this);
        MainMenu.gameObject.SetActive(false);
        ColorCycle.AssignTerminal(this);
        HexCycle.AssignTerminal(this);
        TempCheck.AssignTerminal(this);
        PatternIntegrity.AssignTerminal(this);
        SacrificialHouse.AssignTerminal(this);
        RootedPassword.AssignTerminal(this);
        DisableAllObjects();
    }

    void DisableAllObjects()
    {
        ColorCycle.gameObject.SetActive(false);
        HexCycle.gameObject.SetActive(false);
        TempCheck.gameObject.SetActive(false);
        PatternIntegrity.gameObject.SetActive(false);
        SacrificialHouse.gameObject.SetActive(false);
        RootedPassword.gameObject.SetActive(false);
    }

    public void ToggleMenu(bool toggle) => MainMenu.gameObject.SetActive(toggle);

    public void ToggleObject(int ix, bool toggle) => programObjects[ix].SetActive(toggle);

    public void WaitForStartup() => WaitForBootup = StartCoroutine(WaitForCompleteBootup());

    IEnumerator WaitForCompleteBootup()
    {
        yield return new WaitUntil(() => Bios.TerminalRunning == null);

        if (Bios.RecoverySucessful && !Bios.FirstBoot)
        {
            yield return new WaitForSeconds(3);
            Module.DoLog($"The recovery is successful. Solved!");
            Module.SolveModule();
            Module.Audio.PlaySoundAtTransform("OSBoot", transform);
            Module.MainVCRDisplay.StartSolve(Bios.Screen);
        }
        else
        {
            if (!Bios.FirstBoot)
            {
                Module.DoLog("The recovery has failed and resetted to its previous state. Strike!");
                Module.Module.HandleStrike();
            }
            Bootup();

            if (Bios.FirstBoot)
                Bios.FirstBoot = false;
        }

            
        WaitForBootup = null;
    }

    public void GeneratePrograms()
    {
        var programEnums = (SoftwareProgramType[])Enum.GetValues(typeof(SoftwareProgramType));
        var shuffledPrograms = programEnums.ToList().Shuffle().Take(4);

        programs = shuffledPrograms.Select((x, i) => ObtainProgram(x, i)).ToList();

        Module.DoLog($"The following programs selected are: {programs.Select((x, i) => i == 3 ? $"and {programTypeNames[x.ProgramType]}" : programTypeNames[x.ProgramType]).Join(", ")}");

        foreach (var program in programs)
        {
            Module.DoLog($"{programTypeNames[program.ProgramType]}:");

            if (program.ProgramType == SoftwareProgramType.ColorCycle)
                Module.DoLog(program.DisplayInfo);

            Module.DoLog(program.ToString());
        }

        programObjects = ObtainObjects();
    }

    public void OpenProgram(int ix)
    {
        if (programsCompleted[ix])
            return;

        MainMenu.gameObject.SetActive(false);
        programObjects[ix].SetActive(true);
        switch (programs[ix].ProgramType)
        {
            case SoftwareProgramType.ColorCycle:
                ColorCycle ccProgram = (ColorCycle)programs[ix];
                ccProgram.StartFlashingSequence();
                break;
            case SoftwareProgramType.HexCycle:
                HexCycle.AssignProgram((HexCycle)programs.First(x => x.ProgramType == SoftwareProgramType.HexCycle));
                HexCycle.DisplayEncryptedMessage();
                break;
            case SoftwareProgramType.TempCheck:
                
                TempCheck.AssignProgram((TempCheck)programs.First(x => x.ProgramType == SoftwareProgramType.TempCheck));
                TempCheck.DisplayTemperature();
                break;
            case SoftwareProgramType.PatternIntegrity:
                PatternIntegrity.AssignProgram((PatternIntegrity)programs.First(x => x.ProgramType == SoftwareProgramType.PatternIntegrity));
                PatternIntegrity.DisplayGrids();
                break;
            case SoftwareProgramType.SacrificialHouse:
                SacrificialHouse.AssignProgram((SacrificialHouse)programs.First(x => x.ProgramType == SoftwareProgramType.SacrificialHouse));
                SacrificialHouse.StartProgram();
                break;
            case SoftwareProgramType.RootedPassword:
                RootedPassword.AssignProgram((RootedPassword)programs.First(x => x.ProgramType == SoftwareProgramType.RootedPassword));
                break;
        }
    }

    void Bootup() => MainMenu.gameObject.SetActive(true);

    public void GetProgramName(int pos, out string name) => name = programTypeNames[programs[pos].ProgramType];

    public void DoCreepyShit(int ix)
    {
        programObjects[ix].SetActive(false);
        CreepyShit = StartCoroutine(TheCreepy());
        programsCompleted[ix] = true;
    }

    public void ErrorPayload(int ix, bool caught)
    {
        programObjects[ix].SetActive(false);
        CreepyShit = StartCoroutine(TheError(ix, caught));
    }

    IEnumerator TheError(int ix, bool caught)
    {
        Module.CreepyErrorShit(caught);
        yield return new WaitUntil(() => Module.MainVCRDisplay.Glitch == null);
        programObjects[ix].SetActive(true);
        CreepyShit = null;
    }

    IEnumerator TheCreepy()
    {
        Module.DoCreepyShit();
        yield return new WaitUntil(() => Module.MainVCRDisplay.Glitch == null);
        MainMenu.MarkProgramAsClosed();
        MainMenu.UpdateProgramCompletions(programs.Select(x => x.ProgramComplete).ToArray());
        MainMenu.gameObject.SetActive(true);
        Module.Audio.PlaySoundAtTransform("ProgramComplete", transform);
        Module.RaiseModule(ProgressCount() == 1);
        CreepyShit = null;
    }

    public void ToggleColorblind()
    {
        foreach (var program in programs)
        {
            if (program.NeedColorblind == null)
                continue;

            program.NeedColorblind ^= true;
        }
    }

    private SoftwareProgram ObtainProgram(SoftwareProgramType programType, int ix)
    {
        switch (programType)
        {
            case SoftwareProgramType.ColorCycle:
                return new ColorCycle(programType, ix, ColorCycle, this)
                {
                    NeedColorblind = Module.Colorblind.ColorblindModeActive
                };
            case SoftwareProgramType.HexCycle:
                return new HexCycle(programType, ix);
            case SoftwareProgramType.TempCheck:
                return new TempCheck(programType, ix);
            case SoftwareProgramType.PatternIntegrity:
                return new PatternIntegrity(programType, ix)
                {
                    NeedColorblind = Module.Colorblind.ColorblindModeActive
                };
            case SoftwareProgramType.SacrificialHouse:
                return new SacrificialHouse(programType, ix);
            case SoftwareProgramType.RootedPassword:
                return new RootedPassword(programType, ix, Module.Bomb.GetSerialNumber());
        }

        throw new Exception("Enum is invalid");
    }


}
