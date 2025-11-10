# Ascendancy Floating Nodes Guide

> **Updated:** 2025-11-07

Floating nodes let you bridge two existing passives (usually Majors) without extending either branch. They spawn automatically between the referenced nodes, inherit minor-node visuals, and unlock when **any** of their linked prerequisites is taken.

---

## 1. Prepare the Ascendancy Asset

1. Open your `AscendancyData` asset in the Inspector.
2. Ensure `Use Branch System` and `Use Auto Generated Paths` are enabled so the tree regenerates positions when you save.
3. Confirm the reference nodes you want to bridge have unique, easy-to-read names (Floating nodes search by name).

---

## 2. Add a Floating Node

1. Expand the new **Floating Nodes (Advanced)** list.
2. Click the ➕ icon to add an element.
3. For the new entry:
   - **Node**: Create or assign the `AscendancyPassive` that should appear between branches. It will render using the Minor prefab and behave like a minor node unless you override its scale.
   - **First Node Name**: Type the exact name of the first passive to connect.
   - **Second Node Name**: Type the exact name of the second passive to connect.

> ✅ The node unlocks when *either* referenced passive is allocated. Both names are optional—you can supply only one if you want a floating node that anchors to a single branch.

---

## 3. Fine-Tune the Position (Optional)

Floating nodes automatically sit at the midpoint between the two referenced nodes. Use these fields to tweak their placement:

- **Midpoint Bias** (`-0.5 → +0.5`): Slides the node along the line. Negative values push toward the first node; positive values push toward the second.
- **Distance Multiplier**: Expands or contracts the spacing relative to the two nodes. `1` keeps the exact midpoint; `1.2` nudges outward; `0.8` squeezes inward.
- **Perpendicular Offset**: Raises or lowers the node perpendicular to the connecting line. Positive moves clockwise; negative moves counter-clockwise.
- **Position Offset**: Absolute X/Y adjustment in local canvas space.
- **Node Scale Override**: Force a custom scale (leave at `0` to use the passive’s existing `Node Scale`).

Tip: Start with the defaults, play the scene, and adjust the values with the Game view visible for quick iteration.

---

## 4. Save and Test

1. Press **Ctrl+S** (or manually save) after editing the asset.
2. Enter Play mode and open the `AscendancyDisplayPanel`.
3. Confirm:
   - The floating node appears between the intended majors.
   - Connection lines draw to both prerequisite nodes.
   - Hovering reveals the correct tooltip.
   - Unlock availability turns on as soon as one prerequisite node is chosen.

---

## 5. Designer Notes

- Floating nodes default to Minor styling. If you want a different look, update the assigned passive’s `Icon` or set `Node Scale Override`.
- They do **not** consume branch spacing, so branch counts and automatic spacing remain unchanged.
- The passive still costs whatever `Point Cost` you assign—it just respects the “any prerequisite” rule under the hood.
- Because floating nodes unlock from either prerequisite, they’re ideal for shared capstones, thematic bridges, or reward nodes placed between two majors.

---

## 6. Troubleshooting

| Symptom | Fix |
|---------|-----|
| Node fails to appear | Make sure the floating node’s `AscendancyPassive` is assigned and has a unique name. |
| Only one connection line drawn | Verify both node names are spelled exactly as they appear in the branch list. |
| Node never becomes available | Check the console for warnings; at least one prerequisite must be valid. Ensure the referenced nodes are part of the same `AscendancyData`. |
| Node overlaps other UI | Use `Perpendicular Offset` or `Position Offset` to nudge it clear of neighboring nodes. |

---

## 7. Migrating from Cross-Branch Connections

1. Copy the `To Node Name` from your existing cross-branch setup.
2. Create a floating node that references the same two passives.
3. Remove the original `Cross-Branch Connections` entry (optional but recommended).
4. Save and test. The new node will now unlock when either prerequisite is taken, preventing multi-major exploits.

---

Floating nodes are now the recommended way to intertwine branches. They keep unlock pacing balanced while delivering the visual overlap you wanted from legacy cross-branch connections.

