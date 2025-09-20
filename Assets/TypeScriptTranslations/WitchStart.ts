import type { EnhancedCard } from '../../types/cards/enhancedCards';
import type { CombatState } from '../../types/combat/combat';
import type { Enemy } from '../../types/combat/enemies';
import { v4 as uuidv4 } from 'uuid';
import { calculateCritDamage } from '../../utils/crit';
import { applyDamageToEnemy, applyDamageModifiers, getWeaponAndElementalDamage } from '../../utils/damage';
import { applyPoison, applyIgnite, applyBleed } from '../../utils/statusEffects';
import { applyAilments } from '../../utils/ailments';
import { EQUIPMENT_BASES } from '../../types/loot/equipment';
import { createGearItem } from '../../utils/itemGeneration';
import type { GearItem } from '../../types/loot/loot';
import type { CharacterClass } from '../../types/combat/character';

// Helper for card creation
function createCardInstance(baseCard: Omit<EnhancedCard, 'instanceId'>): EnhancedCard {
  return {
    ...baseCard,
    groupKey: baseCard.id,
    instanceId: uuidv4(),
  };
}

// Helper to get a dummy card with instanceId for calculations
function withInstanceId(card: Omit<EnhancedCard, 'instanceId'>): EnhancedCard {
  return { ...card, instanceId: 'dummy' };
}

// Helper for stat access
function getPlayerStat(state: CombatState, stat: 'strength' | 'dexterity' | 'intelligence') {
  return state.player.stats?.[stat] || 0;
}

