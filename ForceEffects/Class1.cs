using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ThunderRoad;
using SequenceTracker;
using Gesture;
using static ThunderRoad.ItemMagicAreaProjectile;
using static ThunderRoad.TextData;
using static ThunderRoad.BrainModuleStance;

namespace ForceEffects
{
    public class ForceEffectsEntry : SpellCastProjectile
    {
        Step Up;
        Step push;
        Step grab;
        bool end;
        bool grabbed = false;
        Rigidbody grabbedRigidBody;
        Vector3 direction;
        float distance;
        Creature chosenCreature;
        readonly float closestDistance = 5f;
        float currentDistance;
        Vector3 currentDirection;
        RagdollPart targetedPart;
        bool forceChoke = false;
        float forceSpeed = 2f;
        public override void Load(SpellCaster spellCaster, Level level)
        {
            base.Load(spellCaster, level);
            currentDistance = closestDistance;

           Up = Step.Start(() => spellCaster.ragdollHand.HapticTick(1,2));

            Up.Then(Gesture.Gesture.Right.Palm(Direction.Up).Moving(Direction.Up, 4f))
                .Do(() => AddUpwardForce());

            push = Step.Start(() => spellCaster.ragdollHand.HapticTick());

            push.Then(Gesture.Gesture.Right.Palm(Direction.Forward).Moving(Direction.Forward, 3f).Open)
                .Do(() => ForcePush());
            grab = Step.Start(() => spellCaster.ragdollHand.HapticTick(1,2));

            grab.Then(Gesture.Gesture.Right.Gripping)
                .Do(() => ForceGrab()).Then(Gesture.Gesture.Right.Fist).Do(() => ForceChoke());
            
        }

        void ChokeEnemies() {
        

        
        }

        void ForceChoke() {

            forceChoke = true;
            grabbedRigidBody = chosenCreature.ragdoll.GetPartByName("Neck").rb;
            forceSpeed = 1.6f;
            currentDistance = 3f;
        
        
        }
        void AddUpwardForce() {

            /*Creature chosenCreature = Creature.allActive.Where(creature => creature != Player.currentCreature && 
            (creature.transform.position - Player.currentCreature.transform.position).magnitude < 7f).Min();*/
            /*Creature chosenCreature = Creature.allActive.Where(creature3 => !creature3.isKilled && creature3 != Player.currentCreature)
                .OrderBy(creature3 => (creature3.transform.position - Player.currentCreature.transform.position).sqrMagnitude).First();*/
            
            if(Physics.SphereCast(Player.local.head.transform.position + (Player.local.head.transform.forward * 0.2f), 0.3f, Player.local.head.transform.forward, out RaycastHit hit, float.MaxValue, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if(hit.collider.GetComponentInParent<Creature>() is Creature creature && creature != Player.currentCreature)
                {
                    chosenCreature = creature;
                    chosenCreature.ragdoll.SetState(Ragdoll.State.Destabilized);
                    foreach (RagdollPart part in chosenCreature.ragdoll.parts)
                    {
                        part.rb.AddForce(Vector3.up * 15f, ForceMode.Impulse);
                    }
                }
            }/*

            if (chosenCreature != null)
            {
                chosenCreature.ragdoll.SetState(Ragdoll.State.Destabilized);
                foreach (RagdollPart part in chosenCreature.ragdoll.parts)
                {
                    part.rb.AddForce(Vector3.up * 10f, ForceMode.Impulse);
                }
            }*/
        }

        void ForceGrab() {

            /*if (Physics.Raycast(spellCaster.magic.transform.position + (spellCaster.magic.transform.forward * 0.4f), spellCaster.magic.transform.forward, out RaycastHit hit, float.MaxValue, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {*/
            /*chosenCreature = Creature.allActive.Where(creature3 => !creature3.isKilled && creature3 != Player.currentCreature)
            .OrderBy(creature3 => (creature3.transform.position - Player.currentCreature.transform.position).sqrMagnitude).First();*/
            currentDirection = spellCaster.rayDir.forward;
            if (chosenCreature.ragdoll.targetPart.rb is Rigidbody rb)
                {
                    
                    distance = (chosenCreature.ragdoll.targetPart.transform.position - spellCaster.magic.transform.position).magnitude;
                    grabbed = true;
                    grabbedRigidBody = rb;
                }

        }
        void ForcePush() {
            
            
            if (Physics.SphereCast(Player.local.head.transform.position + (Player.local.head.transform.forward * 0.2f), 2f, spellCaster.magic.transform.forward, out RaycastHit hit, float.MaxValue, ((int)LayerName.BodyLocomotion), QueryTriggerInteraction.Ignore))
            {

                Vector3 direction = (hit.point - spellCaster.magic.transform.position).normalized;
                if(hit.collider.GetComponentInParent<Creature>() is Creature creature && creature != Player.currentCreature)
                {
                    creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                    foreach (RagdollPart ragdollPart in creature.ragdoll.parts) {
                        ragdollPart.rb.AddForce(direction * 20f, ForceMode.Impulse);
                    }
                }
            }


        }
        void PlayerAddUpwardForce() {
            foreach (RagdollPart part in Player.currentCreature.ragdoll.parts) {
                part.rb.AddForce(Vector3.up * 1000f, ForceMode.Impulse);
            }
        }
        
        void SpawnDagger() {

            Catalog.GetData<ItemData>("DaggerCommon").SpawnAsync(item =>
            {
                item.transform.position = spellCaster.transform.position;
            });
        
        }

        public override void UpdateCaster()
        {
            base.UpdateCaster();

            if (grabbed) {

                if (!spellCaster.ragdollHand.playerHand.controlHand.gripPressed) {

                    grabbed = false;
                    return;
                }

               
                currentDirection = spellCaster.rayDir.forward;

                grabbedRigidBody.velocity = ((spellCaster.rayDir.position + (currentDirection * currentDistance)) - grabbedRigidBody.transform.position) * forceSpeed;
            }

            if (Up.AtEnd())
            {
                Up.Reset();
            }
            else Up.Update();

            if (push.AtEnd())
            {
                push.Reset();
            }
            else push.Update();

            if(grab.AtEnd() && !grabbed)
            {
                grab.Reset();
                forceSpeed = 2f;
                grabbedRigidBody = null;
            }
            else grab.Update();
        }

    }
}
