namespace Infusion
{
    public abstract class InfusionConditon
    {
        public InfusionConditon() { }

        public virtual bool Check(InfusionDef def) { return false; }
    }
}
