# Enemy Spawn Animations Setup Guide

## **🎯 Overview**
This guide explains how to set up sweep-in animations for enemies spawning on new waves. Enemies will slide in from the left side of the screen with customizable timing and effects.

## **✨ Features**
- **Sweep-in Animation**: Enemies slide in from off-screen left
- **Staggered Spawning**: Multiple enemies spawn with delays between them
- **Customizable Timing**: Adjustable animation duration and stagger delays
- **Animation Curves**: Smooth easing for natural motion
- **Performance Optimized**: Uses LeanTween for efficient animations

## **🔧 Setup Instructions**

### **Step 1: Configure EnemySpawner**

1. **Select your EnemySpawner** in the scene
2. **In Inspector**, expand the **"Spawn Animations"** section:

#### **Animation Settings:**
- **Enable Spawn Animations**: ✓ Checked (enables the system)
- **Spawn Start Distance**: `-800` (how far off-screen enemies start)
- **Spawn Animation Duration**: `0.8` (seconds for sweep-in animation)
- **Spawn Stagger Delay**: `0.2` (seconds between each enemy spawn)
- **Spawn Animation Curve**: `EaseInOut` (smooth acceleration/deceleration)

#### **Recommended Values:**
```
Enable Spawn Animations: ✓ Checked
Spawn Start Distance: -800
Spawn Animation Duration: 0.8
Spawn Stagger Delay: 0.2
Spawn Animation Curve: EaseInOut (default)
```

### **Step 2: Test the Animations**

1. **Enter Play Mode**
2. **Start a combat encounter**
3. **Watch enemies spawn** - they should sweep in from the left
4. **Check console logs** for animation feedback

## **🎮 How It Works**

### **Animation Flow:**
1. **Wave starts** → `SpawnWaveInternal()` called
2. **EnemySpawner** gets list of enemies to spawn
3. **Staggered spawning** begins with delays
4. **Each enemy** starts off-screen left (`spawnStartDistance`)
5. **LeanTween animation** moves them to final position
6. **Animation completes** → enemy ready for combat

### **Timing Example:**
```
Enemy 1: Spawns immediately (0.0s delay)
Enemy 2: Spawns after 0.2s delay
Enemy 3: Spawns after 0.4s delay
Enemy 4: Spawns after 0.6s delay
Enemy 5: Spawns after 0.8s delay

Each enemy takes 0.8s to sweep in
Total wave spawn time: ~1.6s for 5 enemies
```

## **⚙️ Customization Options**

### **Animation Speed:**
- **Faster**: Reduce `Spawn Animation Duration` (0.5s)
- **Slower**: Increase `Spawn Animation Duration` (1.2s)

### **Stagger Effect:**
- **More dramatic**: Increase `Spawn Stagger Delay` (0.3s)
- **Less dramatic**: Decrease `Spawn Stagger Delay` (0.1s)
- **Simultaneous**: Set `Spawn Stagger Delay` to 0

### **Start Position:**
- **Further off-screen**: Increase `Spawn Start Distance` (-1200)
- **Closer to screen**: Decrease `Spawn Start Distance` (-400)

### **Animation Style:**
- **Linear**: Set curve to `Linear`
- **Bouncy**: Set curve to `EaseOutBounce`
- **Sharp**: Set curve to `EaseInOut`

## **🐛 Troubleshooting**

### **Enemies Not Animating:**
- ✅ Check `Enable Spawn Animations` is checked
- ✅ Verify `Spawn Start Distance` is negative (off-screen left)
- ✅ Ensure `Spawn Animation Duration` > 0
- ✅ Check console for animation logs

### **Animations Too Fast/Slow:**
- ✅ Adjust `Spawn Animation Duration`
- ✅ Modify `Spawn Animation Curve`

### **Enemies Overlapping:**
- ✅ Increase `Spawn Stagger Delay`
- ✅ Check spawn point positions

### **Performance Issues:**
- ✅ Reduce `Spawn Animation Duration`
- ✅ Decrease `Spawn Stagger Delay`
- ✅ Use simpler animation curves

## **📊 Performance Notes**

- **LeanTween** is highly optimized for UI animations
- **Object pooling** prevents memory allocation during animations
- **Staggered spawning** spreads CPU load over time
- **Animation curves** are pre-calculated for efficiency

## **🎯 Expected Results**

### **Visual Effect:**
- Enemies smoothly slide in from the left
- Each enemy appears with a slight delay
- Natural, polished wave introduction
- No jarring instant appearances

### **Console Output:**
```
[EnemySpawner] Spawning 3 enemies with staggered animations
[EnemySpawner] Started sweep-in animation for PooledEnemy_0 from (-800, 0, 0) to (0, 0, 0)
[EnemySpawner] Spawn animation completed for PooledEnemy_0
[EnemySpawner] Started sweep-in animation for PooledEnemy_1 from (-800, 0, 0) to (0, 0, 0)
[EnemySpawner] Spawn animation completed for PooledEnemy_1
```

## **🔮 Future Enhancements**

Potential additions to the animation system:
- **Different entry directions** (top, right, bottom)
- **Scale animations** (enemies grow from small to normal)
- **Rotation effects** (enemies spin in)
- **Particle effects** (dust clouds, magic sparkles)
- **Sound effects** (whoosh sounds, impact sounds)
- **Screen shake** on enemy arrival

## **✅ Checklist**

- [ ] EnemySpawner has spawn animation settings configured
- [ ] Enable Spawn Animations is checked
- [ ] Spawn Start Distance is set to negative value
- [ ] Spawn Animation Duration is reasonable (0.5-1.2s)
- [ ] Spawn Stagger Delay creates desired effect
- [ ] Animation curve provides smooth motion
- [ ] Tested in play mode with multiple enemies
- [ ] Console shows animation logs
- [ ] No performance issues observed

**Your enemy spawn animations are now ready!** 🎮✨











