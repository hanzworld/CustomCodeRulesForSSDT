using CustomRules;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyDatabaseUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void NoDatetime7Columns()
        {
            TSqlModel model = TSqlModel.LoadFromDacpac(@"..\..\..\Database1\bin\Debug\Database1.dacpac",
                new ModelLoadOptions(DacSchemaModelStorageType.Memory, true));

            var ruleSettings = new CodeAnalysisRuleSettings()
                    {
                        new RuleConfiguration(DateTimeColumnsWith7ScaleRule.RuleId)
                    };
            ruleSettings.DisableRulesNotInSettings = true;
            
            CodeAnalysisService service2 = new CodeAnalysisServiceFactory().CreateAnalysisService(model.Version, new CodeAnalysisServiceSettings()
            {
                RuleSettings = ruleSettings
            });
            service2.ResultsFile = "results.txt";
            CodeAnalysisResult analysisResult = service2.Analyze(model);

            Assert.AreEqual(0, analysisResult.Problems.Count, "Expect 0 problems to be found");



        }

        #region DoesRuleWork?

        [TestMethod]
        public void TestDateTimeColumnWithScale7()
        {
            string[] scripts = new[]
            {
                "CREATE TABLE t1 (c1 DATETIME2(7) NOT NULL)"
            };

            using (TSqlModel model = new TSqlModel(SqlServerVersion.SqlAzure, new TSqlModelOptions()))
            {
                // Adding objects to the model. 
                foreach (string script in scripts)
                {
                    model.AddObjects(script);
                }

                var ruleSettings = new CodeAnalysisRuleSettings()
                {
                    new RuleConfiguration(DateTimeColumnsWith7ScaleRule.RuleId)
                };
                ruleSettings.DisableRulesNotInSettings = true;


                CodeAnalysisService service = new CodeAnalysisServiceFactory().CreateAnalysisService(model.Version,
                    new CodeAnalysisServiceSettings()
                    {
                        RuleSettings = ruleSettings
                    });
                CodeAnalysisResult analysisResult = service.Analyze(model);

                Assert.AreEqual(1, analysisResult.Problems.Count, "Expect 1 problems to be found");
            }
        }

        [TestMethod]
        public void TestDateTimeColumnWithoutScale7()
        {
            string[] scripts = new[]
            {
                "CREATE TABLE t1 (c1 DATETIME2(2) NOT NULL)"
            };

            using (TSqlModel model = new TSqlModel(SqlServerVersion.SqlAzure, new TSqlModelOptions()))
            {
                // Adding objects to the model. 
                foreach (string script in scripts)
                {
                    model.AddObjects(script);
                }

                var ruleSettings = new CodeAnalysisRuleSettings()
                {
                    new RuleConfiguration(DateTimeColumnsWith7ScaleRule.RuleId)
                };
                ruleSettings.DisableRulesNotInSettings = true;
                
                CodeAnalysisService service = new CodeAnalysisServiceFactory().CreateAnalysisService(model.Version,
                    new CodeAnalysisServiceSettings()
                    {
                        RuleSettings = ruleSettings
                    });
                CodeAnalysisResult analysisResult = service.Analyze(model);

                Assert.AreEqual(0, analysisResult.Problems.Count, "Expect 1 problems to be found");
            }
        }


        #endregion

    }
}
