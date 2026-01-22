# Combat Responsiveness Implementation Checklist

Use this checklist to track implementation progress. Check off items as you complete them.

---

## Phase 1: Intent Queue System (Foundation)

### 1.1 Core Data Structures
- [ ] Create `EnemyIntent` class/struct with:
  - [x] Type (Strike, Channel, Summon, Debuff, etc.)
  - [x] Timing (Turns until execution)
  - [x] Threat Tier (Minor / Major / Lethal)
  - [ ] Tags (Physical, Fire, Channel, Interruptible, etc.)
  - [x] Damage/Effect values
  - [ ] Target information

- [ ] Create `IntentQueue` class/component for enemies:
  - [x] Queue storage (1-3 intents)
  - [x] Insert intent method
  - [x] Remove intent method
  - [x] Delay intent method (increment timing)
  - [ ] Upgrade/Downgrade intent method
  - [x] Get next intent method
  - [x] Clear queue method

- [x] Integrate IntentQueue into `Enemy.cs`:
  - [x] Replace single `currentIntent` with `IntentQueue`
  - [x] Update `SetIntent()` to populate queue
  - [x] Ensure queue updates on turn start/end

### 1.2 Intent Generation
- [x] Modify enemy intent generation to create 1-3 intents ahead
- [x] Ensure intents respect enemy AI patterns
- [ ] Add logic to prevent impossible intent combinations
- [ ] Test with various enemy types

### 1.3 Queue Management
- [x] Implement queue advancement (remove executed intent, shift remaining)
- [x] Handle queue updates when intents are modified
- [x] Ensure queue persists across turns correctly
- [ ] Add validation to prevent empty queues

---

## Phase 2: Threat Vocabulary Integration

### 2.1 Threat Word System
- [ ] Create `ThreatWord` enum/class with 12 core words:
  - [ ] Charging
  - [ ] Primed
  - [ ] Anchoring
  - [ ] Leeching
  - [ ] Suppressing
  - [ ] Escalating
  - [ ] Volatile
  - [ ] Retaliating
  - [ ] Channeling
  - [ ] Converting
  - [ ] Shielded
  - [ ] Terminal

- [ ] Create `ThreatAxis` enum (5 axes):
  - [ ] Time Pressure
  - [ ] Punishment
  - [ ] Disruption
  - [ ] Protection
  - [ ] Volatility

- [ ] Link threat words to axes
- [ ] Create threat word data structure (ScriptableObject or class)

### 2.2 Enemy Threat Assignment
- [ ] Add threat word fields to enemy data:
  - [ ] Primary Threat Word (required)
  - [ ] Secondary Threat Word (optional, 0-1)
  
- [ ] Update enemy creation/loading to assign threats
- [ ] Ensure no enemy has more than 2 threat words
- [ ] Create example enemies with threat combinations:
  - [ ] Crumble Warden (Anchoring + Escalating)
  - [ ] Venom Seer (Primed + Converting)
  - [ ] Shock Inquisitor (Suppressing + Charging)

### 2.3 Intent-to-Threat Mapping
- [ ] Map intent types to threat words
- [ ] Ensure intents reflect assigned threat words
- [ ] Add threat word tags to intents
- [ ] Create visual indicators for threat words

### 2.4 Wave Composition Rules
- [ ] Implement wave composition validation:
  - [ ] Max 1 Suppressing enemy per wave
  - [ ] Max 1 Terminal enemy per wave
  - [ ] Max 2 Time Pressure enemies per wave
  - [ ] Each wave should have 1 Pressure Threat
  - [ ] Each wave should have 1 Disruptor or Punisher
  - [ ] 0-1 Protector per wave

- [ ] Add wave generation logic that respects these rules
- [ ] Test wave generation with various configurations

---

## Phase 3: Reactive Combat Hooks

### 3.1 Intent Manipulation System
- [ ] Create `IntentManipulator` class/interface:
  - [ ] `DelayIntent(Enemy, int turns)` method
  - [ ] `RemoveIntent(Enemy, IntentType)` method
  - [ ] `WeakenIntent(Enemy, float multiplier)` method
  - [ ] `ForceEarlyExecution(Enemy)` method
  - [ ] `RedirectIntent(Enemy, newTarget)` method

- [ ] Integrate into combat system
- [ ] Ensure all manipulations update UI immediately

