import { PassiveBoard } from '../../types/passiveTree';

export const COLD_BOARD: PassiveBoard = {
  id: 'cold_board',
  name: 'Frigid Mastery',
  description: 'Master the chilling power of ice and frost',
  theme: 'cold',
  size: { rows: 7, columns: 7 },
  nodes: [
    // Row 0: Top extension point
    [
      { id: 'cold_0_0', name: '', description: '', position: { row: 0, column: 0 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_0_1', name: '', description: '', position: { row: 0, column: 1 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_0_2', name: '', description: '', position: { row: 0, column: 2 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
       { id: 'cold_0_4', name: '', description: '', position: { row: 0, column: 4 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_0_5', name: '', description: '', position: { row: 0, column: 5 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_0_6', name: '', description: '', position: { row: 0, column: 6 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    // Row 1: Cold damage nodes (moved notables to 1,1 and 1,5)
    [
      { id: 'cold_1_0', name: '', description: '', position: { row: 1, column: 0 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_notable_1', name: 'Frozen Heart', description: '+24% increased Cold Damage, +12 Cold Resistance', position: { row: 1, column: 1 }, type: 'notable', stats: { coldIncrease: 24, cold: 12 }, maxRank: 1, currentRank: 0, cost: 2, requirements: [], connections: [] },
      { id: 'cold_1_2', name: '', description: '', position: { row: 1, column: 2 }, type: 'small', stats: { coldIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_1_3', name: '', description: '', position: { row: 1, column: 3 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_1_4', name: '', description: '', position: { row: 1, column: 4 }, type: 'small', stats: { coldIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_notable_2', name: 'Ice Mastery', description: '+24% inreased Cold Damage, +12% Chance to Freeze', position: { row: 1, column: 5 }, type: 'notable', stats: { coldIncrease: 24, chanceToFreeze: 12 }, maxRank: 1, currentRank: 0, cost: 2, requirements: [], connections: [] },
      { id: 'cold_1_6', name: '', description: '', position: { row: 1, column: 6 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    // Row 2: Notable cold nodes (notables moved out; fill with smalls)
    [
      { id: 'cold_2_0', name: '', description: '', position: { row: 2, column: 0 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_2_1', name: '', description: '', position: { row: 2, column: 1 }, type: 'small', stats: { coldIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_2_2', name: '', description: '', position: { row: 2, column: 2 }, type: 'small', stats: { coldIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_2_3', name: '', description: '', position: { row: 2, column: 3 }, type: 'small', stats: { coldIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_2_4', name: '', description: '', position: { row: 2, column: 4 }, type: 'small', stats: { coldIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_2_5', name: '', description: '', position: { row: 2, column: 5 }, type: 'small', stats: { coldIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_2_6', name: '', description: '', position: { row: 2, column: 6 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    // Row 3: Middle row with extension points
    [
       { id: 'cold_3_1', name: '', description: '', position: { row: 3, column: 1 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_3_2', name: '', description: '', position: { row: 3, column: 2 }, type: 'small', stats: { coldIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_3_3', name: '', description: '', position: { row: 3, column: 3 }, type: 'small', stats: { coldIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_3_4', name: '', description: '', position: { row: 3, column: 4 }, type: 'small', stats: { coldIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_3_5', name: '', description: '', position: { row: 3, column: 5 }, type: 'travel', stats: { intelligence: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
     ],
    // Row 4: Cold conversion nodes (notables moved to row 5)
    [
      { id: 'cold_4_0', name: '', description: '', position: { row: 4, column: 0 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_4_1', name: '', description: '', position: { row: 4, column: 1 }, type: 'small', stats: { coldIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_4_2', name: '', description: '', position: { row: 4, column: 2 }, type: 'small', stats: { coldIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_4_3', name: '', description: '', position: { row: 4, column: 3 }, type: 'small', stats: { coldIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_4_4', name: '', description: '', position: { row: 4, column: 4 }, type: 'small', stats: { coldIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_4_5', name: '', description: '', position: { row: 4, column: 5 }, type: 'small', stats: { coldIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_4_6', name: '', description: '', position: { row: 4, column: 6 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    // Row 5: Freeze and chill nodes (added notables at 5,1 and 5,5)
    [
      { id: 'cold_5_0', name: '', description: '', position: { row: 5, column: 0 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_notable_3', name: 'Frost Conversion', description: '10% of physical damage added as cold', position: { row: 5, column: 1 }, type: 'notable', stats: { addedPhysicalAsCold: 10 }, maxRank: 1, currentRank: 0, cost: 2, requirements: [], connections: [] },
      { id: 'cold_5_2', name: '', description: '', position: { row: 5, column: 2 }, type: 'small', stats: { increasedFreezeDuration: 15 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_5_3', name: '', description: '', position: { row: 5, column: 3 }, type: 'small', stats: { coldIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_5_4', name: '', description: '', position: { row: 5, column: 4 }, type: 'small', stats: { increasedFreezeDuration: 15 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_notable_4', name: 'Winter\'s Grace', description: '18% increased Cold damage and 12% increase Chill magnitude', position: { row: 5, column: 5 }, type: 'notable', stats: { coldIncrease: 18, chillMagnitude: 12 }, maxRank: 1, currentRank: 0, cost: 2, requirements: [], connections: [] },
      { id: 'cold_5_6', name: '', description: '', position: { row: 5, column: 6 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ],
    // Row 6: Bottom row
    [
      { id: 'cold_6_0', name: '', description: '', position: { row: 6, column: 0 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_6_1', name: '', description: '', position: { row: 6, column: 1 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_6_2', name: '', description: '', position: { row: 6, column: 2 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_6_4', name: '', description: '', position: { row: 6, column: 4 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_6_5', name: '', description: '', position: { row: 6, column: 5 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
      { id: 'cold_6_6', name: '', description: '', position: { row: 6, column: 6 }, type: 'small', stats: {}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
    ]
  ],
  maxPoints: 15
}; 