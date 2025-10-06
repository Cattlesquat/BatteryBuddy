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
            RegisterAllEquipment(Object, Registrar);
            base.Register(Object, Registrar);
        }

        public void RegisterAllEquipment(GameObject Object, IEventRegistrar Registrar)
        {
            Object.ForeachEquippedObject(GO => Registrar.Register(GO, CellDepletedEvent.ID));
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
            return base.WantEvent(ID, cascade) || ID == EquipperEquippedEvent.ID;
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

        [CallAfterGameLoaded]
        public static void LoadGameCallback()
        {
            // Called whenever loading a save game
            The.Player.RequirePart<BatteryBuddy>();
        }
	}

    [PlayerMutator]
    public class MyPlayerMutator : IPlayerMutator
    {
        public void mutate(GameObject player)
        {
            player.RequirePart<BatteryBuddy>();
        }
    }
}
