using System;
using System.Linq;
using CharacterCustomizationTool.Extensions;
using UnityEngine;

namespace CharacterCustomizationTool.FaceManagement
{
    public class FacePicker : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private Mesh[] _faceMeshes;

        private SkinnedMeshRenderer _faceRenderer;

        public string FaceName => _faceRenderer.sharedMesh.name.Split("_")[2].ToCapital();

        public void SetFaces(Mesh[] faceMeshes)
        {
            _faceMeshes = faceMeshes;
        }

        public void PickFace(FaceType faceType)
        {
            var faceMesh = _faceMeshes.FirstOrDefault(m => m.name.Contains(faceType.ToString(), StringComparison.InvariantCultureIgnoreCase));
            if (faceMesh == null)
            {
                throw new Exception($"Face not found: {faceType.ToString()}.");
            }

            SetFace(faceMesh);
        }

        public void NextFace()
        {
            ShiftFace(GetNextFaceIndex);
        }

        public void PreviousFace()
        {
            ShiftFace(GetPreviousFaceIndex);
        }

        private void ShiftFace(Func<int, int> indexCalculator)
        {
            var activeFaceIndex = FindActiveFaceIndex();
            var targetFaceIndex = indexCalculator(activeFaceIndex);

            SetFace(_faceMeshes[targetFaceIndex]);
        }

        private int FindActiveFaceIndex()
        {
            var activeFaceIndex = 0;
            for (var i = 0; i < _faceMeshes.Length; i++)
            {
                if (_faceMeshes[i].name == _faceRenderer.sharedMesh.name)
                {
                    activeFaceIndex = i;
                }
            }

            return activeFaceIndex;
        }

        private int GetNextFaceIndex(int activeFaceIndex)
        {
            activeFaceIndex++;
            if (activeFaceIndex >= _faceMeshes.Length)
            {
                activeFaceIndex = 0;
            }

            return activeFaceIndex;
        }

        private int GetPreviousFaceIndex(int activeFaceIndex)
        {
            activeFaceIndex--;
            if (activeFaceIndex < 0)
            {
                activeFaceIndex = _faceMeshes.Length - 1;
            }

            return activeFaceIndex;
        }

        private void SetFace(Mesh faceMesh)
        {
            _faceRenderer.sharedMesh = faceMesh;
        }

        private void Start()
        {
            ValidateFields();
        }

        private void OnValidate()
        {
            ValidateFields();
        }

        private void ValidateFields()
        {
            _faceRenderer = transform
                .Cast<Transform>()
                .First(t => t.name.StartsWith("Face"))
                .GetComponent<SkinnedMeshRenderer>();
        }
    }
}