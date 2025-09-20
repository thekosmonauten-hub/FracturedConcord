import { PassiveBoard } from '../../types/passiveTree';

// Basic square core board with 3 extension points (top/left/right)
export const CORE_BOARD: PassiveBoard = {
	id: 'core_board',
	name: 'Core Board',
		description: 'The foundation of your character progression',
	theme: 'core',
	size: { rows: 7, columns: 7 },
	nodes: [
		// Row 0
		[
			{ id: 'core_0_0', name: 'Max health', description: '4% increased max health', position: { row: 0, column: 0 }, type: 'small', stats: { maxHealthIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_0_1', name: 'Max health', description: '4% Increased Max health', position: { row: 0, column: 1 }, type: 'small', stats: { maxHealthIncrease: 4, strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_0_2', name: 'Intelligence', description: '+10 Intelligence', position: { row: 0, column: 2 }, type: 'small', stats: { intelligence: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_ext_bottom', name: 'Extension Point', description: 'Connect to another board', position: { row: 0, column: 3 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
			{ id: 'core_0_4', name: 'Intelligence', description: '+10 Intelligence', position: { row: 0, column: 4 }, type: 'small', stats: { intelligence: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_0_5', name: 'Increased energy shield', description: '6% increased energy shield', position: { row: 0, column: 5 }, type: 'small', stats: { maxEnergyShieldIncrease: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_0_6', name: 'Increased energy shield', description: '6% increased energy shield', position: { row: 0, column: 6 }, type: 'small', stats: { maxEnergyShieldIncrease: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
		],
		// Row 1
		[
			{ id: 'core_1_0', name: 'Max health', description: '4% increased max health', position: { row: 1, column: 0 }, type: 'small', stats: { maxHealthIncrease: 4 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_Notable_1_1', name: 'Path of the Warrior', description: '+8% increased Max Health, +50 Armour, +20 Strength', position: { row: 1, column: 1 }, type: 'notable', stats: { maxHealthIncrease: 8, armor: 50, strength: 20 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_1_2', name: 'Intelligence', description: '+10 Intelligence', position: { row: 1, column: 2 }, type: 'small', stats: { intelligence: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_1_3', name: 'Intelligence', description: '+10 Intelligence', position: { row: 1, column: 3 }, type: 'small', stats: { intelligence: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_1_4', name: 'Intelligence', description: '+10 Intelligence', position: { row: 1, column: 4 }, type: 'small', stats: { intelligence: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_Notable_1_5', name: 'Path of the Mage', description: '+16% increased Spell damage, +8% increased Energy shield, +20 Intelligence', position: { row: 1, column: 5 }, type: 'notable', stats: { spellPowerIncrease: 16, maxEnergyShield: 8, intelligence: 20 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_1_6', name: 'Increased energy shield', description: '6% increased energy shield', position: { row: 1, column: 6 }, type: 'small', stats: { maxEnergyShieldIncrease: 6 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
		],
		// Row 2
		[
			{ id: 'core_2_0', name: 'Strength', description: '+10 Strength', position: { row: 2, column: 0 }, type: 'small', stats: { strength: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_2_1', name: 'Strength', description: '+10 Strength', position: { row: 2, column: 1 }, type: 'small', stats: { strength: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_2_2', name: 'Intelligence & Strength', description: '+5 Intelligence, 5 Strength', position: { row: 2, column: 2 }, type: 'small', stats: { intelligence: 5, strength: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_2_3', name: 'Intelligence', description: '+10 Intelligence', position: { row: 2, column: 3 }, type: 'small', stats: { intelligence: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_2_4', name: 'Intelligence & Dexterity', description: '+5 Intelligence, 5 Dexterity', position: { row: 2, column: 4 }, type: 'small', stats: { intelligence: 5, dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_2_5', name: 'Dexterity', description: '+10 Dexterity', position: { row: 2, column: 5 }, type: 'small', stats: { dexterity: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_2_6', name: 'Dexterity', description: '+10 Dexterity', position: { row: 2, column: 6 }, type: 'small', stats: { dexterity: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
		],
		// Row 3
		[
			{ id: 'core_ext_left', name: 'Extension Point', description: 'Connect to another board', position: { row: 3, column: 0 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
			{ id: 'core_3_1', name: 'Strength', description: '+10 Strength', position: { row: 3, column: 1 }, type: 'small', stats: { strength: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_3_2', name: 'Strength', description: '+10 Strength', position: { row: 3, column: 2 }, type: 'small', stats: { strength: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_main', name: 'START', description: 'Your journey begins here', position: { row: 3, column: 3 }, type: 'main', stats: {}, maxRank: 1, currentRank: 1, cost: 0, requirements: [], connections: [] },
			{ id: 'core_3_4', name: 'Dexterity', description: '+10 Dexterity', position: { row: 3, column: 4 }, type: 'small', stats: { dexterity: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_3_5', name: 'Dexterity', description: '+10 Dexterity', position: { row: 3, column: 5 }, type: 'small', stats: { dexterity: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_ext_right', name: 'Extension Point', description: 'Connect to another board', position: { row: 3, column: 6 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] }
		],
		// Row 4
		[
			{ id: 'core_4_0', name: 'Strength', description: '+10 Strength', position: { row: 4, column: 0 }, type: 'small', stats: { strength: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_4_1', name: 'Strength', description: '+10 Strength', position: { row: 4, column: 1 }, type: 'small', stats: { strength: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_4_2', name: 'Strength', description: '+10 Strength', position: { row: 4, column: 2 }, type: 'small', stats: { strength: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_4_3', name: 'All attributes', description: '+3 to all attributes', position: { row: 4, column: 3 }, type: 'small', stats: { strength: 3, dexterity: 3, intelligence: 3 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_4_4', name: 'Flat Evasion', description: '+25 Evasion', position: { row: 4, column: 4 }, type: 'small', stats: { evasion: 25 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_4_5', name: 'Dexterity', description: '+10 Dexterity', position: { row: 4, column: 5 }, type: 'small', stats: { dexterity: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_4_6', name: 'Dexterity', description: '+10 Dexterity', position: { row: 4, column: 6 }, type: 'small', stats: { dexterity: 10 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
		],
		// Row 5
		[
			{ id: 'core_5_0', name: 'All attributes', description: '+3 to all attributes', position: { row: 5, column: 0 }, type: 'small', stats: { strength: 5, dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_Notable_5_1', name: 'Path of the Polyglot', description: '+36% increased Armour, Evasion and energy shield, +10% to all Elemental Resistances, +5 to all attributes', position: { row: 5, column: 1 }, type: 'notable', stats: { armorIncrease: 36, increasedEvasion: 36, elementalResist: 10, strength: 5, dexterity: 5, intelligence: 5}, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_5_2', name: 'Strength & Dexterity', description: '+5 Strength, +5 Dexterity', position: { row: 5, column: 2 }, type: 'small', stats: { strength: 5, dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_5_3', name: 'All attributes', description: '+3 to all attributes', position: { row: 5, column: 3 }, type: 'small', stats: { strength: 3, dexterity: 3, intelligence: 3 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_5_4', name: 'Strength & Dexterity', description: '+5 Strength, +5 Dexterity', position: { row: 5, column: 4 }, type: 'small', stats: { strength: 5, dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_Notable_5_5', name: 'Path of the Huntress', description: '+250 to Accuracy rating, +16% increased projectile damage, +20 Dexterity', position: { row: 5, column: 5 }, type: 'notable', stats: { accuracy: 250, increasedProjectileDamage: 16, dexterity: 20 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_5_6', name: 'Strength & Dexterity', description: '+5 Strength, +5 Dexterity', position: { row: 5, column: 6 }, type: 'small', stats: { strength: 5, dexterity: 5 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
		],
		// Row 6 (include START)
		[
			{ id: 'core_6_0', name: 'All attributes', description: '+3 to all attributes', position: { row: 6, column: 0 }, type: 'small', stats: { strength: 3, dexterity: 3, intelligence: 3 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_6_1', name: 'All attributes', description: '+3 to all attributes', position: { row: 6, column: 1 }, type: 'small', stats: { strength: 3, dexterity: 3, intelligence: 3 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_6_2', name: 'All attributes', description: '+3 to all attributes', position: { row: 6, column: 2 }, type: 'small', stats: { strength: 3, dexterity: 3, intelligence: 3 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_ext_top', name: 'Extension Point', description: 'Connect to another board', position: { row: 6, column: 3 }, type: 'extension', stats: {}, maxRank: 1, currentRank: 0, cost: 0, requirements: [], connections: [] },
			{ id: 'core_6_4', name: 'Increased Evasion', description: '12% increased Evasion', position: { row: 6, column: 4 }, type: 'small', stats: { evasionIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_6_5', name: 'Increased Evasion', description: '12% increased Evasion', position: { row: 6, column: 5 }, type: 'small', stats: { evasionIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] },
			{ id: 'core_6_6', name: 'Increased Evasion', description: '12% increased Evasion', position: { row: 6, column: 6 }, type: 'small', stats: { evasionIncrease: 12 }, maxRank: 1, currentRank: 0, cost: 1, requirements: [], connections: [] }
		]
	],
	extensionPoints: [
		{ id: 'core_ext_top', position: { row: 6, column: 3 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
		{ id: 'core_ext_left', position: { row: 3, column: 0 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
		{ id: 'core_ext_right', position: { row: 3, column: 6 }, availableBoards: [], maxConnections: 1, currentConnections: 0 },
		{ id: 'core_ext_bottom', position: { row: 0, column: 3 }, availableBoards: [], maxConnections: 1, currentConnections: 0 }
	],
	maxPoints: 20
} as unknown as PassiveBoard;