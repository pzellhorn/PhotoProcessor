using PhotoProcessor.DTO.enums;
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
            model.JobType = (int)request.JobType;
            model.Status = (int)request.Status;
        }

        public Job CreateEntity(JobRequest request)
        => new()
        {
            MediaId = request.MediaId,
            JobType = (int)request.JobType,
            Status = (int)request.Status,
        };

        public JobResponse ToResponse(Job model)
        => new(model.JobId, model.MediaId, (JobTypes)model.JobType, (JobStatus)model.Status);
    }
}
