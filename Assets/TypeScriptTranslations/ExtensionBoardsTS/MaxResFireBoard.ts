import { PassiveBoard } from '../../types/passiveTree';

export const MAX_RES_FIRE_BOARD: PassiveBoard = {
  id: 'max_res_fire_board',
  name: 'Phoenix Ward',
  description: 'Low values for Fire Damage plus Maximum Fire Resistance.',
  theme: 'fire',
  size: { rows: 7, columns: 7 },
  nodes: [
    [
      { id: 'mrf_0_0', name: '', description: '', position: { row: 0, column: 0 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_0_1', name: '', description: '', position: { row: 0, column: 1 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_0_2', name: '', description: '', position: { row: 0, column: 2 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_ext_top', name: 'Extension Point', description: 'Connect to another board', position: { row: 0, column: 3 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'mrf_0_4', name: '', description: '', position: { row: 0, column: 4 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_0_5', name: '', description: '', position: { row: 0, column: 5 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_0_6', name: '', description: '', position: { row: 0, column: 6 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'mrf_1_0', name: '', description: '', position: { row: 1, column: 0 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_notable_1', name: 'Ember Guard', description: '+12% Fire Damage, +1% Maximum Fire Resistance', position: { row: 1, column: 1 }, type: 'notable', stats: { fireIncrease: 12, maxFireResist: 1 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_1_2', name: '', description: '', position: { row: 1, column: 2 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_1_3', name: '', description: '', position: { row: 1, column: 3 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_1_4', name: '', description: '', position: { row: 1, column: 4 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_notable_2', name: 'Phoenix Skin', description: '+12% Fire Damage, +6% Elemental Resist', position: { row: 1, column: 5 }, type: 'notable', stats: { fireIncrease: 12, elementalResist: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_1_6', name: '', description: '', position: { row: 1, column: 6 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'mrf_2_0', name: '', description: '', position: { row: 2, column: 0 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_2_1', name: '', description: '', position: { row: 2, column: 1 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_2_2', name: '', description: '', position: { row: 2, column: 2 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_2_3', name: '', description: '', position: { row: 2, column: 3 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_2_4', name: '', description: '', position: { row: 2, column: 4 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_2_5', name: '', description: '', position: { row: 2, column: 5 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_2_6', name: '', description: '', position: { row: 2, column: 6 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'mrf_ext_left', name: 'Extension Point', description: 'Connect to another board', position: { row: 3, column: 0 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'mrf_3_1', name: '', description: '', position: { row: 3, column: 1 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_3_2', name: '', description: '', position: { row: 3, column: 2 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_3_3', name: '', description: '', position: { row: 3, column: 3 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_3_4', name: '', description: '', position: { row: 3, column: 4 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_3_5', name: '', description: '', position: { row: 3, column: 5 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_ext_right', name: 'Extension Point', description: 'Connect to another board', position: { row: 3, column: 6 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] }
    ],
    [
      { id: 'mrf_4_0', name: '', description: '', position: { row: 4, column: 0 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_4_1', name: '', description: '', position: { row: 4, column: 1 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_4_2', name: '', description: '', position: { row: 4, column: 2 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_4_3', name: '', description: '', position: { row: 4, column: 3 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_4_4', name: '', description: '', position: { row: 4, column: 4 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_4_5', name: '', description: '', position: { row: 4, column: 5 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_4_6', name: '', description: '', position: { row: 4, column: 6 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'mrf_5_0', name: '', description: '', position: { row: 5, column: 0 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_notable_3', name: 'Purifying Flame', description: '+12% Fire Damage, +6% Max Fire Resist', position: { row: 5, column: 1 }, type: 'notable', stats: { fireIncrease: 12, maxFireResist: 1 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_5_2', name: '', description: '', position: { row: 5, column: 2 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_5_3', name: '', description: '', position: { row: 5, column: 3 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_5_4', name: '', description: '', position: { row: 5, column: 4 }, type: 'travel', stats: { strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_notable_4', name: 'Cleansing Heat', description: '+12% Fire Damage, +6% Elemental Resist', position: { row: 5, column: 5 }, type: 'notable', stats: { fireIncrease: 12, elementalResist: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_5_6', name: '', description: '', position: { row: 5, column: 6 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'mrf_6_0', name: '', description: '', position: { row: 6, column: 0 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_6_1', name: '', description: '', position: { row: 6, column: 1 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_6_2', name: '', description: '', position: { row: 6, column: 2 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_ext_bottom', name: 'Extension Point', description: 'Connect to another board', position: { row: 6, column: 3 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'mrf_6_4', name: '', description: '', position: { row: 6, column: 4 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_6_5', name: '', description: '', position: { row: 6, column: 5 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrf_6_6', name: '', description: '', position: { row: 6, column: 6 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ]
  ],
  extensionPoints: [
    { id: 'mrf_ext_top', position: { row: 0, column: 3 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'mrf_ext_left', position: { row: 3, column: 0 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'mrf_ext_right', position: { row: 3, column: 6 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'mrf_ext_bottom', position: { row: 6, column: 3 }, availableBoards: [], maxConnections: 1, currentConnections: 0 }
  ],
  maxPoints: 15
} as unknown as PassiveBoard;


