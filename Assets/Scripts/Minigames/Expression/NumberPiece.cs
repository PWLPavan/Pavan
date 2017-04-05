using UnityEngine;
using System.Collections.Generic;
using System;
using FGUnity.Utils;

namespace Minigames
{
    public class NumberPiece : ExpressionPiece
    {
        public override ExpressionPieceType Type
        {
            get { return ExpressionPieceType.Number; }
        }

        [Range(0, 99)]
        public int Value;
    }
}
