using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CharacterCustomizationTool.Editor.FaceEditor;
using CharacterCustomizationTool.Editor.MaterialManagement;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace CharacterCustomizationTool.Editor
{
    public class CharacterCustomizationWindow : EditorWindow
    {
        private readonly List<List<SavedPart>> _savedCombinations = new();

        private readonly MaterialProvider _materialProvider = new();

        private PartsEditor _partsEditor;
        private Transform _cameraPivot;
        private Camera _camera;
        private RenderTexture _renderTexture;
        private List<Part> _parts;
        private string _prefabPath;

        private IEnumerable<Part> Parts => _parts ??= LoadParts().ToList();

        [MenuItem("Tools/Character Customization")]
        private static void Init()
        {
            var window = GetWindow<CharacterCustomizationWindow>("Character Customization");
            window.minSize = new Vector2(1150, 720);
            window.Show();
        }

        private void OnEnable()
        {
            _partsEditor = new PartsEditor();
        }

        private void OnGUI()
        {
            var rect = new Rect(10, 10, 300, 300);

            CreateRenderTexture();
            InitializeCamera();
            DrawMesh();
            _partsEditor.OnGUI(new Rect(330, 10, position.width - 330, position.height), Parts);

            GUI.DrawTexture(rect, _renderTexture, ScaleMode.StretchToFill, false);

            GUI.Label(new Rect(10, 320, 100, 25), "Prefab folder:");
            GUI.Label(new Rect(10, 345, 350, 25), AssetsPath.SavedCharacters);
            _prefabPath = GUI.TextField(new Rect(10, 372, 300, 20), _prefabPath);

            var saveButtonRect = new Rect(10, 400, 300, 40);
            if (GUI.Button(saveButtonRect, "Save Prefab"))
            {
                SavePrefab();
            }

            var randomizeButtonRect = new Rect(85, 450, 150, 30);
            if (GUI.Button(randomizeButtonRect, "Randomize"))
            {
                Randomize();
            }

            var isZero = _savedCombinations.Count == 0;
            var isSame = false;
            var lessThenTwo = false;

            if (!isZero)
            {
                isSame = IsSame();
                lessThenTwo = _savedCombinations.Count < 2;
            }

            using (new EditorGUI.DisabledScope(isZero || (isSame && lessThenTwo)))
            {
                var lastButtonRect = new Rect(240, 450, 50, 30);
                if (GUI.Button(lastButtonRect, "Last"))
                {
                    Last();
                }
            }
        }

        private void SavePrefab()
        {
            var characterFbx = LoadBaseMesh();
            var character = Instantiate(characterFbx, Vector3.zero, Quaternion.identity);
            foreach (Transform child in character.transform)
            {
                if (child.TryGetComponent<SkinnedMeshRenderer>(out var skinnedMeshRenderer))
                {
                    var childName = child.name.Split('_').First();
                    var part = _parts.First(part => part.IsOfType(childName));
                    skinnedMeshRenderer.sharedMesh = part.IsEnabled ? part.SelectedVariant.Mesh : null;
                    if (skinnedMeshRenderer.sharedMesh)
                    {
                        ConfigureMaterials(skinnedMeshRenderer, skinnedMeshRenderer.sharedMesh.name);
                    }
                }
                else if (child.TryGetComponent<MeshRenderer>(out var meshRenderer) && meshRenderer.TryGetComponent<MeshFilter>(out var meshFilter))
                {
                    var childName = child.name.Split('_').First();
                    var part = _parts.FirstOrDefault(part => part.Type.ToString().StartsWith(childName));
                    meshFilter.sharedMesh = part.IsEnabled ? part.SelectedVariant.Mesh : null;
                    if (meshFilter.sharedMesh)
                    {
                        ConfigureMaterials(meshRenderer, meshFilter.sharedMesh.name);
                    }
                }
            }

            AddAnimator(character);
            FaceLoader.AddFaces(character);

            var prefabPath = AssetsPath.SavedCharacters + _prefabPath;
            Directory.CreateDirectory(prefabPath);
            var path = AssetDatabase.GenerateUniqueAssetPath($"{prefabPath}/Character.prefab");
            PrefabUtility.SaveAsPrefabAsset(character, path);
            DestroyImmediate(character);
        }

        private static void AddAnimator(GameObject character)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetsPath.AnimationController);
            var characterAnimator = character.GetComponent<Animator>();
            characterAnimator.runtimeAnimatorController = controller;
        }

        private async void Randomize()
        {
            foreach (var part in _parts)
            {
                if (Random.value < .5f && part.Type != PartType.Body)
                {
                    part.IsEnabled = false;
                }
                else
                {
                    part.IsEnabled = true;
                    part.SelectedVariant = part.Variants[Random.Range(0, part.Variants.Count)];
                }
            }

            await Task.Delay(1);

            SaveCombination();
        }

        private void SaveCombination()
        {
            var savedCombinations = new List<SavedPart>();
            foreach (var part in _parts)
            {
                var savedCombination = new SavedPart(part.Type, part.IsEnabled, part.VariantIndex);
                savedCombinations.Add(savedCombination);
            }
            _savedCombinations.Add(savedCombinations);

            while (_savedCombinations.Count > 4)
            {
                _savedCombinations.RemoveAt(0);
            }
        }

        private void Last()
        {
            var lastSavedCombination = _savedCombinations.Last();
            if (IsSame())
            {
                _savedCombinations.Remove(lastSavedCombination);
                lastSavedCombination = _savedCombinations.Last();
            }

            foreach (var part in _parts)
            {
                var savedCombination = lastSavedCombination.Find(c => c.PartType == part.Type);

                part.IsEnabled = savedCombination.IsEnabled;
                part.SelectVariant(savedCombination.VariantIndex);
            }

            _savedCombinations.Remove(lastSavedCombination);
        }

        private bool IsSame()
        {
            var lastSavedCombination = _savedCombinations.Last();
            foreach (var part in _parts)
            {
                var savedCombination = lastSavedCombination.Find(c => c.PartType == part.Type);

                if (part.IsEnabled != savedCombination.IsEnabled ||
                    part.VariantIndex != savedCombination.VariantIndex)
                {
                    return false;
                }
            }

            return true;
        }

        private void InitializeCamera()
        {
            if (_camera)
            {
                return;
            }

            _cameraPivot = new GameObject("CameraPivot").transform;
            _cameraPivot.gameObject.hideFlags = HideFlags.HideAndDontSave;

            var cameraObject = new GameObject("PreviewCamera")
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            _camera = cameraObject.AddComponent<Camera>();
            _camera.targetTexture = _renderTexture;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.renderingPath = RenderingPath.Forward;
            _camera.enabled = false;
            _camera.useOcclusionCulling = false;
            _camera.cameraType = CameraType.Preview;
            _camera.fieldOfView = 4.5f;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.transform.SetParent(_cameraPivot);

            _cameraPivot.Rotate(Vector3.up, 150, Space.Self);
        }

        private void CreateRenderTexture()
        {
            if (_renderTexture)
            {
                return;
            }

            _renderTexture = new RenderTexture(300, 300, 30, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 8
            };
        }

        private void DrawMesh()
        {
            _camera.transform.localPosition = new Vector3(0, 1.1f, -36);

            foreach (var part in Parts.Where(part => part.IsEnabled))
            {
                DrawModel(part.SelectedVariant.Mesh);
            }

            _camera.Render();
        }

        private void DrawModel(Mesh mesh)
        {
            switch (mesh.name)
            {
                case "Astronaut_001":
                    DrawSubMesh(mesh, _materialProvider.MainColor, 0);
                    DrawSubMesh(mesh, _materialProvider.Glass, 1);
                    break;
                case "Sushi_001":
                    DrawSubMesh(mesh, _materialProvider.MainColor, 0);
                    DrawSubMesh(mesh, _materialProvider.Glass, 1);
                    DrawSubMesh(mesh, _materialProvider.Emission, 2);
                    break;
                default:
                    DrawSubMesh(mesh, _materialProvider.MainColor, 0);
                    break;
            }
        }

        private void DrawSubMesh(Mesh mesh, Material material, int subMeshIndex)
        {
            Graphics.DrawMesh(mesh, new Vector3(0, -.01f, 0), Quaternion.identity, material, 31, _camera, subMeshIndex);
        }

        private IEnumerable<Part> LoadParts()
        {
            var baseMesh = LoadBaseMesh();
            var partNames = GetPartNames(baseMesh);
            var parts = CreateParts(partNames);
            var sortedParts = SortParts(parts);

            return sortedParts;
        }

        private static GameObject LoadBaseMesh()
        {
            var availableBaseMeshes = new List<GameObject>();

            foreach (var keyword in AssetsPath.BaseMesh.Keywords)
            {
                var baseMeshes = AssetLoader.LoadAssets<GameObject>(keyword, AssetsPath.BaseMesh.Path);
                availableBaseMeshes.AddRange(baseMeshes);
            }

            var baseMesh = availableBaseMeshes.First();

            return baseMesh;
        }

        private static List<PartName> GetPartNames(GameObject baseMesh)
        {
            var partNames = baseMesh.transform
                .Cast<Transform>()
                .Where(mesh => mesh.TryGetComponent<Renderer>(out _))
                .Select(mesh => new PartName(mesh.name)).ToList();

            return partNames;
        }

        private List<Part> CreateParts(List<PartName> partNames)
        {
            var parts = new List<Part>();

            foreach (var partName in partNames)
            {
                var partMeshes = GetPartMeshes(partName);

                Part part;
                if (partName.IsOfType(PartType.FullBody))
                {
                    part = GetFullBodyPart();
                    part.IsEnabled = false;
                }
                else
                {
                    var variants = partMeshes.Select(m => new Variant(m, CreateVariantPreview(m))).ToList();
                    part = new Part(partName.GetPartType(), variants);
                }

                parts.Add(part);
            }

            return parts;
        }

        private static IEnumerable<Mesh> GetPartMeshes(PartName partName)
        {
            var path = GetPartFolderPath();
            var meshes = AssetLoader.LoadAssets<Mesh>("t:mesh", path);

            return meshes;

            string GetPartFolderPath()
            {
                var subFolders = AssetDatabase.GetSubFolders(AssetsPath.Folder.Parts);

                foreach (var subFolder in subFolders)
                {
                    if (partName.IsValidPath(subFolder))
                    {
                        return subFolder;
                    }
                }

                throw new Exception();
            }
        }

        private static IEnumerable<Part> SortParts(IEnumerable<Part> parts)
        {
            var sortedParts = Enum.GetValues(typeof(PartType))
                .Cast<PartType>()
                .Select(type => parts.FirstOrDefault(p => p.IsOfType(type)))
                .Where(part => part != null)
                .ToList();

            return sortedParts;
        }

        private Part GetFullBodyPart()
        {
            var assets = new List<Object>();

            assets.AddRange(AssetDatabase.FindAssets("t:mesh", new[] { AssetsPath.FullBody })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAllAssetsAtPath)
                .SelectMany(assetsOfFbx => assetsOfFbx));

            var meshes = new List<Mesh>();
            foreach (var asset in assets)
            {
                if (asset is Mesh m)
                {
                    meshes.Add(m);
                }
            }
            var variants = meshes.Select(m => new Variant(m, CreateVariantPreview(m))).ToList();
            var part = new Part(PartType.FullBody, variants);

            return part;
        }

        private GameObject CreateVariantPreview(Mesh mesh)
        {
            var variant = new GameObject(mesh.name);
            variant.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = variant.AddComponent<MeshRenderer>();
            ConfigureMaterials(renderer, mesh.name);
            variant.transform.position = Vector3.one * int.MaxValue;
            variant.hideFlags = HideFlags.HideAndDontSave;

            return variant;
        }

        private void ConfigureMaterials(Renderer renderer, string meshName)
        {
            switch (meshName)
            {
                case "Astronaut_001":
                    renderer.sharedMaterials = new[] { _materialProvider.MainColor, _materialProvider.Glass };
                    break;
                case "Sushi_001":
                    renderer.sharedMaterials = new[] { _materialProvider.MainColor, _materialProvider.Glass, _materialProvider.Emission };
                    break;
                default:
                    renderer.sharedMaterial = _materialProvider.MainColor;
                    break;
            }
        }
    }
}