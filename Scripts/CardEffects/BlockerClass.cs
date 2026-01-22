using System;

public class BlockerClass : ICardEffect, IBlockerEffect, IFieldEffectCardIdentity
{
    public void SetUpBlockerClass(Func<Permanent, bool> PermanentCondition)
    {
        this.PermanentCondition = PermanentCondition;
    }

    public void SetupBlockerCardSource(CardSource cardSource, string cardHashstring)
    {
        EffectCardSource = cardSource;
        EffectCardHashstring = cardHashstring;
    }

    Func<Permanent, bool> PermanentCondition { get; set; }
    public CardSource EffectCardSource { get; private set; } = null;
    public string EffectCardHashstring { get; private set; } = null;

    public bool IsBlocker(Permanent permanent)
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