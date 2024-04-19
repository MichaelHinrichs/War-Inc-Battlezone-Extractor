using System.IO;
using System.IO.Compression;

namespace War_Inc_Battlezone_Extractor
{
    class Program
    {
        static BinaryReader br;
        static void Main(string[] args)
        {
            br = new(File.OpenRead(args[0]));
            if (new string(br.ReadChars(4)) != "arfl")
                throw new System.Exception("Please input WO_00.bin from \"War Inc. Battlezone\".");

            FileTable();
            br.BaseStream.Position = 0;
            System.Collections.Generic.List<Subfile> table = new();
            while (br.BaseStream.Position < br.BaseStream.Length)
                table.Add(new());
            br.Close();

            string path = Path.GetDirectoryName(args[0]) + "//";
            foreach (Subfile file in table)
            {
                if (file.isExternal == 1)
                    continue;

                if (file.archive == 0)
                    br = new(File.OpenRead(path + "//WO_01.bin"));
                else if (file.archive == 1)
                    br = new(File.OpenRead(path + "//WO_02.bin"));
                else throw new System.Exception("Fuck!");

                br.BaseStream.Position = file.start + 4;
                if (file.isCompressed == 1)
                {
                    Directory.CreateDirectory(path + Path.GetDirectoryName(file.name));
                    BinaryWriter bw = new(File.Create(path + file.name));
                    bw.Write(br.ReadBytes(file.size));
                    bw.Close();
                }
                else if(file.isCompressed == 2)
                {
                    br.ReadInt16();
                    Directory.CreateDirectory(path + Path.GetDirectoryName(file.name));
                    using var ds = new DeflateStream(new MemoryStream(br.ReadBytes(file.size - 2)), CompressionMode.Decompress);
                        ds.CopyTo(File.Create(path + file.name));
                }
                else throw new System.Exception("Fuck!");
            }
        }

        static void FileTable()
        {
            br.ReadInt32();
            br.ReadInt32();
            br.ReadInt32();
            int size = br.ReadInt32();
            br.ReadInt32();

            MemoryStream fileTable = new();
            br.ReadInt16();
            using (var ds = new DeflateStream(new MemoryStream(br.ReadBytes(size - 2)), CompressionMode.Decompress))
                ds.CopyTo(fileTable);

            br = new(fileTable);
        }

        class Subfile
        {
            public string name = new string(br.ReadChars(260)).TrimEnd((char)0x00);
            public byte isCompressed = br.ReadByte();
            public byte archive = br.ReadByte();
            public int start = br.ReadInt32();
            public int unknown = br.ReadInt32();
            public int size = br.ReadInt32();
            public int unknown2 = br.ReadInt32();
            public short isExternal = br.ReadInt16();
            public byte[] sixteen = br.ReadBytes(16);
        }
    }
}
