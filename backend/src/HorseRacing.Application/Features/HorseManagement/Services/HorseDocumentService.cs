using System;
using System.Threading.Tasks;
using HorseRacing.Application.Features.HorseManagement.DTOs;
using HorseRacing.Application.Features.HorseManagement.Interfaces;
using HorseRacing.Domain.Entities;

namespace HorseRacing.Application.Features.HorseManagement.Services;

public class HorseDocumentService : IHorseDocumentService
{
    private readonly IHorseRepository _horseRepository;

    public HorseDocumentService(IHorseRepository horseRepository)
    {
        _horseRepository = horseRepository;
    }

    public async Task<HorseDocumentResponse> AddDocumentAsync(int ownerUserId, int horseId, UploadHorseDocumentRequest request)
    {
        var horse = await _horseRepository.GetByIdAsync(horseId);
        if (horse == null)
        {
            throw new ArgumentException($"Horse with ID {horseId} not found.");
        }
        if (horse.OwnerId != ownerUserId)
        {
            throw new InvalidOperationException("Access denied. You do not own this horse.");
        }

        var doc = new HorseDocument
        {
            HorseId = horseId,
            DocumentType = request.DocumentType,
            DocumentUrl = request.DocumentUrl,
            UploadedAt = DateTime.UtcNow
        };

        await _horseRepository.AddDocumentAsync(doc);
        await _horseRepository.SaveChangesAsync();

        return new HorseDocumentResponse
        {
            Id = doc.Id,
            DocumentType = doc.DocumentType,
            DocumentUrl = doc.DocumentUrl,
            UploadedAt = doc.UploadedAt
        };
    }

    public async Task<HorseStatisticResponse?> GetStatisticAsync(int horseId)
    {
        var stat = await _horseRepository.GetStatisticByHorseIdAsync(horseId);
        if (stat == null)
        {
            return null;
        }

        return new HorseStatisticResponse
        {
            Id = stat.Id,
            HorseId = (int)stat.HorseId,
            TotalRaces = stat.TotalRaces,
            TotalWins = stat.TotalWins,
            TotalSecondPlaces = stat.TotalSecondPlaces,
            TotalThirdPlaces = stat.TotalThirdPlaces,
            AverageSpeed = stat.AverageSpeed,
            UpdatedAt = stat.UpdatedAt
        };
    }
}
