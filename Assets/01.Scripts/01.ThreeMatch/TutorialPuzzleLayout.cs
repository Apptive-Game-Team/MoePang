using System;
using UnityEngine;

namespace _01.Scripts._01.ThreeMatch
{
    [CreateAssetMenu(fileName = "TutorialPuzzleLayout", menuName = "Scriptable Objects/TutorialPuzzleLayout")]
    public class TutorialPuzzleLayout : ScriptableObject
    {
        public const int Width = 7;
        public const int BoardHeight = 7;
        public const int TotalHeight = 21;
        public const int RefillCountPerColumn = TotalHeight - BoardHeight;

        [Serializable]
        private class Row
        {
            [Tooltip("Left to right: columns 0 through 6.")]
            [SerializeField] private Habitat[] tiles = new Habitat[Width];

            public Habitat GetTile(int column) => tiles[column];

            public void Validate()
            {
                if (tiles == null || tiles.Length != Width)
                    Array.Resize(ref tiles, Width);
            }
        }

        [Tooltip("Top to bottom. Rows 0-13: refill tiles. Rows 14-20: initial board. Row 20 is the bottom.")]
        [SerializeField] private Row[] rows = CreateRows();

        private static Row[] CreateRows()
        {
            var result = new Row[TotalHeight];
            for (int i = 0; i < result.Length; i++)
                result[i] = new Row();
            return result;
        }

        /// <summary>Initial board tile. Column 0 is left; board row 0 is bottom.</summary>
        public Habitat GetInitialTile(int column, int boardRow)
        {
            if (boardRow < 0 || boardRow >= BoardHeight)
                throw new ArgumentOutOfRangeException(nameof(boardRow));

            return GetTile(column, TotalHeight - 1 - boardRow);
        }

        /// <summary>Refill tile for a column. Index 0 is the first tile to enter the board.</summary>
        public Habitat GetRefillTile(int column, int refillIndex)
        {
            if (refillIndex < 0 || refillIndex >= RefillCountPerColumn)
                throw new ArgumentOutOfRangeException(nameof(refillIndex));

            return GetTile(column, RefillCountPerColumn - 1 - refillIndex);
        }

        private Habitat GetTile(int column, int row)
        {
            if (column < 0 || column >= Width)
                throw new ArgumentOutOfRangeException(nameof(column));

            return rows[row].GetTile(column);
        }

        private void OnValidate()
        {
            if (rows == null || rows.Length != TotalHeight)
                Array.Resize(ref rows, TotalHeight);

            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i] == null)
                    rows[i] = new Row();
                rows[i].Validate();
            }
        }
    }
}
