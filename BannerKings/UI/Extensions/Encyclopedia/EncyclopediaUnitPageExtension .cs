using System.Collections.Generic;
using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace BannerKings.UI.Extensions.Encyclopedia
{
    [PrefabExtension("EncyclopediaUnitPage",
        "descendant::BrushWidget/Children/Widget[1]/Children/ListPanel[1]/Children/Widget[1]/Children/ListPanel[1]/Children",
        "EncyclopediaUnitPage")]
    internal class EncyclopediaUnitPageExtension : PrefabExtensionInsertPatch
    {
        private readonly List<XmlNode> nodes;

        public EncyclopediaUnitPageExtension()
        {
            var dropdown = new XmlDocument();
            dropdown.LoadXml(
                "<Widget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" MarginLeft=\"50\" MarginRight=\"50\" MarginTop=\"20\"><Children><DropdownWidget DataSource=\"{EraSelection}\" WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" MarginTop=\"2\" ListPanel=\"SelectionList\" Button=\"DropdownButton\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Top\" CurrentSelectedIndex=\"@SelectedIndex\" RichTextWidget=\"DropdownButton\\SelectedTextWidget\">  <Children><ButtonWidget Id=\"DropdownButton\" DoNotPassEventsToChildren=\"true\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"150\" SuggestedHeight=\"35\"  HorizontalAlignment=\"Center\" VerticalAlignment=\"Top\" Brush=\"Kingdom.Wars.BehaviorSelection\">  <Children><RichTextWidget Id=\"SelectedTextWidget\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" PositionYOffset=\"1\" Brush=\"SPOptions.Dropdown.Center.Text\"/>  </Children></ButtonWidget><ListPanel Id=\"SelectionList\" DataSource=\"{ItemList}\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"CoverChildren\" SuggestedWidth=\"120\" VerticalAlignment=\"Top\"    Sprite=\"BlankWhiteSquare_9\" Color=\"#100404FF\" IsVisible=\"false\" StackLayout.LayoutMethod=\"VerticalBottomToTop\" UpdateChildrenStates=\"true\">  <ItemTemplate><ButtonWidget DoNotPassEventsToChildren=\"true\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"Fixed\" SuggestedHeight=\"30\" HorizontalAlignment=\"Center\"   VerticalAlignment=\"Bottom\" ButtonType=\"Radio\" UpdateChildrenStates=\"true\"> <Children><ImageWidget DoNotAcceptEvents=\"true\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" MarginLeft=\"8\" MarginRight=\"8\" Brush=\"Standard.DropdownItem\" /><RichTextWidget DoNotAcceptEvents=\"true\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Brush=\"SPOptions.Dropdown.Item.Text\" Text=\"@StringItem\" /><HintWidget DataSource=\"{Hint}\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"Fixed\" SuggestedHeight=\"30\" Command.HoverBegin=\"ExecuteBeginHint\" Command.HoverEnd=\"ExecuteEndHint\" /></Children></ButtonWidget></ItemTemplate></ListPanel></Children></DropdownWidget></Children></Widget>");
            nodes = new List<XmlNode>
                {dropdown};
        }

        public override InsertType Type => InsertType.Child;
        public override int Index => 2;

        [PrefabExtensionXmlNodes] public IEnumerable<XmlNode> Nodes => nodes;
    }
}