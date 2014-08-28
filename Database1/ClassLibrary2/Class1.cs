//------------------------------------------------------------------------------
// <copyright company="Microsoft">
//   Copyright 2013 Microsoft
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.SqlServer.Dac.CodeAnalysis;
using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibary2
{
    /// <summary>
    /// This is probably the simplest possible example of a rule that uses the public model API to analyze properties of elements.
    /// In this case any table whose name ends in "View" is treated as a problem.
    /// 
    /// This does not use a localized export attribute, unlike the <see cref="AvoidWaitForDelayRule"/> and other rules. It is
    /// not recommended that you use this code in a real world scenario
    /// </summary>
    [ExportCodeAnalysisRule(DateTimeColumnsWith7PrecisionRule.RuleId,
        DateTimeColumnsWith7PrecisionRule.RuleDisplayName,
        Description = "Datetime columns should not be specified with a precision of 7",
        Category = "Other",
        RuleScope = SqlRuleScope.Element)]
    public sealed class DateTimeColumnsWith7PrecisionRule : SqlCodeAnalysisRule
    {
        public const string RuleId = "Rules.DateTimeColumnsWith7Precision";
        public const string RuleDisplayName = "CustomRule1";
        public const string NameEndingInViewMsgFormat = "Table name {0} contains colum {1} which has a datetime precision of 7. This level of precision is unnecessary for our work and wastes storage space.";

        /// <summary>
        /// For Element-scoped rules the SupportedElementTypes must be defined, ideally inside the constructor.
        /// Only objects that match one of these types will be passed to the <see cref="Analyze"/> method, so this
        /// helps avoid the need to iterate over the model and select the object to be processed.
        /// 
        /// This rule only operates on regular Tables.
        /// </summary>
        public DateTimeColumnsWith7PrecisionRule()
        {
            SupportedElementTypes = new[]
            {
               //Column.TypeClass,
               Table.TypeClass
            };
        }

        /// <summary>
        /// Analysis is quite simple - the table's name is examined and if it ends with "View" then a problem
        /// is created
        /// </summary>
        /// <param name="ruleExecutionContext"></param>
        /// <returns></returns>
        public override IList<SqlRuleProblem> Analyze(SqlRuleExecutionContext ruleExecutionContext)
        {
            List<SqlRuleProblem> problems = new List<SqlRuleProblem>();
            TSqlObject column = ruleExecutionContext.ModelElement;
            //if (column != null)
            //{
                //if (IsDateTimeWith7Precision(column.GetProperty<int>(Column.Precision)))
                if (true)
                {
                    // DisplayServices is a useful helper service for formatting names
                    DisplayServices displayServices = ruleExecutionContext.SchemaModel.DisplayServices;
                    string formattedName = displayServices.GetElementName(column, ElementNameStyle.FullyQualifiedName);

                    string problemDescription = string.Format(NameEndingInViewMsgFormat, "test",
                                                              formattedName);
                    SqlRuleProblem problem = new SqlRuleProblem(problemDescription, column);
                    problems.Add(problem);
                }
            //}
            
            return problems;
        }

        private bool IsDateTimeWith7Precision(int precision)
        {
            return precision == 7;
            //return id.HasName && id.Parts.Last().EndsWith("View", StringComparison.OrdinalIgnoreCase);
        }
    }
}
