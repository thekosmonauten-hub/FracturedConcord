# Unity Migration Roadmap - PoE Deckbuilder ARPG

## Overview
This roadmap prioritizes UI setup and system dependencies to ensure a logical development flow. Each phase builds upon the previous one, with clear testing milestones.

---

## Phase 1: Foundation & Core UI (Weeks 1-2)
**Goal**: Establish basic Unity project structure and core UI screens

### UI Screens to Create
1. **Main Menu Screen**
   - New Game button
   - Continue Game button
   - Settings button
   - Exit button

2. **Character Creation Screen**
   - Class selection (6 character classes)
   - Character name input
   - Attribute preview (STR/DEX/INT)
   - Starter deck preview
   - Create character button

3. **Loading Screen**
   - Progress bar
   - Loading tips
   - Asset loading feedback

### Systems to Implement
1. **Game Manager (Singleton)**
   - Game state management
   - Scene transitions
   - Save/load system foundation

2. **Character Data System**
   - Character class definitions
   - Attribute system (STR/DEX/INT)
   - Basic character creation logic

3. **Scene Management**
   - Scene loading system
   - Transition effects
   - Error handling

### Dependencies
- ✅ Unity project setup (already done)
- ✅ Basic scene management (already done)

### Testing Milestones
- [ ] Main menu loads and navigates correctly
- [ ] Character creation creates valid character data
- [ ] Scene transitions work smoothly
- [ ] No console errors during navigation

---

## Phase 2: Character System & Stats (Weeks 3-4)
**Goal**: Implement character system with attribute scaling and progression

### UI Screens to Create
1. **Character Stats Screen**
   - Attribute display (STR/DEX/INT)
   - Derived stats (Health, Damage, etc.)
   - Level and experience
   - Attribute point allocation (if applicable)

2. **Character Overview Screen**
   - Character portrait
   - Basic info (name, class, level)
   - Current equipment summary
   - Quick stats overview

3. **Level Up Screen**
   - Attribute point allocation
   - Stat preview (before/after)
   - Confirmation dialog

### Systems to Implement
1. **Character Manager**
   - Character data persistence
   - Attribute calculation
   - Level progression
   - Stat scaling (STR/DEX/INT → derived stats)

2. **Save System**
   - Character data serialization
   - JSON save files
   - Save/load functionality

3. **Attribute Scaling System**
   - STR → Physical damage, Health, Life regen
   - DEX → Accuracy, Crit chance, Evasion
   - INT → Spell damage, Energy Shield, ES regen

### Dependencies
- ✅ Phase 1 foundation
- ✅ Character creation system

### Testing Milestones
- [ ] Character stats calculate correctly from attributes
- [ ] Save/load preserves character data
- [ ] Attribute scaling affects derived stats properly
- [ ] Level up system works correctly

---

## Phase 3: Card System Foundation (Weeks 5-6)
**Goal**: Implement basic card system with UI and data structures

### UI Screens to Create
1. **Deck Management Screen**
   - Card list/grid view
   - Card details panel
   - Add/remove cards
   - Deck size indicator

2. **Card Detail Panel**
   - Card image/art
   - Card text and effects
   - Cost and type
   - Scaling information

3. **Card Collection Screen**
   - All available cards
   - Filter by type/rarity
   - Search functionality
   - Card preview

### Systems to Implement
1. **Card Data System**
   - Card type definitions (Attack, Spell, Aura, etc.)
   - Card effect system
   - Card scaling based on attributes
   - Starter deck definitions

2. **Card Manager**
   - Card collection management
   - Deck building logic
   - Card generation (procedural)
   - Card effects processing

3. **Dynamic Tooltip System**
   - Real-time damage calculation
   - Stat-based scaling display
   - Equipment integration preview

### Dependencies
- ✅ Character system (for attribute scaling)
- ✅ Save system (for card collection)

### Testing Milestones
- [ ] Cards display correctly with proper scaling
- [ ] Deck building works without errors
- [ ] Card effects process correctly
- [ ] Tooltips show accurate damage calculations

---

## Phase 4: Basic Combat System (Weeks 7-8)
**Goal**: Implement turn-based combat with cards and basic UI

### UI Screens to Create
1. **Combat Screen**
   - Player hand (cards)
   - Enemy display
   - Turn indicator
   - Action buttons (end turn, etc.)

2. **Combat HUD**
   - Player health/energy shield
   - Enemy health bars
   - Turn counter
   - Status effects display

3. **Card Play Interface**
   - Card targeting (if needed)
   - Effect preview
   - Confirmation dialog

### Systems to Implement
1. **Combat Manager**
   - Turn-based combat logic
   - Player and enemy phases
   - Card playing mechanics
   - Basic damage calculation

