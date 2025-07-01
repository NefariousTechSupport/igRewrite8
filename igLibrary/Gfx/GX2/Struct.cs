namespace igLibrary.Gfx.GX2Utils
{
    public class Struct
    {
        public static byte[] Pack(string format, params uint[] values)
        {
            List<byte> bytes = new List<byte>();
            foreach (var value in values)
            {
                bytes.AddRange(BitConverter.GetBytes(value));
            }
            return bytes.ToArray();
        }
        public static uint[] Unpack(string format, byte[] data, int pos = 0)
        {
            List<uint> values = new List<uint>();
            for (int i = pos; i < data.Length; i += 4)
            {
                values.Add(BitConverter.ToUInt32(data, i));
            }
            return values.ToArray();
        }

    }
}
