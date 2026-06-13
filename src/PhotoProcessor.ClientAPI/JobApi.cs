using pzellhorn.Core.ClientAPI.Base;
using PhotoProcessor.DTO.RequestDTOs.EntityRequests;
using PhotoProcessor.DTO.ResponseDTOs.EntityResponses;

namespace PhotoProcessor.ClientAPI
{
    public class JobApi(ApiTransport api) : BaseClientApi<JobRequest, JobResponse>(api, "Job")
    {
    }
}
