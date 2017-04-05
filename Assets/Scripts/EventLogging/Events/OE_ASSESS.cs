using System;
using System.Collections.Generic;
using SimpleJSON;
using FGUnity.Utils;

namespace Ekstep
{
    public class OE_ASSESS : GenieEvent
    {
        public enum Subject
        {
            NUM,
            LIT,
            COG,
        }
        public enum QuestionType
        {
            PARA,
            WORD_PROBLEM,
            INFER,
            FIB,
            MCQ
        }
        public enum QuestionLevel
        {
            EASY,
            MEDIUM,
            DIFFICULT,
            RARE,
        }
        public enum Pass
        {
            Yes,
            No,
        }

        public enum ExpressionType
        {
            TargetNumber,
            Expression,
            NestMinigame,
            ExpressionMinigame
        }

        public enum OperationType
        {
            add,
            sub,
            target,
            mixed
        }

        [EKS] [RemovedInVersion(2)] Subject subj;
        [EKS] [RemovedInVersion(2)] string[] mc;
        [EKS] [RenamedInVersion(2, "itemid")] string qid;
        [EKS] [RemovedInVersion(2)] QuestionType qtype;
        [EKS] [RemovedInVersion(2)] QuestionLevel qlevel;
        [EKS] Pass pass;
        [EKS] [RemovedInVersion(2)] string[] mmc;
        [EKS] float score;
        [EKS] [RemovedInVersion(2)] float maxscore;
        [EKS] string[] res;
        [EKS] [RemovedInVersion(2)] string[] exres;
        [EKS] [RenamedInVersion(2, "duration")] float length;
        [EKS] [RenamedInVersion(2, "exduration")] float exlength;
        [EKS] [RemovedInVersion(2)] int atmpts;
        [EKS] [RemovedInVersion(2)]  int failedatmpts;
        [EKS] string uri;

        [EXT] string expression;
        [EXT] ExpressionType expressionType;
        [EXT] OperationType operation;
        [EXT] bool isNumberPanel;
        [EXT] bool isSeatbelted;
        [EXT] bool isTensPlane;
        [EXT] HintingType sawHint;
        [EXT] string var1;
        [EXT] string var2;

        public OE_ASSESS(int inLevelIndex, Level inLevel, int inResult, int inAttempts, float inTime, bool inPass)
        {
            this.subj = Subject.NUM;

            this.mc = Microconcepts.EvaluateConcepts(inLevel);

            this.qid = inLevel.qid;

            this.qtype = QuestionType.FIB;
            this.qlevel = EvaluateDifficulty(inLevel);

            this.pass = inPass ? Pass.Yes : Pass.No;

            this.mmc = inPass ? EMPTY_PARAMS : this.mc; // Unfinished(?)

            this.score = inPass ? 1.0f / inAttempts : 0;
            this.maxscore = 1;

            this.res = new string[] { FGUnity.Utils.LookupTables.ToStringLookup(inResult) };
            this.exres = new string[] { FGUnity.Utils.LookupTables.ToStringLookup(inLevel.value) };

            this.length = inTime;
            this.exlength = 0; // How will we calculate this?

            this.atmpts = inAttempts;
            this.failedatmpts = inPass ? inAttempts - 1 : inAttempts;

            this.uri = string.Empty;

            this.expression = inLevel.expression;
            this.expressionType = inLevel.isTargetNumber ? ExpressionType.TargetNumber : ExpressionType.Expression;
            this.operation = inLevel.isTargetNumber ? OperationType.target : (inLevel.isAdditionProblem ? OperationType.add : OperationType.sub);
            this.isNumberPanel = inLevel.useNumberPad;
            this.isSeatbelted = inLevel.seatbelts;
            this.isTensPlane = inLevel.tensColumnEnabled;
            this.sawHint = Session.instance.currentHint;

            this.var1 = inLevel.expTop;
            this.var2 = inLevel.expBottom;
        }

