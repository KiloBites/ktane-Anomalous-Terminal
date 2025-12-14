public abstract class SoftwareProgram
{
    public SoftwareProgramType ProgramType { get; private set; }
    public int ProgramIndex { get; private set; }
    public bool? NeedColorblind { get; set; }
    public bool ProgramComplete { get; set; }

    protected SoftwareProgram(SoftwareProgramType programType, int programIndex, bool? needColorblind = null)
    {
        ProgramType = programType;
        ProgramIndex = programIndex;
        NeedColorblind = needColorblind;
    }

    public virtual string DisplayInfo => null;
    public abstract bool CheckInformation(object other);
}