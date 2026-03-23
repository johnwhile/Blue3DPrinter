using System;

namespace UnityTool
{
    [Flags]
    public enum BundleFlags : uint
    {
        /// <summary>
        /// 0000 0000 0011 1111
        /// </summary>
        CompressionMask = 0x3F,
        BlocksAndDirectoryInfoCombined = 0x40,
        BlocksInfoAtTheEnd = 0x80,
        OldWebPluginCompatibility = 0x100,
        /// <summary>
        /// Padding is added after blocks info, so files within asset bundles start on aligned boundaries.
        /// </summary>
        BlockInfoNeedPaddingAtStart = 0x200,
        /// <summary>
        /// Chinese encryption flag prior to 2020.3.34f1, 2021.3.2f1, 2022.1.1f1.
        /// </summary>
        EncryptionOld = 0x200,
        /// <summary>
        /// Chinese encryption flag (presumeably) after 2020.3.34f1, 2021.3.2f1, 2022.1.1f1.
        /// </summary>
        EncryptionNew = 0x400,
    }

    public static class BundleFlagExtension
    {
        public static CompressionMode GetCompression(this BundleFlags flag) =>
            (CompressionMode)(flag & BundleFlags.CompressionMask);

        public static BundleFlags SetCompression(this BundleFlags flag, CompressionMode value) =>
            flag & ~BundleFlags.CompressionMask | (BundleFlags)value;

        public static bool GetBlockInfoNeedPaddingAtStart(this BundleFlags flag) =>
            (flag & BundleFlags.BlockInfoNeedPaddingAtStart) != 0;

        public static bool GetBlocksAndDirectoryInfoCombined(this BundleFlags flag) =>
            (flag & BundleFlags.BlocksAndDirectoryInfoCombined) != 0;

        public static BundleFlags SetBlocksAndDirectoryInfoCombined(this BundleFlags flag, bool value)
        {
            if (value)
            {
                flag = flag.SetBlocksInfoAtTheEnd(false);
                return flag | BundleFlags.BlocksAndDirectoryInfoCombined;
            }
            return flag & ~BundleFlags.BlocksAndDirectoryInfoCombined;
        }

        public static bool GetBlocksInfoAtTheEnd(this BundleFlags flag) =>
            (flag & BundleFlags.BlocksInfoAtTheEnd) != 0;

        public static BundleFlags SetBlocksInfoAtTheEnd(this BundleFlags flag, bool value)
        {
            if (value)
            {
                flag = flag.SetBlocksAndDirectoryInfoCombined(false);
                return flag | BundleFlags.BlocksInfoAtTheEnd;
            }
            return flag & ~BundleFlags.BlocksInfoAtTheEnd;
        }
    }


    /// <summary>
    /// ArchiveCompressionTypeMask and StorageBlockCompressionTypeMask<br/>
    /// mask = 0011 1111
    /// </summary>
    public enum CompressionMode : byte
    {
        NONE = 0,
        LZMA = 1,
        LZ4 = 2,
        LZ4HC = 3
    }

    public enum BundleVersion : uint
    {
        Unknown = 0,

        BF_100_250 = 1,
        BF_260_340 = 2,
        BF_350_4x = 3,
        BF_520a1 = 4,
        BF_520aunk = 5,
        BF_520_x = 6,
        /// <summary>
        /// Several 4-byte integers were upgraded to 8-byte integers in order to support files larger than 2 GB.
        /// </summary>
        BF_LargeFilesSupport = 7,
        /// <summary>
        /// This seems to be exactly the same as <see cref="BF_LargeFilesSupport"/>.
        /// </summary>
        BF_2022_2 = 8,
    }
}
