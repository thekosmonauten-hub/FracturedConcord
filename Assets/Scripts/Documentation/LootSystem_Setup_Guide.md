# Loot System Setup Guide

## Overview
The loot system allows you to define rewards for combat encounters including currency, items, experience, and cards.

## Components

### 1. LootTable (ScriptableObject)
Defines what rewards can drop from an encounter.

**Location**: Create via `Assets > Create > Dexiled > Loot System > Loot Table`

**Fields**:
- **Table Name**: Descriptive name for this loot table
- **Base Experience**: Base experience awarded (scales with area level)
- **Experience Per Level**: Additional experience per area level
- **Guaranteed Currency Drops**: Currency that always drops
- **Random Currency Drops**: Currency with a chance to drop
- **Item Drops**: Items that can drop
- **Card Drops**: Cards that can drop (future feature)

### 2. LootEntry
Individual reward definition within a loot table.

**Fields**:
- **Drop Chance**: 0-100% chance to drop this entry
- **Min Quantity**: Minimum amount to drop
- **Max Quantity**: Maximum amount to drop
- **Reward Type**: Currency, Item, Experience, or Card
- **Currency Type**: (if RewardType = Currency) Which currency drops
- **Item Data**: (if RewardType = Item) Which item drops
- **Card Name**: (if RewardType = Card) Which card drops

### 3. LootManager (Singleton)
Manages loot generation and reward distribution.

**Methods**:
- `GenerateLoot(LootTable, int areaLevel)`: Generate rewards from a loot table
- `ApplyRewards(LootDropResult)`: Apply rewards to the character
- `AddCurrency(CurrencyType, int)`: Add currency to player's stash
- `GetCurrencyAmount(CurrencyType)`: Get player's currency count

### 4. LootRewardsUI
Displays rewards after combat victory.

**Setup**:
1. Add `LootRewardsUI` component to combat scene
2. Assign UI references (RewardsPanel, RewardsContent, etc.)
3. Create a reward item prefab with TextMeshProUGUI for display
4. Assign buttons for collecting rewards

## Setup Instructions

### Step 1: Create a Loot Table
1. In Unity, go to `Assets > Create > Dexiled > Loot System > Loot Table`
2. Name it appropriately (e.g., "Act1_BasicEncounter_LootTable")
3. Configure base experience and currency drops

### Step 2: Assign Loot Table to Encounter
1. Open your Encounter Asset (e.g., `1.WhereNightFirstFell.asset`)
2. In the "Loot Rewards" section, assign your loot table
3. The encounter will now use this loot table on victory

### Step 3: Setup Combat Scene UI
1. Open `CombatScene`
2. Add a UI Panel for rewards (or use existing victory screen)
3. Add `LootRewardsUI` component
4. Assign references:
   - **Rewards Panel**: Main container GameObject
   - **Rewards Content**: Transform where reward items spawn
   - **Reward Item Prefab**: Prefab with TextMeshProUGUI
   - **Experience Text**: TMP text for experience display
   - **Victory Text**: TMP text for victory message
   - **Collect Button**: Button to collect rewards
   - **Return Button**: Button to return to main game

### Step 4: Create Reward Item Prefab
1. Create a new UI GameObject
2. Add TextMeshProUGUI component
3. Add LayoutElement component (optional, for grid layouts)
4. Style as desired
5. Save as prefab (e.g., "RewardItemPrefab")

## Example Loot Table Configuration

### Early Game Encounter (Area Level 1)
- **Base Experience**: 50
- **Experience Per Level**: 10
- **Guaranteed Currency**:
  - Orb of Generation: 1-2 (100% chance)
- **Random Currency**:
  - Orb of Infusion: 1 (25% chance)
  - Fire Spirit: 1 (15% chance)

### Boss Encounter (Area Level 5)
- **Base Experience**: 200
- **Experience Per Level**: 25
- **Guaranteed Currency**:
  - Orb of Perfection: 2-3 (100% chance)
  - Orb of Generation: 3-5 (100% chance)
- **Random Currency**:
  - Orb of Mutation: 1 (50% chance)
  - Divine Spirit: 1 (10% chance)
- **Item Drops**:
  - Magic Weapon: 1 (30% chance)
  - Rare Armor: 1 (15% chance)

## Integration with Existing Systems

### Currency System
The loot system integrates with the currency system:
- Rewards are stored in `LootManager`
- Accessible from `EquipmentScreen` (future integration needed)
- Saved with character data

### Experience System
Experience rewards automatically apply to the character:
- Triggers level-up if threshold reached
- Updates character stats
- Auto-saves after applying rewards

### Inventory System
Items can be added to rewards (future integration):
- Currently logs item drops
- Integration with inventory system pending

## Testing

### Test in Combat
1. Start an encounter with a configured loot table
2. Defeat all enemies
3. Loot rewards UI should appear showing:
   - Experience gained
   - Currency drops
   - Item drops
4. Click "Collect Rewards" to apply
5. Check console for confirmation

### Debug Logging
The system provides detailed logging:
- `[LootManager]`: Loot generation and reward application
- `[Combat Victory]`: Rewards generated on victory
- `[LootRewardsUI]`: UI display events

## Future Enhancements
- [ ] Item drop integration with inventory
- [ ] Card drop system
- [ ] Rarity-based drop rates
- [ ] Luck/magic find stats
- [ ] Boss-specific unique drops
- [ ] Multi-stage reward selection (choose your reward)













