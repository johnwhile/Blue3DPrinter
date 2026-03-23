
namespace UnityTool
{
    public enum AssetVersion
    {
        kUnsupported = 1,
        kUnknown_2 = 2,
        kUnknown_3 = 3,
        kUnknown_5 = 5,// 1.2.0 to 2.0.0
        kUnknown_6 = 6,// 2.1.0 to 2.6.1
        kUnknown_7 = 7,// 3.0.0b
        kUnknown_8 = 8,// 3.0.0 to 3.4.2
        kUnknown_9 = 9,// 3.5.0 to 4.7.2
        kUnknown_10 = 10,// 5.0.0aunk1
        kHasScriptTypeIndex = 11,// 5.0.0aunk2
        kUnknown_12 = 12,// 5.0.0aunk3
        kHasTypeTreeHashes = 13,// 5.0.0aunk4
        kUnknown_14 = 14,// 5.0.0unk
        kSupportsStrippedObject = 15,// 5.0.1 to 5.4.0
        kRefactoredClassId = 16,// 5.5.0a
        kRefactorTypeData = 17,// 5.5.0unk to 2018.4
        kRefactorShareableTypeTreeData = 18,// 2019.1a
        kTypeTreeNodeWithTypeFlags = 19,// 2019.1unk
        kSupportsRefObject = 20,// 2019.2
        kStoresTypeDependencies = 21,// 2019.3 to 2019.4
        kLargeFilesSupport = 22// 2020.1 to x
    }
}
