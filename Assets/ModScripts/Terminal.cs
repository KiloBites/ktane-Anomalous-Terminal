using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class Terminal : MonoBehaviour
{
    private List<SoftwareProgram> programs;

    public ColorCycleDisplay ColorCycle;
    public MainMenu MainMenu;
    public AnomalousTerminalScript Module;
    public TerminalBIOS Bios;

    public int ProgressCount() => programs.Count(x => x.ProgramComplete);
    public bool AllProgramsCompleted() => programs.All(x => x.ProgramComplete);

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
        ColorCycle.AssignTerminal(this);
    }

    public void GeneratePrograms()
    {
        var programEnums = (SoftwareProgramType[])Enum.GetValues(typeof(SoftwareProgramType));
        var shuffledPrograms = programEnums.ToList().Shuffle().Take(4);

        programs = shuffledPrograms.Select((x, i) => ObtainProgram(x, i)).ToList();

        Module.DoLog($"The following programs selected are: {programs.Select((x, i) => i == 3 ? $"and {programTypeNames[x.ProgramType]}" : programTypeNames[x.ProgramType]).Join(", ")}");
    }

    public void OpenProgram(int ix)
    {
        if (programs[ix].ProgramComplete)
            return;


        switch (programs[ix].ProgramType)
        {
            case SoftwareProgramType.ColorCycle:
                ColorCycle.gameObject.SetActive(true);
                ColorCycle ccProgram = (ColorCycle)programs[ix];
                ccProgram.StartFlashingSequence();
                break;
        }
    }

    public void GetProgramName(int pos, out string name) => name = programTypeNames[programs[pos].ProgramType];

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
                throw new NotImplementedException();
            case SoftwareProgramType.PatternIntegrity:
                throw new NotImplementedException();
            case SoftwareProgramType.SacrificialHouse:
                throw new NotImplementedException();
            case SoftwareProgramType.RootedPassword:
                return new RootedPassword(programType, ix, Module.Bomb.GetSerialNumber());
        }

        throw new Exception("Enum is invalid");
    }


}
