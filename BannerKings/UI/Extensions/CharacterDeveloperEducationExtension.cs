using System.Collections.Generic;
using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace BannerKings.UI.Extensions;

[PrefabExtension("CharacterDeveloper", "descendant::Widget/Children/ListPanel[1]/Children/Widget[1]/Children", "CharacterDeveloper")]
internal class CharacterDeveloperEducationExtension : PrefabExtensionInsertPatch
{
    private readonly List<XmlNode> nodes;

    public CharacterDeveloperEducationExtension()
    {
        var firstChild = new XmlDocument();
        firstChild.LoadXml(
            "<ButtonWidget DataSource=\"{..}\" DoNotPassEventsToChildren=\"true\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"170\" SuggestedHeight=\"65\" HorizontalAlignment=\"Center\" MarginTop=\"80\" Brush=\"Header.Tab.Center\" Command.Click=\"OpenEducation\" UpdateChildrenStates=\"false\"><Children><TextWidget WidthSizePolicy = \"StretchToParent\" HeightSizePolicy = \"StretchToParent\" MarginTop = \"3\" Brush = \"Clan.TabControl.Text\" Text = \"@EducationText\"/></Children></ButtonWidget>");

        nodes = new List<XmlNode> {firstChild};
    }

    public override InsertType Type => InsertType.Child;
    public override int Index => 2;

    [PrefabExtensionXmlNodes] public IEnumerable<XmlNode> Nodes => nodes;
}

[PrefabExtension("CharacterDeveloper", "descendant::Widget/Children", "CharacterDeveloper")]
internal class CharacterDeveloperEducationExtension2 : PrefabExtensionInsertPatch
{
    private readonly List<XmlNode> nodes;

    public CharacterDeveloperEducationExtension2()
    {
        var firstChild = new XmlDocument();
        firstChild.LoadXml("<EducationInspectPopup/>");

        nodes = new List<XmlNode> {firstChild};
    }

    public override InsertType Type => InsertType.Child;
    public override int Index => 8;

    [PrefabExtensionXmlNodes] public IEnumerable<XmlNode> Nodes => nodes;
}