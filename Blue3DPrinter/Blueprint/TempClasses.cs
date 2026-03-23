
using System;
using System.Collections.Generic;
using System.IO;
using Common;
using Common.Maths;

// all this classes name and property doesn't have sense, all these data are not interpreted
namespace Blue3DPrinter.other
{
    /// <summary>
    /// not implemented
    /// </summary>
    public sealed class VectorInfoLineProvider
    {
        private Dictionary<int, float> parentQuery = new Dictionary<int, float>();
        public static float lastContainer;
        float parentQueryFloat;

        public float ArtilleryDefenseValue { get; private set; }
        public float InfantryAttackValue { get; private set; }
        public float InfantryDefenseValue { get; private set; }

        public void DisposeComponent()
        {
        }
        public void BuildSolution(float f, float artyDef, float infAtk, float infDef)
        {
            parentQueryFloat = f;
            ArtilleryDefenseValue = artyDef;
            InfantryAttackValue = infAtk;
            InfantryDefenseValue = infDef;
        }
    }

    /// <summary>
    /// not implemented, the <see cref="Write"/> function write a zero group
    /// </summary>
    public sealed class BlockGrouping
    {
        public List<FileEditor> fileEditor = new List<FileEditor>();

        public void Clear()
        {
            fileEditor.Clear();
        }

        public void Write(BinaryWriter bw)
        {
            //LogMsg.Message("> BlockGroupInfo not implemented when save, all group are removed", ConsoleColor.Red);
            bw.Write((byte)5);
            bw.Write((ushort)0);
        }
        public bool Write(TextWriter tw)
        {
            tw.WriteLine("\n//+------------------------+");
            tw.WriteLine("//|      BLOCKGROUPS       |");
            tw.WriteLine("//| currently only raw data|");
            tw.WriteLine("//+------------------------+");

            foreach (var item in fileEditor)
            {
                item.Write(tw);
            }

            return true;
        }

        public bool Read(TextReader tr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// see dll's function "buildsolution"
        /// </summary>
        public void Read(BinaryReader br)
        {
            byte num5 = br.ReadByte();//5
            fileEditor.Clear();
            int count = br.ReadUInt16();

            if (fileEditor.Capacity < count) 
                fileEditor.Capacity = count;

            for (int i = 0; i < count; ++i)
            {
                FileEditor fileEditor = new FileEditor(string.Empty);
                fileEditor.CheckDomain(br, num5);
                this.fileEditor.Add(fileEditor);
            }
        }
        /// <summary>
        /// see dll's function "rebuildbuilder"
       /// </summary>
        public void ReadRead(BinaryReader br)
        {
            byte num1 = br.ReadByte();
            int num2 = br.ReadUInt16();
            
            if (fileEditor.Count != num2)
            {
                FileEditor fileEditor = new FileEditor();
                for (int index = 0; index < num2; ++index)
                    fileEditor.UnregisterTreeNode(br, num1);
            }
            else
            {
                for (int index = 0; index < num2; ++index)
                    fileEditor[index].UnregisterTreeNode(br, num1);
            }
        }
    }

    /// <summary>
    /// not implemented
    /// </summary>
    public sealed class FileEditor
    {
        public string strval;
        public List<Vector3ui> Vectorlist = new List<Vector3ui>();
        public List<string> strlist = new List<string>();
        public bool boolval;
        public bool lockInstance;
        public byte byteval;

        private List<Vector3ui> hash = new List<Vector3ui>();
        //private HashSet<Vector3ui> hash = new HashSet<Vector3ui>();

        public FileEditor()
        {

        }
        public FileEditor(string str)
        {

        }
        public bool Write(TextWriter tw)
        {
            tw.WriteLine(string.Format("\"{0}\"\t{1} {2} {3}", strval, boolval, lockInstance, byteval));

            for (int i=0;i< Vectorlist.Count;i++)
            {
                tw.WriteLine(string.Format("{0}\t{1}\t{2}", Vectorlist[i], hash[i] , strlist[i]==null ? "" : strlist[i]));
            }

            return true;
        }
        public void CheckDomain(BinaryReader br, byte param2)
        {
            strval = br.ReadString();
            boolval = br.ReadBoolean();
            lockInstance = param2 <= (byte)4 || br.ReadBoolean();
            byteval = param2 <= (byte)3 ? byte.MaxValue : br.ReadByte();
            Vectorlist.Clear();
            strlist.Clear();
            hash.Clear();
            int count = br.ReadUInt16();

            if (Vectorlist.Capacity < count) Vectorlist.Capacity = count;
            if (strlist.Capacity < count) strlist.Capacity = count;

            for (int i = 0; i < count; ++i)
            {
                Vector3ui Vector3i = param2 <= (byte)2 ? Utils.ReadVector3ui(br) : (Vector3ui)Utils.ReadVector3i_12_8_12(br);
                Vectorlist.Add(Vector3i);
                hash.Add(Vector3i);

                string str = null; //"ServerLineProvider.parentQuery";
                
                if (param2 > (byte)1)
                {
                    str = br.ReadString();
                    if (str.Length == 0) str = null;// "ServerLineProvider.parentQuery";
                }
                strlist.Add(str);
            }
        }
        public void UnregisterTreeNode(BinaryReader br, byte param2) => br.ReadBoolean();

    }



}