### 3.2 Card Effect Integration
- [ ] Add intent manipulation to card effects:
  - [ ] Guard card pushes Strike back 1 turn
  - [ ] Crumble release interrupts Channel
  - [ ] Freeze interrupts enemy actions for X turns
  - [ ] Shock propagates Burst damage to other enemies

- [ ] Create card effect modifiers for intent manipulation
- [ ] Test each card type with intent interactions

### 3.3 Status Effect Integration
- [ ] Update status effects to interact with intents:
  - [ ] Freeze: Clear/delay all intents
  - [ ] Stun: Delay next intent
  - [ ] New status: Intent disruption effects

- [ ] Ensure status effects update intent queue
- [ ] Test status effect + intent interactions

### 3.4 Ability Integration
- [ ] Update enemy abilities to respect intent manipulation
- [ ] Ensure abilities can be interrupted
- [ ] Test ability cancellation scenarios

---

## Phase 4: Visual Feedback (Combat Juice)

### 4.1 Action Confirmation
- [ ] Card snap/compress on play
- [ ] Mana visibly ticks down before effects resolve
- [ ] Sound/pitch shift unique to card type (Attack/Skill/Guard)
- [ ] Brief screen-space text/icon flash ("Crumble +2", "Shock!")
- [ ] Enemy intent icons react (shake/dim/update)

### 4.2 Impact Readability
- [ ] Status stacks visibly increment (count + pulse)
- [ ] Threat countdowns update with emphasis
- [ ] Affected enemies get 0.2-0.3s glow or outline
- [ ] Buffs/debuffs float from source to target
- [ ] Board quiets for ~100ms after resolution (micro-pause)

### 4.3 Directionality
- [ ] Effects move from card → target
- [ ] AoE ripples outward, not instant pop
- [ ] Chain effects show hop indicators
- [ ] Resource gains flow back to player UI
- [ ] Consumed stacks collapse inward visually

### 4.4 Escalation Feedback
- [ ] Stacks feel heavier at higher counts (bigger pulse/glow)
- [ ] Thresholds visually hinted (cracks, sparks, warning color)
- [ ] "Almost there" states look different from empty
- [ ] Release effects are louder/brighter than buildup
- [ ] UI subtly anticipates release (shimmer/tension)

### 4.5 Threat Presence
- [ ] Threat icons always visible
- [ ] Charging enemies visually "tick"
- [ ] Terminal threats override other visuals
- [ ] Suppression visibly distorts UI elements
- [ ] Ignoring a threat makes the board uglier

### 4.6 Decision Weight
- [ ] Different card orders visibly differ
- [ ] Cancelled/interrupted threats react distinctly
- [ ] Overkill produces excess feedback (shatter, overflow)
- [ ] Wasted effects look sad (fizzle, dim)
- [ ] Perfect answers look clean and satisfying

### 4.7 Turn Transitions
- [ ] Turn start has subtle "breath in"
- [ ] Enemy intent refresh is noticeable
- [ ] End turn locks input briefly
- [ ] Delayed effects announce themselves
- [ ] Wave transitions reset visual noise

### 4.8 Resource Emotion Mapping
- [ ] Mana feels liquid (flow, drain)
- [ ] Aggression feels sharp (spikes, red flashes)
- [ ] Focus feels clean (blue, smooth motion)
- [ ] Potential feels unstable (flicker, distortion)
- [ ] Preparation feels tense (tightening rings)

### 4.9 Micro-Reward Frequency
- [ ] Minor procs have minor feedback
- [ ] Big synergies have stacked feedback
- [ ] Rare triggers are unmistakable
- [ ] Combos chain feedback naturally
- [ ] Non-damage plays still feel rewarding

### 4.10 Failure Feedback
- [ ] Cause of damage is highlighted
- [ ] Final blow references threat vocabulary
- [ ] Post-combat summary shows "missed answers"
- [ ] Last turn is readable in hindsight
- [ ] Failure looks intentional, not random

---

## Phase 5: Intent Accuracy & Trust

### 5.1 Real-Time Intent Updates
- [x] Intent queue updates immediately when modified
- [x] UI refreshes on every intent change
- [x] No delayed or batched updates
- [ ] Test rapid intent modifications

### 5.2 Intent Validation
- [ ] Validate intents are always achievable
- [ ] Prevent "fake" intents for surprise damage
- [ ] Mark uncertain/hidden intents explicitly
- [ ] Add intent accuracy logging for debugging

