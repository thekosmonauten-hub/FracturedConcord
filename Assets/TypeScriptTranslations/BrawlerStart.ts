import type { EnhancedCard } from '../../types/cards/enhancedCards';
import type { CombatState } from '../../types/combat/combat';
import type { Enemy } from '../../types/combat/enemies';
import type { StatusEffect } from '../../types/combat/combat';
import { v4 as uuidv4 } from 'uuid';
import { calculateCritDamage } from '../../utils/crit';
import { applyDamageToEnemy, applyDamageModifiers, getWeaponAndElementalDamage } from '../../utils/damage';
import { getMultiHitBreakdown, applyMultiHitAttack, applyOnHitEffects } from '../../utils/combatUtils';

// Helper for card creation
export function createCardInstance(baseCard: Omit<EnhancedCard, 'instanceId'>): EnhancedCard {
  return {
    ...baseCard,
    groupKey: baseCard.id,
    instanceId: uuidv4(),
  };
}

function getPlayerStat(state: CombatState, stat: 'strength' | 'dexterity' | 'intelligence') {
  return state.player.stats?.[stat] || 0;
}

// Helper to get hybrid scaling for Brawler
function getBrawlerScaling(state: CombatState) {
  const str = state.player.stats.strength || 0;
  const dex = state.player.stats.dexterity || 0;
  return Math.floor(str / 3) + Math.floor(dex / 3);
}

// Card templates and helpers defined at the top level
const oneTwoPunch: Omit<EnhancedCard, 'instanceId'> = {
  id: 'one_two_punch',
  name: 'One-Two Punch',
  type: 'Attack',
  cost: 1,
  tags: ['Attack', 'Combo', 'Physical', 'Momentum'],
  description: 'Deal 4 physical damage twice. Gain 1 Momentum.',
  baseDescription: 'Deal 4 physical damage twice. Gain 1 Momentum.',
  comboDescription: 'If played after a Skill card, gain 2 Momentum instead.',
  requirements: { strength: 18, dexterity: 18 },
  comboWith: ['Skill'],
  comboHighlightType: 'type',
  comboEffect: (state: CombatState) => {
    // If last card was Skill, gain 2 Momentum + momentumGain
    if (state.lastPlayedCard?.type === 'Skill') {
      const momentumGain = state.player.stats.momentumGain || 0;
      return {
        ...state,
        player: {
          ...state.player,
          momentum: (state.player.momentum || 0) + 2 + momentumGain,
        },
      };
    }
    return state;
  },
  effect: (state: CombatState, target: Enemy, cardInstance: EnhancedCard, logCallbackMap?: Record<string, (msg: string) => void>) => {
    if (!target) return state;
    let newState = state;
    for (let i = 0; i < 2; i++) {
      const scaling = getBrawlerScaling(state);
      const baseDamage = 4 + scaling + ((cardInstance?.level || 1) - 1);
      const weaponDmg = getWeaponAndElementalDamage({ tags: ['Attack', 'Physical'] }, state.player.stats);
      const totalPhysical = baseDamage + weaponDmg.physical;
      const { damage: finalDamage } = calculateCritDamage(totalPhysical, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
      const dmg = applyDamageModifiers(finalDamage, newState, createCardInstance(oneTwoPunch), target);
      newState = {
        ...newState,
        enemies: newState.enemies.map((e: Enemy) => e.id === target.id ? applyDamageToEnemy(e, dmg, newState.turn, 'physical', 10, logCallbackMap?.[e.id]) : e),
      };
    }
    // Gain 1 Momentum + momentumGain
    const momentumGain = state.player.stats.momentumGain || 0;
    newState = {
      ...newState,
      player: {
        ...newState.player,
        momentum: (newState.player.momentum || 0) + 1 + momentumGain,
      },
    };
    return newState;
  },
  baseEffect: (state: CombatState, target: Enemy, cardInstance: EnhancedCard) => state,
  upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
  embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'one_two_punch'
};

const momentumSpike: Omit<EnhancedCard, 'instanceId'> = {
  id: 'momentum_spike',
  name: 'Momentum Spike',
  type: 'Attack',
  cost: 2,
  tags: ['Attack', 'Momentum', 'AoE', 'Physical'],
  description: 'Spend all Momentum. Deal 3 physical damage to all enemies per Momentum spent.',
  baseDescription: 'Spend all Momentum. Deal 3 physical damage to all enemies per Momentum spent.',
  comboDescription: 'If played after an Attack, deal +1 damage per Momentum.',
  requirements: { strength: 20, dexterity: 16 },
  comboWith: ['Attack'],
  comboHighlightType: 'type',
  comboEffect: (state: CombatState) => {
    // If last card was Attack, deal +1 damage per Momentum
    return state;
  },
  effect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard, logCallbackMap?: Record<string, (msg: string) => void>) => {
    const momentum = state.player.momentum || 0;
    if (momentum === 0) {
      console.debug('[Momentum Spike] No momentum to spend.');
      return state;
    }
    const scaling = getBrawlerScaling(state);
    const baseDamage = 3 + scaling + (state.lastPlayedCard?.type === 'Attack' ? 1 : 0);
    const weaponDmg = getWeaponAndElementalDamage({ tags: ['Attack', 'Physical'] }, state.player.stats);
    const totalPhysical = baseDamage + weaponDmg.physical;
    const { damage: finalDamage } = calculateCritDamage(totalPhysical, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
    console.debug('[Momentum Spike] momentum:', momentum, 'baseDamage:', baseDamage, 'weaponDmg:', weaponDmg, 'finalDamage:', finalDamage);
    const newEnemies = state.enemies.map((enemy: Enemy) => {
      const dmg = applyDamageModifiers(finalDamage, state, createCardInstance(momentumSpike), enemy);
      const resultEnemy = applyDamageToEnemy(enemy, dmg * momentum, state.turn, 'physical', 10, logCallbackMap?.[enemy.id]);
      console.debug(`[Momentum Spike] Enemy ${enemy.name} (${enemy.id}) took`, dmg * momentum, 'damage. HP:', enemy.hp, '->', resultEnemy.hp);
      return resultEnemy;
    });
    return {
      ...state,
      enemies: newEnemies,
      player: {
        ...state.player,
        momentum: 0,
      },
    };
  },
  baseEffect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard) => state,
  upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
  embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'momentum_spike'
};

