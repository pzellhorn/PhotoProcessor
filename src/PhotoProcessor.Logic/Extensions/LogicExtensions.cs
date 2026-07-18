using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhotoProcessor.Logic.Encoding;
using PhotoProcessor.Logic.Scaling;
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
        public static IServiceCollection AddLogicServices(this IServiceCollection services, IConfiguration configuration)
        {
            string textEncoderUrl = configuration["TextEncoder:BaseUrl"] ?? throw new Exception("Can't find TextEncoder:BaseUrl in config");
            services.AddHttpClient<ITextEncoder, HttpTextEncoder>(client =>
            {
                client.BaseAddress = new Uri(textEncoderUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddScoped<MediaItemLogic>();
            services.AddScoped<JobLogic>();
            services.AddScoped<FingerprintLogic>();
            services.AddScoped<TagLogic>();
            services.AddScoped<TagItemLogic>();
            services.AddScoped<TagTypeLogic>();
            services.AddScoped<VideoRenditionLogic>();
            services.AddScoped<ImageEmbeddingLogic>();
            services.AddScoped<UploadSessionLogic>();

            services.AddScoped<IMediaLogic, MediaLogic>();
            services.AddScoped<IMediaIngestLogic, MediaIngestLogic>();
            services.AddScoped<IVideoLogic, VideoLogic>();
            services.AddScoped<IFingerprintSubmissionLogic, FingerprintSubmissionLogic>();
            services.AddScoped<IIdentityLogic, IdentityLogic>();
            services.AddScoped<IFingerprintSearchLogic, FingerprintSearchLogic>();
            services.AddScoped<IImageEmbeddingSubmissionLogic, ImageEmbeddingSubmissionLogic>();
            services.AddScoped<ISearchLogic, SearchLogic>();
            services.AddScoped<IProgressLogic, ProgressLogic>();
            services.AddScoped<IResumableUploadLogic, ResumableUploadLogic>();

            services.Configure<WorkerScalingOptions>(configuration.GetSection("WorkerScaling"));
            services.AddSingleton<IWorkerScaler, KubernetesWorkerScaler>();

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
