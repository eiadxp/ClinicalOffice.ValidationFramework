using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClinicalOffice.ValidationFramework
{
    public class ValidationRules
    {
        readonly List<Func<object, string>> rules = new List<Func<object, string>>();

        public string Validate(object value)
        {
            return string.Concat(rules.Select((r) => r(value) + "\n"))?.Trim();
        }
        #region Objects
        public ValidationRules Required(string errorMessage = "Can not be empty.")
        {
            rules.Add((a) => (a != null) ? "" : errorMessage);
            return this;
        }
        public ValidationRules CustomRule(Func<object, bool> rule, string errorMessage = "Can not be empty.")
        {
            rules.Add((a) => rule(a) ? "" : errorMessage);
            return this;
        }
        #endregion

        #region Strings
        public ValidationRules StringLength(int max, int min = 0,
            string errorMessage = "Can not be less than $min$ characters nor more than $max$ characters.")
        {
            rules.Add((a) => (a != null && a.ToString().Length >= min && a.ToString().Length <= max) ?
            "" : errorMessage.Replace("$min$", min.ToString()).Replace("$max$", max.ToString()));
            return this;
        }
        public ValidationRules StringRequired(string errorMessage = "Can not be empty string.")
        {
            rules.Add((a) => (a != null && !string.IsNullOrWhiteSpace(a.ToString())) ? "" : errorMessage);
            return this;
        }
        public ValidationRules StringRegex(Regex pattern, string errorMessage = "Not matching the pattern.")
        {
            rules.Add((a) => (a != null && pattern.IsMatch(a.ToString())) ? "" : errorMessage);
            return this;
        }
        #endregion
        #region IComparable
        public ValidationRules Range(IComparable min, IComparable max, string errorMessage = "Can not be empty.")
        {
            rules.Add((a) => (((IComparable)a).CompareTo(min) >= 0 &&
                                                     ((IComparable)a).CompareTo(max) <= 0) ? "" : errorMessage);
            return this;
        }
        #endregion
    }
}
