using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

namespace BatteryBuddy
{
    [HasCallAfterGameLoaded]
    public class BatteryBuddy : IPlayerPart
    {
        public static readonly string MOD_ID = "BatteryBuddy";

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("EquipperUnequipped");
            base.Register(Object, Registrar);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "EquipperUnequipped")
            {
                var go = E.GetGameObjectParameter("Object");
                go.UnregisterEvent(this, CellDepletedEvent.ID);
            }
            return base.FireEvent(E);
        }

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) || ID == EquipperEquippedEvent.ID || ID == AfterPlayerBodyChangeEvent.ID;
        }

        public override bool HandleEvent(EquipperEquippedEvent E)
        {
            if (E.Actor.IsPlayer())
            {
                E.Item.RegisterEvent(this, CellDepletedEvent.ID);
            }
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(CellDepletedEvent E)
        {
            if (E.Cell.SlottedIn != null)
            {
                bool usePopup = Options.GetOptionBool("OptionBatteryBuddyPopup");
                if (XRL.Rules.Stat.RandomCosmetic(0, 99) < 50)
                {
                    XDidY(E.Cell.SlottedIn, "emit", Extra: "a sad hiss as its energy cell gives up", UsePopup: usePopup, Color: "W");
                }
                else
                {
                    XDidY(E.Cell.SlottedIn, "sizzle and pop", Extra: "as it runs out of power", UsePopup: usePopup, Color: "W");
                }
            }

            return base.HandleEvent(E);
        }

        public override bool HandleEvent(AfterPlayerBodyChangeEvent E)
        {
            UnregisterEquipment(E.OldBody);
            RegisterEquipment(E.NewBody);
            return base.HandleEvent(E);
        }

        public void RegisterEquipment(GameObject GO)
        {
            GO?.ForeachEquippedObject((GO) => GO.RegisterEvent(this, CellDepletedEvent.ID));
        }

        public void UnregisterEquipment(GameObject GO)
        {
            GO?.ForeachEquippedObject((GO) => GO.UnregisterEvent(this, CellDepletedEvent.ID));
        }

        public void InitializeRegistration()
        {
            // ensure registration for all existing equipment when loading a new save
            RegisterEquipment(The.Player);
        }

        [CallAfterGameLoaded]
        public static void LoadGameCallback()
        {
            // Called whenever loading a save game
            The.Player.RequirePart<BatteryBuddy>().InitializeRegistration();
        }
	}

    [PlayerMutator]
    public class MyPlayerMutator : IPlayerMutator
    {
        public void mutate(GameObject player)
        {
            player.RequirePart<BatteryBuddy>().InitializeRegistration();
        }
    }
}
