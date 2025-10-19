# Enemy Data System - Quick Start Guide

## 🎯 Overview

Create and manage enemy configurations using ScriptableObjects, similar to the card system.

---

## 🚀 Quick Setup (2 minutes)

### Step 1: Create Test Enemies

**Unity Menu:** `Tools > Enemy Data > Create Enemy`

**Click:** "Create Quick Test Enemies"

**Result:** 7 enemies created!
```
✓ Goblin Scout (Normal, Melee)
✓ Orc Warrior (Normal, Tank)
✓ Dark Mage (Normal, Caster)
✓ Skeleton Archer (Normal, Ranged)
✓ Elite Guard (Elite, Tank)
✓ Shadow Assassin (Elite, Melee)
✓ Dragon (Boss, Caster)
```

**Location:** `Assets/Resources/Enemies/`

---

### Step 2: Create Enemy Database

```
1. Create empty GameObject: "EnemyDatabase"
2. Add Component: EnemyDatabase
3. Right-click component → "Reload and Organize"
4. Enemies automatically categorized!
```

---

### Step 3: Use in Combat

Update `CombatDisplayManager.CreateTestEnemies()`:

```csharp
private void CreateTestEnemies()
{
    // OLD WAY (hardcoded):
    // List<Enemy> testEnemies = new List<Enemy>
    // {
    //     new Enemy("Goblin Scout", 30, 6),
    // };
    
    // NEW WAY (from database):
    EnemyDatabase db = EnemyDatabase.Instance;
    if (db != null)
    {
        List<EnemyData> encounter = db.GetRandomEncounter(testEnemyCount);
        
        for (int i = 0; i < encounter.Count && i < enemyDisplays.Count; i++)
        {
            Enemy enemy = encounter[i].CreateEnemy();
            enemyDisplays[i].SetEnemy(enemy);
            activeEnemies.Add(enemy);
        }
    }
}
```

---

## 📊 Enemy Data Properties

### Basic Stats:
- **Name & Description** - Display info
- **Sprite** - Visual representation
- **Health Range** - Min/max HP (randomized)
- **Damage** - Base attack damage
- **Armor** - Damage reduction

### Combat Stats:
- **Critical Chance** - % chance to crit
- **Critical Multiplier** - Crit damage multiplier
- **Dodge Chance** - % chance to evade

### Resistances:
- **Physical/Fire/Cold/Lightning/Chaos** - Damage resistance %
- Negative values = weakness!

### AI Behavior:
- **AI Pattern** - Aggressive, Defensive, Tactical, etc.
- **Attack Preference** - How often to attack vs defend
- **Special Abilities** - List of ability names

### Loot:
- **Gold Drop Range** - Min/max gold
- **Experience Reward** - XP gained
- **Card Drop Chance** - % to drop a card

---

## 🎮 Usage Examples

### Create Random Encounter:
```csharp
EnemyDatabase db = EnemyDatabase.Instance;

// Get 3 random normal enemies
List<EnemyData> enemies = db.GetRandomEncounter(3, EnemyTier.Normal);

// Spawn them
foreach (EnemyData data in enemies)
{
    Enemy enemy = data.CreateEnemy();
    // Add to combat...
}
```

### Create Balanced Encounter:
```csharp
// Get mix of melee, ranged, caster
List<EnemyData> enemies = db.GetBalancedEncounter(3);
```

### Get Specific Enemy Type:
```csharp
// Get random boss
EnemyData boss = db.GetRandomEnemyByTier(EnemyTier.Boss);

// Get random caster
EnemyData caster = db.GetRandomEnemyByCategory(EnemyCategory.Caster);

// Get by name
EnemyData dragon = db.GetEnemyByName("Dragon");
```

---

## 🛠️ Creating Custom Enemies

### Method 1: Unity Editor (GUI)
```
Tools > Enemy Data > Create Enemy
  Enter name, tier, category, stats
  Click "Create Enemy Data"
  
Edit in Inspector:
  - Add sprite
  - Tune resistances
  - Set loot drops
  - Configure AI
```

### Method 2: Right-Click in Project
```
Right-click in Project window
  Create > Dexiled > Combat > Enemy Data
  
Configure in Inspector
```

### Method 3: Code (For Procedural Generation)
```csharp
EnemyData enemy = ScriptableObject.CreateInstance<EnemyData>();
enemy.enemyName = "Procedural Enemy";
enemy.minHealth = 40;
enemy.maxHealth = 60;
enemy.baseDamage = 8;
// ... configure ...
```

---

## 📁 File Organization

```
Assets/
└── Resources/
    └── Enemies/
        ├── Normal/
        │   ├── GoblinScout.asset
        │   ├── OrcWarrior.asset
        │   └── DarkMage.asset
        ├── Elite/
        │   ├── EliteGuard.asset
        │   └── ShadowAssassin.asset
        └── Boss/
            └── Dragon.asset
```

---

## 🎨 Enemy Tiers

### Normal (Common enemies)
- HP: 20-50
- Damage: 4-10
- XP: 10
- Gold: 5-15
- Card drop: 10%

### Elite (Tougher enemies)
- HP: 60-100
- Damage: 10-18
- XP: 25-50
- Gold: 15-40
- Card drop: 25-30%

### Boss (Major encounters)
- HP: 150-300
- Damage: 20-40
- XP: 100+
- Gold: 50-150
- Card drop: 100%

---

## 🤖 AI Patterns

### Aggressive
- Always attacks
- No defensive moves
- High pressure

### Defensive
- Prefers blocking/buffing
- Attacks when safe
- Tanky playstyle

### Balanced
- Mix of attack and defend
- Adaptable
- Medium difficulty

### Tactical
- Uses abilities strategically
- Responds to player state
- Higher difficulty

### Reactive
- Counters player actions
- Defensive when low HP
- Challenging

---

## 🔧 Integration with Existing Systems

### Update CombatDisplayManager:

Add field:
```csharp
[SerializeField] private EnemyDatabase enemyDatabase;
```

Use in CreateTestEnemies():
```csharp
if (enemyDatabase != null)
{
    List<EnemyData> encounter = enemyDatabase.GetRandomEncounter(testEnemyCount);
    foreach (EnemyData data in encounter)
    {
        Enemy enemy = data.CreateEnemy();
        // Spawn enemy...
    }
}
```

### Update EncounterManager:

```csharp
public class EncounterData
{
    public List<EnemyData> enemies; // Use EnemyData instead of hardcoded
    public int minEnemies = 1;
    public int maxEnemies = 3;
}
```

---

## ✅ Benefits

✅ **Easy to balance** - Edit stats in Inspector  
✅ **Reusable** - Same data for multiple encounters  
✅ **Organized** - All enemies in one place  
✅ **Randomization** - Vary HP/damage per spawn  
✅ **Loot tables** - Consistent drop rates  
✅ **Scalable** - Add new enemies easily  

---

## 🎮 Next Steps

1. ✅ Create test enemies (Tools > Enemy Data)
2. ✅ Create EnemyDatabase GameObject
3. ✅ Load enemies (Right-click → Reload and Organize)
4. ✅ Update CombatDisplayManager to use database
5. ✅ Test in play mode!

---

## 💡 Future Enhancements

- [ ] Status effect immunities
- [ ] Elemental attacks for enemies
- [ ] Multi-phase boss fights
- [ ] Enemy abilities/skills
- [ ] Conditional AI (e.g., "enrage at 25% HP")
- [ ] Enemy animations
- [ ] Sound effects per enemy type

---

**Start with "Create Quick Test Enemies" to get 7 pre-configured enemies!** 🎮✨

