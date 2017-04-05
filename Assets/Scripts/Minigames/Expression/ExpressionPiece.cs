using UnityEngine;
using System.Collections.Generic;
using System;
using FGUnity.Utils;

namespace Minigames
{
    [DisallowMultipleComponent]
    public abstract class ExpressionPiece : MonoBehaviour
    {
        public abstract ExpressionPieceType Type { get; }
    }

    public enum ExpressionPieceType
    {
        Number,
        Operator
    }
}
