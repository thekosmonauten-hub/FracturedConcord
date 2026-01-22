# Combat Responsiveness Implementation Plan

## Overview
This document provides a structured implementation plan to transform combat from deterministic/passive to responsive, readable, and interruptible. The goal is to make players engage with enemy intent strategically rather than just optimizing damage math.

---

## Core Problems Identified
1. **Combat feels deterministic**: "I already know the right play. I just click cards and enemy HP goes down."
2. **Enemy intent is shallow**: Not always up-to-date, not reactive, not meaningfully interactable
3. **Lack of visual/audio feedback**: Systems are deep but under-communicated

---

## Implementation Phases

### Phase 1: Intent Queue System (Foundation)
**Goal**: Transform single-action intents into a mutable queue system

**Priority**: CRITICAL - Everything else depends on this

### Phase 2: Threat Vocabulary Integration
**Goal**: Implement the 12 core threat words and integrate them into enemy design

**Priority**: HIGH - Core to making threats readable

### Phase 3: Reactive Combat Hooks
**Goal**: Allow player actions to interact with enemy intents (delay, remove, weaken, redirect)

**Priority**: HIGH - Makes combat interactive

### Phase 4: Visual Feedback (Combat Juice)
**Goal**: Implement visual/audio feedback to communicate system depth

**Priority**: MEDIUM - Improves feel but systems can work without it initially

### Phase 5: Intent Accuracy & Trust
**Goal**: Ensure intent display is always accurate and updates immediately

**Priority**: CRITICAL - Player trust is essential

---

## Success Criteria
- Players make decisions based on enemy intent, not just damage math
- Board state feels different turn-to-turn
- The question becomes: "Which threat do I answer now?" (not "Which card deals the most damage?")
- Combat feels responsive and interactive
- Visual feedback clearly communicates all state changes

---

## Technical Constraints
- Must support 3-5 enemies per wave, up to 10 waves
- No heavy animation work required (use iconography, timers, pulses, color shifts, minor motion)
- Intent display must never lie
- System must be readable in <1 second per enemy

---

## Related Documents
- `Threat vocabulary.md` - Detailed threat vocabulary system design
- `Combat Juice checklist.md` - Visual/audio feedback requirements
