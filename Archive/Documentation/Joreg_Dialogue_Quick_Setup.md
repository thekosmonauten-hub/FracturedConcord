# Joreg Dialogue - Quick Setup Reference

## Dialogue Flow Summary

**Initial State:**
- Start node shows 4 choices, but only 2 are visible:
  - ✅ "Who are you?" (always visible)
  - ✅ "What do you do here?" (always visible)
  - ❌ "The Peacekeepers?" (hidden until "Who are you?" is seen)
  - ❌ "Warrants?" (hidden until "What do you do here?" is seen)

**After "Who are you?":**
- Shows response → returns to start
- "The Peacekeepers?" becomes visible ✅

**After "What do you do here?":**
- Shows response → returns to start
- "Warrants?" becomes visible ✅

## Node Configuration in Unity

### Node 1: "start"

**Basic Info:**
- **nodeId**: `start`
- **speakerName**: `Joreg`
- **dialogueText**: 
```
"...Close the door, would you? Last thing I need is another malformed beast sniffing its way in here.
Hnh. You don't look corrupted yet. That's already better than most travelers these days."
```

**Choices (Add 4 choices):**

1. **Choice 1:**
   - **choiceText**: `"Who are you?"`
   - **targetNodeId**: `who_are_you`
   - **condition**: None (leave empty)

2. **Choice 2:**
   - **choiceText**: `"What do you do here?"`
   - **targetNodeId**: `what_do_you_do`
   - **condition**: None (leave empty)

3. **Choice 3:**
   - **choiceText**: `"The Peacekeepers?"`
   - **targetNodeId**: `peacekeepers`
   - **condition**: 
     - **conditionType**: `DialogueNodeSeen`
     - **conditionValue**: `who_are_you`

4. **Choice 4:**
   - **choiceText**: `"Warrants?"`
   - **targetNodeId**: `warrants`
   - **condition**:
     - **conditionType**: `DialogueNodeSeen`
     - **conditionValue**: `what_do_you_do`

---

### Node 2: "who_are_you"

**Basic Info:**
- **nodeId**: `who_are_you`
- **speakerName**: `Joreg`
- **dialogueText**: 
```
"Name's Joreg. Joreg the Sealwright.
Used to be I worked in a proper Peacekeeper Hall — marble floors, golden seals, the whole parade.
Now I'm in a shack held together by stubbornness and whatever the Law hasn't forgotten yet..\n\n"

"I'm what's left of the Sealwrights — we made the Warrants, forged them, recorded them, maintained them.
These days? I mostly just try not to die. Or explode. Or collapse into entropy.
Depends on the weather..\n\n"

"Anyway. That's me. Congratulations — you've found the last sane Peacekeeper within fifty leagues."
```

**Choices (Add 1 choice):**
- **choiceText**: `"Continue"`
- **targetNodeId**: `start`
- **action** (on the choice):
  - **actionType**: `MarkNodeSeen`
  - **actionValue**: `who_are_you`

**Note:** The node is automatically marked as seen when shown, but the action ensures it's explicitly marked when the choice is selected.

---

### Node 3: "what_do_you_do"

**Basic Info:**
- **nodeId**: `what_do_you_do`
- **speakerName**: `Joreg`
- **dialogueText**: 
```
What do I do?
Well, let's see… I sweep out corrupted dust, patch reality leaks, argue with a teapot that keeps trying to phase out of existence…
And occasionally, when I'm feeling optimistic, I help wandering fools like you not get immediately devoured.\n\n

But really?\n\n
He sighs.
I watch the world fall apart one loose thread at a time and try not to take it personally.\n\n

My real job — what's left of it — is working the Warrants. They're the only thing keeping us from slipping completely into the Unraveler's jaws.
And like it or not, kid… I think you're about to need one.
```

**Choices (Add 1 choice):**
- **choiceText**: `"Continue"`
- **targetNodeId**: `start`
- **action** (on the choice):
  - **actionType**: `MarkNodeSeen`
  - **actionValue**: `what_do_you_do`

