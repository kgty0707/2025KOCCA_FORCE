namespace CharacterCustomizationTool.Editor
{
    public readonly struct SavedPart
    {
        public readonly PartType PartType;
        public readonly bool IsEnabled;
        public readonly int VariantIndex;

        public SavedPart(PartType partType, bool isEnabled, int variantIndex)
        {
            PartType = partType;
            IsEnabled = isEnabled;
            VariantIndex = variantIndex;
        }
    }
}