const guardbreaker: Omit<EnhancedCard, 'instanceId'> = {
  id: 'guardbreaker',
  name: 'Guardbreaker',
  type: 'Skill',
  cost: 1,
  tags: ['Skill', 'Momentum', 'Guard'],
  description: 'Spend up to 2 Momentum. Gain 4 Guard per Momentum spent.',
  baseDescription: 'Spend up to 2 Momentum. Gain 4 Guard per Momentum spent.',
  comboDescription: 'If played after an Attack, gain 2 extra Guard.',
  requirements: { strength: 16, dexterity: 16 },
  comboWith: ['Attack'],
  comboHighlightType: 'type',
  comboEffect: (state: CombatState) => {
    // If last card was Attack, gain 2 extra Guard
    return {
      ...state,
      player: {
        ...state.player,
        guard: (state.player.guard || 0) + 2,
      },
    };
  },
  effect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard, logCallbackMap?: Record<string, (msg: string) => void>) => {
    const spend = Math.min(state.player.momentum || 0, 2);
    return {
      ...state,
      player: {
        ...state.player,
        guard: (state.player.guard || 0) + 4 * spend,
        momentum: (state.player.momentum || 0) - spend,
      },
    };
  },
  baseEffect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard) => state,
  upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
  embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'guardbreaker'
};

