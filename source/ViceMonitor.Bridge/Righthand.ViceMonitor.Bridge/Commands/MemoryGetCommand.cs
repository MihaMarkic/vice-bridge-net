//using System.Collections.Immutable;
//using System.IO;
//using ModernVicePdbMonitor.Bridge.Commands;

//namespace Righthand.ViceMonitor.Bridge.Commands
//{
//    public record MemoryGetCommand(byte SideEffects, ushort StartAddress, ushort EndAddress, byte MemSpace, ushort BankId)
//        : ViceCommand<MemoryGetResponse>(CommandType.MemoryGet)
//    {
//        public override uint ContentLength { get; } = sizeof(byte) + sizeof(ushort) + sizeof(ushort) + sizeof(byte) + sizeof(ushort);
//        public override void WriteContent(BinaryWriter writer)
//        {
//            writer.Write(SideEffects);
//            writer.Write(StartAddress);
//            writer.Write(EndAddress);
//            writer.Write(MemSpace);
//            writer.Write(BankId);
//        }
//        protected internal override MemoryGetResponse CreateResponseFromBuffer(byte apiVersion, ResponseType responseType, uint length, ErrorCode errorCode, BinaryReader reader)
//        {
//            ImmutableArray<byte> data;
//            if (errorCode == ErrorCode.OK)
//            {
//                ushort memorySegmentLength = reader.ReadUInt16();
//                data = reader.ReadBytes(memorySegmentLength).ToImmutableArray();
//            }
//            else
//            {
//                data = ImmutableArray<byte>.Empty;
//            }
//            return new MemoryGetResponse(apiVersion, errorCode, data);
//        }
//    }
//}
