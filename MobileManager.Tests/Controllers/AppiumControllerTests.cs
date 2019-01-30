using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MobileManager.Appium;
using MobileManager.Configuration.Interfaces;
using MobileManager.Controllers;
using MobileManager.Database.Repositories.Interfaces;
using MobileManager.Http.Clients.Interfaces;
using MobileManager.Logging.Logger;
using Moq;
using Xunit;

namespace MobileManagerTests.Controllers
{
    public class AppiumControllerTests
    {
        private readonly Mock<IManagerConfiguration> _config;
        private static readonly IManagerLogger Logger = new Mock<IManagerLogger>().Object;

        public AppiumControllerTests()
        {
            _config = new Mock<IManagerConfiguration>();
            _config.Setup(c => c.LocalIpAddress).Returns("localhost");
            _config.Setup(c => c.ListeningPort).Returns(9876);
            _config.Setup(c => c.AppiumLogFilePath).Returns("appiumLogs");
            _config.Setup(c => c.IdeviceSyslogFolderPath).Returns("syslogs");
        }

        #region Create Tests

        [Fact]
        public void Create_NullRequest_BadRequest()
        {
            var restClientMock = new Mock<IRestClient>();
            var appiumRepositoryMock = new Mock<IRepository<AppiumProcess>>();
            var appiumController = new AppiumController(restClientMock.Object, appiumRepositoryMock.Object, Logger);

            var result = appiumController.Create(null);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Theory]
        [InlineData("", "1111", "1112", 1, "1113", "1114")]
        [InlineData("123", "", "1112", 1, "1113", "1114")]
        [InlineData("123", "1111", "", 1, "1113", "1114")]
        [InlineData("123", "1111", "1112", 0, "1113", "1114")]
        [InlineData("123", "1111", "1112", 1, "", "1114")]
        [InlineData("123", "1111", "1112", 1, "1113", "")]
        public void Create_InvalidDataInRequest_BadRequest(string deviceId, string appiumPort,
            string appiumBootstrapPort, int appiumPid,
            string webkitDebugProxyPort, string wdaLocalPort)
        {
            var restClientMock = new Mock<IRestClient>();
            var appiumRepositoryMock = new Mock<IRepository<AppiumProcess>>();
            var appiumController = new AppiumController(restClientMock.Object, appiumRepositoryMock.Object, Logger);

            var appiumProcess = new AppiumProcess(deviceId, appiumPort, appiumBootstrapPort, appiumPid,
                webkitDebugProxyPort, wdaLocalPort);

            var result = appiumController.Create(appiumProcess);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Create_AlreadyRunningAppiumForDeviceId_ErrorStatusCode()
        {
            var restClientMock = new Mock<IRestClient>();
            var appiumRepositoryMock = new Mock<IRepository<AppiumProcess>>();
            appiumRepositoryMock.Setup(a => a.Find(It.IsAny<string>())).Returns(new AppiumProcess());

            var appiumController = new AppiumController(restClientMock.Object, appiumRepositoryMock.Object, Logger);

            var appiumProcess = new AppiumProcess("123", "1111", "1112", 1, "1113", "1114");
            var result = appiumController.Create(appiumProcess);

            var viewResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, viewResult.StatusCode);
        }

        [Fact]
        public void Create_AddThrowsAnException_ErrorStatusCode()
        {
            var restClientMock = new Mock<IRestClient>();
            var appiumRepositoryMock = new Mock<IRepository<AppiumProcess>>();
            appiumRepositoryMock.Setup(a => a.Find(It.IsAny<string>())).Returns((AppiumProcess) null);
            appiumRepositoryMock.Setup(a => a.Add(It.IsAny<AppiumProcess>())).Throws(new Exception());

            var appiumController = new AppiumController(restClientMock.Object, appiumRepositoryMock.Object, Logger);

            var appiumProcess = new AppiumProcess("123", "1111", "1112", 1, "1113", "1114");
            var result = appiumController.Create(appiumProcess);

            var viewResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, viewResult.StatusCode);
        }

