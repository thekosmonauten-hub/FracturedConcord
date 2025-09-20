import type { EnhancedCard } from '../../types/cards/enhancedCards';
import type { CombatState } from '../../types/combat/combat';
import type { Enemy } from '../../types/combat/enemies';
import { v4 as uuidv4 } from 'uuid';
import { calculateCritDamage } from '../../utils/crit';
import { applyDamageToEnemy, applyDamageModifiers, getWeaponAndElementalDamage } from '../../utils/damage';
import { applyPoison, applyIgnite, applyBleed } from '../../utils/statusEffects';
import { applyAilments } from '../../utils/ailments';
import { getAdjacentEnemies } from '../../utils/combat';
import { EQUIPMENT_BASES } from '../../types/loot/equipment';
import { createGearItem } from '../../utils/itemGeneration';
import type { GearItem } from '../../types/loot/loot';
import type { CharacterClass } from '../../types/combat/character';
import { calculateTemporaryStatsEnhanced } from '../../utils/stats';

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

// Helper for stat access with temporary stats included
function getPlayerStat(state: CombatState, stat: 'strength' | 'dexterity' | 'intelligence') {
  const baseStat = state.player.stats?.[stat] || 0;
  const tempStats = calculateTemporaryStatsEnhanced(state.player.status || []);
  const tempStat = tempStats[stat] || 0;
  return baseStat + tempStat;
}

// Helper to get enhanced discard power with temporary stats
function getEnhancedDiscardPower(state: CombatState): number {
  const baseDiscardPower = state.player.stats.discardPower || 0;
  const tempStats = calculateTemporaryStatsEnhanced(state.player.status || []);
  const tempDiscardPower = tempStats.discardPower || 0;
  return baseDiscardPower + tempDiscardPower;
}

// Helper to get enhanced spell power with temporary stats
function getEnhancedSpellPower(state: CombatState): number {
  const baseSpellPower = state.player.stats.spellPower || 0;
  const tempStats = calculateTemporaryStatsEnhanced(state.player.status || []);
  const tempSpellPower = tempStats.spellPower || 0;
  return baseSpellPower + tempSpellPower;
}

