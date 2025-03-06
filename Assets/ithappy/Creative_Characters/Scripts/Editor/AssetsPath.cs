namespace CharacterCustomizationTool.Editor
{
    public static class AssetsPath
    {
        public const string FullBody = Root + "Meshes/Full_Body";
        public const string AnimationController = Root + "Animations/AnimationController.controller";
        public const string SavedCharacters = Root + "Saved_Characters/";

        private const string Root = "Assets/ithappy/" + PackageName + "/";
        private const string PackageName = "Creative_Characters";

        public static class BaseMesh
        {
            public const string Path = Root + "Meshes/";

            public static readonly string[] Keywords =
            {
                "Base",
                "Basic"
            };
        }

        public static class Folder
        {
            public const string Materials = Root + "Materials/";
            public const string Parts = Root + "Meshes";
            public const string Faces = Root + "Meshes/Faces/";
        }
    }
}