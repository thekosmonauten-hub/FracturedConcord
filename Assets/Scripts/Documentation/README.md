# Passive Tree System Documentation

## 📚 Documentation Index

This directory contains comprehensive documentation for the Passive Tree system. Use this index to find the information you need quickly.

## 🚀 Getting Started

### **New to the System?**
1. **Quick Reference** → `Board_Asset_Generator_Quick_Reference.md`
2. **Full Guide** → `Dynamic_Board_Asset_Generator_Guide.md`
3. **Migration Guide** → `Board_System_Migration_Guide.md` (if migrating from old system)

### **Experienced Developer?**
- **Quick Reference** → `Board_Asset_Generator_Quick_Reference.md`
- **Troubleshooting** → See troubleshooting sections in each guide

## 📖 Documentation Files

### **Core System Documentation**

| File | Purpose | Audience |
|------|---------|----------|
| `Dynamic_Board_Asset_Generator_Guide.md` | Complete system guide | All developers |
| `Board_Asset_Generator_Quick_Reference.md` | Quick commands and tips | Daily reference |
| `Board_System_Migration_Guide.md` | Migration from old system | Developers upgrading |

### **Related Documentation**

| File | Purpose | Audience |
|------|---------|----------|
| `Individual_Board_Scripts_System_Guide.md` | Individual board system | Board developers |
| `ScriptableObject_Board_System_Guide.md` | ScriptableObject system | System architects |
| `Connecting_ScriptableObjects_to_Boards_Guide.md` | Asset connection guide | Asset management |

## 🎯 Quick Navigation

### **I want to...**

#### **Create a New Board**
1. Read `Board_Asset_Generator_Quick_Reference.md` → "Creating New Board Classes"
2. See `Dynamic_Board_Asset_Generator_Guide.md` → "Creating New Board Classes"

#### **Generate Board Assets**
1. Read `Board_Asset_Generator_Quick_Reference.md` → "Quick Start"
2. Use Unity menu: `Tools > Passive Tree > Generate Board Assets`

#### **Troubleshoot Issues**
1. Check `Board_Asset_Generator_Quick_Reference.md` → "Troubleshooting"
2. See `Dynamic_Board_Asset_Generator_Guide.md` → "Troubleshooting"

#### **Migrate from Old System**
1. Read `Board_System_Migration_Guide.md` → Complete migration guide
2. Follow step-by-step migration process

#### **Understand the Architecture**
1. Read `Dynamic_Board_Asset_Generator_Guide.md` → "System Architecture"
2. See `Individual_Board_Scripts_System_Guide.md` → System overview

## 🔧 Common Tasks

### **Daily Development**

| Task | Documentation | Section |
|------|---------------|---------|
| Generate assets | Quick Reference | "Generate Assets" |
| Create new board | Quick Reference | "Creating New Board Classes" |
| Debug issues | Quick Reference | "Troubleshooting" |
| Validate assets | Quick Reference | "Common Commands" |

### **System Administration**

| Task | Documentation | Section |
|------|---------------|---------|
| Clean up assets | Quick Reference | "Common Commands" |
| Migrate system | Migration Guide | "Migration Steps" |
| Understand architecture | Full Guide | "System Architecture" |
| Extend system | Full Guide | "Future Enhancements" |

## 📋 System Overview

### **Core Components**

```
Passive Tree System
├── Dynamic Board Asset Generator
│   ├── Automatic Discovery
│   ├── Dynamic UI Generation
│   └── Asset Management
├── Individual Board ScriptableObjects
│   ├── BaseBoardScriptableObject (Base Class)
│   ├── CoreBoardScriptableObject
│   ├── FireBoardScriptableObject
│   └── [Other Board Types]
└── Integration Systems
    ├── PassiveTreeBoardManager
    ├── PassiveTreeDataManager
    └── UI Components
```

### **Key Features**

- ✅ **Automatic Discovery**: Finds board classes using reflection
- ✅ **Dynamic UI**: Generates buttons automatically
- ✅ **Type Safety**: Each board has its own ScriptableObject type
- ✅ **Zero Maintenance**: Scales automatically with new board types
- ✅ **Future-Proof**: Easy to extend and modify

## 🐛 Troubleshooting Quick Reference

### **Common Issues**

| Issue | Quick Fix | Documentation |
|-------|-----------|---------------|
| No classes detected | Check inheritance from `BaseBoardScriptableObject` | Quick Reference → "Troubleshooting" |
| Assets not generated | Verify naming convention | Quick Reference → "Naming Convention" |
| Compilation errors | Update references to new system | Migration Guide → "Migration Steps" |
| Invalid assets | Use "Clean Up Old Assets" | Quick Reference → "Common Commands" |

### **Debugging Steps**

1. **Check Console Logs** for detailed error messages
2. **Use "Show Available Board Classes"** to verify detection
3. **Validate Assets** to check existing asset status
4. **Clean Up Old Assets** to remove problematic files

## 📞 Support Resources

### **Documentation**
- **Quick Reference**: `Board_Asset_Generator_Quick_Reference.md`
- **Full Guide**: `Dynamic_Board_Asset_Generator_Guide.md`
- **Migration Guide**: `Board_System_Migration_Guide.md`

### **Tools**
- **Board Asset Generator**: `Tools > Passive Tree > Generate Board Assets`
- **Asset Validation**: Use "Validate All Board Assets" in generator
- **Asset Cleanup**: Use "Clean Up Old Assets" in generator

### **Code Examples**
- **Board Class Template**: See Quick Reference → "Creating New Board Classes"
- **Integration Examples**: See Full Guide → "Integration with Existing Systems"
- **Migration Examples**: See Migration Guide → "Code Migration Examples"

## 🔄 Version History

### **Current Version**
- **Dynamic Board Asset Generator**: Automatic discovery and generation
- **Individual Board ScriptableObjects**: Type-safe board system
- **Zero-Maintenance Architecture**: Scales automatically

### **Previous Version**
- **Shared ScriptableObject**: Single class for all boards
- **Manual Asset Management**: Required manual updates
- **Limited Scalability**: Hard to extend

## 🚀 Future Roadmap

### **Planned Features**
- Custom board themes
- Advanced validation
- Batch operations
- Template system

### **Extension Points**
- Custom generators
- Plugin system
- Advanced naming conventions

## 📝 Contributing

### **Documentation Updates**
- Keep documentation current with system changes
- Update examples when APIs change
- Add new troubleshooting scenarios as they arise

### **System Improvements**
- Follow established patterns when adding new features
- Update documentation for new functionality
- Maintain backward compatibility where possible

---

## 📋 Quick Links

- **[Quick Reference](Board_Asset_Generator_Quick_Reference.md)** - Daily development reference
- **[Full Guide](Dynamic_Board_Asset_Generator_Guide.md)** - Complete system documentation
- **[Migration Guide](Board_System_Migration_Guide.md)** - Migration from old system
- **[Individual Board System](Individual_Board_Scripts_System_Guide.md)** - Board development guide

---

*This documentation index should be updated as new documentation is added or existing documentation is modified.*
