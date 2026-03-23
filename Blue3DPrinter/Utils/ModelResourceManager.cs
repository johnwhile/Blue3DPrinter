using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;

using Common;
using Common.Tools;

using UnityTool;

namespace Blue3DPrinter
{
    public static class ModelResourceManager
    {
        static ZipArchive archive;

        static HashSet<string> Exist = new HashSet<string>();


        static Dictionary<string, string> CaseSensitiveName = new Dictionary<string, string>();

        /// <summary>
        /// File already loaded
        /// </summary>
        static Dictionary<string, SceneTree> LoadedModels = new Dictionary<string, SceneTree>();

        /// <summary>
        /// Get the existing models in the storage
        /// </summary>
        public static HashSet<string> ModelFilenames => Exist;


        public static bool TryGetModel(string modelname, out SceneTree model)
        {
            //always fix name, can't exist duplicates
            modelname = modelname.ToLower();
            
            model = null;
            if (!Exist.Contains(modelname)) return false;

            if (!LoadedModels.TryGetValue(modelname, out model))
            {
                if (archive==null) throw new Exception("Archive not loaded");
                if (!CaseSensitiveName.TryGetValue(modelname, out string filename)) throw new Exception("casesensitivename dictionary wrong");

                var entry = archive.GetEntry(filename);

                if (entry != null)
                {
                    using (var stream = entry.Open())
                    using (var memory = new MemoryStream())
                    {
                        stream.CopyTo(memory);
                        memory.Position = 0;
                        using (BinaryReader reader = new BinaryReader(memory))
                        {
                            var empyrion = new EmpyrionModel();
                            if (!empyrion.Read(reader)) return false;
                            model = empyrion.Tree;
                            LoadedModels.Add(modelname, model);
                        }
                    }
                }
                else
                {
                    Debugg.Error($"ZipArchive return null for file {filename}");
                    return false;
                }
            }
            return true;
        }


        public static bool Open(string filename = "Shapes.zip")
        {
            if (!File.Exists(filename))
            {
                Debugg.Error("The Tool require the \"Shapes.zip\" model's storage to get the blueprints meshes");
                return false;
            }
            Close();
            archive = ZipFile.Open(filename, ZipArchiveMode.Read);

            foreach(var entry in archive.Entries)
            {
                string name = Path.GetFileNameWithoutExtension(entry.Name).ToLower();

                if (!Exist.Contains(name))
                {
                    Exist.Add(name);
                    CaseSensitiveName.Add(name, entry.Name);
                }
            }
            return true;
        }

        public static void Close()
        {
            archive?.Dispose();
            archive = null;
        }

