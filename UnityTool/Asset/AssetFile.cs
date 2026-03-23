using System;
using System.Collections.Generic;
using System.IO;
using Common;

namespace UnityTool
{
    /// <summary>
    /// Serialized file
    /// </summary>
    public partial class AssetFile : UnityFile
    {
        #region Asset structure
        Header header;

        public long DataOffset => header.DataOffset;

        public BuildTarget Target = BuildTarget.UnknownPlatform;
        public AssetVersion Version => header != null ? header.Version : AssetVersion.kUnsupported;
        public BuildVersion Build { get; private set; }

        public FileIdentifier[] Externals => externals;
        public ObjectInfo[] ObjectInfos => objectinfos;

        public Dictionary<long, int> ObjectDictionary = new Dictionary<long, int>();

        bool enableTree = true;
        int bigIDEnabled;
        string unityVersion = "2.5.0f5";
        string userInformation = "";

        AssetType[] assetTypes;
        ObjectInfo[] objectinfos;
        ObjectIdentifier[] scriptTypes;
        FileIdentifier[] externals;
        AssetType[] refTypes;
        #endregion

        ObjectBase[] preLoaded;

        public int ObjectCount => objectinfos != null ? objectinfos.Length : 0;



        public bool Read(UnityFileReader reader)
        {
            reader.Position = 0;

            header = new Header(reader, out bool success);
            if (!success)
            {
                Debugg.Error("Asset Header not validated");
                return false;
            }
            var version = header.Version;

            // ReadMetadata
            reader.Endianess = header.Endianess;

            if (version >= AssetVersion.kUnknown_7)
                unityVersion = reader.ReadStringToNull();
            Build = new BuildVersion(unityVersion);

            if (version >= AssetVersion.kUnknown_8)
                Target = (BuildTarget)reader.ReadInt32();
            
            if (version >= AssetVersion.kHasTypeTreeHashes)
                enableTree = reader.ReadBoolean();
            

            // Read Types
            assetTypes = new AssetType[reader.ReadInt32()];
            Debugg.Print($"Serialized assetTypes : {assetTypes.Length}");

            for (int i = 0; i < assetTypes.Length; i++)
                assetTypes[i] = new AssetType(reader, enableTree, false);

            if (version >= AssetVersion.kUnknown_7 && version < AssetVersion.kUnknown_14)
                bigIDEnabled = reader.ReadInt32();

            // Read Objects
            objectinfos = new ObjectInfo[reader.ReadInt32()];
            Debugg.Print($"Objects : {objectinfos.Length}");

            preLoaded = new ObjectBase[objectinfos.Length];

            for (int i = 0; i < objectinfos.Length; i++)
            {
                var obj = new ObjectInfo(this, i);

                if (!obj.Read(reader, version, bigIDEnabled > 0)) return false;
                
                if (version < AssetVersion.kRefactoredClassId)
                    obj.SerializedType = Array.Find(assetTypes, x => (int)x.classID == obj.typeID);
                
                else
                {
                    obj.SerializedType = assetTypes[obj.typeID];
                    obj.classID = obj.SerializedType.classID;
                }

                if (version >= AssetVersion.kHasScriptTypeIndex && version < AssetVersion.kRefactorTypeData)
                    if (obj.SerializedType != null)
                        obj.SerializedType.ScriptTypeIndex = obj.ScriptTypeIndex;

                
                if (!ObjectDictionary.ContainsKey(obj.PathID))
                    ObjectDictionary.Add(obj.PathID, i);
                
                else
                    Debugg.Warning($"the pathID {obj.PathID} already exist !");
                
                objectinfos[i] = obj;
            }

            if (version >= AssetVersion.kHasScriptTypeIndex)
                reader.CreateInstanceArray(out scriptTypes);

            reader.CreateInstanceArray(out externals);

            if (externals.Length>0)
            {
                Debugg.Print("require to load external files:");
                foreach (var external in externals)
                    Debugg.Print($"- {Path.GetFileName(external.pathName)}");
            }
            

            if (version >= AssetVersion.kSupportsRefObject)
            {
                refTypes = new AssetType[reader.ReadInt32()];
                for (int i = 0; i < refTypes.Length; i++)
                    refTypes[i] = new AssetType(reader, enableTree, true);

            }

            if (version >= AssetVersion.kUnknown_5) userInformation = reader.ReadStringToNull();

            reader.AlignStream(16);

            return true;
        }
        
