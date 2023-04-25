using System.Collections.Generic;
using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace BannerKings.UI.Extensions
{
    [PrefabExtension("Inventory", "descendant::Widget[@Id='CharacterSelection']/Children",
        "Inventory")]
    internal class InventoryClearGearExtension : PrefabExtensionInsertPatch
    {
        private readonly List<XmlNode> nodes;

        public InventoryClearGearExtension()
        {
            var clearGear = new XmlDocument();
            clearGear.LoadXml("<ButtonWidget DataSource=\"{..}\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"35\" SuggestedHeight=\"35\" Brush=\"Inventory.SwapMountCharacter.Button\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" MarginLeft=\"575\" MarginBottom=\"90\" Command.Click=\"ExecuteClearGear\" ><Children><HintWidget DataSource=\"{ClearGearHint}\" DoNotAcceptEvents=\"true\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" Command.HoverBegin=\"ExecuteBeginHint\" Command.HoverEnd=\"ExecuteEndHint\" /></Children></ButtonWidget>");

            nodes = new List<XmlNode> {clearGear };
        }

        public override InsertType Type => InsertType.Child;
        public override int Index => 4;

        [PrefabExtensionXmlNodes] public IEnumerable<XmlNode> Nodes => nodes;
    }
}