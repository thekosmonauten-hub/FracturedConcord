# Cross-Branch Connections *(Legacy)*

> **2025-11-07 Update** — Floating nodes now provide the recommended way to bridge branches without granting bonus majors. Use cross-branch connections only when you specifically need hard prerequisites between nodes. See `ASCENDANCY_FLOATING_NODES_GUIDE.md` for the preferred workflow.

Guide for creating intertwined Ascendancy paths with connections between branches.

---

## 🎯 Overview

Create web-like or intertwined trees where branches connect to each other:

```
         START
        /     \
    Branch 1  Branch 2
       |         |
    Node 1   Node 1
       |    ✗    |  ← Cross connection!
    Node 2 ═════ Node 2
       |         |
    Node 3   Node 3
```

**Use Cases:**
- Create "gate" nodes (require both branches)
- Allow alternate paths to same node
- Create complex unlock requirements
- Make interesting visual patterns

---

## ✅ How to Create Cross-Branch Connections

### **Example: Connect Branch 1 Node 2 to Branch 2 Node 2**

**In your Ascendancy asset (e.g., MarauderCrumblingEarth):**

```
Branches → Element 0 (Branch 1)

Cross-Branch Connections: Size = 1

    Element 0:
    ├─ From Node Index: 2 (this branch's node index)
    ├─ To Node Name: "Trembling Echo" (exact name from Branch 2)
    └─ Is Prerequisite: ✅ (Branch 1 Node 2 requires Branch 2's "Trembling Echo")
```

**Result:**
- Node at index [2] in Branch 1 now requires "Trembling Echo" from Branch 2
- Connection line automatically drawn between them
- Player must unlock "Trembling Echo" before unlocking Branch 1's node [2]

---

## 🎨 Visual Examples

### **Example 1: Simple Cross-Connection**

```
         START
        /     \
   Branch 1  Branch 2
       |         |
    Node 0   Node 0
       |         |
    Node 1   Node 1
       |    \  / |  ← Cross connection
    Node 2 ══X══ Node 2
       |         |
    Node 3   Node 3
```

**Setup:**
```
Branch 1:
└─ Cross-Branch Connections → Element 0:
    ├─ From Node Index: 2
    ├─ To Node Name: "[Branch 2 Node 2 name]"
    └─ Is Prerequisite: ✅
```

---

### **Example 2: Gate Node (Requires Both Branches)**

```
         START
        /     \
   Branch 1  Branch 2
       |         |
    Node 0   Node 0
       |         |
    Node 1   Node 1
       └─────┬─────┘
          Node 2 (requires both Node 1's)
             |
```

**Setup:**
```
Branch 1 → Node 2:
└─ Cross-Branch Connections:
    ├─ From Node Index: 2
    ├─ To Node Name: "[Branch 2 Node 1 name]"
    └─ Is Prerequisite: ✅
```

**Result:** Node 2 requires BOTH Branch 1 Node 1 AND Branch 2 Node 1

---

### **Example 3: Alternate Paths**

```
         START
        /     \
   Branch 1  Branch 2
       |         |
    Node 0   Node 0
       |\       /|
       | \     / |
       |  Node 1 |  ← Can be reached from either branch
       |  /   \  |
       | /     \ |
    Node 2   Node 2
```

**Setup:**
```
Branch 1:
└─ Cross-Branch Connections → Element 0:
    ├─ From Node Index: 1
    ├─ To Node Name: "[Branch 2 Node 0 name]"
    └─ Is Prerequisite: ☐ (optional - either works)

Branch 2:
└─ Cross-Branch Connections → Element 0:
    ├─ From Node Index: 1
    ├─ To Node Name: "[Branch 1 Node 0 name]"
    └─ Is Prerequisite: ☐
```

---

## 📋 Full Setup Example

### **Scenario: Intertwine Branches at Node 2**

**Ascendancy: MarauderCrumblingEarth**

```yaml
Branches: Size = 2

Branch 0 (Left):
├─ Branch Name: "Path of Destruction"
├─ Branch Nodes: Size = 4
│   ├─ [0] Attack & Magnitude (Minor)
│   ├─ [1] Blood Price (Major)
│   ├─ [2] Spring of Rage (Minor)
│   └─ [3] Final Offering (Major)
│
└─ Cross-Branch Connections: Size = 1
    └─ Element 0:
        ├─ From Node Index: 2 (Spring of Rage)
        ├─ To Node Name: "Trembling Echo" (from Branch 1)
        └─ Is Prerequisite: ✅

Branch 1 (Right):
├─ Branch Name: "Path of Resilience"
├─ Branch Nodes: Size = 4
│   ├─ [0] Crumble Duration (Minor)
│   ├─ [1] Trembling Echo (Major)
│   ├─ [2] Seismic Hunger (Major)
│   └─ [3] Stoneskin (Major)
│
└─ Cross-Branch Connections: Size = 1
    └─ Element 0:
        ├─ From Node Index: 2 (Seismic Hunger)
        ├─ To Node Name: "Spring of Rage" (from Branch 0)
        └─ Is Prerequisite: ✅
```

