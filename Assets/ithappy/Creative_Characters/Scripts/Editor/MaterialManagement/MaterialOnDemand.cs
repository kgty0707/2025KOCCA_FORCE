using System;
using UnityEditor;
using UnityEngine;

namespace CharacterCustomizationTool.Editor.MaterialManagement
{
    public class MaterialOnDemand
    {
        private readonly string[] _pathOptions;

        private Material _value;

        public Material Value => _value ? _value : LoadMaterial();

        public MaterialOnDemand(params string[] pathOptions)
        {
            _pathOptions = pathOptions;
        }

        private Material LoadMaterial()
        {
            foreach (var path in _pathOptions)
            {
                var loadedMaterial = AssetDatabase.LoadAssetAtPath<Material>(path);

                if (loadedMaterial != null)
                {
                    return loadedMaterial;
                }
            }

            throw new Exception();
        }
    }
}