using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField]
    private ShopInventory shopInventory;
    [SerializeField]
    private PartyInventory partyInventory;
    [SerializeField]
    private ItemSellbackPanel sellBackPanel;

    public PartyInventory PartyInventory { get { return partyInventory; } }

    private void Awake()
    {
        shopInventory.EventPurchase += ShopInventoryPurchase;
        shopInventory.EventSellBackSlot += ShopInventorySellBackSlot;
    }

    public void ActivateShopBehaviour()
    {
        DragManager.Instanse.EventStartDraggingInventorySlot += ActivateSellbackPanel;
        DragManager.Instanse.EventEndDraggingInventorySlot += DeactivateSellbackPanel;
        foreach(var slot in partyInventory.InventorySlots)
            slot.EventActivate += SellSingleItem;
    }

    public void DeactivateShopBehaviour()
    {
        DragManager.Instanse.EventStartDraggingInventorySlot -= ActivateSellbackPanel;
        DragManager.Instanse.EventEndDraggingInventorySlot -= DeactivateSellbackPanel;
        foreach (var slot in partyInventory.InventorySlots)
            slot.EventActivate -= SellSingleItem;
    }

    public void SellOutEverything()
    {
        foreach (var slot in partyInventory.InventorySlots)
        {
            int itemsSold = shopInventory.SellSlotBack(slot);

            if (itemsSold != 0)
            {
                PublishProvision(slot, itemsSold, isSellback: true);
                slot.SlotItem.RemoveItems(itemsSold);
            }
        }

        GameEventRecording.PublishEstateSnapshot();
    }

    public void LoadInitialSetup(Quest quest, RaidPartyPanel raidParty)
    {
        shopInventory.UpdateShop(quest);
        partyInventory.LoadInitialSetup(quest, raidParty);
    }

    private void SellSingleItem(InventorySlot slot)
    {
        if (shopInventory.SellSingeItem(slot))
        {
            PublishProvision(slot, 1, isSellback: true);
            partyInventory.DiscardSingleItem(slot);
            GameEventRecording.PublishEstateSnapshot();
        }
    }

    private void ActivateSellbackPanel(InventorySlot slot)
    {
        sellBackPanel.gameObject.SetActive(true);
    }

    private void DeactivateSellbackPanel(InventorySlot slot)
    {
        sellBackPanel.gameObject.SetActive(false);
    }

    private void ShopInventoryPurchase(ShopSlot slot, InventorySlot dropSlot)
    {
        string itemId = slot != null && slot.Item != null ? slot.Item.Id : null;
        int amount = slot != null && slot.Item != null ? slot.Item.Amount : 0;
        int goldCost = slot != null ? slot.Cost : 0;

        if (shopInventory.BuyShopSlot(slot, partyInventory))
        {
            GameEventRecording.PublishEstateProvisionsPurchased(itemId, amount, goldCost, false);
            partyInventory.DistributeFromShopItem(slot, dropSlot);
            GameEventRecording.PublishEstateSnapshot();
        }
    }

    private void ShopInventorySellBackSlot(InventorySlot slot)
    {
        int itemsSold = shopInventory.SellSlotBack(slot);

        if (itemsSold != 0)
        {
            PublishProvision(slot, itemsSold, isSellback: true);
            slot.SlotItem.RemoveItems(itemsSold);
            GameEventRecording.PublishEstateSnapshot();
        }
    }

    private static void PublishProvision(InventorySlot slot, int amount, bool isSellback)
    {
        if (slot == null || slot.SlotItem == null || slot.SlotItem.Item == null)
            return;

        GameEventRecording.PublishEstateProvisionsPurchased(
            slot.SlotItem.Item.Id, amount, 0, isSellback);
    }
}