using System;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.Random;
using static UnityEngine.Debug;

public class SacrificialHouse : SoftwareProgram
{

    private int currentPosition, anomalyPosition;

    public List<string> CollectedItems = new List<string>();
    private enum FloorNames
    {
        Attic,
        TopFloor,
        MainFloor,
        LowerFloor,
        Basement
    }

    private struct Room
    {
        public FloorNames Floor { get; private set; }
        public string RoomName { get; private set; }
        public List<string> Items { get; private set; }

        public Room(FloorNames floor, string roomName, List<string> items)
        {
            Floor = floor;
            RoomName = roomName;
            Items = items;
        }

        public override string ToString() => RoomName;
    }

    private Room[] house;

    private static readonly string[] roomNames = new[]
    {
        Enumerable.Range(0, 5).Select(x => $"Attic {new[] { "Far Left", "Middle Left", "Middle", "Middle Right", "Far Right" }[x]}").ToArray(),
        new[] { "Stairs Hallway", "Full Bathroom", "Master Bedroom", "Bedroom A", "Bedroom B" },
        new[] { "Entrance", "Living Room", "Kitchen Left Half", "Kitchen Right Half", "Backyard" },
        new[] { "Family Room Far Left", "Family Room Middle Left", "Half Bathroom", "Family Room Middle Right", "Family Room Far Right" },
        new[] { "Basement", "Laundry Room", "The Circle", "Basement", "Closet" }
    }.SelectMany(x => x).ToArray();

    private Room[] GenerateHouse()
    {
        var floorGrid = Enumerable.Range(0, 5).SelectMany(x => Enumerable.Repeat((FloorNames)x, 5)).ToList();

        return Enumerable.Range(0, 25).Select(x => new Room(floorGrid[x], roomNames[x], itemNames[(int)floorGrid[x]].ToArray().Shuffle().Take(Range(0, 4) + 1).ToList())).ToArray();
    }


    private static readonly string[][] itemNames =
    {
        new[] { "Cursed VHS Tape", "Antique Clock", "Haunted Rosie Doll", "Flintstone", "Old Vinyl Record", "Really Old Computer" }, // Attic
        new[] { "Hunting Rifle", "Shotgun", "Pistol", "Pills", "Deer Antlers", "Keys", "Silly Slots", "Hunter's Sausage", "Used Toothbrush", "Teeth" }, // Hallway upstairs / Upper Full Bathroom / Bedrooms
        new[] { "Mug", "Molded Cheese", "Quantum Crack", "Whiskey", "Enraged Mouse", "Blan", "Gas Mask", "Super Glue" }, // Main area / Kitchen / Towards the backyard / Living Room
        new[] { "Cigar", "Trimmed Weed", "Holy Bible", "Battery-Powered Television", "Flashlight", "Batteries" }, // Family room / Lower Bathroom area
        new[] { "Broken Radio", "Blood-Stained Clothing", "Axe", "Dead Man's Hand", "Cross", "Chains", "Folding Chair", "Handcuffs", "Bleach" } // Basement/Underground area
    };

    private List<string> itemsToGet;

    private int?[] GetOrthrogonalAdjacentCells(int pos)
    {
        var row = pos / 5;
        var col = pos % 5;

        return new[] { row - 1, col + 1, row + 1, col - 1 }.Select((x, i) => x < 0 || x > 4 ? (int?)null : ((i % 2 != 0 ? row : x) * 5) + (i % 2 == 0 ? col : x)).ToArray();
    }

    private int?[] GetDiagonalAdjacentCells(int pos)
    {
        var row = pos / 5;
        var col = pos % 5;

        var adj = new[]
        {
            new[] { row - 1, col - 1 },
            new[] { row - 1, col + 1 },
            new[] { row + 1, col + 1 },
            new[] { row + 1, col - 1 }
        };

        return adj.Select(x => x.Any(y => y < 0 || y > 4) ? (int?)null : x[0] * 5 + x[1]).ToArray();
    }