export function createWitchStarterDeck() {
  // Basic Attack - Arcane Bolt (6x)
  const arcaneBolt: Omit<EnhancedCard, 'instanceId'> = {
    id: 'arcane_bolt',
    name: 'Arcane Bolt',
    type: 'Attack',
    cost: 1,
    tags: ['Spell', 'Lightning', 'Combo', 'Projectile'],
    description: 'Deal 6 Lightning damage (+Int/2). Gain 2 Guard.',
    baseDescription: 'Deal 6 Lightning damage (+Int/2). Gain 2 Guard.',
    comboDescription: 'Arcane Bolt + Arcane Bolt: Deal 150% damage and gain 5 Guard',
    requirements: {
      intelligence: 25
    },
    comboWith: ['arcane_bolt'],
    comboHighlightType: 'specific',
    comboEffect: (state: CombatState, target?: Enemy) => {
      console.log('[COMBO DEBUG] Arcane Bolt combo check:', {
        lastPlayedCard: state.lastPlayedCard,
        lastPlayedCardId: state.lastPlayedCard?.id,
        expectedId: 'arcane_bolt',
        matches: state.lastPlayedCard?.id === 'arcane_bolt'
      });
      
      const last = state.lastPlayedCard;
      if (last && last.id === 'arcane_bolt' && target) {
        console.log('[COMBO DEBUG] Arcane Bolt combo triggered!');
        const int = getPlayerStat(state, 'intelligence');
        const baseDamage = 6 + Math.floor(int / 2);
        const weaponDmg = getWeaponAndElementalDamage({ tags: ['Spell', 'Lightning'] }, state.player.stats);
        const totalDamage = (baseDamage + weaponDmg.lightning) * 1.5; // 150% damage
        const { damage: finalDamage } = calculateCritDamage(totalDamage, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
        
        const dmg = applyDamageModifiers(finalDamage, state, withInstanceId(arcaneBolt), target);
        const updatedEnemy = applyDamageToEnemy(target, dmg, state.turn, 'lightning', 10, undefined, undefined, ['Spell', 'Lightning']);
        
        return {
          ...state,
          enemies: state.enemies.map(e => e.id === target.id ? updatedEnemy : e),
          player: {
            ...state.player,
            guard: 5, // Only return the bonus amount, not the total
          },
        };
      }
      console.log('[COMBO DEBUG] Arcane Bolt combo not triggered');
      return state;
    },
    effect: (state: CombatState, target: Enemy, cardInstance: EnhancedCard) => {
      if (!target) return state;
      const int = getPlayerStat(state, 'intelligence');
      const baseDamage = 6 + Math.floor(int / 2);
      const weaponDmg = getWeaponAndElementalDamage({ tags: ['Spell', 'Lightning'] }, state.player.stats);
      const totalDamage = baseDamage + weaponDmg.lightning;
      const { damage: finalDamage } = calculateCritDamage(totalDamage, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
      const dmg = applyDamageModifiers(finalDamage, state, withInstanceId(arcaneBolt), target);
      let updatedEnemy = applyDamageToEnemy(target, dmg, state.turn, 'lightning', 10, undefined, undefined, ['Spell', 'Lightning']);
      updatedEnemy = applyAilments(updatedEnemy, dmg, cardInstance);
      
      return {
        ...state,
        enemies: state.enemies.map(e => e.id === target.id ? updatedEnemy : e),
        player: {
          ...state.player,
          guard: (state.player.guard || 0) + 2,
        },
      };
    },
    baseEffect: (state: CombatState, target: Enemy, cardInstance: EnhancedCard) => state,
    upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
    embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'arcane_bolt'
  };

  // Basic Defense - Arcane Barrier (4x)
  const arcaneBarrier: Omit<EnhancedCard, 'instanceId'> = {
    id: 'witch-basic-defense',
    name: 'Arcane Barrier',
    type: 'Guard',
    cost: 1,
    tags: ['Guard', 'Intelligence', 'Combo', 'Arcane'],
    description: 'Gain 8 Guard (+Int/3). Gain 2 temporary Intelligence.',
    baseDescription: 'Gain 8 Guard (+Int/3). Gain 2 temporary Intelligence.',
    comboDescription: 'Arcane Bolt + Arcane Barrier: Gain 50% more Guard',
    requirements: {
      intelligence: 20
    },
    comboWith: ['arcane_bolt'],
    comboHighlightType: 'specific',
    comboEffect: (state: CombatState) => {
      console.log('[COMBO DEBUG] Arcane Barrier combo check:', {
        lastPlayedCard: state.lastPlayedCard,
        lastPlayedCardId: state.lastPlayedCard?.id,
        expectedId: 'arcane_bolt',
        matches: state.lastPlayedCard?.id === 'arcane_bolt'
      });
      
      const last = state.lastPlayedCard;
      if (last && last.id === 'arcane_bolt') {
        const int = getPlayerStat(state, 'intelligence');
        const baseGuard = 8 + Math.floor(int / 3);
        const guardAmount = Math.floor(baseGuard * 1.5); // 50% more
        
        // Add temporary intelligence status effect
        const tempIntBuff = {
          id: 'temp_intelligence_' + Math.random().toString(36).substr(2, 9),
          name: 'Temporary Intelligence',
          description: 'Intelligence increased by 2',
          type: 'buff' as const,
          value: 2,
          duration: -1, // Rest of combat
          source: 'Skill',
          icon: '🧠',
          effect: (state: CombatState) => state, // This will be processed by the stats system
        };
        
        return {
          ...state,
          player: {
            ...state.player,
            guard: guardAmount, // Only return the bonus amount, not the total
            status: [...state.player.status, tempIntBuff],
          },
        };
      }
      console.log('[COMBO DEBUG] Arcane Barrier combo not triggered');
      return state;
    },
    effect: (state: CombatState) => {
      const int = getPlayerStat(state, 'intelligence');
      const baseGuard = 8 + Math.floor(int / 3);
      
      console.log(`[ARCANE BARRIER DEBUG] Applying Guard - Base: 8, Int: ${int}, Int/3: ${Math.floor(int / 3)}, Total Guard: ${baseGuard}`);
      console.log(`[ARCANE BARRIER DEBUG] Current player guard: ${state.player.guard || 0}, New guard will be: ${(state.player.guard || 0) + baseGuard}`);
      
      // Add temporary intelligence status effect
      const tempIntBuff = {
        id: 'temp_intelligence_' + Math.random().toString(36).substr(2, 9),
        name: 'Temporary Intelligence',
        description: 'Intelligence increased by 2',
        type: 'buff' as const,
        value: 2,
        duration: -1, // Rest of combat
        source: 'Skill',
        icon: '🧠',
        effect: (state: CombatState) => state, // This will be processed by the stats system
      };
      
      const newPlayer = {
        ...state.player,
        guard: (state.player.guard || 0) + baseGuard,
        status: [...state.player.status, tempIntBuff],
      };
      
      console.log(`[ARCANE BARRIER DEBUG] Final player guard after effect: ${newPlayer.guard}`);
      
      return {
        ...state,
        player: newPlayer,
      };
    },
    baseEffect: (state: CombatState, target: Enemy, cardInstance: EnhancedCard) => state,
    upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
    embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'witch-basic-defense'
  };

  // Fireball (2x)
  const fireball: Omit<EnhancedCard, 'instanceId'> = {
    id: 'fireball',
    name: 'Fireball',
    type: 'Attack',
    cost: 2,
    tags: ['Spell', 'Fire', 'Combo', 'Projectile'],
    description: 'Deal 12 fire damage (+Int/3) to all enemies. Apply 2 Ignite.',
    baseDescription: 'Deal 12 fire damage (+Int/3) to all enemies. Apply 2 Ignite.',
    comboDescription: 'Arcane Bolt + Fireball: Apply 3x Ignite duration',
    requirements: {
      intelligence: 30
    },
    comboWith: ['arcane_bolt'],
    comboHighlightType: 'specific',
    comboEffect: (state: CombatState) => {
      console.log('[COMBO DEBUG] Fireball combo check:', {
        lastPlayedCard: state.lastPlayedCard,
        lastPlayedCardId: state.lastPlayedCard?.id,
        expectedId: 'arcane_bolt',
        matches: state.lastPlayedCard?.id === 'arcane_bolt'
      });
      
      const last = state.lastPlayedCard;
      if (last && last.id === 'arcane_bolt') {
        console.log('[COMBO DEBUG] Fireball combo triggered!');
        const int = getPlayerStat(state, 'intelligence');
        const baseDamage = 12 + Math.floor(int / 3);
        const weaponDmg = getWeaponAndElementalDamage({ tags: ['Spell', 'Fire'] }, state.player.stats);
        
        // Apply flat spell power and % spell damage multiplier to base damage
        const flatSpellPower = state.player.stats.flatSpellPower || 0;
        const spellBonus = ((state.player.stats.spellPower || 0) / 100 + ((state.player.stats as any).spellPowerIncrease || 0) / 100);
        const spellModifiedDamage = (baseDamage + flatSpellPower) * (1 + spellBonus);
        
        const totalDamage = spellModifiedDamage + weaponDmg.fire;
        const { damage: finalDamage } = calculateCritDamage(totalDamage, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
        
        return {
          ...state,
          enemies: state.enemies.map((enemy: Enemy) => {
            const dmg = applyDamageModifiers(finalDamage, state, withInstanceId(fireball), enemy);
            let updatedEnemy = applyDamageToEnemy(enemy, dmg, state.turn, 'fire', 10, undefined, undefined, ['Spell', 'Fire']);
            // Apply 3x Ignite duration
            updatedEnemy = applyIgnite(updatedEnemy, Math.floor(dmg * 0.3), 6); // 6 seconds instead of 2
            return updatedEnemy;
          }),
        };
      }
      console.log('[COMBO DEBUG] Fireball combo not triggered');
      return state;
    },
    effect: (state: CombatState, target?: Enemy, cardInstance?: EnhancedCard, logCallbackMap?: Record<string, (msg: string) => void>) => {
      const int = getPlayerStat(state, 'intelligence');
      const baseDamage = 12 + Math.floor(int / 3);
      const weaponDmg = getWeaponAndElementalDamage({ tags: ['Spell', 'Fire'] }, state.player.stats);
      
      // Apply flat spell power and % spell damage multiplier to base damage
      const flatSpellPower = state.player.stats.flatSpellPower || 0;
      const spellBonus = ((state.player.stats.spellPower || 0) / 100 + ((state.player.stats as any).spellPowerIncrease || 0) / 100);
      const spellModifiedDamage = (baseDamage + flatSpellPower) * (1 + spellBonus);
      
      console.log(`[FIREBALL DEBUG] Base damage: ${baseDamage}, Spell bonus: ${spellBonus * 100}%, Spell modified: ${spellModifiedDamage}, Weapon fire: ${weaponDmg.fire}`);
      
      const totalDamage = spellModifiedDamage + weaponDmg.fire;
      const { damage: finalDamage } = calculateCritDamage(totalDamage, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
      
      return {
        ...state,
        enemies: state.enemies.map((enemy: Enemy) => {
          const dmg = applyDamageModifiers(finalDamage, state, withInstanceId(fireball), enemy);
          let updatedEnemy = applyDamageToEnemy(enemy, dmg, state.turn, 'fire', 10, logCallbackMap?.[enemy.id], undefined, ['Spell', 'Fire']);
          updatedEnemy = applyAilments(updatedEnemy, dmg, cardInstance);
          // Apply Ignite
          updatedEnemy = applyIgnite(updatedEnemy, Math.floor(dmg * 0.3), 2);
          return updatedEnemy;
        }),
      };
    },
    baseEffect: (state: CombatState, target?: Enemy, cardInstance?: EnhancedCard) => state,
    upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
    embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'fireball'
  };

  // Frost Nova (2x)
  const frostNova: Omit<EnhancedCard, 'instanceId'> = {
    id: 'frost_nova',
    name: 'Frost Nova',
    type: 'Attack',
    cost: 2,
    tags: ['Spell', 'Cold', 'Combo', 'Area'],
    description: 'Deal 8 cold damage (+Int/4) to all enemies. Apply 3 Chill.',
    baseDescription: 'Deal 8 cold damage (+Int/4) to all enemies. Apply 3 Chill.',
    comboDescription: 'Frost Nova + Fireball: Apply 2x Chill duration',
    requirements: {
      intelligence: 28
    },
    comboWith: ['fireball'],
    comboHighlightType: 'specific',
    comboEffect: (state: CombatState) => {
      console.log('[COMBO DEBUG] Frost Nova combo check:', {
        lastPlayedCard: state.lastPlayedCard,
        lastPlayedCardId: state.lastPlayedCard?.id,
        expectedId: 'fireball',
        matches: state.lastPlayedCard?.id === 'fireball'
      });
      
      const last = state.lastPlayedCard;
      if (last && last.id === 'fireball') {
        console.log('[COMBO DEBUG] Frost Nova combo triggered!');
        const int = getPlayerStat(state, 'intelligence');
        const baseDamage = 8 + Math.floor(int / 4);
        const weaponDmg = getWeaponAndElementalDamage({ tags: ['Spell', 'Cold'] }, state.player.stats);
        
        // Apply flat spell power and % spell damage multiplier to base damage
        const flatSpellPower = state.player.stats.flatSpellPower || 0;
        const spellBonus = ((state.player.stats.spellPower || 0) / 100 + ((state.player.stats as any).spellPowerIncrease || 0) / 100);
        const spellModifiedDamage = (baseDamage + flatSpellPower) * (1 + spellBonus);
        
        const totalDamage = spellModifiedDamage + weaponDmg.cold;
        const { damage: finalDamage } = calculateCritDamage(totalDamage, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
        
        return {
          ...state,
          enemies: state.enemies.map((enemy: Enemy) => {
            const dmg = applyDamageModifiers(finalDamage, state, withInstanceId(frostNova), enemy);
            let updatedEnemy = applyDamageToEnemy(enemy, dmg, state.turn, 'cold', 10, undefined, undefined, ['Spell', 'Cold']);
            // Apply 2x Chill duration
            updatedEnemy = {
              ...updatedEnemy,
              status: [
                ...updatedEnemy.status,
                {
                  id: 'chill_' + Math.random().toString(36).substr(2, 9),
                  name: 'Chilled',
                  description: 'Movement and attack speed reduced by 30%',
                  duration: 6, // 6 seconds instead of 3
                  type: 'debuff',
                  icon: '🧊',
                  appliedOnTurn: 0,
                  effect: (state: CombatState) => state,
                },
              ],
            };
            return updatedEnemy;
          }),
        };
      }
      console.log('[COMBO DEBUG] Frost Nova combo not triggered');
      return state;
    },
    effect: (state: CombatState, target?: Enemy, cardInstance?: EnhancedCard, logCallbackMap?: Record<string, (msg: string) => void>) => {
      const int = getPlayerStat(state, 'intelligence');
      const baseDamage = 8 + Math.floor(int / 4);
      const weaponDmg = getWeaponAndElementalDamage({ tags: ['Spell', 'Cold'] }, state.player.stats);
      
      // Apply flat spell power and % spell damage multiplier to base damage
      const flatSpellPower = state.player.stats.flatSpellPower || 0;
      const spellBonus = ((state.player.stats.spellPower || 0) / 100 + ((state.player.stats as any).spellPowerIncrease || 0) / 100);
      const spellModifiedDamage = (baseDamage + flatSpellPower) * (1 + spellBonus);
      
      console.log(`[FROST NOVA DEBUG] Base damage: ${baseDamage}, Spell bonus: ${spellBonus * 100}%, Spell modified: ${spellModifiedDamage}, Weapon cold: ${weaponDmg.cold}`);
      
      const totalDamage = spellModifiedDamage + weaponDmg.cold;
      const { damage: finalDamage } = calculateCritDamage(totalDamage, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
      
      return {
        ...state,
        enemies: state.enemies.map((enemy: Enemy) => {
          const dmg = applyDamageModifiers(finalDamage, state, withInstanceId(frostNova), enemy);
          let updatedEnemy = applyDamageToEnemy(enemy, dmg, state.turn, 'cold', 10, logCallbackMap?.[enemy.id], undefined, ['Spell', 'Cold']);
          updatedEnemy = applyAilments(updatedEnemy, dmg, cardInstance);
          // Apply Chill
          updatedEnemy = {
            ...updatedEnemy,
            status: [
              ...updatedEnemy.status,
              {
                id: 'chill_' + Math.random().toString(36).substr(2, 9),
                name: 'Chilled',
                description: 'Movement and attack speed reduced by 30%',
                duration: 6, // 6 seconds instead of 3
                type: 'debuff',
                icon: '🧊',
                appliedOnTurn: 0,
                effect: (state: CombatState) => state,
              },
            ],
          };
          return updatedEnemy;
        }),
      };
    },
    baseEffect: (state: CombatState, target?: Enemy, cardInstance?: EnhancedCard) => state,
    upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
    embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'frost_nova'
  };

  // Chaos Bolt (2x)
  const chaosBolt: Omit<EnhancedCard, 'instanceId'> = {
    id: 'chaos_bolt',
    name: 'Chaos Bolt',
    type: 'Attack',
    cost: 2,
    tags: ['Spell', 'Chaos', 'Combo', 'Projectile'],
    description: 'Deal 10 chaos damage (+Int/3) to a single enemy. Apply 3 Poison.',
    baseDescription: 'Deal 10 chaos damage (+Int/3) to a single enemy. Apply 3 Poison.',
    comboDescription: 'Fireball + Chaos Bolt: Apply 2x Poison damage',
    requirements: {
      intelligence: 26
    },
    comboWith: ['fireball'],
    comboHighlightType: 'specific',
    comboEffect: (state: CombatState, target?: Enemy) => {
      console.log('[COMBO DEBUG] Chaos Bolt combo check:', {
        lastPlayedCard: state.lastPlayedCard,
        lastPlayedCardId: state.lastPlayedCard?.id,
        expectedId: 'fireball',
        matches: state.lastPlayedCard?.id === 'fireball'
      });
      
      const last = state.lastPlayedCard;
      if (last && last.id === 'fireball' && target) {
        console.log('[COMBO DEBUG] Chaos Bolt combo triggered!');
        const int = getPlayerStat(state, 'intelligence');
        const baseDamage = 10 + Math.floor(int / 3);
        const weaponDmg = getWeaponAndElementalDamage({ tags: ['Spell', 'Chaos'] }, state.player.stats);
        
        // Apply flat spell power and % spell damage multiplier to base damage
        const flatSpellPower = state.player.stats.flatSpellPower || 0;
        const spellBonus = ((state.player.stats.spellPower || 0) / 100 + ((state.player.stats as any).spellPowerIncrease || 0) / 100);
        const spellModifiedDamage = (baseDamage + flatSpellPower) * (1 + spellBonus);
        
        const totalDamage = spellModifiedDamage + weaponDmg.chaos;
        const { damage: finalDamage } = calculateCritDamage(totalDamage, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
        
        const dmg = applyDamageModifiers(finalDamage, state, withInstanceId(chaosBolt), target);
        let updatedEnemy = applyDamageToEnemy(target, dmg, state.turn, 'chaos', 10, undefined, undefined, ['Spell', 'Chaos']);
        // Apply 2x Poison damage
        updatedEnemy = applyPoison(updatedEnemy, Math.floor(dmg * 0.4), 3); // 40% of damage instead of 20%
        
        return {
          ...state,
          enemies: state.enemies.map(e => e.id === target.id ? updatedEnemy : e),
        };
      }
      console.log('[COMBO DEBUG] Chaos Bolt combo not triggered');
      return state;
    },
    effect: (state: CombatState, target: Enemy, cardInstance?: EnhancedCard, logCallbackMap?: Record<string, (msg: string) => void>) => {
      if (!target) return state;
      const int = getPlayerStat(state, 'intelligence');
      const baseDamage = 10 + Math.floor(int / 3);
      const weaponDmg = getWeaponAndElementalDamage({ tags: ['Spell', 'Chaos'] }, state.player.stats);
      
      // Apply flat spell power and % spell damage multiplier to base damage
      const flatSpellPower = state.player.stats.flatSpellPower || 0;
      const spellBonus = ((state.player.stats.spellPower || 0) / 100 + ((state.player.stats as any).spellPowerIncrease || 0) / 100);
      const spellModifiedDamage = (baseDamage + flatSpellPower) * (1 + spellBonus);
      
      console.log(`[CHAOS BOLT DEBUG] Base damage: ${baseDamage}, Spell bonus: ${spellBonus * 100}%, Spell modified: ${spellModifiedDamage}, Weapon chaos: ${weaponDmg.chaos}`);
      
      const totalDamage = spellModifiedDamage + weaponDmg.chaos;
      const { damage: finalDamage } = calculateCritDamage(totalDamage, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
      const dmg = applyDamageModifiers(finalDamage, state, withInstanceId(chaosBolt), target);
      let updatedEnemy = applyDamageToEnemy(target, dmg, state.turn, 'chaos', 10, logCallbackMap?.[target.id], undefined, ['Spell', 'Chaos']);
      updatedEnemy = applyAilments(updatedEnemy, dmg, cardInstance);
      // Apply Poison
      updatedEnemy = applyPoison(updatedEnemy, Math.floor(dmg * 0.2), 3);
      
      return {
        ...state,
        enemies: state.enemies.map(e => e.id === target.id ? updatedEnemy : e),
      };
    },
    baseEffect: (state: CombatState, target?: Enemy, cardInstance?: EnhancedCard) => state,
    upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
    embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'chaos_bolt'
  };

  // Arcane Focus (2x)
  const arcaneFocus: Omit<EnhancedCard, 'instanceId'> = {
    id: 'arcane_focus',
    name: 'Arcane Focus',
    type: 'Skill',
    cost: 1,
    tags: ['Skill', 'Intelligence', 'Combo', 'Arcane'],
    description: 'Gain 3 temporary Intelligence. Draw 1 card.',
    baseDescription: 'Gain 3 temporary Intelligence. Draw 1 card.',
    comboDescription: 'Arcane Focus + any Spell: Gain 2 Guard',
    requirements: {
      intelligence: 24
    },
    comboWith: ['Spell'],
    comboHighlightType: 'type',
    comboEffect: (state: CombatState) => {
      console.log('[COMBO DEBUG] Arcane Focus combo check:', {
        lastPlayedCard: state.lastPlayedCard,
        lastPlayedCardType: state.lastPlayedCard?.type,
        expectedType: 'Attack',
        matches: state.lastPlayedCard?.type === 'Attack' && state.lastPlayedCard?.tags?.includes('Spell')
      });
      
      const last = state.lastPlayedCard;
      if (last && last.type === 'Attack' && last.tags?.includes('Spell')) {
        console.log('[COMBO DEBUG] Arcane Focus combo triggered!');
        return {
          ...state,
          player: {
            ...state.player,
            guard: state.player.guard + 2, // Add the combo bonus to the existing guard
          },
        };
      }
      console.log('[COMBO DEBUG] Arcane Focus combo not triggered');
      return state;
    },
    effect: (state: CombatState) => {
      // Add temporary intelligence status effect
      const tempIntBuff = {
        id: 'temp_intelligence_' + Math.random().toString(36).substr(2, 9),
        name: 'Temporary Intelligence',
        description: 'Intelligence increased by 3',
        type: 'buff' as const,
        value: 3,
        duration: -1, // Rest of combat
        source: 'Skill',
        icon: '🧠',
        effect: (state: CombatState) => state, // This will be processed by the stats system
      };
      
      return {
        ...state,
        player: {
          ...state.player,
          status: [...state.player.status, tempIntBuff],
        },
      };
    },
    baseEffect: (state: CombatState) => state,
    upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 1, energyRefund: 0,
    embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'arcane_focus'
  };

  return {
    basicAttacks: [
      createCardInstance(arcaneBolt),
      createCardInstance(arcaneBolt),
      createCardInstance(arcaneBolt),
      createCardInstance(arcaneBolt),
      createCardInstance(arcaneBolt),
      createCardInstance(arcaneBolt),
    ],
    basicDefenses: [
      createCardInstance(arcaneBarrier),
      createCardInstance(arcaneBarrier),
      createCardInstance(arcaneBarrier),
      createCardInstance(arcaneBarrier),
    ],
    fireballs: [
      createCardInstance(fireball),
      createCardInstance(fireball),
    ],
    frostNovas: [
      createCardInstance(frostNova),
      createCardInstance(frostNova),
    ],
    chaosBolts: [
      createCardInstance(chaosBolt),
      createCardInstance(chaosBolt),
    ],
    arcaneFocuses: [
      createCardInstance(arcaneFocus),
      createCardInstance(arcaneFocus),
    ],
  };
}

export function getStarterEquipmentForClass(characterClass: CharacterClass): GearItem[] {
  switch (characterClass) {
    case 'Witch':
      return [createGearItem(EQUIPMENT_BASES['oak_wand'], 'Normal', 'Normal', 1)];
    default:
      return [];
  }
} 

