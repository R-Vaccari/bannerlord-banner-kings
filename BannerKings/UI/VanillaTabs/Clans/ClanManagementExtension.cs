using System.Collections.Generic;
using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace BannerKings.UI.VanillaTabs.Clans
{
    [PrefabExtension("ClanScreen", "descendant::ClanScreenWidget[@Id='ClanScreenWidget']/Children/Widget[2]/Children",
        "ClanScreen")]
    internal class ClanManagementExtension : PrefabExtensionInsertPatch
    {
        private readonly List<XmlNode> nodes;

        public ClanManagementExtension()
        {
            var court = new XmlDocument();
            var demesne = new XmlDocument();
            court.LoadXml("<ClanCourt DataSource=\"{Court}\" IsVisible=\"false\" />");
            demesne.LoadXml("<ClanDemesne DataSource=\"{Demesne}\" IsVisible=\"false\" />");

            nodes = new List<XmlNode> { court, demesne };
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
                "<ButtonWidget DoNotPassEventsToChildren=\"true\" IsVisible=\"@CourtEnabled\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"!Header.Tab.Center.Width.Scaled\" SuggestedHeight=\"!Header.Tab.Center.Height.Scaled\" PositionYOffset=\"6\" MarginRight=\"2\" Brush=\"Header.Tab.Center\" Command.Click=\"SelectCourt\" IsSelected=\"@CourtSelected\" UpdateChildrenStates=\"true\"><Children><TextWidget WidthSizePolicy = \"StretchToParent\" HeightSizePolicy = \"StretchToParent\" MarginTop = \"3\" Brush = \"Clan.TabControl.Text\" Text = \"@CourtText\" /></Children></ButtonWidget>");
            var demesneButton = new XmlDocument();
            demesneButton.LoadXml(
                "<ButtonWidget DoNotPassEventsToChildren=\"true\" IsVisible=\"@DemesneEnabled\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"!Header.Tab.Center.Width.Scaled\" SuggestedHeight=\"!Header.Tab.Center.Height.Scaled\" PositionYOffset=\"6\" MarginRight=\"2\" Brush=\"Header.Tab.Center\" Command.Click=\"SelectDemesne\" IsSelected=\"@DemesneSelected\" UpdateChildrenStates=\"true\"><Children><TextWidget WidthSizePolicy = \"StretchToParent\" HeightSizePolicy = \"StretchToParent\" MarginTop = \"3\" Brush = \"Clan.TabControl.Text\" Text = \"@DemesneText\" /></Children></ButtonWidget>");

            nodes = new List<XmlNode> { firstChild, demesneButton };
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

    [PrefabExtension("ClanScreen",
        "descendant::Widget[@VisualDefinition='TopPanel']/Children/Widget[1]/Children/ListPanel[1]", "ClanScreen")]
    internal class TabListAttribute : PrefabExtensionSetAttributePatch
    {
        public override List<Attribute> Attributes => new()
        {
            new Attribute("WidthSizePolicy", "StretchToParent"),
            new Attribute("MarginRight", "350"),
            new Attribute("MarginLeft", "300")
        };
    }

    [PrefabExtension("ClanScreen",
        "descendant::Widget[@VisualDefinition='TopPanel']/Children/Widget[1]/Children/ListPanel[1]/Children/ButtonWidget[1]", "ClanScreen")]
    internal class MembersTabAttribute : PrefabExtensionSetAttributePatch
    {
        public override List<Attribute> Attributes => new()
        {
            new Attribute("WidthSizePolicy", "StretchToParent")
        };
    }

    [PrefabExtension("ClanScreen",
       "descendant::Widget[@VisualDefinition='TopPanel']/Children/Widget[1]/Children/ListPanel[1]/Children/ButtonWidget[2]", "ClanScreen")]
    internal class PartiesTabAttribute : PrefabExtensionSetAttributePatch
    {
        public override List<Attribute> Attributes => new()
        {
            new Attribute("WidthSizePolicy", "StretchToParent")
        };
    }

    [PrefabExtension("ClanScreen",
       "descendant::Widget[@VisualDefinition='TopPanel']/Children/Widget[1]/Children/ListPanel[1]/Children/ButtonWidget[3]", "ClanScreen")]
    internal class FiefsTabAttribute : PrefabExtensionSetAttributePatch
    {
        public override List<Attribute> Attributes => new()
        {
            new Attribute("WidthSizePolicy", "StretchToParent")
        };
    }

    [PrefabExtension("ClanScreen",
       "descendant::Widget[@VisualDefinition='TopPanel']/Children/Widget[1]/Children/ListPanel[1]/Children/ButtonWidget[4]", "ClanScreen")]
    internal class IncomeTabAttribute : PrefabExtensionSetAttributePatch
    {
        public override List<Attribute> Attributes => new()
        {
            new Attribute("WidthSizePolicy", "StretchToParent")
        };
    }

}