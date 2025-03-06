using UnityEngine;

namespace CharacterCustomizationTool.Editor
{
    public class Variant
    {
        public Mesh Mesh { get; }
        public GameObject PreviewObject { get; }
        public string Name => Mesh.name;

        public Variant(Mesh mesh, GameObject previewObject)
        {
            Mesh = mesh;
            PreviewObject = previewObject;
        }
    }
}