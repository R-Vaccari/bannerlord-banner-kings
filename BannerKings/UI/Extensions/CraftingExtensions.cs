using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using System.Collections.Generic;
using System.Xml;

namespace BannerKings.UI.Extensions
{

    [PrefabExtension("Crafting", "descendant::Widget[@Id='RightPanel']/Children", "Crafting")]
    internal class CraftingInsertHoursExtension : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Child;
        public override int Index => 4;

        private List<XmlNode> nodes;

        public CraftingInsertHoursExtension()
        {
            XmlDocument firstChild = new XmlDocument();
            firstChild.LoadXml("<TextWidget Brush=\"Refinement.Amount.Text\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"500\" SuggestedHeight=\"50\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Bottom\" MarginBottom=\"65\" Text=\"@HoursSpentText\" />");

            nodes = new List<XmlNode> { firstChild };
        }

        [PrefabExtensionXmlNodes]
        public IEnumerable<XmlNode> Nodes => nodes;

    }

    [PrefabExtension("Crafting", "descendant::ListPanel[@Id='MainActionListPanel']/Children/ButtonWidget[1]", "Crafting")]
    internal class CraftingCancelButtonPatch : PrefabExtensionSetAttributePatch
    {
        
        public override List<Attribute> Attributes => new()
        {
            new Attribute("Command.Click", "CloseWithWait"),
        };
    }
}
