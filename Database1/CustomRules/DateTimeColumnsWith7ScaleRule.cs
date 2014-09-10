using System;
using System.Linq;
using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using System.Collections.Generic;

namespace CustomRules
{
    /// <summary>
    /// This is probably the simplest possible example of a rule that uses the public model API to analyze properties of elements.
    /// </summary>
    [ExportCodeAnalysisRule(DateTimeColumnsWith7ScaleRule.RuleId,
        DateTimeColumnsWith7ScaleRule.RuleDisplayName,
        Description = "Datetime columns should not be specified with a scale of 7",
        Category = "Other",
        RuleScope = SqlRuleScope.Element)]
    public sealed class DateTimeColumnsWith7ScaleRule : SqlCodeAnalysisRule
    {
        public const string RuleId = "Rules.DateTimeColumnsWith7ScaleRule";
        public const string RuleDisplayName = "CustomRule1";
        public const string DateTime2ColumnWithExcessiveScaleMsgFormat = "Column name {0} has a datetime2 scale of 7. This level of scale is unnecessary for our work and wastes storage space.";

        /// <summary>
        /// For Element-scoped rules the SupportedElementTypes must be defined, ideally inside the constructor.
        /// Only objects that match one of these types will be passed to the <see cref="Analyze"/> method, 
        /// so this helps avoid the need to iterate over the model and select the object to be processed.
        /// 
        /// This rule only operates on regular Tables.
        /// </summary>
        public DateTimeColumnsWith7ScaleRule()
        {
            SupportedElementTypes = new[]
            {
               Table.TypeClass
            };
        }

        /// <summary>
        /// Check if the table has any datetime2 column, and then check if they have a scale of 7 or more.
        /// </summary>
        /// <param name="ruleExecutionContext"></param>
        /// <returns></returns>
        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            List<SqlRuleProblem> problems = new List<SqlRuleProblem>();
            TSqlObject table = ruleExecutionContext.ModelElement;
            if (table != null)
            {
                foreach (var column in table.GetReferenced(Table.Columns))
                {
                    if (IsDateTime2WithExcessiveScale(column))
                    {
                        //DisplayServices is a useful helper service for formatting names
                        DisplayServices displayServices = ruleExecutionContext.SchemaModel.DisplayServices;
                        string formattedName = displayServices.GetElementName(column, ElementNameStyle.FullyQualifiedName);

                        string problemDescription = string.Format(DateTime2ColumnWithExcessiveScaleMsgFormat,
                            formattedName);
                        SqlRuleProblem problem = new SqlRuleProblem(problemDescription, table);
                        problems.Add(problem);
                    }
                }
            }
            return problems;
        }
        
        private bool IsDateTime2WithExcessiveScale(TSqlObject column)
        {
            var scale = column.GetProperty<int>(Column.Scale);
            var dataType = GetDataType(column);

            return (dataType == SqlDataType.DateTime2 && scale > 2);
        }


        private SqlDataType GetDataType(TSqlObject something)
        {
            var dataType = something.GetReferenced(Column.DataType).SingleOrDefault();
            return dataType.GetProperty<SqlDataType>(DataType.SqlDataType);
            //TSqlObject builtInType = dataType.GetReferenced(DataType.Type).SingleOrDefault();

        }
    }
}
