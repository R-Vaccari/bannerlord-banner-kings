using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using System.Collections.Generic;
using System.Xml;

namespace BannerKings.UI.Extensions
{



    [PrefabExtension("Crafting", "descendant::ListPanel[@Id='CategoryParent']/Children", "Crafting")]
    internal class CraftingInsertArmorCategoryExtension : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Child;
        public override int Index => 2;

        private List<XmlNode> nodes;

        public CraftingInsertArmorCategoryExtension()
        {
            XmlDocument firstChild = new XmlDocument();
            firstChild.LoadXml("<ButtonWidget Id=\"CraftingArmorCategoryButton\" DoNotPassEventsToChildren=\"true\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"380\" SuggestedHeight=\"136\" Brush=\"Crafting.CraftingTab.Button\" Command.Click=\"ExecuteSwitchToArmor\" IsSelected=\"@IsInArmorMode\" UpdateChildrenStates=\"true\"><Children><ListPanel DoNotAcceptEvents = \"true\" WidthSizePolicy = \"StretchToParent\" HeightSizePolicy = \"StretchToParent\" HorizontalAlignment = \"Center\" VerticalAlignment = \"Center\" MarginTop = \"7\" StackLayout.LayoutMethod = \"VerticalBottomToTop\" UpdateChildrenStates = \"true\"><Children><BrushWidget WidthSizePolicy = \"Fixed\" HeightSizePolicy = \"Fixed\" SuggestedWidth = \"33\" SuggestedHeight = \"29\" HorizontalAlignment = \"Center\" VerticalAlignment = \"Top\" Brush = \"Crafting.Craft.Icon\" /><TextWidget WidthSizePolicy = \"StretchToParent\" HeightSizePolicy = \"StretchToParent\" MarginLeft = \"100\" MarginRight = \"100\" MarginBottom = \"60\" Brush = \"Crafting.Tabs.Text\" Text = \"@ArmorText\" /></Children></ListPanel><HintWidget DataSource = \"{CraftingArmorHint}\" WidthSizePolicy = \"StretchToParent\" HeightSizePolicy = \"StretchToParent\" Command.HoverBegin = \"ExecuteBeginHint\" Command.HoverEnd = \"ExecuteEndHint\" IsEnabled = \"false\" /></Children></ButtonWidget>");

            nodes = new List<XmlNode> { firstChild };
        }

        [PrefabExtensionXmlNodes]
        public IEnumerable<XmlNode> Nodes => nodes;

    }



    [PrefabExtension("Crafting", "descendant::Widget[@Id='RightPanel']/Children", "Crafting")]
    internal class CraftingInsertHoursExtension : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Child;
        public override int Index => 3;

        private List<XmlNode> nodes;

        public CraftingInsertHoursExtension()
        {
            XmlDocument armorCategory = new XmlDocument();
            armorCategory.LoadXml("<Widget DoNotAcceptEvents=\"true\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" IsVisible=\"@IsInArmorMode\"><Children><ArmorCraftingCategory Id =\"ArmorCraftingCategoryParent\" DataSource=\"{ArmorCrafting}\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\"/></Children></Widget>");

            XmlDocument hoursText = new XmlDocument();
            hoursText.LoadXml("<TextWidget Brush=\"Crafting.Material.Text\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"500\" SuggestedHeight=\"50\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Bottom\" MarginBottom=\"65\" Text=\"@HoursSpentText\" />");

            nodes = new List<XmlNode> { armorCategory, hoursText };
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
