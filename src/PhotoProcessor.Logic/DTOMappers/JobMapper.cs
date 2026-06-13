using PhotoProcessor.DTO.RequestDTOs.EntityRequests;
using PhotoProcessor.DTO.ResponseDTOs.EntityResponses;
using PhotoProcessor.State.Data.Entities;
using pzellhorn.Core.Logic.Base;

namespace PhotoProcessor.Logic.DTOMappers
{
    public class JobMapper
        : IDTOMapper<Job, JobRequest, JobResponse>
    {
        public Guid? ExtractId(JobRequest request) => request.JobId;

        public void ApplyRequestToModel(JobRequest request, Job model)
        {
            model.MediaId = request.MediaId;
            model.JobType = request.JobType;
            model.Status = request.Status;
        }

        public Job CreateEntity(JobRequest request)
        => new()
        {
            MediaId = request.MediaId,
            JobType = request.JobType,
            Status = request.Status,
        };

        public JobResponse ToResponse(Job model)
        => new(model.JobId, model.MediaId, model.JobType, model.Status);
    }
}