---

### Node 4: "peacekeepers"

**Basic Info:**
- **nodeId**: `peacekeepers`
- **speakerName**: `Joreg`
- **dialogueText**: 
```
Ah. The Peacekeepers.
Once upon a time, we enforced the Concordial Law — the rules that kept the world tidy. Beasts stayed beasts. Dead stayed dead. Realms stayed separate.\n\n

We had ranks, halls, authority. Justicars keeping order, Arbiters of Law, Sealsmiths forging artifacts that could bend a storm or seal a demon gate with a whisper.\n\n

Now? ** He gestures around the decrepit shack.** \n\n
"Well… now the Peacekeepers are mostly mold, ruins, or snacks for the malformed.\n\n"

"Our spells don't work right anymore. Reality doesn't listen when we speak. But it'll still listen to someone like you — someone the Law hasn't learned to ignore yet.\n\n"

"So we Peacekeepers do what we can: hide, hold the line, and hand out Warrants to anyone still breathing.
Not glorious work, but it beats being dead. Barely."
```

**Choices (Add 1 choice):**
- **choiceText**: `"Continue"`
- **targetNodeId**: `start`
- **action**: None

---

### Node 5: "warrants"

**Basic Info:**
- **nodeId**: `warrants`
- **speakerName**: `Joreg`
- **dialogueText**: 
```
"Warrants, yes. Half spell, half legal contract — the old kind, forged before the Shattering of Law scrambled everything. The Law still recognizes them.\n\n"

"See, I can't cast a stabilizing spell anymore. Try it and I'd tear a hole clean through the floorboards. Or through myself. Or both.
But a Warrant? That channels the Law through you, not me. Safer. Slightly.\n\n"

"They strengthen you, attune you to the old order, keep the chaos from chewing on your bones.
And they grow with use — the more potential you show, the more they unlock.\n\n"

"Here. You're going to need these."
```

**Choices (Add 1 choice):**
- **choiceText**: `"Continue"`
- **targetNodeId**: `warrant_tutorial_start`
- **action** (on the choice):
  - **actionType**: `OpenPanel` (or `Custom` for tutorial)
  - **actionValue**: `WarrantTutorial` or `PeacekeepersFaction`

---

### Node 6: "warrant_tutorial_start" (Optional - for tutorial flow)

**Basic Info:**
- **nodeId**: `warrant_tutorial_start`
- **speakerName**: `Joreg`
- **dialogueText**: 
```
"Don't get excited. These are baseline Seals — they won't show their true strength until you prove you've got the potential to handle them.
Think of them like… keys to doors you haven't reached yet."
```

**Choices**: None (end node - closes dialogue)

**onEnterAction** (optional - to give warrants):
- **actionType**: `GiveItem` (or `Custom`)
- **actionValue**: `warrant_starter_pack`
- **intValue**: `3`

---

## Important Notes

1. **Node IDs must match exactly:**
   - The `conditionValue` in choices must match the `nodeId` of the node you want to check
   - Example: Condition checking for `who_are_you` must have `conditionValue = "who_are_you"`

2. **Automatic Marking:**
   - Nodes are automatically marked as seen when they're displayed
   - The `MarkNodeSeen` action on choices is redundant but explicit (good for clarity)

3. **Condition Evaluation:**
   - Conditions are evaluated when displaying choices
   - Choices that don't meet conditions won't appear in the UI

4. **Returning to Start:**
   - All response nodes return to `start` to show the updated choice list
   - The start node will automatically show/hide choices based on what's been seen

## Testing Checklist

- [ ] Start dialogue - only 2 choices visible
- [ ] Click "Who are you?" → see response → click Continue → return to start
- [ ] Verify "The Peacekeepers?" is now visible (3 choices total)
- [ ] Click "What do you do here?" → see response → click Continue → return to start
- [ ] Verify "Warrants?" is now visible (4 choices total)
- [ ] Click "The Peacekeepers?" → see response → returns to start
- [ ] Click "Warrants?" → see response → continues to tutorial (if set up)


