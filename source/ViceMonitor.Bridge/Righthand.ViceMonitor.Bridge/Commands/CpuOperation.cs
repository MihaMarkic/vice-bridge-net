namespace Righthand.ViceMonitor.Bridge.Commands
{
    public enum CpuOperation : byte
    {
        Load = 0x01,
        Store = 0x02,
        Exec = 0x04,
    }
}
