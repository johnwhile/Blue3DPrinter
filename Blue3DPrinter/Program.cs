using Common;
using System;
using System.IO;
using System.Windows.Forms;

namespace Blue3DPrinter
{
    public static class Program
    {

        /// <summary>
        /// path of BlockConfig.ecf
        /// </summary>
        public static string BlockConfigPath = @"Content\Configuration\BlocksConfig.ecf";
        public static string BundlesSubDirectory = @"Content\Bundles";

        /// <summary>
        /// List of all Bundles files
        /// </summary>
        public static string[] BundlesFiles = new string[] 
        {
            "shapes",
            "models", 
            //"models2",
        };

        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //output message write also to console
            Debugg.ToConsole = true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Blue3DPrinterForm());
        }
    }
}
