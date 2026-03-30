using Common;
using Common.Maths;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Blue3DPrinter
{
    public static class AppSetting
    {
        public static string GameDirectory = @"E:\SteamLibrary\steamapps\common\Empyrion - Galactic Survival";
        public static string BlockConfigPath = @"Content\Configuration\BlocksConfig.ecf";
        public static string BundlesSubDirectory = @"Content\Bundles";
        public static List<string> BundlesFiles = new List<string> { "shapes", "models" };
        public static int FileVersion = 31;

        static AppSetting()
        {
            string filename = "Setting.ini";

            if (File.Exists(filename))
            {
                using (var reader = XmlReader.Create(filename))
                {
                    while (reader.Read())
                    {
                        switch (reader.Name)
                        {
                            case "GameDirectory":
                                GameDirectory = reader.ReadElementContentAsString();
                                break;
                            case "BlockConfigPath":
                                BlockConfigPath = reader.ReadElementContentAsString();
                                break;
                            case "BundlesSubDirectory":
                                BundlesSubDirectory = reader.ReadElementContentAsString();
                                break;
                            case "FileVersion":
                                FileVersion = reader.ReadElementContentAsInt();
                                break;
                        }
                    }
                }
            }
            else
            {
                Save(filename);
            }
        }

        public static void Save(string filename = "Setting.ini")
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            settings.NewLineOnAttributes = true;

            using (var writer = XmlWriter.Create(filename, settings))
            {
                writer.WriteComment("Application setting XML format");
                writer.WriteStartElement("Setting");
                writer.WriteElementString("GameDirectory", GameDirectory);
                writer.WriteElementString("BlockConfigPath", BlockConfigPath);
                writer.WriteElementString("BundlesSubDirectory", BundlesSubDirectory);
                writer.WriteElementString("FileVersion", FileVersion.ToString());
                writer.WriteEndElement();
                writer.Flush();
            }
        }
    }


    public static class Program
    {

        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //output message write also to console
            Debugg.ToConsole = true;


            Vector3f.CHECK_ZEROLENGHT_WHEN_NORMALIZE = false;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Blue3DPrinterForm());

            AppSetting.Save();
        }
    }
}
