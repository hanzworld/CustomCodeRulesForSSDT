using System;
using System.IO;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;

namespace ClassLibrary1
{
    public class Class1
    {

        /// <summary>
        /// Runs the model filtering example. This shows how to filter a model and save a new
        /// dacpac with the updated model. You can also update the model in the existing dacpac;
        /// the unit tests in TestFiltering.cs show how this is performed.
        /// </summary>
        public static void RunAnalysisExample()
        {

            // Given a model with objects that use "dev", "test" and "prod" schemas
            string resultsFilePath = GetFilePathInCurrentDirectory("scriptResults.xml");
           
            using (TSqlModel model = TSqlModel.LoadFromDacpac(GetFilePathInCurrentDirectory("Database1.dacpac"),
                new ModelLoadOptions(DacSchemaModelStorageType.Memory, loadAsScriptBackedModel: true)))
            {

                // Analyze scripted model
                // Creating a default service will run all discovered rules, treating issues as Warnings.
                // To configure which rules are run you can pass a CodeAnalysisRuleSettings object to the service
                // or as part of the CodeAnalysisServiceSettings passed into the factory method. Examples of this
                // can be seen in the RuleTest.CreateCodeAnalysisService method.

                CodeAnalysisService service = new CodeAnalysisServiceFactory().CreateAnalysisService(model.Version);
                service.ResultsFile = resultsFilePath;
                
                CodeAnalysisResult result = service.Analyze(model);
                
                Console.WriteLine("Code Analysis with output file {0} complete, analysis succeeded? {1}", 
                    resultsFilePath, result.AnalysisSucceeded);
                PrintProblemsAndValidationErrors(model, result);
            }

        }

        private static string GetFilePathInCurrentDirectory(string fileName)
        {
            return Path.Combine(Environment.CurrentDirectory, fileName);
        }

        private static void PrintProblemsAndValidationErrors(TSqlModel model, CodeAnalysisResult analysisResult)
        {
            Console.WriteLine("-----------------");
            Console.WriteLine("Outputting validation issues and problems");
            foreach (var issue in model.Validate())
            {
                Console.WriteLine("\tValidation Issue: '{0}', Severity: {1}",
                    issue.Message,
                    issue.MessageType);
            }

            foreach (var problem in analysisResult.Problems)
            {
                Console.WriteLine("\tCode Analysis Problem: '{0}', Severity: {1}, Source: {2}, StartLine/Column [{3},{4}]",
                    problem.ErrorMessageString,
                    problem.Severity,
                    problem.SourceName,
                    problem.StartLine,
                    problem.StartColumn);
            }
            Console.WriteLine("-----------------");
        }


    }
}
