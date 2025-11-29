# Joreg Dialogue Setup Guide

## Dialogue Flow Structure

Based on the dialogue flow, here's how to set up the DialogueData asset:

### Node Structure

```
start (Initial greeting)
├── Choice 1: "Who are you?" → who_are_you
├── Choice 2: "What do you do here?" → what_do_you_do
├── Choice 3: "The Peacekeepers?" → peacekeepers (CONDITION: who_are_you seen)
└── Choice 4: "Warrants?" → warrants (CONDITION: what_do_you_do seen)

who_are_you (Response to "Who are you?")
└── Returns to: start (with who_are_you marked as seen)

what_do_you_do (Response to "What do you do here?")
└── Returns to: start (with what_do_you_do marked as seen)

peacekeepers (Response to "The Peacekeepers?")
└── Returns to: start

warrants (Response to "Warrants?")
└── Continues to: warrant_tutorial_start
```

## Step-by-Step Setup in Unity

### 1. Create DialogueData Asset

1. Right-click in Project → Create → Dexiled → Dialogue → Dialogue Data
2. Name it: `JoregDialogue`
3. Set:
   - `dialogueId`: "joreg_intro"
   - `dialogueName`: "Joreg the Sealwright - Introduction"
   - `npcId`: "joreg" or "peacekeeper"
   - `startNodeId`: "start"

### 2. Create Nodes

#### Node 1: "start" (Initial Greeting)

