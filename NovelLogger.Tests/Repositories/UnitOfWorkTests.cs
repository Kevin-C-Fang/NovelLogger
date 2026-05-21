using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NovelLogger.Data;
using NovelLogger.Data.Repositories;
using NovelLogger.Data.Repositories.IRepositories;
using NovelLogger.Services.Implementations;
using NovelLogger.Services.Interfaces;
using NovelLogger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NovelLogger.Tests.Repositories
{
    public class UnitOfWorkTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UnitOfWork _unitOfWork;

        public UnitOfWorkTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

            _dbContext = new ApplicationDbContext(options);

            _unitOfWork = new UnitOfWork(_dbContext, NullLogger<UnitOfWork>.Instance);
        }

        [Fact]
        public void TrySave_WhenSaveChangesSucceeds_ReturnsSuccess()
        {
            var result = _unitOfWork.TrySave();
            Assert.Equal(ServiceResult.Success, result);
        }

        [Fact]
        public void TrySave_WhenSaveChangesThrowsAnyException_ReturnsFailed()
        {
            _dbContext.Dispose();

            var result = _unitOfWork.TrySave();
            Assert.Equal(ServiceResult.Failed, result);
        }
    }
}