export function createApostleStarterDeck() {
  // Basic Attack - Sacred Strike (6x)
  const sacredStrike: Omit<EnhancedCard, 'instanceId'> = {
    id: 'sacred_strike',
    name: 'Sacred Strike',
    type: 'Attack',
    cost: 1,
    tags: ['WeaponAttack', 'Attack', 'Physical', 'Chaos', 'Combo', 'AoE'],
    description: 'Deal 8 physical damage (+Str/2) to targetted Pack.',
    baseDescription: 'Deal 8 physical damage (+Str/2) to targetted Pack.',
    effect: (state: CombatState, target: Enemy, cardInstance: EnhancedCard, logCallbackMap?: Record<string, (msg: string) => void>) => {
      if (!target) return state;
      const str = getPlayerStat(state, 'strength');
      let baseDamage = 8 + Math.floor(str / 2) + 2 * ((cardInstance?.level || 1) - 1); // +2 per level
      
      // Check if any cards were discarded before playing this card
      // We check if the discard count is > 0 because the current card's discard hasn't been counted yet
      const hasDiscarded = state.cardsDiscardedThisTurn && state.cardsDiscardedThisTurn > 0;
      console.log(`[Sacred Strike] cardsDiscardedThisTurn: ${state.cardsDiscardedThisTurn}, hasDiscarded: ${hasDiscarded}`);
      if (hasDiscarded) {
        const discardPower = getEnhancedDiscardPower(state);
        baseDamage += 5 + discardPower;
        console.log(`[Sacred Strike] Applied discard bonus: +${5 + discardPower} damage`);
      }
      
      // Get enhanced stats for weapon damage calculation
      const enhancedStats = {
        ...state.player.stats,
        strength: getPlayerStat(state, 'strength'),
        discardPower: getEnhancedDiscardPower(state),
      };
      const weaponDmg = getWeaponAndElementalDamage({ tags: ['WeaponAttack', 'Physical'] }, enhancedStats);
      const totalPhysical = baseDamage + weaponDmg.physical;
      const { damage: finalDamage } = calculateCritDamage(totalPhysical, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
      
      // Get all enemies in the same pack as the target
      const targetPackId = target.packId;
      const packEnemies = state.enemies.filter(e => e.packId && targetPackId && e.packId === targetPackId);
      // Always include the main target (in case it has no packId)
      const affectedEnemyIds = new Set([target.id, ...packEnemies.map(e => e.id)]);
      // Apply damage to all enemies in the pack
      const newEnemies = state.enemies.map(enemy => {
        if (affectedEnemyIds.has(enemy.id)) {
          const dmg = applyDamageModifiers(finalDamage, state, withInstanceId(sacredStrike), enemy);
          let updatedEnemy = applyDamageToEnemy(enemy, dmg, state.turn, 'physical', 25, logCallbackMap?.[enemy.id], undefined, ['WeaponAttack', 'Physical']);
          updatedEnemy = applyAilments(updatedEnemy, dmg, cardInstance);
          return updatedEnemy;
        }
        return enemy;
      });
      
      return {
        ...state,
        enemies: newEnemies,
      };
    },
    baseEffect: (state: CombatState, target: Enemy, cardInstance: EnhancedCard) => state,
    ifDiscarded: (state: CombatState, card) => {
      if (state.enemies.length === 0) return state;
      
      // Get all unique pack IDs
      const packIds = [...new Set(state.enemies.map(e => e.packId).filter(id => id))];
      
      if (packIds.length === 0) {
        // If no packs, target a random enemy
        const randomEnemy = state.enemies[Math.floor(Math.random() * state.enemies.length)];
        const discardPower = getEnhancedDiscardPower(state);
        const damage = 8 + discardPower;
        
        const { damage: finalDamage } = calculateCritDamage(damage, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
        const dmg = applyDamageModifiers(finalDamage, state, withInstanceId(sacredStrike), randomEnemy);
        const updatedEnemy = applyDamageToEnemy(randomEnemy, dmg, state.turn, 'chaos', 15, undefined, undefined, ['Attack', 'Chaos', 'Spell']);
        
        return {
          ...state,
          enemies: state.enemies.map(e => e.id === randomEnemy.id ? updatedEnemy : e),
          statusLog: [...state.statusLog, `Sacred Strike discarded dealt ${dmg} chaos damage!`]
        };
      }
      
      // Select a random pack
      const randomPackId = packIds[Math.floor(Math.random() * packIds.length)];
      const packEnemies = state.enemies.filter(e => e.packId === randomPackId);
      
      const discardPower = getEnhancedDiscardPower(state);
      const damage = 8 + discardPower;
      
      const { damage: finalDamage } = calculateCritDamage(damage, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
      
      // Apply damage to all enemies in the selected pack
      const newEnemies = state.enemies.map(enemy => {
        if (enemy.packId === randomPackId) {
          const dmg = applyDamageModifiers(finalDamage, state, withInstanceId(sacredStrike), enemy);
          const updatedEnemy = applyDamageToEnemy(enemy, dmg, state.turn, 'chaos', 15, undefined, undefined, ['Attack', 'Chaos', 'Spell']);
          return updatedEnemy;
        }
        return enemy;
      });
      
      return {
        ...state,
        enemies: newEnemies,
        statusLog: [...state.statusLog, `Sacred Strike discarded dealt ${damage} chaos damage to pack!`]
      };
    },
    ifDiscardedDescription: 'Deal {discardPower} chaos damage to a random pack.',
    upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
    embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'sacred_strike'
  };

  // Basic Defense - Divine Ward (4x)
  const divineWard: Omit<EnhancedCard, 'instanceId'> = {
    id: 'apostle-basic-defense',
    name: 'Divine Ward',
    type: 'Guard',
    cost: 1,
    tags: ['Guard', 'Chaos', 'Combo'],
    description: 'Gain 8 Guard (+Str/3).',
    baseDescription: 'Gain 8 Guard (+Str/3).',
    effect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard, logCallbackMap?: Record<string, (msg: string) => void>) => {
      const str = getPlayerStat(state, 'strength');
      let guardAmount = 8 + Math.floor(str / 3) + ((cardInstance?.level || 1) - 1); // +1 per level
      
      // Check if any cards were discarded before playing this card
      // We check if the discard count is > 0 because the current card's discard hasn't been counted yet
      const hasDiscarded = state.cardsDiscardedThisTurn && state.cardsDiscardedThisTurn > 0;
      console.log(`[Divine Ward] cardsDiscardedThisTurn: ${state.cardsDiscardedThisTurn}, hasDiscarded: ${hasDiscarded}`);
      if (hasDiscarded) {
        const discardPower = getEnhancedDiscardPower(state);
        guardAmount += 4 + discardPower;
        console.log(`[Divine Ward] Applied discard bonus: +${4 + discardPower} guard`);
      }
      
      return {
        ...state,
        player: {
          ...state.player,
          guard: (state.player.guard || 0) + guardAmount,
        },
      };
    },
    baseEffect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard) => state,
    ifDiscarded: (state: CombatState, card) => {
      const discardPower = getEnhancedDiscardPower(state);
      const guardAmount = 5 + Math.floor(discardPower / 2);
      
      return {
        ...state,
        player: {
          ...state.player,
          guard: (state.player.guard || 0) + guardAmount,
        },
        statusLog: [...state.statusLog, `Divine Ward discarded: Gained ${guardAmount} Guard!`]
      };
    },
    ifDiscardedDescription: 'Gain {discardPower} Guard.',
    upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
    embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'apostle-basic-defense'
  };

  // AoE Attack - Divine Wrath (2x)
  const divineWrath: Omit<EnhancedCard, 'instanceId'> = {
    id: 'apostle-divine-wrath',
    name: 'Divine Wrath',
    type: 'Attack',
    cost: 2,
    tags: ['Attack', 'Chaos', 'AoE', 'Combo', 'Spell'],
    description: 'Deal 8 chaos damage (+Int/3) to all enemies.',
    baseDescription: 'Deal 8 chaos damage (+Int/3) to all enemies.',
    effect: (state: CombatState, target: Enemy, cardInstance: EnhancedCard, logCallbackMap?: Record<string, (msg: string) => void>) => {
      const int = getPlayerStat(state, 'intelligence');
      let baseDamage = 8 + Math.floor(int / 3) + Math.round(1.5 * ((cardInstance?.level || 1) - 1)); // +1.5 per level
      
      // Check if any cards were discarded before playing this card
      // We check if the discard count is > 0 because the current card's discard hasn't been counted yet
      const hasDiscarded = state.cardsDiscardedThisTurn && state.cardsDiscardedThisTurn > 0;
      console.log(`[Divine Wrath] cardsDiscardedThisTurn: ${state.cardsDiscardedThisTurn}, hasDiscarded: ${hasDiscarded}`);
      if (hasDiscarded) {
        const discardPower = getEnhancedDiscardPower(state);
        baseDamage += 4 + discardPower;
        console.log(`[Divine Wrath] Applied discard bonus: +${4 + discardPower} damage`);
      }
      
      // Apply spell power bonus to base damage
      const enhancedSpellPower = getEnhancedSpellPower(state);
      const spellPowerBonus = enhancedSpellPower / 100; // Convert percentage to multiplier
      const spellModifiedDamage = baseDamage * (1 + spellPowerBonus);
      
      const { damage: finalDamage } = calculateCritDamage(spellModifiedDamage, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
      
      return {
        ...state,
        enemies: state.enemies.map((enemy: Enemy) => {
          const dmg = applyDamageModifiers(finalDamage, state, withInstanceId(divineWrath), enemy);
          let updatedEnemy = applyDamageToEnemy(enemy, dmg, state.turn, 'chaos', 20, logCallbackMap?.[enemy.id], undefined, ['Attack', 'Chaos']);
          updatedEnemy = applyAilments(updatedEnemy, dmg, cardInstance);
          return updatedEnemy;
        }),
      };
    },
    baseEffect: (state: CombatState, target: Enemy, cardInstance: EnhancedCard) => state,
    ifDiscarded: (state: CombatState, card) => {
      if (state.enemies.length === 0) return state;
      
      const discardPower = getEnhancedDiscardPower(state);
      const damage = 4 + Math.floor(discardPower / 2);
      
      const { damage: finalDamage } = calculateCritDamage(damage, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
      
      return {
        ...state,
        enemies: state.enemies.map((enemy: Enemy) => {
          const dmg = applyDamageModifiers(finalDamage, state, withInstanceId(divineWrath), enemy);
          const updatedEnemy = applyDamageToEnemy(enemy, dmg, state.turn, 'chaos', 15, logCallbackMap?.[enemy.id], undefined, ['Attack', 'Chaos', 'Spell']);
          return updatedEnemy;
        }),
        statusLog: [...state.statusLog, `Divine Wrath discarded dealt ${damage} chaos damage to all enemies!`]
      };
    },
    ifDiscardedDescription: 'Deal {discardPower} chaos damage to all enemies.',
    upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
    embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'apostle-divine-wrath'
  };

  // Skill - Scripture Burn (2x)
  const scriptureBurn: Omit<EnhancedCard, 'instanceId'> = {
    id: 'apostle-scripture-burn',
    name: 'Scripture Burn',
    type: 'Skill',
    cost: 1,
    tags: ['Skill', 'Chaos', 'Discard', 'Combo'],
    description: 'Discard 1 card. Gain 6 Guard (+Int/2 + Discard Power) and 3 temporary Intelligence.',
    baseDescription: 'Discard 1 card. Gain 6 Guard (+Int/2 + Discard Power) and 3 temporary Intelligence.',
    effect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard, logCallbackMap?: Record<string, (msg: string) => void>) => {
      const int = getPlayerStat(state, 'intelligence');
      const guardAmount = 6 + Math.floor(int / 2) + ((cardInstance?.level || 1) - 1); // +1 per level
      
      // Add temporary intelligence status effect
      const tempIntBuff = {
        id: 'temp_intelligence_' + Math.random().toString(36).substr(2, 9),
        name: 'Temporary Intelligence',
        description: `Intelligence increased by ${3}`,
        type: 'buff' as const,
        value: 3,
        duration: -1, // Rest of combat
        source: 'Skill',
        icon: '🧠',
        effect: (state: CombatState) => state,
      };
      
      return {
        ...state,
        player: {
          ...state.player,
          guard: (state.player.guard || 0) + guardAmount,
          status: [...state.player.status, tempIntBuff],
        },
      };
    },
    baseEffect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard) => state,
    ifDiscarded: (state: CombatState, card) => {
      const discardPower = getEnhancedDiscardPower(state);
      const cardsToDraw = 1 + Math.floor(discardPower / 6); // Reduced from 2 + Math.floor(discardPower / 4)
      
      let drawnCards = [];
      let newDeck = [...state.deck];
      let newHand = [...state.hand];
      
      for (let i = 0; i < cardsToDraw && newDeck.length > 0; i++) {
        const drawnCard = newDeck.shift()!;
        drawnCards.push(drawnCard);
        newHand.push(drawnCard);
      }
      
      if (drawnCards.length > 0) {
        return {
          ...state,
          deck: newDeck,
          hand: newHand,
          statusLog: [...state.statusLog, `Scripture Burn discarded: Drew ${drawnCards.length} card(s)!`]
        };
      }
      return {
        ...state,
        statusLog: [...state.statusLog, 'Scripture Burn discarded: No cards to draw!']
      };
    },
    ifDiscardedDescription: 'Draw {discardPower} card(s).',
    upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
    embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'apostle-scripture-burn'
  };

  // Power - Divine Favor (2x)
  const divineFavor: Omit<EnhancedCard, 'instanceId'> = {
    id: 'apostle-divine-favor',
    name: 'Divine Favor',
    type: 'Power',
    cost: 2,
    tags: ['Power', 'Chaos', 'Combo', 'Spell'],
    description: 'The next card you play applies their "discarded" effect.',
    baseDescription: 'The next card you play applies their "discarded" effect.',
    effect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard, logCallbackMap?: Record<string, (msg: string) => void>) => {
      // Add a status effect to track the next card's "If discarded" effect
      const nextCardDiscardEffect = {
        id: 'next_card_discard_effect_' + Math.random().toString(36).substr(2, 9),
        name: 'Divine Favor',
        description: 'Next card you play applies their "Discarded" effect',
        type: 'buff' as const,
        value: 1,
        duration: -1, // Until consumed
        source: 'Power',
        icon: '✝️',
        effect: (state: CombatState) => state,
      };
      
      return {
        ...state,
        player: {
          ...state.player,
          status: [...state.player.status, nextCardDiscardEffect],
        },
      };
    },
    baseEffect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard) => state,
    ifDiscarded: (state: CombatState, card) => {
      const discardPower = getEnhancedDiscardPower(state);
      const discardPowerIncrease = 3 + Math.floor(discardPower / 3);
      
      // Add a status effect for temporary Discard Power
      const tempDiscardPowerEffect = {
        id: 'temp_discard_power_' + Math.random().toString(36).substr(2, 9),
        name: 'Temporary Discard Power',
        description: `Discard Power increased by ${discardPowerIncrease}`,
        type: 'buff' as const,
        value: discardPowerIncrease,
        duration: -1, // Rest of combat
        source: 'Power',
        icon: '✨',
        effect: (state: CombatState) => state,
      };
      
      return {
        ...state,
        player: {
          ...state.player,
          status: [...state.player.status, tempDiscardPowerEffect],
        },
        statusLog: [...state.statusLog, `Divine Favor discarded: Gained ${discardPowerIncrease} Discard Power!`]
      };
    },
    ifDiscardedDescription: 'Gain {discardPower} Discard Power.',
    upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
    embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'apostle-divine-favor'
  };

  // Skill - Forbidden Prayer (2x)
  const forbiddenPrayer: Omit<EnhancedCard, 'instanceId'> = {
    id: 'apostle-forbidden-prayer',
    name: 'Forbidden Prayer',
    type: 'Skill',
    cost: 0,
    tags: ['Skill', 'Discard', 'Chaos'],
    description: 'Discard 1 card: draw 3 cards.',
    baseDescription: 'Discard 1 card: draw 3 cards.',
    effect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard, logCallbackMap?: Record<string, (msg: string) => void>) => {
      // This card requires manual discard selection, so we'll just return the state
      // The actual discard and draw logic will be handled by the UI/combat system
      return {
        ...state,
        // The combat system should handle: discard 1 card, then draw 3 cards
      };
    },
    baseEffect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard) => state,
    ifDiscarded: (state: CombatState, card) => {
      const int = getPlayerStat(state, 'intelligence');
      const discardPower = getEnhancedDiscardPower(state);
      const spellPowerIncrease = 2 + Math.floor(int / 10) + Math.floor(discardPower / 4);
      
      // Add a temporary spell power status effect
      const tempSpellPowerEffect = {
        id: 'temp_spell_power_' + Math.random().toString(36).substr(2, 9),
        name: 'Temporary Spell Power',
        description: `Spell Power increased by ${spellPowerIncrease}`,
        type: 'buff' as const,
        value: spellPowerIncrease,
        duration: -1, // Rest of combat
        source: 'Skill',
        icon: '✨',
        effect: (state: CombatState) => state,
      };
      
      return {
        ...state,
        player: {
          ...state.player,
          status: [...state.player.status, tempSpellPowerEffect],
        },
        statusLog: [...state.statusLog, `Forbidden Prayer discarded: Increased spell power by ${spellPowerIncrease}!`]
      };
    },
    ifDiscardedDescription: 'Increase spell power by {intelligence}.',
    upgrades: 0, retainOnTurn: false, recycleOnUse: false, exhaustOnUse: false, drawOnPlay: 0, energyRefund: 0,
    embossingSlots: 0, activeEmbossings: [], experience: 0, level: 1, maxLevel: 20, modifiers: [], rarity: 'Normal', groupKey: 'apostle-forbidden-prayer'
  };

  // Return the deck structure
  return {
    basicAttacks: [
      createCardInstance(sacredStrike),
      createCardInstance(sacredStrike),
      createCardInstance(sacredStrike),
      createCardInstance(sacredStrike),
      createCardInstance(sacredStrike),
      createCardInstance(sacredStrike),
    ],
    basicDefenses: [
      createCardInstance(divineWard),
      createCardInstance(divineWard),
      createCardInstance(divineWard),
      createCardInstance(divineWard),
    ],
    divineWraths: [
      createCardInstance(divineWrath),
      createCardInstance(divineWrath),
    ],
    scriptureBurns: [
      createCardInstance(scriptureBurn),
      createCardInstance(scriptureBurn),
    ],
    divineFavors: [
      createCardInstance(divineFavor),
      createCardInstance(divineFavor),
    ],
    forbiddenPrayers: [
      createCardInstance(forbiddenPrayer),
      createCardInstance(forbiddenPrayer),
    ],
  };
}

// Note: Starter equipment is now handled in the centralized StarterEquipment.ts file 