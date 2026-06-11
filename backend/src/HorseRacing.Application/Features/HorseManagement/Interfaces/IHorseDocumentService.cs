using System.Threading.Tasks;
using HorseRacing.Application.Features.HorseManagement.DTOs;

namespace HorseRacing.Application.Features.HorseManagement.Interfaces;

public interface IHorseDocumentService
{
    Task<HorseDocumentResponse> AddDocumentAsync(int ownerUserId, int horseId, UploadHorseDocumentRequest request);
    Task<HorseStatisticResponse?> GetStatisticAsync(int horseId);
}
