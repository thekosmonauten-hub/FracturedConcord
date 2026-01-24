# Combat Responsiveness Documentation

This folder contains the complete implementation plan for making combat feel more responsive, readable, and interactive.

## Quick Start

1. **Read First**: `Combat Responsiveness Implementation Plan.md` - Overview of the system and phases
2. **Track Progress**: `Implementation Checklist.md` - Detailed checklist for implementation
3. **Reference**: 
   - `Threat vocabulary.md` - Threat vocabulary system design
   - `Combat Juice checklist.md` - Visual/audio feedback requirements

## Documentation Structure

### 📋 Implementation Plan
**File**: `Combat Responsiveness Implementation Plan.md`

High-level overview of the implementation:
- Core problems identified
- 7 implementation phases
- Success criteria
- Technical constraints
- Priority order

### ✅ Implementation Checklist
**File**: `Implementation Checklist.md`

Detailed, actionable checklist organized by phase:
- Phase 1: Intent Queue System (Foundation)
- Phase 2: Threat Vocabulary Integration
- Phase 3: Reactive Combat Hooks
- Phase 4: Visual Feedback (Combat Juice)
- Phase 5: Intent Accuracy & Trust
- Phase 6: UI/UX Implementation
- Phase 7: Testing & Polish

**Use this to track your progress!**

### 🎯 Threat Vocabulary
**File**: `Threat vocabulary.md`

Complete design document for the threat vocabulary system:
- 5 Threat Axes (Time Pressure, Punishment, Disruption, Protection, Volatility)
- 10 Core Threat Words (Charging, Anchoring, Leeching, etc.)
- Enemy construction rules
- Wave composition rules
- Visual language guidelines

### 🎨 Combat Juice Checklist
**File**: `Combat Juice checklist.md`

Visual and audio feedback requirements:
- 10 categories of feedback
- Action confirmation
- Impact readability
- Directionality
- Escalation feedback
- Threat presence
- And more...

## Implementation Priority

### Must-Have (MVP)
1. ✅ Phase 1: Intent Queue System
2. ✅ Phase 5: Intent Accuracy & Trust
3. ✅ Phase 3: Reactive Combat Hooks (basic)
4. ✅ Phase 6: UI/UX Implementation (basic)

### Should-Have (Core Experience)
5. ✅ Phase 2: Threat Vocabulary Integration
6. ✅ Phase 3: Reactive Combat Hooks (full)
7. ✅ Phase 4: Visual Feedback (5 key items)

### Nice-to-Have (Polish)
8. ✅ Phase 4: Visual Feedback (full checklist)
9. ✅ Phase 7: Testing & Polish

## Solo-Dev Quick Wins

If time is limited, implement these 5 things for immediate impact:
1. Stack pulses
2. Threat countdown ticks
3. Resource flow motion
4. Micro-pauses after actions
5. Strong sound design per card type

**These alone can double perceived fun.**

## Key Principles

1. **Intent Never Lies**: Enemy intent display must always be accurate and update immediately
2. **Threat Vocabulary**: Each enemy = 1 Primary Threat Word + 0-1 Secondary (never more)
3. **Wave Composition**: Never stack more than 1 Suppressing, 1 Terminal, or 2 Time Pressure enemies
4. **Visual Constraints**: No heavy animations - use iconography, timers, pulses, color shifts, minor motion
5. **Communication Over Mechanics**: You're not making a boring game - you're under-communicating

## Success Criteria

The system is successful when:
- Players make decisions based on enemy intent, not just damage math
- Board state feels different turn-to-turn
- The question becomes: **"Which threat do I answer now?"**
- Not: **"Which card deals the most damage?"**

## Getting Started

1. Review the Implementation Plan to understand the scope
2. Start with Phase 1 (Intent Queue System) - it's the foundation
3. Use the Implementation Checklist to track progress
4. Reference Threat Vocabulary when designing enemies
5. Use Combat Juice Checklist when implementing feedback

## Notes

- All systems must support 3-5 enemies per wave, up to 10 waves
- Threats must be readable in <1 second per enemy
- Player learning curve: Early = 1 threat word, Mid = 2 threat words, Late = synergistic waves
- Trust is critical: Intent display must never lie

---

**Last Updated**: [Update this when you make changes]  
**Current Status**: Planning Phase  
**Next Milestone**: Phase 1 - Intent Queue System
