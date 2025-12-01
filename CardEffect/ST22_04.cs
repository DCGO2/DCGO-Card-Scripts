using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Taomon
namespace DCGO.CardEffects.ST22
{
	public class ST22_04 : CEntity_Effect
	{
		public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
		{
			List<ICardEffect> cardEffects = new List<ICardEffect>();
 
            #region OP/WD Shared
            
            string EffectDiscriptionShared(string tag)
			{
				return $"[{tag}] Until your opponent's turn ends, 1 of their Digimon can't activate [When Digivolving] effects and gets -3000 DP.";
			}
            
            bool CanActivateConditionShared(Hashtable hashtable)
			{
				return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
			}

			IEnumerator ActivateCoroutineShared(Hashtable hashtable, ActivateClass activateClass)
			{
				bool IsOpponentsDigimon(Permanent permanent)
				{
					return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
				}

				if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentsDigimon))
				{
					Permanent selectedPermanent = null;

					SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
					int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsOpponentsDigimon));

					selectPermanentEffect.SetUp(
						selectPlayer: card.Owner,
						canTargetCondition: IsOpponentsDigimon,
						canTargetCondition_ByPreSelecetedList: null,
						canEndSelectCondition: null,
						maxCount: maxCount,
						canNoSelect: false,
						canEndNotMax: false,
						selectPermanentCoroutine: SelectedPermanent,
						afterSelectPermanentCoroutine: null,
						mode: SelectPermanentEffect.Mode.Custom,
						cardEffect: activateClass);

					IEnumerator SelectedPermanent(Permanent target)
					{
						selectedPermanent = target;
						yield return null;
					}

					selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will gain cant activate [When Digivolving] effects & -3K DP.", "The opponent is selecting 1 Digimon that will gain cant activate [When Digivolving] effects & -3k DP.");

					yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

