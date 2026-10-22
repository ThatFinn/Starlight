using Starlight.Storage;

namespace Starlight.Utils;

public static class InventoryEUtil
{
    public static int GetAllSlotCount() => sceneContext.PlayerState.Ammo.Slots.Count;
    public static List<int> GetUnlockedSlots()
    {
        var unlocked = new List<int>();
        var i = -1;
        foreach (var slot in sceneContext.PlayerState.Ammo.Slots)
        {
            i++;
            if (slot.IsUnlocked) unlocked.Add(i);
        }
        return unlocked;
    }
    
    public static int GetSelectedSlotIndex() => sceneContext.PlayerState.Ammo._selectedAmmoIdx;

    public static void AddSlotItemCount(int slotIndex, int additionalCount, bool allowOverflow = false)
    {
        var slot = sceneContext.PlayerState.Ammo.Slots[slotIndex];
        if (!slot.Id) return;
        var newCount = slot.Count + additionalCount;
        if(!allowOverflow && newCount > slot.MaxCount) newCount = slot.MaxCount; 
        if(newCount <= 0) slot.Clear();
        else slot.Count = newCount;
    }
    public static void RemoveSlotItemCount(int slotIndex, int removeCount, bool allowOverflow = false)
    {
        var slot = sceneContext.PlayerState.Ammo.Slots[slotIndex];
        if (!slot.Id) return;
        var newCount = slot.Count - removeCount;
        if(!allowOverflow && newCount > slot.MaxCount) newCount = slot.MaxCount; 
        if(newCount <= 0) slot.Clear();
        else slot.Count = newCount;
    }
    
    public static void SetSlotItemCount(int slotIndex, int newCount, bool allowOverflow = false)
    {
        var slot = sceneContext.PlayerState.Ammo.Slots[slotIndex];
        if (!slot.Id) return;
        var _newCount = newCount;
        if(!allowOverflow && _newCount > slot.MaxCount) _newCount = slot.MaxCount; 
        if(_newCount <= 0) slot.Clear();
        else slot.Count = _newCount;
    }
    public static void ClearSlot(int slotIndex) => sceneContext.PlayerState.Ammo.Slots[slotIndex].Clear();
    public static int GetSlotItemCount(int slotIndex) => sceneContext.PlayerState.Ammo.GetSlotCount(slotIndex);
    public static int GetSlotMaxItemCount(int slotIndex) => sceneContext.PlayerState.Ammo.GetSlotMaxCount(slotIndex);
    public static int GetSlotItemSpaceLeft(int slotIndex) => sceneContext.PlayerState.Ammo.GetSlotMaxCount(slotIndex)-sceneContext.PlayerState.Ammo.GetSlotCount(slotIndex);

    public static bool GetIsSlotUnlocked(int slotIndex) => sceneContext.PlayerState.Ammo.Slots[slotIndex].IsUnlocked;
    public static bool GetIsSlotEmpty(int slotIndex) => !sceneContext.PlayerState.Ammo.Slots[slotIndex].Id;

    public static IdentifiableType[] GetSlotAllowedIdentifiableTypes(int slotIndex)
    {
        var slot = sceneContext.PlayerState.Ammo.Slots[slotIndex];
        var list = new List<IdentifiableType>();
        var remove = slot.Definition.SlotBlockList;
        foreach (var identType in slot.Definition.SlotTypeGroup.GetAllMembersHashSet())
            if(!remove.Contains(identType))
                list.Add(identType);
        return list.ToArray();
    }
    public static StarlightSlotItemInfo GetStarlightSlotInfo(int slotIndex)
    {
        var slot = sceneContext.PlayerState.Ammo.Slots[slotIndex];
        var info = new StarlightSlotItemInfo()
        {
            Count = slot.Count,
            Emotions = slot.Emotions,
            IdentifiableType = slot.Id,
            //SaveSet = slot.Appearance,
        };
        if (SupportRadiant.HasFlag())
        {
            dynamic dynamicSlot = slot;
            info.IsRadiant = dynamicSlot.Radiant;
        }
        return info;
    }
    public static void SetStarlightSlotInfo(int slotIndex, StarlightSlotItemInfo info)
    {
        var slot = sceneContext.PlayerState.Ammo.Slots[slotIndex];
        if(info==null||!info.IdentifiableType)
        {
            slot.Clear();
            return;
        }
        slot._id = info.IdentifiableType;
        //slot.Appearance = info.SaveSet;
        slot.Appearance = info.IdentifiableType.GetAppearanceSet();
        slot.Count = info.Count;
        slot.Emotions = info.Emotions;

        if (!SupportRadiant.HasFlag()) return;
        dynamic dynamicSlot = slot;
        dynamicSlot.Radiant = info.IsRadiant;
        if(dynamicSlot.Radiant && info.IsRadiant)
        {
            //Refresh the appearance in the slot
            var count = slot.Count;
            slot._count++;
            slot.Count = count;
            ExecuteInTicks(() =>
            {
                var execGetter = slot.Count;
            },1);
        }
        dynamicSlot.Metadata.Radiant = info.IsRadiant;
        
    }
    
    
    public static int GetStarlightSlotItemPossibleSlot(StarlightSlotItemInfo info)
    {
        if (info == null || !info.IdentifiableType) return -1;

        var slotID = -1;
        var i = -1;
        foreach (var slot in sceneContext.PlayerState.Ammo.Slots)
        {
            i++;
            if (!slot.IsUnlocked) continue;
            if (!slot.Id) continue;
            if (slot.Id.ReferenceId != info.IdentifiableType.ReferenceId) continue;
            dynamic dynamicSlot = slot;
            if(SupportRadiant.HasFlag())
                if (dynamicSlot.Radiant != info.IsRadiant) continue;
            slotID = i;
            break;
        }
        if (slotID == -1)
        {
            i = -1;
            foreach (var slot in sceneContext.PlayerState.Ammo.Slots)
            {
                i++;
                if (!slot.IsUnlocked) continue;
                if (slot.Id) continue;
                if (!slot.Definition.IsAllowed(info.IdentifiableType)) continue;
                slotID = i;
                break;
            }
        }

        return slotID;
    }
    
    
    
}