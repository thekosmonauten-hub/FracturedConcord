# Affix System Quick Reference

## Quick Start Guide

### 1. Create Database (One-time setup)
```
Unity Menu: Dexiled → Create Affix Database
Location: Assets/Resources/AffixDatabase.asset
```

### 2. Add Physical Damage Affixes (Example)
```
1. Select AffixDatabase asset
2. Click "Add Physical Damage Affixes" button
3. Verify 25 affixes added in organized categories
```

---

## Adding New Affix Types

### Template for New Affix Category

```csharp
[ContextMenu("Add [Type] Damage Affixes")]
public void Add[Type]DamageAffixes()
{
    // Get or create category
    AffixCategory category = GetOrCreateCategory(weaponPrefixCategories, "[Type]");
    
    // Create sub-categories
    AffixSubCategory increased = GetOrCreateSubCategory(category, "Increased");
    AffixSubCategory flat = GetOrCreateSubCategory(category, "Flat");
    AffixSubCategory hybrid = GetOrCreateSubCategory(category, "Hybrid");
    
    // Add affixes (example for Fire damage)
    Add[Type]DamageAffix(increased, "Burning", 1, 10, 15, AffixTier.Tier9);
    Add[Type]DamageAffix(increased, "Blazing", 11, 20, 25, AffixTier.Tier8);
    // ... continue for all tiers
    
    Add[Type]FlatDamageAffix(flat, "Flaming", 2, 1, 3, AffixTier.Tier9);
    Add[Type]FlatDamageAffix(flat, "Scorching", 13, 4, 9, AffixTier.Tier8);
    // ... continue for all tiers
}

private void Add[Type]DamageAffix(AffixSubCategory category, string name, int itemLevel, int minPercent, int maxPercent, AffixTier tier)
{
    Affix affix = new Affix(name, $"{minPercent}-{maxPercent}% increased [Type] Damage", AffixType.Prefix, tier);
    
    AffixModifier mod = new AffixModifier("[Type]Damage", minPercent, maxPercent, ModifierType.Increased);
    mod.damageType = DamageType.[Type]; // Fire, Cold, Lightning, etc.
    affix.modifiers.Add(mod);
    
    affix.requiredTags = new List<string> { "weapon", "attack", "[type]" };
    category.affixes.Add(affix);
}
```

### Common Affix Patterns

#### Damage Types
- **Physical**: `DamageType.Physical`, tag: `"physical"`
- **Fire**: `DamageType.Fire`, tag: `"fire"`
- **Cold**: `DamageType.Cold`, tag: `"cold"`
- **Lightning**: `DamageType.Lightning`, tag: `"lightning"`
- **Chaos**: `DamageType.Chaos`, tag: `"chaos"`

#### Modifier Types
- **Flat**: `ModifierType.Flat` - Adds X to Y damage
- **Increased**: `ModifierType.Increased` - +X% increased damage
- **More**: `ModifierType.More` - X% more damage (multiplicative)
- **Reduced**: `ModifierType.Reduced` - -X% reduced damage
- **Less**: `ModifierType.Less` - X% less damage (multiplicative)

---

## Tier System

### Tier Progression (Worst to Best)
```
Tier 9: Level 1+   (Lowest tier)
Tier 8: Level 10+  (Low tier)
Tier 7: Level 20+  (Low-mid tier)
Tier 6: Level 30+  (Mid tier)
Tier 5: Level 40+  (Mid-high tier)
Tier 4: Level 50+  (High tier)
Tier 3: Level 60+  (Very high tier)
Tier 2: Level 70+  (Highest tier)
Tier 1: Level 80+  (Best tier)
```

### Value Scaling Examples
```
Tier 9: 10-15% increased damage
Tier 8: 20-25% increased damage
Tier 7: 30-35% increased damage
Tier 6: 40-50% increased damage
Tier 5: 55-70% increased damage
Tier 4: 75-90% increased damage
Tier 3: 95-110% increased damage
Tier 2: 115-130% increased damage
Tier 1: 135-150% increased damage
```

---

## Naming Conventions

### Affix Names (Path of Exile Style)
```
Physical: Heavy, Serrated, Wicked, Vicious, Bloodthirsty, Cruel, Tyrannical, Merciless
Fire: Burning, Blazing, Scorching, Infernal, Fiery, Molten, Volcanic, Hellish
Cold: Chilling, Freezing, Frosty, Icy, Glacial, Arctic, Polar, Frigid
Lightning: Sparking, Shocking, Electric, Thunderous, Stormy, Lightning, Voltaic, Electrifying
```

### Category Names
```
Use PascalCase: "Physical", "Fire", "Cold", "Lightning", "AttackSpeed", "CriticalStrike"
```

### Sub-Category Names
```
Use PascalCase: "Increased", "Flat", "Hybrid", "Speed", "Critical", "Accuracy"
```

---

## Tag System

### Common Tags
```csharp
// Item types
"weapon", "armour", "accessory", "consumable", "material"

// Damage types
"physical", "fire", "cold", "lightning", "chaos"

// Attack types
"attack", "spell", "melee", "ranged", "projectile"

// Weapon types
"sword", "axe", "mace", "bow", "wand", "staff", "dagger", "claw"
```

### Tag Usage Examples
```csharp
// Physical damage on any weapon
affix.requiredTags = new List<string> { "weapon", "attack", "physical" };

// Fire damage on swords only
affix.requiredTags = new List<string> { "weapon", "attack", "fire", "sword" };

// Attack speed on melee weapons
affix.requiredTags = new List<string> { "weapon", "attack", "melee" };
```

---

## Testing Your Affixes

### 1. Add Test Button
```csharp
// In AffixDatabaseEditor.cs
if (GUILayout.Button("Add [Type] Damage Affixes"))
{
    affixDatabase.Add[Type]DamageAffixes();
    EditorUtility.SetDirty(affixDatabase);
    AssetDatabase.SaveAssets();
    Debug.Log("[Type] damage affixes added!");
}
```

### 2. Test Affix Generation
```csharp
// Use AffixTest component
1. Add AffixTest to GameObject
2. Assign test weapon
3. Set item level
4. Run test to verify affix generation
```

### 3. Verify in Inspector
```
1. Select AffixDatabase asset
2. Check statistics show correct counts
3. Expand categories to see organized affixes
4. Verify tier progression and values
```

---

## Common Issues and Solutions

### Issue: Affixes not appearing
**Solution**: Check required tags match item tags in `GetItemTags()` method

### Issue: Wrong tier affixes appearing
**Solution**: Verify `GetMaxTierForLevel()` method and item level requirements

### Issue: Affix values too high/low
**Solution**: Adjust min/max values in affix creation methods

### Issue: Database not found
**Solution**: Ensure AffixDatabase asset exists in `Assets/Resources/` folder

---

## Performance Tips

### For Large Affix Databases
1. **Use helper methods**: `GetOrCreateCategory()` and `GetOrCreateSubCategory()`
2. **Cache lookups**: Consider caching frequently accessed affixes
3. **Lazy loading**: Load affixes only when needed
4. **Memory management**: Monitor memory usage with many affixes

### Best Practices
1. **Consistent naming**: Follow established patterns
2. **Proper tiering**: Ensure logical value progression
3. **Tag consistency**: Use consistent tag naming
4. **Documentation**: Comment complex affix logic
5. **Testing**: Always test new affix categories

---

*This quick reference provides the essential information needed to add new affix types to the hierarchical system. For detailed implementation examples, see the main development log.*
