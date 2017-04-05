using System;

namespace Enlearn.Client
{
    /// <summary>
    /// Represents a client which can be used for problem adaptation. 
    /// These calls are Asynchronous and will call a callback when the function returns.
    /// 
    /// In addition to their normal responses, all calls may be called back with the following errors:
    /// 
    /// {"error":"in progress"} :
    /// Our client only supports making one request to the Enlearn service at a time. If a call is made before a response is received, this error will be returned.
    ///
    /// {"error":"jni", "exception":  "diagnostic exception text" } :
    /// This indicates that the Enlearn Unity package had a problem at the JNI layer. Typically this means that the service APK isn't installed, but other problems connecting to the Enlearn service could generate exceptions as well. This error indicates that something is unrecoverably wrong, and that retries will not work. The exception that generated this error is included for diagnostic purposes.
    ///
    /// {"error":"timeout"} :
    /// A timeout occurred.
    /// </summary>
    public interface IUnityEnlearnClient
    {
        /// <summary>
        ///     Returns the next ChickenProblem based on adaptation.
        /// </summary>
        /// <param name="studentId">A Guid identifying the student involved</param>
        /// <param name="callback">A callback that will be called when the function returns
        /// <param name="Callback argument"> A JSON string representing the next ChickenProblem. Example:
        /// {"tutorials": Array,
        ///   "expression" : "2 + 1" , 
        ///   "tensCount" :  0 ,
        ///   "onesCount" :  2 ,
        ///   "tensColumnEnabled" : true , 
        ///   "tensQueueEnabled": true ,
        ///   "onesQueueEnabled": true,"
        ///   "useNumberPad": bool, 
        ///   "twoPartProblem": bool, 
        ///   "seatbelts": bool}.
        /// Errors will also be returned in a JSON string with the format:
        /// {"error": "reason"}
        /// </param></param>
        void GetNextProblem(Guid studentId, Action<string> callback);
        
        
        /// <summary>
        ///     Logs student actions.
        /// </summary>
        /// <param name="studentId">A Guid identifying the student involved</param>
        /// <param name="studentActionString">A JSON string representing the student action. Examples:
        ///  {"action" : "ProblemCompleted", "data":{"result":"correct"}}
        ///  {"action" : "ProblemCompleted", "data":{"result":"incorrect"}}</param>
        /// <param name="callback">A callback that will be called when the function returns
        /// <param name="Callback argument"> A JSON string representing the next Hint. Example: { "nextHint" : "highlightAnswer" }.
        /// Errors will also be returned in a JSON string with the format:
        /// {"error": "reason"}
        /// </param></param>
        void LogStudentActions(Guid studentId, string studentActionString, Action<string> callback);
        
        
        /// <summary>
        /// Update student information for the given student
        /// </summary>
        /// <param name="studentId">A Guid identifying the student</param>
        /// <param name="studentInfo">A JSON string representing the data to save.  Keys and values are expected to be strings.  
        /// Example: {"name": "Bob", "age": "10", "grade": "5",...}</param>
        void UpdateStudentInfo(Guid studentId, string studentInfo);
    }
}