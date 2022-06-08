using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using System.Collections.Generic;
using System.Xml;

namespace BannerKings.UI.Extensions
{

    [PrefabExtension("CharacterDeveloper", "descendant::Widget/Children/ListPanel[1]/Children/Widget[1]/Children", "CharacterDeveloper")]
    internal class CharacterDeveloperEducationExtension : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Child;
        public override int Index => 2;

        private List<XmlNode> nodes;

        public CharacterDeveloperEducationExtension()
        {
            XmlDocument firstChild = new XmlDocument();
            firstChild.LoadXml("<ButtonWidget DoNotPassEventsToChildren=\"true\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"170\" SuggestedHeight=\"65\" HorizontalAlignment=\"Center\" MarginTop=\"80\" Brush=\"Header.Tab.Center\" Command.Click=\"OpenEducation\" UpdateChildrenStates=\"false\"><Children><TextWidget WidthSizePolicy = \"StretchToParent\" HeightSizePolicy = \"StretchToParent\" MarginTop = \"3\" Brush = \"Clan.TabControl.Text\" Text = \"@EducationText\"/></Children></ButtonWidget>");

            nodes = new List<XmlNode> { firstChild };
        }

        [PrefabExtensionXmlNodes]
        public IEnumerable<XmlNode> Nodes => nodes;
        
    }

    [PrefabExtension("CharacterDeveloper", "descendant::Widget/Children", "CharacterDeveloper")]
    internal class CharacterDeveloperEducationExtension2 : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Child;
        public override int Index => 8;

        private List<XmlNode> nodes;

        public CharacterDeveloperEducationExtension2()
        {
            XmlDocument firstChild = new XmlDocument();
            firstChild.LoadXml("<EducationInspectPopup/>");

            nodes = new List<XmlNode> { firstChild };
        }

        [PrefabExtensionXmlNodes]
        public IEnumerable<XmlNode> Nodes => nodes;

    }
}
