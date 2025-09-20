import { PassiveBoard } from '../../types/passiveTree';

export const LIFE_DEFENSE_BOARD: PassiveBoard = {
  id: 'life_defense_board',
  name: 'Bulwark of Vitality',
  description: 'Maximum Life, Armor, Evasion, and defensive utility.',
  theme: 'life',
  size: { rows: 7, columns: 7 },
  nodes: [
    [
      { id: 'life_0_0', name: '', description: '', position: { row: 0, column: 0 }, type: 'small', stats: { maxHealthIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_0_1', name: '', description: '', position: { row: 0, column: 1 }, type: 'small', stats: { armor: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_0_2', name: '', description: '', position: { row: 0, column: 2 }, type: 'small', stats: { evasion: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_ext_top', name: 'Extension Point', description: 'Connect to another board', position: { row: 0, column: 3 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'life_0_4', name: '', description: '', position: { row: 0, column: 4 }, type: 'small', stats: { maxHealthIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_0_5', name: '', description: '', position: { row: 0, column: 5 }, type: 'small', stats: { armor: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_0_6', name: '', description: '', position: { row: 0, column: 6 }, type: 'small', stats: { evasion: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'life_1_0', name: '', description: '', position: { row: 1, column: 0 }, type: 'small', stats: { maxHealthIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_notable_1', name: 'Hearthguard', description: '+8% Max Life, +200 Armor', position: { row: 1, column: 1 }, type: 'notable', stats: { maxHealthIncrease: 8, armor: 200 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_1_2', name: '', description: '', position: { row: 1, column: 2 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_1_3', name: '', description: '', position: { row: 1, column: 3 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_1_4', name: '', description: '', position: { row: 1, column: 4 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_notable_2', name: 'Bulwark', description: '+8% Max Life, +5% Block Chance', position: { row: 1, column: 5 }, type: 'notable', stats: { maxHealthIncrease: 8, blockChance: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_1_6', name: '', description: '', position: { row: 1, column: 6 }, type: 'small', stats: { evasion: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'life_2_0', name: '', description: '', position: { row: 2, column: 0 }, type: 'small', stats: { armor: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_2_1', name: '', description: '', position: { row: 2, column: 1 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_2_2', name: '', description: '', position: { row: 2, column: 2 }, type: 'small', stats: { maxHealthIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_2_3', name: '', description: '', position: { row: 2, column: 3 }, type: 'small', stats: { armor: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_2_4', name: '', description: '', position: { row: 2, column: 4 }, type: 'small', stats: { evasion: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_2_5', name: '', description: '', position: { row: 2, column: 5 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_2_6', name: '', description: '', position: { row: 2, column: 6 }, type: 'small', stats: { maxHealthIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'life_ext_left', name: 'Extension Point', description: 'Connect to another board', position: { row: 3, column: 0 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'life_3_1', name: '', description: '', position: { row: 3, column: 1 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_3_2', name: '', description: '', position: { row: 3, column: 2 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_3_3', name: '', description: '', position: { row: 3, column: 3 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_3_4', name: '', description: '', position: { row: 3, column: 4 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_3_5', name: '', description: '', position: { row: 3, column: 5 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_ext_right', name: 'Extension Point', description: 'Connect to another board', position: { row: 3, column: 6 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] }
    ],
    [
      { id: 'life_4_0', name: '', description: '', position: { row: 4, column: 0 }, type: 'small', stats: { evasion: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_4_1', name: '', description: '', position: { row: 4, column: 1 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_4_2', name: '', description: '', position: { row: 4, column: 2 }, type: 'small', stats: { armor: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_4_3', name: '', description: '', position: { row: 4, column: 3 }, type: 'small', stats: { maxHealthIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_4_4', name: '', description: '', position: { row: 4, column: 4 }, type: 'small', stats: { armor: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_4_5', name: '', description: '', position: { row: 4, column: 5 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_4_6', name: '', description: '', position: { row: 4, column: 6 }, type: 'small', stats: { evasion: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'life_5_0', name: '', description: '', position: { row: 5, column: 0 }, type: 'small', stats: { maxHealthIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_notable_3', name: 'Stonewall', description: '+8% Max Life, +12% Melee Damage', position: { row: 5, column: 1 }, type: 'notable', stats: { maxHealthIncrease: 8, increasedMeleeDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_5_2', name: '', description: '', position: { row: 5, column: 2 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_5_3', name: '', description: '', position: { row: 5, column: 3 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_5_4', name: '', description: '', position: { row: 5, column: 4 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_notable_4', name: 'Defiance', description: '+8% Max Life, +8% Elemental Resist', position: { row: 5, column: 5 }, type: 'notable', stats: { maxHealthIncrease: 8, elementalResist: 8 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_5_6', name: '', description: '', position: { row: 5, column: 6 }, type: 'small', stats: { armor: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'life_6_0', name: '', description: '', position: { row: 6, column: 0 }, type: 'small', stats: { evasion: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_6_1', name: '', description: '', position: { row: 6, column: 1 }, type: 'small', stats: { armor: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_6_2', name: '', description: '', position: { row: 6, column: 2 }, type: 'small', stats: { maxHealthIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_ext_bottom', name: 'Extension Point', description: 'Connect to another board', position: { row: 6, column: 3 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'life_6_4', name: '', description: '', position: { row: 6, column: 4 }, type: 'small', stats: { evasion: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_6_5', name: '', description: '', position: { row: 6, column: 5 }, type: 'small', stats: { armor: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'life_6_6', name: '', description: '', position: { row: 6, column: 6 }, type: 'small', stats: { maxHealthIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ]
  ],
  extensionPoints: [
    { id: 'life_ext_top', position: { row: 0, column: 3 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'life_ext_left', position: { row: 3, column: 0 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'life_ext_right', position: { row: 3, column: 6 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'life_ext_bottom', position: { row: 6, column: 3 }, availableBoards: [], maxConnections: 1, currentConnections: 0 }
  ],
  maxPoints: 15
} as unknown as PassiveBoard;


