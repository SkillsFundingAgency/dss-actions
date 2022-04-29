using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Action.Models;
using NCS.DSS.Action.ReferenceData;

namespace NCS.DSS.Action.Validation
{
    public class Validate : IValidate
    {

        public List<ValidationResult> ValidateResource(IAction resource, bool validateModelForPost)
        {
            var context = new ValidationContext(resource, null, null);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(resource, context, results, true);
            ValidateActionRules(resource, results, validateModelForPost);

            return results;
        }

        private void ValidateActionRules(IAction actionResource, List<ValidationResult> results, bool validateModelForPost)
        {
            if (actionResource == null)
                return;

            if (validateModelForPost)
            {
                if (string.IsNullOrWhiteSpace(actionResource.ActionSummary))
                    results.Add(new ValidationResult("Action Summary is a required field", new[] { "ActionSummary" }));
            }

            if (actionResource.DateActionAgreed.HasValue)
            {
                if (actionResource.DateActionAgreed.Value > DateTime.UtcNow)
                    results.Add(new ValidationResult("Date Action Agreed must be less than or equal to the current date/time", new[] { "DateActionAgreed" }));

                if (actionResource.DateActionAimsToBeCompletedBy.HasValue && 
                    !(actionResource.DateActionAimsToBeCompletedBy.Value >= actionResource.DateActionAgreed.Value))
                    results.Add(new ValidationResult("Date Action Aims To Be Completed By must be greater than Date Action Agreed", new[] { "DateActionAimsToBeCompletedBy" }));

                if (actionResource.DateActionActuallyCompleted.HasValue &&
                    !(actionResource.DateActionActuallyCompleted.Value >= actionResource.DateActionAgreed.Value))
                    results.Add(new ValidationResult("Date Action Actually Completed must be greater than Date Action Agreed", new[] { "DateActionActuallyCompleted" }));
            }

            if (actionResource.DateActionActuallyCompleted.HasValue)
            {
                if (actionResource.DateActionActuallyCompleted.Value > DateTime.UtcNow)
                    results.Add(new ValidationResult("Date Action Actually Completed must be less the current date/time", new[] { "DateActionActuallyCompleted" }));
            }

            if (actionResource.LastModifiedDate.HasValue && actionResource.LastModifiedDate.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Last Modified Date must be less the current date/time", new[] { "LastModifiedDate" }));

            if (actionResource.ActionType.HasValue && !Enum.IsDefined(typeof(ActionType), actionResource.ActionType.Value))
                results.Add(new ValidationResult("Please supply a valid Action Type", new[] { "ActionType" }));

            if (actionResource.ActionStatus.HasValue && !Enum.IsDefined(typeof(ActionStatus), actionResource.ActionStatus.Value))
                results.Add(new ValidationResult("Please supply a valid Action Status", new[] { "ActionStatus" }));

            if (actionResource.PersonResponsible.HasValue && !Enum.IsDefined(typeof(PersonResponsible), actionResource.PersonResponsible.Value))
                results.Add(new ValidationResult("Please supply a valid Person Responsible", new[] { "PersonResponsible" }));

            if (actionResource.SignpostedToCategory.HasValue && !Enum.IsDefined(typeof(SignpostedToCategory), actionResource.SignpostedToCategory.Value))
                results.Add(new ValidationResult("Please supply a valid Signposted To Category", new[] { "SignpostedToCategory" }));

            if (!actionResource.SignpostedToCategory.HasValue)
                results.Add(new ValidationResult("Signposted To is a required field", new[] { "SignpostedToCategory" }));
        }
    }
}