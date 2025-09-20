import type { EnhancedCard } from '../../types/cards/enhancedCards';
import type { CombatState } from '../../types/combat/combat';
import type { Enemy } from '../../types/combat/enemies';
import { getSeparatedWeaponDamage, applySeparatedWeaponDamageModifiers } from '../../utils/damage';
import { v4 as uuidv4 } from 'uuid';
import { EQUIPMENT_BASES } from '../../types/loot/equipment';
import { createGearItem } from '../../utils/itemGeneration';
import type { GearItem } from '../../types/loot/loot';
import type { CharacterClass } from '../../types/combat/character';
import { applyBleed, applyPoison, applyIgnite } from '../../utils/statusEffects';
import { applyAilments } from '../../utils/ailments';
import { calculateCritDamage } from '../../utils/crit';

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

export function createThiefStarterDeck() {
  // Basic Attack - Twin Strike (6 copies)
  const twinStrike: Omit<EnhancedCard, 'instanceId'> = {
    id: 'thief-twin-strike',
    name: 'Twin Strike',
    type: 'Attack',
    cost: 1,
    tags: ['WeaponAttack','Attack','Physical','Dual'],
    description: 'Deal 5 physical damage (+Dex/4).',
    baseDescription: 'Deal 5 physical damage (+Dex/4).',
    ifDualWieldingDescription: 'Deal 1(+Dex/4) off-hand damage to all enemies aswell.',
    requirements: {
      dexterity: 20
    },
    comboWith: ['Guard'],
    comboHighlightType: 'type',
    effect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard) => {
      if (!_target) return state;
      
      const dexterity = state.player.stats.dexterity || 0;
      const isDualWieldingActive = state.player.stats.weaponTypes?.includes('dual_wield') || false;
      
      // Get separated weapon damage
      const separatedDamage = getSeparatedWeaponDamage(state.player.stats);
      const modifiedDamage = applySeparatedWeaponDamageModifiers(separatedDamage, state.player.stats, cardInstance.tags);
      
      // Calculate base damage from card + main hand weapon
      const baseDamage = 5 + Math.floor(dexterity / 4);
      const mainHandDamage = baseDamage + Math.floor(modifiedDamage.mainHand.physical);
      const offHandDamage = isDualWieldingActive ? Math.floor(modifiedDamage.offHand.physical) : 0;
      
      // Deal main hand damage to target
      let updatedEnemy = {
        ..._target,
        hp: Math.max(0, _target.hp - mainHandDamage)
      };
      
      let newEnemies = state.enemies.map(e => e.id === _target.id ? updatedEnemy : e);
      
      // If dual wielding, deal off-hand damage to all enemies
      if (isDualWieldingActive && offHandDamage > 0) {
        newEnemies = newEnemies.map(enemy => ({
          ...enemy,
          hp: Math.max(0, enemy.hp - offHandDamage)
        }));
      }
      
      return { ...state, enemies: newEnemies };
    },
    baseEffect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard) => {
      if (!_target) return state;
      
      const dexterity = state.player.stats.dexterity || 0;
      const isDualWieldingActive = state.player.stats.weaponTypes?.includes('dual_wield') || false;
      
      // Get separated weapon damage
      const separatedDamage = getSeparatedWeaponDamage(state.player.stats);
      const modifiedDamage = applySeparatedWeaponDamageModifiers(separatedDamage, state.player.stats, cardInstance.tags);
      
      // Calculate base damage from card + main hand weapon
      const baseDamage = 5 + Math.floor(dexterity / 4);
      const mainHandDamage = baseDamage + Math.floor(modifiedDamage.mainHand.physical);
      const offHandDamage = isDualWieldingActive ? Math.floor(modifiedDamage.offHand.physical) : 0;
      
      // Deal main hand damage to target
      let updatedEnemy = {
        ..._target,
        hp: Math.max(0, _target.hp - mainHandDamage)
      };
      
      let newEnemies = state.enemies.map(e => e.id === _target.id ? updatedEnemy : e);
      
      // If dual wielding, deal off-hand damage to all enemies
      if (isDualWieldingActive && offHandDamage > 0) {
        newEnemies = newEnemies.map(enemy => ({
          ...enemy,
          hp: Math.max(0, enemy.hp - offHandDamage)
        }));
      }
      
      return { ...state, enemies: newEnemies };
    },
    comboEffect: (state: CombatState): CombatState => {
      // If the last played card was a Guard, the damage bonus is already applied in the main effect
      return state;
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
    groupKey: 'thief-twin-strike'
  };

  // Basic Defense - Shadow Step (4 copies)
  const shadowStep: Omit<EnhancedCard, 'instanceId'> = {
    id: 'thief-shadow-step',
    name: 'Shadow Step',
    type: 'Guard',
    cost: 1,
    tags: ['Guard','Stealth','Dexterity'],
    description: 'Gain 4 Guard (+Dex/6).',
    baseDescription: 'Gain 4 Guard (+Dex/6).',
    ifDualWieldingDescription: 'Gain 6 Guard (+Dex/6) and 250(+Dex*2) evasion until your next turn.',
    requirements: {
      dexterity: 18
    },
    comboWith: ['Attack'],
    comboHighlightType: 'type',
    effect: (state: CombatState, _target?: Enemy, cardInstance?: EnhancedCard) => {
      const dexterity = state.player.stats.dexterity || 0;
      const isDualWieldingActive = state.player.stats.weaponTypes?.includes('dual_wield') || false;
      const baseGuard = isDualWieldingActive ? 6 : 4;
      const guardAmount = baseGuard + Math.floor(dexterity / 6);
      
      let newState = {
        ...state,
        player: {
          ...state.player,
          guard: (state.player.guard || 0) + guardAmount,
        },
      };
      
      // If dual wielding, add temporary evasion buff
      if (isDualWieldingActive) {
        const evasionAmount = 250 + (dexterity * 2);
        newState.player.status = [
          ...(newState.player.status || []),
          {
            id: 'shadow_step_evasion',
            name: 'Shadow Step Evasion',
            description: `+${evasionAmount} evasion until next turn`,
            duration: 1, // Until next turn
            type: 'buff',
            effect: (state: CombatState) => state,
            appliedOnTurn: state.turn
          }
        ];
      }
      
      return newState;
    },
    baseEffect: (state: CombatState, _target?: Enemy, cardInstance?: EnhancedCard) => state,
    comboEffect: (state: CombatState): CombatState => {
      if (state.lastPlayedCard?.type === 'Attack') {
        return {
          ...state,
          player: {
            ...state.player,
            guard: (state.player.guard || 0) + 1,
          },
        };
      }
      return state;
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
    groupKey: 'thief-shadow-step'
  };

  // Fan of Blades Card (3 copies)
  const fanOfBlades: Omit<EnhancedCard, 'instanceId'> = {
    id: 'thief-fan-of-blades',
    name: 'Fan of Blades',
    type: 'Attack',
    cost: 2,
    tags: ['WeaponAttack','Attack','Physical','Dual','AoE'],
    description: 'Deal 6 physical damage to all enemies (+Dex/5).',
    baseDescription: 'Deal 6 physical damage to all enemies (+Dex/5).',
    ifDualWieldingDescription: 'Attacks twice.',
    requirements: {
      dexterity: 23
    },
    comboWith: ['DualWield'],
    comboHighlightType: 'type',
    effect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard) => {
      if (!_target) return state;
      
      const dexterity = state.player.stats.dexterity || 0;
      const isDualWieldingActive = state.player.stats.weaponTypes?.includes('dual_wield') || false;
      
      // Get separated weapon damage
      const separatedDamage = getSeparatedWeaponDamage(state.player.stats);
      const modifiedDamage = applySeparatedWeaponDamageModifiers(separatedDamage, state.player.stats, cardInstance.tags);
      
      // Calculate base damage from card + main hand weapon
      const baseDamage = 6 + Math.floor(dexterity / 5);
      const mainHandDamage = baseDamage + Math.floor(modifiedDamage.mainHand.physical);
      const offHandDamage = isDualWieldingActive ? Math.floor(modifiedDamage.offHand.physical) : 0;
      
      // Deal damage to all enemies
      let newEnemies = state.enemies.map(enemy => {
        let totalDamage = mainHandDamage;
        if (isDualWieldingActive && offHandDamage > 0) {
          totalDamage += offHandDamage; // Add off-hand damage for dual-wielding
        }
        return {
          ...enemy,
          hp: Math.max(0, enemy.hp - totalDamage)
        };
      });
      
      return { ...state, enemies: newEnemies };
    },
    baseEffect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard) => {
      if (!_target) return state;
      
      const dexterity = state.player.stats.dexterity || 0;
      const isDualWieldingActive = state.player.stats.weaponTypes?.includes('dual_wield') || false;
      
      // Get separated weapon damage
      const separatedDamage = getSeparatedWeaponDamage(state.player.stats);
      const modifiedDamage = applySeparatedWeaponDamageModifiers(separatedDamage, state.player.stats, cardInstance.tags);
      
      // Calculate base damage from card + main hand weapon
      const baseDamage = 6 + Math.floor(dexterity / 5);
      const mainHandDamage = baseDamage + Math.floor(modifiedDamage.mainHand.physical);
      const offHandDamage = isDualWieldingActive ? Math.floor(modifiedDamage.offHand.physical) : 0;
      
      // Deal damage to all enemies
      let newEnemies = state.enemies.map(enemy => {
        let totalDamage = mainHandDamage;
        if (isDualWieldingActive && offHandDamage > 0) {
          totalDamage += offHandDamage; // Add off-hand damage for dual-wielding
        }
        return {
          ...enemy,
          hp: Math.max(0, enemy.hp - totalDamage)
        };
      });
      
      return { ...state, enemies: newEnemies };
    },
    comboEffect: (state: CombatState): CombatState => {
      // The dual wield bonus is already applied in the main effect
      return state;
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
    groupKey: 'thief-fan-of-blades'
  };

  // Evasive Maneuver Card (3 copies)
  const evasiveManeuver: Omit<EnhancedCard, 'instanceId'> = {
    id: 'thief-evasive-maneuver',
    name: 'Evasive Maneuver',
    type: 'Guard',
    cost: 1,
    tags: ['Guard','Stealth','Dexterity'],
    description: 'Gain 5 Guard (+Dex/5).',
    baseDescription: 'Gain 5 Guard (+Dex/5).',
    ifDualWieldingDescription: 'Gain 7 Guard (+Dex/5) and 50(+Dex*2) evasion for the remainder of combat.',
    requirements: {
      dexterity: 22
    },
    comboWith: ['Attack'],
    comboHighlightType: 'type',
    effect: (state: CombatState, _target?: Enemy, cardInstance?: EnhancedCard) => {
      const dexterity = state.player.stats.dexterity || 0;
      const isDualWieldingActive = state.player.stats.weaponTypes?.includes('dual_wield') || false;
      const baseGuard = isDualWieldingActive ? 7 : 5;
      const guardAmount = baseGuard + Math.floor(dexterity / 5);
      
      let newState = {
        ...state,
        player: {
          ...state.player,
          guard: (state.player.guard || 0) + guardAmount,
        },
      };
      
      // If dual wielding, add permanent evasion buff
      if (isDualWieldingActive) {
        const evasionAmount = 50 + (dexterity * 2);
        newState.player.status = [
          ...(newState.player.status || []),
          {
            id: 'evasive_maneuver_evasion',
            name: 'Evasive Maneuver Evasion',
            description: `+${evasionAmount} evasion for the remainder of combat`,
            duration: 999, // Permanent for the combat
            type: 'buff',
            effect: (state: CombatState) => state,
            appliedOnTurn: state.turn
          }
        ];
      }
      
      return newState;
    },
    baseEffect: (state: CombatState, _target?: Enemy, cardInstance?: EnhancedCard) => state,
    comboEffect: (state: CombatState): CombatState => {
      if (state.lastPlayedCard?.type === 'Attack') {
        return {
          ...state,
          player: {
            ...state.player,
            guard: (state.player.guard || 0) + 2,
          },
        };
      }
      return state;
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
    groupKey: 'thief-evasive-maneuver'
  };

  // Silent Preparation Card (2 copies)
  const silentPreparation: Omit<EnhancedCard, 'instanceId'> = {
    id: 'thief-silent-preparation',
    name: 'Silent Preparation',
    type: 'Skill',
    cost: 0,
    tags: ['Skill','Stealth','Dexterity'],
    description: 'Gain 1 energy.',
    baseDescription: 'Gain 1 energy.',
    ifDualWieldingDescription: 'Gain 1 energy and draw a card aswell.',
    requirements: {
      dexterity: 20
    },
    comboWith: ['Guard'],
    comboHighlightType: 'type',
    effect: (state: CombatState, _target?: Enemy, cardInstance?: EnhancedCard) => {
      const dexterity = state.player.stats.dexterity || 0;
      const isDualWieldingActive = state.player.stats.weaponTypes?.includes('dual_wield') || false;
      const energyGain = dexterity >= 7 ? 2 : 1;
      
      let newState = {
        ...state,
        player: {
          ...state.player,
          mana: (state.player.mana || 0) + energyGain,
        },
      };
      
      // If dual wielding, draw a card
      if (isDualWieldingActive) {
        // Draw a card from deck to hand
        if (newState.player.deck.length > 0) {
          const drawnCard = newState.player.deck[0];
          newState.player.deck = newState.player.deck.slice(1);
          newState.player.hand = [...newState.player.hand, drawnCard];
        }
      }
      
      return newState;
    },
    baseEffect: (state: CombatState, _target?: Enemy, cardInstance?: EnhancedCard) => state,
    comboEffect: (state: CombatState): CombatState => {
      if (state.lastPlayedCard?.type === 'Guard') {
        return {
          ...state,
          player: {
            ...state.player,
            mana: (state.player.mana || 0) + 1,
          },
        };
      }
      return state;
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
    groupKey: 'thief-silent-preparation'
  };

  // Concealed Blade Card (2 copies)
  const concealedBlade: Omit<EnhancedCard, 'instanceId'> = {
    id: 'thief-concealed-blade',
    name: 'Concealed Blade',
    type: 'Attack',
    cost: 1,
    tags: ['Attack','Physical','Stealth','Dexterity','Dual'],
    description: 'Deal 8 physical damage (+Dex/3).',
    baseDescription: 'Deal 8 physical damage (+Dex/3)',
    ifDualWieldingDescription: 'Deal 8(+Dex/3) damage and hold your off-hand damage for the next attack.',
    requirements: {
      dexterity: 23
    },
    comboWith: ['Guard'],
    comboHighlightType: 'type',
    effect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard) => {
      if (!_target) return state;
      
      const dexterity = state.player.stats.dexterity || 0;
      const isDualWieldingActive = state.player.stats.weaponTypes?.includes('dual_wield') || false;
      
      // Get separated weapon damage
      const separatedDamage = getSeparatedWeaponDamage(state.player.stats);
      const modifiedDamage = applySeparatedWeaponDamageModifiers(separatedDamage, state.player.stats, cardInstance.tags);
      
      // Calculate base damage from card + main hand weapon
      const baseDamage = dexterity >= 9 ? 10 : 8;
      const totalDamage = baseDamage + Math.floor(dexterity / 3) + Math.floor(modifiedDamage.mainHand.physical);
      
      let updatedEnemy = {
        ..._target,
        hp: Math.max(0, _target.hp - totalDamage)
      };
      
      let newState = {
        ...state,
        enemies: state.enemies.map(e => e.id === _target.id ? updatedEnemy : e)
      };
      
      // If dual wielding, store off-hand damage for next attack
      if (isDualWieldingActive) {
        const offHandDamage = Math.floor(modifiedDamage.offHand.physical);
        if (offHandDamage > 0) {
          newState.player.status = [
            ...(newState.player.status || []),
            {
              id: 'concealed_blade_stored_damage',
              name: 'Stored Off-Hand Damage',
              description: `Next attack deals +${offHandDamage} damage`,
              duration: 1, // Until next attack
              type: 'buff',
              effect: (state: CombatState) => state,
              appliedOnTurn: state.turn,
              value: offHandDamage // Store the damage amount
            }
          ];
        }
      }
      
      return newState;
    },
    baseEffect: (state: CombatState, _target: Enemy, cardInstance: EnhancedCard) => {
      if (!_target) return state;
      
      const dexterity = state.player.stats.dexterity || 0;
      const isDualWieldingActive = state.player.stats.weaponTypes?.includes('dual_wield') || false;
      
      // Get separated weapon damage
      const separatedDamage = getSeparatedWeaponDamage(state.player.stats);
      const modifiedDamage = applySeparatedWeaponDamageModifiers(separatedDamage, state.player.stats, cardInstance.tags);
      
      // Calculate base damage from card + main hand weapon
      const baseDamage = dexterity >= 9 ? 10 : 8;
      const totalDamage = baseDamage + Math.floor(dexterity / 3) + Math.floor(modifiedDamage.mainHand.physical);
      
      let updatedEnemy = {
        ..._target,
        hp: Math.max(0, _target.hp - totalDamage)
      };
      
      let newState = {
        ...state,
        enemies: state.enemies.map(e => e.id === _target.id ? updatedEnemy : e)
      };
      
      // If dual wielding, store off-hand damage for next attack
      if (isDualWieldingActive) {
        const offHandDamage = Math.floor(modifiedDamage.offHand.physical);
        if (offHandDamage > 0) {
          newState.player.status = [
            ...(newState.player.status || []),
            {
              id: 'concealed_blade_stored_damage',
              name: 'Stored Off-Hand Damage',
              description: `Next attack deals +${offHandDamage} damage`,
              duration: 1, // Until next attack
              type: 'buff',
              effect: (state: CombatState) => state,
              appliedOnTurn: state.turn,
              value: offHandDamage // Store the damage amount
            }
          ];
        }
      }
      
      return newState;
    },
    comboEffect: (state: CombatState): CombatState => {
      if (state.lastPlayedCard?.type === 'Guard') {
        // The damage bonus is already applied in the main effect
        return state;
      }
      return state;
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
    groupKey: 'thief-concealed-blade'
  };

  return {
    twinStrikes: Array(6).fill(null).map(() => createCardInstance(twinStrike)),
    shadowSteps: Array(4).fill(null).map(() => createCardInstance(shadowStep)),
    fanOfBlades: Array(3).fill(null).map(() => createCardInstance(fanOfBlades)),
    evasiveManeuvers: Array(3).fill(null).map(() => createCardInstance(evasiveManeuver)),
    silentPreparations: Array(2).fill(null).map(() => createCardInstance(silentPreparation)),
    concealedBlades: Array(2).fill(null).map(() => createCardInstance(concealedBlade)),
  };
}

export function getStarterEquipmentForClass(characterClass: CharacterClass): GearItem[] {
  if (characterClass !== 'Thief') return [];

  const weapon = createGearItem(EQUIPMENT_BASES['steel_dagger'], 'Normal', 'Normal', 1);
  weapon.slot = 'Weapon';

  const offhand = createGearItem(EQUIPMENT_BASES['steel_dagger'], 'Normal', 'Normal', 1);
  offhand.slot = 'Offhand';

  return [weapon, offhand];
} 