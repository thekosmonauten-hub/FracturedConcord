BestPracticeCardHandling

Deckbuilder Card Resolution & Animation
Best Practice Guide

Purpose
Prevent card effects from failing, desyncing, or being canceled when cards are played quickly.
Ensure the game remains deterministic, fast, and robust regardless of animation speed.

1. Core Rule (Non-Negotiable)

Game logic must never depend on animation state.
Animations are a visualization of resolved actions — never the driver.

If disabling all animations causes gameplay to break, the architecture is incorrect.

2. Card Play Is a Multi-Stage Process

Playing a card is not a single action.
It is a pipeline with clearly separated responsibilities.

Correct Conceptual Flow
Player Input
→ Validation
→ Resource Spend
→ Action Queued
→ Logic Resolution
→ Visual Feedback
→ Cleanup (discard, exhaust, etc.)

Incorrect Flow
Player Input
→ Animation
→ OnAnimationComplete → Apply Effect


Animations must never gate logic execution.

3. Action Queue Architecture (Required)

Every card play creates an Action object that is pushed into a central queue.

Example Structure
class CardAction {
    CardData card;
    TargetData targets;
    bool resolved;
}

Processing Loop
while (ActionQueue.HasNext()) {
    ResolveNextAction();
}

Why This Matters

Allows fast card play

Prevents animation interruption bugs

Guarantees deterministic resolution

Enables future features (rewind, replay, speed control)

4. Resource Spending Timing
Always Spend Resources Immediately

Mana

Charges

Reliance

Cooldowns

Resources are deducted before animations play.

If an effect later fails, the bug is visible — not hidden.

5. Card State Model

Each card should move through explicit states:

InHand
→ Queued
→ Resolving
→ Resolved
→ Discarded / Exhausted

Rules

Cards in Queued or Resolving cannot be interacted with

State transitions are logical, not visual

Animations do not change state — they reflect it

6. Animation Rules (Critical)
6.1 Animations Are Fire-and-Forget

Animations:

May be skipped

May be sped up

May be interrupted

Game logic must already be resolved.

Never Do This
PlayDiscardAnimation(() => ResolveEffect());

Always Do This
ResolveEffect();
PlayDiscardAnimation();

7. Visual Cards vs Logical Cards
Separation of Concerns

Logical Card

Exists in deck/hand systems

Holds rules and effects

Can be destroyed or pooled instantly

Visual Card

A temporary representation

Used only for animation

Safe to cancel or skip

Best Practice

Snapshot or clone the card visual when played

Animate the clone

Immediately move the real card to discard logic-wise

8. Handling Fast Play & Spam Clicking
Recommended Approaches

Option A — Soft Queue (Preferred)

Allow unlimited rapid inputs

Queue all actions

Resolve one action per tick/frame

Option B — Micro Input Lock

Lock input for 0.05–0.1 seconds per card

Feels instant

Prevents visual overlap chaos

Never hard-lock the player during animations.

9. Debug & Validation Tools (Strongly Recommended)
Animation Kill Switch

Add a debug option:

DisableAllAnimations = true;


If gameplay breaks when animations are disabled:
❌ Logic is still tied to visuals
✅ Fix before adding more content

10. Determinism Guarantees

Card resolution must be:

Order-dependent

Repeatable

Independent of framerate

Independent of animation speed

This is essential for:

Balance

Bug reproduction

Replay systems

Multiplayer (future-proofing)

11. Common Failure Patterns to Avoid
Bad Pattern	Why It Breaks
Logic in animation callbacks	Animation can be interrupted
Destroying card objects early	Effects never resolve
Shared animation coroutines	Fast play cancels them
Visual timing controlling effects	Non-deterministic
12. Industry Reference (Why This Works)

This architecture is used by:

Slay the Spire

Monster Train

Griftlands

Inscryption

These games:

Resolve logic instantly

Animate independently

Remain stable at high speed

13. Final Rule of Thumb

If the game can be played at 10× speed with zero animations and still work perfectly — your architecture is correct.