const sweepingStrike: Omit<EnhancedCard, 'instanceId'> = {
  id: 'sweeping_strike',
  name: 'Sweeping Strike',
  type: 'Attack',
  cost: 1,
  tags: ['Attack', 'AoE', 'Momentum', 'Physical'],
  description: 'Deal 5 physical damage to all enemies. If you have Momentum, deal +2 damage and lose 1 Momentum.',
  baseDescription: 'Deal 5 physical damage to all enemies. If you have Momentum, deal +2 damage and lose 1 Momentum.',
  comboDescription: 'If played after a Skill card, gain 1 Momentum.',
  requirements: { strength: 18, dexterity: 16 },
  comboWith: ['Skill'],
  comboHighlightType: 'type',
  comboEffect: (state: CombatState) => {
    // If last card was Skill, gain 1 Momentum
    return {
      ...state,
      player: {
        ...state.player,
        momentum: (state.player.momentum || 0) + 1,
      },
    };
  },
  effect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard, logCallbackMap?: Record<string, (msg: string) => void>) => {
    const hasMomentum = (state.player.momentum || 0) > 0;
    const scaling = getBrawlerScaling(state);
    const baseDamage = 5 + scaling + ((cardInstance?.level || 1) - 1) + (hasMomentum ? 2 : 0);
    const weaponDmg = getWeaponAndElementalDamage({ tags: ['Attack', 'Physical'] }, state.player.stats);
    const totalPhysical = baseDamage + weaponDmg.physical;
    const { damage: finalDamage } = calculateCritDamage(totalPhysical, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
    return {
      ...state,
      enemies: state.enemies.map((enemy: Enemy) => {
        const dmg = applyDamageModifiers(finalDamage, state, createCardInstance(sweepingStrike), enemy);
        return applyDamageToEnemy(enemy, dmg, state.turn, 'physical', 10, logCallbackMap?.[enemy.id]);
      }),
      player: {
        ...state.player,
        momentum: hasMomentum ? (state.player.momentum - 1) : state.player.momentum,
      },
    };
  },
  baseEffect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard) => state,
  upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
  embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'sweeping_strike'
};

const adrenalineFlow: Omit<EnhancedCard, 'instanceId'> = {
  id: 'adrenaline_flow',
  name: 'Adrenaline Flow',
  type: 'Skill',
  cost: 1,
  tags: ['Skill', 'Buff', 'Momentum'],
  description: 'Gain +10% attack speed and +5% crit chance for 2 turns.',
  baseDescription: 'Gain +10% attack speed and +5% crit chance for 2 turns.',
  comboDescription: 'If played after an Attack, gain 1 Momentum.',
  requirements: { strength: 14, dexterity: 20 },
  comboWith: ['Attack'],
  comboHighlightType: 'type',
  comboEffect: (state: CombatState) => {
    // If last card was Attack, gain 1 Momentum
    return {
      ...state,
      player: {
        ...state.player,
        momentum: (state.player.momentum || 0) + 1,
      },
    };
  },
  effect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard, logCallbackMap?: Record<string, (msg: string) => void>) => {
    // Add status effect for attack speed and crit chance
    const buff: StatusEffect = {
      id: 'adrenaline_buff',
      name: 'Adrenaline Flow',
      description: '+10% attack speed, +5% crit chance',
      type: 'buff',
      value: 1,
      duration: 2,
      source: 'Skill',
      icon: '💥',
      effect: (s: CombatState) => s,
    };
    return {
      ...state,
      player: {
        ...state.player,
        status: [...(state.player.status || []), buff],
      },
    };
  },
  baseEffect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard) => state,
  upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
  embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'adrenaline_flow'
};

