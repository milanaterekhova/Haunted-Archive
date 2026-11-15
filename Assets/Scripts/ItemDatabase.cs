using System.Collections.Generic;
using UnityEngine;

public static class ItemDatabase
{
    public static List<ItemData> CreateItems()
    {
        return new List<ItemData>
        {
            new ItemData(
                "Candle",
                new Vector2Int(1, 2),
                new Color(0.85f, 0.68f, 0.32f),
                ItemType.Permanent,
                darkness: -2,
                itemDescription: "-2 Darkness. With Rose: -3 Darkness. Feather blocks the Rose bonus.",
                itemSprite: Resources.Load<Sprite>("Items/Candle")
            ),

            new ItemData(
                "Cursed Book",
                new Vector2Int(2, 2),
                new Color(0.48f, 0.30f, 0.65f),
                ItemType.Permanent,
                score: 5,
                darkness: 5,
                itemDescription: "+5 Score, +5 Darkness. With Feather: +3 extra Score.",
                itemSprite: Resources.Load<Sprite>("Items/CursedBook")
            ),

            new ItemData(
                "Black Raven Feather",
                new Vector2Int(1, 3),
                new Color(0.30f, 0.60f, 0.42f),
                ItemType.Permanent,
                score: 3,
                darkness: 3,
                itemDescription: "+3 Score, +3 Darkness. With Book: +3 extra Score for the Book. Blocks Rose bonuses for adjacent Candles.",
                itemSprite: Resources.Load<Sprite>("Items/BlackRavenFeather")
            ),

            new ItemData(
                "Rose in Glass",
                new Vector2Int(2, 3),
                new Color(0.75f, 0.30f, 0.42f),
                ItemType.Permanent,
                score: 2,
                darkness: -3,
                itemDescription: "+2 Score, -3 Darkness. With Candle: Candle changes from -2 to -3 Darkness. Feather blocks this bonus.",
                itemSprite: Resources.Load<Sprite>("Items/RoseInGlass")
            ),

            new ItemData(
                "Tarot Cards",
                new Vector2Int(1, 2),
                Color.magenta,
                ItemType.Permanent,
                itemDescription: "Choose: +10 Score and +5 Darkness OR -8 Darkness.",
                itemSprite: Resources.Load<Sprite>("Items/TarotCards")
            ),

            new ItemData(
                "Crystal Ball",
                new Vector2Int(2, 2),
                Color.cyan,
                ItemType.Permanent,
                itemDescription: "40%: +20 Score. 30%: -10 Darkness. 30%: +8 Darkness.",
                itemSprite: Resources.Load<Sprite>("Items/CrystallBall")
            ),

            new ItemData(
                "Oracle Eye",
                new Vector2Int(2, 2),
                new Color(0.48f, 0.38f, 0.72f),
                ItemType.Permanent,
                itemDescription: "Choose 1 adjacent item. Copies 50% of its Score and Darkness effects.",
                shape: new Vector2Int[]
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(0, 1),
                },
                itemSprite: Resources.Load<Sprite>("Items/Oracle")
            ),

            new ItemData(
                "Gold Potion",
                new Vector2Int(1, 1),
                Color.yellow,
                ItemType.Consumable,
                score: 5,
                itemDescription: "+5 Score. Then disappears.",
                itemSprite: Resources.Load<Sprite>("Items/GoldPotion")
            ),

            new ItemData(
                "Purification Potion",
                new Vector2Int(1, 1),
                Color.white,
                ItemType.Consumable,
                darkness: -5,
                itemDescription: "-5 Darkness. With Book: -3 Darkness. Then disappears.",
                itemSprite: Resources.Load<Sprite>("Items/PurificationPotion")
            )
        };
    }
}
