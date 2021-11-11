using NUnit.Framework;
using FileLoggerKata;
using NSubstitute;
using System;

namespace FileLoggerTests
{
  public class Tests
  {
    IFileSystem _fileSystem = Substitute.For<IFileSystem>();
    IDateProvider _dateProvider = Substitute.For<IDateProvider>();
    
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void GIVEN_DayIsWeekday_WHEN_LogMethodInvoked_THEN_FileNameShouldContainDate()
    {
      //arange
      var date = Convert.ToDateTime("07/06/2021");
      var expectedFileName = "log20210607.txt";
      _dateProvider.Today.Returns(date);
      var logger = new FileLogger(_fileSystem, _dateProvider);
      
      //act
      logger.Log("test message");

      //assert
      _fileSystem.Received().Create(expectedFileName);
    }

    [Test]
    public void GIVEN_DayIsWeekend_WHEN_LogMethodInvoked_THEN_FileNameShouldContainWeekend()
    {
      //arange
      var date = Convert.ToDateTime("05/06/2021");
      var expectedFileName = "weekend.txt";
      
      _dateProvider.Today.Returns(date);
      _fileSystem.Exists(Arg.Any<string>()).ReturnsForAnyArgs(false);

      var logger = new FileLogger(_fileSystem, _dateProvider);
      
      //act
      logger.Log("another test message");

      //assert
      _fileSystem.Received().Create(expectedFileName);
    }

    [Test]
    public void GIVEN_LogFileAlreadyExists_WHEN_LogMethodInvoked_THEN_AppendMethodShoudlBeCalled()
    {
      //arange
      var logMessage = "test append method being called";
      _fileSystem.Exists(Arg.Any<string>()).ReturnsForAnyArgs(true);
      var logger = new FileLogger(_fileSystem, _dateProvider);
      
      //act
      logger.Log(logMessage);

      //assert
      _fileSystem.Received().Append(Arg.Any<string>(), logMessage);
      _fileSystem.DidNotReceiveWithAnyArgs().Create(Arg.Any<string>());
    }

    [Test]
    public void GIVEN_LogFileDoesnotExists_WHEN_LogMethodInvoked_THEN_CreateMethodShoudlBeCalled()
    {
      //arange
      var logMessage = "test create method being called";
      _fileSystem.Exists(Arg.Any<string>()).ReturnsForAnyArgs(false);
      var logger = new FileLogger(_fileSystem, _dateProvider);
      
      //act
      logger.Log(logMessage);

      //assert
      _fileSystem.Received().Create(Arg.Any<string>());
      _fileSystem.ReceivedWithAnyArgs().Append(Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public void GIVEN_DayIsWeekend_WHEN_WeekendLogFileAlreadyExists_Then_ArchiveWeekendLogShouldBeInvoked()
    {
      //arange
      var lastSavedDate = Convert.ToDateTime("05/06/2021");
      var currentDate = Convert.ToDateTime("12/06/2021");
      var currentFileName = "weekend.txt";
      var archivedFileName = "weekend-20210605.txt";
      var logMessage = "test archival";

      _dateProvider.Today.Returns(currentDate);
      _fileSystem.Exists(Arg.Any<string>()).Returns(true);
      _fileSystem.GetLastWriteTime(Arg.Any<string>()).Returns(lastSavedDate);

      var logger = new FileLogger(_fileSystem, _dateProvider);
      
      //act
      logger.Log(logMessage);

      //assert
      _fileSystem.Received().Rename(currentFileName, archivedFileName);
      _fileSystem.Received().Append(currentFileName, logMessage);
    }

    [Test]
    public void GIVEN_DayIsWeekend_WHEN_LessThan3DaysSinceLastWriteTime_Then_ArchiveWeekendLogShouldNotBeInvoked()
    {
      //arange
      var lastSavedDate = Convert.ToDateTime("05/06/2021");
      var currentDate = Convert.ToDateTime("06/06/2021");
      var currentFileName = "weekend.txt";
      var archivedFileName = "weekend-20210606.txt";

      _dateProvider.Today.Returns(currentDate);
      _fileSystem.Exists(Arg.Any<string>()).Returns(true);
      _fileSystem.GetLastWriteTime(Arg.Any<string>()).Returns(lastSavedDate);

      var logger = new FileLogger(_fileSystem, _dateProvider);
      
      //act
      logger.Log("test archival not called if =< 2 days since last weekend log writen");

      //assert
      _fileSystem.DidNotReceive().Rename(currentFileName, archivedFileName);
    }

    [Test]
    public void GIVEN_DayIsWeekday_WHEN_MoreThan2DaysSinceLastWriteTime_Then_ArchiveWeekendLogShouldNotBeInvoked()
    {
      //arange
      var lastSavedDate = Convert.ToDateTime("05/06/2021");
      var currentDate = Convert.ToDateTime("08/06/2021");

      _dateProvider.Today.Returns(currentDate);
      _fileSystem.Exists(Arg.Any<string>()).Returns(true);
      _fileSystem.GetLastWriteTime(Arg.Any<string>()).Returns(lastSavedDate);

      var logger = new FileLogger(_fileSystem, _dateProvider);
      
      //act
      logger.Log("test archival not called if it is a weekday even if a weekend.txt file exists");

      //assert
      _fileSystem.DidNotReceiveWithAnyArgs().Rename(Arg.Any<string>(), Arg.Any<string>());
    }

    [TearDown]
    public void Teardown()
    {
      _fileSystem.ClearReceivedCalls();
      _dateProvider.ClearReceivedCalls();
    }
  }
}