        public static void Clear()
        {
            Close();
            Exist.Clear();
            LoadedModels.Clear();
            CaseSensitiveName.Clear();

        }
    }




/*
    /// <summary>
    /// Manager for "mystoragefilename.data" and "mystoragefilename.index" files.<br/>
    /// The main use is for <b><seealso cref="BlockCollection.LoadModels(FileStorageManager)"/></b>, 
    /// the method loads on demand the 3d object saved as <b><see cref="SceneFile"/></b> using <b><see cref="FileStorageManager.GetFile(string)"/></b>
    /// </summary>
    public class ModelResourceManager
    {
        string storageFilenameWithoutExt;

        public static Dictionary<string, string> FbxFiles;

        static ModelResourceManager()
        {
        }


        private ModelResourceManager(string storagefilename)
        {
            storageFilenameWithoutExt = storagefilename;
        }


        public static int SearchFbxFilsToOverrideVanillaModels(string directory)
        {
            LogMsg.Message("> search fbx files to override originals");
            
            if (!Directory.Exists(directory))
            {
                LogMsg.Error("> the directory \"" + Path.GetFullPath(directory) + " not exit !");
                return 0;
            }

            string[] fileEntries = Directory.GetFiles(directory, "*.fbx", SearchOption.AllDirectories);

            int count = fileEntries != null ? fileEntries.Length : 0;

            FbxFiles = new Dictionary<string, string>(count);
            if (count > 0)
            {
                foreach (var filename in fileEntries)
                {
                    FbxFiles.Add(Path.GetFullPath(filename), Path.GetFileNameWithoutExtension(filename));
                }
            }
            LogMsg.Message("> found " + count + " fbx files");

            return count;
        }


        public static ModelResourceManager Load(Blue3DPrinterForm myform, string storagefilename)
        {
            LogMsg.Message("> ModelResourceManager");

            ModelResourceManager manager = new ModelResourceManager(storagefilename);

            if (!File.Exists(storagefilename + ".data") || !File.Exists(storagefilename + ".index"))
            {
                DialogResult result = MessageBox.Show(myform,
                    "the file " + Path.GetFullPath(storagefilename) + ".data not found\n\n" +
                    "To speed up loading of geometry, the tool require the decompressed bundles files.\n\n" +
                     "create the files " + storagefilename + ".data and .index ? (it can take several minutes)", "Build my storage", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    foreach (string bundlefile in Program.BundlesFiles)
                    {
                        string bundleFilename = Path.Combine(Blue3DPrinter.Default.GameDirectory, Program.BundlesSubDirectory, bundlefile);

                        if (!File.Exists(bundleFilename))
                        {
                            MessageBox.Show(myform, "file not found :\n" + bundleFilename);
                            LogMsg.Error("file not found :\n" + bundleFilename);
                        }
                        else
                        {
                            LogMsg.Message("> uncompress file \"" + bundlefile + "\"", ConsoleColor.Yellow);

                            var list = ExtractResourcesFromBundles(bundleFilename);
                            if (list==null) return null;

                            if (!SaveToStorage(list.Values, storagefilename)) return null;
                        }
                    }
                    manager.PrintManifest();
                }
                return null;
            }
            else
            {
                LogMsg.Message("> my storage file " + storagefilename + " found, not need to create a new one", ConsoleColor.Green);
            }
            return manager;
        }

        public static bool SaveToStorage(IEnumerable<SceneFile> listOfFiles, string storageFilename)
        {
            FileStorageManager storage = FileStorageManager.LoadOrCreate(storageFilename);
            
            if (!storage.OpenAndLoad()) return false;

            foreach(var file in listOfFiles)
                storage.AppendFile(file.Name, file);
            storage.Close();

            return true;
        }


        /// <summary>
        /// Convert Empyrion's assets to my simple fast storage
        /// </summary>
        /// <param name="filename">the bundles full filename</param>
        /// <param name="mystoragefilename">the full filename of storage</param>
        /// <remarks>
        /// remember that this function generate 2 file:
        /// mystoragefilename.data
        /// mystoragefilename.index
        /// </remarks>
        public static Dictionary<string,SceneFile> ExtractResourcesFromBundles(string filename, string saveonlythis = null)
        {
            BundleUtilities assetool = new BundleUtilities();

            LogMsg.Message("> extracting...");

            List<GameObject> objects = assetool.GetMainObjects(filename);
            Dictionary<string, SceneFile> listOfFiles = new Dictionary<string, SceneFile>();

            if (objects == null || objects.Count < 1) return null;

            LogMsg.Message("> creating my files...");

            foreach(var obj in objects)
            {
                string name = obj.m_Name;

                if (saveonlythis==null || (saveonlythis!=null && name == saveonlythis))
                {
                    SceneFile scene = BundleUtilities.ConvertAssetToScene(obj);
                    if (scene != null)
                    {
                        if (scene.TotalObjects == 0) LogMsg.Message("> the file " + name + " not contain meshes", ConsoleColor.Red);
                        listOfFiles.Add(name, scene);
                    }
                }
            }

            LogMsg.Message("> done", ConsoleColor.Green);

            return listOfFiles;
        }

        public void PrintManifest()
        {
            if (!ExistStorage(storageFilenameWithoutExt)) return;

            FileStorageManager storage = FileStorageManager.LoadOrCreate(storageFilenameWithoutExt);
            if (storage == null) return;
            storage.OpenAndLoad();
            storage.PrintManifest(storageFilenameWithoutExt);
            storage.Close();
        }

        public FileStorageManager GetMyFileStorageManager()
        {
            if (!ExistStorage(storageFilenameWithoutExt)) return null;
            FileStorageManager storage = FileStorageManager.LoadOrCreate(storageFilenameWithoutExt);
            return storage;
        }

        private bool ExistStorage(string TheStorageFilenameWithoutExt)
        {
            string folder = Path.GetDirectoryName(storageFilenameWithoutExt);
            string filename = folder + "\\" + Path.GetFileNameWithoutExtension(storageFilenameWithoutExt) + ".data";
            return Directory.Exists(folder) && File.Exists(filename);
        }
    }
*/
}
