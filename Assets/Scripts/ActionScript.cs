using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionScript
{
    //The actor who is performing the action
    public ActorController Who;
    //A link to the coroutine attached to the action
    public Coroutine Coro;
    //What phase is the action on? Usually set in the animator.
    public int Phase;
    //The actor automatically plays this animation when they begin the action. If "", they do their DefaultAnim.
    public string Anim="";
    //If set to >0, the action ends automatically after this much time passes.
    public float Duration;
    //Just used to track how much time is left in the action’s Duration.
    public float Timer;

    //How fast you can move while you do the action. 1 is default, 0 is immobile
    public float MoveMult = 0;
    //If false, you can’t rotate while doing this action.
    public float RotateMult = 0;
    //If true, you stop all movement when you begin the move
    public bool HaltMomentum = true;
    //How hard this action is to interrupt. 0=idle, 1=default, 3=stun, 5=dead
    public float Priority = 1;

    //Equivalent to Update. Runs every frame as the action is performed.
    public void Run()
    {
        OnRun();
        if (Duration > 0)
        {
            Timer+=Time.deltaTime;
            if(Timer >= Duration)
                End();
        }
    }

    //Runs once per frame, just like Update. Meant to be overridden
    public virtual void OnRun() { }

    //Runs on the physics tic, just like FixedUpdate
    public virtual void HandleMove()
    {
        //Safety check, to make sure they have a RB
        if (Who.RB == null) return;
        //A character's movement is their desired direction * their speed * the action's speed. Knockback goes on top
        Who.RB.velocity = (MoveMult * Who.Speed * Who.DesiredMove.normalized) + Who.Knockback;
    }

    //Runs when the action starts, equivalent to Start
    public virtual void Begin()
    {
        //First we reset the action to make sure it's configured right
        Reset();
        //If HaltMomentum is true, then halt the character's movement
        if(HaltMomentum && Who.RB != null) Who.RB.velocity = Vector2.zero;
        //Start the action's coroutine and record it in a variable so we can end it early if needed
        Coro = Who.StartCoroutine(Script());
        //If Anim is set. . .
        if (Anim != "")
        {
            //Play the animation
            Who.Anim.Play(Anim);
            //And find the length of the animation and set the action's duration to be equal to the animation's duration
            foreach(AnimationClip c in Who.Anim.runtimeAnimatorController.animationClips)
                if (c.name == Anim)
                    Duration = c.length * Who.Anim.speed;
            
        }
        else //Otherwise, play the character's default 'idle' animation
            Who.Anim.Play(Who.DefaultAnim);
        //Phase always starts at 0
        ChangePhase(0);
    }
    
    //Runs if the action ends early, like if you get stunned out of it
    public virtual bool Interrupt(ActionScript newAct)
    {
        //Calls OnEnd, to make sure that your 'end of effect' events happen.
        OnEnd();
        //If you return true, it cancels out the interruption & this action doesn't end
        return false;
    }
    
    //Runs at the natural end of an action. If you want to customize the action's end, override OnEnd instead
    public virtual void End()
    {
        //Safety check--don't end the current action if you aren't it
        if (Who.CurrentAction == this)
        {
            //Clean the character's action
            Who.CurrentAction = null;
            //And give them a new action to do
            Who.DoAction(NextAction());
        }
        //Calls OnEnd, to make sure that your 'end of effect' events happen.
        OnEnd();
    }

    //Called by both Interrupt and End, this is a virtual function that runs when the action ends
    public virtual void OnEnd()
    {
        //Make sure any coroutines running end when the action ends
        if (Coro != null)
        {
            Who.StopCoroutine(Coro);
            Coro = null;
        }
    }

    //A coroutine function. Gets run automatically when the action begins.
    public virtual IEnumerator Script()
    {
        //Empty
        yield return null;
    }
    
    //Resets the function's info
    //Honestly, you shouldn't need this--whenever you start an action you should do a 'new ActionType(actor)'
    //But this is just in case you don't do that
    public virtual void Reset()
    {
        //Reset the timer
        Timer = 0;
    }

    //What action should you take when this action ends?
    //Could be overridden to make a multi-stage attack/etc
    public virtual ActionScript NextAction()
    {
        //By default, use the actor's default 'idle' action
        return Who.DefaultAction();
    }

    //Sets the phase of the action
    //You can override this to let certain code run just as a new phase 'begins'
    public virtual void ChangePhase(int newPhase)
    {
        Phase = newPhase;
    }

    //Called when your hurtbox first hits an actor. Equivalent to OnTriggerEnter2D 
    public virtual void HitBegin(ActorController hit, HurtboxController box) { }
    
    //Called when your hurtbox leaves an actor. Equivalent to OnTriggerExit2D
    public virtual void HitEnd(ActorController hit, HurtboxController box) { }

    //Returns a number between 0 and 1, equal to the percent of the way through your action you are.
    //Only works on actions with durations.
    public float Perc()
    {
        if (Duration <= 0) return 0;
        return Timer / Duration;
    }
}