using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleAction : ActionScript
{
    //A constructor with no parameters will run for any children of this class when they get built
    //That means that the following code runs for PatrolAction and ChaseAction as well
    public IdleAction()
    {
        //Low priority--you can cancel out of this action at any time
        Priority = 0;
        //You can move normally
        MoveMult = 1;
        //You can look around normally
        RotateMult = 1;
    }
    
    public IdleAction(ActorController who)
    {
        //Make sure I know who the actor actually is
        Who = who;
        //Low priority--you can cancel out of this action at any time
        Priority = 0;
        //You can move normally
        MoveMult = 1;
        //You can look around normally
        RotateMult = 1;
    }

    public override void OnRun()
    {
        base.OnRun();
        if (Who.DesiredMove.magnitude > 0.1f)
        {
            Who.Anim.Play("Walk");
        }
        else
            Who.Anim.Play("Idle");
    }
}

public class PatrolAction : IdleAction
{
    //This action makes the monster walk between random points close to where they started the game.
    //They just walk around at random, picking a new point to walk towards whenever they reach their target
    // or realize they walked into a wall
    //If they see the player while in this state, they begin to chase them
    
    //This action uses some variables that only monsters have, so let's make a replacement for the 'Who' variable
    public MonsterController Mon;
    //Where am I walking towards?
    public Vector3 Target;
    
    public PatrolAction(ActorController who)
    {
        //Make sure I know who the actor actually is
        Who = who;
        //The actor is a monster, so cast them into a MonsterController variable so I can access their monster variables
        Mon = (MonsterController)Who;
    }

    public override void Begin()
    {
        base.Begin();
        //Calculate the first place I'm going to walk towards
        NewTarget();
    }

    public override void OnRun()
    {
        base.OnRun();
        //If the player is within the monster's vision range. . .
        if (Mon.Target != null && Vector2.Distance(Mon.Target.transform.position,
            Who.transform.position) < Mon.VisionRange)
        {
            //Shoot a raycast from the monster to the player, looking for walls in between them
            Vector2 dir = Mon.Target.transform.position - Mon.transform.position;
            RaycastHit2D hit = Physics2D.Raycast(Mon.transform.position,
                dir, Mon.VisionRange, LayerMask.GetMask("Wall"));
            //If you didn't hit a wall, you can probably see the player
            if (hit.collider == null)
            {
                //So start chasing them, go aggro!
                Mon.DoAction(new ChaseAction(Mon));
                //And none of the rest of this matters, because we're now in chase mode
                return;
            }
        }
        //If I reached the patrol point I was walking towards. . .
        if (Vector2.Distance(Target, Who.transform.position) < 0.2f)
        {
            //Pick a new patrol point to walk towards
            NewTarget();
        }
        //Rotate to look at the patrol point I'm walking towards
        float turn = Mon.LookAt(Target,0.5f);
        //If I'm facing my target. . .
        if (turn < 5)
        {
            //Do a raycast one unit forward from my face
            RaycastHit2D hit = Physics2D.Raycast(Mon.transform.position, Mon.transform.right,
                1, LayerMask.GetMask("Wall"));
            //If there's a wall in front of me, I've walked into a wall...
            if (hit.collider != null)
            {
                //So pick a new target to walk towards, otherwise I'll get stuck against the wall
                NewTarget();
                return;
            }
            //Walk towards my target
            Mon.MoveTowards(Target,0);
        }
        
    }

    //When patrolling, pick a random spot close to where you started to be the next place you walk towards
    void NewTarget()
    {
        Target = Who.StartSpot + new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
    }

}

public class ChaseAction : IdleAction
{
    //A simple action for monsters--they just move straight towards the player and attack when they get within range.
    
    //This action uses some variables that only monsters have, so let's make a replacement for the 'Who' variable
    public MonsterController Mon;
    
    public ChaseAction(ActorController who)
    {
        //Make sure I know who the actor actually is
        Who = who;
        //The actor is a monster, so cast them into a MonsterController variable so I can access their monster variables
        Mon = (MonsterController)Who;
    }

    public override void OnRun()
    {
        base.OnRun();
        //Move towards the player, but come to a stop once you enter attack range
        Mon.MoveTowards(Mon.Target,Mon.AttackRange);
        //Rotate to look at the player at a medium speed
        Mon.LookAt(Mon.Target,0.5f);
        
        //If you're within attack range and facing the player, do your default attack at them!
        if(Mon.Distance(Mon.Target) <= Mon.AttackRange && Mon.IsFacing(Mon.Target,5))
            Mon.DoAction(Mon.DefaultAttackAction());
    }
}