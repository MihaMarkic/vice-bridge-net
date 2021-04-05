namespace Righthand.ViceMonitor.Bridge.Responses
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="BankId">Describes which bank you want. This is dependent on your machine. See section 13.4.19 Banks available (0x82). If the memspace selected doesn't support banks, this value is ignored. </param>
    /// <param name="Name"></param>
    public record BankItem(ushort BankId, string Name);
    /// <summary>
    /// Register item
    /// </summary>
    /// <param name="Id">ID of the register</param>
    /// <param name="Size">Size of the register in bits</param>
    /// <param name="Name"></param>
    public record FullRegisterItem(byte Id, byte Size, string Name);
}
