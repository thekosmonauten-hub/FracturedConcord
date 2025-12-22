# Warrant Tree Quick Setup Guide

## Instant Tree Generation (30 seconds)

Instead of manually placing nodes, use the procedural generator:

### Step 1: Create Graph Definition
1. Right-click in Project → `Create → Dexiled → Warrants → Board Graph Definition`
2. Name it (e.g., `MasterWarrantGraph`)

### Step 2: Generate Tree
1. Select the graph definition asset in the Project window
2. In the Inspector, scroll to **"Quick Generation"** section
3. Adjust parameters if needed (or use defaults):
   - **Rectangle Width**: 220 (slightly tighter horizontal spacing)
   - **Rectangle Height**: 140 (slightly tighter vertical spacing)
   - **Effect Nodes Per Edge**: 3 (how many effect nodes between sockets)
   - **Anchor Position**: (0, -120) (where the start node sits)
4. Click **"Generate Rectangle Frame"**

**Done!** You now have a fully functional tree with:
- Anchor node at bottom
- Three branches connecting to a fully socketed rectangular frame
- Center hub connecting to every frame junction (corners, midpoints, mid-edge sockets)
- All effect nodes automatically positioned between every socket pair

### Step 3: Wire Up in Scene
1. In your Warrant Tree scene, select the `WarrantBoardGraphBuilder` component
2. Assign the generated graph definition to the `Graph Definition` field
3. Set `Effect Nodes Per Edge` to `0` (graph already contains all effect nodes)
4. Assign node prefabs (or leave blank to use auto-generated fallbacks)
5. Play the scene - tree builds automatically!

## What Gets Generated

- **1 Anchor** node (start point at bottom)
- **16 Frame Socket** nodes (corners, midpoints, and additional mid-edge sockets)
- **1 Center Hub** socket (connects to all frame junctions)
- **3 Anchor Path Socket** nodes (between anchor and bottom frame points)
- **8 Center Diagonal Socket** nodes (between center and each corner/midpoint)
- **4 Special Socket** nodes (two branching from MidTop, two from MidBottom)
- **126 Effect** nodes  
  - 48 around the rectangle frame (16 segments × 3 effects)  
  - 18 along the three anchor branches (6 segments × 3 effects)  
  - 48 along the center diagonals (16 segments × 3 effects)  
  - 12 along the MidTop/MidBottom special branches (4 segments × 3 effects)
- **All edges** properly linked (bidirectional connections)

Total: **159 nodes** with full adjacency support.

## Fine-Tuning

After generation, you can still:
- Manually adjust node positions in the graph definition
- Add/remove nodes using the visual editor (if needed)
- Modify the graph definition values directly

The procedural generator is just for **fast initial setup** - the graph definition remains fully editable!

