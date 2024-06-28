using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Linq;

public class DebugController : MonoBehaviour
{
    public PlayerMain player;
    public Camera_Behavior cam;
    public CameraZoom camZoom;
    public GameManager gameManager;

    bool showConsole;
    bool showHelp;

    string input;

    public List<object> commandList;

    public static DebugCommand HELP;
    public static DebugCommand<string, int> SPAWN_MOB;
    public static DebugCommand<string, int> SPAWN_OBJ;
    public static DebugCommand<string, int> SPAWN_ITEM;
    public static DebugCommand<string, int> GIVE_ITEM;
    public static DebugCommand<string> SET_BIOME;
    public static DebugCommand GODMODE;
    public static DebugCommand SPAWN_PBASE;
    public static DebugCommand SPAWN_RAID;
    public static DebugCommand SPREAD;
    public static DebugCommand FREECRAFTING;
    public static DebugCommand SUPERSPEED;

    public void OnToggleDebug(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ToggleConsole();
        }
    }

    public void OnReturn (InputAction.CallbackContext context)
    {
        if (showConsole && context.performed)
        {
            HandleInput();
            if (input != "help")
            {
                ToggleConsole();
            }
            input = "";
        }
    }

    private void ToggleConsole()
    {
        showConsole = !showConsole;
        showHelp = false;
        cam.controlsEnabled = !cam.controlsEnabled;
        camZoom.controlsEnabled = !camZoom.controlsEnabled;
        if (showConsole)
        {
            player.StateMachine.ChangeState(player.waitingState);//change to a do nothing state...
            Time.timeScale = 0;
        }
        else
        {
            player.StateMachine.ChangeState(player.StateMachine.previousPlayerState);
            Time.timeScale = 1;
        }
    }

    private void Awake()
    {
        HELP = new DebugCommand("help", "Shows a list of all commands", "help", () =>
        {
            showHelp = true;
        });

        SPAWN_MOB = new DebugCommand<string, int>("spawn_mob", "Spawns a mob with X amount at mouse position", "spawn_mob <mobType>, <amount>", (mob, amount) =>
        {
            if (amount == 0)
            {
                amount = 1;
            }

            if (MobObjArray.Instance.SearchMobList(mob) != null)
            {
                for (int i = 0; i < amount; i++)
                {
                    RealMob.SpawnMob(GetMousePos(), new Mob { mobSO = MobObjArray.Instance.SearchMobList(mob) });
                    Debug.Log("Mob spawned");
                }
            }
            else
            {
                Debug.LogError($"Invalid mob name: {mob}");
            }
        });

        SPAWN_OBJ = new DebugCommand<string, int>("spawn_obj", "Spawns a world object with X amount at mouse position", "spawn_obj <objType>, <amount>", (obj, amount) =>
        {
            if (amount == 0)
            {
                amount = 1;
            }

            if (WosoArray.Instance.SearchWOSOList(obj) != null)
            {
                for (int i = 0; i < amount; i++)
                {
                    RealWorldObject.SpawnWorldObject(GetMousePos(), new WorldObject { woso = WosoArray.Instance.SearchWOSOList(obj) });
                    Debug.Log("Object spawned!");
                }
            }
            else
            {
                Debug.LogError($"Invalid object name: {obj}");
            }
        });

        SPAWN_ITEM = new DebugCommand<string, int>("spawn_item", "Spawns an item with X amount at mouse position", "spawn_obj <itemType>, <amount>", (item, amount) =>
        {
            if (amount == 0)
            {
                amount = 1;
            }

            if (ItemObjectArray.Instance.SearchItemList(item) != null)
            {
                RealItem.SpawnRealItem(GetMousePos(), new Item { itemSO = ItemObjectArray.Instance.SearchItemList(item), amount = amount , uses = ItemObjectArray.Instance.SearchItemList(item).maxUses});
                Debug.Log("item spawned!");
            }
            else
            {
                Debug.LogError($"Invalid item name: {item}");
            }
        });

        GIVE_ITEM = new DebugCommand<string, int>("give_item", "Gives the player an item with X amount directly into their inventory", "give_item, <itemType>, <amount>", (item, amount) =>
        {
            if (amount == 0)
            {
                amount = 1;
            }

            if (ItemObjectArray.Instance.SearchItemList(item) != null)
            {
                player.inventory.AddItem(new Item { itemSO = ItemObjectArray.Instance.SearchItemList(item), amount = amount, uses = ItemObjectArray.Instance.SearchItemList(item).maxUses }, player.transform.position);
            }
        });

        SET_BIOME = new DebugCommand<string>("set_biome", "World generator will only generate this biome from now on", "set_biome, <biomeName>", biome =>
        {
            gameManager.world.forceBiome = true;
            if (Enum.TryParse<Cell.BiomeType>(biome, out gameManager.world.forcedBiome))
            {
                Debug.Log("World converted to " + biome);
            }
            else
            {
                Debug.LogError("Invalid biome name: " + biome);
            }
        });

        GODMODE = new DebugCommand("godmode", "No longer take damage and one-shot any enemy that hits you. Type again to toggle off.", "godmode", () =>
        {
            gameManager.ToggleGodMode(true);
        });

        SPAWN_PBASE = new DebugCommand("spawn_pbase", "Generates a new parasite base", "spawn_pbase", () =>
        {
            ParasiteFactionManager.Instance.SpawnNewParasiteBase();
        });

        SPAWN_RAID = new DebugCommand("start_raid", "Starts a parasite raid", "start_raid", () =>
        {
            ParasiteFactionManager.StartParasiteRaid();
        });

        SPREAD = new DebugCommand("spread", "Spreads parasite biome", "spread", () =>
        {
            ParasiteFactionManager.Instance.SpreadParasiteBiome();
        });

        FREECRAFTING = new DebugCommand("freecrafting", "Unlock all recipes and craft for free. Type again to toggle off.", "freecrafting", () =>
        {
            gameManager.ToggleFreeCrafting();
        });

        SUPERSPEED = new DebugCommand("superspeed", "Time moves 5x faster! Type again to toggle off.", "superspeed", () =>
        {
            gameManager.ToggleSpeedMode(true);
        });

        commandList = new List<object>
        {
            HELP,
            SPAWN_MOB,
            SPAWN_OBJ,
            SPAWN_ITEM,
            GIVE_ITEM,
            SET_BIOME,
            GODMODE,
            SPAWN_PBASE,
            SPAWN_RAID,
            SPREAD,
            FREECRAFTING,
            SUPERSPEED
        };
    }

    private Vector3 GetMousePos()
    {
        Ray ray = player.mainCam.ScreenPointToRay(player.playerInput.PlayerDefault.MousePosition.ReadValue<Vector2>());
        RaycastHit rayHit;
        Physics.Raycast(ray, out rayHit);
        rayHit.point = new Vector3(rayHit.point.x, 0, rayHit.point.z);
        return rayHit.point;
    }

    Vector2 scroll;

    private void OnGUI()
    {
        float y = 0;

        if (showHelp)
        {
            GUI.Box(new Rect(0, y, Screen.width, 100), "");

            Rect viewPort = new Rect(0, 0, Screen.width - 30, 20 * commandList.Count);

            scroll = GUI.BeginScrollView(new Rect(0, y + 5f, Screen.width, 90), scroll, viewPort);

            for (int i = 0; i < commandList.Count; i++)
            {
                DebugCommandBase command = commandList[i] as DebugCommandBase;

                string label = $"{command.commandFormat} - {command.commandDescription}";

                Rect labelRect = new Rect(5, 20 * i, viewPort.width - 100, 20);

                GUI.Label(labelRect, label);
            }

            GUI.EndScrollView();

            y += 100;
        }

        if (showConsole)
        {
            GUI.Box(new Rect(0, y, Screen.width, 30), "");
            GUI.backgroundColor = new Color(0, 0, 0, 0);
            input = GUI.TextField(new Rect(10f, y + 5f, Screen.width - 20f, 20f), input);
        }
    }

    private void HandleInput()
    {
        string[] properties = input.Split(' ');

        for (int i = 0; i < commandList.Count; i++)
        {
            DebugCommandBase commandBase = commandList[i] as DebugCommandBase;

            if (input.Contains(commandBase.commandId))
            {
                if (commandList[i] as DebugCommand != null)
                {
                    (commandList[i] as DebugCommand).Invoke();
                }
                else if (commandList[i] as DebugCommand<string> != null)
                {
                    (commandList[i] as DebugCommand<string>).Invoke(properties[1]);
                }
                else if (commandList[i] as DebugCommand<string, int> != null)
                {
                    if (properties.Length <= 2)
                    {
                        (commandList[i] as DebugCommand<string, int>).Invoke(properties[1], 0);
                    }
                    else if (properties.Length == 3)
                    {
                        (commandList[i] as DebugCommand<string, int>).Invoke(properties[1], int.Parse(properties[2]));
                    }
                }
            }
        }
    }
}
