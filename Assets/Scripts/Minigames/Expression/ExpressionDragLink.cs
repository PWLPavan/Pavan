using UnityEngine;
using System.Collections.Generic;
using System;
using FGUnity.Utils;

namespace Minigames
{
    /// <summary>
    /// Updates an expression's symbols from DragHolder objects.
    /// </summary>
    public class ExpressionDragLink : MonoBehaviour
    {
        /// <summary>
        /// The Expression to update.
        /// </summary>
        public Expression Expression;

        /// <summary>
        /// What position in the expression this maps to.
        /// </summary>
        public int PieceIndex;

        /// <summary>
        /// The DragHolder this maps to.
        /// </summary>
        public DragHolder DragHolder;

        private void Start()
        {
            Assert.True(DragHolder.MaxAllowed == 1, "DragHolder can only hold one piece.");
            DragHolder.OnObjectAdded += OnHolderAdded;
            DragHolder.OnObjectRemoved += OnHolderRemoved;
        }

        private void OnHolderAdded(DragHolder inHolder, DragObject inObject)
        {
            ExpressionPiece piece = inObject.GetComponent<ExpressionPiece>();
            Expression[PieceIndex] = piece;
        }

        private void OnHolderRemoved(DragHolder inHolder, DragObject inObject)
        {
            ExpressionPiece piece = inObject.GetComponent<ExpressionPiece>();
            if (Expression[PieceIndex] == piece)
                Expression[PieceIndex] = null;
        }
    }
}