- **nodeId**: `start`
- **speakerName**: `Joreg`
- **dialogueText**: 
```
"...Close the door, would you? Last thing I need is another malformed beast sniffing its way in here.
Hnh. You don't look corrupted yet. That's already better than most travelers these days."
```
- **speakerPortrait**: [Assign Joreg's portrait sprite]
- **Choices** (4 choices):
  1. **choiceText**: `"Who are you?"`
     - **targetNodeId**: `who_are_you`
     - **condition**: None
  2. **choiceText**: `"What do you do here?"`
     - **targetNodeId**: `what_do_you_do`
     - **condition**: None
  3. **choiceText**: `"The Peacekeepers?"`
     - **targetNodeId**: `peacekeepers`
     - **condition**: 
       - **conditionType**: `DialogueNodeSeen`
       - **conditionValue**: `who_are_you`
  4. **choiceText**: `"Warrants?"`
     - **targetNodeId**: `warrants`
     - **condition**:
       - **conditionType**: `DialogueNodeSeen`
       - **conditionValue**: `what_do_you_do`

#### Node 2: "who_are_you" (Response to Option 1)

- **nodeId**: `who_are_you`
- **speakerName**: `Joreg`
- **dialogueText**:
```
"Name's Joreg. Joreg the Sealwright.
Used to be I worked in a proper Peacekeeper Hall — marble floors, golden seals, the whole parade.
Now I'm in a shack held together by stubbornness and whatever the Law hasn't forgotten yet."

"I'm what's left of the Sealwrights — we made the Warrants, forged them, recorded them, maintained them.
These days? I mostly just try not to die. Or explode. Or collapse into entropy.
Depends on the weather."

"Anyway. That's me. Congratulations — you've found the last sane Peacekeeper within fifty leagues."
```
- **Choices** (1 choice):
  1. **choiceText**: `"Continue"`
     - **targetNodeId**: `start`
     - **action** (on choice):
       - **actionType**: `MarkNodeSeen`
       - **actionValue**: `who_are_you`

#### Node 3: "what_do_you_do" (Response to Option 2)

- **nodeId**: `what_do_you_do`
- **speakerName**: `Joreg`
- **dialogueText**:
```
"What do I do?
Well, let's see… I sweep out corrupted dust, patch reality leaks, argue with a teapot that keeps trying to phase out of existence…
And occasionally, when I'm feeling optimistic, I help wandering fools like you not get immediately devoured."

"But really?"
He sighs.
"I watch the world fall apart one loose thread at a time and try not to take it personally."

"My real job — what's left of it — is working the Warrants. They're the only thing keeping us from slipping completely into the Unraveler's jaws.
And like it or not, kid… I think you're about to need one."
```
- **Choices** (1 choice):
  1. **choiceText**: `"Continue"`
     - **targetNodeId**: `start`
     - **action** (on choice):
       - **actionType**: `MarkNodeSeen`
       - **actionValue**: `what_do_you_do`

#### Node 4: "peacekeepers" (Response to Option 3)

- **nodeId**: `peacekeepers`
- **speakerName**: `Joreg`
- **dialogueText**:
```
"Ah. The Peacekeepers.
Once upon a time, we enforced the Concordial Law — the rules that kept the world tidy. Beasts stayed beasts. Dead stayed dead. Realms stayed separate."

"We had ranks, halls, authority. Justicars keeping order, Arbiters of Law, Sealsmiths forging artifacts that could bend a storm or seal a demon gate with a whisper."

"Now?"
He gestures around the decrepit shack.
"Well… now the Peacekeepers are mostly mold, ruins, or snacks for the malformed."

"Our spells don't work right anymore. Reality doesn't listen when we speak. But it'll still listen to someone like you — someone the Law hasn't learned to ignore yet."

"So we Peacekeepers do what we can: hide, hold the line, and hand out Warrants to anyone still breathing.
Not glorious work, but it beats being dead. Barely."
```
- **Choices** (1 choice):
  1. **choiceText**: `"Continue"`
     - **targetNodeId**: `start`

#### Node 5: "warrants" (Response to Option 4)

- **nodeId**: `warrants`
- **speakerName**: `Joreg`
- **dialogueText**:
```
"Warrants, yes. Half spell, half legal contract — the old kind, forged before the Cataclysm scrambled everything. The Law still recognizes them."

"See, I can't cast a stabilizing spell anymore. Try it and I'd tear a hole clean through the floorboards. Or through myself. Or both.
But a Warrant? That channels the Law through you, not me. Safer. Slightly."

"They strengthen you, attune you to the old order, keep the chaos from chewing on your bones.
And they grow with use — the more potential you show, the more they unlock."

"Here. You're going to need these."
```
- **Choices** (1 choice):
  1. **choiceText**: `"Continue"`
     - **targetNodeId**: `warrant_tutorial_start`
     - **action** (on choice):
       - **actionType**: `OpenPanel` (or `Custom` for tutorial trigger)
       - **actionValue**: `WarrantTutorial` or `PeacekeepersFaction`

#### Node 6: "warrant_tutorial_start" (After warrants given)

- **nodeId**: `warrant_tutorial_start`
- **speakerName**: `Joreg`
- **dialogueText**:
```
"Don't get excited. These are baseline Seals — they won't show their true strength until you prove you've got the potential to handle them.
Think of them like… keys to doors you haven't reached yet."
```
- **Choices**: None (end node - closes dialogue)
- **onEnterAction**:
  - **actionType**: `GiveItem` (or custom action to give warrants)
  - **actionValue**: `warrant_starter_pack` (or similar)
  - **intValue**: `3`

#### Node 7: "after_tutorial" (Optional - for when player returns)

- **nodeId**: `after_tutorial`
- **speakerName**: `Joreg`
- **dialogueText**:
```
"Well… you didn't explode. That's a promising sign."

"You can keep those Warrants. They should stabilize around your… unique kind of recklessness."

"Just remember: the more you push yourself, the more they'll open up.
And with the state of the world, you'll be doing plenty of pushing."

"Now get going. This shack barely has room for one doomed soul, let alone two."
```
- **Choices**: None (end node)

## How Conditional Unlocks Work

1. **Initial State**: Only "Who are you?" and "What do you do here?" are visible
2. **After "Who are you?"**: 
   - Node `who_are_you` is marked as seen
   - "The Peacekeepers?" choice becomes visible (condition checks if `who_are_you` is in seenNodeIds)
3. **After "What do you do here?"**:
   - Node `what_do_you_do` is marked as seen
   - "Warrants?" choice becomes visible (condition checks if `what_do_you_do` is in seenNodeIds)

## Testing

1. Start dialogue with Joreg
2. Verify only 2 choices are visible initially
3. Click "Who are you?" → should see response → click Continue → returns to start
4. Verify "The Peacekeepers?" is now visible
5. Click "What do you do here?" → should see response → click Continue → returns to start
6. Verify "Warrants?" is now visible
7. Click "Warrants?" → should continue to warrant tutorial flow

## Notes

- The `seenNodeIds` HashSet persists during the dialogue session
- If you want to reset seen nodes when dialogue closes, add `seenNodeIds.Clear()` in `EndDialogue()`
- Node IDs must match exactly between `conditionValue` and actual node IDs
- The `MarkNodeSeen` action is executed when the choice is selected, so the node is marked before returning to start


