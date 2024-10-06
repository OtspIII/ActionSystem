using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// The script for anything that can perform Actions within the game, and the parent of both PlayerController and MonsterController.
public class ActorController : MonoBehaviour
{
    //---Action Variables---
    /// The action currently being performed
    public ActionScript CurrentAction;
    /// The animation you play when your action doesn't have one set
    public string DefaultAnim = "Idle";
    
    //---Character Stat Variables---
    /// How fast you walk
    public float Speed;
    /// Your max health. If 0, you're immune to damage.
    public float MaxHP = 0;
    /// Your current health. You die if it hits 0.
    public float HP;
    
    //---Backend Variables---
    /// The direction you want to move. Based on WASD for player and AI for NPCs.
    public Vector2 DesiredMove;
    /// Any involuntary movement you're suffering
    public Vector2 Knockback;
    /// Where you spawned into the level. Used for some AI stuff
    public Vector3 StartSpot;
    
    //---Component Variables--- (hidden because they're set in code)
    [HideInInspector] public Rigidbody2D RB;
    [HideInInspector] public Animator Anim;
    
    //-----Event Functions-----
    //You probably already know about Awake/Start/Update/Etc
    //One issue with inheritance is that if you define "Update" on both a parent & child
    //  then the child's version hides the parent's code
    //I basically only use the default event functions (Awake) to call the "OnAwake" versions of themselves
    //Because OnAwake has the virtual tag, I can add code to OnAwake in PlayerController but use 'base.OnAwake()'
    //  to still make sure the OnAwake code from this script gets called

    void Awake() { OnAwake(); }
    public virtual void OnAwake()
    {
        //I set my components automatically
        RB = GetComponent<Rigidbody2D>();
        Anim = GetComponent<Animator>();
        //Record where I spawned in
        StartSpot = transform.position;
    }
    
    public void Start() { OnStart(); }
    public virtual void OnStart()
    {
        //Make sure I start with full health
        HP = MaxHP;
        //Call DoAction with no specified action so I begin my idle/default action
        DoAction();
    }
    
    void FixedUpdate() { OnFixedUpdate(); }
    public virtual void OnFixedUpdate()
    {
        //If you're currently suffering knockback. . .
        if (Knockback != Vector2.zero)
        {
            //Slow it down by 10% every physics tic
            Knockback *= 0.9f;
            //And if you've almost come to a stop, just set it to 0
            if (Knockback.magnitude < 0.1)
                Knockback = Vector2.zero;
        }
        //Your current action dictates exactly how you move
        if(CurrentAction != null) CurrentAction.HandleMove();
        else
        {
            //But if you aren't doing an action right now, you just suffer knockback and can't walk
            RB.velocity = Knockback;
        }
    }

    void Update()
    {
        OnUpdate();
        //If we have a CurrentAction, call its Run (its equivalent to Update).
        CurrentAction?.Run();
    }
    public virtual void OnUpdate(){ }
    
    
    //-----Action Functions-----

    /// <summary>Call this to make an actor perform an action.</summary>
    /// <param name="a">The action to perform. If null, they do their default action.</param>
    /// <param name="priority">The higher this number is, the more high-priority actions it can interrupt.</param>
    public virtual void DoAction(ActionScript a=null, float priority=1)
    {
        //If we're already doing an action, and that action is higher-priority, we can't interrupt it
        if (CurrentAction != null && CurrentAction.Priority >= priority) return;
        //Find the action we'll be doing. If we haven't set one yet, pick your default action
        ActionScript newAct = a ?? DefaultAction();
        //Call 'Interrupt' on the action you're in the middle of doing (if any). There's a chance it'll say 'no'
        if (CurrentAction != null && CurrentAction.Interrupt(newAct)) return;
        //Set the current action to be the new action
        CurrentAction = newAct;
        //Assuming we have a new action. . .
        if (CurrentAction != null)
        {
            //Call Begin on it to get it started
            CurrentAction.Begin();
        }
    }
    
    ///Some actions have phases, where they run different code during different phases.
    ///This function sets what phase is currently going on.
    ///Usually called from within an animation using the little bookmark symbol
    public void SetPhase(int n)
    {
        //Safety check: if we don't have a current action, end the function
        if (CurrentAction == null) return;
        //Tell the current action to set its phase to 'n'
        CurrentAction.ChangePhase(n);
    }
    
    ///When an actor finishes an action, this is what they start doing afterwards
    public virtual ActionScript DefaultAction()
    {
        //Characters idle when they aren't doing anything else
        //Note that Monsters override this function and return PatrolAction instead
        return new IdleAction(this);
    }
    
    ///When an actor attacks, this is what the action they take.
    public virtual ActionScript DefaultAttackAction()
    {
        //By default, actors attack with a sword swing
        //Note that the Lunger monster overrides this to attack with a LungeAction instead
        return new SwingAction(this);
    }
    
