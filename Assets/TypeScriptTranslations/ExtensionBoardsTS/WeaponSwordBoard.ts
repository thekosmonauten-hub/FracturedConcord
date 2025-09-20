import { PassiveBoard } from '../../types/passiveTree';

// Weapon-specific board: Swords (Physical)
export const WEAPON_SWORD_BOARD: PassiveBoard = {
  id: 'weapon_sword_board',
  name: 'Blademaster',
  description: 'Specialize in swords: precision, speed and physical prowess.',
  theme: 'physical',
  size: { rows: 7, columns: 7 },
  nodes: [
    // Row 0
    [
      { id: 'sword_0_0', name: '', description: '', position: { row: 0, column: 0 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_0_1', name: '', description: '', position: { row: 0, column: 1 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_0_2', name: '', description: '', position: { row: 0, column: 2 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_ext_top', name: 'Extension Point', description: 'Connect to another board', position: { row: 0, column: 3 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'sword_0_4', name: '', description: '', position: { row: 0, column: 4 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_0_5', name: '', description: '', position: { row: 0, column: 5 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_0_6', name: '', description: '', position: { row: 0, column: 6 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    // Row 1 (notables at 1,1 and 1,5)
    [
      { id: 'sword_1_0', name: '', description: '', position: { row: 1, column: 0 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_notable_1', name: 'Edge of Precision', description: '+24% Sword Damage, +100 Accuracy, +5 Dexterity', position: { row: 1, column: 1 }, type: 'notable', stats: { increasedSwordDamage: 24, accuracy: 100, dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_1_2', name: '', description: '', position: { row: 1, column: 2 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_1_3', name: '', description: '', position: { row: 1, column: 3 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_1_4', name: '', description: '', position: { row: 1, column: 4 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_notable_2', name: 'Quicksteel', description: '+24% Sword Damage, +10% Draw Speed, +5 Dexterity', position: { row: 1, column: 5 }, type: 'notable', stats: { increasedSwordDamage: 24, attackSpeed: 10, dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_1_6', name: '', description: '', position: { row: 1, column: 6 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    // Row 2
    [
      { id: 'sword_2_0', name: '', description: '', position: { row: 2, column: 0 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_2_1', name: '', description: '', position: { row: 2, column: 1 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_2_2', name: '', description: '', position: { row: 2, column: 2 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_2_3', name: '', description: '', position: { row: 2, column: 3 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_2_4', name: '', description: '', position: { row: 2, column: 4 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_2_5', name: '', description: '', position: { row: 2, column: 5 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_2_6', name: '', description: '', position: { row: 2, column: 6 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    // Row 3 (midline with extensions left/right)
    [
      { id: 'sword_ext_left', name: 'Extension Point', description: 'Connect to another board', position: { row: 3, column: 0 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'sword_3_1', name: '', description: '', position: { row: 3, column: 1 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_3_2', name: '', description: '', position: { row: 3, column: 2 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_3_3', name: '', description: '', position: { row: 3, column: 3 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_3_4', name: '', description: '', position: { row: 3, column: 4 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_3_5', name: '', description: '', position: { row: 3, column: 5 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_ext_right', name: 'Extension Point', description: 'Connect to another board', position: { row: 3, column: 6 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] }
    ],
    // Row 4
    [
      { id: 'sword_4_0', name: '', description: '', position: { row: 4, column: 0 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_4_1', name: '', description: '', position: { row: 4, column: 1 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_4_2', name: '', description: '', position: { row: 4, column: 2 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_4_3', name: '', description: '', position: { row: 4, column: 3 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_4_4', name: '', description: '', position: { row: 4, column: 4 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_4_5', name: '', description: '', position: { row: 4, column: 5 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_4_6', name: '', description: '', position: { row: 4, column: 6 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    // Row 5 (notables at 5,1 and 5,5)
    [
      { id: 'sword_5_0', name: '', description: '', position: { row: 5, column: 0 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_notable_3', name: 'Razor Dance', description: '+24% Sword Damage, +8% Critical Chance', position: { row: 5, column: 1 }, type: 'notable', stats: { increasedSwordDamage: 24, criticalChance: 8 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_5_2', name: '', description: '', position: { row: 5, column: 2 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_5_3', name: '', description: '', position: { row: 5, column: 3 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_5_4', name: '', description: '', position: { row: 5, column: 4 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_notable_4', name: 'Steel Tempest', description: '+24% Sword Damage, +12% Melee Damage', position: { row: 5, column: 5 }, type: 'notable', stats: { increasedSwordDamage: 24, increasedMeleeDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_5_6', name: '', description: '', position: { row: 5, column: 6 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    // Row 6
    [
      { id: 'sword_6_0', name: '', description: '', position: { row: 6, column: 0 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_6_1', name: '', description: '', position: { row: 6, column: 1 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_6_2', name: '', description: '', position: { row: 6, column: 2 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_ext_bottom', name: 'Extension Point', description: 'Connect to another board', position: { row: 6, column: 3 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'sword_6_4', name: '', description: '', position: { row: 6, column: 4 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_6_5', name: '', description: '', position: { row: 6, column: 5 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'sword_6_6', name: '', description: '', position: { row: 6, column: 6 }, type: 'small', stats: { increasedSwordDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ]
  ],
  extensionPoints: [
    { id: 'sword_ext_top', position: { row: 0, column: 3 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'sword_ext_left', position: { row: 3, column: 0 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'sword_ext_right', position: { row: 3, column: 6 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'sword_ext_bottom', position: { row: 6, column: 3 }, availableBoards: [], maxConnections: 1, currentConnections: 0 }
  ],
  maxPoints: 15
} as unknown as PassiveBoard;


