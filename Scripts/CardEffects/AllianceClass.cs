using System;

public class AllianceClass : ICardEffect, IAllianceEffect, IFieldEffectCardIdentity
{
    public void SetUpAllianceClass(Func<Permanent, bool> PermanentCondition)
    {
        this.PermanentCondition = PermanentCondition;
    }

    public void SetupAllianceCardSource(CardSource cardSource, string cardHashstring)
    {
        EffectCardSource = cardSource;
        EffectCardHashstring = cardHashstring;
    }

    Func<Permanent, bool> PermanentCondition { get; set; }

    public CardSource EffectCardSource { get; private set; } = null;
    public string EffectCardHashstring { get; private set; } = null;

    public bool HasAlliance(Permanent permanent)
    {
        if (PermanentCondition != null)
        {
            if (permanent != null)
            {
                if (permanent.TopCard != null)
                {
                    if (PermanentCondition(permanent))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}