        public OE_ASSESS(Minigames.ExpressionMinigame inMinigame, float inTime)
        {
            this.subj = Subject.NUM;

            this.mc = Microconcepts.EvaluateConcepts(inMinigame);

            this.qid = "ek.n.minigame-expression";

            this.qtype = QuestionType.MCQ;
            this.qlevel = EvaluteDifficulty(inMinigame.Level);

            this.pass = inMinigame.WinState ? Pass.Yes : Pass.No;
            this.mmc = inMinigame.WinState ? EMPTY_PARAMS : this.mc;

            this.score = inMinigame.WinState ? 1.0f : 0;
            this.maxscore = 1;

            this.res = new string[]{ inMinigame.WinState ? inMinigame.Level.Value.ToString() : "" };
            this.exres = new string[] { inMinigame.Level.Value.ToString() };

            this.length = inTime;
            this.exlength = 0.0f;

            this.atmpts = 1;
            this.failedatmpts = inMinigame.WinState ? 0 : 1;

            this.uri = string.Empty;

            this.expression = BuildExpressionMinigameString(inMinigame.Level);
            this.expressionType = ExpressionType.ExpressionMinigame;

            if (inMinigame.Level.FixedOperator)
                this.operation = (inMinigame.Level.Operator == Minigames.OperatorType.Add ? OperationType.add : OperationType.sub);
            else
                this.operation = OperationType.mixed;
        }

        public OE_ASSESS(Minigames.NestTest inNest, float inTime)
        {
            this.subj = Subject.NUM;

            this.mc = Microconcepts.EvaluateConcepts(inNest);

            this.qid = "ek.n.minigame-make10";

            this.qtype = QuestionType.FIB;
            this.qlevel = QuestionLevel.EASY;

            this.pass = inNest.WinState ? Pass.Yes : Pass.No;
            this.mmc = inNest.WinState ? EMPTY_PARAMS : this.mc;

            this.score = inNest.WinState ? 1.0f : 0;
            this.maxscore = 1;

            this.res = new string[] { inNest.Holder.Count.ToStringLookup() };
            this.exres = new string[] { inNest.Holder.NumSeats.ToStringLookup() };

            this.length = inTime;
            this.exlength = 0.0f;

            this.atmpts = 1;
            this.failedatmpts = inNest.WinState ? 0 : 1;

            this.uri = string.Empty;

            this.expression = "Make 10";
            this.expressionType = ExpressionType.NestMinigame;

            this.operation = OperationType.add;
        }

        static private QuestionLevel EvaluateDifficulty(Level inLevel)
        {
            if (inLevel.isSingleDigitProblem)
                return QuestionLevel.EASY;

            // Small subtraction problems start less than 20 and subtract less than 10.
            bool bSmallSub = inLevel.isSubtractionProblem && inLevel.startingTens < 2 && inLevel.valueOnes > inLevel.startingOnes;
            if (bSmallSub || !inLevel.requiresRegrouping)
                return QuestionLevel.MEDIUM;

            return QuestionLevel.DIFFICULT;
        }

        static private QuestionLevel EvaluteDifficulty(Minigames.ExpressionMinigameSettings inSettings)
        {
            if ((inSettings.Categories & Minigames.ExpressionMinigameCategories.Single) > 0)
                return QuestionLevel.EASY;
            if ((inSettings.Categories & Minigames.ExpressionMinigameCategories.Regroup) > 0)
                return QuestionLevel.DIFFICULT;

            return QuestionLevel.MEDIUM;
        }

        static private string BuildExpressionMinigameString(Minigames.ExpressionMinigameSettings inSettings)
        {
            using(PooledStringBuilder stringBuilder = PooledStringBuilder.Create())
            {
                for (int i = 0; i < inSettings.NumValueSlots * 2 - 1; i += 2 )
                {
                    stringBuilder.Builder.Append('_');
                    if (i + 1 < inSettings.NumValueSlots * 2 - 1)
                    {
                        stringBuilder.Builder.Append(' ');
                        if (inSettings.FixedOperator)
                        {

                            if (inSettings.Operator == Minigames.OperatorType.Add)
                                stringBuilder.Builder.Append('+');
                            else
                                stringBuilder.Builder.Append('-');
                        }
                        else
                        {
                            stringBuilder.Builder.Append(" ? ");
                        }
                    }
                }
                stringBuilder.Builder.Append(" = ").Append(inSettings.Value);

                return stringBuilder.Builder.ToString();
            }
        }

        static private readonly string[] EMPTY_PARAMS = new string[0];
    }
}