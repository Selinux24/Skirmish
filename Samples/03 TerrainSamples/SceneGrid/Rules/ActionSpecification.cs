
namespace TerrainSamples.SceneGrid.Rules
{
    using TerrainSamples.SceneGrid.Rules.Enum;

    /// <summary>
    /// Action specification
    /// </summary>
    public class ActionSpecification
    {
        public string Name { get; set; }
        public Actions Action { get; set; }
        public bool Automatic { get; set; }
        public bool ItemAction { get; set; }
        public bool LeadersOnly { get; set; }
        public bool NeedsCommunicator { get; set; }
        public bool MeleeOnly { get; set; }
        public SoldierClasses Classes { get; set; }
        public Selectors Selector { get; set; }
        public SelectorArguments Arguments { get; set; }

        public ActionSpecification()
        {
            this.Automatic = false;
            this.ItemAction = false;
            this.LeadersOnly = false;
            this.NeedsCommunicator = false;
            this.MeleeOnly = false;
            this.Classes = SoldierClasses.Line | SoldierClasses.Heavy | SoldierClasses.Support | SoldierClasses.Medic;
        }

        public override string ToString()
        {
            return string.Format("{0}", this.Name);
        }
    }
}
