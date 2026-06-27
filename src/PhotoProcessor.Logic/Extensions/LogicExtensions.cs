using Microsoft.Extensions.DependencyInjection;
using PhotoProcessor.DTO.DTOAdapters.Interfaces;
using PhotoProcessor.DTO.RequestDTOs.EntityRequests;
using PhotoProcessor.DTO.ResponseDTOs.EntityResponses;
using PhotoProcessor.Logic.DTOAdapters.DTOAdapters;
using PhotoProcessor.Logic.DTOMappers;
using PhotoProcessor.Logic.EntityLogic;
using PhotoProcessor.Logic.ServiceLogic;
using PhotoProcessor.State.Data.Entities;
using pzellhorn.Core.Logic.Base;

namespace PhotoProcessor.Logic.Extensions
{
    public static class LogicExtensions
    { 
        public static IServiceCollection AddLogicServices(this IServiceCollection services)
        { 
            services.AddScoped<MediaItemLogic>();
            services.AddScoped<JobLogic>();
            services.AddScoped<FingerprintLogic>();
            services.AddScoped<TagLogic>();
            services.AddScoped<TagItemLogic>();
            services.AddScoped<TagTypeLogic>();

            services.AddScoped<IPhotoLogic, PhotoLogic>();
            services.AddScoped<IFingerprintSubmissionLogic, FingerprintSubmissionLogic>();

            services.AddScoped<IDTOMapper<MediaItem, MediaItemRequest, MediaItemResponse>, MediaItemMapper>();
            services.AddScoped<IDTOMapper<Job, JobRequest, JobResponse>, JobMapper>();
            services.AddScoped<IDTOMapper<Tag, TagRequest, TagResponse>, TagMapper>();
            services.AddScoped<IDTOMapper<TagItem, TagItemRequest, TagItemResponse>, TagItemMapper>();
            services.AddScoped<IDTOMapper<TagType, TagTypeRequest, TagTypeResponse>, TagTypeMapper>();
             
            services.AddScoped<IMediaItemDtoAdapter, MediaItemDtoAdapter>();
            services.AddScoped<IJobDtoAdapter, JobDtoAdapter>();
            services.AddScoped<ITagDtoAdapter, TagDtoAdapter>();
            services.AddScoped<ITagItemDtoAdapter, TagItemDtoAdapter>();
            services.AddScoped<ITagTypeDtoAdapter, TagTypeDtoAdapter>();

            return services;
        }
    }
}
