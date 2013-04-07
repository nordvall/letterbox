using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Letterbox.Clients;
using Letterbox.ServiceBus;
using NSubstitute;
using NUnit.Framework;

namespace Letterbox.Tests.ServiceBus
{
    [TestFixture]
    public class CachingQueueValidatorTests
    {
        [Test]
        public void ValidateQueue_OnFirstCall_InnerValidatorIsInvoked()
        {
            IQueueValidator innerValidator = Substitute.For<IQueueValidator>();
            var validator = new CachingQueueValidator(innerValidator);

            validator.EnsureQueue("Test");

            innerValidator.Received(1).EnsureQueue("Test");
        }

        [Test]
        public void ValidateQueue_OnSecondCall_InnerValidatorIsNotInvoked()
        {
            IQueueValidator innerValidator = Substitute.For<IQueueValidator>();
            var validator = new CachingQueueValidator(innerValidator);

            validator.EnsureQueue("Test");
            validator.EnsureQueue("Test");

            innerValidator.Received(1).EnsureQueue("Test");
        }
    }
}
