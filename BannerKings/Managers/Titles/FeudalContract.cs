using BannerKings.Managers.Titles.Laws;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Titles
{
    public class FeudalContract
    {
        public FeudalContract(Dictionary<FeudalDuties, float> duties, List<FeudalRights> rights, GovernmentType government,
            SuccessionType succession, InheritanceType inheritance, GenderLaw genderLaw)
        {
            Duties = duties;
            Rights = rights;
            Government = government;
            Succession = succession;
            Inheritance = inheritance;
            GenderLaw = genderLaw;
            DemesneLaws = new List<DemesneLaw>();
        }

        [SaveableProperty(1)] public Dictionary<FeudalDuties, float> Duties { get; set; }

        [SaveableProperty(2)] public List<FeudalRights> Rights { get; set; }

        [SaveableProperty(3)] public GovernmentType Government { get; private set; }

        [SaveableProperty(4)] public SuccessionType Succession { get; private set; }

        [SaveableProperty(5)] public InheritanceType Inheritance { get; private set; }

        [SaveableProperty(6)] public GenderLaw GenderLaw { get; private set; }

        [SaveableProperty(7)] public List<DemesneLaw> DemesneLaws { get; private set; }

        public void PostInitialize()
        {
            foreach (var law in DemesneLaws)
            {
                var type = DefaultDemesneLaws.Instance.GetById(law);
                law.Initialize(type.Name, type.Description, type.Effects, type.LawType, type.AuthoritarianWeight,
                    type.EgalitarianWeight, type.OligarchicWeight, type.InfluenceCost, type.Culture, type.IsAdequateForKingdom);
            }
        }

        public DemesneLaw GetLawByType(DemesneLawTypes law) => DemesneLaws.FirstOrDefault(x => x.LawType == law);

        public bool IsLawEnacted(DemesneLaw law) => DemesneLaws.Contains(law);

        public void EnactLaw(DemesneLaw law)
        {
            var existingLaw = DemesneLaws.FirstOrDefault(x => x.LawType == law.LawType);
            if (existingLaw != null)
            {
                DemesneLaws.Remove(existingLaw);
            }

            DemesneLaws.Add(law.GetCopy());
        }

        public void SetLaws(List<DemesneLaw> laws)
        {
            DemesneLaws = laws;
        }
        

        public void ChangeGovernment(GovernmentType governmentType)
        {
            Government = governmentType;
        }

        public void ChangeSuccession(SuccessionType successionType)
        {
            Succession = successionType;
        }

        public void ChangeInheritance(InheritanceType inheritanceType)
        {
            Inheritance = inheritanceType;
        }

        public void ChangeGenderLaw(GenderLaw genderLaw)
        {
            GenderLaw = genderLaw;
        }
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
        Republic,
        FeudalElective
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