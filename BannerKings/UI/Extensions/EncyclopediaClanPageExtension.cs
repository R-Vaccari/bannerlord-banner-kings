using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using System.Collections.Generic;
using System.Xml;

namespace BannerKings.UI.Extensions
{

    [PrefabExtension("EncyclopediaClanPage", "descendant::ListPanel[@Id='RightSideList']/Children/Widget[1]/Children/ListPanel[1]/Children", "EncyclopediaClanPage")]
    internal class EncyclopediaClanPageExtension : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Child;
        public override int Index => 5;

        private List<XmlNode> nodes;

        public EncyclopediaClanPageExtension()
        {
            XmlDocument knights1 = new XmlDocument();
            knights1.LoadXml("<EncyclopediaDivider MarginTop=\"30\" Parameter.Title=\"@KnightsText\" Parameter.ItemList=\"..\\KnightsGrid\"/>");
            XmlDocument knights2 = new XmlDocument();
            knights2.LoadXml("<GridWidget Id=\"KnightsGrid\" DataSource=\"{Knights}\" WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" SuggestedWidth=\"350\" SuggestedHeight=\"350\" DefaultCellWidth=\"100\" DefaultCellHeight=\"100\" HorizontalAlignment=\"Left\" ColumnCount=\"7\" MarginTop=\"10\" MarginLeft=\"15\"><ItemTemplate><EncyclopediaSubPageElement/></ItemTemplate></GridWidget>");

            XmlDocument companions1 = new XmlDocument();
            companions1.LoadXml("<EncyclopediaDivider MarginTop=\"30\" Parameter.Title=\"@CompanionsText\" Parameter.ItemList=\"..\\CompanionssGrid\"/>");
            XmlDocument companions2 = new XmlDocument();
            companions2.LoadXml("<GridWidget Id=\"CompanionssGrid\" DataSource=\"{Companions}\" WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" SuggestedWidth=\"350\" SuggestedHeight=\"350\" DefaultCellWidth=\"100\" DefaultCellHeight=\"100\" HorizontalAlignment=\"Left\" ColumnCount=\"7\" MarginTop=\"10\" MarginLeft=\"15\"><ItemTemplate><EncyclopediaSubPageElement/></ItemTemplate></GridWidget>");

            XmlDocument council1 = new XmlDocument();
            council1.LoadXml("<EncyclopediaDivider MarginTop=\"30\" Parameter.Title=\"@CouncilText\" Parameter.ItemList=\"..\\CouncilGrid\"/>");
            XmlDocument council2 = new XmlDocument();
            council2.LoadXml("<GridWidget Id=\"CouncilGrid\" DataSource=\"{Councillors}\" WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" SuggestedWidth=\"350\" SuggestedHeight=\"350\" DefaultCellWidth=\"100\" DefaultCellHeight=\"100\" HorizontalAlignment=\"Left\" ColumnCount=\"7\" MarginTop=\"10\" MarginLeft=\"15\"><ItemTemplate><EncyclopediaSubPageElement/></ItemTemplate></GridWidget>");

            XmlDocument culture1 = new XmlDocument();
            culture1.LoadXml("<EncyclopediaDivider MarginTop=\"30\" Parameter.Title=\"@CultureText\" Parameter.ItemList=\"..\\CultureGrid\"/>");
            XmlDocument culture2 = new XmlDocument();
            culture2.LoadXml("<ListPanel Id=\"CultureGrid\" DataSource=\"{CultureInfo}\" StackLayout.LayoutMethod=\"VerticalBottomToTop\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\"><Children><RichTextWidget HeightSizePolicy = \"CoverChildren\" WidthSizePolicy = \"StretchToParent\" Brush = \"Encyclopedia.SubPage.Info.Text\" Text = \"@Description\" MarginTop = \"25\" MarginLeft = \"15\" MarginRight = \"25\" ClipContents = \"false\" /><GridWidget Id = \"Info\" DataSource = \"{Information}\" WidthSizePolicy = \"StretchToParent\" HeightSizePolicy = \"CoverChildren\" DefaultCellWidth = \"275\" DefaultCellHeight = \"30\" HorizontalAlignment = \"Left\" ColumnCount = \"2\" MarginTop = \"10\" MarginLeft = \"15\" ><ItemTemplate><ListPanel HeightSizePolicy = \"CoverChildren\" WidthSizePolicy = \"CoverChildren\" MarginLeft = \"15\" MarginTop = \"3\" ><Children><AutoHideRichTextWidget HeightSizePolicy = \"CoverChildren\" WidthSizePolicy = \"CoverChildren\" VerticalAlignment = \"Center\" Brush = \"Encyclopedia.Stat.DefinitionText\" Text = \"@Definition\" MarginRight = \"5\" /><AutoHideRichTextWidget HeightSizePolicy = \"CoverChildren\" WidthSizePolicy = \"CoverChildren\" VerticalAlignment = \"Center\" Brush = \"Encyclopedia.Stat.ValueText\" Text = \"@Value\" PositionYOffset = \"1\" /></Children></ListPanel></ItemTemplate></GridWidget><Widget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"Fixed\" SuggestedHeight=\"7\" MarginLeft=\"250\" MarginRight=\"250\" MarginTop=\"20\" VerticalAlignment=\"Bottom\" PositionYOffset=\"7\" Sprite=\"SPGeneral\\TownManagement\\horizontal_divider\" /><ListPanel Id=\"Leader\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" HorizontalAlignment=\"Center\" StackLayout.LayoutMethod=\"HorizontalRightToLeft\" MarginTop=\"10\"><Children><ButtonWidget IsEnabled=\"@AssumeHeadPossible\" DoNotPassEventsToChildren=\"true\" WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"170\" SuggestedHeight=\"65\" HorizontalAlignment=\"Center\" MarginRight=\"10\" MarginLeft=\"10\" Brush=\"ButtonBrush2\" Command.Click=\"AssumeCultureHead\" UpdateChildrenStates=\"false\"><Children><RichTextWidget WidthSizePolicy = \"CoverChildren\" HeightSizePolicy = \"StretchToParent\" MarginTop = \"3\" MarginRight=\"3\" MarginLeft=\"3\" Brush = \"OverlayPopup.ButtonText\" Text = \"@AssumeCultureHeadText\"/></Children></ButtonWidget><ButtonWidget IsEnabled=\"@ChangeFascinationPossible\" DoNotPassEventsToChildren=\"true\" WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"170\" SuggestedHeight=\"65\" HorizontalAlignment=\"Center\" MarginRight=\"10\" MarginLeft=\"10\" Brush=\"ButtonBrush2\" Command.Click=\"ChangeFascination\" UpdateChildrenStates=\"false\"><Children><RichTextWidget WidthSizePolicy = \"CoverChildren\" HeightSizePolicy = \"StretchToParent\" MarginTop = \"3\" MarginRight=\"3\" MarginLeft=\"3\" Brush = \"OverlayPopup.ButtonText\" Text = \"@ChangeFascinationText\"/></Children></ButtonWidget></Children></ListPanel><GridWidget DataSource=\"{Innovations}\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" MarginRight=\"100\" MarginLeft=\"100\" HorizontalAlignment=\"Center\" DefaultCellWidth=\"350\" DefaultCellHeight=\"150\" ColumnCount=\"2\" MarginTop=\"10\" ><ItemTemplate><Widget HorizontalAlignment = \"Center\" HeightSizePolicy = \"CoverChildren\" WidthSizePolicy = \"StretchToParent\" VerticalAlignment = \"Center\" DoNotPassEventsToChildren = \"true\" MarginTop=\"5\" MarginBottom=\"15\" ><Children><ListPanel HorizontalAlignment = \"Center\" HeightSizePolicy = \"CoverChildren\" WidthSizePolicy = \"StretchToParent\" StackLayout.LayoutMethod = \"VerticalBottomToTop\" ><Children><RichTextWidget WidthSizePolicy = \"StretchToParent\" HeightSizePolicy = \"CoverChildren\" HorizontalAlignment = \"Center\" Brush = \"Clan.Leader.Text\"  Brush.FontSize = \"30\" Text = \"@Definition\" MarginBottom = \"8\" /><RichTextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" HorizontalAlignment=\"Center\" Brush=\"Clan.Leader.Text\"  Brush.FontSize=\"20\" Text=\"@SecondValue\" /><RichTextWidget WidthSizePolicy = \"StretchToParent\" HeightSizePolicy = \"CoverChildren\" HorizontalAlignment = \"Center\" Brush = \"Clan.Leader.Text\"  Brush.FontSize = \"20\" Text = \"@Value\" /></Children></ListPanel><HintWidget DataSource = \"{Hint}\" WidthSizePolicy = \"Fixed\" HeightSizePolicy = \"Fixed\" SuggestedHeight = \"15\" SuggestedWidth = \"100\" VerticalAlignment = \"Center\" HorizontalAlignment = \"Center\" Command.HoverBegin = \"ExecuteBeginHint\" Command.HoverEnd = \"ExecuteEndHint\" /></Children></Widget></ItemTemplate></GridWidget><Widget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"Fixed\" SuggestedHeight=\"7\" MarginLeft=\"250\" MarginRight=\"250\" MarginTop=\"20\" VerticalAlignment=\"Bottom\" PositionYOffset=\"7\" Sprite=\"SPGeneral\\TownManagement\\horizontal_divider\" /><GridWidget Id = \"TraitsGrid\" DataSource = \"{Traits}\" WidthSizePolicy = \"StretchToParent\" HeightSizePolicy = \"CoverChildren\" MarginRight = \"100\" MarginLeft = \"100\" HorizontalAlignment = \"Center\" DefaultCellWidth = \"350\" DefaultCellHeight = \"120\" ColumnCount = \"2\" MarginTop = \"10\" ><ItemTemplate><Widget HorizontalAlignment = \"Center\" HeightSizePolicy = \"StretchToParent\" WidthSizePolicy = \"StretchToParent\" VerticalAlignment = \"Center\" DoNotPassEventsToChildren = \"true\" ><Children> <ListPanel HorizontalAlignment = \"Center\" HeightSizePolicy = \"CoverChildren\" WidthSizePolicy = \"StretchToParent\" SuggestedWidth = \"350\" StackLayout.LayoutMethod = \"VerticalBottomToTop\" ><Children><RichTextWidget WidthSizePolicy = \"StretchToParent\" HeightSizePolicy = \"CoverChildren\" HorizontalAlignment = \"Center\" Brush = \"Clan.Leader.Text\"  Brush.FontSize = \"30\" Text = \"@Definition\" MarginBottom = \"15\" /><RichTextWidget WidthSizePolicy = \"StretchToParent\" HeightSizePolicy = \"CoverChildren\" HorizontalAlignment = \"Center\" Brush = \"Clan.Leader.Text\"  Brush.FontSize = \"20\" Text = \"@Value\" /></Children></ListPanel></Children></Widget></ItemTemplate></GridWidget></Children></ListPanel>");

            nodes = new List<XmlNode> { knights1, knights2, companions1, companions2, council1, council2, culture1, culture2  };
        }