const bleedingEdge: Omit<EnhancedCard, 'instanceId'> = {
  id: 'bleeding_edge',
  name: 'Bleeding Edge',
  type: 'Attack',
  cost: 1,
  tags: ['Attack', 'Physical', 'Bleed'],
  description: 'Deal 6 physical damage. Apply Bleed (2 turns, 20% of hit).',
  baseDescription: 'Deal 6 physical damage. Apply Bleed (2 turns, 20% of hit).',
  comboDescription: 'If played after a Skill card, gain 1 Momentum.',
  requirements: { strength: 18, dexterity: 18 },
  comboWith: ['Skill'],
  comboHighlightType: 'type',
  comboEffect: (state: CombatState) => {
    // If last card was Skill, gain 1 Momentum
    return {
      ...state,
      player: {
        ...state.player,
        momentum: (state.player.momentum || 0) + 1,
      },
    };
  },
  effect: (state: CombatState, target: Enemy, cardInstance: EnhancedCard, logCallbackMap?: Record<string, (msg: string) => void>) => {
    if (!target) return state;
    const scaling = getBrawlerScaling(state);
    const baseDamage = 6 + scaling + ((cardInstance?.level || 1) - 1);
    const weaponDmg = getWeaponAndElementalDamage({ tags: ['Attack', 'Physical'] }, state.player.stats);
    const totalPhysical = baseDamage + weaponDmg.physical;
    const attackSpeed = state.player.stats.drawSpeed || 0;
    return applyMultiHitAttack(state, target, totalPhysical, attackSpeed, state.turn, (s, t, dmg) =>
      applyOnHitEffects(s, t, dmg, cardInstance)
    );
  },
  baseEffect: (state: CombatState, target: Enemy, cardInstance: EnhancedCard) => state,
  upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
  embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'bleeding_edge'
};

// Quick Jab (replaces Aggressive Advance)
const quickJab: Omit<EnhancedCard, 'instanceId'> = {
  id: 'quick_jab',
  name: 'Quick Jab',
  type: 'Attack',
  cost: 0,
  tags: ['Attack', 'Combo', 'Momentum', 'AttackSpeed'],
  description: 'Deal 3 physical damage (+reduced scaling). If you have Momentum, gain +30% Attack Speed for 2 turns.',
  baseDescription: 'Deal 3 physical damage (+reduced scaling). If you have Momentum, gain +30% Attack Speed for 2 turns.',
  comboDescription: 'If played after a Guard card, gain 1 extra Momentum.',
  requirements: { strength: 16, dexterity: 18 },
  comboWith: ['Guard'],
  comboHighlightType: 'type',
  comboEffect: (state: CombatState) => {
    // If last card was Guard, gain 1 extra Momentum + momentumGain
    const momentumGain = state.player.stats.momentumGain || 0;
    return {
      ...state,
      player: {
        ...state.player,
        momentum: (state.player.momentum || 0) + 1 + momentumGain,
      },
    };
  },
  effect: (state: CombatState, target: Enemy, cardInstance: EnhancedCard, logCallbackMap?: Record<string, (msg: string) => void>) => {
    if (!target) return state;
    const scaling = getBrawlerScaling(state);
    // Deal 3 physical damage (reduced scaling, multi-hit with attack speed)
    const baseDamage = 3 + Math.floor(scaling/2) + ((cardInstance?.level || 1) - 1);
    const weaponDmg = getWeaponAndElementalDamage({ tags: ['Attack', 'Physical'] }, state.player.stats);
    const totalPhysical = baseDamage + weaponDmg.physical;
    const attackSpeed = state.player.stats.drawSpeed || 0; // Or use a dedicated attack speed stat if available
    const hits = getMultiHitBreakdown(totalPhysical, attackSpeed);
    let newState = state;
    hits.forEach(dmg => {
      newState = {
        ...newState,
        enemies: newState.enemies.map((e: Enemy) =>
          e.id === target.id ? applyDamageToEnemy(e, dmg, newState.turn, 'physical', 10, logCallbackMap?.[e.id]) : e
        ),
      };
      // Placeholder: applyOnHitEffects(newState, target, dmg, cardInstance);
    });
    // Always gain 1 Momentum + momentumGain
    const momentumGain = state.player.stats.momentumGain || 0;
    newState = {
      ...newState,
      player: {
        ...newState.player,
        momentum: (newState.player.momentum || 0) + 1 + momentumGain,
      },
    };
    // If player has Momentum, grant +30% Attack Speed for 2 turns
    if ((state.player.momentum || 0) > 0) {
      const buff: StatusEffect = {
        id: 'attack_speed_buff',
        name: 'Attack Speed',
        description: '+30% Attack Speed',
        type: 'buff',
        value: 30,
        duration: 2,
        source: 'Skill',
        icon: '💨',
        effect: (s: CombatState) => s,
      };
      newState = {
        ...newState,
        player: {
          ...newState.player,
          status: [...(newState.player.status || []), buff],
        },
      };
    }
    return newState;
  },
  baseEffect: (state: CombatState, target: Enemy, cardInstance: EnhancedCard) => state,
  upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
  embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'quick_jab'
};