2. **Enemy System**
   - Enemy data structures
   - Basic enemy AI
   - Enemy actions and patterns
   - Health and status management

3. **Status Effect System**
   - Status effect application
   - Duration tracking
   - Effect processing
   - Visual indicators

### Dependencies
- ✅ Card system (for playing cards)
- ✅ Character system (for stats and health)

### Testing Milestones
- [ ] Combat starts and progresses correctly
- [ ] Cards can be played and resolve effects
- [ ] Enemy AI takes actions
- [ ] Status effects apply and expire properly

---

## Phase 5: Equipment & Loot System (Weeks 9-10)
**Goal**: Implement equipment system with crafting and inventory

### UI Screens to Create
1. **Inventory Screen**
   - Equipment grid
   - Item details panel
   - Equip/unequip functionality
   - Item comparison

2. **Equipment Screen**
   - Character equipment slots
   - Stat changes preview
   - Equipment requirements
   - Quick equip options

3. **Crafting Screen**
   - Currency display
   - Item selection
   - Crafting options
   - Result preview

### Systems to Implement
1. **Equipment System**
   - Equipment data structures
   - Affix system
   - Stat modification
   - Equipment requirements

2. **Crafting System**
   - 6 currency types (Infusion, Perfection, etc.)
   - Crafting logic
   - Currency management
   - Result validation

3. **Inventory System**
   - Item storage
   - Grid-based layout
   - Item stacking
   - Drag and drop

### Dependencies
- ✅ Character system (for equipment stats)
- ✅ Save system (for inventory persistence)

### Testing Milestones
- [ ] Equipment can be equipped and affects stats
- [ ] Crafting system works with all currency types
- [ ] Inventory manages items correctly
- [ ] Equipment requirements are enforced

---

## Phase 6: Passive Tree System (Weeks 11-12)
**Goal**: Implement modular passive tree with board-based design

### UI Screens to Create
1. **Passive Tree Screen**
   - Board grid display
   - Node connections
   - Point allocation
   - Stat preview

2. **Board Selection Screen**
   - Available boards
   - Board themes (Fire, Cold, etc.)
   - Connection points
   - Board requirements

3. **Node Detail Panel**
   - Node effects
   - Point cost
   - Requirements
   - Stat changes

### Systems to Implement
1. **Passive Tree Manager**
   - Board management
   - Node allocation
   - Connection logic
   - Stat calculation

2. **Board System**
   - Board data structures
   - Extension points
   - Auto-allocation
   - Theme-based boards

3. **Keystone System**
   - Keystone effects
   - Build-defining abilities
   - Special mechanics

### Dependencies
- ✅ Character system (for point allocation)
- ✅ Save system (for tree state)

### Testing Milestones
- [ ] Boards can be connected and allocated
- [ ] Node effects apply correctly
- [ ] Keystones work as intended
- [ ] Tree state persists correctly

---

## Phase 7: World Map & Progression (Weeks 13-14)
**Goal**: Implement world map with area progression and encounters

### UI Screens to Create
1. **World Map Screen**
   - Area nodes
   - Connection lines
   - Completion indicators
   - Area information

2. **Area Detail Panel**
   - Area description
   - Enemy information
   - Loot summary
   - Requirements

3. **Progression Screen**
   - Act completion
   - Area statistics
   - Unlock progress
   - Achievement tracking

### Systems to Implement
1. **World Map Manager**
   - Area progression
   - Node unlocking
   - Completion tracking
   - Enemy scaling

2. **Area System**
   - Area data structures
   - Enemy packs
   - Loot tables
   - Requirements

3. **Progression System**
   - Act-based progression
   - Area completion
   - Unlock conditions
   - Achievement system

### Dependencies
- ✅ Combat system (for encounters)
- ✅ Character system (for progression)

### Testing Milestones
- [ ] Areas unlock correctly based on progression
- [ ] Encounters trigger properly
- [ ] Completion tracking works
- [ ] Enemy scaling is appropriate

---

## Phase 8: Advanced Combat Features (Weeks 15-16)
**Goal**: Implement advanced combat mechanics and polish

### UI Screens to Create
1. **Advanced Combat HUD**
   - Momentum display
   - Combo indicators
   - Status effect icons
   - Turn timer

2. **Combo System UI**
   - Combo highlighting
   - Effect preview
   - Chain indicators
   - Combo counter

3. **Victory/Defeat Screens**
   - Results summary
   - Loot display
   - Experience gained
   - Continue options

### Systems to Implement
1. **Advanced Combat**
   - Wave-based encounters
   - Momentum system
   - Combo mechanics
   - Advanced AI

2. **Status Effect System**
   - Poison, Bleed, Burn
   - Duration management
   - Stacking effects
   - Visual feedback

