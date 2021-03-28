namespace Righthand.ViceMonitor.Bridge.Commands
{
    public record BankItem(ushort BankId, string Name);

    public record FullRegisterItem(byte Id, byte Size, string Name);
}
