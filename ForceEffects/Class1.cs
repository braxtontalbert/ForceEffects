using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ThunderRoad;
using SequenceTracker;

namespace ForceEffects
{
    public class ForceEffectsEntry : SpellCastProjectile
    {
        Step root;
        bool end;
        public override void Load(SpellCaster spellCaster, Level level)
        {
            base.Load(spellCaster, level);
            root = Step.Start(() => Player.currentCreature.handRight.HapticTick());

            root.Then("Fist made", () => PlayerControl.handRight.gripPressed)
                .Do(() => SpawnDagger());

            end = root.AtEnd();
            
        }

        void SpawnDagger() {

            Catalog.GetData<ItemData>("DaggerCommon").SpawnAsync(item => {

                item.transform.position = Player.currentCreature.handRight.transform.position;
                
            });
        }

        public override void UpdateCaster()
        {
            base.UpdateCaster();
            if (end)
            {
                root.Reset();
                return;
            }
            root.Update();
            
        }

    }
}
