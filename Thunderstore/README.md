# Description

## Mod made at request for Majestic

## The FishingBonus mod enhances the fishing experience in the game by allowing players and modders to customize the drops obtained from fishing. With this mod, each type of fish can have its own unique set of items that it can drop when caught. These settings are easily configured through a YAML file, giving users the flexibility to:

- **Add or replace default drops**: Decide whether new item drops should be added to the existing default drops of a
  fish or completely replace them.
- **Control drop quantities**: Specify the minimum and maximum number of items a fish can drop.
- **Adjust drop chances**: Set the probability for drops to occur, offering the ability to fine-tune how often items are
  received from fishing.
- **Ensure variety with one of each**: Choose whether a fish can drop one of each item listed or if duplicates of the
  same item can appear in a single catch.
- **Weight items differently**: Influence the likelihood of each item being dropped through relative weight settings,
  allowing for rare or common drops.

This mod opens up new possibilities for game customization and enrichment, making fishing a more rewarding and exciting
activity tailored to the player's or modder's preference.

`Version checks with itself. If installed on the server, it will kick clients who do not have it installed.`

`This mod uses ServerSync, if installed on the server and all clients, it will sync all configs to client`

`This mod uses a file watcher. If the configuration file is not changed with BepInEx Configuration manager, but changed in the file directly on the server, upon file save, it will sync the changes to all clients.`


---

<details>
<summary><b>Installation Instructions</b></summary>

***You must have BepInEx installed correctly! I can not stress this enough.***

### Manual Installation

`Note: (Manual installation is likely how you have to do this on a server, make sure BepInEx is installed on the server correctly)`

1. **Download the latest release of BepInEx.**
2. **Extract the contents of the zip file to your game's root folder.**
3. **Download the latest release of FishingBonus from Thunderstore.io.**
4. **Extract the contents of the zip file to the `BepInEx/plugins` folder.**
5. **Launch the game.**

### Installation through r2modman or Thunderstore Mod Manager

1. **Install [r2modman](https://valheim.thunderstore.io/package/ebkr/r2modman/)
   or [Thunderstore Mod Manager](https://www.overwolf.com/app/Thunderstore-Thunderstore_Mod_Manager).**

   > For r2modman, you can also install it through the Thunderstore site.
   ![](https://i.imgur.com/s4X4rEs.png "r2modman Download")

   > For Thunderstore Mod Manager, you can also install it through the Overwolf app store
   ![](https://i.imgur.com/HQLZFp4.png "Thunderstore Mod Manager Download")
2. **Open the Mod Manager and search for "FishingBonus" under the Online
   tab. `Note: You can also search for "Azumatt" to find all my mods.`**

   `The image below shows VikingShip as an example, but it was easier to reuse the image.`

   ![](https://i.imgur.com/5CR5XKu.png)

3. **Click the Download button to install the mod.**
4. **Launch the game.**

</details>


# Fish Drops Configuration Guide

This guide explains how to customize fish drops in the game using the YAML configuration file provided by the
FishingBonus mod. This feature allows you to tailor the loot that each fish drops upon being caught, including the
chance of dropping, the minimum and maximum number of items dropped, and whether to add these drops to the default ones.

## YAML Structure

Below is the general structure of the YAML configuration file for defining fish drops:

`Please note: This is an example and not the actual file. The actual file will be generated upon first run of the mod by the time you are at the main menu. All fish found in your game will be added to the file automatically.`

The file will be located at `BepInEx/config/Azumatt.FishingBonus.yml` once generated. If you do not see the file, you can create it manually, the current vanilla generated file will look like the one on my github page here: [FishingBonus Example Config](https://github.com/AzumattDev/FishingBonus/blob/master/Example.yml)

```yaml
fishDrops:
  Fish1:
    addToDefaultDrops: false
    dropMin: 1
    dropMax: 1
    dropChance: 1.0
    oneOfEach: false
    drops:
      - item: Stone
        minStack: 1
        maxStack: 2
        relativeWeight: 1.0
      - item: Amber
        minStack: 1
        maxStack: 1
        relativeWeight: 0.9
```

### Parameters

- `addToDefaultDrops`: If `true`, custom drops are added to the fish's default drops. If `false`, the custom drops
  replace the default ones.
- `dropMin`: The minimum number of different items the fish will drop.
- `dropMax`: The maximum number of different items the fish will drop.
- `dropChance`: The chance (0 to 1) that any drops will occur at all.
- `oneOfEach`: If `true`, ensures that only one of each item in the `drops` list will be dropped. If `false`, multiple
  items of the same type can be dropped based on their `relativeWeight`.
- `drops`: A list of items that the fish can drop, each with its own configuration:
    - `item`: The name of the item prefab to drop. This must match the item's internal name in the game.
    - `minStack`: The minimum stack size for the item when it drops.
    - `maxStack`: The maximum stack size for the item when it drops.
    - `relativeWeight`: Influences the likelihood of this item being dropped relative to other items in the list. Higher
      values increase the chance.

## Adding Custom Fish Drops

1. **Identify the Fish**: Determine the internal name of the fish for which you want to customize drops. This name must
   exactly match the fish's name as defined in the game.

2. **Configure Drops**: Follow the structure shown above to add or modify the fish's drops. You can specify as many
   items as you wish under the `drops` section for each fish.

3. **Adjust Drop Behavior**: Use the `addToDefaultDrops`, `dropMin`, `dropMax`, `dropChance`, and `oneOfEach` parameters
   to control how and when drops are added.

4. **Save Changes**: After configuring your fish drops, save the YAML file and ensure it's correctly placed in the mod's
   configuration directory.

5. **Reload/Restart**: Depending on the mod's capabilities, you may need to reload the configuration or restart the game
   for changes to take effect.

## Example

To ensure a fish named `Fish1` always drops between 1 to 2 stones with a high chance, alongside 1 amber with a slightly
lower chance, and these are the only drops (replacing any default drops), your YAML configuration would look like the
example provided above.


<br>
<br>

`Feel free to reach out to me on discord if you need manual download assistance.`

# Author Information

### Azumatt

`DISCORD:` Azumatt#2625

`STEAM:` https://steamcommunity.com/id/azumatt/

For Questions or Comments, find me in the Odin Plus Team Discord or in mine:

[![https://i.imgur.com/XXP6HCU.png](https://i.imgur.com/XXP6HCU.png)](https://discord.gg/qhr2dWNEYq)
<a href="https://discord.gg/pdHgy6Bsng"><img src="https://i.imgur.com/Xlcbmm9.png" href="https://discord.gg/pdHgy6Bsng" width="175" height="175"></a>