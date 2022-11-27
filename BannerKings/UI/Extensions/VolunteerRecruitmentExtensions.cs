using System.Collections.Generic;
using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace BannerKings.UI.Extensions
{
    [PrefabExtension("RecruitVolunteerTuple", "descendant::NavigatableListPanel[@Id='NavigatableList']", "RecruitVolunteerTuple")]
    internal class VolunteerRecruitmentExtensions1 : PrefabExtensionInsertPatch
    {
        private readonly List<XmlNode> nodes;

        public VolunteerRecruitmentExtensions1()
        {
            var firstChild = new XmlDocument();
            firstChild.LoadXml(
                "<VolunteersExtension/>");
            nodes = new List<XmlNode> {firstChild};
        }

        public override InsertType Type => InsertType.Replace;

        [PrefabExtensionXmlNodes] public IEnumerable<XmlNode> Nodes => nodes;
    }

    [PrefabExtension("RecruitVolunteerTuple", "descendant::Widget", "RecruitVolunteerTuple")]
    internal class VolunteerRecruitmentExtensions2 : PrefabExtensionSetAttributePatch
    {

        public override List<Attribute> Attributes => new()
        {
            new Attribute("SuggestedHeight", "160")
        };
    }
}