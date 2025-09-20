import type { EnhancedCard } from '../../types/cards/enhancedCards';
import type { CombatState } from '../../types/combat/combat';
import type { Enemy } from '../../types/combat/enemies';
import { v4 as uuidv4 } from 'uuid';
import { calculateCritDamage } from '../../utils/crit';
import { applyDamageToEnemy, applyDamageModifiers, getWeaponAndElementalDamage } from '../../utils/damage';
import { applyPoison, applyBleed } from '../../utils/statusEffects';
import { applyMultiHitAttack, applyOnHitEffects } from '../../utils/combatUtils';

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

// Helper to add or update a single evasion_buff status effect
function addOrUpdateEvasionBuff(player: any, flat: number, increased: number, duration: number) {
  let found = false;
  const newStatus = (player.status || []).map((effect: any) => {
    if (effect.id === 'evasion_buff') {
      found = true;
      // If duration is -1, keep it permanent; otherwise, set to max of existing/new
      const newDuration = (effect.duration === -1 || duration === -1) ? -1 : Math.max(effect.duration || 0, duration);
      return {
        ...effect,
        value: (effect.value || 0) + flat,
        increased: (effect.increased || 0) + increased,
        duration: newDuration,
        description: `Evasion increased by ${(effect.value || 0) + flat} and ${(effect.increased || 0) + increased}%${newDuration === -1 ? ' (rest of combat)' : ` (${newDuration} turns left)`}`,
      };
    }
    return effect;
  });
  if (!found) {
    newStatus.push({
      id: 'evasion_buff',
      name: 'Evasion Buff',
      description: `Evasion increased by ${flat} and ${increased}%${duration === -1 ? ' (rest of combat)' : ` (${duration} turns left)`}`,
      type: 'buff',
      value: flat,
      increased: increased,
      duration,
      source: 'Skill',
      icon: '🟢',
    });
  }
  return newStatus;
}

