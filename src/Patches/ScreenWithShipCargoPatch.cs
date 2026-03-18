using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MGSC;
using UnityEngine;

namespace QM_SortAllTabs
{
    /// <summary>
    /// Adds a "Sort All" button next to the existing per-tab sort button.
    /// Clicking it redistributes items from all sortable regular cargo tabs and
    /// then sorts every regular tab. Fridge and recycler tabs are untouched.
    /// </summary>
    [HarmonyPatch(typeof(ScreenWithShipCargo), "Awake")]
    static class ScreenWithShipCargoPatch
    {
        static void Postfix(ScreenWithShipCargo __instance)
        {
            try
            {
                AddSortAllButton(__instance);
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError("Failed to add Sort All button.");
                Plugin.Logger.LogException(ex);
            }
        }

        static void AddSortAllButton(ScreenWithShipCargo screen)
        {
            CommonButton sortBtn   = screen._sortArsenalButton;
            CommonButton manageBtn = screen._manageTabButton;

            if (sortBtn == null)
            {
                Plugin.Logger.LogError("_sortArsenalButton not found on ScreenWithShipCargo.");
                return;
            }

            Transform parent = sortBtn.transform.parent;

            // Guard: don't add the button twice (e.g. if this screen is re-awakened).
            if (parent.Find("SortAllArsenalButton") != null)
                return;

            // Clone the existing sort button so we inherit its look and feel.
            GameObject newBtnGO = UnityEngine.Object.Instantiate(sortBtn.gameObject, parent);
            newBtnGO.name = "SortAllArsenalButton";

            // Update the button caption and wire up the handler.
            CommonButton newBtn = newBtnGO.GetComponent<CommonButton>();
            if (newBtn.CaptionLabel != null)
                newBtn.CaptionLabel.ChangeLabel(Plugin.SortAllLocalizationKey);
            newBtn.OnClick += (btn, clickCount) => SortAllTabs(screen);

            // Fix the tooltip: the cloned HintTooltipHandler still points at the
            // sort-current-tab localization key.
            var tooltip = newBtnGO.GetComponent<HintTooltipHandler>();
            if (tooltip != null)
            {
                tooltip._rawValue = false;
                tooltip.SetTag(Plugin.SortAllLocalizationKey);
            }

            // Apply custom sprites loaded from embedded resources.
            Sprite sprNormal  = LoadEmbeddedSprite("QM_SortAllTabs.SortAll_Normal.png");
            Sprite sprHover   = LoadEmbeddedSprite("QM_SortAllTabs.SortAll_Hover.png");
            Sprite sprPressed = LoadEmbeddedSprite("QM_SortAllTabs.SortAll_Pressed.png");
            if (sprNormal  != null) newBtn.RefreshNormalBackground(sprNormal);
            if (sprHover   != null) newBtn.hoverBgSprite   = sprHover;
            if (sprPressed != null) newBtn.pressedBgSprite = sprPressed;

            RectTransform sortRt   = (RectTransform)sortBtn.transform;
            RectTransform parentRt = (RectTransform)parent;
            RectTransform newRt    = (RectTransform)newBtnGO.transform;

            // Measure the step between the two existing buttons at runtime so this
            // works even if the prefab layout changes in a future game update.
            Vector2 step;
            if (manageBtn != null && manageBtn.transform.parent == parent)
            {
                RectTransform manageRt = (RectTransform)manageBtn.transform;
                step = sortRt.anchoredPosition - manageRt.anchoredPosition;
            }
            else
            {
                step = new Vector2(18f, 0f); // known from the prefab
            }

            // From the prefab: CaptionBlock (parent) uses anchor-stretch and its right
            // edge stops 36px short of CargoBackground's right edge (offsetMax.x = -36).
            // That 36px strip is where the two existing buttons overflow into.
            // Adding a third button requires the strip to grow by one step (18px), so we
            // pull the right edge further in by the same amount.
            // We use offsetMax (not sizeDelta) because CaptionBlock's pivot is (0.5,1):
            // changing sizeDelta would move both edges, shifting the whole block right.
            parentRt.offsetMax += new Vector2(-Mathf.Abs(step.x), 0f);

            // Mirror the anchor, pivot and size of the original sort button, then
            // place the new button one step further along.
            newRt.anchorMin        = sortRt.anchorMin;
            newRt.anchorMax        = sortRt.anchorMax;
            newRt.pivot            = sortRt.pivot;
            newRt.sizeDelta        = sortRt.sizeDelta;
            newRt.anchoredPosition = sortRt.anchoredPosition + step;

            Plugin.Logger.Log("Sort All button added to ScreenWithShipCargo.");
        }

        static Sprite LoadEmbeddedSprite(string resourceName)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                Plugin.Logger.LogError($"Embedded resource not found: {resourceName}");
                return null;
            }
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            stream.Dispose();
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
            tex.filterMode = FilterMode.Point;
            ImageConversion.LoadImage(tex, data);
            return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        static void SortAllTabs(ScreenWithShipCargo screen)
        {
            try
            {
                MagnumCargo cargo     = screen._magnumCargo;
                SpaceTime   spaceTime = screen._spaceTime;

                // Tabs whose IncludeToSort=false keep their items;
                // just sort them in place at the end.
                // Tabs whose IncludeToSort=true have items pooled and
                // redistributed according to per-tab item-class filters.
                int count = Math.Min(cargo.ShipCargo.Count, cargo.Tabs.Count);
                var pooled = new List<BasePickupItem>();

                for (int i = 0; i < count; i++)
                {
                    if (cargo.Tabs[i].IncludeToSort)
                    {
                        pooled.AddRange(cargo.ShipCargo[i].Items);
                        cargo.ShipCargo[i].RemoveAllItems();
                    }
                }

                // AddCargo with tabFilter=true calls FilterCargo, which respects
                // each tab's IncludeToSort flag and item-class exclusion filters,
                // and returns the best-matching tab for every item.
                foreach (BasePickupItem item in pooled)
                {
                    MagnumCargoSystem.AddCargo(
                        cargo, spaceTime, item,
                        specificStorage: null,
                        splittedItem: false,
                        tabFilter: true);
                }

                // Sort every regular cargo tab (Fridge and Recycler are not in ShipCargo).
                foreach (ItemStorage storage in cargo.ShipCargo)
                {
                    storage.SortWithExpandByTypeAndName(spaceTime);
                }

                screen.RefreshView();
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError("Error during Sort All.");
                Plugin.Logger.LogException(ex);
            }
        }
    }
}
