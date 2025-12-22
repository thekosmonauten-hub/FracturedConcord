# The Weave of Broken Clauses - TDP 

🧩 Technical Development Plan: Maze of Fragmented Law
PHASE 1 — Core Architecture Setup
1. Data Structures

Create core JSON or scriptable objects for:

A. MazeRun
{
  "run_id": "uuid",
  "current_floor": 1,
  "floors_total": 8,
  "seed": 12345,
  "floors": [],
  "player_state": {},
  "run_modifiers": []
}

B. Floor (Minimap)
{
  "floor_id": 1,
  "node_grid_size": 4,
  "nodes": []
}

C. Node
{
  "node_id": "uuid",
  "type": "combat | shrine | chest | trap | forge | boss | stairs | special",
  "position": { "x": 1, "y": 2 },
  "visited": false,
  "data": {}
}

D. NodeData (per node type)

Examples:
Combat
{ "enemy_pool": "floor1_basic", "modifiers": [] }

Shrine
{ "shrine_type": "Cohesion", "buff_strength": 1 }

Trap
{ "trap_type": "entropy_spike", "dc": 12 }

Forge
{ "options": ["upgrade", "remove_card", "fuse_card"] }


—
You can add new node types seamlessly by adding new data definitions.

PHASE 2 — Minimap Grid Generation
Goal:

Generate a 12–16 node floor with valid adjacency and at least 1 path from start → stairs.
Tasks:

Implement seeded procedural generation so runs are reproducible.
Generate a grid of cells (4×4 or 4×5).
Randomly activate 12–16 cells.
Ensure connectivity using BFS or an A* connectivity validation step.
Assign node types using weighted distribution tables:

Example weight structure:

{
  "combat": 45,
  "chest": 15,
  "shrine": 10,
  "trap": 10,
  "forge": 10,
  "special": 5
}


Guarantee:
exactly 1 Start node
exactly 1 Stairs node
Boss node is injected immediately before stairs if first time on floor
Checklist for success:
Always pathable
Always 12–16 nodes
Always 3–6 optional branches

PHASE 3 — Node Interaction System
Node click logic:
Check if node is adjacent to player position:
Manhattan distance = 1
If valid → enter node
Switch logic based on node.type
After resolution → node.markVisited()
Reveal newly adjacent nodes on minimap
This is simple state management.

PHASE 4 — Combat Encounters Integration
Flow:
Node triggers encounter
Load combat scene/state
Select enemy group from enemy pool appropriate to:
floor number
run modifiers (buffs/curses)
Combat resolves through your existing deckbuilder system

On victory:
grant floor-specific rewards
apply shrine modifiers (if active)
return to minimap state

Enemy Pools

Define pools like:
floor1_basic
floor2_entropy
floor3_law_echoes
floor4_elites
bossfloor

This allows easy scaling as more content is added.

PHASE 5 — Boss System
Boss structure:
Boss appears every 2 floors → when entering stairs for the first time.
Boss data file:

{
  "name": "Clauseborn Warden",
  "base_stats": {...},
  "ability_set": [...],
  "phases": {
     "phase1": { "health_threshold": 0.55, "modifier_pool": ["rupture", "echo", "mandate"]},
     "final": { "health_threshold": 0, "modifier_pool": ["all"] }
  }
}

Random modifiers:
Each boss encounter rolls 1–2 random modifiers from a global pool:
buff cards
special mechanics
environmental hazards

Retreat Logic:
When boss HP <= retreat_threshold (e.g., 45%)
Trigger retreat animation or state
Fight ends, player proceeds to next floor
Final floor → retreat disabled.

PHASE 6 — Global Run Modifiers (Shrines & Curses)

A simple buff/debuff system:
Storage

In MazeRun.run_modifiers:

{
  "modifier": "shrine_cohesion",
  "value": 1,
  "duration": "run"
}

Application
Your combat manager checks run_modifiers and applies effects:
improved draws
buffed card effects
reduced randomness
enemy buffs
entropy traps
This is easily extendable.

PHASE 7 — UI & UX Development
Minimap UI
Grid-based UI element
Node icons by type
Node highlight when selectable
Fog of war (hide non-adjacent nodes)
Hover tooltips with info (optional)
Run Summary UI
Current floor
Floors remaining
Run modifiers
Boss progression indicator
Boss Encounter UI
Persistent buff icons
Unique boss health bar
Modifier icons with explanations

PHASE 8 — Saving & Loading Runs
Simple save structure:

Save MazeRun JSON on:
entering a node
finishing a node
entering combat
finishing combat
entering new floor

Make runs:
resumable
deterministic
debuggable
Use seed + node definitions for regeneration if needed.

PHASE 9 — Content Pipeline Tools
To support long-term development:

1. Node Type Editor
Define new node types without hardcoding.

2. Enemy Pool Editor
Add enemies dynamically to floors.

3. Shrine Editor
Create new buffs in minutes.

4. Boss Modifier Editor
Use modular scripts to apply effects.
These tools prevent content bottlenecks.

PHASE 10 — Testing & Balancing
Key test cycles:
Path generation validity
Node transition bugs
Boss retreat threshold edge cases
Shrine stacking behavior
Curses not breaking runs
Performance during long runs
Save/load integrity

Automate tests for:
connectivity
seed reproducibility
run completion rates

✅ FINAL OUTPUT: Feature Milestones
Milestone 1 — Floor generation prototype

node generation
path validation
UI for minimap

Milestone 2 — Node interactions

combat
shrines
treasure
traps
stairs

Milestone 3 — Full run system

floors 1–8
boss spawns
retreat logic
run modifiers

Milestone 4 — Saving/loading
stable runs
reproducible bugs

Milestone 5 — Polish & content
special nodes
unique traps
new enemies
lore integration later