export function createRangerStarterDeck() {
  // Basic Attack (6x)
  const arrowShot: Omit<EnhancedCard, 'instanceId'> = {
    id: 'pack_hunter',
    name: 'Pack Hunter',
    type: 'Attack',
    cost: 1,
    tags: ['WeaponAttack','BowAttack', 'Attack', 'Physical', 'Combo','Projectile'],
    description: 'Deal 5 physical damage (+Dex/2) to a single enemy. (Bows: Splash 50% to pack)',
    baseDescription: 'Deal 5 physical damage (+Dex/2) to a single enemy. (Bows: Splash 50% to pack)',
    comboDescription: 'Pack Hunter + Pack Hunter: Deal 100% damage to ALL enemies',
    requirements: {
      dexterity: 25
    },
    comboWith: ['pack_hunter'],
    comboHighlightType: 'specific',
    comboEffect: (state: CombatState, target?: any) => {
      // Pack Hunter + Pack Hunter: Deal 100% damage to ALL enemies
      console.log('[COMBO DEBUG] Pack Hunter combo check:', {
        lastPlayedCard: state.lastPlayedCard,
        lastPlayedCardId: state.lastPlayedCard?.id,
        expectedId: 'pack_hunter',
        matches: state.lastPlayedCard?.id === 'pack_hunter'
      });
      
      const last = state.lastPlayedCard;
      if (last && last.id === 'pack_hunter') {
        console.log('[COMBO DEBUG] Pack Hunter combo triggered! Applying damage to all enemies');
        const dex = getPlayerStat(state, 'dexterity');
        const baseDamage = 5 + Math.floor(dex / 2);
        const weaponDmg = getWeaponAndElementalDamage({ tags: ['WeaponAttack','BowAttack', 'Physical'] }, state.player.stats);
        const totalPhysical = baseDamage + weaponDmg.physical;
        const { damage: finalDamage } = calculateCritDamage(totalPhysical, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
        
        return {
          ...state,
          enemies: state.enemies.map((enemy: Enemy) => {
            const dmg = applyDamageModifiers(finalDamage, state, withInstanceId(arrowShot), enemy);
            return applyDamageToEnemy(enemy, dmg, state.turn, 'physical', 10, undefined, undefined, ['WeaponAttack', 'Physical']);
          })
        };
      }
      console.log('[COMBO DEBUG] Pack Hunter combo not triggered');
      return state;
    },
    effect: (state: CombatState, target: Enemy, cardInstance: EnhancedCard) => {
      if (!target) return state;
      const dex = getPlayerStat(state, 'dexterity');
      const baseDamage = 5 + Math.floor(dex / 2) + 2 * ((cardInstance?.level || 1) - 1); // +2 per level
      const weaponDmg = getWeaponAndElementalDamage({ tags: ['WeaponAttack','BowAttack', 'Physical'] }, state.player.stats);
      const totalPhysical = baseDamage + weaponDmg.physical;
      const attackSpeed = state.player.stats.drawSpeed || 0;
      // Multi-hit logic for main target
      let newState = applyMultiHitAttack(state, target, totalPhysical, attackSpeed, state.turn, (s, t, dmg) =>
        applyOnHitEffects(s, t, dmg, cardInstance)
      );
      // --- Splash logic ---
      const splash = state.player.stats.splash || 0;
      newState = {
        ...newState,
        enemies: newState.enemies.map((e: Enemy) => {
          if (e.id === target.id) return e;
          // Splash to same pack only
          if (splash > 0 && target.packId && e.packId === target.packId) {
            const splashDmg = Math.floor(totalPhysical * 0.5); // 50% splash
            return applyDamageToEnemy(e, splashDmg, newState.turn, 'physical', 10, undefined, undefined, ['WeaponAttack', 'Physical']);
          }
          return e;
        })
      };
      return newState;
    },
    baseEffect: (state: CombatState, target: Enemy, cardInstance: EnhancedCard) => state,
    upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
    embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'pack_hunter'
  };

  // Basic Defense (4x)
  const dodge: Omit<EnhancedCard, 'instanceId'> = {
    id: 'ranger-basic-defense',
    name: 'Dodge',
    type: 'Guard',
    cost: 1,
    tags: ['Guard', 'Dexterity', 'Combo'],
    description: 'Gain 15 Evasion (+Dex/2) for the rest of combat.',
    baseDescription: 'Gain 15 Evasion (+Dex/2) for the rest of combat.',
    comboDescription: 'Pack Hunter + Dodge: 25% increased evasion for 10 seconds',
    requirements: {
      dexterity: 20
    },
    comboWith: ['pack_hunter'],
    comboHighlightType: 'specific',
    comboEffect: (state: CombatState, target?: any) => {
      // Dodge + Pack Hunter: 100% increased evasion for 10 seconds
      console.log('[COMBO DEBUG] Dodge combo check:', {
        lastPlayedCard: state.lastPlayedCard,
        lastPlayedCardId: state.lastPlayedCard?.id,
        expectedId: 'pack_hunter',
        matches: state.lastPlayedCard?.id === 'pack_hunter'
      });
      
      const last = state.lastPlayedCard;
      if (last && last.id === 'pack_hunter') {
        console.log('[COMBO DEBUG] Dodge combo triggered! Applying 25% increased evasion');
        const dex = getPlayerStat(state, 'dexterity');
        const evasionAmount = 15 + Math.floor(dex / 2);
        return {
          ...state,
          player: {
            ...state.player,
            status: addOrUpdateEvasionBuff(state.player, evasionAmount, 25, 10), // 25% increased for 10 turns
          },
        };
      }
      console.log('[COMBO DEBUG] Dodge combo not triggered');
      return state;
    },
    effect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard) => {
      const dex = getPlayerStat(state, 'dexterity');
      const evasionAmount = 15 + Math.floor(dex / 2) + 2 * ((cardInstance?.level || 1) - 1); // +2 per level
      return {
        ...state,
        player: {
          ...state.player,
          status: addOrUpdateEvasionBuff(state.player, evasionAmount, 0, -1),
        },
      };
    },
    baseEffect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard) => state,
    upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
    embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'ranger-basic-defense'
  };

  // Multi-Shot (2x)
  const multiShot: Omit<EnhancedCard, 'instanceId'> = {
    id: 'multi_shot',
    name: 'Multi-Shot',
    type: 'Attack',
    cost: 2,
    tags: ['WeaponAttack','BowAttack', 'Attack', 'Physical', 'Combo','Projectile'],
    description: 'Deal 10 physical damage (+Dex/2) to all enemies.',
    baseDescription: 'Deal 10 physical damage (+Dex/2) to all enemies.',
    comboDescription: 'Focus + Multi-Shot: Apply bleed + Gain Frenzy charge',
    requirements: {
      dexterity: 30
    },
    comboWith: ['focus'],
    comboHighlightType: 'specific',
    comboEffect: (state: CombatState, target?: any) => {
      // Focus + Multi-shot: Apply bleed + Gain Frenzy charge
      console.log('[COMBO DEBUG] Multi-Shot combo check:', {
        lastPlayedCard: state.lastPlayedCard,
        lastPlayedCardId: state.lastPlayedCard?.id,
        expectedId: 'focus',
        matches: state.lastPlayedCard?.id === 'focus'
      });
      
      const last = state.lastPlayedCard;
      if (last && last.id === 'focus') {
        console.log('[COMBO DEBUG] Multi-Shot combo triggered! Applying bleed and gaining frenzy charge');
        const dex = getPlayerStat(state, 'dexterity');
        const baseDamage = 10 + Math.floor(dex / 2);
        const weaponDmg = getWeaponAndElementalDamage({ tags: ['WeaponAttack','BowAttack', 'Physical'] }, state.player.stats);
        const totalPhysical = baseDamage + weaponDmg.physical;
        const { damage: finalDamage } = calculateCritDamage(totalPhysical, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
        
        // Apply bleed to all enemies and gain frenzy charge
        const newEnemies = state.enemies.map((enemy: Enemy) => {
          const dmg = applyDamageModifiers(finalDamage, state, withInstanceId(multiShot), enemy);
          let updatedEnemy = applyDamageToEnemy(enemy, dmg, state.turn, 'physical', 10, undefined, undefined, ['WeaponAttack', 'Physical']);
          // Apply bleed
          updatedEnemy = applyBleed(updatedEnemy, Math.floor(dmg * 0.2), 4);
          return updatedEnemy;
        });
        
        // Gain frenzy charge
        const newCharges = { ...state.player.charges, frenzy: (state.player.charges?.frenzy || 0) + 1 };
        
        return {
          ...state,
          enemies: newEnemies,
          player: {
            ...state.player,
            charges: newCharges
          }
        };
      }
      console.log('[COMBO DEBUG] Multi-Shot combo not triggered');
      return state;
    },
    effect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard) => {
      const dex = getPlayerStat(state, 'dexterity');
      const baseDamage = 10 + Math.floor(dex / 2) + Math.round(1.5 * ((cardInstance?.level || 1) - 1)); // +1.5 per level
      const weaponDmg = getWeaponAndElementalDamage({ tags: ['WeaponAttack','BowAttack', 'Physical'] }, state.player.stats);
      const totalPhysical = baseDamage + weaponDmg.physical;
      const { damage: finalDamage } = calculateCritDamage(totalPhysical, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
      return {
        ...state,
        enemies: state.enemies.map((enemy: Enemy) => {
          const dmg = applyDamageModifiers(finalDamage, state, withInstanceId(multiShot), enemy);
          let updatedEnemy = applyDamageToEnemy(enemy, dmg, state.turn, 'physical', 10, undefined, undefined, ['WeaponAttack', 'Physical']);
          return updatedEnemy;
        })
      };
    },
    baseEffect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard) => state,
    upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
    embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'multi_shot'
  };

  // Quickstep (2x)
  const quickstep: Omit<EnhancedCard, 'instanceId'> = {
    id: 'quickstep',
    name: 'Quickstep',
    type: 'Skill',
    cost: 1,
    tags: ['Skill', 'Dexterity', 'Combo'],
    description: 'Gain 20 Evasion (+Dex/2) for the rest of combat.',
    baseDescription: 'Gain 20 Evasion (+Dex/2) for the rest of combat.',
    comboDescription: 'Focus + Quickstep: 3x Evasion gained',
    requirements: {
      dexterity: 22
    },
    comboWith: ['focus'],
    comboHighlightType: 'specific',
    comboEffect: (state: CombatState, target?: any) => {
      // Focus + Quickstep: 3x Evasion gained
      const last = state.lastPlayedCard;
      if (last && last.id === 'focus') {
        const dex = getPlayerStat(state, 'dexterity');
        const evasionAmount = (20 + Math.floor(dex / 2)) * 3; // 3x evasion
        return {
          ...state,
          player: {
            ...state.player,
            status: addOrUpdateEvasionBuff(state.player, evasionAmount, 0, -1),
          },
        };
      }
      return state;
    },
    effect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard) => {
      const dex = getPlayerStat(state, 'dexterity');
      const evasionAmount = 20 + Math.floor(dex / 2) + 2 * ((cardInstance?.level || 1) - 1); // +2 per level
      return {
        ...state,
        player: {
          ...state.player,
          status: addOrUpdateEvasionBuff(state.player, evasionAmount, 0, -1),
        },
      };
    },
    baseEffect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard) => state,
    upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
    embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'quickstep'
  };

  // Poison Arrow (2x)
  const poisonArrow: Omit<EnhancedCard, 'instanceId'> = {
    id: 'poison_arrow',
    name: 'Poison Arrow',
    type: 'Attack',
    cost: 1,
    tags: ['WeaponAttack','BowAttack', 'Attack', 'Physical', 'Poison','Projectile'],
    description: 'Deal 5 physical damage and apply 2 Poison stacks to all enemies.',
    baseDescription: 'Deal 5 physical damage and apply 2 Poison to all enemies for 20% of hit damage',
    comboDescription: 'Focus + Poison Arrow: 3x Poison duration',
    requirements: {
      dexterity: 28
    },
    comboWith: ['focus'],
    comboHighlightType: 'specific',
    comboEffect: (state: CombatState, target?: any) => {
      // Focus + Poison Arrow: 3x Poison duration
      const last = state.lastPlayedCard;
      if (last && last.id === 'focus') {
        if (!target) return state;
        const baseDamage = 5;
        const weaponDmg = getWeaponAndElementalDamage({ tags: ['WeaponAttack','BowAttack', 'Physical'] }, state.player.stats);
        const totalPhysical = baseDamage + weaponDmg.physical;
        const { damage: finalDamage } = calculateCritDamage(totalPhysical, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
        const dmg = applyDamageModifiers(finalDamage, state, withInstanceId(poisonArrow), target);
        let updatedEnemy = applyDamageToEnemy(target, dmg, state.turn, 'physical', 10, undefined, undefined, ['WeaponAttack', 'Physical']);
        
        // --- Poison scaling logic with 3x duration ---
        let poisonDamage = Math.round(dmg * 0.20);
        let magnitude = 1;
        if (state.player.stats.increasedPoisonMagnitude) {
          magnitude *= 1 + state.player.stats.increasedPoisonMagnitude / 100;
        }
        if (state.player.stats.increasedAilmentEffect) {
          magnitude *= 1 + state.player.stats.increasedAilmentEffect / 100;
        }
        poisonDamage = Math.round(poisonDamage * magnitude);
        
        // Apply 2 Poison stacks to all enemies with 3x duration (9 turns instead of 3)
        const newEnemies = state.enemies.map((e: Enemy) => {
          let poisoned = e;
          for (let i = 0; i < 2; i++) {
            poisoned = applyPoison(poisoned, poisonDamage, 9); // 3x duration
          }
          // Only the target takes damage, all get poison
          return e.id === target.id ? { ...updatedEnemy, status: poisoned.status } : poisoned;
        });
        return { ...state, enemies: newEnemies };
      }
      return state;
    },
    effect: (state: CombatState, target: Enemy, cardInstance: EnhancedCard, logCallbackMap?: Record<string, (msg: string) => void>) => {
      if (!target) return state;
      const baseDamage = 5;
      const weaponDmg = getWeaponAndElementalDamage({ tags: ['WeaponAttack','BowAttack', 'Physical'] }, state.player.stats);
      const totalPhysical = baseDamage + weaponDmg.physical;
      const { damage: finalDamage } = calculateCritDamage(totalPhysical, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
      const dmg = applyDamageModifiers(finalDamage, state, withInstanceId(poisonArrow), target);
      let updatedEnemy = applyDamageToEnemy(target, dmg, state.turn, 'physical', 10, logCallbackMap?.[target.id], undefined, ['WeaponAttack', 'Physical']);
      // --- Poison scaling logic ---
      let poisonDamage = Math.round(dmg * 0.20);
      let magnitude = 1;
      if (state.player.stats.increasedPoisonMagnitude) {
        magnitude *= 1 + state.player.stats.increasedPoisonMagnitude / 100;
      }
      if (state.player.stats.increasedAilmentEffect) {
        magnitude *= 1 + state.player.stats.increasedAilmentEffect / 100;
      }
      poisonDamage = Math.round(poisonDamage * magnitude);
      // Apply 2 Poison stacks to all enemies
      const newEnemies = state.enemies.map((e: Enemy) => {
        let poisoned = e;
        for (let i = 0; i < 2; i++) {
          poisoned = applyPoison(poisoned, poisonDamage, 3);
        }
        // Only the target takes damage, all get poison
        return e.id === target.id ? { ...updatedEnemy, status: poisoned.status } : poisoned;
      });
      return { ...state, enemies: newEnemies };
    },
    baseEffect: (state: CombatState, target: Enemy, cardInstance: EnhancedCard) => state,
    upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
    embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'poison_arrow'
  };

  // Focus (2x)
  const focus: Omit<EnhancedCard, 'instanceId'> = {
    id: 'focus',
    name: 'Focus',
    type: 'Skill',
    cost: 0,
    tags: ['Skill', 'Dexterity'],
    requirements: {
      dexterity: 18
    },
    description: 'Recover 1 mana or overcharge if at full mana.',
    baseDescription: 'Recover 1 mana or overcharge if at full mana.',
    effect: (state: CombatState) => {
      const currentMana = state.player.mana || 0;
      const maxMana = state.player.maxMana || 3;
      
      let newMana: number;
      if (currentMana < maxMana) {
        // Recover 1 mana if not at full
        newMana = Math.min(maxMana, currentMana + 1);
      } else {
        // Overcharge if at full mana
        newMana = maxMana + 1;
      }
      
      let newState = {
        ...state,
        player: {
          ...state.player,
          mana: newMana,
        },
      };
      
      // Add 20% increased evasion for 2 turns
      newState = {
        ...newState,
        player: {
          ...newState.player,
          status: addOrUpdateEvasionBuff(newState.player, 0, 20, 2),
        },
      };
      return newState;
    },
    baseEffect: (state: CombatState) => state,
    upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 1,
    embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'focus'
  };

  return {
    basicAttacks: Array(6).fill(null).map(() => createCardInstance(arrowShot)),
    basicDefenses: Array(4).fill(null).map(() => createCardInstance(dodge)),
    groundSlams: Array(3).fill(null).map(() => createCardInstance(multiShot)),
    intimidatingShouts: Array(2).fill(null).map(() => createCardInstance(quickstep)),
    cleaves: Array(2).fill(null).map(() => createCardInstance(poisonArrow)),
    endures: Array(1).fill(null).map(() => createCardInstance(focus)),
  };
}

