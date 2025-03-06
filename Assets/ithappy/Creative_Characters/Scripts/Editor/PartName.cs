using System;
using System.Linq;

namespace CharacterCustomizationTool.Editor
{
    public class PartName
    {
        private readonly string[] _nameSections;

        public PartName(string name)
        {
            _nameSections = SplitName(name);
        }

        public PartType GetPartType()
        {
            var fullName = string.Join("", _nameSections);
            if (Enum.TryParse(fullName, true, out PartType partType))
            {
                return partType;
            }

            throw new Exception();
        }

        public bool IsOfType(PartType expectedType)
        {
            var partType = GetPartType();

            return partType == expectedType;
        }

        public bool IsValidPath(string path)
        {
            var isValidPath = _nameSections.All(s => path.Contains(s, StringComparison.InvariantCultureIgnoreCase));

            return isValidPath;
        }

        private static string[] SplitName(string name)
        {
            var nameParts = name.Split('_', '-');

            return nameParts;
        }
    }
}