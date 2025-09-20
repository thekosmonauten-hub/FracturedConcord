import { PassiveBoard } from '../../types/passiveTree';

export const FIRE_BOARD: PassiveBoard = {
  id: 'fire_board',
  name: 'Infernal Mastery',
  description: 'Master the destructive power of fire',
  theme: 'fire',
  size: { rows: 7, columns: 7 },
  nodes: [
    // Row 0: Top extension point
    [
      { id: 'fire_0_0', name: '', description: '', position: { row: 0, column: 0 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_0_1', name: '', description: '', position: { row: 0, column: 1 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_0_2', name: '', description: '', position: { row: 0, column: 2 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_0_4', name: '', description: '', position: { row: 0, column: 4 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_0_5', name: '', description: '', position: { row: 0, column: 5 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_0_6', name: '', description: '', position: { row: 0, column: 6 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    // Row 1: Fire damage nodes (moved notables to 1,1 and 1,5)
    [
      { id: 'fire_1_0', name: '', description: '', position: { row: 1, column: 0 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_notable_1', name: 'Infernal Power', description: '+24% Fire Damage, +12 Fire Resistance', position: { row: 1, column: 1 }, type: 'notable', stats: { fireIncrease: 24, fire: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_1_2', name: '', description: '', position: { row: 1, column: 2 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_1_3', name: '', description: '+5 Intelligence', position: { row: 1, column: 3 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_1_4', name: '', description: '', position: { row: 1, column: 4 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_notable_2', name: 'Burning Fury', description: '+24% Fire Damage, +12% Chance to Ignite', position: { row: 1, column: 5 }, type: 'notable', stats: { fireIncrease: 24, chanceToIgnite: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_1_6', name: '', description: '', position: { row: 1, column: 6 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    // Row 2: Notable fire nodes (notables moved out; fill with smalls)
    [
      { id: 'fire_2_0', name: '', description: '', position: { row: 2, column: 0 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_2_1', name: '', description: '', position: { row: 2, column: 1 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_2_2', name: '', description: '', position: { row: 2, column: 2 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_2_3', name: '', description: '', position: { row: 2, column: 3 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_2_4', name: '', description: '', position: { row: 2, column: 4 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_2_5', name: '', description: '', position: { row: 2, column: 5 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_2_6', name: '', description: '', position: { row: 2, column: 6 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    // Row 3: Middle row with extension points
    [
      { id: 'fire_3_1', name: '', description: '', position: { row: 3, column: 1 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_3_2', name: '', description: '', position: { row: 3, column: 2 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_3_3', name: '', description: '', position: { row: 3, column: 3 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_3_4', name: '', description: '', position: { row: 3, column: 4 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_3_5', name: '', description: '', position: { row: 3, column: 5 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
    ],
    // Row 4: Fire conversion nodes (notables moved to row 5)
    [
      { id: 'fire_4_0', name: '', description: '', position: { row: 4, column: 0 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_4_1', name: '', description: '', position: { row: 4, column: 1 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_4_2', name: '', description: '', position: { row: 4, column: 2 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_4_3', name: '', description: '', position: { row: 4, column: 3 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_4_4', name: '', description: '', position: { row: 4, column: 4 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_4_5', name: '', description: '', position: { row: 4, column: 5 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_4_6', name: '', description: '', position: { row: 4, column: 6 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    // Row 5: Ignite and burning nodes (added notables at 5,1 and 5,5)
    [
      { id: 'fire_5_0', name: '', description: '', position: { row: 5, column: 0 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_notable_3', name: 'Flame Conversion', description: '16% of Physical Damage Added as Fire', position: { row: 5, column: 1 }, type: 'notable', stats: { addedPhysicalAsFire: 16 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_5_2', name: '', description: '', position: { row: 5, column: 2 }, type: 'small', stats: { increasedIgniteMagnitude: 15 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_5_3', name: '', description: '', position: { row: 5, column: 3 }, type: 'small', stats: { fireIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_5_4', name: '', description: '', position: { row: 5, column: 4 }, type: 'small', stats: { increasedIgniteMagnitude: 15 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_notable_4', name: 'Infernal Conversion', description: '16% of Fire Damage Added as Cold', position: { row: 5, column: 5 }, type: 'notable', stats: { addedFireAsCold: 16 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_5_6', name: '', description: '', position: { row: 5, column: 6 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    // Row 6: Bottom row
    [
      { id: 'fire_6_0', name: '', description: '', position: { row: 6, column: 0 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_6_1', name: '', description: '', position: { row: 6, column: 1 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_6_2', name: '', description: '', position: { row: 6, column: 2 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_6_4', name: '', description: '', position: { row: 6, column: 4 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_6_5', name: '', description: '', position: { row: 6, column: 5 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'fire_6_6', name: '', description: '', position: { row: 6, column: 6 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ]
  ],
  extensionPoints: [
    { id: 'fire_ext_top', position: { row: 0, column: 3 }, availableBoards: ['fire_keystone'], maxConnections: 1, currentConnections: 0 },
    { id: 'fire_ext_left', position: { row: 3, column: 0 }, availableBoards: ['chaos_board'], maxConnections: 1, currentConnections: 0 },
    { id: 'fire_ext_right', position: { row: 3, column: 6 }, availableBoards: ['chaos_board'], maxConnections: 1, currentConnections: 0 },
    { id: 'fire_ext_bottom', position: { row: 6, column: 3 }, availableBoards: ['chaos_board'], maxConnections: 1, currentConnections: 0 }
  ],
  maxPoints: 15
}; 