        [Fact]
        public void Create_ValidScenario_CreatedAtRoute()
        {
            var restClientMock = new Mock<IRestClient>();
            var appiumRepositoryMock = new Mock<IRepository<AppiumProcess>>();
            appiumRepositoryMock.Setup(a => a.Find(It.IsAny<string>())).Returns((AppiumProcess) null);

            var appiumController = new AppiumController(restClientMock.Object, appiumRepositoryMock.Object, Logger);

            var appiumProcess = new AppiumProcess("123", "1111", "1112", 1, "1113", "1114");
            var result = appiumController.Create(appiumProcess);

            var viewResult = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal(appiumProcess, viewResult.Value);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public void Delete_NullRequest_BadRequest()
        {
            var restClientMock = new Mock<IRestClient>();
            var appiumRepositoryMock = new Mock<IRepository<AppiumProcess>>();
            var appiumController = new AppiumController(restClientMock.Object, appiumRepositoryMock.Object, Logger);

            var result = appiumController.Delete(null);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Delete_EmptyRequest_BadRequest()
        {
            var restClientMock = new Mock<IRestClient>();
            var appiumRepositoryMock = new Mock<IRepository<AppiumProcess>>();
            var appiumController = new AppiumController(restClientMock.Object, appiumRepositoryMock.Object, Logger);

            var result = appiumController.Delete(string.Empty);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Delete_AppiumProcessNotInDatabase_NotFound()
        {
            var restClientMock = new Mock<IRestClient>();
            var appiumRepositoryMock = new Mock<IRepository<AppiumProcess>>();

            var appiumProcess = new AppiumProcess("123", "1111", "1112", 1, "1113", "1114");
            appiumRepositoryMock.Setup(a => a.Find(It.IsAny<string>())).Returns((AppiumProcess) null);

            var appiumController = new AppiumController(restClientMock.Object, appiumRepositoryMock.Object, Logger);

            var result = appiumController.Delete("123");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void Delete_RemoveThrowsException_ErrorStatusCode()
        {
            var restClientMock = new Mock<IRestClient>();
            var appiumRepositoryMock = new Mock<IRepository<AppiumProcess>>();

            var appiumProcess = new AppiumProcess("123", "1111", "1112", 1, "1113", "1114");
            appiumRepositoryMock.Setup(a => a.Find(It.IsAny<string>())).Returns(appiumProcess);
            appiumRepositoryMock.Setup(a => a.Remove(It.IsAny<string>())).Throws(new Exception());


            var appiumController = new AppiumController(restClientMock.Object, appiumRepositoryMock.Object, Logger);

            var result = appiumController.Delete("123");

            var viewResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, viewResult.StatusCode);
        }

        [Fact]
        public void Delete_ValidScenario_Ok()
        {
            var restClientMock = new Mock<IRestClient>();
            var appiumRepositoryMock = new Mock<IRepository<AppiumProcess>>();

            var appiumProcess = new AppiumProcess("123", "1111", "1112", 1, "1113", "1114");
            appiumRepositoryMock.Setup(a => a.Find(It.IsAny<string>())).Returns(appiumProcess);

            var appiumController = new AppiumController(restClientMock.Object, appiumRepositoryMock.Object, Logger);

            var result = appiumController.Delete("123");

            Assert.IsType<OkResult>(result);
        }

        #endregion


        #region GetAll Tests

        [Fact]
        public void GetAll_ValidScenario_ListOfAppiumProcesses()
        {
            var restClientMock = new Mock<IRestClient>();
            var appiumRepositoryMock = new Mock<IRepository<AppiumProcess>>();
            var appiumController = new AppiumController(restClientMock.Object, appiumRepositoryMock.Object, Logger);

            var appiumProcess1 = new AppiumProcess("123", "1111", "1112", 1, "1113", "1114");
            var appiumProcess2 = new AppiumProcess("123", "1111", "1112", 1, "1113", "1114");

            var appiumProcesses = new List<AppiumProcess>
            {
                appiumProcess1,
                appiumProcess2
            };

            appiumRepositoryMock.Setup(a => a.GetAll()).Returns(appiumProcesses);

            var result = appiumController.GetAll();

            var viewResult = Assert.IsType<List<AppiumProcess>>(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(appiumProcess1, viewResult);
            Assert.Contains(appiumProcess2, viewResult);
        }

        [Fact]
        public void GetAll_NoAppiumProcesses_EmptyResult()
        {
            var restClientMock = new Mock<IRestClient>();
            var appiumRepositoryMock = new Mock<IRepository<AppiumProcess>>();
            var appiumController = new AppiumController(restClientMock.Object, appiumRepositoryMock.Object, Logger);


            appiumRepositoryMock.Setup(a => a.GetAll()).Returns(new List<AppiumProcess>());

            var result = appiumController.GetAll();

            var viewResult = Assert.IsType<List<AppiumProcess>>(result);
            Assert.Empty(viewResult);
        }


        #endregion

        #region GetById Tests

        [Fact]
        public void GetById_NullRequest_BadRequest()
        {
            var restClientMock = new Mock<IRestClient>();
            var appiumRepositoryMock = new Mock<IRepository<AppiumProcess>>();
            var appiumController = new AppiumController(restClientMock.Object, appiumRepositoryMock.Object, Logger);

            var result = appiumController.GetById(null);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void GetById_EmptyRequest_BadRequest()
        {
            var restClientMock = new Mock<IRestClient>();
            var appiumRepositoryMock = new Mock<IRepository<AppiumProcess>>();
            var appiumController = new AppiumController(restClientMock.Object, appiumRepositoryMock.Object, Logger);

            var result = appiumController.GetById(string.Empty);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void GetById_AppiumProcessNotInDatabase_NotFound()
        {
            var restClientMock = new Mock<IRestClient>();
            var appiumRepositoryMock = new Mock<IRepository<AppiumProcess>>();

            var appiumProcess = new AppiumProcess("123", "1111", "1112", 1, "1113", "1114");
            appiumRepositoryMock.Setup(a => a.Find(It.IsAny<string>())).Returns((AppiumProcess) null);

            var appiumController = new AppiumController(restClientMock.Object, appiumRepositoryMock.Object, Logger);

            var result = appiumController.GetById("123");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void GetById_ValidScenario_Ok()
        {
            var restClientMock = new Mock<IRestClient>();
            var appiumRepositoryMock = new Mock<IRepository<AppiumProcess>>();

            var appiumProcess = new AppiumProcess("123", "1111", "1112", 1, "1113", "1114");
            appiumRepositoryMock.Setup(a => a.Find(It.IsAny<string>())).Returns(appiumProcess);

            var appiumController = new AppiumController(restClientMock.Object, appiumRepositoryMock.Object, Logger);

            var result = appiumController.GetById("123");

            var viewResult = Assert.IsType<JsonResult>(result);
            Assert.Equal(appiumProcess, viewResult.Value);
        }

        #endregion
    }
}
