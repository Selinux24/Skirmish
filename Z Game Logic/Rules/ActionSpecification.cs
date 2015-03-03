
namespace GameLogic.Rules
{
    using GameLogic.Rules.Enum;

    /// <summary>
    /// Action specification
    /// </summary>
    public class ActionSpecification
    {
        public string Name { get; set; }
        public ActionsEnum Action { get; set; }
        public bool Automatic { get; set; }
        public bool ItemAction { get; set; }
        public bool LeadersOnly { get; set; }
        public bool NeedsCommunicator { get; set; }
        public SoldierClassEnum Classes { get; set; }
        public SelectorEnum Selector { get; set; }
        public SelectorArguments Arguments { get; set; }

        public ActionSpecification()
        {
            this.Automatic = false;
            this.ItemAction = false;
            this.LeadersOnly = false;
            this.NeedsCommunicator = false;
            this.Classes = SoldierClassEnum.Line | SoldierClassEnum.Heavy | SoldierClassEnum.Support | SoldierClassEnum.Medic;
        }

        public override string ToString()
        {
            return string.Format("{0}", this.Name);
        }
    }
}
