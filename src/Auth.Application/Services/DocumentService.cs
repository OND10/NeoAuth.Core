using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Common;
using Auth.Domain.Common.Validation;
using Auth.Domain.Entities;
using Auth.Domain.Enums;
using Auth.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Auth.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantContext _tenantContext;
    private readonly IFileService _fileService;
    private readonly ITenantService _tenantService;

    public DocumentService(
        IDocumentRepository documentRepository,
        IRoleRepository roleRepository,
        UserManager<ApplicationUser> userManager,
        ITenantContext tenantContext,
        IFileService fileService,
        ITenantService tenantService)
    {
        _documentRepository = documentRepository;
        _roleRepository = roleRepository;
        _userManager = userManager;
        _tenantContext = tenantContext;
        _fileService = fileService;
        _tenantService = tenantService;
    }

    public async Task<Result<IEnumerable<RequiredDocumentDto>>> GetRequiredDocumentsAsync(IEnumerable<string> roleNames)
    {
        var tenantId = _tenantContext.TenantId;
        var docs = await _documentRepository.GetRequiredDocumentsByRolesAsync(roleNames, tenantId ?? Guid.Empty);

        var response = docs.Select(x => new RequiredDocumentDto(
            x.Id, x.Name, x.Description, x.MinFilesRequired, x.TargetRoleId, x.TriggerRoleId, x.IsActive,
            x.RequiresDocumentNumber, x.RequiresIssueDate, x.RequiresExpiryDate, x.MetadataSchemaJson, x.ValidationRulesJson));

        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<RequiredDocumentDto>>> GetTenantRequiredDocumentsAsync()
    {
        var tenantId = _tenantContext.TenantId;
        var docs = await _documentRepository.GetRequiredDocumentsForTenantAsync(tenantId ?? Guid.Empty);

        var response = docs.Select(x => new RequiredDocumentDto(
            x.Id, x.Name, x.Description, x.MinFilesRequired, x.TargetRoleId, x.TriggerRoleId, x.IsActive,
            x.RequiresDocumentNumber, x.RequiresIssueDate, x.RequiresExpiryDate, x.MetadataSchemaJson, x.ValidationRulesJson));

        return Result.Success(response);
    }
    
    public async Task<Result<IEnumerable<RequiredDocumentDto>>> GetRequiredDocumentsAsync()
    {
        var docs = await _documentRepository.GetRequiredDocuments();

        var response = docs.Select(x => new RequiredDocumentDto(
            x.Id, x.Name, x.Description, x.MinFilesRequired, x.TargetRoleId, x.TriggerRoleId, x.IsActive,
            x.RequiresDocumentNumber, x.RequiresIssueDate, x.RequiresExpiryDate, x.MetadataSchemaJson, x.ValidationRulesJson));

        return Result.Success(response);
    }

    public async Task<Result<RequiredDocumentDto>> CreateRequiredDocumentAsync(ConfigureRequiredDocumentRequest request)
    {
        try
        {
            var tenantId = _tenantContext.TenantId;

            var doc = new RequiredDocument
            {
                Name = request.Name,
                Description = request.Description,
                MinFilesRequired = request.MinFilesRequired,
                TargetRoleId = request.TargetRoleId,
                TriggerRoleId = request.TriggerRoleId,
                TenantId =  null,
                IsActive = request.IsActive,
                RequiresDocumentNumber = request.RequiresDocumentNumber,
                RequiresIssueDate = request.RequiresIssueDate,
                RequiresExpiryDate = request.RequiresExpiryDate,
                MetadataSchemaJson = request.MetadataSchemaJson,
                ValidationRulesJson = request.ValidationRulesJson
            };

            await _documentRepository.AddRequiredDocumentAsync(doc);
            await _documentRepository.SaveChangesAsync();

            return Result.Success(new RequiredDocumentDto(
                doc.Id, doc.Name, doc.Description, doc.MinFilesRequired, doc.TargetRoleId, doc.TriggerRoleId, doc.IsActive,
                doc.RequiresDocumentNumber, doc.RequiresIssueDate, doc.RequiresExpiryDate, doc.MetadataSchemaJson, doc.ValidationRulesJson));
        }
        catch (Exception ex)
        {

            throw ex;
        }
    }

    public async Task<Result> UpdateRequiredDocumentAsync(Guid id, ConfigureRequiredDocumentRequest request)
    {
        var doc = await _documentRepository.GetRequiredDocumentByIdAsync(id);
        if (doc == null) return Result.Failure(Error.NotFound("Document.NotFound", "Required document not found."));

        doc.Name = request.Name;
        doc.Description = request.Description;
        doc.MinFilesRequired = request.MinFilesRequired;
        doc.TargetRoleId = request.TargetRoleId;
        doc.TriggerRoleId = request.TriggerRoleId;
        doc.IsActive = request.IsActive;
        doc.RequiresDocumentNumber = request.RequiresDocumentNumber;
        doc.RequiresIssueDate = request.RequiresIssueDate;
        doc.RequiresExpiryDate = request.RequiresExpiryDate;
        doc.MetadataSchemaJson = request.MetadataSchemaJson;
        doc.ValidationRulesJson = request.ValidationRulesJson;

        await _documentRepository.UpdateRequiredDocumentAsync(doc);
        await _documentRepository.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteRequiredDocumentAsync(Guid id)
    {
        await _documentRepository.DeleteRequiredDocumentAsync(id);
        await _documentRepository.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<Guid>> CreateVerificationRequestAsync(Guid userId, Guid tenantId, CreateVerificationRequest request)
    {
        var requiredDoc = await _documentRepository.GetRequiredDocumentByIdAsync(request.RequiredDocumentId);
        if (requiredDoc == null) return Result.Failure<Guid>(Error.NotFound("Document.NotFound", "Required document definition not found."));

        if (!requiredDoc.IsActive)
            return Result.Failure<Guid>(Error.Validation("Document.Inactive", "This document requirement is no longer active."));


        var validationResult = ValidateDocumentMetadata(requiredDoc, request.DocumentNumber, request.IssuedAt, request.ExpiresAt);
        if (!validationResult.IsSuccess)
            return Result.Failure<Guid>(validationResult.Error);
        if (request.Files == null || request.Files.Count() < requiredDoc.MinFilesRequired)
            return Result.Failure<Guid>(Error.Validation("Document.InsufficientFiles", $"At least {requiredDoc.MinFilesRequired} files are required."));

        var verificationRequest = new VerificationRequest
        {
            UserId = userId,
            TenantId = tenantId == Guid.Empty ? null : tenantId,
            RequiredDocumentId = request.RequiredDocumentId,
            Status = VerificationStatus.Pending,
            DocumentNumber = request.DocumentNumber,
            IssuedAt = request.IssuedAt,
            ExpiresAt = request.ExpiresAt,
            MetadataJson = request.MetadataJson
        };

        foreach (var file in request.Files)
        {
            var fileUrl = await _fileService.SaveFileAsync(file, $"verifications/{userId}");
            verificationRequest.Documents.Add(new UserDocument
            {
                FileUrl = fileUrl,
                FileName = file.FileName
            });
        }

        await _documentRepository.AddVerificationRequestAsync(verificationRequest);
        await _documentRepository.SaveChangesAsync();

        return Result.Success(verificationRequest.Id);
    }

    public async Task<Result> SubmitBulkVerificationAsync(Guid userId, Guid tenantId, BulkVerificationRequest request)
    {
        try
        {
            if (request.Documents == null || !request.Documents.Any())
                return Result.Failure(Error.Validation("Bulk.Empty", "No documents provided."));

            // ── Validate all file types up-front before touching the DB ──────────────
            foreach (var doc in request.Documents)
            {
                if (doc.Files != null)
                {
                    foreach (var file in doc.Files)
                    {
                        if (!ImageValidation.ValidationFileUpload(file))
                            return Result.Failure(Error.Validation("File.Invalid",
                                $"File '{file.FileName}' is invalid or exceeds the allowed size."));
                    }
                }
            }

            // ── Process each document item ────────────────────────────────────────────
            foreach (var item in request.Documents)
            {
                var requiredDoc = await _documentRepository.GetRequiredDocumentByIdAsync(item.RequiredDocumentId);

                if (requiredDoc is null)
                    return Result.Failure(Error.NotFound("Document.NotFound",
                        $"Required document '{item.RequiredDocumentId}' not found."));

                if (!requiredDoc.IsActive)
                    return Result.Failure(Error.Validation("Document.Inactive",
                        $"'{requiredDoc.Name}' is no longer active."));

                var fileCount = item.Files?.Count() ?? 0;
                if (fileCount < requiredDoc.MinFilesRequired)
                    return Result.Failure(Error.Validation("Document.InsufficientFiles",
                        $"'{requiredDoc.Name}' requires at least {requiredDoc.MinFilesRequired} file(s), " +
                        $"but {fileCount} were uploaded."));

                var validationResult = ValidateDocumentMetadata(requiredDoc, item.DocumentNumber, item.IssuedAt, item.ExpiresAt);
                if (!validationResult.IsSuccess)
                    return Result.Failure(validationResult.Error);


                var tenant = await _tenantService.GetByIdAsync(tenantId);
                if (tenant.Data is null)
                    tenantId = Guid.Empty;
                // ── Save files into Documents/{userId}/{requiredDocId}/ ───────────────
                var verificationRequest = new VerificationRequest
                {
                    UserId = userId,
                    TenantId = tenantId == Guid.Empty ? null : tenantId,
                    RequiredDocumentId = item.RequiredDocumentId,
                    Status = VerificationStatus.Pending,
                    DocumentNumber = item.DocumentNumber,
                    IssuedAt = item.IssuedAt,
                    ExpiresAt = item.ExpiresAt,
                    MetadataJson = item.MetadataJson
                };

                if (item.Files != null)
                {
                    foreach (var file in item.Files)
                    {
                        var fileUrl = await _fileService.UploadToDocumentsAsync(
                            file, specificDirectory: $"{userId}/{item.RequiredDocumentId}");

                        verificationRequest.Documents.Add(new UserDocument
                        {
                            FileUrl = fileUrl,
                            FileName = file.FileName
                        });
                    }
                }

                await _documentRepository.AddVerificationRequestAsync(verificationRequest);
            }

            await _documentRepository.SaveChangesAsync();
            return Result.Success("Bulk verification submitted successfully.");
        }
        catch (Exception ex)
        {

            throw ex;
        }
    }

    /// <summary>
    /// Persists a bulk submission where files have already been saved by the API layer.
    /// Accepts URL strings instead of IFormFile; validates metadata and MinFilesRequired.
    /// </summary>
    public async Task<Result> SubmitBulkProcessedAsync(Guid userId, Guid tenantId, BulkProcessedVerificationRequest request)
    {
        if (request.Documents == null || !request.Documents.Any())
            return Result.Failure(Error.Validation("Bulk.Empty", "No documents provided in the request."));

        foreach (var item in request.Documents)
        {
            // Fetch the required document config to enforce MinFilesRequired & metadata rules
            var requiredDoc = await _documentRepository.GetRequiredDocumentByIdAsync(item.RequiredDocumentId);
            if (requiredDoc == null)
                return Result.Failure(Error.NotFound("Document.NotFound",
                    $"Required document definition '{item.RequiredDocumentId}' not found."));

            if (!requiredDoc.IsActive)
                return Result.Failure(Error.Validation("Document.Inactive",
                    $"Document '{requiredDoc.Name}' is no longer active."));

            // Validate file count against MinFilesRequired
            if (item.FileUrls == null || item.FileUrls.Count < requiredDoc.MinFilesRequired)
                return Result.Failure(Error.Validation("Document.InsufficientFiles",
                    $"'{requiredDoc.Name}' requires at least {requiredDoc.MinFilesRequired} file(s)."));

            // Validate dynamic metadata fields
            if (requiredDoc.RequiresDocumentNumber && string.IsNullOrWhiteSpace(item.DocumentNumber))
                return Result.Failure(Error.Validation("Metadata.MissingNumber",
                    $"Document number is required for '{requiredDoc.Name}'."));

            if (requiredDoc.RequiresIssueDate && !item.IssuedAt.HasValue)
                return Result.Failure(Error.Validation("Metadata.MissingIssueDate",
                    $"Issue date is required for '{requiredDoc.Name}'."));

            if (requiredDoc.RequiresExpiryDate && !item.ExpiresAt.HasValue)
                return Result.Failure(Error.Validation("Metadata.MissingExpiryDate",
                    $"Expiry date is required for '{requiredDoc.Name}'."));

            // Build and persist the VerificationRequest with the pre-saved URLs
            var verificationRequest = new VerificationRequest
            {
                UserId = userId,
                TenantId = tenantId == Guid.Empty ? null : tenantId,
                RequiredDocumentId = item.RequiredDocumentId,
                Status = VerificationStatus.Pending,
                DocumentNumber = item.DocumentNumber,
                IssuedAt = item.IssuedAt,
                ExpiresAt = item.ExpiresAt,
                MetadataJson = item.MetadataJson
            };

            foreach (var url in item.FileUrls)
            {
                verificationRequest.Documents.Add(new UserDocument
                {
                    FileUrl = url,
                    FileName = Path.GetFileName(url)
                });
            }

            await _documentRepository.AddVerificationRequestAsync(verificationRequest);
        }

        await _documentRepository.SaveChangesAsync();
        return Result.Success("Bulk verification submitted successfully.");
    }

    public async Task<Result> ReviewRequestAsync(Guid requestId, ReviewVerificationRequest review, Guid adminId)
    {
        var request = await _documentRepository.GetVerificationRequestByIdAsync(requestId);
        if (request == null) return Result.Failure(Error.NotFound("Request.NotFound", "Verification request not found."));

        request.Status = review.Status;
        request.AdminNotes = review.AdminNotes;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewedBy = adminId;

        if (review.Status == VerificationStatus.Approved)
        {
            // Perform Role Transition
            var targetRole = await _roleRepository.FindByIdAsync(request.RequiredDocument.TargetRoleId);
            if (targetRole != null)
            {
                await _userManager.AddToRoleAsync(request.User, targetRole.Name!);

                if (request.RequiredDocument.TriggerRoleId.HasValue)
                {
                    var triggerRole = await _roleRepository.FindByIdAsync(request.RequiredDocument.TriggerRoleId.Value);
                    if (triggerRole != null)
                    {
                        await _userManager.RemoveFromRoleAsync(request.User, triggerRole.Name!);
                    }
                }
            }
        }

        await _documentRepository.UpdateVerificationRequestAsync(request);
        await _documentRepository.SaveChangesAsync();
        return Result.Success(request.Status== VerificationStatus.Approved? "User is Verfied Successfully": "User is Rejected Successfully");
    }

    public async Task<Result<IEnumerable<VerificationRequestDto>>> GetPendingRequestsAsync()
    {
        var requests = await _documentRepository.GetPendingRequestsAsync();

        var response = requests.Select(x => new VerificationRequestDto(
            x.Id,
            x.UserId,
            x.RequiredDocumentId,
            x.Status,
            x.AdminNotes,
            x.CreatedAt,
            x.Documents.Select(d => new UserDocumentDto(d.Id, d.FileUrl, d.FileName, d.UploadedAt)).ToList(),
            x.DocumentNumber,
            x.IssuedAt,
            x.ExpiresAt,
            x.MetadataJson
        ));

        return Result.Success(response);
    }

    public async Task<Result<VerificationRequestDto>> GetUserStatusAsync(Guid userId, Guid tenantId)
    {
        var request = await _documentRepository.GetLatestUserRequestAsync(userId, tenantId);
        if (request == null) return Result.Failure<VerificationRequestDto>(Error.NotFound("Request.NotFound", "No verification request found for this user."));

        var response = new VerificationRequestDto(
            request.Id,
            request.UserId,
            request.RequiredDocumentId,
            request.Status,
            request.AdminNotes,
            request.CreatedAt,
            request.Documents.Select(d => new UserDocumentDto(d.Id, d.FileUrl, d.FileName, d.UploadedAt)).ToList(),
            request.DocumentNumber,
            request.IssuedAt,
            request.ExpiresAt,
            request.MetadataJson
        );

        return Result.Success(response);
    }

    private Result ValidateDocumentMetadata(
        RequiredDocument requiredDoc,
        string? documentNumber,
        DateTime? issuedAt,
        DateTime? expiresAt)
    {
        // 1. Basic checks
        if (requiredDoc.RequiresDocumentNumber && string.IsNullOrWhiteSpace(documentNumber))
            return Result.Failure(Error.Validation("Metadata.MissingNumber", "Document number is required."));

        if (requiredDoc.RequiresIssueDate && !issuedAt.HasValue)
            return Result.Failure(Error.Validation("Metadata.MissingIssueDate", "Issue date is required."));

        if (requiredDoc.RequiresExpiryDate && !expiresAt.HasValue)
            return Result.Failure(Error.Validation("Metadata.MissingExpiryDate", "Expiry date is required."));

        // 2. Dynamic Rules checks
        if (!string.IsNullOrWhiteSpace(requiredDoc.ValidationRulesJson))
        {
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var rules = System.Text.Json.JsonSerializer.Deserialize<DocumentValidationRules>(requiredDoc.ValidationRulesJson, options);
                if (rules != null)
                {
                    // Validate Document Number Format / Length
                    if (!string.IsNullOrWhiteSpace(documentNumber))
                    {
                        if (rules.DocumentNumberMinLength.HasValue && documentNumber.Length < rules.DocumentNumberMinLength.Value)
                        {
                            return Result.Failure(Error.Validation("Metadata.NumberTooShort",
                                $"Document number must be at least {rules.DocumentNumberMinLength.Value} characters."));
                        }

                        if (rules.DocumentNumberMaxLength.HasValue && documentNumber.Length > rules.DocumentNumberMaxLength.Value)
                        {
                            return Result.Failure(Error.Validation("Metadata.NumberTooLong",
                                $"Document number cannot exceed {rules.DocumentNumberMaxLength.Value} characters."));
                        }

                        if (!string.IsNullOrWhiteSpace(rules.DocumentNumberRegex))
                        {
                            var regex = new System.Text.RegularExpressions.Regex(rules.DocumentNumberRegex);
                            if (!regex.IsMatch(documentNumber))
                            {
                                return Result.Failure(Error.Validation("Metadata.NumberFormatInvalid",
                                    "Document number format is invalid."));
                            }
                        }
                    }

                    // Validate duration between IssuedAt and ExpiresAt
                    if (issuedAt.HasValue && expiresAt.HasValue)
                    {
                        if (expiresAt.Value <= issuedAt.Value)
                        {
                            return Result.Failure(Error.Validation("Metadata.ExpiryBeforeIssue",
                                "Expiry date must be after the issue date."));
                        }

                        var totalDays = (expiresAt.Value - issuedAt.Value).TotalDays;

                        if (rules.MinDurationDays.HasValue && totalDays < rules.MinDurationDays.Value)
                        {
                            return Result.Failure(Error.Validation("Metadata.DurationTooShort",
                                $"Duration between issue and expiry must be at least {rules.MinDurationDays.Value} days."));
                        }

                        if (rules.MaxDurationDays.HasValue && totalDays > rules.MaxDurationDays.Value)
                        {
                            return Result.Failure(Error.Validation("Metadata.DurationTooLong",
                                $"Duration between issue and expiry cannot exceed {rules.MaxDurationDays.Value} days."));
                        }

                        if (rules.MinDurationYears.HasValue || rules.MaxDurationYears.HasValue)
                        {
                            int yearsDiff = expiresAt.Value.Year - issuedAt.Value.Year;
                            if (expiresAt.Value.Month < issuedAt.Value.Month || 
                                (expiresAt.Value.Month == issuedAt.Value.Month && expiresAt.Value.Day < issuedAt.Value.Day))
                            {
                                yearsDiff--;
                            }

                            if (rules.MinDurationYears.HasValue && yearsDiff < rules.MinDurationYears.Value)
                            {
                                return Result.Failure(Error.Validation("Metadata.DurationYearsTooShort",
                                    $"Duration between issue and expiry must be at least {rules.MinDurationYears.Value} years."));
                            }

                            if (rules.MaxDurationYears.HasValue && yearsDiff > rules.MaxDurationYears.Value)
                            {
                                return Result.Failure(Error.Validation("Metadata.DurationYearsTooLong",
                                    $"Duration between issue and expiry cannot exceed {rules.MaxDurationYears.Value} years."));
                            }
                        }
                    }
                }
            }
            catch (System.Text.Json.JsonException)
            {
                return Result.Failure(Error.Validation("Metadata.InvalidValidationRules", "The document configuration contains invalid validation rules JSON."));
            }
        }

        return Result.Success();
    }
}
