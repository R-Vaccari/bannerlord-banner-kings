using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.SaveSystem;
using static Populations.PolicyManager;
using static Populations.PopulationManager;

namespace Populations
{
    class SaveDefiner : SaveableTypeDefiner
    {

        public SaveDefiner() : base(81818181)
        {

        }

        protected override void DefineClassTypes()
        {
            base.AddEnumDefinition(typeof(PopType), 1);
            base.AddClassDefinition(typeof(PopulationClass), 2);
            base.AddClassDefinition(typeof(PopulationData), 3);
            base.AddEnumDefinition(typeof(PolicyType), 4);
            base.AddClassDefinition(typeof(PolicyElement), 5);
            base.AddEnumDefinition(typeof(TaxType), 6);
            base.AddEnumDefinition(typeof(MilitiaPolicy), 7);
            base.AddEnumDefinition(typeof(WorkforcePolicy), 8);
            base.AddClassDefinition(typeof(PopulationManager), 9);
            base.AddClassDefinition(typeof(PolicyManager), 10);
        }

        protected override void DefineEnumTypes()
        {
            base.DefineEnumTypes();
        }

        protected override void DefineStructTypes()
        {
            base.DefineStructTypes();
        }
    }
}
