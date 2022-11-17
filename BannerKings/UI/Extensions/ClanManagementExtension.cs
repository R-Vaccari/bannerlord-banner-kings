using System.Collections.Generic;
using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace BannerKings.UI.Extensions
{
    [PrefabExtension("ClanScreen", "descendant::ClanScreenWidget[@Id='ClanScreenWidget']/Children/Widget[2]/Children",
        "ClanScreen")]
    internal class ClanManagementExtension : PrefabExtensionInsertPatch
    {
        private readonly List<XmlNode> nodes;

        public ClanManagementExtension()
        {
            var firstChild = new XmlDocument();
            firstChild.LoadXml("<ClanCourt DataSource=\"{Court}\" IsVisible=\"false\" />");

            nodes = new List<XmlNode> {firstChild};
        }

        public override InsertType Type => InsertType.Child;
        public override int Index => 4;

        [PrefabExtensionXmlNodes] public IEnumerable<XmlNode> Nodes => nodes;
    }

    [PrefabExtension("ClanScreen",
        "descendant::Widget[@VisualDefinition='TopPanel']/Children/Widget[1]/Children/ListPanel[1]/Children", "ClanScreen")]
    internal class ClanManagementExtension2 : PrefabExtensionInsertPatch
    {
        private readonly List<XmlNode> nodes;

        public ClanManagementExtension2()
        {
            var firstChild = new XmlDocument();
            firstChild.LoadXml(
                "<ButtonWidget DoNotPassEventsToChildren=\"true\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" IsEnabled=\"@CourtEnabled\" SuggestedWidth=\"!Header.Tab.Center.Width.Scaled\" SuggestedHeight=\"!Header.Tab.Center.Height.Scaled\" PositionYOffset=\"6\" MarginRight=\"2\" Brush=\"Header.Tab.Center\" Command.Click=\"SelectCourt\" IsSelected=\"@CourtSelected\" UpdateChildrenStates=\"true\"><Children><TextWidget WidthSizePolicy = \"StretchToParent\" HeightSizePolicy = \"StretchToParent\" MarginTop = \"3\" Brush = \"Clan.TabControl.Text\" Text = \"@CourtText\" /></Children></ButtonWidget > ");

            nodes = new List<XmlNode> {firstChild};
        }

        public override InsertType Type => InsertType.Child;
        public override int Index => 2;

        [PrefabExtensionXmlNodes] public IEnumerable<XmlNode> Nodes => nodes;
    }

    [PrefabExtension("ClanScreen", "descendant::Widget[@Id='FinancePanelWidget']", "ClanScreen")]
    internal class AddMultipleAttributesExamplePatch : PrefabExtensionSetAttributePatch
    {
        public override List<Attribute> Attributes => new()
        {
            new Attribute("IsVisible", "@FinancesVisible")
        };
    }
}