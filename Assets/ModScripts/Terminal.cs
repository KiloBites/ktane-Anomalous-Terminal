using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Terminal : MonoBehaviour
{
    private List<SoftwareProgram> programs;

    private bool[] completedPrograms = new bool[4];

    public ColorCycleDisplay ColorCycle;
    public MainMenu MainMenu;

    private AnomalousTerminalScript _module;

    public int ProgressCount() => completedPrograms.Count(x => x);

    public bool AllProgramsCompleted() => completedPrograms.All(x => x);

    public void MarkProgramAsComplete(int pos) => completedPrograms[pos] = true;

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
        ColorCycle.AssignModule(_module);
    }

    public void GeneratePrograms()
    {
        var programEnums = (SoftwareProgramType[])Enum.GetValues(typeof(SoftwareProgramType));
        var shuffledPrograms = programEnums.ToList().Shuffle().Take(4);

        programs = shuffledPrograms.Select((x, i) => ObtainProgram(x, i)).ToList();
    }

    public void OpenProgram(int ix)
    {
        if (completedPrograms[ix])
            return;


        switch (programs[ix].ProgramType)
        {
            case SoftwareProgramType.ColorCycle:
                ColorCycle.gameObject.SetActive(true);
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

            program.NeedColorblind = !program.NeedColorblind;
        }
    }

    private SoftwareProgram ObtainProgram(SoftwareProgramType programType, int ix)
    {
        switch (programType)
        {
            case SoftwareProgramType.ColorCycle:
                return new ColorCycle(programType, ix, ColorCycle, this)
                {
                    NeedColorblind = _module.Colorblind.ColorblindModeActive
                };
            case SoftwareProgramType.HexCycle:
                return new HexCycle(programType, ix);
        }

        throw new NotImplementedException();
    }


}