    private int ManhattanDistance(int pos, int neighbor) =>
        (Math.Max(pos / 5, neighbor / 5) - Math.Min(pos / 5, neighbor / 5)) + (Math.Max(pos % 5, neighbor % 5) - Math.Min(pos % 5, neighbor % 5));

    public SacrificialHouse(SoftwareProgramType programType, int programIndex) : base(programType, programIndex)
    {
        Reset();
    }

    public bool KilledOrInvalid;

    public void Reset()
    {
        KilledOrInvalid = false;
        CollectedItems.Clear();
        house = GenerateHouse();
        currentPosition = 10;
        anomalyPosition = Enumerable.Range(0, 25).Where(x => (x != 10) || ManhattanDistance(currentPosition, x) >= 10).PickRandom();
        itemsToGet = house.SelectMany(x => x.Items).Distinct().ToList().Shuffle().Take(3).ToList();
    }

    public string ObtainMessage(bool view = false, bool itemList = false, bool pickingUpItem = false, bool droppingItem = false, string item = null, bool peek = false)
    {
        if (pickingUpItem)
            return $"Item {item} has been picked up and is now in your possession.";

        else if (droppingItem)
            return $"Item {item} has been dropped.";

        else if (view)
        {
            if (itemList)
                return $"Items needed for sacrifice: {itemsToGet.Join(", ")}";

            return CollectedItems.Count == 0 ? "You have no items in your possession." : $"Items collected: {CollectedItems.Join(", ")}";
        }
        else if (peek)
            return $"Items currently in the room are: {house[currentPosition].Items.Join(", ")}";

        if (GetOrthrogonalAdjacentCells(currentPosition).Concat(GetDiagonalAdjacentCells(currentPosition)).Contains(anomalyPosition))
            return $"You are currently in {house[currentPosition]}. You can enter into {GetOrthrogonalAdjacentCells(currentPosition).Where(x => x != null).Select(x => house[x.Value]).Join(", ")}. The anomaly is nearby. You have to hide.";

        return $"You are currently in {house[currentPosition]}. You can enter into {GetOrthrogonalAdjacentCells(currentPosition).Where(x => x != null).Select(x => house[x.Value]).Join(", ")}.";
    }

    public bool Killed() => currentPosition == anomalyPosition;
    public bool InvalidSacrifice() => !CheckInformation(CollectedItems);

