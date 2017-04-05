using UnityEngine;
using System.Collections.Generic;
using System;
using FGUnity.Utils;

namespace Minigames
{
    public class OperatorPiece : ExpressionPiece
    {
        public override ExpressionPieceType Type
        {
            get { return ExpressionPieceType.Operator; }
        }

        public OperatorType Operator = OperatorType.Add;
    }

    public enum OperatorType
    {
        Add,
        Subtract
    }
}
