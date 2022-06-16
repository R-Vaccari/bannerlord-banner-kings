using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using System.Collections.Generic;
using System.Xml;

namespace BannerKings.UI.Extensions
{

    [PrefabExtension("ClanScreen", "descendant::ClanScreenWidget[@Id='ClanScreenWidget']/Children/Widget[2]/Children", "ClanScreen")]
    internal class ClanManagementExtension : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Child;
        public override int Index => 4;

        private List<XmlNode> nodes;

        public ClanManagementExtension()
        {
            XmlDocument firstChild = new XmlDocument();
            firstChild.LoadXml("<ClanCourt DataSource=\"{Court}\" IsVisible=\"false\" />");

            nodes = new List<XmlNode> { firstChild };
        }

        [PrefabExtensionXmlNodes]
        public IEnumerable<XmlNode> Nodes => nodes;
        
    }

    [PrefabExtension("ClanScreen", "descendant::Widget[@VisualDefinition='TopPanel']/Children/Widget[1]/Children/ListPanel[1]/Children", "ClanScreen")]
    internal class ClanManagementExtension2 : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Child;
        public override int Index => 2;

        private List<XmlNode> nodes;

        public ClanManagementExtension2()
        {
            XmlDocument firstChild = new XmlDocument();
            firstChild.LoadXml("<ButtonWidget DoNotPassEventsToChildren=\"true\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"!Header.Tab.Center.Width.Scaled\" SuggestedHeight=\"!Header.Tab.Center.Height.Scaled\" PositionYOffset=\"6\" MarginRight=\"2\" Brush=\"Header.Tab.Center\" Command.Click=\"SelectCourt\" IsSelected=\"@CourtSelected\" UpdateChildrenStates=\"true\"><Children><TextWidget WidthSizePolicy = \"StretchToParent\" HeightSizePolicy = \"StretchToParent\" MarginTop = \"3\" Brush = \"Clan.TabControl.Text\" Text = \"@CourtText\" /></Children></ButtonWidget > ");

            nodes = new List<XmlNode> { firstChild };
        }

        [PrefabExtensionXmlNodes]
        public IEnumerable<XmlNode> Nodes => nodes;

    }
}
