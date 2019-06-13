using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentGateway.Controllers;
using System.Web.Mvc;
namespace PaymentgatwayUnitTest
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            HomeController controllerUnderTest = new HomeController();
            ViewResult result = controllerUnderTest.Index() as ViewResult;
            Assert.IsNotNull(result);
        }
    }
}
