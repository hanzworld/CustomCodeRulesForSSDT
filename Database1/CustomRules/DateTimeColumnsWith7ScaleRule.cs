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
            TSqlObject dataType = something.GetReferenced(Column.DataType).SingleOrDefault();

            if (dataType == null)
            {
                return SqlDataType.Unknown;
            }

            // Some data types don't cleanly convert
            switch (dataType.Name.Parts.Last())
            {
                case "hierarchyid":
                case "geometry":
                case "geography":
                    return SqlDataType.Variant;
            }

            // Note: User Defined Data Types (UDDTs) are not supported during deployment of memory optimized tables. 
            // The code below handles UDDTs in order to show how properties of a UDDT should be accessed and because
            // the model validation does not actually block this syntax at present there are tests that validate this behavior. 

            // User Defined Data Types and built in types are merged in the public model.
            // We want to examine the built in type: for user defined data types this will be
            // found by accessing the DataType.Type object, which will not exist for a built in type
            TSqlObject builtInType = dataType.GetReferenced(DataType.Type).SingleOrDefault();
            if (builtInType != null)
            {
                dataType = builtInType;
            }

            return dataType.GetProperty<SqlDataType>(DataType.SqlDataType);
            //TSqlObject builtInType = dataType.GetReferenced(DataType.Type).SingleOrDefault();

        }
    }
}