        /// <summary>
        /// Return the Asset Objects. The internal pre-loaded objects are used to link the classes
        /// </summary>
        public ObjectBase GetObjectByIndex(UnityFileReader reader, int index)
        {
            if (preLoaded[index] == null)
                preLoaded[index] = ReadObject(reader, objectinfos[index]);
            return preLoaded[index];
        }
        public ObjectBase GetObjectByPathID(UnityFileReader reader, long pathID)
        {
            if (ObjectDictionary.TryGetValue(pathID, out var index)) return GetObjectByIndex(reader, index);
            return null;
        }


        /// <summary>
        /// Read object class from assetfile (reader is the same used to load the assetfile). Currently the classes needed for the tool are: <br/>
        /// </summary>
        private ObjectBase ReadObject(UnityFileReader reader, ObjectInfo info)
        {
            if (reader.Asset != this) throw new Exception("reader is not for this file");

            reader.Position = info.byteStart + header.DataOffset;
            reader.ObjectInfo = info;

            ObjectBase obj = null;

            try
            {
                switch (info.classID)
                {
                    case ClassIDType.Animation:
                        //obj = new Animation(reader);
                        break;
                    case ClassIDType.AnimationClip:
                        //obj = new AnimationClip(reader);
                        break;
                    case ClassIDType.Animator:
                        //obj = new Animator(reader);
                        break;
                    case ClassIDType.AnimatorController:
                        //obj = new AnimatorController(reader);
                        break;
                    case ClassIDType.AnimatorOverrideController:
                        //obj = new AnimatorOverrideController(reader);
                        break;
                    case ClassIDType.AssetBundle:
                        obj = new AssetBundle(reader);
                        break;
                    case ClassIDType.AudioClip:
                        //obj = new AudioClip(reader);
                        break;
                    case ClassIDType.Avatar:
                        //obj = new Avatar(reader);
                        break;
                    case ClassIDType.Font:
                        //obj = new Font(reader);
                        break;
                    case ClassIDType.GameObject:
                        obj = new GameObject(reader);
                        break;
                    case ClassIDType.Material:
                        //obj = new Material(reader);
                        break;
                    case ClassIDType.Mesh:
                        obj = new Mesh(reader);
                        break;
                    case ClassIDType.MeshFilter:
                        obj = new MeshFilter(reader);
                        break;
                    case ClassIDType.MeshRenderer:
                        obj = new MeshRenderer(reader);
                        break;
                    case ClassIDType.MonoBehaviour:
                        //obj = new MonoBehaviour(reader);
                        break;
                    case ClassIDType.MonoScript:
                        //obj = new MonoScript(reader);
                        break;
                    case ClassIDType.MovieTexture:
                        //obj = new MovieTexture(reader);
                        break;
                    case ClassIDType.PlayerSettings:
                        //obj = new PlayerSettings(reader);
                        break;
                    case ClassIDType.RectTransform:
                        //obj = new RectTransform(reader);
                        break;
                    case ClassIDType.Shader:
                        //obj = new Shader(reader);
                        break;
                    case ClassIDType.SkinnedMeshRenderer:
                        obj = new SkinnedMeshRenderer(reader);
                        break;
                    case ClassIDType.Sprite:
                        //obj = new Sprite(reader);
                        break;
                    case ClassIDType.SpriteAtlas:
                        //obj = new SpriteAtlas(reader);
                        break;
                    case ClassIDType.TextAsset:
                        //obj = new TextAsset(reader);
                        break;
                    case ClassIDType.Texture2D:
                        obj = new Texture2D(reader);
                        break;
                    case ClassIDType.Transform:
                        obj = new Transform(reader);
                        break;
                    case ClassIDType.VideoClip:
                        //obj = new VideoClip(reader);
                        break;
                    case ClassIDType.ResourceManager:
                        //obj = new ResourceManager(reader);
                        break;

                    case ClassIDType.UnknownType:
                        break;
                }

                if (obj == null)
                {
                    obj = new ObjectBase(reader);
                }
            }
            catch(Exception e)
            {
                Debugg.Error(e);
                obj = null;
            }

            return obj;
        }




        public override void Dispose()
        {

        }
    }
}
