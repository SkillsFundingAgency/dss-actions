using NCS.DSS.Action.ReferenceData;
using NCS.DSS.Action.Validation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Action.Tests.ValidationTests
{
    [TestFixture]
    public class ValidateTests
    {

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenDateActionAgreedIsNotSuppliedForPost()
        {
            // Arrange
            var action = new Models.Action
            {
                DateActionAimsToBeCompletedBy = DateTime.UtcNow,
                ActionSummary = "Summary",
                ActionType = ActionType.Other,
                PersonResponsible = PersonResponsible.Customer
            };

            var validation = new Validate();


            // Act
            var result = validation.ValidateResource(action, true);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenSignPostedToCategoryIsNotSuppliedForPost()
        {
            // Arrange
            var action = new Models.Action
            {
                DateActionAgreed = DateTime.UtcNow,
                DateActionAimsToBeCompletedBy = DateTime.UtcNow,
                ActionSummary = "Summary",
                ActionType = ActionType.Other,
                PersonResponsible = PersonResponsible.Customer
            };

            var validation = new Validate();


            // Act
            var result = validation.ValidateResource(action, true);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
        }

        [Test]
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
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenActionSummaryIsNotSuppliedForPost()
        {
            // Arrange
            var action = new Models.Action
            {
                DateActionAgreed = DateTime.UtcNow,
                DateActionAimsToBeCompletedBy = DateTime.UtcNow,
                ActionType = ActionType.Other,
                PersonResponsible = PersonResponsible.Customer
            };
            var validation = new Validate();

            // Act
            var result = validation.ValidateResource(action, true);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
        }

        [Test]
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

            // Act
            var result = validation.ValidateResource(action, true);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenPersonResponsibleIsNotSuppliedForPost()
        {
            // Arrange
            var action = new Models.Action
            {
                DateActionAgreed = DateTime.UtcNow,
                DateActionAimsToBeCompletedBy = DateTime.UtcNow,
                ActionSummary = "Summary",
                ActionType = ActionType.Other
            };
            var validation = new Validate();

            // Act
            var result = validation.ValidateResource(action, true);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenActionSummaryIsEmptyForPost()
        {
            // Arrange
            var action = new Models.Action
            {
                DateActionAgreed = DateTime.UtcNow,
                DateActionAimsToBeCompletedBy = DateTime.UtcNow,
                ActionType = ActionType.Other,
                PersonResponsible = PersonResponsible.Customer
            };
            var validation = new Validate();

            // Act
            var result = validation.ValidateResource(action, true);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenDateActionAgreedIsInTheFuture()
        {
            // Arrange
            var action = new Models.Action
            {
                DateActionAgreed = DateTime.Today.AddDays(1),
                DateActionAimsToBeCompletedBy = DateTime.Today.AddDays(-2),
                ActionSummary = "Summary",
                ActionType = ActionType.Other,
                PersonResponsible = PersonResponsible.Customer
            };
            var validation = new Validate();

            // Act
            var result = validation.ValidateResource(action, false);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenDateActionActuallyCompletedIsInTheFuture()
        {
            // Arrange
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

            // Act
            var result = validation.ValidateResource(action, true);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedDateIsInTheFuture()
        {
            // Arrange
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

            // Act
            var result = validation.ValidateResource(action, false);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenActionTypeIsNotValid()
        {
            // Arrange
            var action = new Models.Action
            {
                DateActionAgreed = DateTime.UtcNow,
                DateActionAimsToBeCompletedBy = DateTime.UtcNow,
                ActionSummary = "Summary",
                ActionType = (ActionType)100,
                PersonResponsible = PersonResponsible.Customer
            };
            var validation = new Validate();

            // Act
            var result = validation.ValidateResource(action, false);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenActionStatusIsNotValid()
        {
            // Arrange
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

            // Act
            var result = validation.ValidateResource(action, false);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenPersonResponsibleIsNotValid()
        {
            // Arrange
            var action = new Models.Action
            {
                DateActionAgreed = DateTime.UtcNow,
                DateActionAimsToBeCompletedBy = DateTime.UtcNow,
                ActionSummary = "Summary",
                ActionType = ActionType.Other,
                PersonResponsible = (PersonResponsible)100
            };
            var validation = new Validate();

            // Act
            var result = validation.ValidateResource(action, false);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
        }

    }
}
