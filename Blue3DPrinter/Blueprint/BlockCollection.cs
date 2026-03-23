
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Common;
using Common.Maths;
using Common.Tools;

namespace Blue3DPrinter
{
    /// <summary>
    /// After some tests with the BenchmarkDotNet's library, instead of creating a single array of offsets
    /// with size = size.x*y*z, i create a partitioned array which increase size dynamically with an initial cost
    /// of: sizeof(class) * 10 * 10 * 10 = 12 byte(for x64) * 1000 = 12KB. This is necessary when you are creating a new
    /// blueprint and you don't know the size.
    /// </summary>
    public class BlockCollection : IEnumerable<BlockObject>
    {
        const uint mHASH = 1023; // 0011 1111 1111

        BucketTable collection;
        //ArrayTable<BlockObject> collection;
        Blueprint blueprint;
        Vector3i capacity;

        /// <summary>
        /// the bound size of collection, increase every time you add a block
        /// </summary>
        public Vector3i Size => collection.Size;
        /// <summary>
        /// Maximum limit of bound size
        /// </summary>
        public Vector3i Capacity => capacity;

        public int Count => collection.Count;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        public BlockCollection(Blueprint owner) : this(owner, new Vector3i(250, 250, 250)) { }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="capacity">game limit if 250 blocks per size</param>
        public BlockCollection(Blueprint owner, Vector3i capacity) 
        {
            blueprint = owner;
            collection = new BucketTable();
            this.capacity = capacity;
        }

        /// <summary>
        /// Create an empty block using a valid position. Return null if position is already occupied. 
        /// </summary>
        public BlockObject Create(int x, int y, int z)
        {
            if (x < 0 || x >= capacity.x || y < 0 || y >= capacity.y || z < 0 || z >= capacity.z)
                throw new ArgumentOutOfRangeException(string.Format("position must be in the range of [0,0,0] - [{0},{1},{2}]", capacity.x, capacity.y, capacity.z));

            BlockObject newobj = null;

            if (collection.GetObject(x, y, z) == null)
            {
                newobj = new BlockObject(blueprint, x, y, z);
                collection.Add(newobj, x, y, z);
            }
            return newobj;
        }

        public BlockObject Create(Vector3b position)
        {
            return Create(position.x, position.y, position.z);
        }

