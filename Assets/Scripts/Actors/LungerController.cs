using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LungerController : MonsterController
{
    public override ActionScript DefaultAttackAction()
    {
        return new LungeAction(this, 10);
    }
}
