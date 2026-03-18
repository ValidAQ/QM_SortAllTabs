# Sort All Tabs

This mod adds a "sort all cargo tabs" button to the Ship Cargo UI.
The button only affects the normal tabs, not touching the cryochamber or recycler tabs.

## Translations

The mod is localized, but most of translations are currently machine-generated.
If you want to help improve the translations, please let me know.

## Mod Compatibility

This mod patches the following game classes. Other mods that patch the same methods may conflict.

| Class                      | Method  | Patch type |
| -------------------------- | ------- | ---------- |
| `MGSC.ScreenWithShipCargo` | `Awake` | Postfix    |

The mod also shifts the existing CaptionBlock UI element for ArsenalScreen to make room for the new button.
This may cause visual issues if other mods also modify ArsenalScreen.

# Source Code
Source code is available on GitHub at https://github.com/ValidAQ/QM_SortAllTabs

## Changelog

### 1.0.0
* Initial release.