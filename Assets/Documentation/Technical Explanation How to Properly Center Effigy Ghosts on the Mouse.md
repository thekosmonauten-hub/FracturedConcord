Technical Explanation: How to Properly Center Effigy Ghosts on the Mouse

Effigies in this game are composed of multiple tiles arranged in different shapes (L, T, Z, Cross, etc.).
When a player picks up an effigy to drag it around the grid, the visual ghost should be centered under the mouse cursor—not aligned to the top-left tile.

However, by default most systems anchor the object to its first tile (0,0), which causes:

❌ Ghost appears “offset”
❌ Large shapes drift far from the cursor
❌ Cross/S/T shapes feel inconsistent and awkward

This happens because the shape's visual center is not the same as its tile origin.

✅ The Correct Approach: Use the Shape's Geometric Centroid

Instead of anchoring the ghost to (0,0) of its tile coordinates, you compute its centroid—the true average center of the tile layout:

Centroid Formula

If the shape contains N tiles with positions 
𝑝
1
,
𝑝
2
,
.
.
.
𝑝
𝑁
p
1
	​

,p
2
	​

,...p
N
	​

:

𝑐
𝑒
𝑛
𝑡
𝑟
𝑜
𝑖
𝑑
=
1
𝑁
∑
𝑖
=
1
𝑁
𝑝
𝑖
centroid=
N
1
	​

i=1
∑
N
	​

p
i
	​


This gives a tile-space coordinate (e.g. 0.75, 1.25) representing the actual center of the effigy shape.

🎯 Placement Logic

When the ghost is rendered:

Convert centroid into pixel/world offset
(e.g., centroid × tileSize)

Subtract that offset from the mouse world position

This makes the visual center of the effigy follow the cursor exactly.

🧠 Why This Works

This fixes all problems:

1. Shapes of any configuration center correctly

Cross → centered

T-Shape → centered

Z-Shape → centered

S-Shape → centered

Asymmetrical layouts → still centered

2. Rotation becomes stable

If the shape rotates 90/180/270 degrees:

Its centroid rotates with it

The ghost remains perfectly under the cursor

No drifting or snapping required

3. Intuitive UX

Players feel like they are moving a single object rather than dragging a cluster of tiles.

🧩 What the Other Agent Needs to Implement

Here is the minimal workflow they must follow:

1. Effigy defines its tiles (Vector2Int list)

Example:

(0,0), (0,1), (1,1), (2,1)

2. On pickup: compute centroid
centroid = average of tile positions

3. Compute pixel/world offset
pixelOffset = centroid * tileSize

4. While dragging:
ghost.position = mouseWorldPosition - pixelOffset

5. When rotating: rotate tile positions around centroid

This preserves proper centering

Prevents drift after rotations

📦 Result

The ghost is always perfectly centered, consistent, and responsive—regardless of effigy shape, size, rotation, or asymmetry.