using System;
using System.Collections.Generic;

public class Inventory {
    private List<ItemDefinition> items = new();
    public event Action<ItemDefinition> OnItemAdded;
    public Inventory() {
        this.items = new List<ItemDefinition>();
    }

    public void AddItem(ItemDefinition item) {
        items.Add(item);
        OnItemAdded?.Invoke(item);
    }

    public IReadOnlyList<ItemDefinition> Items => items;

    public void Reset() {
        items.Clear();
    }
}