3. **Combat Polish**
   - Animations
   - Sound effects
   - Visual feedback
   - Performance optimization

### Dependencies
- ✅ Basic combat system
- ✅ Card system
- ✅ Equipment system

### Testing Milestones
- [ ] Advanced combat mechanics work correctly
- [ ] Status effects apply and stack properly
- [ ] Combo system functions as intended
- [ ] Performance is acceptable

---

## Phase 9: Currency & Embossing Systems (Weeks 17-18)
**Goal**: Implement advanced crafting and card modification systems

### UI Screens to Create
1. **Currency Crafting Screen**
   - Currency display
   - Item selection
   - Crafting options
   - Result preview

2. **Embossing Workspace**
   - Card selection
   - Embossing slots
   - Currency application
   - Effect preview

3. **Advanced Inventory**
   - Currency management
   - Embossing materials
   - Item organization
   - Search/filter

### Systems to Implement
1. **Advanced Crafting**
   - 6 currency types
   - Spirit orbs
   - Crafting validation
   - Result generation

2. **Embossing System**
   - 4 embossing currencies
   - Slot management
   - Effect application
   - Tag system

3. **Currency Management**
   - Currency persistence
   - Acquisition methods
   - Usage tracking
   - Balance management

### Dependencies
- ✅ Equipment system
- ✅ Card system
- ✅ Save system

### Testing Milestones
- [ ] All currency types work correctly
- [ ] Embossing system functions properly
- [ ] Currency persistence works
- [ ] Crafting results are valid

---

## Phase 10: Polish & Optimization (Weeks 19-20)
**Goal**: Final polish, optimization, and bug fixes

### UI Polish
1. **Visual Polish**
   - Animations and transitions
   - Sound effects
   - Particle effects
   - UI responsiveness

2. **UX Improvements**
   - Tooltip system
   - Help system
   - Tutorial integration
   - Accessibility features

3. **Performance Optimization**
   - Memory management
   - Loading optimization
   - UI performance
   - Asset optimization

### Systems to Implement
1. **Performance Systems**
   - Object pooling
   - Asset management
   - Memory optimization
   - Loading systems

2. **Quality of Life**
   - Auto-save
   - Settings system
   - Debug tools
   - Error handling

3. **Final Integration**
   - System integration testing
   - Cross-system validation
   - Save/load testing
   - Performance testing

### Dependencies
- ✅ All previous phases
- ✅ Complete system integration

### Testing Milestones
- [ ] All systems work together correctly
- [ ] Performance meets targets
- [ ] No critical bugs remain
- [ ] User experience is smooth

---

## Development Guidelines

### UI Development Principles
1. **Consistent Design**: Use Unity UI Toolkit for consistent styling
2. **Responsive Layout**: Design for different screen sizes
3. **Accessibility**: Include keyboard navigation and screen reader support
4. **Performance**: Optimize UI updates and rendering

### System Integration
1. **Loose Coupling**: Use events for system communication
2. **Data Flow**: Clear data flow between systems
3. **Error Handling**: Robust error handling and recovery
4. **Testing**: Test each system individually and together

### Unity-Specific Considerations
1. **ScriptableObjects**: Use for static data configuration
2. **Prefabs**: Create reusable UI components
3. **Scene Management**: Efficient scene loading and transitions
4. **Asset Management**: Proper asset organization and loading

### Testing Strategy
1. **Unit Testing**: Test individual systems
2. **Integration Testing**: Test system interactions
3. **UI Testing**: Test user interface flows
4. **Performance Testing**: Monitor performance metrics

---

## Risk Mitigation

### High-Risk Areas
1. **Complex System Integration**: Start simple and add complexity gradually
2. **Performance Issues**: Monitor performance from the start
3. **Data Persistence**: Implement robust save/load early
4. **UI Complexity**: Break down complex UI into manageable components

### Contingency Plans
1. **Scope Reduction**: Identify features that can be simplified or delayed
2. **Alternative Approaches**: Have backup plans for complex systems
3. **Testing Strategy**: Implement comprehensive testing to catch issues early
4. **Documentation**: Maintain clear documentation for troubleshooting

---

## Success Metrics

### Technical Metrics
- [ ] All core systems implemented and functional
- [ ] Performance targets met (60 FPS, <2GB memory)
- [ ] No critical bugs in core gameplay
- [ ] Save/load system works reliably

### User Experience Metrics
- [ ] Smooth navigation between screens
- [ ] Intuitive UI design
- [ ] Responsive controls
- [ ] Clear feedback for user actions

### Development Metrics
- [ ] Code maintainability and organization
- [ ] Documentation completeness
- [ ] Testing coverage
- [ ] Development velocity

This roadmap provides a structured approach to migrating your complex deckbuilder ARPG to Unity while maintaining the depth and quality of your original implementation.