        [PrefabExtensionXmlNodes]
        public IEnumerable<XmlNode> Nodes => nodes;

    }


    [PrefabExtension("EncyclopediaClanPage", "descendant::GridWidget[@Id='Info']/ItemTemplate/ListPanel[1]", "EncyclopediaClanPage")]
    internal class ReplaceClanInfoExtension : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Replace;

        [PrefabExtensionText]
        public string Xml => "<Widget WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" VerticalAlignment=\"Center\" UpdateChildrenStates=\"true\" MarginTop=\"12\" MarginBottom=\"5\" MarginLeft=\"20\" MarginRight=\"20\"><Children><ListPanel HeightSizePolicy = \"CoverChildren\" WidthSizePolicy=\"CoverChildren\" MarginLeft=\"15\" MarginTop=\"3\" ><Children><AutoHideRichTextWidget HeightSizePolicy = \"CoverChildren\" WidthSizePolicy=\"CoverChildren\" VerticalAlignment=\"Center\" Brush=\"Encyclopedia.Stat.DefinitionText\" Text=\"@Definition\" MarginRight=\"5\"/><AutoHideRichTextWidget HeightSizePolicy = \"CoverChildren\" WidthSizePolicy=\"CoverChildren\" VerticalAlignment=\"Center\" Brush=\"Encyclopedia.Stat.ValueText\" Text=\"@Value\" PositionYOffset=\"1\" /></Children></ListPanel><HintWidget DataSource = \"{Hint}\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"Fixed\" SuggestedHeight=\"15\" SuggestedWidth=\"100\" VerticalAlignment=\"Center\" HorizontalAlignment=\"Center\" Command.HoverBegin=\"ExecuteBeginHint\" Command.HoverEnd=\"ExecuteEndHint\" /></Children></Widget>";

    }
}
