using Common;
using System;
using System.Collections.Generic;
using System.IO;


namespace Blue3DPrinter
{

    /// <summary>
    /// 
    /// </summary>
    public class BlockMapping
    {
        internal Dictionary<string, ushort> dictionary;

        public void Add(string blockName, int blockId)
        {
            if (!dictionary.ContainsKey(blockName))
            {
                dictionary.Add(blockName, (ushort)blockId);
            }
        }
        /// <summary>
        /// return the blockid using blockname, return -1 if not found
        /// </summary>
        public int TryGetBlockId(string blockname)
        {
            return dictionary.TryGetValue(blockname, out ushort blockid) ? blockid :-1;
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public BlockMapping(int capacity = 4096)
        {
            dictionary = new Dictionary<string, ushort>(capacity);
        }

        public bool Read(BinaryReader br)
        {
            int num1 = br.ReadByte();//1
            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string blockName = br.ReadString();
                int blockId = br.ReadUInt16();

                if (dictionary.ContainsKey(blockName))
                    LogMsg.Message("> ERROR when reading block mapping, the name \"" + blockName + "\" already exist");
                else
                    dictionary.Add(blockName, (ushort)blockId);
            }
            return true;
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write((byte)1);
            bw.Write(dictionary.Count);

            foreach (var pair in dictionary)
            {
                bw.Write(pair.Key);
                bw.Write(pair.Value);
            }
        }

        public bool Write(TextWriter tw)
        {
            tw.WriteLine("\n//+------------------------+");
            tw.WriteLine("//|       BLOCKSMAP        |");
            tw.WriteLine("//+------------------------+");

            foreach (var pair in dictionary)
                tw.WriteLine(Utils.mytxtformat(pair.Key, pair.Value, 22));
            
            return true;
        }

        public bool Read(TextReader tr)
        {
            throw new NotImplementedException();
        }

    }


}
