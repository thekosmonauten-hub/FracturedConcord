import { PassiveBoard } from '../../types/passiveTree';

export const SPELL_POWER_BOARD: PassiveBoard = {
  id: 'spell_power_board',
  name: 'Arcane Mastery',
  description: 'Spell damage, cast speed, and critical scaling.',
  theme: 'spell',
  size: { rows: 7, columns: 7 },
  nodes: [
    [
      { id: 'spell_0_0', name: '', description: '', position: { row: 0, column: 0 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_0_1', name: '', description: '', position: { row: 0, column: 1 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_0_2', name: '', description: '', position: { row: 0, column: 2 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_ext_top', name: 'Extension Point', description: 'Connect to another board', position: { row: 0, column: 3 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'spell_0_4', name: '', description: '', position: { row: 0, column: 4 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_0_5', name: '', description: '', position: { row: 0, column: 5 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_0_6', name: '', description: '', position: { row: 0, column: 6 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'spell_1_0', name: '', description: '', position: { row: 1, column: 0 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_notable_1', name: 'Arcanist', description: '+24% Spell Damage, +8% Crit Chance', position: { row: 1, column: 1 }, type: 'notable', stats: { spellPowerIncrease: 24, criticalChance: 8 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_1_2', name: '', description: '', position: { row: 1, column: 2 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_1_3', name: '', description: '', position: { row: 1, column: 3 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_1_4', name: '', description: '', position: { row: 1, column: 4 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_notable_2', name: 'Focus', description: '+24% Spell Damage, +12% Spell Power', position: { row: 1, column: 5 }, type: 'notable', stats: { spellPowerIncrease: 24, spellPower: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_1_6', name: '', description: '', position: { row: 1, column: 6 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'spell_2_0', name: '', description: '', position: { row: 2, column: 0 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_2_1', name: '', description: '', position: { row: 2, column: 1 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_2_2', name: '', description: '', position: { row: 2, column: 2 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_2_3', name: '', description: '', position: { row: 2, column: 3 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_2_4', name: '', description: '', position: { row: 2, column: 4 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_2_5', name: '', description: '', position: { row: 2, column: 5 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_2_6', name: '', description: '', position: { row: 2, column: 6 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'spell_ext_left', name: 'Extension Point', description: 'Connect to another board', position: { row: 3, column: 0 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'spell_3_1', name: '', description: '', position: { row: 3, column: 1 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_3_2', name: '', description: '', position: { row: 3, column: 2 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_3_3', name: '', description: '', position: { row: 3, column: 3 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_3_4', name: '', description: '', position: { row: 3, column: 4 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_3_5', name: '', description: '', position: { row: 3, column: 5 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_ext_right', name: 'Extension Point', description: 'Connect to another board', position: { row: 3, column: 6 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] }
    ],
    [
      { id: 'spell_4_0', name: '', description: '', position: { row: 4, column: 0 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_4_1', name: '', description: '', position: { row: 4, column: 1 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_4_2', name: '', description: '', position: { row: 4, column: 2 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_4_3', name: '', description: '', position: { row: 4, column: 3 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_4_4', name: '', description: '', position: { row: 4, column: 4 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_4_5', name: '', description: '', position: { row: 4, column: 5 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_4_6', name: '', description: '', position: { row: 4, column: 6 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'spell_5_0', name: '', description: '', position: { row: 5, column: 0 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_notable_3', name: 'Runeword', description: '+24% Spell Damage, +12% Crit Multiplier', position: { row: 5, column: 1 }, type: 'notable', stats: { spellPowerIncrease: 24, criticalMultiplier: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_5_2', name: '', description: '', position: { row: 5, column: 2 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_5_3', name: '', description: '', position: { row: 5, column: 3 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_5_4', name: '', description: '', position: { row: 5, column: 4 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_notable_4', name: 'Arcane Current', description: '+24% Spell Damage, +10% Chance to Shock', position: { row: 5, column: 5 }, type: 'notable', stats: { spellPowerIncrease: 24, chanceToShock: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_5_6', name: '', description: '', position: { row: 5, column: 6 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    [
      { id: 'spell_6_0', name: '', description: '', position: { row: 6, column: 0 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_6_1', name: '', description: '', position: { row: 6, column: 1 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_6_2', name: '', description: '', position: { row: 6, column: 2 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_ext_bottom', name: 'Extension Point', description: 'Connect to another board', position: { row: 6, column: 3 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
      { id: 'spell_6_4', name: '', description: '', position: { row: 6, column: 4 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_6_5', name: '', description: '', position: { row: 6, column: 5 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'spell_6_6', name: '', description: '', position: { row: 6, column: 6 }, type: 'small', stats: { spellPowerIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ]
  ],
  extensionPoints: [
    { id: 'spell_ext_top', position: { row: 0, column: 3 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'spell_ext_left', position: { row: 3, column: 0 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'spell_ext_right', position: { row: 3, column: 6 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
    { id: 'spell_ext_bottom', position: { row: 6, column: 3 }, availableBoards: [], maxConnections: 1, currentConnections: 0 }
  ],
  maxPoints: 15
} as unknown as PassiveBoard;