        /// <summary>
        /// return null if not found
        /// </summary>
        public BlockObject this[int x, int y, int z]
        {
            get
            {
                return collection.GetObject(x, y, z);
            }
            set 
            {
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// return null if not found
        /// </summary>
        public BlockObject this[Vector3b position]
        {
            get { return this[position.x, position.y, position.z]; }
            set { this[position.x, position.y, position.z] = value;}
        }


        /// <summary>
        /// If you remove some blocks, you must update the size before get
        /// </summary>
        public void UpdateSize()
        {
            collection.TrimRange();
        }

        public IEnumerator<BlockObject> GetEnumerator() { return collection.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }


        /// <summary>
        /// Foreach visible blocks found in blueprint structure, resolve its description
        /// </summary>
        public bool LoadDescriptions(BlocksConfig config)
        {
            LogMsg.Message("> Resolve blockid's description", ConsoleColor.Yellow);

            if (config == null) throw new ArgumentNullException("config can't be null");

            if (Count == 0)
            {
                LogMsg.Message("> my visible blocks dictionary is empty", ConsoleColor.Red);
                return false;
            }
            
            HashSet<int> missingSet = new HashSet<int>();
            Dictionary<int, BlockDescription> decriptionFound = new Dictionary<int, BlockDescription>();

            //first check if blockid from blockmapping is valid

            foreach (var IDmap in blueprint.Header.BlockMap.dictionary)
            {
                BlockDescription descr = config.GetDescription(IDmap.Value);
                if (descr == null)
                {
                    LogMsg.Message("> ERROR can't find description for block name \"" + IDmap.Value + "\", this block will be omitted", ConsoleColor.Red);
                    missingSet.Add(IDmap.Value);
                }
                else if (descr.BlockId != IDmap.Value)
                {
                    LogMsg.Message("> ERROR the description's block id not match with block name, this block id will be omitted", ConsoleColor.Red);
                    missingSet.Add(IDmap.Value);
                }
                else
                {
                    decriptionFound.Add(IDmap.Value, descr);
                }
            }

            if (missingSet.Count > 0)
                LogMsg.Message(string.Format("> missing descriptions {0}/{1}", missingSet.Count, blueprint.Header.BlockMap.dictionary.Count), ConsoleColor.Red);

            int missingblocks = 0;

            foreach (var block in this)
            {
                int blockId = block.BlockId;


                if (!missingSet.Contains(blockId))
                {
                    if (!decriptionFound.TryGetValue(blockId, out BlockDescription descr))
                    {
                        missingblocks++;
                        block.Enable = false;
                        LogMsg.Message(string.Format("> block id {0} doesn't have a description", blockId), ConsoleColor.Red);
                        continue;
                    }
                    block.Description = descr;
                }
                else
                {
                    missingblocks++;
                    block.Enable = false;
                }
            }
            LogMsg.Message(string.Format("> missing block {0}/{1}", missingblocks, Count), missingblocks > 0 ? ConsoleColor.Red : ConsoleColor.Green);
            //if all missing there are something wrong
            return missingblocks != Count;
        }

        /// <summary>
        /// Foreach visible blocks founded in blueprint structure, load once the unique 3d model
        /// </summary>
        public bool LoadModels()
        {
            if (Count == 0)
            {
                LogMsg.Message("> my visible blocks dictionary is empty", ConsoleColor.Red);
                return false;
            }

            int missing = 0;
            HashSet<string> missingSet = new HashSet<string>();
            try
            {
                LogMsg.Message("> Load Block's models", ConsoleColor.Yellow);

                //this dictionary is very important because you need to load only once the model
                Dictionary<string, BlockModel> UniqueModelsList = new Dictionary<string, BlockModel>();

                foreach (var block in this)
                {
                    if (!block.Enable || block.Description == null)
                    {
                        missing++;
                        continue;
                    }

                    string modelFilename = block.Description.GetFilenameAsset(block.ChildIndex);

                    if (modelFilename == null)
                    {
                        block.Enable = false;
                        missing++;
                        LogMsg.Message("> ERROR can't find asset name for child " + block.ChildIndex + " of block id " + block.BlockId + ", this block will be omitted", ConsoleColor.Red);
                        continue;
                    }

                    //already get a missing result
                    if (missingSet.Contains(modelFilename))
                    {
                        block.Enable = false;
                        missing++;
                        continue;
                    }

                    //first time
                    if (!UniqueModelsList.TryGetValue(modelFilename, out BlockModel model))
                    {
                        if (ModelResourceManager.TryGetModel(modelFilename, out var scene))
                        {
                            model = new BlockModel(scene, block.Description, modelFilename);
                            UniqueModelsList.Add(modelFilename, model);
                        }
                        else model = null;

                    }

                    if (model == null)
                    {
                        block.Enable = false;
                        missing++;
                        missingSet.Add(modelFilename);
                        LogMsg.Message("> ERROR 3d model not found, all block with same model will be omitted :  ", ConsoleColor.Red, false);
                        LogMsg.Message(modelFilename, ConsoleColor.DarkMagenta);
                        continue;
                    }
                    block.Model = model;
                }
            }
            finally
            {
                //storage.Close();
            }
            LogMsg.Message(string.Format("> total omitted blocks {0}/{1}", missing, Count), missing > 0 ? ConsoleColor.Red : ConsoleColor.Green);
            return true;
        }

        private class ArrayTable<T> : IEnumerable<T> where T : class
        {
            T[,,] buckets;

            public int Count { get; private set; }

            public ArrayTable(int sizex, int sizey, int sizez)
            {
                buckets = new T[sizex, sizey, sizez];
            }

            public void Add(T block, int x, int y, int z)
            {
                if (buckets[x, y, z] == null) Count++;
                buckets[x,y,z] = block;
            }

            public T GetObject(int x, int y, int z)
            {
                return buckets[x, y, z];
            }

            public void Remove(int x, int y, int z)
            {

                if (buckets[x, y, z] != null) Count--;
                buckets[x, y, z] = null;
            }
            public IEnumerator<T> GetEnumerator()
            {
                for (int x = 0; x < buckets.GetLength(0); x++)
                    for (int y = 0; y < buckets.GetLength(1); y++)
                        for (int z = 0; z < buckets.GetLength(2); z++)
                        {
                            var obj = GetObject(x, y, z);
                            if (obj != null) yield return obj;
                        }
            }
            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        }


        private class BucketTable : IEnumerable<BlockObject>
        {
            public Vector3i Size => lenght;

            Vector3i lenght;
            BucketStruct[,,] buckets;

            public int Count { get; private set; }
            
            public BucketTable()
            {
                buckets = new BucketStruct[10, 10, 10];
                lenght = new Vector3i(0, 0, 0);
            }

            public void TrimRange()
            {
                Vector3i newlenght = new Vector3i(0, 0, 0);

                foreach (var obj in this)
                {
                    if (obj.x >= newlenght.x) newlenght.x = obj.x + 1;
                    if (obj.y >= newlenght.y) newlenght.y = obj.y + 1;
                    if (obj.z >= newlenght.z) newlenght.z = obj.z + 1;
                }
                lenght = newlenght;
            }

            public void Add(BlockObject block, int x, int y, int z)
            {
                var list = buckets[x / 25, y / 25, z / 25].list;
                if (list == null) list = buckets[x / 25, y / 25, z / 25].list = new BlockObject[25, 25, 25];
                if (list[x % 25, y % 25, z % 25] == null)
                {
                    if (x >= lenght.x) lenght.x = x + 1;
                    if (y >= lenght.y) lenght.y = y + 1;
                    if (z >= lenght.z) lenght.z = z + 1;
                    Count++;
                }
                list[x % 25, y % 25, z % 25] = block;
            }

            public BlockObject GetObject(int x, int y, int z)
            {
                if (buckets[x / 25, y / 25, z / 25].list == null) return null;
                return buckets[x / 25, y / 25, z / 25].list[x % 25, y % 25, z % 25];
            }

            public void Remove(int x, int y, int z)
            {
                var list = buckets[x / 25, y / 25, z / 25].list;
                if (list != null)
                {
                    if (list[x % 25, y % 25, z % 25] != null) Count--;

                    list[x % 25, y % 25, z % 25] = null;
                }
            }
            public IEnumerator<BlockObject> GetEnumerator()
            {
                for (int x = 0; x < lenght.x; x++)
                    for (int y = 0; y < lenght.y; y++)
                        for (int z = 0; z < lenght.z; z++)
                        {
                            var obj = GetObject(x, y, z);
                            if (obj != null) 
                                yield return obj;
                        }
            }
            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            public struct BucketStruct
            {
                public BlockObject[,,] list;
            }
        }
    }
}
