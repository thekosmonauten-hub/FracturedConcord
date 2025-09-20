import { PassiveBoard } from '../../types/passiveTree';

export const MAX_RES_COLD_BOARD: PassiveBoard = {
  id: 'max_res_cold_board',
  name: 'Glacial Ward',
  description: 'Low values for Cold Damage plus Maximum Cold Resistance.',
  theme: 'cold',
  size: { rows: 7, columns: 7 },
  nodes: [
    [
      { id: 'mrc_0_0', name: '', description: '', position: { row: 0, column: 0 }, type: 'small', stats: { coldIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_0_1', name: '', description: '', position: { row: 0, column: 1 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_0_2', name: '', description: '', position: { row: 0, column: 2 }, type: 'small', stats: { coldIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_ext_top', name: 'Extension Point', description: 'Connect to another board', position: { row: 0, column: 3 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'mrc_0_4', name: '', description: '', position: { row: 0, column: 4 }, type: 'small', stats: { coldIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_0_5', name: '', description: '', position: { row: 0, column: 5 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_0_6', name: '', description: '', position: { row: 0, column: 6 }, type: 'small', stats: { coldIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'mrc_1_0', name: '', description: '', position: { row: 1, column: 0 }, type: 'small', stats: { coldIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_notable_1', name: 'Frostguard', description: '+12% Cold Damage, +1% Maximum Cold Resistance', position: { row: 1, column: 1 }, type: 'notable', stats: { coldIncrease: 12, maxColdResist: 1 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_1_2', name: '', description: '', position: { row: 1, column: 2 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_1_3', name: '', description: '', position: { row: 1, column: 3 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_1_4', name: '', description: '', position: { row: 1, column: 4 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_notable_2', name: 'Frozen Skin', description: '+12% Cold Damage, +6% Elemental Resist', position: { row: 1, column: 5 }, type: 'notable', stats: { coldIncrease: 12, elementalResist: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_1_6', name: '', description: '', position: { row: 1, column: 6 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'mrc_2_0', name: '', description: '', position: { row: 2, column: 0 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_2_1', name: '', description: '', position: { row: 2, column: 1 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_2_2', name: '', description: '', position: { row: 2, column: 2 }, type: 'small', stats: { coldIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_2_3', name: '', description: '', position: { row: 2, column: 3 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_2_4', name: '', description: '', position: { row: 2, column: 4 }, type: 'small', stats: { coldIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_2_5', name: '', description: '', position: { row: 2, column: 5 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_2_6', name: '', description: '', position: { row: 2, column: 6 }, type: 'small', stats: { coldIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'mrc_ext_left', name: 'Extension Point', description: 'Connect to another board', position: { row: 3, column: 0 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'mrc_3_1', name: '', description: '', position: { row: 3, column: 1 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_3_2', name: '', description: '', position: { row: 3, column: 2 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_3_3', name: '', description: '', position: { row: 3, column: 3 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_3_4', name: '', description: '', position: { row: 3, column: 4 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_3_5', name: '', description: '', position: { row: 3, column: 5 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_ext_right', name: 'Extension Point', description: 'Connect to another board', position: { row: 3, column: 6 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] }
    ],
    [
      { id: 'mrc_4_0', name: '', description: '', position: { row: 4, column: 0 }, type: 'small', stats: { coldIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_4_1', name: '', description: '', position: { row: 4, column: 1 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_4_2', name: '', description: '', position: { row: 4, column: 2 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_4_3', name: '', description: '', position: { row: 4, column: 3 }, type: 'small', stats: { coldIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_4_4', name: '', description: '', position: { row: 4, column: 4 }, type: 'small', stats: { coldIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_4_5', name: '', description: '', position: { row: 4, column: 5 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_4_6', name: '', description: '', position: { row: 4, column: 6 }, type: 'small', stats: { coldIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'mrc_5_0', name: '', description: '', position: { row: 5, column: 0 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_notable_3', name: 'Purifying Ice', description: '+12% Cold Damage, +1% Max Cold Resist', position: { row: 5, column: 1 }, type: 'notable', stats: { coldIncrease: 12, maxColdResist: 1 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_5_2', name: '', description: '', position: { row: 5, column: 2 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_5_3', name: '', description: '', position: { row: 5, column: 3 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_5_4', name: '', description: '', position: { row: 5, column: 4 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_notable_4', name: 'Chilling Ward', description: '+12% Cold Damage, +6% Elemental Resist', position: { row: 5, column: 5 }, type: 'notable', stats: { coldIncrease: 12, elementalResist: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_5_6', name: '', description: '', position: { row: 5, column: 6 }, type: 'small', stats: { coldIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'mrc_6_0', name: '', description: '', position: { row: 6, column: 0 }, type: 'small', stats: { coldIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_6_1', name: '', description: '', position: { row: 6, column: 1 }, type: 'small', stats: {coldIncrease: 4}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_6_2', name: '', description: '', position: { row: 6, column: 2 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_ext_bottom', name: 'Extension Point', description: 'Connect to another board', position: { row: 6, column: 3 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'mrc_6_4', name: '', description: '', position: { row: 6, column: 4 }, type: 'small', stats: { coldIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_6_5', name: '', description: '', position: { row: 6, column: 5 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrc_6_6', name: '', description: '', position: { row: 6, column: 6 }, type: 'small', stats: { coldIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ]
  ],
  extensionPoints: [
    { id: 'mrc_ext_top', position: { row: 0, column: 3 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'mrc_ext_left', position: { row: 3, column: 0 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'mrc_ext_right', position: { row: 3, column: 6 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'mrc_ext_bottom', position: { row: 6, column: 3 }, availableBoards: [], maxConnections: 1, currentConnections: 0 }
  ],
  maxPoints: 15
} as unknown as PassiveBoard;


