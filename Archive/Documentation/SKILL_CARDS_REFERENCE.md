# Skill Cards Reference - For Testing Echo of Breaking

## ⚠️ Important: Guard ≠ Skill

**Common Confusion:**
- **Guard cards** = Grant Guard/Block (defensive) → CardType.Guard
- **Skill cards** = Utility/buffs/special effects → CardType.Skill

**Echo of Breaking triggers ONLY on CardType.Skill!**

---

## ✅ Skill Cards by Class

### **Marauder**
- **Rallying Cry** - Gain Strength
- **War Cry** - AoE utility
- **Intimidating Shout** - Debuff enemy

### **Witch**
- Various spell utilities

### **Thief**
- **Feint** - Evasion boost

### **Ranger**
- **Focus** - Accuracy/damage boost

### **Apostle**
- **Scripture Burn** - Spell utility

### **Brawler**
- Check starter deck for utility cards

---

## ❌ Common Non-Skill Cards (Won't Trigger)

### **Attack Cards (CardType.Attack)**
- Heavy Strike
- Cleave
- Ground Slam
- Molten Strike
- Sunder
- Twin Slash

### **Guard Cards (CardType.Guard)**
- Brace
- Block
- Steel Shield
- Enduring Guard
- Fortify

---

## 🧪 Testing Quick Reference

| Card | Type | Triggers Retaliation? |
|------|------|----------------------|
| Heavy Strike | Attack | ❌ No |
| Brace | Guard | ❌ No |
| Block | Guard | ❌ No |
| Rallying Cry | Skill | ✅ YES |
| War Cry | Skill | ✅ YES |
| Feint | Skill | ✅ YES |

---

## 🔍 How to Check Card Type in Unity

1. **Select any card asset** (e.g., RallyingCry_Extended.asset)
2. **Look for "Card Type" field** in Inspector
3. **Value should be:**
   - 0 = Attack
   - 1 = Guard
   - 2 = Skill
   - 3 = Power
   - 4 = Aura

---

## 📝 Testing Steps

1. **Load a character** with Skill cards in deck
2. **Start combat** with Weeper-of-Bark
3. **Play a Skill card** (Rallying Cry recommended)
4. **Check console** for retaliation messages
5. **Verify damage** applied to player

---

If you're playing Guard cards and expecting retaliation, that's the issue! Try a Skill card instead.

