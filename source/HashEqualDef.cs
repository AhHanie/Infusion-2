using Verse;

namespace Infusion
{
    public class HashEqualDef : Def
    {
        public override bool Equals(object obj)
        {
            if (obj is HashEqualDef def)
            {
                return this.defName == def.defName;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.defName.GetHashCode();
        }
    }
}