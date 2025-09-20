import { PassiveBoard } from '../../types/passiveTree';

export const LIGHTNING_BOARD_CRIT_AILMENT_BOARD: PassiveBoard = {
  "id": "lightning_board_crit_ailment_board",
  "name": "Lightning Board - Crit / Ailment",
  "description": "Lightning board focusing on maximizing Critical strike chance and damage to inflict high shocks.",
  "theme": "lightning",
  "size": {
    "rows": 7,
    "columns": 7
  },
  "nodes": [
    [
      {
        "id": "lightning_board_crit_ailment_0_0",
        "name": "",
        "description": "",
        "position": {
          "row": 0,
          "column": 0
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_0_1",
        "name": "",
        "description": "",
        "position": {
          "row": 0,
          "column": 1
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_0_2",
        "name": "",
        "description": "",
        "position": {
          "row": 0,
          "column": 2
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_ext_top",
        "name": "Extension Point",
        "description": "Connect to another board",
        "position": {
          "row": 0,
          "column": 3
        },
        "type": "extension",
        "stats": {},
        "maxRank": 1,
        "currentRank": 0,
        "cost": 0,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_0_4",
        "name": "",
        "description": "",
        "position": {
          "row": 0,
          "column": 4
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_0_5",
        "name": "",
        "description": "",
        "position": {
          "row": 0,
          "column": 5
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_0_6",
        "name": "",
        "description": "",
        "position": {
          "row": 0,
          "column": 6
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      }
    ],
    [
      {
        "id": "lightning_board_crit_ailment_1_0",
        "name": "",
        "description": "",
        "position": {
          "row": 1,
          "column": 0
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_notable_1",
        "name": "Lightning Notable 1",
        "description": "",
        "position": {
          "row": 1,
          "column": 1
        },
        "type": "notable",
        "stats": {
          "lightningIncrease": 24,
          "critChanceIncrease": 8,
          "critMultiplierIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_1_2",
        "name": "",
        "description": "",
        "position": {
          "row": 1,
          "column": 2
        },
        "type": "travel",
        "stats": {
          "intelligence": 5,
          "dexterity": 5
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_1_3",
        "name": "",
        "description": "",
        "position": {
          "row": 1,
          "column": 3
        },
        "type": "travel",
        "stats": {
          "intelligence": 5,
          "dexterity": 5
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_1_4",
        "name": "",
        "description": "",
        "position": {
          "row": 1,
          "column": 4
        },
        "type": "travel",
        "stats": {
          "intelligence": 5,
          "dexterity": 5
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_notable_2",
        "name": "Lightning Notable 2",
        "description": "",
        "position": {
          "row": 1,
          "column": 5
        },
        "type": "notable",
        "stats": {
          "lightningIncrease": 24,
          "increasedAilmentEffect": 12,
          "spellPowerIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_1_6",
        "name": "",
        "description": "",
        "position": {
          "row": 1,
          "column": 6
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      }
    ],
    [
      {
        "id": "lightning_board_crit_ailment_2_0",
        "name": "",
        "description": "",
        "position": {
          "row": 2,
          "column": 0
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_2_1",
        "name": "",
        "description": "",
        "position": {
          "row": 2,
          "column": 1
        },
        "type": "travel",
        "stats": {
          "intelligence": 5,
          "dexterity": 5
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_2_2",
        "name": "",
        "description": "",
        "position": {
          "row": 2,
          "column": 2
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_2_3",
        "name": "",
        "description": "",
        "position": {
          "row": 2,
          "column": 3
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_2_4",
        "name": "",
        "description": "",
        "position": {
          "row": 2,
          "column": 4
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_2_5",
        "name": "",
        "description": "",
        "position": {
          "row": 2,
          "column": 5
        },
        "type": "travel",
        "stats": {
          "intelligence": 5,
          "dexterity": 5
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_2_6",
        "name": "",
        "description": "",
        "position": {
          "row": 2,
          "column": 6
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      }
    ],
    [
      {
        "id": "lightning_board_crit_ailment_ext_left",
        "name": "Extension Point",
        "description": "Connect to another board",
        "position": {
          "row": 3,
          "column": 0
        },
        "type": "extension",
        "stats": {},
        "maxRank": 1,
        "currentRank": 0,
        "cost": 0,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_3_1",
        "name": "",
        "description": "",
        "position": {
          "row": 3,
          "column": 1
        },
        "type": "travel",
        "stats": {
          "intelligence": 5,
          "dexterity": 5
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_3_2",
        "name": "",
        "description": "",
        "position": {
          "row": 3,
          "column": 2
        },
        "type": "travel",
        "stats": {
          "intelligence": 5,
          "dexterity": 5
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_3_3",
        "name": "",
        "description": "",
        "position": {
          "row": 3,
          "column": 3
        },
        "type": "travel",
        "stats": {
          "intelligence": 5,
          "dexterity": 5
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_3_4",
        "name": "",
        "description": "",
        "position": {
          "row": 3,
          "column": 4
        },
        "type": "travel",
        "stats": {
          "intelligence": 5,
          "dexterity": 5
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_3_5",
        "name": "",
        "description": "",
        "position": {
          "row": 3,
          "column": 5
        },
        "type": "travel",
        "stats": {
          "intelligence": 5,
          "dexterity": 5
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_ext_right",
        "name": "Extension Point",
        "description": "Connect to another board",
        "position": {
          "row": 3,
          "column": 6
        },
        "type": "extension",
        "stats": {},
        "maxRank": 1,
        "currentRank": 0,
        "cost": 0,
        "requirements": [],
        "connections": []
      }
    ],
    [
      {
        "id": "lightning_board_crit_ailment_4_0",
        "name": "",
        "description": "",
        "position": {
          "row": 4,
          "column": 0
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_4_1",
        "name": "",
        "description": "",
        "position": {
          "row": 4,
          "column": 1
        },
        "type": "travel",
        "stats": {
          "intelligence": 5,
          "dexterity": 5
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_4_2",
        "name": "",
        "description": "",
        "position": {
          "row": 4,
          "column": 2
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_4_3",
        "name": "",
        "description": "",
        "position": {
          "row": 4,
          "column": 3
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_4_4",
        "name": "",
        "description": "",
        "position": {
          "row": 4,
          "column": 4
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_4_5",
        "name": "",
        "description": "",
        "position": {
          "row": 4,
          "column": 5
        },
        "type": "travel",
        "stats": {
          "intelligence": 5,
          "dexterity": 5
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_4_6",
        "name": "",
        "description": "",
        "position": {
          "row": 4,
          "column": 6
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      }
    ],
    [
      {
        "id": "lightning_board_crit_ailment_5_0",
        "name": "",
        "description": "",
        "position": {
          "row": 5,
          "column": 0
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_notable_3",
        "name": "Lightning Notable 3",
        "description": "",
        "position": {
          "row": 5,
          "column": 1
        },
        "type": "notable",
        "stats": {
          "lightningIncrease": 24,
          "increasedMeleeDamage": 12,
          "critChanceIncrease": 8
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_5_2",
        "name": "",
        "description": "",
        "position": {
          "row": 5,
          "column": 2
        },
        "type": "travel",
        "stats": {
          "intelligence": 5,
          "dexterity": 5
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_5_3",
        "name": "",
        "description": "",
        "position": {
          "row": 5,
          "column": 3
        },
        "type": "travel",
        "stats": {
          "intelligence": 5,
          "dexterity": 5
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_5_4",
        "name": "",
        "description": "",
        "position": {
          "row": 5,
          "column": 4
        },
        "type": "travel",
        "stats": {
          "intelligence": 5,
          "dexterity": 5
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_notable_4",
        "name": "Lightning Notable 4",
        "description": "",
        "position": {
          "row": 5,
          "column": 5
        },
        "type": "notable",
        "stats": {
          "lightningIncrease": 24,
          "increasedProjectileDamage": 12,
          "increasedAilmentEffect": 12,
          "increasedShockEffect": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_5_6",
        "name": "",
        "description": "",
        "position": {
          "row": 5,
          "column": 6
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      }
    ],
    [
      {
        "id": "lightning_board_crit_ailment_6_0",
        "name": "",
        "description": "",
        "position": {
          "row": 6,
          "column": 0
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_6_1",
        "name": "",
        "description": "",
        "position": {
          "row": 6,
          "column": 1
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_6_2",
        "name": "",
        "description": "",
        "position": {
          "row": 6,
          "column": 2
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_ext_bottom",
        "name": "Extension Point",
        "description": "Connect to another board",
        "position": {
          "row": 6,
          "column": 3
        },
        "type": "extension",
        "stats": {},
        "maxRank": 1,
        "currentRank": 0,
        "cost": 0,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_6_4",
        "name": "",
        "description": "",
        "position": {
          "row": 6,
          "column": 4
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_6_5",
        "name": "",
        "description": "",
        "position": {
          "row": 6,
          "column": 5
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      },
      {
        "id": "lightning_board_crit_ailment_6_6",
        "name": "",
        "description": "",
        "position": {
          "row": 6,
          "column": 6
        },
        "type": "small",
        "stats": {
          "lightningIncrease": 12
        },
        "maxRank": 1,
        "currentRank": 0,
        "cost": 1,
        "requirements": [],
        "connections": []
      }
    ]
  ],
  "extensionPoints": [
    {
      "id": "lightning_board_crit_ailment_ext_top",
      "position": {
        "row": 0,
        "column": 3
      },
      "availableBoards": [],
      "maxConnections": 1,
      "currentConnections": 0
    },
    {
      "id": "lightning_board_crit_ailment_ext_left",
      "position": {
        "row": 3,
        "column": 0
      },
      "availableBoards": [],
      "maxConnections": 1,
      "currentConnections": 0
    },
    {
      "id": "lightning_board_crit_ailment_ext_right",
      "position": {
        "row": 3,
        "column": 6
      },
      "availableBoards": [],
      "maxConnections": 1,
      "currentConnections": 0
    },
    {
      "id": "lightning_board_crit_ailment_ext_bottom",
      "position": {
        "row": 6,
        "column": 3
      },
      "availableBoards": [],
      "maxConnections": 1,
      "currentConnections": 0
    }
  ],
  "maxPoints": 15
} as unknown as PassiveBoard;
