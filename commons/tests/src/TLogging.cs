// using Microsoft.VisualStudio.TestTools.UnitTesting;
// using HIVE.Commons.Logging;
// using System.IO;

// namespace HIVE.Commons.Tests
// {
//     [TestClass]
//     public sealed class TLoggingTest
//     {
//         [TestMethod]
//         public void TestLogInfo()
//         {
//             StringWriter sw = new StringWriter();
//             Console.SetOut(sw);
//             Logger.Log("Info message", 0, LogType.ELogType.Info);
//             string result = sw.ToString().Trim();
//             Assert.IsTrue(result.Length > 0);
//         }

//         [TestMethod]
//         public void TestLogWarning()
//         {
//             StringWriter sw = new StringWriter();
//             Console.SetOut(sw);
//             Logger.Log("Warning message", 0, LogType.ELogType.Warning);
//             string result = sw.ToString().Trim();
//             Assert.IsTrue(result.Length > 0);
//         }

//         [TestMethod]
//         public void TestLogError()
//         {
//             StringWriter sw = new StringWriter();
//             Console.SetOut(sw);
//             Logger.Log("Error message", 0, LogType.ELogType.Error);
//             string result = sw.ToString().Trim();
//             Assert.IsTrue(result.Length > 0);
//         }

//         [TestMethod]
//         public void TestLogDebug()
//         {
//             StringWriter sw = new StringWriter();
//             Console.SetOut(sw);
//             Logger.Log("Debug message", 0, LogType.ELogType.Debug);
//             string result = sw.ToString().Trim();
//             Assert.IsTrue(result.Length > 0);
//         }
//     }
// }
