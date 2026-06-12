using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Application.Services;
using Auth.Domain.Common;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace Auth.Tests;

public class DocumentServiceTests
{
    private readonly Mock<IDocumentRepository> _documentRepoMock;
    private readonly Mock<IRoleRepository> _roleRepoMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly Mock<ITenantService> _tenantServiceMock;
    private readonly DocumentService _documentService;

    public DocumentServiceTests()
    {
        _documentRepoMock = new Mock<IDocumentRepository>();
        _roleRepoMock = new Mock<IRoleRepository>();
        
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        
        _tenantContextMock = new Mock<ITenantContext>();
        _fileServiceMock = new Mock<IFileService>();
        _tenantServiceMock = new Mock<ITenantService>();

        _documentService = new DocumentService(
            _documentRepoMock.Object,
            _roleRepoMock.Object,
            _userManagerMock.Object,
            _tenantContextMock.Object,
            _fileServiceMock.Object,
            _tenantServiceMock.Object);
    }

    [Fact]
    public async Task CreateVerificationRequestAsync_ValidMetadata_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var requiredDocId = Guid.NewGuid();

        var requiredDoc = new RequiredDocument
        {
            Id = requiredDocId,
            IsActive = true,
            RequiresDocumentNumber = true,
            RequiresIssueDate = true,
            RequiresExpiryDate = true,
            ValidationRulesJson = "{\"DocumentNumberMinLength\": 5, \"DocumentNumberMaxLength\": 10, \"MinDurationDays\": 30, \"MinDurationYears\": 10}"
        };

        _documentRepoMock.Setup(x => x.GetRequiredDocumentByIdAsync(requiredDocId))
            .ReturnsAsync(requiredDoc);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("passport.pdf");

        var request = new CreateVerificationRequest(
            RequiredDocumentId: requiredDocId,
            DocumentNumber: "ABC12345",
            IssuedAt: new DateTime(2025, 11, 5),
            ExpiresAt: new DateTime(2035, 11, 5), // exactly 10 years
            Files: new List<IFormFile> { fileMock.Object },
            MetadataJson: null
        );

        _fileServiceMock.Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync("http://storage.local/passport.pdf");

        // Act
        var result = await _documentService.CreateVerificationRequestAsync(userId, tenantId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CreateVerificationRequestAsync_DocumentNumberTooShort_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var requiredDocId = Guid.NewGuid();

        var requiredDoc = new RequiredDocument
        {
            Id = requiredDocId,
            IsActive = true,
            RequiresDocumentNumber = true,
            ValidationRulesJson = "{\"DocumentNumberMinLength\": 5}"
        };

        _documentRepoMock.Setup(x => x.GetRequiredDocumentByIdAsync(requiredDocId))
            .ReturnsAsync(requiredDoc);

        var request = new CreateVerificationRequest(
            RequiredDocumentId: requiredDocId,
            DocumentNumber: "123", // too short (length 3, min is 5)
            IssuedAt: null,
            ExpiresAt: null,
            Files: new List<IFormFile>(),
            MetadataJson: null
        );

        // Act
        var result = await _documentService.CreateVerificationRequestAsync(userId, tenantId, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Metadata.NumberTooShort");
    }

    [Fact]
    public async Task CreateVerificationRequestAsync_RegexMismatch_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var requiredDocId = Guid.NewGuid();

        var requiredDoc = new RequiredDocument
        {
            Id = requiredDocId,
            IsActive = true,
            RequiresDocumentNumber = true,
            ValidationRulesJson = "{\"DocumentNumberRegex\": \"^[0-9]+$\"}" // Digits only
        };

        _documentRepoMock.Setup(x => x.GetRequiredDocumentByIdAsync(requiredDocId))
            .ReturnsAsync(requiredDoc);

        var request = new CreateVerificationRequest(
            RequiredDocumentId: requiredDocId,
            DocumentNumber: "ABC12345", // mismatch
            IssuedAt: null,
            ExpiresAt: null,
            Files: new List<IFormFile>(),
            MetadataJson: null
        );

        // Act
        var result = await _documentService.CreateVerificationRequestAsync(userId, tenantId, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Metadata.NumberFormatInvalid");
    }

    [Fact]
    public async Task CreateVerificationRequestAsync_DurationTooShortInDays_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var requiredDocId = Guid.NewGuid();

        var requiredDoc = new RequiredDocument
        {
            Id = requiredDocId,
            IsActive = true,
            RequiresIssueDate = true,
            RequiresExpiryDate = true,
            ValidationRulesJson = "{\"MinDurationDays\": 30}"
        };

        _documentRepoMock.Setup(x => x.GetRequiredDocumentByIdAsync(requiredDocId))
            .ReturnsAsync(requiredDoc);

        var request = new CreateVerificationRequest(
            RequiredDocumentId: requiredDocId,
            DocumentNumber: null,
            IssuedAt: new DateTime(2025, 11, 5),
            ExpiresAt: new DateTime(2025, 11, 15), // only 10 days, min is 30
            Files: new List<IFormFile>(),
            MetadataJson: null
        );

        // Act
        var result = await _documentService.CreateVerificationRequestAsync(userId, tenantId, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Metadata.DurationTooShort");
    }

    [Fact]
    public async Task CreateVerificationRequestAsync_DurationTooShortInYears_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var requiredDocId = Guid.NewGuid();

        var requiredDoc = new RequiredDocument
        {
            Id = requiredDocId,
            IsActive = true,
            RequiresIssueDate = true,
            RequiresExpiryDate = true,
            ValidationRulesJson = "{\"MinDurationYears\": 10}"
        };

        _documentRepoMock.Setup(x => x.GetRequiredDocumentByIdAsync(requiredDocId))
            .ReturnsAsync(requiredDoc);

        var request = new CreateVerificationRequest(
            RequiredDocumentId: requiredDocId,
            DocumentNumber: null,
            IssuedAt: new DateTime(2025, 11, 5),
            ExpiresAt: new DateTime(2035, 11, 4), // 1 day short of exactly 10 years
            Files: new List<IFormFile>(),
            MetadataJson: null
        );

        // Act
        var result = await _documentService.CreateVerificationRequestAsync(userId, tenantId, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Metadata.DurationYearsTooShort");
    }
}
