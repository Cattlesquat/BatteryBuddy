using System;
using System.Collections.Generic;
using System.Linq;
using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Parts;
using XRL.World.Parts.Skill;

namespace BatteryHelper
{
    [HasCallAfterGameLoadedAttribute]
    public class BatteryHelper : IPlayerPart
    {
        public static readonly string MOD_ID = "BatteryHelper";
        
        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("EquipperUnequipped");
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
        
        public override bool HandleEvent(XRL.World.EquipperEquippedEvent E)
        {
            if (E.Actor.IsPlayer())
            {
                E.Item.RegisterEvent(this, XRL.World.CellDepletedEvent.ID);
            }
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(CellDepletedEvent E)
        {
            if (E.Cell.SlottedIn != null)
            {
                if (XRL.Rules.Stat.RandomCosmetic(1, 2) == 1)
                {
                    XRL.UI.Popup.Show("Your " + E.Cell.SlottedIn.ShortDisplayName + " emits a sad hiss as its energy cell gives up.");
                }
                else
                {
                    XRL.UI.Popup.Show("Your " + E.Cell.SlottedIn.ShortDisplayName + " sizzles and pops as it runs out of power.");
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
            if (GO == null) return;
            foreach (BodyPart part in GO.Body?.GetParts())
            {
                if (part.Equipped == null) continue;
                part.Equipped.RegisterEvent(this, XRL.World.CellDepletedEvent.ID);
            }
        }
        
        public void UnregisterEquipment(GameObject GO)
        {
            if (GO == null) return;
            foreach (BodyPart part in GO.Body?.GetParts())
            {
                if (part.Equipped == null) continue;
                part.Equipped.UnregisterEvent(this, XRL.World.CellDepletedEvent.ID);
            }
        }
        
        public void Initialize()
        {
            // ensure registration for all existing equipment when loading a new save
            RegisterEquipment(XRL.Core.XRLCore.Core?.Game?.Player?.Body);
        }
        
        [CallAfterGameLoadedAttribute]
        public static void LoadGameCallback()
        {
            // Called whenever loading a save game
            var player = XRL.Core.XRLCore.Core?.Game?.Player?.Body;
            player.RequirePart<BatteryHelper>().Initialize();
        }
	}
    
    [PlayerMutator]
    public class MyPlayerMutator : IPlayerMutator
    {
        public void mutate(GameObject player)
        {
            player.RequirePart<BatteryHelper>().Initialize();
        }
    }
}
