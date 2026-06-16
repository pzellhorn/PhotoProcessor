using System;
using System.Collections.Generic;
using System.Text;
using PhotoProcessor.DTO.enums;
using PhotoProcessor.DTO.RequestDTOs.EntityRequests;
using PhotoProcessor.Logic.EntityLogic;
using PhotoProcessor.State.Data.Entities;
using pzellhorn.Core.State.Storage;

namespace PhotoProcessor.Logic.ServiceLogic
{
    public interface IPhotoLogic
    {
        Task IngestPhoto(string fileName, Stream stream, CancellationToken cancellationToken = default);
    }

    public class PhotoLogic(MediaItemLogic mediaItems, IStorageManager storageManager, IDistributedQueue queueMananger) : IPhotoLogic
    {
        public async Task IngestPhoto(string fileName, Stream stream, CancellationToken cancellationToken = default)
        { 
            await storageManager.Upsert(fileName, stream, cancellationToken);
             
            JobRequest job = new()
            {
                JobId = Guid.NewGuid(),
                JobType = JobTypes.FaceRecognition,
                Status = JobStatus.Queued,
            }; 
        }
    }
}