					if (selectedPermanent != null)
					{
						bool CanUseConditionDebuff(Hashtable hashtableDebuff)
						{
							return true;
						}

						bool InvalidateCondition(ICardEffect cardEffect)
						{
							if (selectedPermanent.TopCard != null)
							{
								if (cardEffect != null)
								{
									if (cardEffect.EffectSourceCard != null)
									{
										if (isExistOnField(cardEffect.EffectSourceCard))
										{
											if (cardEffect.EffectSourceCard.PermanentOfThisCard() == selectedPermanent)
											{
												if (cardEffect.IsWhenDigivolving)
												{
													if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
													{
														return true;
													}
												}
											}
										}
									}
								}
							}

							return false;
						}

						DisableEffectClass invalidationClass = new DisableEffectClass();
						invalidationClass.SetUpICardEffect("Ignore [When Digivolving] Effect", CanUseConditionDebuff, card);
						invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
						selectedPermanent.UntilOwnerTurnEndEffects.Add(_ => invalidationClass);

						yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
							targetPermanent: selectedPermanent,
							changeValue: -3000,
							effectDuration: EffectDuration.UntilOpponentTurnEnd,
							activateClass: activateClass));
					}
				}
			}

			#endregion

			#region On Play

			if (timing == EffectTiming.OnEnterFieldAnyone)
			{
				ActivateClass activateClass = new ActivateClass();
				activateClass.SetUpICardEffect("1 digimon gains 'cant activate [When Digivolving] effects' and -6K DP", CanUseCondition, card);
				activateClass.SetUpActivateClass(CanActivateConditionShared, hash => SharedActivateCoroutine(hash, activateClass), -1, false, EffectDiscriptionShared("On Play"));
				cardEffects.Add(activateClass);

				bool CanUseCondition(Hashtable hashtable)
				{
					return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
						&& CardEffectCommons.CanTriggerOnPlay(hashtable, card);
				}
			}

			#endregion

			#region When Digivolving

			if (timing == EffectTiming.OnEnterFieldAnyone)
			{
				ActivateClass activateClass = new ActivateClass();
				activateClass.SetUpICardEffect("1 digimon gains 'cant activate [When Digivolving] effects' and -6K DP", CanUseCondition, card);
				activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, EffectDiscriptionShared("When Digivolving"));
				cardEffects.Add(activateClass);

				bool CanUseCondition(Hashtable hashtable)
				{
					return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
						&& CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
				}
			}

			#endregion
            
            #region When Attacking
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Use 1 Option with [Onmyōjutsu]/[Plug-In] trait", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("ST22_04_WA");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] You may use 1 Option card with the [Onmyōjutsu] or [Plug-In] trait from your hand or under your Tamers without paying the cost.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (!cardSource.CanNotPlayThisOption)
                    {
                        if (cardSource.IsOption)
                        {
                            if (cardSource.EqualsTraits("Onmyōjutsu") || cardSource.EqualsTraits("Plug-In"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }
                
                bool CanSelectTamerCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card))
                    {
                        totalSourceCount = permanent.DigivolutionCards.Count(CanSelectCardCondition);
                        if (totalSourceCount >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersHand(CanSelectCardCondition))
                        {
                            return true;
                        }
                        
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTamerCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }
                
                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(CanSelectCardCondition);
                    bool canSelectTamer = CardEffectCommons.HasMatchConditionPermanent(CanSelectTamerCondition);
                    
                    if (canSelectHand || canSelectTamer)
                    {
                        #region Option Selection

                        if (canSelectHand && canSelectTamer)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From tamer", value : false, spriteIndex: 1),
                        };

                            string selectPlayerMessage = "From which area do you select a card?";
                            string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(canSelectHand);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        #endregion
                        
                        if (fromHand)
                        {
                            #region Play from Hand

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectHandEffect.SetUpCustomMessage("Select 1 option card to use.", "The opponent is selecting 1 option card to use.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Used Card");

                            yield return StartCoroutine(selectHandEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            yield return ContinuousController.instance.StartCoroutine(
                                CardEffectCommons.PlayOptionCards(
                                cardSources: selectedCards,
                                activateClass: activateClass,
                                payCost: false,
                                root: SelectCardEffect.Root.Hand));
                        }
                        #endregion
                            
                        if (!fromHand)
                        {
                            #region Select 1 Tamer
							Permanent selectedPermanent = null;
							
							SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectTamerCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: true,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: SelectPermanentCoroutine,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Custom,
                                        cardEffect: activateClass);

                                    selectPermanentEffect.SetUpCustomMessage("Select a Tamer.", "The opponent is selecting a Tamer.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        selectedPermanent = permanent;
										yield return null;
                                    }
							#endregion

							#region Use from Tamer
							if(selectedPermanent != null)
							{		
								List<CardSource> selectedCards = new list<CardSource>();
	                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
	
	                            selectCardEffect.SetUp(
	                                canTargetCondition: CanSelectCardCondition,
	                                canTargetCondition_ByPreSelecetedList: null,
	                                canEndSelectCondition: null,
	                                canNoSelect: () => true,
	                                selectCardCoroutine: SelectCardCoroutine,
	                                afterSelectCardCoroutine: null,
	                                message: "Select 1 option to use.",
	                                maxCount: 1,
	                                canEndNotMax: false,
	                                isShowOpponent: true,
	                                mode: SelectCardEffect.Mode.Custom,
	                                root: SelectCardEffect.Root.Custm,
	                                customRootCardList: selectedPermanent.DigivolutionCards,
	                                canLookReverseCard: true,
	                                selectPlayer: card.Owner,
	                                cardEffect: activateClass);
	
	                            selectCardEffect.SetUpCustomMessage("Select 1 option card to use.", "The opponent is selecting 1 option card to use.");
	
	                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
	
	                            IEnumerator SelectCardCoroutine(CardSource cardSource)
	                            {
	                                selectedCards.Add(cardSource);
	
	                                yield return null;
	                            }
	
	                            yield return ContinuousController.instance.StartCoroutine(
	                                CardEffectCommons.PlayOptionCards(
	                                cardSources: selectedCards,
	                                activateClass: activateClass,
	                                payCost: false,
	                                root: SelectCardEffect.Root.Hand));
							}
                            #endregion
                        }
                    }
				}
                #endregion
                
                #Inherit EoA
                if (timing == EffectTiming.OnEndAttack)
                {
                    ActivateClass activateClass = new ActivateClass();
                    activateClass.SetUpICardEffect("Trash 1 security to unsuspend 1 Digimon", CanUseCondition, card);
                    activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                    activateClass.SetHashString("ST22_04_EoA");
                    cardEffects.Add(activateClass);

                    string EffectDiscription()
                    {
                        return "[End of Attack] [Once Per Turn] By trashing your top security card, 1 of your Digimon with [Sakuyamon] in its name unsuspends.";
                    }

                    bool CanUseCondition(Hashtable hashtable)
                    {
                        return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                               CardEffectCommons.CanTriggerOnEndAttack(hashtable, card);
                    }

                    bool CanActivateCondition(Hashtable hashtable)
                    {
                        return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                               (card.Owner.SecurityCards.Count >= 1);                                
                    }
                    
                    bool CanSelectPermanentCondition(Permanent permanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                               permanent.TopCard.ContainsCardName("Sakuyamon");
                    }
                    
                    IEnumerator ActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
                    {
                        if (card.Owner.SecurityCards.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                            player: card.Owner,
                            destroySecurityCount: 1,
                            cardEffect: activateClass,
                            fromTop: true).DestroySecurity());
                                                
                        	if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
	                        {
	                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));
	
	                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
	
	                            selectPermanentEffect.SetUp(
	                                selectPlayer: card.Owner,
	                                canTargetCondition: CanSelectPermanentCondition,
	                                canTargetCondition_ByPreSelecetedList: null,
	                                canEndSelectCondition: null,
	                                maxCount: maxCount,
	                                canNoSelect: false,
	                                canEndNotMax: false,
	                                selectPermanentCoroutine: null,
	                                afterSelectPermanentCoroutine: null,
	                                mode: SelectPermanentEffect.Mode.UnTap,
	                                cardEffect: activateClass);
	
	                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());                
	                        }
                        }
                    }
                }
                #endregion
            }
            return cardEffects;
        }
    }
}
