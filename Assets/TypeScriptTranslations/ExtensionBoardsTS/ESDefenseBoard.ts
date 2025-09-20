import { PassiveBoard } from '../../types/passiveTree';

export const ES_DEFENSE_BOARD: PassiveBoard = {
  id: 'es_defense_board',
  name: 'Aegis of Insight',
  description: 'Energy Shield, resistances, and spell mitigation.',
  theme: 'life',
  size: { rows: 7, columns: 7 },
  nodes: [
    [
      { id: 'es_0_0', name: '', description: '', position: { row: 0, column: 0 }, type: 'small', stats: { maxEnergyShield: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_0_1', name: '', description: '', position: { row: 0, column: 1 }, type: 'small', stats: { elementalResist: 3 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_0_2', name: '', description: '', position: { row: 0, column: 2 }, type: 'small', stats: { spellPowerIncrease: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_ext_top', name: 'Extension Point', description: 'Connect to another board', position: { row: 0, column: 3 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'es_0_4', name: '', description: '', position: { row: 0, column: 4 }, type: 'small', stats: { maxEnergyShield: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_0_5', name: '', description: '', position: { row: 0, column: 5 }, type: 'small', stats: { elementalResist: 3 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_0_6', name: '', description: '', position: { row: 0, column: 6 }, type: 'small', stats: { spellPowerIncrease: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'es_1_0', name: '', description: '', position: { row: 1, column: 0 }, type: 'small', stats: { maxEnergyShield: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_notable_1', name: 'Ward of Thought', description: '+8% Spell Damage, +100 Energy Shield', position: { row: 1, column: 1 }, type: 'notable', stats: { spellPowerIncrease: 8, maxEnergyShield: 100 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_1_2', name: '', description: '', position: { row: 1, column: 2 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_1_3', name: '', description: '', position: { row: 1, column: 3 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_1_4', name: '', description: '', position: { row: 1, column: 4 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_notable_2', name: 'Arcane Aegis', description: '+8% Spell Damage, +8% Elemental Resist', position: { row: 1, column: 5 }, type: 'notable', stats: { spellPowerIncrease: 8, elementalResist: 8 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_1_6', name: '', description: '', position: { row: 1, column: 6 }, type: 'small', stats: { maxEnergyShield: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'es_2_0', name: '', description: '', position: { row: 2, column: 0 }, type: 'small', stats: { elementalResist: 3 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_2_1', name: '', description: '', position: { row: 2, column: 1 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_2_2', name: '', description: '', position: { row: 2, column: 2 }, type: 'small', stats: { maxEnergyShield: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_2_3', name: '', description: '', position: { row: 2, column: 3 }, type: 'small', stats: { elementalResist: 3 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_2_4', name: '', description: '', position: { row: 2, column: 4 }, type: 'small', stats: { spellPowerIncrease: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_2_5', name: '', description: '', position: { row: 2, column: 5 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_2_6', name: '', description: '', position: { row: 2, column: 6 }, type: 'small', stats: { maxEnergyShield: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'es_ext_left', name: 'Extension Point', description: 'Connect to another board', position: { row: 3, column: 0 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'es_3_1', name: '', description: '', position: { row: 3, column: 1 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_3_2', name: '', description: '', position: { row: 3, column: 2 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_3_3', name: '', description: '', position: { row: 3, column: 3 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_3_4', name: '', description: '', position: { row: 3, column: 4 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_3_5', name: '', description: '', position: { row: 3, column: 5 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_ext_right', name: 'Extension Point', description: 'Connect to another board', position: { row: 3, column: 6 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] }
    ],
    [
      { id: 'es_4_0', name: '', description: '', position: { row: 4, column: 0 }, type: 'small', stats: { spellPowerIncrease: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_4_1', name: '', description: '', position: { row: 4, column: 1 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_4_2', name: '', description: '', position: { row: 4, column: 2 }, type: 'small', stats: { elementalResist: 3 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_4_3', name: '', description: '', position: { row: 4, column: 3 }, type: 'small', stats: { maxEnergyShield: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_4_4', name: '', description: '', position: { row: 4, column: 4 }, type: 'small', stats: { elementalResist: 3 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_4_5', name: '', description: '', position: { row: 4, column: 5 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_4_6', name: '', description: '', position: { row: 4, column: 6 }, type: 'small', stats: { spellPowerIncrease: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'es_5_0', name: '', description: '', position: { row: 5, column: 0 }, type: 'small', stats: { maxEnergyShield: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_notable_3', name: 'Null Shield', description: '+8% Spell Damage, +12% Shock Effect', position: { row: 5, column: 1 }, type: 'notable', stats: { spellPowerIncrease: 8, increasedShockEffect: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_5_2', name: '', description: '', position: { row: 5, column: 2 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_5_3', name: '', description: '', position: { row: 5, column: 3 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_5_4', name: '', description: '', position: { row: 5, column: 4 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_notable_4', name: 'Runic Barrier', description: '+8% Spell Damage, +8% Elemental Resist', position: { row: 5, column: 5 }, type: 'notable', stats: { spellPowerIncrease: 8, elementalResist: 8 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_5_6', name: '', description: '', position: { row: 5, column: 6 }, type: 'small', stats: { maxEnergyShield: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'es_6_0', name: '', description: '', position: { row: 6, column: 0 }, type: 'small', stats: { elementalResist: 3 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_6_1', name: '', description: '', position: { row: 6, column: 1 }, type: 'small', stats: { spellPowerIncrease: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_6_2', name: '', description: '', position: { row: 6, column: 2 }, type: 'small', stats: { maxEnergyShield: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_ext_bottom', name: 'Extension Point', description: 'Connect to another board', position: { row: 6, column: 3 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'es_6_4', name: '', description: '', position: { row: 6, column: 4 }, type: 'small', stats: { elementalResist: 3 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_6_5', name: '', description: '', position: { row: 6, column: 5 }, type: 'small', stats: { spellPowerIncrease: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'es_6_6', name: '', description: '', position: { row: 6, column: 6 }, type: 'small', stats: { maxEnergyShield: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ]
  ],
  extensionPoints: [
    { id: 'es_ext_top', position: { row: 0, column: 3 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'es_ext_left', position: { row: 3, column: 0 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'es_ext_right', position: { row: 3, column: 6 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'es_ext_bottom', position: { row: 6, column: 3 }, availableBoards: [], maxConnections: 1, currentConnections: 0 }
  ],
  maxPoints: 15
} as unknown as PassiveBoard;


