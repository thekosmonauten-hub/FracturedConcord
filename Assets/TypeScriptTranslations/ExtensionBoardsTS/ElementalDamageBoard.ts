import { PassiveBoard } from '../../types/passiveTree';

export const ELEMENTAL_DAMAGE_BOARD: PassiveBoard = {
  id: 'elemental_damage_board',
  name: 'Elemental Conflux',
  description: 'Balanced increases to Fire, Cold, and Lightning damage.',
  theme: 'elemental',
  size: { rows: 7, columns: 7 },
  nodes: [
    [
      { id: 'elem_0_0', name: '', description: '', position: { row: 0, column: 0 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_0_1', name: '', description: '', position: { row: 0, column: 1 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_0_2', name: '', description: '', position: { row: 0, column: 2 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_ext_top', name: 'Extension Point', description: 'Connect to another board', position: { row: 0, column: 3 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'elem_0_4', name: '', description: '', position: { row: 0, column: 4 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_0_5', name: '', description: '', position: { row: 0, column: 5 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_0_6', name: '', description: '', position: { row: 0, column: 6 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'elem_1_0', name: '', description: '', position: { row: 1, column: 0 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_notable_1', name: 'Prismatic Surge', description: '+8% to each Elemental Damage, +8% Elemental Resist', position: { row: 1, column: 1 }, type: 'notable', stats: { fireIncrease: 8, coldIncrease: 8, lightningIncrease: 8, elementalResist: 8 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_1_2', name: '', description: '', position: { row: 1, column: 2 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_1_3', name: '', description: '', position: { row: 1, column: 3 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_1_4', name: '', description: '', position: { row: 1, column: 4 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_notable_2', name: 'Elemental Focus', description: '+8% to each Elemental Damage, +10 Spell Power', position: { row: 1, column: 5 }, type: 'notable', stats: { fireIncrease: 8, coldIncrease: 8, lightningIncrease: 8, flatSpellPower: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_1_6', name: '', description: '', position: { row: 1, column: 6 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'elem_2_0', name: '', description: '', position: { row: 2, column: 0 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_2_1', name: '', description: '', position: { row: 2, column: 1 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_2_2', name: '', description: '', position: { row: 2, column: 2 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_2_3', name: '', description: '', position: { row: 2, column: 3 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_2_4', name: '', description: '', position: { row: 2, column: 4 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_2_5', name: '', description: '', position: { row: 2, column: 5 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_2_6', name: '', description: '', position: { row: 2, column: 6 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'elem_ext_left', name: 'Extension Point', description: 'Connect to another board', position: { row: 3, column: 0 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'elem_3_1', name: '', description: '', position: { row: 3, column: 1 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_3_2', name: '', description: '', position: { row: 3, column: 2 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_3_3', name: '', description: '', position: { row: 3, column: 3 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_3_4', name: '', description: '', position: { row: 3, column: 4 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_3_5', name: '', description: '', position: { row: 3, column: 5 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_ext_right', name: 'Extension Point', description: 'Connect to another board', position: { row: 3, column: 6 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] }
    ],
    [
      { id: 'elem_4_0', name: '', description: '', position: { row: 4, column: 0 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_4_1', name: '', description: '', position: { row: 4, column: 1 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_4_2', name: '', description: '', position: { row: 4, column: 2 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_4_3', name: '', description: '', position: { row: 4, column: 3 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_4_4', name: '', description: '', position: { row: 4, column: 4 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_4_5', name: '', description: '', position: { row: 4, column: 5 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_4_6', name: '', description: '', position: { row: 4, column: 6 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'elem_5_0', name: '', description: '', position: { row: 5, column: 0 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_notable_3', name: 'Trinity', description: '+8% to each Elemental Damage, +12% Crit Multiplier', position: { row: 5, column: 1 }, type: 'notable', stats: { fireIncrease: 8, coldIncrease: 8, lightningIncrease: 8, critMultiplierIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_5_2', name: '', description: '', position: { row: 5, column: 2 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_5_3', name: '', description: '', position: { row: 5, column: 3 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_5_4', name: '', description: '', position: { row: 5, column: 4 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_notable_4', name: 'Conduction', description: '+8% to each Elemental Damage, +10% Chance to Shock', position: { row: 5, column: 5 }, type: 'notable', stats: { fireIncrease: 8, coldIncrease: 8, lightningIncrease: 8, chanceToShock: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_5_6', name: '', description: '', position: { row: 5, column: 6 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'elem_6_0', name: '', description: '', position: { row: 6, column: 0 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_6_1', name: '', description: '', position: { row: 6, column: 1 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_6_2', name: '', description: '', position: { row: 6, column: 2 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_ext_bottom', name: 'Extension Point', description: 'Connect to another board', position: { row: 6, column: 3 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'elem_6_4', name: '', description: '', position: { row: 6, column: 4 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_6_5', name: '', description: '', position: { row: 6, column: 5 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'elem_6_6', name: '', description: '', position: { row: 6, column: 6 }, type: 'small', stats: { fireIncrease: 4, coldIncrease: 4, lightningIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ]
  ],
  extensionPoints: [
    { id: 'elem_ext_top', position: { row: 0, column: 3 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'elem_ext_left', position: { row: 3, column: 0 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'elem_ext_right', position: { row: 3, column: 6 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'elem_ext_bottom', position: { row: 6, column: 3 }, availableBoards: [], maxConnections: 1, currentConnections: 0 }
  ],
  maxPoints: 15
} as unknown as PassiveBoard;


