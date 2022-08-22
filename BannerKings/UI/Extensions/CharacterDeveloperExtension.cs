using System.Collections.Generic;
using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace BannerKings.UI.Extensions
{

    [PrefabExtension("CharacterDeveloper", "descendant::Widget/Children", "CharacterDeveloper")]
    internal class CharacterDeveloperExtension : PrefabExtensionInsertPatch
    {
        private readonly List<XmlNode> nodes;

        public CharacterDeveloperExtension()
        {
            var list = new XmlDocument();
            list.LoadXml(
               "<ListPanel WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" StackLayout.LayoutMethod=\"HorizontalRightToLeft\" VerticalAlignment=\"Top\" HorizontalAlignment=\"Right\" MarginRight=\"20\" MarginBottom=\"40\" ><Children><ButtonWidget DoNotPassEventsToChildren =\"true\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"170\" SuggestedHeight=\"65\" HorizontalAlignment=\"Center\" MarginTop=\"80\" Brush=\"Header.Tab.Center\" Command.Click=\"OpenFaith\" UpdateChildrenStates=\"false\"><Children><TextWidget WidthSizePolicy = \"StretchToParent\" HeightSizePolicy = \"StretchToParent\" MarginTop = \"3\" Brush = \"Clan.TabControl.Text\" Text = \"@FaithText\"/></Children></ButtonWidget><ButtonWidget DoNotPassEventsToChildren =\"true\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"170\" SuggestedHeight=\"65\" HorizontalAlignment=\"Center\" MarginTop=\"80\" Brush=\"Header.Tab.Center\" Command.Click=\"OpenEducation\" UpdateChildrenStates=\"false\"><Children><TextWidget WidthSizePolicy = \"StretchToParent\" HeightSizePolicy = \"StretchToParent\" MarginTop = \"3\" Brush = \"Clan.TabControl.Text\" Text = \"@EducationText\"/></Children></ButtonWidget><ButtonWidget DoNotPassEventsToChildren =\"true\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"170\" SuggestedHeight=\"65\" HorizontalAlignment=\"Center\" MarginTop=\"80\" Brush=\"Header.Tab.Center\" Command.Click=\"OpenDecisions\" UpdateChildrenStates=\"false\"><Children><TextWidget WidthSizePolicy = \"StretchToParent\" HeightSizePolicy = \"StretchToParent\" MarginTop = \"3\" Brush = \"Clan.TabControl.Text\" Text = \"@DecisionsText\"/></Children></ButtonWidget></Children></ListPanel>");

            var firstChild = new XmlDocument();
            firstChild.LoadXml("<EducationInspectPopup/>");

            nodes = new List<XmlNode> { list, firstChild };
        }

        public override InsertType Type => InsertType.Child;
        public override int Index => 8;

        [PrefabExtensionXmlNodes] public IEnumerable<XmlNode> Nodes => nodes;
    }

    [PrefabExtension("CharacterDeveloper", "descendant::ListPanel[@Id='Traits']/Children/TextWidget", "CharacterDeveloper")]
    internal class RemoveTraitsTextExtension : PrefabExtensionSetAttributePatch
    {

        public override List<Attribute> Attributes => new()
        {
            new Attribute("IsVisible", "false")
        };
    }
}