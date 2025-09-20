import { PassiveBoard } from '../../types/passiveTree';

export const RELIANCE_BOARD: PassiveBoard = {
  id: 'reliance_board',
  name: 'Steadfast Reliance',
  description: 'Grow your Reliance meter and its cap; synergizes with defense and utility.',
  theme: 'utility',
  size: { rows: 7, columns: 7 },
  nodes: [
    [
      { id: 'rel_0_0', name: '', description: '', position: { row: 0, column: 0 }, type: 'small', stats: { maxRelianceIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_0_1', name: '', description: '', position: { row: 0, column: 1 }, type: 'small', stats: { reliance: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_0_2', name: '', description: '', position: { row: 0, column: 2 }, type: 'small', stats: { armor: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_ext_top', name: 'Extension Point', description: 'Connect to another board', position: { row: 0, column: 3 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'rel_0_4', name: '', description: '', position: { row: 0, column: 4 }, type: 'small', stats: { maxRelianceIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_0_5', name: '', description: '', position: { row: 0, column: 5 }, type: 'small', stats: { reliance: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_0_6', name: '', description: '', position: { row: 0, column: 6 }, type: 'small', stats: { evasion: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'rel_1_0', name: '', description: '', position: { row: 1, column: 0 }, type: 'small', stats: { maxRelianceIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_notable_1', name: 'Resolve', description: '+8% Max Reliance, +40 Reliance', position: { row: 1, column: 1 }, type: 'notable', stats: { maxRelianceIncrease: 8, maxReliance: 40 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_1_2', name: '', description: '', position: { row: 1, column: 2 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_1_3', name: '', description: '', position: { row: 1, column: 3 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_1_4', name: '', description: '', position: { row: 1, column: 4 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_notable_2', name: 'Composure', description: '+8% Max Reliance, +5% Dodge Chance', position: { row: 1, column: 5 }, type: 'notable', stats: { maxRelianceIncrease: 8, dodgeChance: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_1_6', name: '', description: '', position: { row: 1, column: 6 }, type: 'small', stats: { reliance: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'rel_2_0', name: '', description: '', position: { row: 2, column: 0 }, type: 'small', stats: { armor: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_2_1', name: '', description: '', position: { row: 2, column: 1 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_2_2', name: '', description: '', position: { row: 2, column: 2 }, type: 'small', stats: { maxRelianceIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_2_3', name: '', description: '', position: { row: 2, column: 3 }, type: 'small', stats: { reliance: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_2_4', name: '', description: '', position: { row: 2, column: 4 }, type: 'small', stats: { evasion: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_2_5', name: '', description: '', position: { row: 2, column: 5 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_2_6', name: '', description: '', position: { row: 2, column: 6 }, type: 'small', stats: { maxRelianceIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'rel_ext_left', name: 'Extension Point', description: 'Connect to another board', position: { row: 3, column: 0 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'rel_3_1', name: '', description: '', position: { row: 3, column: 1 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_3_2', name: '', description: '', position: { row: 3, column: 2 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_3_3', name: '', description: '', position: { row: 3, column: 3 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_3_4', name: '', description: '', position: { row: 3, column: 4 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_3_5', name: '', description: '', position: { row: 3, column: 5 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_ext_right', name: 'Extension Point', description: 'Connect to another board', position: { row: 3, column: 6 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] }
    ],
    [
      { id: 'rel_4_0', name: '', description: '', position: { row: 4, column: 0 }, type: 'small', stats: { evasion: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_4_1', name: '', description: '', position: { row: 4, column: 1 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_4_2', name: '', description: '', position: { row: 4, column: 2 }, type: 'small', stats: { reliance: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_4_3', name: '', description: '', position: { row: 4, column: 3 }, type: 'small', stats: { maxRelianceIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_4_4', name: '', description: '', position: { row: 4, column: 4 }, type: 'small', stats: { armor: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_4_5', name: '', description: '', position: { row: 4, column: 5 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_4_6', name: '', description: '', position: { row: 4, column: 6 }, type: 'small', stats: { evasion: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'rel_5_0', name: '', description: '', position: { row: 5, column: 0 }, type: 'small', stats: { maxRelianceIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_notable_3', name: 'Steel Nerves', description: '+8% Max Reliance, +12% Armor', position: { row: 5, column: 1 }, type: 'notable', stats: { maxRelianceIncrease: 8, armorIncrease: 12 } as any, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_5_2', name: '', description: '', position: { row: 5, column: 2 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_5_3', name: '', description: '', position: { row: 5, column: 3 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_5_4', name: '', description: '', position: { row: 5, column: 4 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_notable_4', name: 'Unwavering', description: '+8% Max Reliance, +5% Block Chance', position: { row: 5, column: 5 }, type: 'notable', stats: { maxRelianceIncrease: 8, blockChance: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_5_6', name: '', description: '', position: { row: 5, column: 6 }, type: 'small', stats: { reliance: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'rel_6_0', name: '', description: '', position: { row: 6, column: 0 }, type: 'small', stats: { reliance: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_6_1', name: '', description: '', position: { row: 6, column: 1 }, type: 'small', stats: { maxRelianceIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_6_2', name: '', description: '', position: { row: 6, column: 2 }, type: 'small', stats: { armor: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_ext_bottom', name: 'Extension Point', description: 'Connect to another board', position: { row: 6, column: 3 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'rel_6_4', name: '', description: '', position: { row: 6, column: 4 }, type: 'small', stats: { evasion: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_6_5', name: '', description: '', position: { row: 6, column: 5 }, type: 'small', stats: { maxRelianceIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'rel_6_6', name: '', description: '', position: { row: 6, column: 6 }, type: 'small', stats: { reliance: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ]
  ],
  extensionPoints: [
    { id: 'rel_ext_top', position: { row: 0, column: 3 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'rel_ext_left', position: { row: 3, column: 0 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'rel_ext_right', position: { row: 3, column: 6 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'rel_ext_bottom', position: { row: 6, column: 3 }, availableBoards: [], maxConnections: 1, currentConnections: 0 }
  ],
  maxPoints: 15
} as unknown as PassiveBoard;


