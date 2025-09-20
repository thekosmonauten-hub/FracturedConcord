import type { EnhancedCard } from '../../types/cards/enhancedCards';
import type { CombatState } from '../../types/combat/combat';
import type { Enemy } from '../../types/combat/enemies';
import { detonateSpikes } from '../../utils/combat';
import { applyDamageToEnemy, roundDamage, applyDamageModifiers, getWeaponAndElementalDamage } from '../../utils/damage';
import { v4 as uuidv4 } from 'uuid';
import { EQUIPMENT_BASES } from '../../types/loot/equipment';
import { createGearItem } from '../../utils/itemGeneration';
import type { GearItem } from '../../types/loot/loot';
import type { CharacterClass } from '../../types/combat/character';
import { applyBleed, applyPoison, applyIgnite } from '../../utils/statusEffects';
import { applyAilments } from '../../utils/ailments';
import { calculateCritDamage } from '../../utils/crit';
import { applyMultiHitAttack, applyOnHitEffects } from '../../utils/combatUtils';

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

export function createMarauderStarterDeck() {
  // Basic Attack Card (6 copies)
  const basicAttack: Omit<EnhancedCard, 'instanceId'> = {
    id: 'heavy_strike',
    name: 'Heavy Strike',
    type: 'Attack',
    cost: 1,
    tags: ['WeaponAttack','Attack','Physical','Combo'],
    description: 'A powerful blow that deals 8 physical damage (+ Strength/2). Deals double damage if target is stunned.',
    baseDescription: 'A powerful blow that deals 8 physical damage (+ Strength/2). Deals double damage if target is stunned.',
    comboDescription: 'After using a Skill card, gain +1 energy.',
    requirements: {
      strength: 25
    },
    effect: (state: CombatState, _target?: Enemy, cardInstance?: EnhancedCard, logCallbackMap?: Record<string, (msg: string) => void>) => {
      if (!_target) return state;
      const playerStats = state.player.stats || { strength: 0 };
      let baseDamage = 8 + Math.floor((playerStats.strength || 0) / 2) + 2 * ((cardInstance?.level || 1) - 1); // +2 per level
      const weaponDmg = getWeaponAndElementalDamage({ tags: ['WeaponAttack'] }, playerStats);
      let totalPhysical = baseDamage + weaponDmg.physical;
      if (_target.status.some(s => s.name === 'Stunned' && s.duration > 0)) {
        totalPhysical *= 2;
      }
      const attackSpeed = playerStats.drawSpeed || 0;
      return applyMultiHitAttack(state, _target, totalPhysical, attackSpeed, state.turn, (s, t, dmg) =>
        applyOnHitEffects(s, t, dmg, cardInstance)
      );
    },
    baseEffect: (state: CombatState, _target?: Enemy, cardInstance?: EnhancedCard) => {
      if (!_target) return state;
      const playerStats = state.player.stats || { strength: 0 };
      let baseDamage = 8 + Math.floor((playerStats.strength || 0) / 2) + 2 * ((cardInstance?.level || 1) - 1); // +2 per level
      const weaponDmg = getWeaponAndElementalDamage({ tags: ['WeaponAttack'] }, playerStats);
      let totalPhysical = baseDamage + weaponDmg.physical;
      
      if (_target.status.some(s => s.name === 'Stunned' && s.duration > 0)) {
        totalPhysical *= 2;
      }
      const { damage: finalDamage, isCrit } = calculateCritDamage(totalPhysical, playerStats.criticalChance || 0, playerStats.criticalMultiplier || 150);
      const dmg = applyDamageModifiers(finalDamage, state, withInstanceId(basicAttack), _target);
      // Calculate stagger with increasedStagger
      const baseStagger = 20;
      const increasedStagger = playerStats.increasedStagger || 0;
      const finalStagger = Math.round(baseStagger * (1 + increasedStagger / 100));
      let updatedEnemy = applyDamageToEnemy(_target, dmg, state.turn, 'physical', finalStagger, undefined, undefined, ['WeaponAttack', 'Physical']);
      
      updatedEnemy = applyAilments(updatedEnemy, dmg, cardInstance);
      
      const newEnemies = state.enemies.map(e =>
        e.id === _target.id ? updatedEnemy : e
      );
      
      return { ...state, enemies: newEnemies };
    },
    upgrades: 0,
    retainOnTurn: false,
    recycleOnUse: false,
    exhaustOnUse: false,
    drawOnPlay: 0,
    energyRefund: 0,
    embossingSlots: 0,
    activeEmbossings: [],
    experience: 0,
    level: 1,
    maxLevel: 20,
    modifiers: [],
    rarity: 'Normal',
    groupKey: 'heavy_strike'
  };

  // Basic Defense Card (4 copies)
  const basicDefense: Omit<EnhancedCard, 'instanceId'> = {
    id: 'marauder-basic-defense',
    name: 'Brace',
    type: 'Guard',
    cost: 1,
    tags: ['Guard','Strength','Combo'],
    description: 'Gain 5 Guard (+ Strength/4). Gain 2 temporary Strength.',
    baseDescription: 'Gain 5 Guard (+ Strength/4). Gain 2 temporary Strength.',
    comboDescription: 'After using an Attack card, gain +2 Guard.',
    requirements: {
      strength: 20
    },
    comboWith: ['Attack'],
    comboHighlightType: 'type',
    comboEffect: (state: CombatState): CombatState => {
      if (state.lastPlayedCard?.type === 'Attack') {
        return {
          ...state,
          player: {
            ...state.player,
            guard: state.player.guard + 2, // Add the combo bonus to the existing guard
          },
        };
      }
      return state;
    },
    effect: (state: CombatState, _target?: Enemy, cardInstance?: EnhancedCard) => {
      const strengthBonus = Math.floor(state.player.stats.strength / 4);
      const tempStrengthBonus = Math.floor(state.player.tempStrength / 4);
      const guardAmount = 5 + strengthBonus + tempStrengthBonus + ((cardInstance?.level || 1) - 1); // +1 per level
      
      // Create temporary strength status effect
      const tempStrengthBuff = {
        id: 'temp_strength_' + Math.random().toString(36).substr(2, 9),
        name: 'Temporary Strength',
        description: 'Strength increased by 2',
        type: 'buff' as const,
        value: 2,
        duration: -1, // Rest of combat
        source: 'Skill',
        icon: '💪',
        effect: (state: CombatState) => state, // This will be processed by the stats system
      };
      
      return {
        ...state,
        player: {
          ...state.player,
          guard: state.player.guard + guardAmount,
          tempStrength: state.player.tempStrength + 2,
          status: [...state.player.status, tempStrengthBuff],
        },
      };
    },
    baseEffect: (state: CombatState): CombatState => state, // fallback
    upgrades: 0,
    retainOnTurn: false,
    recycleOnUse: false,
    exhaustOnUse: false,
    drawOnPlay: 0,
    energyRefund: 0,
    embossingSlots: 0,
    activeEmbossings: [],
    experience: 0,
    level: 1,
    maxLevel: 20,
    modifiers: [],
    rarity: 'Normal',
    groupKey: 'marauder-basic-defense'
  };

  // Ground Slam Card (2 copies)
  const groundSlam: Omit<EnhancedCard, 'instanceId'> = {
    id: 'marauder-ground-slam',
    name: 'Ground Slam',
    type: 'Attack',
    cost: 2,
    tags: ['WeaponAttack','Attack','Physical','Combo'],
    description: 'Deal 4 physical damage (+ Strength/4) to all enemies. Apply 2 Vulnerability.',
    baseDescription: 'Deal 4 physical damage (+ Strength/4) to all enemies. Apply 2 Vulnerability.',
    comboDescription: 'After using a Skill card, apply +1 Vulnerability.',
    requirements: {
      strength: 30
    },
    effect: (state: CombatState, _target?: Enemy, cardInstance?: EnhancedCard): CombatState => {
      const strengthBonus = Math.floor(state.player.stats.strength / 4);
      const tempStrengthBonus = Math.floor(state.player.tempStrength / 4);
      const baseDamage = 4 + strengthBonus + tempStrengthBonus + Math.round(1.5 * ((cardInstance?.level || 1) - 1)); // +1.5 per level
      const weaponDmg = getWeaponAndElementalDamage({ tags: ['WeaponAttack'] }, state.player.stats);
      const totalPhysical = baseDamage + weaponDmg.physical;
      const { damage: finalDamage, isCrit } = calculateCritDamage(totalPhysical, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
      return {
        ...state,
        enemies: state.enemies.map((enemy: Enemy) => {
          const dmg = applyDamageModifiers(finalDamage, state, withInstanceId(groundSlam), enemy);
          let updatedEnemy = applyDamageToEnemy(enemy, dmg, state.turn, 'physical', 30, undefined, undefined, ['WeaponAttack', 'Physical']);
          updatedEnemy = applyAilments(updatedEnemy, dmg, cardInstance);
          return {
            ...updatedEnemy,
            status: [
              ...updatedEnemy.status,
              {
                id: 'vulnerability_' + Math.random().toString(36).substr(2, 9),
                name: 'Vulnerability',
                description: 'Take 50% more damage from attacks',
                duration: 2,
                type: 'debuff',
                value: 2,
                effect: (state: CombatState) => state
              },
            ],
          };
        }),
      };
    },
    baseEffect: (state: CombatState, _target?: Enemy, cardInstance?: EnhancedCard): CombatState => state, // fallback
    upgrades: 0,
    retainOnTurn: false,
    recycleOnUse: false,
    exhaustOnUse: false,
    drawOnPlay: 0,
    energyRefund: 0,
    embossingSlots: 0,
    activeEmbossings: [],
    experience: 0,
    level: 1,
    maxLevel: 20,
    modifiers: [],
    rarity: 'Normal',
    groupKey: 'marauder-ground-slam'
  };

  // Intimidating Shout Card (2 copies)
  const intimidatingShout: Omit<EnhancedCard, 'instanceId'> = {
    id: 'marauder-intimidating-shout',
    name: 'Intimidating Shout',
    type: 'Skill',
    cost: 1,
    tags: ['Warcry','Skill','Strength','Combo'],
    description: 'Apply 2 Vulnerability to all enemies (+ 1 per 15 Strength).',
    baseDescription: 'Apply 2 Vulnerability to all enemies (+ 1 per 15 Strength).',
    comboDescription: 'After using an Attack card\nDetonate all pending spikes and gain an Endurance charge.',
    comboWith: ['Attack'],
    comboHighlightType: 'type',
    effect: (state: CombatState, _target?: Enemy, cardInstance?: EnhancedCard) => {
      const extraVulnerability = Math.floor(state.player.stats.strength / 15);
      const levelBonus = (cardInstance?.level || 1) - 1;
      const totalVulnerability = 2 + extraVulnerability + levelBonus; // +1 per level
      return {
        ...state,
        enemies: state.enemies.map((enemy: Enemy) => ({
          ...enemy,
          status: [
            ...enemy.status,
            {
              id: 'vulnerability_' + Math.random().toString(36).substr(2, 9),
              name: 'Vulnerability',
              description: 'Take 50% more damage from attacks',
              duration: 2,
              type: 'debuff',
              value: totalVulnerability,
              effect: (state) => state
            },
          ],
        })),
      };
    },
    comboEffect: function(this: EnhancedCard, state: CombatState): CombatState {
      let newState = this.effect ? this.effect(state) : state;
      if (state.lastPlayedCard?.type === 'Attack') {
        let updatedEnemies = newState.enemies.map((enemy: Enemy) => {
          const detonationResult = detonateSpikes(enemy, state.turn);
          return detonationResult.enemy;
        });
        const currentCharges = newState.player.charges || { endurance: 0, power: 0, frenzy: 0 };
        return {
          ...newState,
          enemies: updatedEnemies,
          player: {
            ...newState.player,
            charges: {
              ...currentCharges,
              endurance: currentCharges.endurance + 1,
            },
          },
        };
      }
      return newState;
    },
    baseEffect: (state: CombatState): CombatState => state, // fallback
    upgrades: 0,
    retainOnTurn: false,
    recycleOnUse: false,
    exhaustOnUse: false,
    drawOnPlay: 0,
    energyRefund: 0,
    embossingSlots: 0,
    activeEmbossings: [],
    experience: 0,
    level: 1,
    maxLevel: 20,
    modifiers: [],
    rarity: 'Normal',
    groupKey: 'marauder-intimidating-shout'
  };

  // Cleave Card (2 copies)
  const cleave: Omit<EnhancedCard, 'instanceId'> = {
    id: 'cleave',
    name: 'Cleave',
    type: 'Attack',
    cost: 1,
    tags: ['WeaponAttack','Attack','Physical','Combo'],
    description: 'Deal 7 physical damage (+ Strength/3) to all enemies.',
    baseDescription: 'Deal 7 physical damage (+ Strength/3) to all enemies.',
    comboDescription: 'After using a Guard card\nApply 1 Vulnerability to enemies hit',
    requirements: {
      strength: 28
    },
    comboWith: ['Guard'],
    comboHighlightType: 'type',
    effect: (state: CombatState, _target?: Enemy, cardInstance?: EnhancedCard): CombatState => {
      const strengthBonus = Math.floor(state.player.stats.strength / 3);
      const tempStrengthBonus = Math.floor(state.player.tempStrength / 3);
      const baseDamage = 7 + strengthBonus + tempStrengthBonus + ((cardInstance?.level || 1) - 1); // +1 per level
      const weaponDmg = getWeaponAndElementalDamage({ tags: ['WeaponAttack'] }, state.player.stats);
      const totalPhysical = baseDamage + weaponDmg.physical;
              const { damage: finalDamage, isCrit } = calculateCritDamage(totalPhysical, state.player.stats.criticalChance || 0, state.player.stats.criticalMultiplier || 150);
      // Apply damage to all enemies
      const newEnemies = state.enemies.map((enemy: Enemy) => {
        const dmg = applyDamageModifiers(finalDamage, state, withInstanceId(cleave), enemy);
        const physicalResist = Math.min(enemy.resistances.Physical, 75);
        const reductionMultiplier = (100 - physicalResist) / 100;
        const dmgAfterReduction = roundDamage(dmg * reductionMultiplier);
        let updatedEnemy = applyDamageToEnemy(enemy, dmgAfterReduction, state.turn, 'physical', 30, undefined, undefined, ['WeaponAttack', 'Physical']);
        updatedEnemy = applyAilments(updatedEnemy, dmgAfterReduction, cardInstance);
        return updatedEnemy;
      });
      return {
        ...state,
        enemies: newEnemies,
      };
    },
    comboEffect: function(this: EnhancedCard, state: CombatState): CombatState {
      let newState = this.effect ? this.effect(state) : state;
      if (state.lastPlayedCard?.type === 'Guard') {
        newState = {
          ...newState,
          enemies: newState.enemies.map((enemy: Enemy) => ({
            ...enemy,
            status: [
              ...enemy.status,
              {
                id: 'vulnerability_' + Math.random().toString(36).substr(2, 9),
                name: 'Vulnerability',
                description: 'Take 50% more damage from attacks',
                duration: 2,
                type: 'debuff',
                value: 1,
                effect: (state: CombatState) => state
              },
            ],
          })),
        };
      }
      return newState;
    },
    baseEffect: (state: CombatState, _target?: Enemy, cardInstance?: EnhancedCard): CombatState => state, // fallback
    upgrades: 0,
    retainOnTurn: false,
    recycleOnUse: false,
    exhaustOnUse: false,
    drawOnPlay: 0,
    energyRefund: 0,
    embossingSlots: 0,
    activeEmbossings: [],
    experience: 0,
    level: 1,
    maxLevel: 20,
    modifiers: [],
    rarity: 'Normal',
    groupKey: 'cleave'
  };

  // Endure Card (2 copies)
  const endure: Omit<EnhancedCard, 'instanceId'> = {
    id: 'marauder-endure',
    name: 'Endure',
    type: 'Guard',
    cost: 1,
    tags: ['Guard','Strength','Combo'],
    description: 'Gain 8 Guard (+ Strength/4). Draw a card.',
    baseDescription: 'Gain 8 Guard (+ Strength/4). Draw a card.',
    comboDescription: 'After using an Attack card, gain +3 Guard.',
    requirements: {
      strength: 22
    },
    comboWith: ['Attack'],
    comboHighlightType: 'type',
    comboEffect: (state: CombatState): CombatState => {
      if (state.lastPlayedCard?.type === 'Attack') {
        return {
          ...state,
          player: {
            ...state.player,
            guard: state.player.guard + 3,
          },
        };
      }
      return state;
    },
    effect: (state: CombatState, _target?: Enemy, cardInstance?: EnhancedCard) => {
      const strengthBonus = Math.floor(state.player.stats.strength / 4);
      const tempStrengthBonus = Math.floor(state.player.tempStrength / 4);
      const guardAmount = 8 + strengthBonus + tempStrengthBonus + ((cardInstance?.level || 1) - 1); // +1 per level
      return {
        ...state,
        player: {
          ...state.player,
          guard: state.player.guard + guardAmount,
        },
      };
    },
    baseEffect: (state: CombatState): CombatState => state, // fallback
    upgrades: 0,
    retainOnTurn: false,
    recycleOnUse: false,
    exhaustOnUse: false,
    drawOnPlay: 1,
    energyRefund: 0,
    embossingSlots: 0,
    activeEmbossings: [],
    experience: 0,
    level: 1,
    maxLevel: 20,
    modifiers: [],
    rarity: 'Normal',
    groupKey: 'marauder-endure'
  };

  return {
    basicAttacks: Array(6).fill(null).map(() => createCardInstance(basicAttack)),
    basicDefenses: Array(4).fill(null).map(() => createCardInstance(basicDefense)),
    groundSlams: Array(2).fill(null).map(() => createCardInstance(groundSlam)),
    intimidatingShouts: Array(2).fill(null).map(() => createCardInstance(intimidatingShout)),
    cleaves: Array(2).fill(null).map(() => createCardInstance(cleave)),
    endures: Array(2).fill(null).map(() => createCardInstance(endure)),
  };
}

export function getStarterEquipmentForClass(characterClass: CharacterClass): GearItem[] {
  switch (characterClass) {
    case 'Marauder':
      return [createGearItem(EQUIPMENT_BASES['wooden_staff'], 'Normal', 'Normal', 1)];
    case 'Ranger':
      return [createGearItem(EQUIPMENT_BASES['hunter_bow'], 'Normal', 'Normal', 1)];
    // ... other classes ...
    default:
      return [];
  }
}

export { chainbreakerCard, wallSlamCard, sirenSongCard, queensWrathCard } from './BossCards';