### 5.3 UI Trust Indicators
- [ ] Clear visual distinction between certain/uncertain intents
- [ ] Show intent confidence level if applicable
- [ ] Ensure intent display never lies
- [ ] Test edge cases (enemy death, status changes, etc.)

### 5.4 Intent History
- [ ] Log intent changes for debugging
- [ ] Show intent change reasons (optional, for dev)
- [ ] Ensure intent queue state is always consistent

---

## Phase 6: UI/UX Implementation

### 6.1 Intent Queue Display
- [x] Create UI component for intent queue (1-3 intents)
- [ ] Show intent type icon
- [ ] Show timing (turns until execution)
- [ ] Show threat tier (color coding: Minor/Major/Lethal)
- [ ] Show tags visually
- [x] Update display in real-time

### 6.2 Threat Word Visualization
- [ ] Create icons for each threat word
- [ ] Display threat words on enemy UI
- [ ] Color-code by threat axis
- [ ] Ensure readability in <1 second

### 6.3 Intent Queue Interactions
- [ ] Show which intents can be manipulated
- [ ] Highlight manipulable intents on hover
- [ ] Show preview of manipulation result
- [ ] Animate intent queue changes

### 6.4 Visual Language (Minimal Animation)
- [ ] Icon stacks for multiple intents
- [ ] Countdown rings for timing
- [ ] Board highlights for active threats
- [ ] Color-coded threat borders
- [ ] Ensure board "looks dangerous" without movement

---

## Phase 7: Testing & Polish

### 7.1 System Testing
- [ ] Test intent queue with 3-5 enemies
- [ ] Test across 10 waves
- [ ] Test intent manipulation edge cases
- [ ] Test threat word combinations
- [ ] Test wave composition rules

### 7.2 Performance Testing
- [ ] Ensure intent updates don't cause frame drops
- [ ] Test with maximum enemy count
- [ ] Profile intent queue operations
- [ ] Optimize if needed

### 7.3 Playtesting
- [ ] Playtest early game (1 threat word enemies)
- [ ] Playtest mid game (2 threat word enemies)
- [ ] Playtest late game (synergistic waves)
- [ ] Verify "Which threat do I answer now?" decision-making
- [ ] Verify combat feels responsive and interactive

### 7.4 Balance Testing
- [ ] Ensure threat combinations are fair
- [ ] Test intent manipulation balance
- [ ] Verify wave difficulty scaling
- [ ] Check threat word distribution

---

## Priority Implementation Order

### Must-Have (MVP)
1. Phase 1: Intent Queue System (Foundation)
2. Phase 5: Intent Accuracy & Trust
3. Phase 3: Reactive Combat Hooks (basic: delay, remove)
4. Phase 6: UI/UX Implementation (basic queue display)

### Should-Have (Core Experience)
5. Phase 2: Threat Vocabulary Integration
6. Phase 3: Reactive Combat Hooks (full: weaken, redirect, force early)
7. Phase 4: Visual Feedback (Solo-Dev Rule of Thumb - 5 key items)

### Nice-to-Have (Polish)
8. Phase 4: Visual Feedback (full checklist)
9. Phase 7: Testing & Polish (extensive)

---

## Solo-Dev Quick Wins (If Time Limited)

If you only implement 5 things for immediate impact:
1. Stack pulses
2. Threat countdown ticks
3. Resource flow motion
4. Micro-pauses after actions
5. Strong sound design per card type

These alone can double perceived fun.

---

## Notes

- **Enemy Construction Rule**: Each enemy = 1 Primary Threat Word + 0-1 Secondary Threat Word. Never more.
- **Wave Composition**: Never stack more than 1 Suppressing, 1 Terminal, or 2 Time Pressure enemies.
- **Visual Constraints**: No heavy animations required. Use iconography, timers, pulses, color shifts, minor motion.
- **Player Learning**: Early game = 1 threat word, Mid game = 2 threat words, Late game = synergistic waves.
- **Trust is Critical**: Intent display must never lie. Player trust = player willingness to engage strategically.

---

## Progress Tracking

**Current Status**: Active Implementation (Phases 1, 5, 6 in progress)
**Last Updated**: [Date]
**Next Milestone**: [Milestone Name]

**Completed Phases**: 
- [x] Phase 1
- [ ] Phase 2
- [ ] Phase 3
- [ ] Phase 4
- [x] Phase 5
- [x] Phase 6
- [ ] Phase 7

**Blockers**: [List any blockers here]
