using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Action.ReferenceData;
using NCS.DSS.Action.Validation;
using Xunit;

namespace NCS.DSS.Action.Tests.ValidationTests
{
    public class ValidateTests
    {

        [Fact]
        public void ValidateTests_ReturnValidationResult_WhenDateActionAgreedIsNotSuppliedForPost()
        {
            var action = new Models.Action
            {
                DateActionAimsToBeCompletedBy = DateTime.UtcNow,
                ActionSummary = "Summary",
                ActionType = ActionType.Other,
                PersonResponsible = PersonResponsible.Customer
            };

            var validation = new Validate();

            var result = validation.ValidateResource(action, true);

            // Assert
            Assert.IsType<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void ValidateTests_ReturnValidationResult_WhenDateActionAimsToBeCompletedByIsNotSuppliedForPost()
        {
            var action = new Models.Action
            {
                DateActionAgreed = DateTime.UtcNow,
                ActionSummary = "Summary",
                ActionType = ActionType.Other,
                PersonResponsible = PersonResponsible.Customer
            };

            var validation = new Validate();

            var result = validation.ValidateResource(action, true);

            // Assert
            Assert.IsType<List<ValidationResult>>(result);
            Assert.NotNull(result);
             Assert.Single(result);
        }

        [Fact]
        public void ValidateTests_ReturnValidationResult_WhenActionSummaryIsNotSuppliedForPost()
        {
            var action = new Models.Action
            {
                DateActionAgreed = DateTime.UtcNow,
                DateActionAimsToBeCompletedBy = DateTime.UtcNow,
                ActionType = ActionType.Other,
                PersonResponsible = PersonResponsible.Customer
            };

            var validation = new Validate();

            var result = validation.ValidateResource(action, true);

            // Assert
            Assert.IsType<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void ValidateTests_ReturnValidationResult_WhenActionTypeIsNotSuppliedForPost()
        {
            var action = new Models.Action
            {
                DateActionAgreed = DateTime.UtcNow,
                DateActionAimsToBeCompletedBy = DateTime.UtcNow,
                ActionSummary = "Summary",
                PersonResponsible = PersonResponsible.Customer
            };

            var validation = new Validate();

            var result = validation.ValidateResource(action, true);

            // Assert
            Assert.IsType<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void ValidateTests_ReturnValidationResult_WhenPersonResponsibleIsNotSuppliedForPost()
        {
            var action = new Models.Action
            {
                DateActionAgreed = DateTime.UtcNow,
                DateActionAimsToBeCompletedBy = DateTime.UtcNow,
                ActionSummary = "Summary",
                ActionType = ActionType.Other
            };

            var validation = new Validate();

            var result = validation.ValidateResource(action, true);

            // Assert
            Assert.IsType<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void ValidateTests_ReturnValidationResult_WhenActionSummaryIsEmptyForPost()
        {
            var action = new Models.Action
            {
                DateActionAgreed = DateTime.UtcNow,
                DateActionAimsToBeCompletedBy = DateTime.UtcNow,
                ActionType = ActionType.Other,
                PersonResponsible = PersonResponsible.Customer
            };

            var validation = new Validate();

            var result = validation.ValidateResource(action, true);

            // Assert
            Assert.IsType<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void ValidateTests_ReturnValidationResult_WhenDateActionAgreedIsInTheFuture()
        {
            var action = new Models.Action
            {
                DateActionAgreed = DateTime.Today.AddDays(1),
                DateActionAimsToBeCompletedBy = DateTime.Today.AddDays(-2),
                ActionSummary = "Summary",
                ActionType = ActionType.Other,
                PersonResponsible = PersonResponsible.Customer
            };

            var validation = new Validate();

            var result = validation.ValidateResource(action, false);

            // Assert
            Assert.IsType<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void ValidateTests_ReturnValidationResult_WhenDateActionActuallyCompletedIsInTheFuture()
        {
            var action = new Models.Action
            {
                DateActionAgreed = DateTime.UtcNow,
                DateActionAimsToBeCompletedBy = DateTime.MaxValue,
                ActionSummary = "Summary",
                ActionType = ActionType.Other,
                PersonResponsible = PersonResponsible.Customer,
                DateActionActuallyCompleted = DateTime.MaxValue
            };

            var validation = new Validate();

            var result = validation.ValidateResource(action, true);

            // Assert
            Assert.IsType<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedDateIsInTheFuture()
        {
            var action = new Models.Action
            {
                DateActionAgreed = DateTime.UtcNow,
                DateActionAimsToBeCompletedBy = DateTime.UtcNow,
                ActionSummary = "Summary",
                ActionType = ActionType.Other,
                PersonResponsible = PersonResponsible.Customer,
                LastModifiedDate = DateTime.MaxValue
            };

            var validation = new Validate();

            var result = validation.ValidateResource(action, false);

            // Assert
            Assert.IsType<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void ValidateTests_ReturnValidationResult_WhenActionTypeIsNotValid()
        {
            var action = new Models.Action
            {
                DateActionAgreed = DateTime.UtcNow,
                DateActionAimsToBeCompletedBy = DateTime.UtcNow,
                ActionSummary = "Summary",
                ActionType = (ActionType)100,
                PersonResponsible = PersonResponsible.Customer
            };

            var validation = new Validate();

            var result = validation.ValidateResource(action, false);

            // Assert
            Assert.IsType<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void ValidateTests_ReturnValidationResult_WhenActionStatusIsNotValid()
        {
            var action = new Models.Action
            {
                DateActionAgreed = DateTime.UtcNow,
                DateActionAimsToBeCompletedBy = DateTime.UtcNow,
                ActionSummary = "Summary",
                ActionType = ActionType.Other,
                PersonResponsible = PersonResponsible.Customer,
                ActionStatus = (ActionStatus)100
            };

            var validation = new Validate();

            var result = validation.ValidateResource(action, false);

            // Assert
            Assert.IsType<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void ValidateTests_ReturnValidationResult_WhenPersonResponsibleIsNotValid()
        {
            var action = new Models.Action
            {
                DateActionAgreed = DateTime.UtcNow,
                DateActionAimsToBeCompletedBy = DateTime.UtcNow,
                ActionSummary = "Summary",
                ActionType = ActionType.Other,
                PersonResponsible = (PersonResponsible)100
            };

            var validation = new Validate();

            var result = validation.ValidateResource(action, false);

            // Assert
            Assert.IsType<List<ValidationResult>>(result);
            Assert.NotNull(result);
            Assert.Single(result);
        }

    }
}
