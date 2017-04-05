using UnityEngine;
using System.Collections.Generic;
using System;
using FGUnity.Utils;

namespace Minigames
{
    /// <summary>
    /// Evaluates an expression from the given pieces.
    /// </summary>
    public class Expression : MonoBehaviour
    {
        #region Inspector

        /// <summary>
        /// Desired outcome of the expression.
        /// </summary>
        [Range(0, 99)]
        public int DesiredValue;

        // Symbols in the expression
        [SerializeField]
        private ExpressionPiece[] m_Pieces;

        #endregion

        #region Callbacks

        /// <summary>
        /// Called whenever an ExpressionPiece changes.
        /// </summary>
        public event Action<Expression, int> OnPieceChanged;

        #endregion

        /// <summary>
        /// Gets and sets the pieces of the expression.
        /// </summary>
        public ExpressionPiece this[int inIndex]
        {
            get { return m_Pieces[inIndex]; }
            set
            {
                if (m_Pieces[inIndex] != value)
                {
                    m_Pieces[inIndex] = value;
                    if (OnPieceChanged != null)
                        OnPieceChanged(this, inIndex);
                }
            }
        }

        public int NumPieces { get{ return m_Pieces.Length; } }

        public void SetNumPieces(int inNumPieces)
        {
            Assert.True(inNumPieces >= 1 && (inNumPieces % 2) != 0, "Number of pieces is valid.");
            Array.Resize(ref m_Pieces, inNumPieces);
        }

        /// <summary>
        /// Returns if the expression is valid
        /// and evaluates to the desired result.
        /// </summary>
        public bool Evaluate(out int outCalculatedValue)
        {
            if (!IsValidFormat())
            {
                outCalculatedValue = -1;
                return false;
            }

            // We can simplify each expression into [number]
            // followed by sets of [operator] [number]
            // If we start adding more operators then we'll need
            // to take into account order of operations, which is
            // more complicated, but this should work for now.
            int value = GetNumber(0);

            for(int i = 1; i < m_Pieces.Length; i += 2)
            {
                OperatorType op = GetOperator(i);
                int number = GetNumber(i + 1);
                value += (op == OperatorType.Add ? number : -number);
            }

            outCalculatedValue = value;
            return value == DesiredValue;
        }

        /// <summary>
        /// Returns if the expression is in a valid format.
        /// </summary>
        public bool IsValidFormat()
        {
            Assert.True((m_Pieces.Length % 2) != 0, "Expression has valid number of symbols.", "Must form expression out of odd number of pieces (0 + 1, 2 + 3 + 4) - {0} is invalid.", m_Pieces.Length);

            for (int i = 0; i < m_Pieces.Length; ++i)
            {
                // If we're missing pieces, it's not valid.
                if (m_Pieces[i] == null)
                    return false;

                // Even-numbered parameters should be numbers
                // Odd-numbered parameters should be operators
                ExpressionPieceType expectedSymbol = (i % 2) == 0 ? ExpressionPieceType.Number : ExpressionPieceType.Operator;

                if (m_Pieces[i].Type != expectedSymbol)
                    return false;
            }

            return true;
        }

        private OperatorType GetOperator(int inIndex)
        {
            return ((OperatorPiece)m_Pieces[inIndex]).Operator;
        }

        private int GetNumber(int inIndex)
        {
            return ((NumberPiece)m_Pieces[inIndex]).Value;
        }
    }
}
