using System.Collections.Generic;
using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace BannerKings.UI.VanillaTabs.Clans
{
    [PrefabExtension("ClanIncome", "descendant::ScrollablePanel[@Id='ClanElementsScrollablePanel']/Children",
        "ClanIncome")]
    internal class EstateButtonExtension : PrefabExtensionInsertPatch
    {
        private readonly List<XmlNode> nodes;

        public EstateButtonExtension()
        {
            var first = new XmlDocument();
            first.LoadXml("<PartyHeaderToggleWidget Id=\"EstatesToggleButton\" DoNotPassEventsToChildren=\"true\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"!Clan.Management.Collapser.Width\" SuggestedHeight=\"!Clan.Management.Collapser.Height\" CollapseIndicator=\"WorkshopsCollapser\\WorkshopsCollapseIndicator\" ListPanel=\"..\\ClanElementsRect\\ClanElementsListPanel\\EstateList\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Top\" Brush=\"Clan.Management.Collapser\" RenderLate=\"true\" WidgetToClose=\"..\\ClanElementsRect\\ClanElementsListPanel\\EstateList\">\r\n                          <Children>\r\n                            <ListPanel Id=\"EstatesCollapser\" WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" >\r\n                              <Children>\r\n                                <BrushWidget Id=\"WorkshopsCollapseIndicator\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"!Party.Toggle.ExpandIndicator.Width\" SuggestedHeight=\"!Party.Toggle.ExpandIndicator.Height\" VerticalAlignment=\"Center\" PositionYOffset=\"-2\" MarginRight=\"5\" Brush=\"Party.Toggle.ExpandIndicator\" />\r\n                                <TextWidget WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" Brush=\"Clan.Management.Collapser.Text\" Text=\"@EstatesText\" />\r\n                              </Children>\r\n                            </ListPanel>\r\n                          </Children>\r\n                        </PartyHeaderToggleWidget>");

            nodes = new List<XmlNode> { first };
        }

        public override InsertType Type => InsertType.Child;
        public override int Index => 4;

        [PrefabExtensionXmlNodes] public IEnumerable<XmlNode> Nodes => nodes;
    }

    [PrefabExtension("ClanIncome", "descendant::Widget[@Id='ClanIncomeWidget']/Children/ListPanel[1]/Children",
        "ClanIncome")]
    internal class EstatePageExtension : PrefabExtensionInsertPatch
    {
        private readonly List<XmlNode> nodes;

        public EstatePageExtension()
        {
            var first = new XmlDocument();
            first.LoadXml("<Widget WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"StretchToParent\" SuggestedWidth=\"650\" MarginLeft=\"200\" IsVisible=\"@IsAnyValidEstateSelected\" >\r\n              <Children>\r\n                <Widget DataSource=\"{CurrentSelectedEstate}\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\">\r\n                  <Children>\r\n                    <ClanIncomeEstate />\r\n                  </Children>\r\n                </Widget>\r\n              </Children>\r\n            </Widget>");

            nodes = new List<XmlNode> { first };
        }

        public override InsertType Type => InsertType.Child;
        public override int Index => 4;

        [PrefabExtensionXmlNodes] public IEnumerable<XmlNode> Nodes => nodes;
    }

    [PrefabExtension("ClanIncome", "descendant::NavigatableListPanel[@Id='ClanElementsListPanel']/Children",
        "ClanIncome")]
    internal class EstateListExtension : PrefabExtensionInsertPatch
    {
        private readonly List<XmlNode> nodes;

        public EstateListExtension()
        {
            var first = new XmlDocument();
            var second = new XmlDocument();
            var third = new XmlDocument();
            first.LoadXml("<NavigationAutoScrollWidget TrackedWidget=\"..\\EstatesHeader\" />");
            second.LoadXml("<ScrollablePanelFixedHeaderWidget Id=\"EstatesHeader\" FixedHeader=\"..\\..\\..\\EstatesToggleButton\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"!Clan.Management.Collapser.Width\" HeaderHeight=\"!Clan.Management.Collapser.Height\"/>");
            third.LoadXml("<NavigatableListPanel Id=\"EstateList\" DataSource=\"{Estates}\" WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" HorizontalAlignment=\"Right\" StackLayout.LayoutMethod=\"VerticalBottomToTop\" UseSelfIndexForMinimum=\"true\">\r\n                                  <ItemTemplate>\r\n                                    <ClanIncomeEstateTuple />\r\n                                  </ItemTemplate>\r\n          </NavigatableListPanel>");
            nodes = new List<XmlNode> { first, second, third };
        }

        public override InsertType Type => InsertType.Child;
        public override int Index => 4;

        [PrefabExtensionXmlNodes] public IEnumerable<XmlNode> Nodes => nodes;
    }
}