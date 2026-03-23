using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityTool
{
    public class BuildVersion
    {
        public int Major;
        public int Minor;
        public int Build;
        public int Revision;
        public BuildType Type;

        public bool IsPatch => Type == BuildType.Patch;

        public BuildVersion(string parse)
        {
            var split = parse.Split(new char[] { '.' });
            int.TryParse(split[0], out Major);
            int.TryParse(split[1], out Minor);

            //not sure if work
            try
            {
                string split2 = split[2];
                int i = 0;
                for (int j = 0; j < split2.Length; j++)
                {
                    var c = split2[j];
                    if (char.IsLetter(c))
                    {
                        switch (c)
                        {
                            case 'f': Type = BuildType.Release; break;
                            case 'p': Type = BuildType.Patch; break;
                            case 'b': Type = BuildType.Beta; break;
                            case 'a': Type = BuildType.Alpha; break;
                        }
                        i = j;
                    }
                }
                int.TryParse(split2.Substring(0, i), out Build);
                int.TryParse(split2.Substring(i + 1, split.Length - i), out Revision);
            }
            catch
            {
                Build = 0;
                Revision = 0;
                Type = BuildType.Unknow;
            }
        }
        public BuildVersion(int major, int minor, int build, int revision)
        {
            Major = major;
            Minor = minor;
            Build = build;
            Revision = revision;
        }

        public bool IsGreater(int major, int minor = 0) => Major > major || (Major == major && Minor > minor);
        public bool IsGreaterEqual(int major, int minor = 0) => Major > major || (Major == major && Minor >= minor);
        public bool IsEqualOrEarlier(int major, int minor = 0) => Major < major || (Major == major && Minor <= minor);
        public bool IsEqualOrDownVersion(int major, int minor) => Major == major && Minor <= minor;
        public bool IsEarlier(int major, int minor = 0) => Major < major || (Major == major && Minor < minor);

        /// <summary>
        /// a negative value mean is valid for any values
        /// </summary>
        public bool IsVersion(int major = -1, int minor = -1, int build = -1)
        {
            if (major < 0) return true;
            else if (minor < 0) return major == Major;
            else if (build < 0) return major == Major && minor == Minor;
            else return major == Major && minor == Minor && build == Build;
        }

    }
        
    public enum BuildType : byte
    {
        Unknow,
        Release,
        Patch,
        Beta,
        Alpha
    }



    public enum BuildTarget : int
    {
        NoTarget = -2,
        DashboardWidget = 1,
        StandaloneOSX = 2,
        StandaloneOSXPPC = 3,
        StandaloneOSXIntel = 4,
        StandaloneWindows,
        WebPlayer,
        WebPlayerStreamed,
        Wii = 8,
        iOS = 9,
        PS3,
        XBOX360,
        Android = 13,
        StandaloneGLESEmu = 14,
        NaCl = 16,
        StandaloneLinux = 17,
        FlashPlayer = 18,
        StandaloneWindows64 = 19,
        WebGL,
        WSAPlayer,
        StandaloneLinux64 = 24,
        StandaloneLinuxUniversal,
        WP8Player,
        StandaloneOSXIntel64,
        BlackBerry,
        Tizen,
        PSP2,
        PS4,
        PSM,
        XboxOne,
        SamsungTV,
        N3DS,
        WiiU,
        tvOS,
        Switch,
        Lumin,
        Stadia,
        CloudRendering,
        GameCoreXboxSeries,
        GameCoreXboxOne,
        PS5,
        UnknownPlatform = 9999
    }
}

