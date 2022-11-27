using System.Collections.Generic;
using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace BannerKings.UI.Extensions
{
    [PrefabExtension("KingdomManagement", "descendant::Widget[1]/Children", "KingdomManagement")]
    internal class KingdomManagementExtension : PrefabExtensionInsertPatch
    {
        private readonly List<XmlNode> nodes;

        public KingdomManagementExtension()
        {
            var firstChild = new XmlDocument();
            XmlDocument secondChild = new XmlDocument();
            firstChild.LoadXml(
                "<KingdomCourt Id=\"CourtPanel\" DataSource=\"{Court}\" MarginTop=\"188\" MarginBottom=\"75\" />");
            secondChild.LoadXml("<KingdomDemesne Id=\"DemesnePanel\" DataSource=\"{Demesne}\" MarginTop=\"188\" MarginBottom=\"75\" />");
            nodes = new List<XmlNode> { firstChild, secondChild };
        }

        public override InsertType Type => InsertType.Child;
        public override int Index => 4;

        [PrefabExtensionXmlNodes] public IEnumerable<XmlNode> Nodes => nodes;
    }

    [PrefabExtension("KingdomManagement",
        "descendant::KingdomTabControlListPanel[1]/Children",
        "KingdomManagement")]
    internal class KingdomManagementExtension2 : PrefabExtensionInsertPatch
    {
        private readonly List<XmlNode> nodes;

        public KingdomManagementExtension2()
        {
            var firstChild = new XmlDocument();
            XmlDocument secondChild = new XmlDocument();
            firstChild.LoadXml(
                "<ButtonWidget Id=\"CourtButton\" IsSelected=\"@CourtSelected\" IsEnabled=\"@CourtEnabled\"  DoNotPassEventsToChildren=\"true\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"!Header.Tab.Center.Width.Scaled\" SuggestedHeight=\"!Header.Tab.Center.Height.Scaled\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" PositionYOffset=\"2\" MarginLeft=\"5\" Brush=\"Header.Tab.Center\" Command.Click=\"SelectCourt\" UpdateChildrenStates=\"true\"><Children><TextWidget WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Brush=\"Clan.TabControl.Text\" Text=\"@CourtText\" /></Children></ButtonWidget>");
            secondChild.LoadXml("<ButtonWidget Id=\"DemesneButton\" IsSelected=\"@DemesneSelected\" IsVisible=\"@DemesneEnabled\" DoNotPassEventsToChildren=\"true\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"!Header.Tab.Center.Width.Scaled\" SuggestedHeight=\"!Header.Tab.Center.Height.Scaled\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" PositionYOffset=\"2\" MarginLeft=\"5\" Brush=\"Header.Tab.Center\" Command.Click=\"SelectDemesne\" UpdateChildrenStates=\"true\"><Children><TextWidget WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Brush=\"Clan.TabControl.Text\" Text=\"@DemesneText\" /></Children></ButtonWidget>");
            nodes = new List<XmlNode> { firstChild, secondChild };
        }

        public override InsertType Type => InsertType.Child;
        public override int Index => 2;

        [PrefabExtensionXmlNodes] public IEnumerable<XmlNode> Nodes => nodes;
    }


    [PrefabExtension("KingdomManagement", "descendant::KingdomTabControlListPanel", "KingdomManagement")]
    internal class KingdomManagementAttribute : PrefabExtensionSetAttributePatch
    {

        public override List<Attribute> Attributes => new()
        {
            new Attribute("WidthSizePolicy", "StretchToParent"),
            new Attribute("MarginRight", "300"),
            new Attribute("MarginLeft", "250")
        };
    }

    [PrefabExtension("KingdomManagement", "descendant::ButtonWidget[@Id='ClanTabButton']", "KingdomManagement")]
    internal class KingdomManagementClanAttribute : PrefabExtensionSetAttributePatch
    {

        public override List<Attribute> Attributes => new()
        {
            new Attribute("WidthSizePolicy", "StretchToParent")
        };
    }

    [PrefabExtension("KingdomManagement", "descendant::ButtonWidget[@Id='FiefsTabButton']", "KingdomManagement")]
    internal class KingdomManagementFiefsAttribute : PrefabExtensionSetAttributePatch
    {

        public override List<Attribute> Attributes => new()
        {
            new Attribute("WidthSizePolicy", "StretchToParent")
        };
    }

    [PrefabExtension("KingdomManagement", "descendant::ButtonWidget[@Id='PoliciesTabButton']", "KingdomManagement")]
    internal class KingdomManagementPoliciesAttribute : PrefabExtensionSetAttributePatch
    {

        public override List<Attribute> Attributes => new()
        {
            new Attribute("WidthSizePolicy", "StretchToParent")
        };
    }

    [PrefabExtension("KingdomManagement", "descendant::ButtonWidget[@Id='ArmiesTabButton']", "KingdomManagement")]
    internal class KingdomManagementArmiesAttribute : PrefabExtensionSetAttributePatch
    {

        public override List<Attribute> Attributes => new()
        {
            new Attribute("WidthSizePolicy", "StretchToParent")
        };
    }

    [PrefabExtension("KingdomManagement", "descendant::ButtonWidget[@Id='DiplomacyTabButton']", "KingdomManagement")]
    internal class KingdomManagementDiplomacyAttribute : PrefabExtensionSetAttributePatch
    {

        public override List<Attribute> Attributes => new()
        {
            new Attribute("WidthSizePolicy", "StretchToParent")
        };
    }
}