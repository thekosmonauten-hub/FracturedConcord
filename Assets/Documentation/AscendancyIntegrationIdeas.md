Ascendancy Implementation Ideas:

Make Ascendancy nodes easy to:

Register

Activate/deactivate

Apply buffs/auras

Listen to events (attack, spell cast, discard, take damage, etc.)

Modify stats dynamically

Trigger one-off effects

Scale with character stats / combat states

🧩 CORE SUGGESTION: “Modifier Event System” + “Effect Definitions”

You want THREE layers:

1) The Modifier Registry

A global place that defines what each Ascendancy node does, without coding each individually over and over.

Example schema:
{
  "Spellwoven_Strikes": {
    "type": "modifier",
    "tags": ["ascendancy", "bladeweaver"],
    "effects": [
      {
        "event": "OnAttack",
        "apply": {
          "add_element_brand": "fire"
        }
      }
    ]
  }
}


Modifiers should describe WHAT happens, not HOW.
Your engine handles the “how.”

2) A Character-Attached Modifier Handler

This is the system that:

listens to combat events

checks if active modifiers want to respond

applies their effects

Core engine events:
OnAttackUsed
OnSpellCast
OnDamageTaken
OnKill
OnTurnStart
OnTurnEnd
OnDeckShuffle
OnDiscard
OnManaSpent
OnStatusApplied


Each Ascendancy node will respond to 1–4 of these events.

Example pseudocode:
void OnAttackUsed(AttackData attack) {
    foreach (modifier in character.modifiers) {
        modifier.TryTrigger("OnAttack", attack);
    }
}

3) Effect Scripts / Resolvers

These are small reusable functions like:
add X damage
gain ward
apply status
create stacks
transform damage types
reduce cost
trigger nova
duplicate card
randomize element
damage as extra X element
add brand effects

Example effect registry:
"add_brand_effect": {
  "params": ["elementType"],
  "function_name": "ApplyBrandToAttack"
}


Your engine has the function:

void ApplyBrandToAttack(Character c, Attack a, string elementType) {
    if (elementType == "fire")
        a.AddExtraFlatDamage("fire", c.intelligence * 0.6);
}

🚀 PUTTING IT TOGETHER: 
1. Modifier Definition: Mirrorsteel Guard (Guard node)
{
  "Mirrorsteel_Guard": {
    "type": "modifier",
    "tags": ["ascendancy", "defense", "bladeweaver"],
    "variables": {
      "maxStacks": 3,
      "damageReduction": 0.40
    },
    "effects": [
      {
        "event": "OnCombatStart",
        "apply": {
          "set_stack": {
            "id": "Mirrorsteel",
            "amount": 3
          }
        }
      },
      {
        "event": "OnDamageTaken",
        "conditions": [
          { "has_stack": "Mirrorsteel" }
        ],
        "apply": {
          "reduce_hit_percent": 40,
          "consume_stack": { "id": "Mirrorsteel", "amount": 1 }
        }
      },
      {
        "event": "OnCardPlayed",
        "conditions": [
          { "card_type": "Guard" },
          { "previous_card_type": "Spell" }
        ],
        "apply": {
          "add_stack": {
            "id": "Mirrorsteel",
            "amount": 1,
            "respectMax": true
          }
        }
      }
    ]
  }
}

2. Stack Definition (Mirrorsteel)
{
  "Mirrorsteel": {
    "max": 3,
    "persistent": false,
    "description": "Reduces next hit damage by 40%. Consumed when triggered."
  }
}



Any node can now reference "Mirrorsteel" without rewriting logic.

🔺 KEY PRINCIPLES ANOTHER AGENT SHOULD FOLLOW

Here’s exactly what to tell them:

1. Make all Ascendancy Nodes data-driven.

No hardcoding one-off cases.
Nodes should be read by the engine and processed through a generic modifier handler.

2. Create a standardized Event Listener Framework.

This lets any node hook into gameplay without adding new code.

3. Build modular “Effect Resolvers.”

Examples:

AddFlatDamage

AddPercentDamage

AddStatus

ConsumeStacks

HealPercMissing

CopyCard

ChangeDamageType

Modifiers only describe what they want → resolvers handle execution.

4. Use a StackManager for complex stacking mechanics.

Perfect for:

Corruption

Wardweave

Entropy

Combo

Momentum

Brands

5. Provide a Debug Console to inspect active modifiers.

Super helpful for developers and balancing.

🧪 OPTIONAL: Node Tags for Filtering/Scaling

Useful for hybrid classes like Witch Battlemage.

"tags": ["spell", "attack", "hybrid", "elemental", "defensive"]


Engine can then do things like:

Scale all “elemental” nodes together

Check synergy or incompatibilities

UI grouping