    //-----RPG Functions-----
    
    ///Called when you get hit by an attack or otherwise get hurt. Manages health and checks to see if you died
    public virtual void TakeDamage(float amt)
    {
        //If you don't have HP, you're immune to damage
        if (MaxHP <= 0) return;
        //Lower your HP by the damage you took
        HP -= amt;
        //If you're at 0HP or less, you die
        if (HP <= 0)
        {
           Die();
        }
    }
    
    ///Called when you hit 0HP. Just deletes your GameObject for now.
    public virtual void Die()
    {
        Destroy(gameObject);
    }
    
    /// 
    /// <summary>Sends you flying.</summary>
    /// <param name="from">The thing you fly away from.</param>
    /// <param name="amt">How hard you go flying. 10 is average.</param>
    public virtual void TakeKnockback(Vector3 from,float amt)
    {
        //Calculate what direction is away from the thing that's sending you flying
        Vector2 dir = transform.position - from;
        //Set your Knockback to be equal to that
        Knockback = dir.normalized * amt;
    }

    //-----Quality of Life Functions-----
    
    //Set my desired movement to be 'walk forwards'
    public void MoveForwards()
    {
        //We define 'forwards' as 'right' in this game, because it's 2D
        DesiredMove = transform.right;
    }
    
    //Set my desired movement to be walking towards a target
    //If thresh isn't 0, the actor stops walking when they get that close to the target
    public void MoveTowards(ActorController targ,float thresh=0)
    {
        if (targ == null) return;
        MoveTowards(targ.transform.position,thresh);
    }
    public void MoveTowards(GameObject targ,float thresh=0)
    {
        if (targ == null) return;
        MoveTowards(targ.transform.position,thresh);
    }
    public void MoveTowards(Vector3 targ,float thresh=0)
    {
        //If thresh is set to non-0, I stop once I get that close to the target
        if (thresh > 0)
        {
            //Calculate how far I am from them
            float d = Distance(targ);
            //If I'm closer to them than my threshold
            if (d < thresh)
            {
                //If I'm extra-close to them, walk backwards
                if (d < thresh - 1)
                    DesiredMove = transform.position - targ;
                else //Otherwise don't move
                    DesiredMove = Vector2.zero;
                //And end the function early
                return;
            }
        }
        //Set my move to be towards my target
        DesiredMove = targ - transform.position;
    }

    //Returns the distance between you and the target
    //Just a lazy way of calling Vector3.Distance
    public float Distance(ActorController targ)
    {
        if (targ == null) return 999;
        return Distance(targ.transform.position);
    }
    public float Distance(GameObject targ)
    {
        if (targ == null) return 999;
        return Distance(targ.transform.position);
    }
    public float Distance(Vector3 targ)
    {
        return Vector3.Distance(targ, transform.position);
    }

    //Rotate to look at a target. If turnTime is over 0, it takes you turnTime to do a full turn around
    public float LookAt(ActorController targ,float turnTime=0)
    {
        if (targ == null) return 0;
        return LookAt(targ.transform.position,turnTime);
    }
    public float LookAt(GameObject targ,float turnTime=0)
    {
        if (targ == null) return 0;
        return LookAt(targ.transform.position,turnTime);
    }
    public float LookAt(Vector3 targ,float turnTime=0)
    {
        //Calculate what z-rotation you'd need to face the target. This is just trig
        Vector3 diff = targ - transform.position;
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        //If you turn slowly, only rotate part of the way there
        float z = turnTime > 0 ? Mathf.MoveTowardsAngle(transform.rotation.eulerAngles.z, rot_z, (180/turnTime) * Time.deltaTime) : rot_z;
        //Plug the rotation you calculated into your actual transform.rotation
        transform.rotation = Quaternion.Euler(0,0,z);
        //Returns how much rotation you still need to do to look at your target 
        return Mathf.Abs(Mathf.DeltaAngle(z, rot_z));
    }

    //Returns true if you're looking at a target, false if you aren't.
    //thresh defines what 'looking at' means. The bigger the number, the wider your peripheral vision is
    public bool IsFacing(ActorController targ,float thresh=45)
    {
        if (targ == null) return false;
        return IsFacing(targ.transform.position,thresh);
    }
    public bool IsFacing(GameObject targ,float thresh=45)
    {
        if (targ == null) return false;
        return IsFacing(targ.transform.position,thresh);
    }
    public bool IsFacing(Vector3 targ, float thresh=45)
    {
        //Calculate what z-rotation you'd need to be looking at the target
        Vector3 diff = targ - transform.position;
        float tdir = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        //Record your current rotation
        float rot = transform.rotation.eulerAngles.z;
        //Compare the difference between the two. If it's less than thresh, return 'true, I am facing them'
        return Mathf.Abs(Mathf.DeltaAngle(tdir, rot)) < thresh;
    }
}
