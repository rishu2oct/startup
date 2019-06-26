using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentGateway.Controllers;
using System.Web.Mvc;
using NUnit.Framework;
namespace PaymentgatwayUnitTest
{
    [TestFixture]
    public class HomeControllerTest
    {
        HomeController controllerUnderTest;
        [OneTimeSetUp]
        public void setupOnce()
        {
            controllerUnderTest = new HomeController();
        }
        [Test]
        public void TestMethod1()
        {
            
            ViewResult result = controllerUnderTest.Index() as ViewResult;
            Assert.IsNotNull(result);
        }
    }
}
