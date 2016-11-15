using UnityEngine;
using System.Collections.Generic;
using System;

abstract public class Effect
{
    abstract public void activate(Unit unit);
}

public class Slow : Effect
{
    override public void activate(Unit unit)
    {
        float speedMultiplier = 0.75f;
        unit.setCurrentMovement(Mathf.RoundToInt(unit.getCurrentMovement() * speedMultiplier));
    }
}

public class Haste : Effect
{
    public override void activate(Unit unit)
    {
        float speedMultiplier = 1.3f;
        unit.setCurrentMovement( Mathf.RoundToInt(unit.getCurrentMovement() * speedMultiplier) );
    }
}