    public bool CheckCommandInput(string command, out string message)
    {
        var split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

        string item = null;

        if (split.Length == 0)
            goto commandinvalid;

        switch (split[0])
        {
            case "GO":
                if (split.Length == 1)
                    goto default;
                else if ("TO".Contains(split[1]))
                {
                    if (split.Length == 2)
                        goto default;

                    if (!roomNames.Contains(split.Skip(2).Join()) && !GetOrthrogonalAdjacentCells(currentPosition).Where(x => x != null).Select(x => house[x.Value]).Any(x => x.RoomName.ToUpperInvariant().Contains(split.Skip(2).Join())))
                        goto default;

                    goto movecommandvalid;
                }
                goto default;
            case "PICKUP":
                if (split.Length == 1)
                    goto default;
                else if (!itemNames.SelectMany(x => x.Select(y => y.ToUpperInvariant())).Contains(split.Skip(1).Join()) && !house[currentPosition].Items.Contains(split.Skip(1).Join()))
                    goto default;

                if (CollectedItems.Count == 3)
                {
                    message = "You cannot carry more than three items!";
                    return false;
                }

                item = house[currentPosition].Items.First(x => x.ToUpperInvariant().Contains(split.Skip(1).Join()));
                CollectedItems.Add(item);
                house[currentPosition].Items.Remove(item);

                message = ObtainMessage(pickingUpItem: true, item: item);
                return true;
            case "DROP":
                if (split.Length == 1)
                    goto default;
                else if (CollectedItems.Count == 0)
                {
                    message = "There are no items to drop!";
                    return false;
                }
                else if (!CollectedItems.Any(x => x.ToUpperInvariant().Contains(split.Skip(1).Join())))
                goto default;


                item = CollectedItems.First(x => x.ToUpperInvariant().Contains(split.Skip(1).Join()));
                house[currentPosition].Items.Add(item);
                CollectedItems.Remove(item);
                message = ObtainMessage(droppingItem: true, item: item);
                return true;
            case "HIDE":
                if (split.Length > 1)
                    goto default;
                else if (!GetOrthrogonalAdjacentCells(currentPosition).Concat(GetDiagonalAdjacentCells(currentPosition)).Contains(anomalyPosition))
                {
                    message = "The anomaly isn't nearby yet.";
                    return false;
                }
                anomalyPosition = Enumerable.Range(0, 25).Where(x => (x != currentPosition) || ManhattanDistance(currentPosition, x) >= 10).PickRandom();
                message = "You have hidden successfully and the anomaly went somewhere else.";
                return true;
            case "LOOK":
                if (split.Length > 1)
                    goto default;

                message = ObtainMessage(peek: true);
                return true;
            case "VIEW":
                if (split.Length == 1 || split.Length > 3)
                    goto default;

                if (split[1].Contains("ITEMS"))
                {
                    message = ObtainMessage(view: true);
                    return true;
                }
                else if (split.Skip(1).Join().Contains("ITEM LIST"))
                {
                    message = ObtainMessage(view: true, itemList: true);
                    return true;
                }
                goto default;
            case "LOCATION":
                if (split.Length > 1)
                    goto default;

                message = ObtainMessage();
                return true;
            case "SACRIFICE":
                if (split.Length > 1)
                    goto default;
                else if (CollectedItems.Count != 3)
                {
                    message = $"You only have {CollectedItems.Count}/3 items to sacrifice. You cannot proceed.";
                    return false;
                }
                else if (house[currentPosition].RoomName != "The Circle")
                {
                    message = "You are currently not at the circle.";
                    return false;
                }

                KilledOrInvalid = InvalidSacrifice();
                message = !KilledOrInvalid ? "Thank you for your honor." : "This isn't right. We'll sacrifice you instead.";
                return true;
            default:
                goto commandinvalid;
        }

    movecommandvalid:;

        currentPosition = (int)GetOrthrogonalAdjacentCells(currentPosition).Where(x => x != null).First(x => house[x.Value].RoomName.ToUpperInvariant().Contains(split.Skip(2).Join()));
        anomalyPosition = GetOrthrogonalAdjacentCells(anomalyPosition).Concat(GetDiagonalAdjacentCells(anomalyPosition)).Contains(currentPosition) ? currentPosition : (int)GetOrthrogonalAdjacentCells(anomalyPosition).Concat(GetDiagonalAdjacentCells(anomalyPosition)).Where(x => x != null).Where(x => x.Value != currentPosition).PickRandom();

        Log($"{"ABCDE"[currentPosition % 5]}{(currentPosition / 5) + 1}");
        Log($"{"ABCDE"[anomalyPosition % 5]}{(anomalyPosition / 5) + 1}");


        KilledOrInvalid = Killed();

        message = KilledOrInvalid ? "THE ANOMALY CAUGHT YOU!" : ObtainMessage();
        return true;
    commandinvalid:;

        message = "The command you inputted is invalid.";
        return false;
    }

    public override bool CheckInformation(object other)
    {
        if (other is List<string>)
        {
            var collectedItems = other as List<string>;

            return collectedItems.All(itemsToGet.Contains) && collectedItems.Distinct().Count() == 3;
        }

        return false;
    }

    public override string ToString() => $"Items to be collected from the house are: {itemsToGet.Select((x, i) => i == 2 ? $"and {x}" : x).Join(", ")}. Each item can be located in [{house.Where(x => x.Items.Any(itemsToGet.Contains)).Join(", ")}]";
}
