using Blue3DPrinter.other;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blue3DPrinter
{


    public class BlockStatistics
    {
        public int cntLights;
        public int cntDoors;
        public int cntBlockEntities;
        public int cntBlockModels;
        public int cntSolid;
        public int cntTriangles;
        public int cntTrianglesReal;
        public int cntTeleporters;
        public int cntSpawnPointsUnlocked;
        public int cntSpawnPointsLocked;
        public bool bAdminCore = false;
        public bool bKeepContainers = false;

        public Dictionary<int, int> BlockIdDistribution = new Dictionary<int, int>();

        //not implemented
        VectorInfoLineProvider AttDefValues = new VectorInfoLineProvider();
        public float ArtAttack => 0.0f;
        public float ArtDefense => AttDefValues.ArtilleryDefenseValue;
        public float InfAttack => AttDefValues.InfantryAttackValue;
        public float InfDefense => AttDefValues.InfantryDefenseValue;



        public void Clear()
        {
            cntLights = 0;
            cntDoors = 0;
            cntBlockEntities = 0;
            cntSolid = 0;
            cntBlockModels = 0;
            cntTriangles = 0;
            cntTeleporters = 0;
            cntSpawnPointsUnlocked = cntSpawnPointsLocked = 0;
            BlockIdDistribution.Clear();
            AttDefValues.DisposeComponent();
            bAdminCore = false;
            bKeepContainers = false;
        }

        public bool IsCalculated() => (uint)BlockIdDistribution.Count > 0U;


        public bool Write(BinaryWriter bw, uint version = 29)
        {
            bw.Write(cntLights);
            bw.Write(cntDoors);
            bw.Write(cntBlockEntities);
            bw.Write(cntBlockModels);
            bw.Write(cntSolid);
            if (version > 12U) bw.Write(cntTriangles);
            if (version > 17U) bw.Write(cntTrianglesReal);
            if (version > 23U)
            {
                bw.Write(cntTeleporters);
                bw.Write(cntSpawnPointsUnlocked);
                bw.Write(cntSpawnPointsLocked);
            }
            if (BlockIdDistribution != null)
            {
                bw.Write((ushort)BlockIdDistribution.Count);

                if (version <= 12U)
                    foreach (var pair in BlockIdDistribution)
                    {
                        bw.Write((int)pair.Key);
                        bw.Write(pair.Value);
                    }
                else
                    foreach (var pair in BlockIdDistribution)
                    {
                        bw.Write((ushort)pair.Key);
                        bw.Write(pair.Value);
                    }
            }
            else
            {
                bw.Write((ushort)0);
            }

            if (version >= 26U)
            {
                if (version < 27U)
                {
                    Debug.WriteLine("> attacks and defend values not implemented for < 27");
                    return false;
                }
                else
                {
                    Debug.WriteLine("> all attack and defend values set to zero.");
                    bw.Write((float)0);
                    bw.Write((float)0);
                    bw.Write((float)0);
                    bw.Write((float)0);
                }
            }
            if (version > 28U)
            {
                bw.Write(bAdminCore);
                bw.Write(bKeepContainers);
            }
            return true;
        }

        public bool Read(BinaryReader br, uint version)
        {
            cntLights = br.ReadInt32();
            cntDoors = br.ReadInt32();
            cntBlockEntities = br.ReadInt32();
            cntBlockModels = br.ReadInt32();
            cntSolid = br.ReadInt32();

            if (version > 12U) cntTriangles = br.ReadInt32();
            if (version > 17U) cntTrianglesReal = br.ReadInt32();
            if (version > 23U)
            {
                cntTeleporters = br.ReadInt32();
                cntSpawnPointsUnlocked = br.ReadInt32();
                cntSpawnPointsLocked = br.ReadInt32();
            }
            BlockIdDistribution.Clear();
            int count = br.ReadUInt16();

            if (version <= 12U)
            {
                for (int i = 0; i < count; ++i)
                    BlockIdDistribution[br.ReadInt32()] = br.ReadInt32();
            }
            else
            {
                for (int i = 0; i < count; ++i)
                    BlockIdDistribution[br.ReadUInt16()] = br.ReadInt32();
            }
            AttDefValues.DisposeComponent();

            if (version >= 26U)
            {
                if (version < 27U)
                {
                    float num2 = br.ReadSingle();
                    if (num2 == 4711.080078125)
                    {
                        int num3 = br.ReadByte();
                        AttDefValues.BuildSolution(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    }
                    else
                        AttDefValues.BuildSolution(num2, br.ReadSingle(), 0.0f, 0.0f);
                }
                else
                    AttDefValues.BuildSolution(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            }
            bAdminCore = false;
            bKeepContainers = false;

            if (version > 28U)
            {
                bAdminCore = br.ReadBoolean();
                bKeepContainers = br.ReadBoolean();
            }
            return true;
        }


        public bool Write(TextWriter tw)
        {
            tw.WriteLine("\n//+------------------------+");
            tw.WriteLine("//|       STATISTICS       |");
            tw.WriteLine("//+------------------------+");

            tw.WriteLine(Utils.mytxtformat("lights", cntLights));
            tw.WriteLine(Utils.mytxtformat("doors", cntDoors));
            tw.WriteLine(Utils.mytxtformat("block_entities", cntBlockEntities));
            tw.WriteLine(Utils.mytxtformat("block_models", cntBlockModels));
            tw.WriteLine(Utils.mytxtformat("solid", cntSolid));
            tw.WriteLine(Utils.mytxtformat("triangles", cntTriangles));
            tw.WriteLine(Utils.mytxtformat("triangles_real", cntTrianglesReal));
            tw.WriteLine(Utils.mytxtformat("teleporters", cntTeleporters));
            tw.WriteLine(Utils.mytxtformat("spawn_unlocked", cntSpawnPointsUnlocked));
            tw.WriteLine(Utils.mytxtformat("spawn_locked", cntSpawnPointsLocked));

            tw.WriteLine("<BlockIdDistribution>");
            foreach (var pair in BlockIdDistribution)
                tw.WriteLine(string.Format("\tid {0}\t\tn {1}", pair.Key, pair.Value));

            tw.WriteLine(Utils.mytxtformat("ArtAttack", ArtAttack));
            tw.WriteLine(Utils.mytxtformat("ArtDefense", ArtDefense));
            tw.WriteLine(Utils.mytxtformat("InfAttack", InfAttack));
            tw.WriteLine(Utils.mytxtformat("InfDefense", InfDefense));

            tw.WriteLine(Utils.mytxtformat("admin_core", bAdminCore));
            tw.WriteLine(Utils.mytxtformat("keep_containers", bKeepContainers));

            return true;
        }


        public bool Read(TextReader tr)
        {
            throw new NotImplementedException();
        }



    }
}
