using System.Collections.Generic;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Titles
{
    public class FeudalContract
    {
        [SaveableProperty(1)]
        public Dictionary<FeudalDuties, float> Duties { get; private set; }

        [SaveableProperty(2)]
        public List<FeudalRights> Rights { get; private set; }

        [SaveableProperty(3)]
        public GovernmentType Government { get; private set; }

        [SaveableProperty(4)]
        public SuccessionType Succession { get; private set; }

        [SaveableProperty(5)]
        public InheritanceType Inheritance { get; private set; }

        [SaveableProperty(6)]
        public GenderLaw GenderLaw { get; private set; }

        public FeudalContract(Dictionary<FeudalDuties, float> duties, List<FeudalRights> rights, GovernmentType government,
            SuccessionType succession, InheritanceType inheritance, GenderLaw genderLaw)
        {
            Duties = duties;
            Rights = rights;
            Government = government;
            Succession = succession;
            Inheritance = inheritance;
            GenderLaw = genderLaw;
        }

        public void ChangeGovernment(GovernmentType governmentType) => Government = governmentType;
        public void ChangeSuccession(SuccessionType successionType) => Succession = successionType;
        public void ChangeInheritance(InheritanceType inheritanceType) => Inheritance = inheritanceType;
        public void ChangeGenderLaw(GenderLaw genderLaw) => GenderLaw = genderLaw;
    }

    public enum FeudalDuties
    {
        Ransom,
        Taxation,
        Auxilium
    }

    public enum FeudalRights
    {
        Absolute_Land_Rights,
        Conquest_Rights,
        Enfoeffement_Rights,
        Assistance_Rights,
        Army_Compensation_Rights
    }

    public enum CasusBelli
    {
        None,
        Conquest,
        Provocation,
        Lawful_Claim,
        Imperial_Reconquest
    }

    public enum LegitimacyType
    {
        Lawful,
        Lawful_Foreigner,
        Unlawful,
        Unlawful_Foreigner
    }

    public enum SuccessionType
    {
        Hereditary_Monarchy,
        Elective_Monarchy,
        Imperial,
        Republic
    }

    public enum InheritanceType
    {
        Primogeniture,
        Ultimogeniture,
        Seniority
    }

    public enum GenderLaw
    {
        Agnatic,
        Cognatic
    }

    public enum GovernmentType
    {
        Feudal,
        Tribal,
        Imperial,
        Republic
    }
}
