using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAction : ActionScript
{
    //A generic attack action, meant to be a parent class to other attack actions
    //You could make a real simple attack action by just setting an animation and nothing else, though
    
    //How much damage the attack does
    public float Damage = 1;
    //How much knockback it does, 10 is enough to make it so you don't get juggled
    public float Knockback = 10;
    //Track who you've already hit, so you don't hit anyone twice with one attack
    public List<ActorController> AlreadyHit = new List<ActorController>();

    public AttackAction(){ }
    
    public AttackAction(ActorController who, string anim)
    {
        //Make sure I know who the actor actually is
        Who = who;
        //What animation to use
        Anim = anim;
    }

    public override void Reset()
    {
        base.Reset();
        //When you reset, make sure you clear out the list of everyone you've hit
        AlreadyHit.Clear();
    }

    public override void HitBegin(ActorController hit, HurtboxController box)
    {
        //When you hit a target with an attack. . .
        base.HitBegin(hit, box);
        //If you already hit them with this attack, don't double-stack damage
        if (AlreadyHit.Contains(hit)) return;
        //Add them to the list of actors you've hit
        AlreadyHit.Add(hit);
        //Make them take damage
        hit.TakeDamage(Damage);
        //Stun them for a half-second
        hit.DoAction(new StunAction(hit,0.5f),3);
        //And slam them back with some knockback
        hit.TakeKnockback(Who.transform.position,Knockback);
    }
}

public class SwingAction : AttackAction
{
    //A sword swing attack. You take a small step forward while swinging your sword.
    //Phase 0: You swing your sword while lunging forward
    //Phase 1: You recover and bring your sword back to neutral
    
    public SwingAction(ActorController who)
    {
        //Make sure I know who the actor actually is
        Who = who;
        //This attack uses the 'swing' animation
        Anim = "Swing";
    }

    public override void OnRun()
    {
        base.OnRun();
        //At the start of the attack, you take a step forwards
        //But once you hit phase 1, you don't
        if (Phase < 1)
        {
            Who.MoveForwards();
        }
    }

    public override void ChangePhase(int newPhase)
    {
        base.ChangePhase(newPhase);
        if (Phase == 0)
        {
            //You can move during phase 0
            MoveMult = 1;
        }
        else //But not during other phases
            MoveMult = 0;
    }
}


public class LungeAction : AttackAction
{
    //A charge attack. You turn red and fly towards your target, your whole body a hurtbox.
    //Phase 0: You turn red and fly forwards, hurtbox on.
    //Phase 1: You go back to normal and recover for a moment before you can act again.

    //This action uses some variables that only monsters have, so let's make a replacement for the 'Who' variable
    public MonsterController Mon;
    
    public LungeAction(ActorController who, float pow=10)
    {
        //Make sure I know who the actor actually is
        Who = who;
        //The actor is a monster, so cast them into a MonsterController variable so I can access their monster variables
        Mon = (MonsterController)Who;
        //This attack uses the 'lunge' animation
        Anim = "Lunge";
        //You can't move normally during this
        MoveMult = 0;
        //This attack does no knockback--the monster flies through you
        Knockback = 0;
    }

    public override void OnRun()
    {
        base.OnRun();
        //While lunging. . .
        if (Phase == 0)
        {
            //You fly forward faster the longer your range is
            MoveMult = Mon.AttackRange;
            Who.MoveForwards();
        }
        else //But you can't move during recovery
            MoveMult = 0;
    }
}