**Result:**
- "Spring of Rage" (Branch 0[2]) requires "Trembling Echo" (Branch 1[1])
- "Seismic Hunger" (Branch 1[2]) requires "Spring of Rage" (Branch 0[2])
- Creates X-pattern connection between branches!

---

## 🎨 Visual Result

```
          START
         /     \
    Branch 0  Branch 1
        |        |
   Node 0    Node 0
        |        |
   Node 1    Node 1
        |   ✗    |
   Node 2 ═════ Node 2  ← Connected!
        |        |
   Node 3    Node 3
```

**Connection lines drawn automatically!**

---

## 🔧 Connection Types

### **Is Prerequisite: ✅ (True)**
- Node REQUIRES other branch's node first
- Must unlock Branch 2 Node before Branch 1 Node
- Creates hard dependency

**Example:**
```
Spring of Rage requires Trembling Echo
└─ Must unlock Trembling Echo before Spring of Rage
```

---

### **Is Prerequisite: ☐ (False) - Optional**
- Node CAN connect but doesn't require
- Either path works
- Visual connection only

**Example:**
```
Node can be unlocked via Branch 1 OR Branch 2
└─ Flexible pathing
```

---

## 📊 Complex Example: Diamond Pattern

Create a diamond shape with 4 connection points:

```
         START
        /     \
   Branch 1  Branch 2
       |        |
    Node 0   Node 0
       |   ╱╲   |
       |  ╱  ╲  |
    Node 1    Node 1
       |  ╲  ╱  |
       |   ╲╱   |
    Node 2   Node 2
       └────┴────┘
          Gate Node
```

**Setup:**
```
Branch 1:
├─ Cross-Branch Connections: Size = 2
│   ├─ Element 0: From Index 1 → To "Branch 2 Node 0" (✅ Prerequisite)
│   └─ Element 1: From Index 2 → To "Branch 2 Node 1" (✅ Prerequisite)

Branch 2:
├─ Cross-Branch Connections: Size = 2
│   ├─ Element 0: From Index 1 → To "Branch 1 Node 0" (✅ Prerequisite)
│   └─ Element 1: From Index 2 → To "Branch 1 Node 1" (✅ Prerequisite)

Gate Node:
└─ Prerequisites: [Branch 1 Node 2, Branch 2 Node 2]
    (Both required!)
```

---

## 🧪 Testing

1. **Add cross-branch connection**
2. **Save asset**
3. **Press Play**
4. **Click Ascendancy**
5. **Verify:**
   - ✅ Connection line drawn between branches
   - ✅ Hovering shows both connections
   - ✅ Node shows as locked until prerequisite met

**Console:**
```
[AscendancyBranch] Added cross-branch prerequisite: Spring of Rage requires Trembling Echo
[AscendancyTreeDisplay] Drew line from Trembling Echo to Spring of Rage
```

---

## 💡 Design Tips

### **Good Use Cases:**
- ✅ Create "gate" nodes (require progress in both branches)
- ✅ Allow alternate unlock paths
- ✅ Visual symmetry (X or diamond patterns)
- ✅ Story-driven connections (thematically linked nodes)

### **Avoid:**
- ❌ Too many cross-connections (confusing)
- ❌ Circular dependencies (A requires B, B requires A)
- ❌ Every node connected (defeats branching purpose)

### **Best Practices:**
- 1-3 cross-connections per Ascendancy
- Connect at similar depths (Node 2 to Node 2, not Node 1 to Node 4)
- Use for thematic synergies
- Test unlock order thoroughly

---

## 📋 Quick Setup Checklist

- [ ] Open Ascendancy asset
- [ ] Expand a branch
- [ ] Add Cross-Branch Connections
- [ ] Set From Node Index
- [ ] Set To Node Name (exact name from other branch)
- [ ] Set Is Prerequisite (✅ or ☐)
- [ ] Save asset
- [ ] Test tree display

---

## 🐛 Troubleshooting

### **❌ Connection not appearing:**

**Check 1: Node name exact match**
```
To Node Name must EXACTLY match the other node's name
Case-sensitive!
"Trembling Echo" ≠ "trembling echo"
```

**Check 2: Save asset**
```
After adding connection, press Ctrl+S
```

**Check 3: Enable debug logs**
```
Console should show:
[AscendancyBranch] Added cross-branch prerequisite: ...
```

---

### **❌ Can't unlock node:**

**Check:** Is the other branch's node unlocked?
- Cross-branch prerequisites create unlock requirements
- Must progress in both branches

---

## 🔄 Alternative: Manual Prerequisites

If cross-branch system doesn't work, you can manually edit:

1. **Disable auto-generation:**
   ```
   Use Auto Generated Paths: ☐
   ```

2. **Manually set prerequisites in each node:**
   ```
   Node 2 (Branch 1):
   └─ Prerequisites:
       ├─ [0] "Node 1" (same branch)
       └─ [1] "Trembling Echo" (other branch)
   ```

**System will still draw connection lines automatically!**

---

**Last Updated:** 2024-12-19
**Status:** ✅ Cross-Branch Connections Implemented
**Try it:** Connect Branch 1 Node 2 to Branch 2 Node 2 for intertwined paths!


