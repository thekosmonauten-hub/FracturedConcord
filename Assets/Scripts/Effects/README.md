# Effects System Documentation

## Overview

This effects system provides a database-driven approach to managing visual effects in combat. It integrates with ParticleEffectForUGUI for UI-based particle effects and supports both impact effects and projectile effects.

## Components

### 1. EffectData (ScriptableObject)
Defines individual visual effects with all their properties:
- Effect type (Impact, Projectile, Area, etc.)
- Damage type (Fire, Cold, Lightning, etc.)
- UI Particle settings
- Projectile settings (speed, arc trajectory)
- Impact effect linking
- Tags and requirements

**Create:** Right-click → Create → Dexiled → Effects → Effect Data

### 2. EffectsDatabase (ScriptableObject)
Stores and organizes all effects. Provides query-based lookup:
- Find effects by type, damage type, category
- Auto-categorizes effects
- Priority-based matching

**Create:** Right-click → Create → Dexiled → Effects → Effects Database
**Location:** Must be saved to `Resources/EffectsDatabase.asset` for singleton access

### 3. EffectPointProvider (Component)
Attach to GameObjects (character icons, enemies) to define effect spawn points:
- Head, Chest, Weapon, Default points
- Easy lookup by name

**Usage:** Add component to prefab, create child GameObjects for each point, assign in Inspector

### 4. UIParticleHelper (Static Utility)
Automatically sets up UIParticle and ParticleAttractor components:
- Configures UIParticle for UI space
- Sets up ParticleAttractor for projectiles
- Handles canvas positioning

### 5. CombatEffectManager Integration
New methods added:
- `PlayEffectFromDatabase()` - Play effect using query
- `PlayProjectileFromDatabase()` - Play projectile from player to enemy
- `FindPlayerCharacterIcon()` - Helper to find spawned character

## Setup Steps

### Step 1: Create Effect Prefabs
1. Create GameObject with ParticleSystem
2. Configure ParticleSystem for your effect
3. Save as prefab

### Step 2: Create EffectData Assets
1. Right-click → Create → Dexiled → Effects → Effect Data
2. Assign prefab
3. Configure settings:
   - Effect Type (Impact/Projectile)
   - Damage Type
   - **Associated Card Name** (optional - for card-specific effects)
   - UI Particle Scale (or set useUIParticle=false if prefab already has UIParticle)
   - Projectile Speed (if projectile)
   - Duration

**Note:** If your effect prefab already has UIParticle configured (like FxFireball.prefab), set `useUIParticle = false` to avoid adding a duplicate component.

### Step 3: Create EffectsDatabase
1. Right-click → Create → Dexiled → Effects → Effects Database
2. Assign all EffectData assets to "All Effects" list
3. Click "Categorize Effects" in Inspector (or it auto-runs)
4. Save to `Resources/EffectsDatabase.asset`

### Step 4: Setup Effect Points
1. Open character icon prefabs (RangerIcon, ThiefIcon, etc.)
2. Add `EffectPointProvider` component
3. Create child empty GameObjects:
   - "EffectPoint_Head"
   - "EffectPoint_Chest"
   - "EffectPoint_Weapon"
   - "EffectPoint_Default"
4. Position them visually
5. Assign to EffectPointProvider fields
6. Repeat for enemy prefabs

## Usage Examples

### Card-Specific Effects (Recommended)
```csharp
// Automatically uses card-specific effect if available, otherwise falls back to damage type
Card card = /* your card */;
Transform target = /* enemy or player */;

// For impact effects
CombatEffectManager.Instance.PlayEffectForCard(card, target, isCritical: false);

// For projectile effects
Transform playerIcon = CombatEffectManager.Instance.FindPlayerCharacterIcon();
CombatEffectManager.Instance.PlayProjectileForCard(
    card,
    playerIcon,
    target,
    "Weapon",  // Start point (player)
    "Default"  // End point (enemy always uses "Default")
);
```

### Simple Impact Effect (by damage type)
```csharp
var query = new EffectQuery 
{ 
    effectType = VisualEffectType.Impact, 
    damageType = DamageType.Fire 
};
CombatEffectManager.Instance.PlayEffectFromDatabase(query, enemyTransform, "Chest");
```

### Projectile from Player to Enemy (by damage type)
```csharp
Transform playerIcon = CombatEffectManager.Instance.FindPlayerCharacterIcon();
EnemyCombatDisplay enemyDisplay = /* get enemy */;

CombatEffectManager.Instance.PlayProjectileFromDatabase(
    playerIcon,
    enemyDisplay.transform,
    DamageType.Fire,
    isCritical: false,
    "Weapon",  // Start point
    "Default"  // End point (enemies always use "Default")
);
```

### Using EffectData Directly
```csharp
EffectData fireProjectile = EffectsDatabase.Instance.FindProjectileEffect(DamageType.Fire);
CombatEffectManager.Instance.PlayProjectileFromData(
    fireProjectile,
    playerIcon,
    enemyTransform
);
```

## Card-Specific Effects Setup

To create a card-specific effect (like Fireball):

1. **Create EffectData** for the card:
   - Set `Associated Card Name` = "Fireball" (exact card name)
   - Set `Effect Type` = Projectile (if it's a projectile)
   - Assign your effect prefab (e.g., FxFireball.prefab)
   - Set `useUIParticle = false` if prefab already has UIParticle configured

2. **Use in code:**
```csharp
// Automatically finds card-specific effect, falls back to damage type if not found
CombatEffectManager.Instance.PlayProjectileForCard(card, playerIcon, enemyTransform);
```

## Integration with Card System

In your card effect processor, you can now use:

```csharp
// When processing any card (automatically uses card-specific effect if available)
Card card = /* your card */;
Transform playerIcon = CombatEffectManager.Instance.FindPlayerCharacterIcon();
EnemyCombatDisplay targetEnemy = /* get enemy */;

if (targetEnemy != null && playerIcon != null)
{
    // For projectile cards
    if (card.scalesWithProjectileWeapon || card.tags.Contains("Projectile"))
    {
        CombatEffectManager.Instance.PlayProjectileForCard(
            card,
            playerIcon,
            targetEnemy.transform,
            "Weapon",
            "Default"  // Enemies always use "Default"
        );
    }
    else
    {
        // For melee/impact cards
        CombatEffectManager.Instance.PlayEffectForCard(card, targetEnemy.transform);
    }
}
```

**Priority System:**
1. **Card-specific effect** (if `Associated Card Name` matches)
2. **Damage type effect** (falls back to card's `primaryDamageType`)
3. **Warning logged** if no effect found

## Notes

- Effects automatically use UIParticle if `useUIParticle = true` in EffectData
- Projectiles use ParticleAttractor from ParticleEffectForUGUI package
- Effect points are optional - system falls back to transform position if not found
- Database auto-categorizes effects on validate
- All effects work in UI space (Canvas-based)

