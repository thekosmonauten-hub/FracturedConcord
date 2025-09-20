import { PassiveBoard } from '../../types/passiveTree';

export const MAX_RES_LIGHTNING_BOARD: PassiveBoard = {
  id: 'max_res_lightning_board',
  name: 'Storm Ward',
  description: 'Low values for Lightning Damage plus Maximum Lightning Resistance.',
  theme: 'lightning',
  size: { rows: 7, columns: 7 },
  nodes: [
    [
      { id: 'mrl_0_0', name: '', description: '', position: { row: 0, column: 0 }, type: 'small', stats: { lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_0_1', name: '', description: '', position: { row: 0, column: 1 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_0_2', name: '', description: '', position: { row: 0, column: 2 }, type: 'small', stats: { lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_ext_top', name: 'Extension Point', description: 'Connect to another board', position: { row: 0, column: 3 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'mrl_0_4', name: '', description: '', position: { row: 0, column: 4 }, type: 'small', stats: { lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_0_5', name: '', description: '', position: { row: 0, column: 5 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_0_6', name: '', description: '', position: { row: 0, column: 6 }, type: 'small', stats: { lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'mrl_1_0', name: '', description: '', position: { row: 1, column: 0 }, type: 'small', stats: { lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_notable_1', name: 'Stormguard', description: '+12% Lightning Damage, +1% Maximum Lightning Resistance', position: { row: 1, column: 1 }, type: 'notable', stats: { lightningIncrease: 12, maxLightningResist: 1 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_1_2', name: '', description: '', position: { row: 1, column: 2 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_1_3', name: '', description: '', position: { row: 1, column: 3 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_1_4', name: '', description: '', position: { row: 1, column: 4 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_notable_2', name: 'Storm Skin', description: '+12% Lightning Damage, +6% Elemental Resist', position: { row: 1, column: 5 }, type: 'notable', stats: { lightningIncrease: 12, elementalResist: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_1_6', name: '', description: '', position: { row: 1, column: 6 }, type: 'small', stats: { lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'mrl_2_0', name: '', description: '', position: { row: 2, column: 0 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_2_1', name: '', description: '', position: { row: 2, column: 1 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_2_2', name: '', description: '', position: { row: 2, column: 2 }, type: 'small', stats: { lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_2_3', name: '', description: '', position: { row: 2, column: 3 }, type: 'small', stats: { lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_2_4', name: '', description: '', position: { row: 2, column: 4 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_2_5', name: '', description: '', position: { row: 2, column: 5 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_2_6', name: '', description: '', position: { row: 2, column: 6 }, type: 'small', stats: { lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'mrl_ext_left', name: 'Extension Point', description: 'Connect to another board', position: { row: 3, column: 0 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'mrl_3_1', name: '', description: '', position: { row: 3, column: 1 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_3_2', name: '', description: '', position: { row: 3, column: 2 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_3_3', name: '', description: '', position: { row: 3, column: 3 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_3_4', name: '', description: '', position: { row: 3, column: 4 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_3_5', name: '', description: '', position: { row: 3, column: 5 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_ext_right', name: 'Extension Point', description: 'Connect to another board', position: { row: 3, column: 6 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] }
    ],
    [
      { id: 'mrl_4_0', name: '', description: '', position: { row: 4, column: 0 }, type: 'small', stats: { lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_4_1', name: '', description: '', position: { row: 4, column: 1 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_4_2', name: '', description: '', position: { row: 4, column: 2 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_4_3', name: '', description: '', position: { row: 4, column: 3 }, type: 'small', stats: { lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_4_4', name: '', description: '', position: { row: 4, column: 4 }, type: 'small', stats: { lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_4_5', name: '', description: '', position: { row: 4, column: 5 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_4_6', name: '', description: '', position: { row: 4, column: 6 }, type: 'small', stats: { lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'mrl_5_0', name: '', description: '', position: { row: 5, column: 0 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_notable_3', name: 'Purifying Storm', description: '+12% Lightning Damage, +1% Max Lightning Resist', position: { row: 5, column: 1 }, type: 'notable', stats: { lightningIncrease: 12, maxLightningResist: 1 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_5_2', name: '', description: '', position: { row: 5, column: 2 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_5_3', name: '', description: '', position: { row: 5, column: 3 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_5_4', name: '', description: '', position: { row: 5, column: 4 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_notable_4', name: 'Crackling Ward', description: '+12% Lightning Damage, +6% Elemental Resist', position: { row: 5, column: 5 }, type: 'notable', stats: { lightningIncrease: 12, elementalResist: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_5_6', name: '', description: '', position: { row: 5, column: 6 }, type: 'small', stats: { lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'mrl_6_0', name: '', description: '', position: { row: 6, column: 0 }, type: 'small', stats: { lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_6_1', name: '', description: '', position: { row: 6, column: 1 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_6_2', name: '', description: '', position: { row: 6, column: 2 }, type: 'small', stats: { lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_ext_bottom', name: 'Extension Point', description: 'Connect to another board', position: { row: 6, column: 3 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'mrl_6_4', name: '', description: '', position: { row: 6, column: 4 }, type: 'small', stats: { lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_6_5', name: '', description: '', position: { row: 6, column: 5 }, type: 'small', stats: { elementalResist: 2 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'mrl_6_6', name: '', description: '', position: { row: 6, column: 6 }, type: 'small', stats: { lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ]
  ],
  extensionPoints: [
    { id: 'mrl_ext_top', position: { row: 0, column: 3 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'mrl_ext_left', position: { row: 3, column: 0 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'mrl_ext_right', position: { row: 3, column: 6 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'mrl_ext_bottom', position: { row: 6, column: 3 }, availableBoards: [], maxConnections: 1, currentConnections: 0 }
  ],
  maxPoints: 15
} as unknown as PassiveBoard;


