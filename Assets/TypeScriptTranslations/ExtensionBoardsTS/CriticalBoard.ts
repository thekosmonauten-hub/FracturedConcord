import { PassiveBoard } from '../../types/passiveTree';

export const CRITICAL_BOARD: PassiveBoard = {
  id: 'critical_board',
  name: 'Assassin’s Guile',
  description: 'Critical chance and multiplier with utility hooks.',
  theme: 'critical',
  size: { rows: 7, columns: 7 },
  nodes: [
    [
      { id: 'crit_0_0', name: '', description: '', position: { row: 0, column: 0 }, type: 'small', stats: { criticalChance: 3 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_0_1', name: '', description: '', position: { row: 0, column: 1 }, type: 'small', stats: { criticalMultiplier: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_0_2', name: '', description: '', position: { row: 0, column: 2 }, type: 'small', stats: { increasedAccuracy: 8 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_ext_top', name: 'Extension Point', description: 'Connect to another board', position: { row: 0, column: 3 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'crit_0_4', name: '', description: '', position: { row: 0, column: 4 }, type: 'small', stats: { criticalChance: 3 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_0_5', name: '', description: '', position: { row: 0, column: 5 }, type: 'small', stats: { criticalMultiplier: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_0_6', name: '', description: '', position: { row: 0, column: 6 }, type: 'small', stats: { increasedAccuracy: 8 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'crit_1_0', name: '', description: '', position: { row: 1, column: 0 }, type: 'small', stats: { criticalChance: 3 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_notable_1', name: 'Opportune Strike', description: '+8% Crit Chance, +12% Crit Multiplier', position: { row: 1, column: 1 }, type: 'notable', stats: { criticalChance: 8, criticalMultiplier: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_1_2', name: '', description: '', position: { row: 1, column: 2 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_1_3', name: '', description: '', position: { row: 1, column: 3 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_1_4', name: '', description: '', position: { row: 1, column: 4 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_notable_2', name: 'Deadly Precision', description: '+8% Crit Chance, +12% Accuracy', position: { row: 1, column: 5 }, type: 'notable', stats: { criticalChance: 8, increasedAccuracy: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_1_6', name: '', description: '', position: { row: 1, column: 6 }, type: 'small', stats: { criticalMultiplier: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'crit_2_0', name: '', description: '', position: { row: 2, column: 0 }, type: 'small', stats: { increasedAccuracy: 8 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_2_1', name: '', description: '', position: { row: 2, column: 1 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_2_2', name: '', description: '', position: { row: 2, column: 2 }, type: 'small', stats: { criticalChance: 3 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_2_3', name: '', description: '', position: { row: 2, column: 3 }, type: 'small', stats: { criticalMultiplier: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_2_4', name: '', description: '', position: { row: 2, column: 4 }, type: 'small', stats: { increasedAccuracy: 8 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_2_5', name: '', description: '', position: { row: 2, column: 5 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_2_6', name: '', description: '', position: { row: 2, column: 6 }, type: 'small', stats: { criticalChance: 3 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'crit_ext_left', name: 'Extension Point', description: 'Connect to another board', position: { row: 3, column: 0 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'crit_3_1', name: '', description: '', position: { row: 3, column: 1 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_3_2', name: '', description: '', position: { row: 3, column: 2 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_3_3', name: '', description: '', position: { row: 3, column: 3 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_3_4', name: '', description: '', position: { row: 3, column: 4 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_3_5', name: '', description: '', position: { row: 3, column: 5 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_ext_right', name: 'Extension Point', description: 'Connect to another board', position: { row: 3, column: 6 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] }
    ],
    [
      { id: 'crit_4_0', name: '', description: '', position: { row: 4, column: 0 }, type: 'small', stats: { criticalMultiplier: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_4_1', name: '', description: '', position: { row: 4, column: 1 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_4_2', name: '', description: '', position: { row: 4, column: 2 }, type: 'small', stats: { increasedAccuracy: 8 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_4_3', name: '', description: '', position: { row: 4, column: 3 }, type: 'small', stats: { criticalChance: 3 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_4_4', name: '', description: '', position: { row: 4, column: 4 }, type: 'small', stats: { increasedAccuracy: 8 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_4_5', name: '', description: '', position: { row: 4, column: 5 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_4_6', name: '', description: '', position: { row: 4, column: 6 }, type: 'small', stats: { criticalMultiplier: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'crit_5_0', name: '', description: '', position: { row: 5, column: 0 }, type: 'small', stats: { increasedAccuracy: 8 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_notable_3', name: 'Execution', description: '+8% Crit Chance, +12% Melee Damage', position: { row: 5, column: 1 }, type: 'notable', stats: { criticalChance: 8, increasedMeleeDamage: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_5_2', name: '', description: '', position: { row: 5, column: 2 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_5_3', name: '', description: '', position: { row: 5, column: 3 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_5_4', name: '', description: '', position: { row: 5, column: 4 }, type: 'travel', stats: { dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_notable_4', name: 'Apex Strike', description: '+8% Crit Chance, +12% Crit Multiplier', position: { row: 5, column: 5 }, type: 'notable', stats: { criticalChance: 8, criticalMultiplier: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_5_6', name: '', description: '', position: { row: 5, column: 6 }, type: 'small', stats: { criticalChance: 3 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'crit_6_0', name: '', description: '', position: { row: 6, column: 0 }, type: 'small', stats: { criticalMultiplier: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_6_1', name: '', description: '', position: { row: 6, column: 1 }, type: 'small', stats: { increasedAccuracy: 8 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_6_2', name: '', description: '', position: { row: 6, column: 2 }, type: 'small', stats: { criticalChance: 3 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_ext_bottom', name: 'Extension Point', description: 'Connect to another board', position: { row: 6, column: 3 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'crit_6_4', name: '', description: '', position: { row: 6, column: 4 }, type: 'small', stats: { criticalMultiplier: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_6_5', name: '', description: '', position: { row: 6, column: 5 }, type: 'small', stats: { increasedAccuracy: 8 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'crit_6_6', name: '', description: '', position: { row: 6, column: 6 }, type: 'small', stats: { criticalChance: 3 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ]
  ],
  extensionPoints: [
    { id: 'crit_ext_top', position: { row: 0, column: 3 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'crit_ext_left', position: { row: 3, column: 0 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'crit_ext_right', position: { row: 3, column: 6 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'crit_ext_bottom', position: { row: 6, column: 3 }, availableBoards: [], maxConnections: 1, currentConnections: 0 }
  ],
  maxPoints: 15
} as unknown as PassiveBoard;


