using System.Threading.Tasks;

namespace Obsidian.Net.Packets
{
    public class ClientSettings : Packet
    {
        public ClientSettings(byte[] data) : base(0x04, data)
        {
        }

        public string Locale { get; private set; }
        public sbyte ViewDistance { get; private set; }
        public int ChatMode { get; private set; }
        public bool ChatColors { get; private set; }
        public byte SkinParts { get; private set; } // skin parts that are displayed. might not be necessary to decode?
        public int MainHand { get; private set; }

        protected override async Task PopulateAsync(MinecraftStream stream)
        {
            Locale = await stream.ReadStringAsync();
            ViewDistance = await stream.ReadByteAsync();
            ChatMode = await stream.ReadIntAsync();
            ChatColors = await stream.ReadBooleanAsync();
            SkinParts = await stream.ReadUnsignedByteAsync();
            MainHand = await stream.ReadVarIntAsync();
        }

        protected override async Task ComposeAsync(MinecraftStream stream)
        {
            // afaik these never get sent but that's OK
            await stream.WriteStringAsync(Locale);
            await stream.WriteByteAsync(ViewDistance);
            await stream.WriteIntAsync(ChatMode);
            await stream.WriteBooleanAsync(ChatColors);
            await stream.WriteUnsignedByteAsync(SkinParts);
            await stream.WriteVarIntAsync(MainHand);
        }
    }
}