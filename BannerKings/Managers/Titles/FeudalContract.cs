using BannerKings.Managers.Titles.Governments;
using BannerKings.Managers.Titles.Laws;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Titles
{
    public class FeudalContract
    {
        public FeudalContract(Dictionary<FeudalDuties, float> duties, List<FeudalRights> rights, Government government,
            Succession succession, Inheritance inheritance, GenderLaw genderLaw)
        {
            Duties = duties;
            Rights = rights;
            Government = government;
            Succession = succession;
            Inheritance = inheritance;
            GenderLaw = genderLaw;
            DemesneLaws = new List<DemesneLaw>(8);
        }

        [SaveableProperty(1)] public Dictionary<FeudalDuties, float> Duties { get; set; }
        [SaveableProperty(2)] public List<FeudalRights> Rights { get; set; }
        [SaveableProperty(3)] public Government Government { get; private set; }
        [SaveableProperty(4)] public Succession Succession { get; private set; }
        [SaveableProperty(5)] public Inheritance Inheritance { get; private set; }
        [SaveableProperty(6)] public GenderLaw GenderLaw { get; private set; }
        [SaveableProperty(7)] public List<DemesneLaw> DemesneLaws { get; private set; }
        [SaveableProperty(8)] public List<ContractAspect> ContractAspects { get; private set; }

        public void PostInitialize(Kingdom kingdom)
        {
            Government ??= DefaultGovernments.Instance.GetKingdomIdealSuccession(kingdom);
            Succession ??= DefaultSuccessions.Instance.GetKingdomIdealSuccession(kingdom, Government);
            Inheritance ??= DefaultInheritances.Instance.GetKingdomIdealInheritance(kingdom, Government);
            GenderLaw ??= DefaultGenderLaws.Instance.GetKingdomIdealGenderLaw(kingdom, Government);
            Government.PostInitialize();
            Succession.PostInitialize();
            Inheritance.PostInitialize();
            GenderLaw.PostInitialize();
            foreach (var law in DemesneLaws)
            {
                var type = DefaultDemesneLaws.Instance.GetById(law);
                law.Initialize(type.Name, type.Description, type.Effects, type.LawType, type.AuthoritarianWeight,
                    type.EgalitarianWeight, type.OligarchicWeight, type.InfluenceCost, type.Culture, type.IsAdequateForKingdom);
            }

            if (ContractAspects == null) ContractAspects = new List<ContractAspect>();
            else
            {
                foreach (var aspect in ContractAspects)
                {
                    aspect.PostInitialize();
                }
            }

            foreach (var aspect in DefaultContractAspects.Instance.GetIdealKingdomAspects(kingdom, Government))
            {
                if (!ContractAspects.Any(x => x.StringId == aspect.StringId))
                    ContractAspects.Add(aspect);
            }
        }

        public bool HasContractAspect(ContractAspect aspect)  
        {
            if (ContractAspects != null)
            {
                ContractAspects.Contains(aspect);
            }

            return false;
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

        public void ChangeGovernment(Government government)
        {
            Government = government;
        }

        public void ChangeSuccession(Succession succession)
        {
            Succession = succession;
        }

        public void ChangeInheritance(Inheritance inhertiance)
        {
            Inheritance = inhertiance;
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

    public enum LegitimacyType
    {
        Lawful,
        Lawful_Foreigner,
        Unlawful,
        Unlawful_Foreigner
    }
}