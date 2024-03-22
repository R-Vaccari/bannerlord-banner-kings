using System.Collections.Generic;
using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace BannerKings.UI.Extensions.Encyclopedia
{
    [PrefabExtension("EncyclopediaClanPage",
        "descendant::ListPanel[@Id='RightSideList']/Children/Widget[1]/Children/ListPanel[1]/Children",
        "EncyclopediaClanPage")]
    internal class EncyclopediaClanPageExtension : PrefabExtensionInsertPatch
    {
        private readonly List<XmlNode> nodes;

        public EncyclopediaClanPageExtension()
        {
            var vassals1 = new XmlDocument();
            vassals1.LoadXml(
                "<EncyclopediaDivider MarginTop=\"30\" Parameter.Title=\"@VassalsText\" Parameter.ItemList=\"..\\VassalsGrid\"/>");
            var vassals2 = new XmlDocument();
            vassals2.LoadXml(
                "<GridWidget Id=\"VassalsGrid\" DataSource=\"{Vassals}\" WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" SuggestedWidth=\"350\" SuggestedHeight=\"350\" DefaultCellWidth=\"100\" DefaultCellHeight=\"100\" HorizontalAlignment=\"Left\" ColumnCount=\"7\" MarginTop=\"10\" MarginLeft=\"15\"><ItemTemplate><EncyclopediaSubPageElement/></ItemTemplate></GridWidget>");

            var succession1 = new XmlDocument();
            succession1.LoadXml(
                "<EncyclopediaDivider MarginTop=\"30\" Parameter.Title=\"@InheritanceText\" Parameter.ItemList=\"..\\InheritanceGrid\"/>");
            var succession2 = new XmlDocument();
            succession2.LoadXml(
                "<ListPanel Id=\"InheritanceGrid\" WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"Fixed\" HorizontalAlignment=\"Center\" SuggestedHeight=\"150\"\r\n\t\t\t\t\t\t\t\t\t\t\t\t   VerticalAlignment=\"Center\" StackLayout.LayoutMethod=\"HorizontalLeftToRight\" MarginTop=\"30\" DoNotAcceptEvents=\"false\">\r\n\t\t\t\t\t\t\t\t\t\t\t<Children>\r\n\t\t\t\t\t\t\t\t\t\t\t\r\n\t\t\t\t\t\t\t\t\t\t\t\t<ListPanel WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" HorizontalAlignment=\"Center\" SuggestedHeight=\"150\"\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t   VerticalAlignment=\"Bottom\" StackLayout.LayoutMethod=\"VerticalBottomToTop\" MarginBottom=\"29\" DoNotAcceptEvents=\"false\">\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t<Children>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t<RichTextWidget WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" HorizontalAlignment=\"Center\" Brush=\"Clan.Leader.Text\" Brush.FontSize=\"22\" Text=\"@HeirText\" MarginBottom=\"5\" VerticalAlignment=\"Top\"/> <ToggleButtonWidget DataSource=\"{MainHeir}\" DoNotPassEventsToChildren=\"true\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"StretchToParent\" VerticalAlignment=\"Bottom\" SuggestedWidth=\"171\" SuggestedHeight=\"100\" MarginRight=\"2\" MarginLeft=\"2\"\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tHorizontalAlignment=\"Center\" Brush=\"TownManagement.Governor\" UpdateChildrenStates=\"true\"\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tGamepadNavigationIndex=\"0\" IsEnabled=\"true\">\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<Children>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<ImageIdentifierWidget Id=\"ElementImage\" DataSource=\"{ImageIdentifier}\" WidthSizePolicy=\"StretchToParent\"\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t   HeightSizePolicy=\"StretchToParent\" HorizontalAlignment=\"Center\" MarginLeft=\"3\"\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t   MarginRight=\"3\" MarginTop=\"3\" MarginBottom=\"3\" AdditionalArgs=\"@AdditionalArgs\"\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t   ImageId=\"@Id\" ImageTypeCode=\"@ImageTypeCode\" />\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<HintWidget DataSource=\"{Hint}\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\"\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tCommand.HoverBegin=\"ExecuteBeginHint\" Command.HoverEnd=\"ExecuteEndHint\" />\r\n\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</Children>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t</ToggleButtonWidget></Children></ListPanel><GridWidget Id=\"CoreGrid\" DataSource=\"{Heirs}\" WidthSizePolicy=\"CoverChildren\" \r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tHeightSizePolicy=\"StretchToParent\" VerticalAlignment=\"Bottom\" HorizontalAlignment=\"Center\"\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tSuggestedHeight=\"350\" DefaultCellWidth=\"160\" DefaultCellHeight=\"110\" ColumnCount=\"5\">\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t<ItemTemplate>\r\n\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t<ToggleButtonWidget Id=\"GovernorSelectionButton\" Command.Click=\"SetId\" DoNotPassEventsToChildren=\"true\" \r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tWidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"151\" SuggestedHeight=\"100\" MarginRight=\"2\" MarginLeft=\"2\"\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tHorizontalAlignment=\"Center\" Brush=\"TownManagement.Governor\" UpdateChildrenStates=\"true\"\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tGamepadNavigationIndex=\"0\" IsEnabled=\"true\">\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<Children>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<ImageIdentifierWidget Id=\"ElementImage\" DataSource=\"{ImageIdentifier}\" WidthSizePolicy=\"StretchToParent\"\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t   HeightSizePolicy=\"StretchToParent\" HorizontalAlignment=\"Center\" MarginLeft=\"3\"\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t   MarginRight=\"3\" MarginTop=\"3\" MarginBottom=\"3\" AdditionalArgs=\"@AdditionalArgs\" ImageId=\"@Id\" ImageTypeCode=\"@ImageTypeCode\" /> <HintWidget DataSource=\"{Hint}\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\"\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tCommand.HoverBegin=\"ExecuteBeginHint\" Command.HoverEnd=\"ExecuteEndHint\" /></Children></ToggleButtonWidget></ItemTemplate></GridWidget></Children></ListPanel>");

            var knights1 = new XmlDocument();
            knights1.LoadXml(
                "<EncyclopediaDivider MarginTop=\"30\" Parameter.Title=\"@KnightsText\" Parameter.ItemList=\"..\\KnightsGrid\"/>");
            var knights2 = new XmlDocument();
            knights2.LoadXml(
                "<GridWidget Id=\"KnightsGrid\" DataSource=\"{Knights}\" WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" SuggestedWidth=\"350\" SuggestedHeight=\"350\" DefaultCellWidth=\"100\" DefaultCellHeight=\"100\" HorizontalAlignment=\"Left\" ColumnCount=\"7\" MarginTop=\"10\" MarginLeft=\"15\"><ItemTemplate><EncyclopediaSubPageElement/></ItemTemplate></GridWidget>");

            var companions1 = new XmlDocument();
            companions1.LoadXml(
                "<EncyclopediaDivider MarginTop=\"30\" Parameter.Title=\"@CompanionsText\" Parameter.ItemList=\"..\\CompanionssGrid\"/>");
            var companions2 = new XmlDocument();
            companions2.LoadXml(
                "<GridWidget Id=\"CompanionssGrid\" DataSource=\"{Companions}\" WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" SuggestedWidth=\"350\" SuggestedHeight=\"350\" DefaultCellWidth=\"100\" DefaultCellHeight=\"100\" HorizontalAlignment=\"Left\" ColumnCount=\"7\" MarginTop=\"10\" MarginLeft=\"15\"><ItemTemplate><EncyclopediaSubPageElement/></ItemTemplate></GridWidget>");

            var council1 = new XmlDocument();
            council1.LoadXml(
                "<EncyclopediaDivider MarginTop=\"30\" Parameter.Title=\"@CouncilText\" Parameter.ItemList=\"..\\CouncilGrid\"/>");
            var council2 = new XmlDocument();
            council2.LoadXml(
                "<GridWidget Id=\"CouncilGrid\" DataSource=\"{Councillors}\" WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" SuggestedWidth=\"350\" SuggestedHeight=\"350\" DefaultCellWidth=\"100\" DefaultCellHeight=\"100\" HorizontalAlignment=\"Left\" ColumnCount=\"7\" MarginTop=\"10\" MarginLeft=\"15\"><ItemTemplate><EncyclopediaSubPageElement/></ItemTemplate></GridWidget>");

            var culture1 = new XmlDocument();
            culture1.LoadXml(
                "<EncyclopediaDivider MarginTop=\"30\" Parameter.Title=\"@CultureText\" Parameter.ItemList=\"..\\CultureGrid\"/>");
            var culture2 = new XmlDocument();
            culture2.LoadXml(
                "<ListPanel Id=\"CultureGrid\" DataSource=\"{CultureInfo}\" StackLayout.LayoutMethod=\"VerticalBottomToTop\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\">     <Children>      <RichTextWidget HeightSizePolicy = \"CoverChildren\" WidthSizePolicy = \"StretchToParent\" Brush = \"Encyclopedia.SubPage.Info.Text\" Text = \"@Description\" MarginTop = \"25\" MarginLeft = \"15\" MarginRight = \"25\" ClipContents = \"false\" />       <GridWidget Id = \"Info\" DataSource = \"{Information}\" WidthSizePolicy = \"StretchToParent\" HeightSizePolicy = \"CoverChildren\" DefaultCellWidth = \"275\" DefaultCellHeight = \"30\" HorizontalAlignment = \"Left\" ColumnCount = \"2\" MarginTop = \"10\" MarginLeft = \"15\" >        <ItemTemplate>         <ListPanel HeightSizePolicy = \"CoverChildren\" WidthSizePolicy = \"CoverChildren\" MarginLeft = \"15\" MarginTop = \"3\" >          <Children>           <AutoHideRichTextWidget HeightSizePolicy = \"CoverChildren\" WidthSizePolicy = \"CoverChildren\" VerticalAlignment = \"Center\" Brush = \"Encyclopedia.Stat.DefinitionText\" Text = \"@Definition\" MarginRight = \"5\" />           <AutoHideRichTextWidget HeightSizePolicy = \"CoverChildren\" WidthSizePolicy = \"CoverChildren\" VerticalAlignment = \"Center\" Brush = \"Encyclopedia.Stat.ValueText\" Text = \"@Value\" PositionYOffset = \"1\" />          </Children>         </ListPanel>        </ItemTemplate>       </GridWidget>       <Widget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"Fixed\" SuggestedHeight=\"7\" MarginLeft=\"250\" MarginRight=\"250\" MarginTop=\"20\" VerticalAlignment=\"Bottom\" PositionYOffset=\"7\" Sprite=\"SPGeneral\\TownManagement\\horizontal_divider\" />       <Widget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"Fixed\" SuggestedHeight=\"7\" MarginLeft=\"250\" MarginRight=\"250\" MarginTop=\"20\" VerticalAlignment=\"Bottom\" PositionYOffset=\"7\" Sprite=\"SPGeneral\\TownManagement\\horizontal_divider\" />       <GridWidget Id = \"TraitsGrid\" DataSource = \"{Traits}\" WidthSizePolicy = \"StretchToParent\" HeightSizePolicy = \"CoverChildren\" MarginRight = \"100\" MarginLeft = \"100\" HorizontalAlignment = \"Center\" DefaultCellWidth = \"350\" DefaultCellHeight = \"120\" ColumnCount = \"2\" MarginTop = \"10\" >        <ItemTemplate>         <Widget HorizontalAlignment = \"Center\" HeightSizePolicy = \"StretchToParent\" WidthSizePolicy = \"StretchToParent\" VerticalAlignment = \"Center\" DoNotPassEventsToChildren = \"true\" >          <Children>            <ListPanel HorizontalAlignment = \"Center\" HeightSizePolicy = \"CoverChildren\" WidthSizePolicy = \"StretchToParent\" SuggestedWidth = \"350\" StackLayout.LayoutMethod = \"VerticalBottomToTop\" >            <Children>            <RichTextWidget WidthSizePolicy = \"StretchToParent\" HeightSizePolicy = \"CoverChildren\" HorizontalAlignment = \"Center\" Brush = \"Clan.Leader.Text\"  Brush.FontSize = \"30\" Text = \"@Definition\" MarginBottom = \"15\" />             <RichTextWidget WidthSizePolicy = \"StretchToParent\" HeightSizePolicy = \"CoverChildren\" HorizontalAlignment = \"Center\" Brush = \"Clan.Leader.Text\"  Brush.FontSize = \"20\" Text = \"@Value\" />            </Children>             </ListPanel>            </Children>           </Widget>          </ItemTemplate>   </GridWidget>        </Children>       </ListPanel>");

            nodes = new List<XmlNode>
                {vassals1, vassals2, succession1, succession2, knights1, knights2, companions1, companions2, council1, council2, culture1, culture2};
        }

        public override InsertType Type => InsertType.Child;
        public override int Index => 10;

        [PrefabExtensionXmlNodes] public IEnumerable<XmlNode> Nodes => nodes;
    }

    [PrefabExtension("EncyclopediaClanPage", "descendant::GridWidget[@Id='Info']/ItemTemplate/ListPanel[1]",
        "EncyclopediaClanPage")]
    internal class ReplaceClanInfoExtension : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Replace;

        [PrefabExtensionText]
        public string Xml =>
            "<Widget WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" VerticalAlignment=\"Center\" UpdateChildrenStates=\"true\" MarginTop=\"12\" MarginBottom=\"5\" MarginLeft=\"20\" MarginRight=\"20\"><Children><ListPanel HeightSizePolicy = \"CoverChildren\" WidthSizePolicy=\"CoverChildren\" MarginLeft=\"15\" MarginTop=\"3\" ><Children><AutoHideRichTextWidget HeightSizePolicy = \"CoverChildren\" WidthSizePolicy=\"CoverChildren\" VerticalAlignment=\"Center\" Brush=\"Encyclopedia.Stat.DefinitionText\" Text=\"@Definition\" MarginRight=\"5\"/><AutoHideRichTextWidget HeightSizePolicy = \"CoverChildren\" WidthSizePolicy=\"CoverChildren\" VerticalAlignment=\"Center\" Brush=\"Encyclopedia.Stat.ValueText\" Text=\"@Value\" PositionYOffset=\"1\" /></Children></ListPanel><HintWidget DataSource = \"{Hint}\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"Fixed\" SuggestedHeight=\"15\" SuggestedWidth=\"100\" VerticalAlignment=\"Center\" HorizontalAlignment=\"Center\" Command.HoverBegin=\"ExecuteBeginHint\" Command.HoverEnd=\"ExecuteEndHint\" /></Children></Widget>";
    }
}