// Steadfast Guard (basic Guard card)
const steadfastGuard: Omit<EnhancedCard, 'instanceId'> = {
  id: 'steadfast_guard',
  name: 'Steadfast Guard',
  type: 'Guard',
  cost: 1,
  tags: ['Guard', 'Combo', 'Momentum'],
  description: 'Gain 6 Guard. If you have Momentum, gain +2 Guard.',
  baseDescription: 'Gain 6 Guard. If you have Momentum, gain +2 Guard.',
  comboDescription: 'If played after an Attack, gain 1 Momentum.',
  requirements: { strength: 16, dexterity: 16 },
  comboWith: ['Attack'],
  comboHighlightType: 'type',
  comboEffect: (state: CombatState) => {
    // If last card was Attack, gain 1 Momentum
    return {
      ...state,
      player: {
        ...state.player,
        momentum: (state.player.momentum || 0) + 1,
      },
    };
  },
  effect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard, logCallbackMap?: Record<string, (msg: string) => void>) => {
    const baseGuard = 6 + ((cardInstance?.level || 1) - 1);
    const bonusGuard = (state.player.momentum || 0) > 0 ? 2 : 0;
    return {
      ...state,
      player: {
        ...state.player,
        guard: (state.player.guard || 0) + baseGuard + bonusGuard,
      },
    };
  },
  baseEffect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard) => state,
  upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
  embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'steadfast_guard'
};

const brawlersInstinctAura: Omit<EnhancedCard, 'instanceId'> = {
  id: 'brawlers_instinct',
  name: "Brawler's Instinct",
  type: 'Aura',
  cost: 0,
  tags: ['Aura', 'Passive', 'Brawler'],
  description: 'Each time you play an Attack card this turn, gain a stack. Each stack causes your next Attack to hit an additional random enemy. Resets each turn.',
  baseDescription: 'Each time you play an Attack card this turn, gain a stack. Each stack causes your next Attack to hit an additional random enemy. Resets each turn.',
  requirements: {},
  effect: (state: CombatState) => state,
  baseEffect: (state: CombatState) => state,
  upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
  embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 1, modifiers: [], rarity: 'Unique', groupKey: 'brawlers_instinct'
};


export function createBrawlerStarterDeck() {
  const starterDeck = [
    ...Array(6).fill(null).map(() => createCardInstance(oneTwoPunch)),
    ...Array(1).fill(null).map(() => createCardInstance(momentumSpike)),
    ...Array(2).fill(null).map(() => createCardInstance(guardbreaker)),
    ...Array(2).fill(null).map(() => createCardInstance(sweepingStrike)),
    ...Array(1).fill(null).map(() => createCardInstance(adrenalineFlow)),
    ...Array(2).fill(null).map(() => createCardInstance(bleedingEdge)),
    ...Array(2).fill(null).map(() => createCardInstance(quickJab)),
    ...Array(2).fill(null).map(() => createCardInstance(steadfastGuard)),
  ];
  return starterDeck;
}

export const BRAWLER_STARTER_CARDS = [
  ...Array(4).fill(null).map(() => createCardInstance(oneTwoPunch)),
  ...Array(2).fill(null).map(() => createCardInstance(momentumSpike)),
  ...Array(2).fill(null).map(() => createCardInstance(guardbreaker)),
  ...Array(2).fill(null).map(() => createCardInstance(sweepingStrike)),
  ...Array(2).fill(null).map(() => createCardInstance(adrenalineFlow)),
  ...Array(2).fill(null).map(() => createCardInstance(bleedingEdge)),
  ...Array(2).fill(null).map(() => createCardInstance(quickJab)),
  ...Array(2).fill(null).map(() => createCardInstance(steadfastGuard)),
]; 