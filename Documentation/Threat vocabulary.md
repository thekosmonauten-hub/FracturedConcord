Problem:
Combat currently feels deterministic and passive:
“I already know the right play. I just click cards and enemy HP goes down.”
Enemy actions resolve, but player interaction with enemy intent is shallow. Enemy intent exists, but it is:
Not always up-to-date
Not reactive
Not meaningfully interactable
The goal is to make combat feel responsive, readable, and interruptible, without adding heavy animation work.

Core Goal
Introduce a Threat Vocabulary system that:
Clearly telegraphs enemy intent
Allows player actions to interact with, delay, weaken, or redirect that intent
Supports encounters with 3–5 enemies per wave, up to 10 waves

Required System Changes
1. Enemy Intent as a Queue (Not a Single Action)

Enemy intent must be treated as a mutable queue, not a static “next action”.
Each enemy should have:
An Intent Queue (1–3 upcoming intents)
Each intent has:
Type (Strike, Channel, Summon, Debuff, etc.)
Timing (Turns until execution)
Threat Tier (Minor / Major / Lethal)
Tags (Physical, Fire, Channel, Interruptible, etc.)

This queue must be:
Insertable
Removable
Delayable
Upgradable / Downgradable

3. Reactive Combat Hooks

Player cards, skills, and statuses must be able to:

Delay an intent
Remove an intent
Weaken an intent
Force an early execution
Redirect an intent to another target

Example interactions:
Guard card pushes a Strike back 1 turn
Crumble release interrupts Channel
Freeze interrupts any enemy actions for X amount of turns
Shock propagates Burst damage to other enemies

4. Intent Accuracy & Trust

Enemy intent display must never lie.

Rules:
If an enemy intent changes, the UI must update immediately
No “fake” intent for surprise damage
Any hidden or uncertain intent must be explicitly marked as such
Player trust in intent = player willingness to engage strategically.

5. Visual & UX Constraints (No Heavy Animation)

This system must rely on:

Iconography
Timers
Pulses / ticks
Color shifts
Minor motion (shake, glow, slide)
No complex animations required.

6. Success Criteria
This change is successful if:
Players are making decisions based on enemy intent, not just damage math
The board state feels different turn-to-turn

The question becomes:
“Which threat do I answer now?”

Not:
“Which card deals the most damage?”

7. Threat vocabulary
What a Threat Vocabulary Is
A threat vocabulary is a small, reusable set of threat types that enemies express through intent, timing, and consequence.
Each enemy doesn’t invent new danger — it combines known threat words.
This lets the player:
Read the board instantly
Prioritize correctly
Feel smart instead of overwhelmed
The Hard Constraint (3–5 enemies, 10 waves)
This is crucial.
With many enemies and long fights:
Threats must stack cleanly
Threats must be readable in <1 second
Threats must not require inspecting each enemy deeply
So the system must:

Be iconic
Be layered
Be predictable but combinable

The Core Threat Axes (Foundation)
All threats fall into 5 axes.
Every enemy expresses 1 primary and optionally 1 secondary.

Axis 1: Time Pressure
“If I wait, things get worse.”
Examples:

Charging attacks
Escalating damage
Growing stacks
Countdown to release
UI cue: ticking ring, pulsing number

Axis 2: Punishment
“If I ignore this, I suffer.”
Examples:
On-hit debuffs
Resource drain
Lockouts
Permanent modifiers
UI cue: red warning icon, spike motifs

Axis 3: Disruption
“I interfere with your plan.”
Examples:
Delays
Cost increases
Preparation disruption
Stack inversion
UI cue: broken chains, static, fractures

Axis 4: Protection
“I make others harder to deal with.”
Examples:
Guards
Damage redirection
Resistance sharing
On-death buffs
UI cue: links, shields, halos

Axis 5: Volatility
“This can explode unexpectedly.”
Examples:
Retaliation
Death triggers
Stack detonations
Random targeting
UI cue: cracks, sparks, unstable glow

The Vocabulary: 12 Core Threat Words

These are keywords enemies use.
You should reuse these relentlessly.

1. Charging
Axis: Time Pressure
Will unleash a stronger effect next turn(s)
Stack-based countdown
Interruptible
Escalates visibly

2. Primed
Axis: Volatility
Something bad happens when condition is met
Examples:
“Primed: at 5 Poison”
“Primed: on death”

3. Anchoring
Axis: Protection
Buffs others while alive
Examples:
Shared armor
Reduced damage taken
Stack redirection

4. Leeching
Axis: Punishment
Converts your success into their gain
Examples:
Steal Focus
Convert your stacks
Heal on debuff

5. Suppressing
Axis: Disruption
Limits how you can play
Examples:
Skill costs more
Preparation reduced
Card draw locked

6. Escalating
Axis: Time Pressure
Grows every turn
Examples:
Damage +X per turn
Stack growth
Area corruption

7. Volatile
Axis: Volatility
Unstable, unpredictable damage source
Examples:
Explodes
Chains
Random targets

8. Retaliating
Axis: Punishment
Hitting it hurts you
Examples:
Thorns
Counter stacks
Reverse damage

9. Channeling
Axis: Time Pressure + Disruption
Locks itself into an action that warps the board
Examples:
Summoning zones
Locking cards
Charging AoE

10. Converting
Axis: Disruption
Turns one resource into another
Examples:
Poison → Damage
Crumble → Shield
Focus → Enemy buff

11. Shielded
Axis: Protection
Requires a specific answer
Examples:
Only breaks on release
Element-locked
Stack-gated

12. Terminal
Axis: Time Pressure + Volatility
If not answered, ends the fight (or you)
Examples:
Executes
Board wipe
Permanent corruption
Use sparingly.

Enemy Construction Rule (VERY IMPORTANT)
Each enemy:
1 Primary Threat Word
0–1 Secondary Threat Word
Never more.
Examples
Crumble Warden
Primary: Anchoring
Secondary: Escalating
Shares Crumble stacks among enemies, gains armor per turn

Venom Seer
Primary: Primed
Secondary: Converting
At 6 Poison, converts Poison into AoE damage

Shock Inquisitor
Primary: Suppressing
Secondary: Charging
Increases Skill costs, charges chain lightning

Wave Composition Rules (This Solves the 5-Enemy Problem)

Each wave should contain:

1 Pressure Threat (Charging / Escalating)
1 Disruptor or Punisher
0–1 Protector
Rest: Low-noise enemies

Never stack:
More than 1 Suppressing
More than 1 Terminal
More than 2 Time Pressure
This keeps cognition manageable.

Visual Language (Minimal Animation Required)
You don’t need animations.
Use:
Icon stacks
Countdown rings
Board highlights
Color-coded threat borders
The board should “look dangerous” without movement.

Player Learning Curve
Early game:
Enemies use 1 threat word
Mid game:
Enemies combine 2
Late game:
Waves combine synergies

Players don’t memorize enemies — they read sentences:
“That one is Charging + Anchoring → kill it now.”

---

Implementation Hooks (Live)

- EnemyData fields: primaryThreat, secondaryThreat
- Enemy instance stores primaryThreat / secondaryThreat at spawn
- ThreatAxis mapping: ThreatVocabulary.GetAxes(word)
- ThreatBehaviorTable: source of truth for word effects, hooks, and counters