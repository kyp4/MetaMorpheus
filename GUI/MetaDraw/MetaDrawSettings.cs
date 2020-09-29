using EngineLayer;
using EngineLayer.GlycoSearch;
using Proteomics.Fragmentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace MetaMorpheusGUI
{
    public static class MetaDrawSettings
    {
        public static Dictionary<ProductType, double> productTypeToYOffset;
        public static Dictionary<ProductType, Color> productTypeToColor;
        public static Color variantCrossColor;
        public static SolidColorBrush modificationAnnotationColor;

        // filter settings
        public static bool ShowDecoys { get; private set; }
        public static bool ShowContaminants { get; private set; }
        public static double QValueFilter { get; private set; }
        public static LocalizationLevel LocalizationLevelStart { get; set; }
        public static LocalizationLevel LocalizationLevelEnd { get; set; }

        public static void SetUpDictionaries()
        {
            // colors of each fragment to annotate on base sequence
            productTypeToColor = ((ProductType[])Enum.GetValues(typeof(ProductType))).ToDictionary(p => p, p => Colors.Aqua);
            productTypeToColor[ProductType.b] = Colors.Blue;
            productTypeToColor[ProductType.y] = Colors.Purple;
            productTypeToColor[ProductType.zDot] = Colors.Orange;
            productTypeToColor[ProductType.c] = Colors.Gold;

            // offset for annotation on base sequence
            productTypeToYOffset = ((ProductType[])Enum.GetValues(typeof(ProductType))).ToDictionary(p => p, p => 0.0);
            productTypeToYOffset[ProductType.b] = 50;
            productTypeToYOffset[ProductType.y] = 0;
            productTypeToYOffset[ProductType.c] = 53.6;
            productTypeToYOffset[ProductType.zDot] = -3.6;
        }

        public static bool FilterAcceptsPsm(PsmFromTsv psm)
        {
            return true;
        }
    }
}
